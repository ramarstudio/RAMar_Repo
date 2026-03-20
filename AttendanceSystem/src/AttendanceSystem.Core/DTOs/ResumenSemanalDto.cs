namespace AttendanceSystem.Core.DTOs
{
    public sealed class ResumenSemanalDto
    {
        public string DiaNombre   { get; set; }  // Lun, Mar, Mié...
        public string FechaCorta  { get; set; }  // 18/03
        public int    Asistencias { get; set; }
        public int    Tardanzas   { get; set; }
        public int    Ausencias   { get; set; }
        public int    Total       { get; set; }  // empleados activos ese día
    }
}
