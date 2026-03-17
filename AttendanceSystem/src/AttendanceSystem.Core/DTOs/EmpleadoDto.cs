using System;

namespace AttendanceSystem.Core.DTOs
{
    /// <summary>
    /// DTO para transferir información pública/segura del empleado.
    /// Nunca debe contener información biométrica pura ni credenciales.
    /// </summary>
    public class EmpleadoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        
        /// <summary>
        /// Enviamos solo las horas representativas, pero puede ser string para mejor serialización en JSON (ej "08:00").
        /// </summary>
        public string HorarioEntrada { get; set; }
        public string HorarioSalida { get; set; }
        
        public int ToleranciaMinutos { get; set; }
        public bool Activo { get; set; }
        
        // Información resumida de sus relaciones
        public bool TieneBiometriaRegistrada { get; set; }
        public bool TieneConsentimientoFirmado { get; set; }

        public EmpleadoDto() { }
    }
}
