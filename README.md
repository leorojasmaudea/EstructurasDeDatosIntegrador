# Estructuras de Datos — Hashing Extensible

Implementación de **Hashing Extensible sobre archivos binarios** como proyecto integrador de la asignatura Estructuras de Datos.

**Autores:** Leon Andres Rojas Martínez · Ulises Orozco Villegas — Universidad de Antioquia

---

## Descripción

El proyecto implementa un índice de **Hashing Extensible (Extendible Hashing)** persistido en disco que permite insertar, buscar y listar registros de usuarios sin cargar toda la base de datos en memoria.

La estructura mantiene tres archivos binarios:

| Archivo | Contenido |
|---|---|
| `directory.dat` | Profundidad global + punteros a buckets |
| `buckets.dat` | Buckets con profundidad local, contador y entradas `(cc, offset)` |
| `users.dat` | Registros completos `(cc, nombre, email)` + contador al inicio |

### Estructura de un bucket

```
┌──────────────────────────────────────────────────────┐
│ localDepth (int, 4 bytes)                            │
│ count      (int, 4 bytes)                            │
│ cc[0]      (long, 8 bytes) │ dataOffset[0] (long, 8) │
│ cc[1]      (long, 8 bytes) │ dataOffset[1] (long, 8) │
└──────────────────────────────────────────────────────┘
Tamaño fijo: 8 + BUCKET_CAPACITY × 16 = 40 bytes
```

### Función hash

```
hash(cc)  = cc % 97
índice    = hash(cc) & ((1 << globalDepth) - 1)
```

---

## Algoritmo de inserción

```
1. Verificar que la CC no exista (evitar duplicados).
2. Escribir el registro completo al final de users.dat → obtener dataOffset.
3. Calcular índice de directorio → leer offset del bucket.
4. Si el bucket tiene espacio  → insertar (cc, dataOffset) y retornar.
5. Si el bucket está lleno:
   a. Si localDepth == globalDepth → duplicar el directorio.
   b. Crear nuevo bucket con localDepth + 1.
   c. Redistribuir entradas existentes entre el bucket viejo y el nuevo.
   d. Actualizar punteros del directorio.
   e. Volver al paso 3 (puede requerir múltiples splits).
```

---

## Estructura del proyecto

```
EstructurasDeDatosIntegrador/
├── Storage/
│   ├── HashingStorage.cs       # Motor de hashing extensible (C#)
│   └── User.cs                 # Entidad de usuario (C#)
├── str/
│   ├── HashingStorage.java     # Implementación de referencia (Java)
│   └── User.java
├── EstructurasDeDatosIntegrador.Tests/
│   ├── HashingStorageTests.cs  # 19 pruebas unitarias (NUnit)
│   └── *.csproj
├── Form1.cs                    # Interfaz gráfica (WinForms)
├── EstructurasDeDatosIntegrador.csproj
└── EstructurasDeDatosIntegrador.sln
```

---

## Requisitos

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download) — para ejecutar las pruebas
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con soporte **.NET Framework 4.8.1** — para la aplicación de escritorio

---

## Ejecutar las pruebas

```bash
cd EstructurasDeDatosIntegrador.Tests
dotnet test
```

Salida esperada:

```
Total tests: 19
     Passed: 19
 Total time: ~2.5 Seconds
```

### Casos cubiertos

| Categoría | Pruebas |
|---|---|
| Inicialización | Crea los tres archivos, contador en 0 |
| `AddUser` | Retorna `true`/`false`, no duplica, incrementa contador |
| `GetUser` (hash) | Datos correctos, `null` si no existe |
| `GetUserSeq` (secuencial) | Datos correctos, `null` si no existe, coincide con hash |
| `GetAllUsers` | Lista vacía, todos los usuarios |
| `GetUserCount` | Coincide con el número de inserciones |
| Split de bucket | Tercer usuario en mismo bucket fuerza `doubleDirectory` + split |
| Múltiples usuarios | Varios buckets, accesibles por hash y secuencial |

---

## Detalles de la implementación C#

La clase `HashingStorage` replica exactamente la lógica de `RandomAccessFile` de Java usando `FileStream` con E/S binaria **big-endian** y un formato de cadenas equivalente a `writeUTF`/`readUTF` (prefijo de 2 bytes con la longitud en bytes + UTF-8).

```csharp
var storage = new HashingStorage();
storage.InitializeFiles();

storage.AddUser(new User(123456L, "Ana López", "ana@correo.com"));

User? encontrado = storage.GetUser(123456L);
// encontrado.Cc   → 123456
// encontrado.Name → "Ana López"

string total = storage.GetUserCount(); // "1"
```

---

## Complejidad

| Operación | Caso promedio | Caso peor |
|---|---|---|
| `AddUser` | O(1) | O(n) — múltiples splits en cascada |
| `GetUser` | O(1) | O(1) — un acceso al directorio + un acceso al bucket |
| `GetUserSeq` | O(n) | O(n) — recorrido secuencial del archivo |
| `GetAllUsers` | O(n) | O(n) |
