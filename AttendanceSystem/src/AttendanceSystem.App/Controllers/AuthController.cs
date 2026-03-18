using System;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Security;

namespace AttendanceSystem.App.Controllers
{
    public class AuthController
    {
        private readonly IAuthService _authService;
        private readonly SessionManager _sessionManager;
        private readonly NavigationController _navigationController;

        public AuthController(
            IAuthService authService, 
            SessionManager sessionManager, 
            NavigationController navigationController)
        {
            _authService = authService;
            _sessionManager = sessionManager;
            _navigationController = navigationController;
        }

        public async Task<(bool Exito, string Mensaje)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "El usuario y la contraseña son obligatorios.");
            }

            try
            {
                var request = new LoginRequest { Username = username, Password = password };
                
                // 1. Llamar al servicio de la Etapa 3 para validar credenciales
                var usuario = await _authService.ValidarCredencialesAsync(request);

                if (usuario != null)
                {
                    // 2. Guardar el usuario en la sesión global
                    _sessionManager.IniciarSesion(usuario);

                    // 3. Redirigir a la pantalla que le corresponda
                    _navigationController.NavegarAlMenuPrincipal();
                    
                    return (true, "Login exitoso.");
                }
                
                return (false, "Credenciales incorrectas.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void Logout()
        {
            // 1. Limpiar sesión
            _sessionManager.CerrarSesion();
            
            // 2. Regresar a la pantalla de login
            _navigationController.NavegarALogin();
        }
    }
}