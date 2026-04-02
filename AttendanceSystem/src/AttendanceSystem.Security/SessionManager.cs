using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Security
{
    public sealed class SessionOptions
    {
        public int DurationMinutes         { get; }
        public int TokenSizeBytes          { get; }
        public int CleanupIntervalMinutes  { get; }

        public SessionOptions(int durationMinutes = 30, int tokenSizeBytes = 32, int cleanupIntervalMinutes = 5)
        {
            if (durationMinutes <= 0)       throw new ArgumentOutOfRangeException(nameof(durationMinutes));
            if (tokenSizeBytes < 16)        throw new ArgumentOutOfRangeException(nameof(tokenSizeBytes), "Mínimo 16 bytes.");
            if (cleanupIntervalMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(cleanupIntervalMinutes));

            DurationMinutes        = durationMinutes;
            TokenSizeBytes         = tokenSizeBytes;
            CleanupIntervalMinutes = cleanupIntervalMinutes;
        }
    }

    // Implementa ISessionManager completo: operaciones de infraestructura + helpers WPF.
    // Singleton — una única sesión activa por instancia de aplicación.
    public sealed class SessionManager : ISessionManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
        private readonly ReaderWriterLockSlim _cleanupLock = new();
        private readonly Timer               _cleanupTimer;
        private readonly SessionOptions      _options;
        private string                       _currentSessionToken;
        private bool                         _disposed;

        public SessionManager(SessionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _cleanupTimer = new Timer(
                _ => CleanExpiredSessions(), null,
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
        }

        // ── Operaciones de infraestructura ───────────────────────────────────

        public string CreateSession(int userId, string username, string role)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username obligatorio.", nameof(username));
            if (string.IsNullOrEmpty(role))     throw new ArgumentException("Role obligatorio.", nameof(role));

            var now   = DateTime.UtcNow;
            var token = GenerateSecureToken();
            var info  = new SessionInfo(userId, username, role, token, now, now.AddMinutes(_options.DurationMinutes));
            _sessions.TryAdd(token, info);
            return token;
        }

        public SessionInfo GetSession(string token)
        {
            if (string.IsNullOrEmpty(token) || !_sessions.TryGetValue(token, out var session))
                return null;

            if (!session.IsActive)
            {
                _sessions.TryRemove(token, out _);
                return null;
            }
            return session;
        }

        public bool InvalidateSession(string token)
            => !string.IsNullOrEmpty(token) && _sessions.TryRemove(token, out _);

        public void CleanExpiredSessions()
        {
            if (!_cleanupLock.TryEnterWriteLock(TimeSpan.FromSeconds(5))) return;
            try
            {
                foreach (var kvp in _sessions)
                    if (!kvp.Value.IsActive) _sessions.TryRemove(kvp.Key, out _);
            }
            finally { _cleanupLock.ExitWriteLock(); }
        }

        // ── Helpers de ciclo de vida para la capa de presentación ────────────

        public void IniciarSesion(Usuario usuario)
        {
            var rol       = usuario.GetRol();
            string nombre = rol?.GetNombre().ToString() ?? nameof(RolUsuario.Empleado);
            _currentSessionToken = CreateSession(usuario.GetId(), usuario.GetUsername(), nombre);
        }

        public void CerrarSesion()
        {
            if (!string.IsNullOrEmpty(_currentSessionToken))
            {
                InvalidateSession(_currentSessionToken);
                _currentSessionToken = null;
            }
        }

        public bool EstaLogueado()
        {
            if (string.IsNullOrEmpty(_currentSessionToken)) return false;
            var session = GetSession(_currentSessionToken);
            return session != null && session.IsActive;
        }

        public bool EsSuperAdmin()
        {
            var session = GetSession(_currentSessionToken);
            return session != null && session.Role == nameof(RolUsuario.SuperAdmin);
        }

        public bool EsAdministrador()
        {
            var session = GetSession(_currentSessionToken);
            return session != null
                && (session.Role == nameof(RolUsuario.Admin) || session.Role == nameof(RolUsuario.SuperAdmin));
        }

        public bool EsRRHH()
        {
            var session = GetSession(_currentSessionToken);
            return session != null && session.Role == nameof(RolUsuario.RRHH);
        }

        public SessionInfo GetCurrentSession()    => GetSession(_currentSessionToken);
        public SessionInfo ObtenerUsuarioActual() => GetCurrentSession();

        // ── Token criptográfico URL-safe ─────────────────────────────────────

        private string GenerateSecureToken()
        {
            byte[] bytes = CryptoHelper.GenerateRandomBytes(_options.TokenSizeBytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
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
