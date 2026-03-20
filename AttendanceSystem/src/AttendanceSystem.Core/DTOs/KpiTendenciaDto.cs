namespace AttendanceSystem.Core.DTOs
{
    public sealed class KpiTendenciaDto
    {
        public double ValorActual    { get; set; }
        public double ValorAnterior  { get; set; }
        public double Diferencia     => ValorActual - ValorAnterior;
        public bool   Subio          => Diferencia > 0;
        public bool   Bajo           => Diferencia < 0;
        public bool   Igual          => Diferencia == 0;
        public string DiferenciaTexto => Diferencia > 0 ? $"+{Diferencia:0.#}" : $"{Diferencia:0.#}";
    }
}
