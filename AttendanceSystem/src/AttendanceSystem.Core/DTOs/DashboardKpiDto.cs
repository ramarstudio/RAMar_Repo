namespace AttendanceSystem.Core.DTOs
{
    public sealed class DashboardKpiDto
    {
        public int    TotalEmpleadosActivos { get; set; }
        public int    AsistenciasHoy        { get; set; }
        public int    TardanzasHoy          { get; set; }
        public int    TotalUsuarios         { get; set; }
        public double TasaAsistenciaHoy     { get; set; }
    }
}
