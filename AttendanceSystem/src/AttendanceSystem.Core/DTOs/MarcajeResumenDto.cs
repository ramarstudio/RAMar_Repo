namespace AttendanceSystem.Core.DTOs
{
    public sealed class MarcajeResumenDto
    {
        public string EmpleadoId { get; set; }
        public string FechaHora  { get; set; }
        public string Tipo       { get; set; }
        public string EsTardanza { get; set; }
    }
}
