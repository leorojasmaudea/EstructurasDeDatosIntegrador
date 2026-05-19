namespace EstructurasDeDatosIntegrador.Storage
{
    internal enum TipoTarifa
    {
        PorHora,
        PorDia
    }

    // AplicaA == null  →  aplica a todos los tipos de vehículo.
    // HoraInicio / HoraFin definen el tramo horario en que rige la tarifa (0–24).
    internal class Tarifa
    {
        public string        Codigo      { get; }
        public string        Nombre      { get; }
        public TipoTarifa    Tipo        { get; }
        public TipoVehiculo? AplicaA     { get; }
        public double        Valor       { get; }
        public int           HoraInicio  { get; }  // 0–23
        public int           HoraFin     { get; }  // 1–24  (intervalo semi-abierto [inicio, fin))
        public string        Descripcion { get; }

        public Tarifa(string codigo, string nombre, TipoTarifa tipo,
                      TipoVehiculo? aplicaA, double valor,
                      int horaInicio, int horaFin,
                      string descripcion = "")
        {
            Codigo      = codigo;
            Nombre      = nombre;
            Tipo        = tipo;
            AplicaA     = aplicaA;
            Valor       = valor;
            HoraInicio  = horaInicio;
            HoraFin     = horaFin;
            Descripcion = descripcion ?? System.String.Empty;
        }
    }
}
