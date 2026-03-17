using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.Core.DTOs
{
    /// <summary>
    /// DTO que encapsula la solicitud de marcaje (Entrada, Salida, Break).
    /// </summary>
    public class MarcajeRequest
    {
        [Required(ErrorMessage = "El código del empleado es obligatorio.")]
        [StringLength(20)]
        public string CodigoEmpleado { get; set; }

        [Required(ErrorMessage = "El tipo de marcaje es obligatorio.")]
        [JsonConverter(typeof(JsonStringEnumConverter))] // Permite que el Front envíe "Entrada" en texto en JSON y C# lo convierta al Enum automáticamente.
        public TipoMarcaje Tipo { get; set; }

        /// <summary>
        /// Representa los datos del reconocimiento facial en Base64 o un token validado.
        /// Abstraemos la biometría como un string validable.
        /// </summary>
        [Required(ErrorMessage = "Los datos biométricos son obligatorios para validar la identidad.")]
        public string DatosBiometricos { get; set; }

        public MarcajeRequest() { }
    }
}
