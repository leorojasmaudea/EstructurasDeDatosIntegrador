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
        public string          Placa       { get; }
        public TipoVehiculo    Tipo        { get; }
        public TipoTarifa      Tarifa      { get; }
        public System.DateTime HoraEntrada { get; }
        public string          Comentarios { get; }
        public byte[]          Foto        { get; }

        public Vehiculo(string placa, TipoVehiculo tipo, TipoTarifa tarifa,
                        System.DateTime horaEntrada, string comentarios, byte[] foto = null)
        {
            Placa       = placa;
            Tipo        = tipo;
            Tarifa      = tarifa;
            HoraEntrada = horaEntrada;
            Comentarios = comentarios;
            Foto        = foto ?? System.Array.Empty<byte>();
        }
    }
}
