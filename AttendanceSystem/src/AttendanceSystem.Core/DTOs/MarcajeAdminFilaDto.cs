namespace AttendanceSystem.Core.DTOs
{
    public sealed class MarcajeAdminFilaDto
    {
        public int    MarcajeId { get; set; }
        public string Fecha     { get; set; }
        public string Hora      { get; set; }
        public string Tipo      { get; set; }
        public string Tardanza  { get; set; }
        public string Minutos   { get; set; }
        public string Metodo    { get; set; }
    }
}
