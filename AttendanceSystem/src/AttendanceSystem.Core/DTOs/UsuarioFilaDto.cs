namespace AttendanceSystem.Core.DTOs
{
    public sealed class UsuarioFilaDto
    {
        public int    Id            { get; set; }
        public string Username      { get; set; }
        public string Nombre        { get; set; }
        public string Rol           { get; set; }
        public string Estado        { get; set; }
        public string FechaCreacion { get; set; }
    }
}
