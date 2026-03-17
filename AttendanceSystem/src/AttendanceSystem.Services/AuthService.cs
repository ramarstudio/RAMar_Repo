using System;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    // Autenticación (mismo patrón DI que AuditService)
    public sealed class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;

        // Inyección de dependencias
        public AuthService(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<Usuario> ValidarCredencialesAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var usuario = await _usuarioRepository.GetByUsernameAsync(request.Username);

            if (usuario == null || !usuario.EstaActivo())
                return null;

            return _passwordHasher.VerifyPassword(request.Password, usuario.GetPassword()) ? usuario : null;
        }

        public string GenerarHashContrasena(string passwordPlano)
        {
            if (string.IsNullOrEmpty(passwordPlano))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(passwordPlano));

            return _passwordHasher.HashPassword(passwordPlano);
        }

        public bool VerificarContrasena(string passwordPlano, string hashGuardado)
        {
            if (string.IsNullOrEmpty(passwordPlano) || string.IsNullOrEmpty(hashGuardado))
                return false;

            return _passwordHasher.VerifyPassword(passwordPlano, hashGuardado);
        }
    }
}
