using System;

namespace AttendanceSystem.Core.DTOs
{
    public sealed class AsistenciaDiariaDto
    {
        public DateTime Fecha     { get; set; }  // fecha del día agrupado
        public int      Total     { get; set; }  // total entradas ese día
        public int      Tardanzas { get; set; }  // de esas entradas, cuántas con tardanza
    }
}
