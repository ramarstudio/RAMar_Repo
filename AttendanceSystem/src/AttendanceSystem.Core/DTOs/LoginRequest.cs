using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Core.DTOs
{
    /// <summary>
    /// Objeto de Transferencia de Datos para el inicio de sesión.
    /// Inmutable y validado para prevenir inyección o datos maliciosos en la capa superficial.
    /// </summary>
    public class LoginRequest
    {
        // El atributo Required asegura que la API no acepte peticiones sin este campo
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        public string Password { get; set; }

        public LoginRequest() { }
    }
}
