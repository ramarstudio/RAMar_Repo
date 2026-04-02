namespace AttendanceSystem.Core.Enums
{
    /// <summary>
    /// Define los roles formales en el sistema.
    /// Facilita la validación de permisos en la capa de seguridad.
    /// </summary>
    public enum RolUsuario
    {
        SuperAdmin = 0,
        Admin = 1,
        RRHH = 2,
        Empleado = 3
    }
}
