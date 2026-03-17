using System;

namespace AttendanceSystem.Core.DTOs
{
    // Información inmutable de una sesión activa
    public sealed class SessionInfo
    {
        public int UserId { get; }
        public string Username { get; }
        public string Role { get; }
        public string Token { get; }
        public DateTime CreatedAt { get; }
        public DateTime ExpiresAt { get; }
        public bool IsActive => DateTime.UtcNow < ExpiresAt;

        public SessionInfo(int userId, string username, string role, string token, DateTime createdAt, DateTime expiresAt)
        {
            UserId = userId;
            Username = username;
            Role = role;
            Token = token;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        public override string ToString() => $"Session[{UserId}]: {Username} ({Role}) expira {ExpiresAt:dd/MM/yyyy HH:mm}";
    }
}
