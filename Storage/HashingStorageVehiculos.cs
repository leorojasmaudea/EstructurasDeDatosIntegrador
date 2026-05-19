using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EstructurasDeDatosIntegrador.Storage
{
    internal class HashingStorageVehiculos
    {
        private const string DIRECTORY_FILE = "directory.dat";
        private const string BUCKETS_FILE   = "buckets.dat";
        private const string DATA_FILE      = "vehiculos.dat";

        // Capacidad máxima de registros por bucket
        private const int BUCKET_CAPACITY = 2;

        // Bytes fijos reservados para la placa en cada entrada del bucket (≤ 8 chars UTF-8)
        private const int PLACA_KEY_SIZE = 8;

        // Formato de cada bucket: localDepth(int,4) + count(int,4) + BUCKET_CAPACITY × (placa(byte[8]) + dataOffset(long,8))
        // Tamaño total de un bucket: 8 + BUCKET_CAPACITY × 16 = 40 bytes  ← idéntico a la versión anterior

        // Formato del directorio: globalDepth(int,4) + 2^globalDepth × puntero(long,8)

        private int globalDepth = 1;

        private void ResetFiles()
        {
            if (File.Exists(DIRECTORY_FILE)) File.Delete(DIRECTORY_FILE);
            if (File.Exists(BUCKETS_FILE))   File.Delete(BUCKETS_FILE);
            if (File.Exists(DATA_FILE))      File.Delete(DATA_FILE);
        }

        public void InitializeFiles()
        {
            ResetFiles();
            globalDepth = 1;

            using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Create, FileAccess.ReadWrite);
            using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Create, FileAccess.ReadWrite);
            using var dataStream      = new FileStream(DATA_FILE,      FileMode.Create, FileAccess.ReadWrite);

            WriteInt(directoryStream, globalDepth);

            long bucket1Offset = CreateEmptyBucket(bucketsStream, 1);
            long bucket2Offset = CreateEmptyBucket(bucketsStream, 1);

            WriteLong(directoryStream, bucket1Offset); // índice 0 → bucket 0
            WriteLong(directoryStream, bucket2Offset); // índice 1 → bucket 1

            WriteInt(dataStream, 0); // contador de registros = 0
        }

        private long CreateEmptyBucket(FileStream bucketsStream, int localDepth)
        {
            long offset = bucketsStream.Length;
            bucketsStream.Seek(0, SeekOrigin.End);
            WriteInt(bucketsStream, localDepth);
            WriteInt(bucketsStream, 0);
            for (int i = 0; i < BUCKET_CAPACITY; i++)
            {
                WritePlacaKey(bucketsStream, string.Empty); // placa placeholder (8 ceros)
                WriteLong(bucketsStream, -1L);              // dataOffset placeholder
            }
            return offset;
        }

        // ── Hash y directorio ──────────────────────────────────────────────────

        private int Hash(string placa)
        {
            int sum = 0;
            foreach (char c in placa) sum += c;
            return Math.Abs(sum % 97);
        }

        private int GetDirectoryIndex(string placa)
        {
            return Hash(placa) & ((1 << globalDepth) - 1);
        }

        private static void DoubleDirectory(FileStream directory, int oldGlobalDepth)
        {
            int oldSize = 1 << oldGlobalDepth;
            var oldPointers = new List<long>();
            directory.Seek(4, SeekOrigin.Begin);
            for (int i = 0; i < oldSize; i++)
                oldPointers.Add(ReadLong(directory));

            directory.SetLength(0);
            directory.Seek(0, SeekOrigin.Begin);
            WriteInt(directory, oldGlobalDepth + 1);
            for (int i = 0; i < oldSize; i++) WriteLong(directory, oldPointers[i]);
            for (int i = 0; i < oldSize; i++) WriteLong(directory, oldPointers[i]);
        }

        // Abre los archivos existentes o los crea desde cero si no existen.
        public void EnsureInitialized()
        {
            if (!File.Exists(DIRECTORY_FILE) || !File.Exists(BUCKETS_FILE) || !File.Exists(DATA_FILE))
                InitializeFiles();
        }

        // ── API pública ────────────────────────────────────────────────────────

        public bool AddVehiculo(Vehiculo vehiculo)
        {
            if (GetVehiculo(vehiculo.Placa) != null)
                return false;

            long dataOffset = AddVehiculoData(vehiculo);
            InsertEntry(vehiculo.Placa, dataOffset);
            return true;
        }

        public bool DeleteVehiculo(string placa)
        {
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.ReadWrite);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.ReadWrite);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.ReadWrite);

                globalDepth = ReadInt(directoryStream);
                int dirIndex = GetDirectoryIndex(placa);

                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                ReadInt(bucketsStream); // localDepth — no se usa aquí
                int count = ReadInt(bucketsStream);

                long entryBase  = bucketOffset + 8;
                int  foundIndex = -1;
                for (int i = 0; i < count; i++)
                {
                    bucketsStream.Seek(entryBase + i * 16, SeekOrigin.Begin);
                    string stored = ReadPlacaKey(bucketsStream);
                    ReadLong(bucketsStream);
                    if (stored == placa) { foundIndex = i; break; }
                }
                if (foundIndex < 0) return false;

                // Desplazar entradas posteriores una posición hacia atrás.
                for (int i = foundIndex; i < count - 1; i++)
                {
                    bucketsStream.Seek(entryBase + (i + 1) * 16, SeekOrigin.Begin);
                    string nextPlaca  = ReadPlacaKey(bucketsStream);
                    long   nextOffset = ReadLong(bucketsStream);
                    bucketsStream.Seek(entryBase + i * 16, SeekOrigin.Begin);
                    WritePlacaKey(bucketsStream, nextPlaca);
                    WriteLong(bucketsStream, nextOffset);
                }

                // Limpiar el último slot y decrementar el contador del bucket.
                bucketsStream.Seek(entryBase + (count - 1) * 16, SeekOrigin.Begin);
                WritePlacaKey(bucketsStream, string.Empty);
                WriteLong(bucketsStream, -1L);
                bucketsStream.Seek(bucketOffset + 4, SeekOrigin.Begin);
                WriteInt(bucketsStream, count - 1);

                // Decrementar el contador global en vehiculos.dat.
                dataStream.Seek(0, SeekOrigin.Begin);
                int total = ReadInt(dataStream);
                dataStream.Seek(0, SeekOrigin.Begin);
                WriteInt(dataStream, Math.Max(0, total - 1));

                return true;
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return false;
        }

        // Devuelve solo los vehículos actualmente en el parqueadero
        // recorriendo los buckets (ignora registros eliminados del hash).
        public List<Vehiculo> GetVehiculosPresentes()
        {
            var lista = new List<Vehiculo>();
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.Read);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.Read);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.Read);

                globalDepth = ReadInt(directoryStream);
                int dirSize = 1 << globalDepth;
                var visitados = new System.Collections.Generic.HashSet<long>();

                for (int i = 0; i < dirSize; i++)
                {
                    directoryStream.Seek(4 + i * 8L, SeekOrigin.Begin);
                    long bucketOffset = ReadLong(directoryStream);
                    if (!visitados.Add(bucketOffset)) continue;

                    bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                    ReadInt(bucketsStream); // localDepth
                    int count = ReadInt(bucketsStream);

                    for (int j = 0; j < count; j++)
                    {
                        string placa      = ReadPlacaKey(bucketsStream);
                        long   dataOffset = ReadLong(bucketsStream);
                        if (string.IsNullOrEmpty(placa) || dataOffset < 0) continue;
                        dataStream.Seek(dataOffset, SeekOrigin.Begin);
                        lista.Add(ReadVehiculoRecord(dataStream));
                    }
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return lista;
        }

        public List<Vehiculo> GetAllVehiculos()
        {
            var lista = new List<Vehiculo>();
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin);
                while (dataStream.Position < dataStream.Length)
                    lista.Add(ReadVehiculoRecord(dataStream));
            }
            catch (IOException e) { Console.WriteLine(e.Message); }

            Console.WriteLine("\n=== Todos los vehículos ===");
            foreach (var v in lista)
                Console.WriteLine($"  Placa: {v.Placa}, Tipo: {v.Tipo}, Comentarios: {v.Comentarios}");
            Console.WriteLine($"Total: {lista.Count} vehículos");
            return lista;
        }

        public string GetVehiculoCount()
        {
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(0, SeekOrigin.Begin);
                return ReadInt(dataStream).ToString();
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return "0";
        }

        // Búsqueda secuencial: recorre vehiculos.dat comparando placa registro a registro.
        public Vehiculo GetVehiculoSeq(string placa)
        {
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin);
                while (dataStream.Position < dataStream.Length)
                {
                    var v = ReadVehiculoRecord(dataStream);
                    if (v.Placa == placa) return v;
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            Console.WriteLine($"Vehículo con placa {placa} no encontrado.");
            return null;
        }

        // Búsqueda por hashing: calcula el índice del directorio, accede al bucket
        // y localiza la placa; luego lee el registro completo desde vehiculos.dat.
        public Vehiculo GetVehiculo(string placa)
        {
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.Read);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.Read);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.Read);

                globalDepth = ReadInt(directoryStream);
                int dirIndex = GetDirectoryIndex(placa);

                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                int localDepth = ReadInt(bucketsStream); // avanza el puntero, no se usa aquí
                int count      = ReadInt(bucketsStream);

                for (int i = 0; i < count; i++)
                {
                    string storedPlaca = ReadPlacaKey(bucketsStream);
                    long   dataOffset  = ReadLong(bucketsStream);
                    if (storedPlaca == placa)
                    {
                        dataStream.Seek(dataOffset, SeekOrigin.Begin);
                        return ReadVehiculoRecord(dataStream);
                    }
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return null;
        }

        // ── Operaciones internas ───────────────────────────────────────────────

        private long AddVehiculoData(Vehiculo vehiculo)
        {
            using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.ReadWrite);
            dataStream.Seek(0, SeekOrigin.Begin);
            int count = ReadInt(dataStream);
            dataStream.Seek(0, SeekOrigin.Begin);
            WriteInt(dataStream, count + 1);

            long dataOffset = dataStream.Length;
            dataStream.Seek(0, SeekOrigin.End);
            WriteVehiculoRecord(dataStream, vehiculo);
            return dataOffset;
        }

        private void InsertEntry(string placa, long dataOffset)
        {
            using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.ReadWrite);
            using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.ReadWrite);

            directoryStream.Seek(0, SeekOrigin.Begin);
            globalDepth = ReadInt(directoryStream);

            while (true)
            {
                int  dirIndex    = GetDirectoryIndex(placa);
                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                int localDepth = ReadInt(bucketsStream);
                int count      = ReadInt(bucketsStream);

                // CASO 1: hay espacio en el bucket → insertar directamente.
                // Cada entrada ocupa 16 bytes: 8 (placa fija) + 8 (dataOffset).
                if (count < BUCKET_CAPACITY)
                {
                    bucketsStream.Seek(bucketOffset + 8 + (long)count * 16, SeekOrigin.Begin);
                    WritePlacaKey(bucketsStream, placa);
                    WriteLong(bucketsStream, dataOffset);
                    bucketsStream.Seek(bucketOffset + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, count + 1);
                    return;
                }

                // Leer entradas existentes del bucket lleno.
                var existingPlacas   = new string[count];
                var existingOffsets  = new long[count];
                bucketsStream.Seek(bucketOffset + 8, SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    existingPlacas[i]  = ReadPlacaKey(bucketsStream);
                    existingOffsets[i] = ReadLong(bucketsStream);
                }

                // CASO 2: localDepth == globalDepth → duplicar directorio antes del split.
                if (localDepth == globalDepth)
                {
                    DoubleDirectory(directoryStream, globalDepth);
                    globalDepth++;
                }

                int  newLocalDepth   = localDepth + 1;
                long newBucketOffset = CreateEmptyBucket(bucketsStream, newLocalDepth);

                // Limpiar bucket original.
                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                WriteInt(bucketsStream, newLocalDepth);
                WriteInt(bucketsStream, 0);
                for (int i = 0; i < BUCKET_CAPACITY; i++)
                {
                    WritePlacaKey(bucketsStream, string.Empty);
                    WriteLong(bucketsStream, -1L);
                }

                // Reasignar punteros del directorio.
                int dirSize  = 1 << globalDepth;
                int oldMask  = (1 << localDepth) - 1;
                int pattern  = dirIndex & oldMask;

                for (int i = 0; i < dirSize; i++)
                {
                    if ((i & oldMask) == pattern)
                    {
                        directoryStream.Seek(4 + i * 8L, SeekOrigin.Begin);
                        WriteLong(directoryStream,
                            ((i >> localDepth) & 1) == 1 ? newBucketOffset : bucketOffset);
                    }
                }

                // Redistribuir entradas entre bucket viejo y nuevo.
                for (int i = 0; i < existingPlacas.Length; i++)
                {
                    int  idx          = GetDirectoryIndex(existingPlacas[i]);
                    directoryStream.Seek(4 + idx * 8L, SeekOrigin.Begin);
                    long targetBucket = ReadLong(directoryStream);

                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    int targetCount = ReadInt(bucketsStream);
                    bucketsStream.Seek(targetBucket + 8 + (long)targetCount * 16, SeekOrigin.Begin);
                    WritePlacaKey(bucketsStream, existingPlacas[i]);
                    WriteLong(bucketsStream, existingOffsets[i]);
                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, targetCount + 1);
                }
                // Reintentar la inserción del registro nuevo.
            }
        }

        // ── Serialización de Vehiculo ──────────────────────────────────────────

        // vehiculos.dat: placa(string) + tipo(int) + tarifa(int) + horaEntrada(long ticks) + comentarios(string) + foto(int len + bytes)
        private static void WriteVehiculoRecord(FileStream stream, Vehiculo v)
        {
            WriteString(stream, v.Placa);
            WriteInt(stream, (int)v.Tipo);
            WriteInt(stream, (int)v.Tarifa);
            WriteLong(stream, v.HoraEntrada.Ticks);
            WriteString(stream, v.Comentarios);
            WriteInt(stream, v.Foto.Length);
            if (v.Foto.Length > 0)
                stream.Write(v.Foto, 0, v.Foto.Length);
        }

        private static Vehiculo ReadVehiculoRecord(FileStream stream)
        {
            string       placa       = ReadString(stream);
            TipoVehiculo tipo        = (TipoVehiculo)ReadInt(stream);
            TipoTarifa   tarifa      = (TipoTarifa)ReadInt(stream);
            var          horaEntrada = new System.DateTime(ReadLong(stream));
            string       comentarios = ReadString(stream);
            int          fotoLen     = ReadInt(stream);
            byte[]       foto        = new byte[fotoLen];
            if (fotoLen > 0) stream.Read(foto, 0, fotoLen);
            return new Vehiculo(placa, tipo, tarifa, horaEntrada, comentarios, foto);
        }

        // ── E/S binaria (big-endian) ───────────────────────────────────────────

        // Escribe la placa en exactamente PLACA_KEY_SIZE bytes (UTF-8 + padding de ceros).
        private static void WritePlacaKey(FileStream stream, string placa)
        {
            byte[] key      = new byte[PLACA_KEY_SIZE];
            byte[] placaBytes = Encoding.UTF8.GetBytes(placa);
            int    len      = Math.Min(placaBytes.Length, PLACA_KEY_SIZE);
            Array.Copy(placaBytes, key, len);
            stream.Write(key, 0, PLACA_KEY_SIZE);
        }

        // Lee PLACA_KEY_SIZE bytes y devuelve la placa como string (sin bytes nulos de relleno).
        private static string ReadPlacaKey(FileStream stream)
        {
            byte[] key = new byte[PLACA_KEY_SIZE];
            stream.Read(key, 0, PLACA_KEY_SIZE);
            int len = Array.IndexOf(key, (byte)0);
            if (len < 0) len = PLACA_KEY_SIZE;
            return Encoding.UTF8.GetString(key, 0, len);
        }

        private static void WriteInt(FileStream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 4);
        }

        private static void WriteLong(FileStream stream, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 8);
        }

        private static int ReadInt(FileStream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static long ReadLong(FileStream stream)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        // Prefijo de 2 bytes con la longitud + UTF-8 (equivalente a writeUTF/readUTF de Java).
        private static void WriteString(FileStream stream, string value)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            byte[] lenBytes = BitConverter.GetBytes((ushort)strBytes.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
            stream.Write(lenBytes, 0, 2);
            stream.Write(strBytes, 0, strBytes.Length);
        }

        private static string ReadString(FileStream stream)
        {
            byte[] lenBytes = new byte[2];
            stream.Read(lenBytes, 0, 2);
            if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
            ushort length   = BitConverter.ToUInt16(lenBytes, 0);
            byte[] strBytes = new byte[length];
            stream.Read(strBytes, 0, length);
            return Encoding.UTF8.GetString(strBytes);
        }
    }
}
