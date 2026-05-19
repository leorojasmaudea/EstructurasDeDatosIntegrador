using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EstructurasDeDatosIntegrador.Storage
{
    internal class HashingStorage
    {
        private const string DIRECTORY_FILE = "directory.dat";
        private const string BUCKETS_FILE = "buckets.dat";
        private const string DATA_FILE = "users.dat";

        // Capacidad máxima de registros por bucket
        private const int BUCKET_CAPACITY = 2;

        // Formato de cada bucket: localDepth(int,4) + count(int,4) + BUCKET_CAPACITY * (cc(long,8) + dataOffset(long,8))
        // Tamaño total de un bucket: 8 + BUCKET_CAPACITY * 16 = 40 bytes

        // Formato del directorio: globalDepth(int,4) + 2^globalDepth * puntero(long,8)

        private int globalDepth = 1;

        private void ResetFiles()
        {
            if (File.Exists(DIRECTORY_FILE)) File.Delete(DIRECTORY_FILE);
            if (File.Exists(BUCKETS_FILE)) File.Delete(BUCKETS_FILE);
            if (File.Exists(DATA_FILE)) File.Delete(DATA_FILE);
        }

        public void InitializeFiles()
        {
            ResetFiles();
            globalDepth = 1;

            using var bucketsStream = new FileStream(BUCKETS_FILE, FileMode.Create, FileAccess.ReadWrite);
            using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Create, FileAccess.ReadWrite);
            using var dataStream = new FileStream(DATA_FILE, FileMode.Create, FileAccess.ReadWrite);

            // Escribimos la profundidad global en el directorio
            WriteInt(directoryStream, globalDepth);

            // Creamos dos buckets vacíos con localDepth = 1
            long bucket1Offset = CreateEmptyBucket(bucketsStream, 1);
            long bucket2Offset = CreateEmptyBucket(bucketsStream, 1);

            // Directorio: 2 entradas (2^1), cada una es un long que apunta al offset del bucket
            WriteLong(directoryStream, bucket1Offset); // índice 0 → bucket 0
            WriteLong(directoryStream, bucket2Offset); // índice 1 → bucket 1

            // Inicializamos el archivo de datos con un contador de registros en 0
            WriteInt(dataStream, 0);
        }

        /// <summary>
        /// Crea un bucket vacío al final del archivo de buckets y retorna su offset.
        /// </summary>
        private long CreateEmptyBucket(FileStream bucketsStream, int localDepth)
        {
            long offset = bucketsStream.Length;
            bucketsStream.Seek(0, SeekOrigin.End);
            WriteInt(bucketsStream, localDepth); // Profundidad local
            WriteInt(bucketsStream, 0);          // Cantidad de registros = 0
            for (int i = 0; i < BUCKET_CAPACITY; i++)
            {
                WriteLong(bucketsStream, -1L); // CC placeholder
                WriteLong(bucketsStream, -1L); // dataOffset placeholder
            }
            return offset;
        }

        private int GetDirectoryIndex(long cc)
        {
            return Hash(cc) & ((1 << globalDepth) - 1);
        }

        private int Hash(long cc)
        {
            return (int)(cc % 97);
        }

        private static void DoubleDirectory(FileStream directory, int oldGlobalDepth)
        {
            // Calculamos el tamaño del directorio antes de la duplicación, el cual es 2^oldGlobalDepth,
            // y leemos todos los punteros actuales a los buckets para luego escribirlos dos veces en el nuevo directorio.
            int oldSize = 1 << oldGlobalDepth;
            List<long> oldPointers = new List<long>();
            directory.Seek(4, SeekOrigin.Begin); // Saltamos el globalDepth
            // Creamos una lista para almacenar los punteros actuales del directorio,
            // los cuales se encuentran después de los 4 bytes de la profundidad global.
            for (int i = 0; i < oldSize; i++)
                oldPointers.Add(ReadLong(directory));

            // Reseteamos el directorio para escribir la nueva profundidad global y luego los punteros a los buckets.
            directory.SetLength(0);
            directory.Seek(0, SeekOrigin.Begin);
            // Escribimos la nueva profundidad global, que es el doble de la anterior,
            // y luego escribimos los punteros a los buckets dos veces para reflejar la duplicación del directorio.
            WriteInt(directory, oldGlobalDepth + 1);
            for (int i = 0; i < oldSize; i++)
                WriteLong(directory, oldPointers[i]);
            for (int i = 0; i < oldSize; i++)
                WriteLong(directory, oldPointers[i]);
        }

        public bool AddUser(User userData)
        {
            // Primero verificamos que el usuario no exista previamente para evitar duplicados.
            if (GetUser(userData.Cc) != null)
                return false;

            // Primero almacenar al final del archivo de datos para obtener el offset correspondiente
            long dataOffset = AddUserData(userData);
            // Segundo, insertar la entrada (cc, dataOffset) en el bucket correspondiente según el directorio y la función hash.
            // Si el bucket está lleno, se divide y se reintenta la inserción.
            InsertEntry(userData.Cc, dataOffset);
            return true;
        }

        private long AddUserData(User userData)
        {
            using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.ReadWrite);
            // Leer el contador actual de registros y actualizarlo
            dataStream.Seek(0, SeekOrigin.Begin);
            int count = ReadInt(dataStream);
            dataStream.Seek(0, SeekOrigin.Begin);
            WriteInt(dataStream, count + 1); // contador++

            // Escribir el registro al final del archivo
            long dataOffset = dataStream.Length;
            dataStream.Seek(0, SeekOrigin.End);
            WriteLong(dataStream, userData.Cc);
            WriteString(dataStream, userData.Name);
            WriteString(dataStream, userData.Email);

            return dataOffset;
        }

        /// <summary>
        /// Inserta una entrada (cc, dataOffset) en el bucket correspondiente.
        /// Si el bucket está lleno, lo divide y reintenta hasta lograr la inserción.
        /// </summary>
        private void InsertEntry(long cc, long dataOffset)
        {
            using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.ReadWrite);
            using var bucketsStream = new FileStream(BUCKETS_FILE, FileMode.Open, FileAccess.ReadWrite);

            directoryStream.Seek(0, SeekOrigin.Begin);
            globalDepth = ReadInt(directoryStream);

            // Es true ya que el criterio de parada es cuando se logra insertar el nuevo registro, lo que puede requerir
            // múltiples splits si los registros siguen cayendo en el mismo bucket.
            while (true)
            {
                // Obtenemos el indice, el cual está dado por los bits menos significativos del hash de la CC,
                // limitados por la profundidad global (globalDepth).
                int dirIndex = GetDirectoryIndex(cc);

                // Leer el puntero al bucket desde el directorio
                // Saltamos los 4 bytes de la profundidad global la cual está almacenada en disco y luego multiplicamos
                // el índice por 8 (tamaño de un long) para obtener la posición del puntero al bucket.
                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                // Obtenemos el offset del bucket al que corresponde el índice calculado.
                long bucketOffset = ReadLong(directoryStream);

                // Leer la cabecera del bucket
                // Nos posicionamos en el offset del bucket para leer su cabecera, la cual contiene
                // la profundidad local y la cantidad de registros actualmente almacenados.
                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                int localDepth = ReadInt(bucketsStream);
                int count = ReadInt(bucketsStream);

                // CASO 1: Bucket con espacio disponible — insertar directamente
                // Nos ubicamos en el offset del bucket, saltamos los 8 bytes de la cabecera (localDepth + count) y
                // luego avanzamos count * 16 bytes para posicionarnos al final de las entradas actuales
                // (cada entrada ocupa 16 bytes: 8 para cc y 8 para dataOffset).
                if (count < BUCKET_CAPACITY)
                {
                    bucketsStream.Seek(bucketOffset + 8 + (long)count * 16, SeekOrigin.Begin);
                    WriteLong(bucketsStream, cc);
                    WriteLong(bucketsStream, dataOffset);
                    // Actualizar el contador
                    bucketsStream.Seek(bucketOffset + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, count + 1);
                    return;
                }

                long[] existingCCs = new long[count];
                long[] existingOffsets = new long[count];
                // Nos posicionamos en el offset del bucket, saltamos los 8 bytes de la cabecera (localDepth + count)
                // y luego leemos las entradas existentes (cc y dataOffset) para almacenarlas temporalmente en arrays.
                bucketsStream.Seek(bucketOffset + 8, SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    existingCCs[i] = ReadLong(bucketsStream);
                    existingOffsets[i] = ReadLong(bucketsStream);
                }

                // CASO 2: Split con duplicación del directorio (localDepth == globalDepth)
                // En este caso, el bucket está lleno y además no tenemos espacio en el directorio para crear un nuevo bucket,
                // por lo que debemos duplicar el directorio antes de dividir el bucket.
                if (localDepth == globalDepth)
                {
                    DoubleDirectory(directoryStream, globalDepth);
                    // En DoubleDirectory ya se actualiza el valor de globalDepth en el archivo, pero es necesario
                    // actualizar la variable en memoria para que el resto del código funcione correctamente.
                    globalDepth++;
                }

                int newLocalDepth = localDepth + 1;

                // Crear el nuevo bucket con la nueva profundidad local
                long newBucketOffset = CreateEmptyBucket(bucketsStream, newLocalDepth);

                // Limpiar el bucket original y actualizar su profundidad local
                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                WriteInt(bucketsStream, newLocalDepth);
                WriteInt(bucketsStream, 0); // count = 0
                for (int i = 0; i < BUCKET_CAPACITY; i++)
                {
                    WriteLong(bucketsStream, -1L);
                    WriteLong(bucketsStream, -1L);
                }

                // Actualizar los punteros del directorio
                int dirSize = 1 << globalDepth; // Tamaño actual del directorio después de la posible duplicación
                int oldMask = (1 << localDepth) - 1; // Máscara para obtener los bits relevantes de la profundidad local anterior
                int pattern = dirIndex & oldMask; // bits bajos que identificaban al bucket viejo

                for (int i = 0; i < dirSize; i++)
                {
                    if ((i & oldMask) == pattern)
                    {
                        directoryStream.Seek(4 + i * 8L, SeekOrigin.Begin);
                        // El bit en la posición localDepth decide si va al bucket viejo (0) o al nuevo (1)
                        if (((i >> localDepth) & 1) == 1)
                            WriteLong(directoryStream, newBucketOffset);
                        else
                            WriteLong(directoryStream, bucketOffset);
                    }
                }

                // Redistribuir las entradas existentes entre los dos buckets
                for (int i = 0; i < existingCCs.Length; i++)
                {
                    // Recalculamos el índice del directorio para cada CC existente usando la nueva profundidad global,
                    // lo que nos permitirá determinar a qué bucket debe ir cada entrada (viejo o nuevo).
                    int idx = GetDirectoryIndex(existingCCs[i]);
                    // Saltamos los 4 bytes de la profundidad global en el directorio y luego multiplicamos el índice
                    // por 8 para obtener la posición del puntero al bucket destino.
                    directoryStream.Seek(4 + idx * 8L, SeekOrigin.Begin);
                    long targetBucket = ReadLong(directoryStream);

                    // Nos posicionamos en el campo count del bucket destino (offset + 4 bytes de localDepth)
                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    // Leemos cuántas entradas tiene actualmente el bucket destino
                    int targetCount = ReadInt(bucketsStream);
                    // Nos posicionamos al final de las entradas existentes del bucket destino:
                    // offset + 8 (cabecera: localDepth + count) + targetCount * 16 (cada entrada: 8 cc + 8 dataOffset)
                    bucketsStream.Seek(targetBucket + 8 + (long)targetCount * 16, SeekOrigin.Begin);
                    // Escribimos la CC y el dataOffset de la entrada redistribuida
                    WriteLong(bucketsStream, existingCCs[i]);
                    WriteLong(bucketsStream, existingOffsets[i]);
                    // Volvemos al campo count del bucket destino para incrementarlo en 1
                    bucketsStream.Seek(targetBucket + 4, SeekOrigin.Begin);
                    WriteInt(bucketsStream, targetCount + 1);
                }

                // Volver al inicio del while para reintentar la inserción del nuevo registro
                // (el bucket destino puede seguir lleno si todos los registros fueron al mismo lado)
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin); // Saltamos los 4 bytes del contador de registros
                while (dataStream.Position < dataStream.Length)
                {
                    long cc = ReadLong(dataStream);
                    string name = ReadString(dataStream);
                    string email = ReadString(dataStream);
                    users.Add(new User(cc, name, email));
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\n=== Todos los usuarios ===");
            foreach (var u in users)
                Console.WriteLine($"  CC: {u.Cc}, Nombre: {u.Name}, Email: {u.Email}");
            Console.WriteLine($"Total: {users.Count} usuarios");
            return users;
        }

        public string GetUserCount()
        {
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(0, SeekOrigin.Begin);
                return ReadInt(dataStream).ToString();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return "0";
        }

        // Búsqueda de usuario por CC usando búsqueda secuencial: se recorre el archivo de datos desde el inicio,
        // leyendo cada registro completo (CC, nombre, email) y comparando la CC con la buscada.
        // Si se encuentra una coincidencia, se retorna el usuario.
        public User GetUserSeq(long cc)
        {
            try
            {
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);
                dataStream.Seek(4, SeekOrigin.Begin); // Saltamos los 4 bytes del contador de registros
                while (dataStream.Position < dataStream.Length)
                {
                    long readCC = ReadLong(dataStream);
                    string name = ReadString(dataStream);
                    string email = ReadString(dataStream);
                    if (readCC == cc)
                        return new User(readCC, name, email);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine($"Usuario con CC {cc} no encontrado.");
            return null;
        }

        // Búsqueda de usuario por CC usando Extendible Hashing: se calcula el índice del directorio usando la función hash
        // y la profundidad global, luego se accede al bucket correspondiente y se busca la CC entre las entradas del bucket.
        // Si se encuentra, se lee el registro completo del archivo de datos usando el offset almacenado en el bucket.
        public User GetUser(long cc)
        {
            try
            {
                using var directoryStream = new FileStream(DIRECTORY_FILE, FileMode.Open, FileAccess.Read);
                using var bucketsStream = new FileStream(BUCKETS_FILE, FileMode.Open, FileAccess.Read);
                using var dataStream = new FileStream(DATA_FILE, FileMode.Open, FileAccess.Read);

                globalDepth = ReadInt(directoryStream);
                int dirIndex = GetDirectoryIndex(cc);

                // Leer el puntero al bucket desde el directorio
                directoryStream.Seek(4 + dirIndex * 8L, SeekOrigin.Begin);
                long bucketOffset = ReadLong(directoryStream);

                // Leer la cabecera del bucket
                bucketsStream.Seek(bucketOffset, SeekOrigin.Begin);
                int localDepth = ReadInt(bucketsStream); // Solo para mover el puntero del archivo, no se usa en la búsqueda
                int count = ReadInt(bucketsStream);

                // Buscar la CC en las entradas del bucket
                for (int i = 0; i < count; i++)
                {
                    long storedCC = ReadLong(bucketsStream);
                    long dataOffset = ReadLong(bucketsStream);
                    if (storedCC == cc)
                    {
                        // Encontrado: leer el registro completo del archivo de datos
                        dataStream.Seek(dataOffset, SeekOrigin.Begin);
                        long readCC = ReadLong(dataStream);
                        string name = ReadString(dataStream);
                        string email = ReadString(dataStream);
                        return new User(readCC, name, email);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        // Métodos auxiliares para E/S binaria (big-endian, equivalente a RandomAccessFile de Java)

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

        // Equivalente a writeUTF/readUTF de Java: prefijo de 2 bytes con la longitud en bytes + UTF-8
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
            ushort length = BitConverter.ToUInt16(lenBytes, 0);
            byte[] strBytes = new byte[length];
            stream.Read(strBytes, 0, length);
            return Encoding.UTF8.GetString(strBytes);
        }
    }
}
