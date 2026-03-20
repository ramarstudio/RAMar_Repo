using System;

namespace AttendanceSystem.Core.Options
{
    public sealed class EmpleadoDefaultOptions
    {
        public int HorarioEntradaHora  { get; }
        public int HorarioSalidaHora   { get; }
        public int ToleranciaMins      { get; }

        public EmpleadoDefaultOptions(int entradaHora = 8, int salidaHora = 17, int toleranciaMins = 15)
        {
            if (entradaHora < 0 || entradaHora > 23)
                throw new ArgumentOutOfRangeException(nameof(entradaHora));
            if (salidaHora < 0 || salidaHora > 23)
                throw new ArgumentOutOfRangeException(nameof(salidaHora));
            if (toleranciaMins < 0)
                throw new ArgumentOutOfRangeException(nameof(toleranciaMins));

            HorarioEntradaHora = entradaHora;
            HorarioSalidaHora  = salidaHora;
            ToleranciaMins     = toleranciaMins;
        }
    }
}
