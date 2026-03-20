namespace AttendanceSystem.Core.DTOs
{
    public sealed class MarcajeResumenDto
    {
        public string NombreEmpleado { get; set; }  // código o nombre resuelto
        public string FechaHora      { get; set; }
        public string Tipo           { get; set; }
        public string EsTardanza     { get; set; }
    }
}
