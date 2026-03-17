using System;

namespace AttendanceSystem.Core.DTOs
{
    /// <summary>
    /// Objeto abstracto flexible que puede representar un resumen de asistencia
    /// utilizado para reportería de administradores y RRHH.
    /// </summary>
    public class ReporteDto
    {
        public int EmpleadoId { get; set; }
        public string CodigoEmpleado { get; set; }
        
        public int TotalAsistencias { get; set; }
        public int TotalTardanzas { get; set; }
        public int TotalFaltas { get; set; }
        
        public int SumatoriaMinutosTardanza { get; set; }
        
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFin { get; set; }

        public ReporteDto() { }
    }
}
