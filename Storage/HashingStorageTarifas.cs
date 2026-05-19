using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EstructurasDeDatosIntegrador.Storage
{
    // Hashing extensible para tarifas. Misma estructura de 3 archivos binarios
    // que HashingStorage; los datos se almacenan en el directorio de trabajo
    // actual — el llamador debe apuntar a TarifsData/ antes de usar esta clase.
    internal class HashingStorageTarifas
    {
        private const string DIRECTORY_FILE  = "directory.dat";
        private const string BUCKETS_FILE    = "buckets.dat";
        private const string DATA_FILE       = "tarifas.dat";

        // Capacidad máxima de registros por bucket (idéntica a HashingStorage).
        private const int BUCKET_CAPACITY = 2;

        // Bytes fijos reservados para el código en cada entrada del bucket (≤ 8 chars UTF-8).
        // Bucket: localDepth(4) + count(4) + BUCKET_CAPACITY × (codigo(8) + dataOffset(8)) = 40 bytes.
        private const int CODIGO_KEY_SIZE = 8;

        private int globalDepth = 1;

        // ── Inicialización ─────────────────────────────────────────────────────

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
            WriteLong(directoryStream, CreateEmptyBucket(bucketsStream, 1)); // índice 0
            WriteLong(directoryStream, CreateEmptyBucket(bucketsStream, 1)); // índice 1
            WriteInt(dataStream, 0);
        }

        public void EnsureInitialized()
        {
            if (!File.Exists(DIRECTORY_FILE) || !File.Exists(BUCKETS_FILE) || !File.Exists(DATA_FILE))
                InitializeFiles();
        }

        private long CreateEmptyBucket(FileStream bucketsStream, int localDepth)
        {
            long offset = bucketsStream.Length;
            bucketsStream.Seek(0, SeekOrigin.End);
            WriteInt(bucketsStream, localDepth);
            WriteInt(bucketsStream, 0);
            for (int i = 0; i < BUCKET_CAPACITY; i++)
            {
                WriteCodigoKey(bucketsStream, string.Empty);
                WriteLong(bucketsStream, -1L);
            }
            return offset;
        }

        // ── Hash y directorio ──────────────────────────────────────────────────

        private int Hash(string codigo)
        {
            int sum = 0;
            foreach (char c in codigo) sum += c;
            return Math.Abs(sum % 97);
        }

        private int GetDirectoryIndex(string codigo)
        {
            return Hash(codigo) & ((1 << globalDepth) - 1);
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

        // ── API pública ────────────────────────────────────────────────────────

        public bool AddTarifa(Tarifa tarifa)
        {
            if (GetTarifa(tarifa.Codigo) != null)
                return false;

            long dataOffset = AddTarifaData(tarifa);
            InsertEntry(tarifa.Codigo, dataOffset);
            return true;
        }

        public bool DeleteTarifa(string codigo)
        {
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.ReadWrite);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.ReadWrite);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.ReadWrite);

                globalDepth = ReadInt(directoryStream);
                int dirIndex = GetDirectoryIndex(codigo);

                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                ReadInt(bucketsStream); // localDepth
                int count = ReadInt(bucketsStream);

                long entryBase  = bucketOffset + 8;
                int  foundIndex = -1;
                for (int i = 0; i < count; i++)
                {
                    bucketsStream.Seek(entryBase + i * 16, SeekOrigin.Begin);
                    string stored = ReadCodigoKey(bucketsStream);
                    ReadLong(bucketsStream);
                    if (stored == codigo) { foundIndex = i; break; }
                }
                if (foundIndex < 0) return false;

                for (int i = foundIndex; i < count - 1; i++)
                {
                    bucketsStream.Seek(entryBase + (i + 1) * 16, SeekOrigin.Begin);
                    string nextCodigo = ReadCodigoKey(bucketsStream);
                    long   nextOffset = ReadLong(bucketsStream);
                    bucketsStream.Seek(entryBase + i * 16, SeekOrigin.Begin);
                    WriteCodigoKey(bucketsStream, nextCodigo);
                    WriteLong(bucketsStream, nextOffset);
                }

                bucketsStream.Seek(entryBase + (count - 1) * 16, SeekOrigin.Begin);
                WriteCodigoKey(bucketsStream, string.Empty);
                WriteLong(bucketsStream, -1L);
                bucketsStream.Seek(bucketOffset + 4, SeekOrigin.Begin);
                WriteInt(bucketsStream, count - 1);

                dataStream.Seek(0, SeekOrigin.Begin);
                int total = ReadInt(dataStream);
                dataStream.Seek(0, SeekOrigin.Begin);
                WriteInt(dataStream, Math.Max(0, total - 1));

                return true;
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return false;
        }

        public Tarifa GetTarifa(string codigo)
        {
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.Read);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.Read);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.Read);

                globalDepth = ReadInt(directoryStream);
                int dirIndex = GetDirectoryIndex(codigo);

                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                ReadInt(bucketsStream); // localDepth
                int count = ReadInt(bucketsStream);

                for (int i = 0; i < count; i++)
                {
                    string storedCodigo = ReadCodigoKey(bucketsStream);
                    long   dataOffset   = ReadLong(bucketsStream);
                    if (storedCodigo == codigo)
                    {
                        dataStream.Seek(dataOffset, SeekOrigin.Begin);
                        return ReadTarifaRecord(dataStream);
                    }
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return null;
        }

        public Tarifa GetTarifaSeq(string codigo)
        {
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin);
                while (dataStream.Position < dataStream.Length)
                {
                    var t = ReadTarifaRecord(dataStream);
                    if (t.Codigo == codigo) return t;
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return null;
        }

        // Recorre los buckets para devolver solo las tarifas actualmente registradas.
        public List<Tarifa> GetTarifasRegistradas()
        {
            var lista = new List<Tarifa>();
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.Read);
                using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.Read);
                using var dataStream      = new FileStream(DATA_FILE,      FileMode.Open, FileAccess.Read);

                globalDepth = ReadInt(directoryStream);
                int dirSize   = 1 << globalDepth;
                var visitados = new HashSet<long>();

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
                        string codigo     = ReadCodigoKey(bucketsStream);
                        long   dataOffset = ReadLong(bucketsStream);
                        if (string.IsNullOrEmpty(codigo) || dataOffset < 0) continue;
                        dataStream.Seek(dataOffset, SeekOrigin.Begin);
                        lista.Add(ReadTarifaRecord(dataStream));
                    }
                }
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return lista;
        }

        public List<Tarifa> GetAllTarifas()
        {
            var lista = new List<Tarifa>();
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin);
                while (dataStream.Position < dataStream.Length)
                    lista.Add(ReadTarifaRecord(dataStream));
            }
            catch (IOException e) { Console.WriteLine(e.Message); }
            return lista;
        }

        public string GetTarifaCount()
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

        // ── Operaciones internas ───────────────────────────────────────────────

        private long AddTarifaData(Tarifa tarifa)
        {
            using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.ReadWrite);
            dataStream.Seek(0, SeekOrigin.Begin);
            int count = ReadInt(dataStream);
            dataStream.Seek(0, SeekOrigin.Begin);
            WriteInt(dataStream, count + 1);

            long dataOffset = dataStream.Length;
            dataStream.Seek(0, SeekOrigin.End);
            WriteTarifaRecord(dataStream, tarifa);
            return dataOffset;
        }

        private void InsertEntry(string codigo, long dataOffset)
        {
            using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.ReadWrite);
            using var bucketsStream   = new FileStream(BUCKETS_FILE,   FileMode.Open, FileAccess.ReadWrite);

            directoryStream.Seek(0, SeekOrigin.Begin);
            globalDepth = ReadInt(directoryStream);

            while (true)
            {
                int  dirIndex     = GetDirectoryIndex(codigo);
                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                int localDepth = ReadInt(bucketsStream);
                int count      = ReadInt(bucketsStream);

                if (count < BUCKET_CAPACITY)
                {
                    bucketsStream.Seek(bucketOffset + 8 + (long)count * 16, SeekOrigin.Begin);
                    WriteCodigoKey(bucketsStream, codigo);
                    WriteLong(bucketsStream, dataOffset);
                    bucketsStream.Seek(bucketOffset + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, count + 1);
                    return;
                }

                var existingCodigos  = new string[count];
                var existingOffsets  = new long[count];
                bucketsStream.Seek(bucketOffset + 8, SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    existingCodigos[i]  = ReadCodigoKey(bucketsStream);
                    existingOffsets[i]  = ReadLong(bucketsStream);
                }

                if (localDepth == globalDepth)
                {
                    DoubleDirectory(directoryStream, globalDepth);
                    globalDepth++;
                }

                int  newLocalDepth   = localDepth + 1;
                long newBucketOffset = CreateEmptyBucket(bucketsStream, newLocalDepth);

                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                WriteInt(bucketsStream, newLocalDepth);
                WriteInt(bucketsStream, 0);
                for (int i = 0; i < BUCKET_CAPACITY; i++)
                {
                    WriteCodigoKey(bucketsStream, string.Empty);
                    WriteLong(bucketsStream, -1L);
                }

                int dirSize = 1 << globalDepth;
                int oldMask = (1 << localDepth) - 1;
                int pattern = dirIndex & oldMask;

                for (int i = 0; i < dirSize; i++)
                {
                    if ((i & oldMask) == pattern)
                    {
                        directoryStream.Seek(4 + i * 8L, SeekOrigin.Begin);
                        WriteLong(directoryStream,
                            ((i >> localDepth) & 1) == 1 ? newBucketOffset : bucketOffset);
                    }
                }

                for (int i = 0; i < existingCodigos.Length; i++)
                {
                    int  idx          = GetDirectoryIndex(existingCodigos[i]);
                    directoryStream.Seek(4 + idx * 8L, SeekOrigin.Begin);
                    long targetBucket = ReadLong(directoryStream);

                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    int targetCount = ReadInt(bucketsStream);
                    bucketsStream.Seek(targetBucket + 8 + (long)targetCount * 16, SeekOrigin.Begin);
                    WriteCodigoKey(bucketsStream, existingCodigos[i]);
                    WriteLong(bucketsStream, existingOffsets[i]);
                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, targetCount + 1);
                }
            }
        }

        // ── Serialización de Tarifa ────────────────────────────────────────────

        // tarifas.dat: codigo(str) + nombre(str) + tipo(int) + aplicaA(int,-1=todos)
        //              + valor(double) + horaInicio(int) + horaFin(int) + descripcion(str)
        private static void WriteTarifaRecord(FileStream stream, Tarifa t)
        {
            WriteString(stream, t.Codigo);
            WriteString(stream, t.Nombre);
            WriteInt(stream, (int)t.Tipo);
            WriteInt(stream, t.AplicaA.HasValue ? (int)t.AplicaA.Value : -1);
            WriteDouble(stream, t.Valor);
            WriteInt(stream, t.HoraInicio);
            WriteInt(stream, t.HoraFin);
            WriteString(stream, t.Descripcion);
        }

        private static Tarifa ReadTarifaRecord(FileStream stream)
        {
            string        codigo      = ReadString(stream);
            string        nombre      = ReadString(stream);
            TipoTarifa    tipo        = (TipoTarifa)ReadInt(stream);
            int           aplicaARaw  = ReadInt(stream);
            TipoVehiculo? aplicaA     = aplicaARaw < 0 ? (TipoVehiculo?)null : (TipoVehiculo)aplicaARaw;
            double        valor       = ReadDouble(stream);
            int           horaInicio  = ReadInt(stream);
            int           horaFin     = ReadInt(stream);
            string        descripcion = ReadString(stream);
            return new Tarifa(codigo, nombre, tipo, aplicaA, valor, horaInicio, horaFin, descripcion);
        }

        // ── E/S binaria (big-endian) ───────────────────────────────────────────

        private static void WriteCodigoKey(FileStream stream, string codigo)
        {
            byte[] key        = new byte[CODIGO_KEY_SIZE];
            byte[] codigoBytes = Encoding.UTF8.GetBytes(codigo);
            int    len        = Math.Min(codigoBytes.Length, CODIGO_KEY_SIZE);
            Array.Copy(codigoBytes, key, len);
            stream.Write(key, 0, CODIGO_KEY_SIZE);
        }

        private static string ReadCodigoKey(FileStream stream)
        {
            byte[] key = new byte[CODIGO_KEY_SIZE];
            stream.Read(key, 0, CODIGO_KEY_SIZE);
            int len = Array.IndexOf(key, (byte)0);
            if (len < 0) len = CODIGO_KEY_SIZE;
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

        private static void WriteDouble(FileStream stream, double value)
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

        private static double ReadDouble(FileStream stream)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

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
