namespace AttendanceSystem.Core.Enums
{
    /// <summary>
    /// Especifica cómo se verificó la identidad del empleado.
    /// Vital para auditoría de seguridad y confianza en el sistema.
    /// </summary>
    public enum MetodoVerificacion
    {
        ReconocimientoFacial = 1,
        AsistidoPorAdmin = 2, // Cuándo un admin registra manualmente un marcaje válido
        Contrasena = 3 // Como contingencia si falla la biometría
    }
}
