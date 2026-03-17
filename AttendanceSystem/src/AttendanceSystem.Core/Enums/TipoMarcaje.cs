namespace AttendanceSystem.Core.Enums
{
    /// <summary>
    /// Define los tipos exactos de marcajes permitidos en el sistema.
    /// Evita errores de tipografía y hardcodeo de strings como "ENTRADA" o "entrada".
    /// </summary>
    public enum TipoMarcaje
    {
        Entrada = 1,
        Salida = 2,
        BreakInicio = 3,
        BreakFin = 4
    }
}
