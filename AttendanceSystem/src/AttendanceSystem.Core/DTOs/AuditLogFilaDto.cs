namespace AttendanceSystem.Core.DTOs
{
    public sealed class AuditLogFilaDto
    {
        public int    Id               { get; set; }
        public string Accion           { get; set; }
        public string Entidad          { get; set; }
        public int    RegistroId       { get; set; }
        public string DatosAnteriores  { get; set; }
        public string DatosNuevos      { get; set; }
        public string Motivo           { get; set; }
        public string Fecha            { get; set; }
        public string Usuario          { get; set; }
    }
}
