namespace AttendanceSystem.Core.DTOs
{
    public sealed class HorarioFilaDto
    {
        public int    Id            { get; set; }
        public string Empleado      { get; set; }
        public int    EmpleadoId    { get; set; }
        public string Dia           { get; set; }
        public string Entrada       { get; set; }
        public string Salida        { get; set; }
        public string VigenteDesde  { get; set; }
        public string VigenteHasta  { get; set; }
    }
}
