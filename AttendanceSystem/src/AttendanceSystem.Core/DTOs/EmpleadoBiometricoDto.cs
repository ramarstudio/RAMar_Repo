namespace AttendanceSystem.Core.DTOs
{
    public sealed class EmpleadoBiometricoDto
    {
        public int    Id                    { get; set; }
        public string Codigo                { get; set; }
        public string Nombre                { get; set; }
        public bool   TieneEmbedding        { get; set; }
        public bool   TieneConsentimiento   { get; set; }

        public string EstadoIcono => TieneEmbedding ? "CheckCircle" : "CloseCircle";
        public string EstadoTexto => TieneEmbedding ? "Registrado" : "Sin registro facial";
    }
}
