using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    // Gestión de sesiones con concurrencia y expiración automática
    public interface ISessionManager
    {
        string CreateSession(int userId, string username, string role);
        SessionInfo GetSession(string token);
        bool InvalidateSession(string token);
        void CleanExpiredSessions();
    }
}
