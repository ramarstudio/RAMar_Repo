namespace AttendanceSystem.Core.DTOs
{
    public sealed class TardanzaEmpleadoDto
    {
        public int    EmpleadoId      { get; set; }
        public string NombreEmpleado  { get; set; }
        public int    TotalTardanzas  { get; set; }
        public int    MinutosTotales  { get; set; }
    }
}
