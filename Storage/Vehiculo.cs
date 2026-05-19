namespace EstructurasDeDatosIntegrador.Storage
{
    internal enum TipoVehiculo
    {
        Carro,
        Moto,
        Camion,
        Bus,
        Bicicleta,
        Otro
    }

    internal class Vehiculo
    {
        public string       Placa       { get; }
        public TipoVehiculo Tipo        { get; }
        public string       Comentarios { get; }
        public byte[]       Foto        { get; }

        public Vehiculo(string placa, TipoVehiculo tipo, string comentarios, byte[] foto = null)
        {
            Placa       = placa;
            Tipo        = tipo;
            Comentarios = comentarios;
            Foto        = foto ?? System.Array.Empty<byte>();
        }
    }
}
