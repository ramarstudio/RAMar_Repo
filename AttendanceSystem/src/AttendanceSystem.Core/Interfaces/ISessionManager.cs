using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    // Contrato completo de gestión de sesión.
    // Separa las operaciones de infraestructura (Create/Get/Invalidate)
    // de los helpers semánticos para WPF (IniciarSesion, EsAdministrador, etc.).
    public interface ISessionManager
    {
        // ── Operaciones de infraestructura ───────────────────────────────────
        string      CreateSession(int userId, string username, string role);
        SessionInfo GetSession(string token);
        bool        InvalidateSession(string token);
        void        CleanExpiredSessions();

        // ── Helpers de ciclo de vida para la capa de presentación ───────────
        void        IniciarSesion(Usuario usuario);
        void        CerrarSesion();
        bool        EstaLogueado();
        bool        EsAdministrador();
        SessionInfo GetCurrentSession();
        SessionInfo ObtenerUsuarioActual();
    }
}
