namespace AttendanceSystem.Core.Enums
{
    /// <summary>
    /// Enum estandarizado para los días de la semana, alineando los identificadores db
    /// para la creación de horarios. (Se puede castear a System.DayOfWeek de C#)
    /// </summary>
    public enum DiaSemana
    {
        Lunes = 1,
        Martes = 2,
        Miercoles = 3,
        Jueves = 4,
        Viernes = 5,
        Sabado = 6,
        Domingo = 0 // Estándar americano
    }
}
