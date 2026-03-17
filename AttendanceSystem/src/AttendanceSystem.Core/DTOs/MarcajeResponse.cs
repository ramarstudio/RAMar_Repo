using System;

namespace AttendanceSystem.Core.DTOs
{
    /// <summary>
    /// DTO que encapsula la respuesta del sistema al registrar un marcaje.
    /// Inmutable para evitar modificaciones accidentales en tránsito.
    /// </summary>
    public class MarcajeResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        
        /// <summary>
        /// Representa la fecha y hora oficial del servidor en el momento exacto del registro.
        /// Previene ataques o alteraciones donde el cliente modifique la hora.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        public bool EsTardanza { get; set; }
        public int MinutosTardanza { get; set; }

        public MarcajeResponse() { }
    }
}
