using System;

namespace AttendanceSystem.Core.DTOs
{
    public sealed class HeatmapDiaDto
    {
        public DateTime Fecha       { get; set; }
        public int      Asistencias { get; set; }
        public int      Tardanzas   { get; set; }
        public int      Ausencias   { get; set; }
    }
}
