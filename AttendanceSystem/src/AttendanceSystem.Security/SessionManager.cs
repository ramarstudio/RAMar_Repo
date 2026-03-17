using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Security
{
    // Configuración de sesiones (evita primitivas sueltas)
    public sealed class SessionOptions
    {
        public int DurationMinutes { get; }
        public int TokenSizeBytes { get; }
        public int CleanupIntervalMinutes { get; }

        public SessionOptions(int durationMinutes = 30, int tokenSizeBytes = 32, int cleanupIntervalMinutes = 5)
        {
            if (durationMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationMinutes));
            if (tokenSizeBytes < 16)
                throw new ArgumentOutOfRangeException(nameof(tokenSizeBytes), "Mínimo 16 bytes.");
            if (cleanupIntervalMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(cleanupIntervalMinutes));

            DurationMinutes = durationMinutes;
            TokenSizeBytes = tokenSizeBytes;
            CleanupIntervalMinutes = cleanupIntervalMinutes;
        }
    }

    // Sesiones en memoria con concurrencia y limpieza automática
    public sealed class SessionManager : ISessionManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new ConcurrentDictionary<string, SessionInfo>();
        private readonly ReaderWriterLockSlim _cleanupLock = new ReaderWriterLockSlim();
        private readonly Timer _cleanupTimer;
        private readonly SessionOptions _options;
        private bool _disposed;

        public SessionManager(SessionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _cleanupTimer = new Timer(
                _ => CleanExpiredSessions(), null,
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
        }

        public string CreateSession(int userId, string username, string role)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("El nombre de usuario es obligatorio.", nameof(username));
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("El rol es obligatorio.", nameof(role));

            string token = GenerateSecureToken();
            var now = DateTime.UtcNow;

            var session = new SessionInfo(userId, username, role, token, now, now.AddMinutes(_options.DurationMinutes));
            _sessions.TryAdd(token, session);
            return token;
        }

        public SessionInfo GetSession(string token)
        {
            if (string.IsNullOrEmpty(token) || !_sessions.TryGetValue(token, out SessionInfo session))
                return null;

            if (!session.IsActive)
            {
                _sessions.TryRemove(token, out _);
                return null;
            }

            return session;
        }

        public bool InvalidateSession(string token)
        {
            return !string.IsNullOrEmpty(token) && _sessions.TryRemove(token, out _);
        }

        public void CleanExpiredSessions()
        {
            if (!_cleanupLock.TryEnterWriteLock(TimeSpan.FromSeconds(5)))
                return;

            try
            {
                foreach (var kvp in _sessions)
                {
                    if (!kvp.Value.IsActive)
                        _sessions.TryRemove(kvp.Key, out _);
                }
            }
            finally { _cleanupLock.ExitWriteLock(); }
        }

        // Token criptográfico URL-safe
        private string GenerateSecureToken()
        {
            byte[] tokenBytes = CryptoHelper.GenerateRandomBytes(_options.TokenSizeBytes);

            return Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public void Dispose()
        {
            if (_disposed) return;
            _cleanupTimer?.Dispose();
            _cleanupLock?.Dispose();
            _disposed = true;
        }
    }
}
