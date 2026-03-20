using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.Core.DTOs
{
    public sealed class RolSelectorDto
    {
        public int        Id    { get; set; }
        public RolUsuario Valor { get; set; }
        public string     Nombre => Valor.ToString();
    }
}
