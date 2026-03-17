using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario> ValidarCredencialesAsync(LoginRequest request);
        string GenerarHashContrasena(string passwordPlano);
        bool VerificarContrasena(string passwordPlano, string hashGuardado);
    }
}
