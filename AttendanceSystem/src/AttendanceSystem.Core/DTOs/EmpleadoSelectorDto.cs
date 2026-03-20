namespace AttendanceSystem.Core.DTOs
{
    public sealed class EmpleadoSelectorDto
    {
        public int    Id     { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Display => string.IsNullOrEmpty(Nombre) ? Codigo : $"{Nombre}  ({Codigo})";
    }
}
