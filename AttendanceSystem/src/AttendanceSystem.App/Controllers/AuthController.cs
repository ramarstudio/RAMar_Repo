using System;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    public class AuthController
    {
        private readonly IAuthService        _authService;
        private readonly ISessionManager     _sessionManager;
        private readonly NavigationController _navigationController;

        public AuthController(
            IAuthService         authService,
            ISessionManager      sessionManager,
            NavigationController navigationController)
        {
            _authService          = authService;
            _sessionManager       = sessionManager;
            _navigationController = navigationController;
        }

        public async Task<(bool Exito, string Mensaje)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "El usuario y la contraseña son obligatorios.");

            try
            {
                var request = new LoginRequest { Username = username, Password = password };
                var usuario = await _authService.ValidarCredencialesAsync(request);

                if (usuario == null)
                    return (false, "Credenciales incorrectas.");

                _sessionManager.IniciarSesion(usuario);
                _navigationController.NavegarAlMenuPrincipal();
                return (true, "Login exitoso.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void Logout()
        {
            _sessionManager.CerrarSesion();
            _navigationController.NavegarALogin();
        }
    }
}
