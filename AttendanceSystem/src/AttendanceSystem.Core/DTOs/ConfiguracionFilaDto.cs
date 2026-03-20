namespace AttendanceSystem.Core.DTOs
{
    public sealed class ConfiguracionFilaDto
    {
        public int    Id          { get; set; }
        public string Clave       { get; set; }
        public string Valor       { get; set; }
        public string TipoDato    { get; set; }
        public string Descripcion { get; set; }
    }
}
