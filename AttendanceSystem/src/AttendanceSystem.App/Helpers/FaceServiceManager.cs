using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.App.Helpers
{
    /// <summary>
    /// Gestiona el ciclo de vida del microservicio Python de reconocimiento facial.
    ///
    /// - Arranque bajo demanda: solo se inicia cuando se necesita
    /// - Auto-stop por inactividad: libera ~400MB de RAM cuando no se usa
    /// - Transparente para el usuario: no necesita saber que existe
    /// - Health check antes de cada operación
    /// </summary>
    public sealed class FaceServiceManager : IFaceServiceLifecycle, IDisposable

    {
        private readonly HttpClient _healthClient;
        private readonly ILogger<FaceServiceManager> _logger;
        private readonly string _serviceUrl;
        private readonly int _idleTimeoutMinutes;

        private Process _serviceProcess;
        private Timer _idleTimer;
        private DateTime _lastUsed;
        private readonly SemaphoreSlim _startLock = new(1, 1);
        private bool _disposed;

        public FaceServiceManager(
            string serviceUrl = "http://localhost:5001",
            int idleTimeoutMinutes = 10,
            ILogger<FaceServiceManager> logger = null)
        {
            _serviceUrl = serviceUrl;
            _idleTimeoutMinutes = idleTimeoutMinutes;
            _logger = logger;
            _healthClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

            // Timer que revisa inactividad cada minuto
            _idleTimer = new Timer(CheckIdle, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Indica si el servicio está corriendo y respondiendo.
        /// </summary>
        public bool IsRunning => _serviceProcess is { HasExited: false };

        /// <summary>
        /// Asegura que el servicio esté listo. Si no está corriendo, lo inicia.
        /// Idempotente — seguro de llamar múltiples veces.
        /// </summary>
        public async Task<bool> EnsureRunningAsync(CancellationToken ct = default)
        {
            _lastUsed = DateTime.UtcNow;

            // Si ya está corriendo, verificar salud
            if (IsRunning)
            {
                if (await HealthCheckAsync(ct))
                    return true;

                // Proceso existe pero no responde — reiniciar
                StopService();
            }

            await _startLock.WaitAsync(ct);
            try
            {
                // Double-check después del lock
                if (IsRunning && await HealthCheckAsync(ct))
                    return true;

                return await StartServiceAsync(ct);
            }
            finally
            {
                _startLock.Release();
            }
        }

        /// <summary>
        /// Marca actividad — resetea el timer de inactividad.
        /// Llamar antes de cada operación biométrica.
        /// </summary>
        public void Touch() => _lastUsed = DateTime.UtcNow;

        /// <summary>
        /// Detiene el servicio inmediatamente para liberar RAM.
        /// </summary>
        public void StopService()
        {
            var proc = _serviceProcess;
            _serviceProcess = null;

            if (proc == null || proc.HasExited) return;

            _logger?.LogInformation("Deteniendo FaceService (PID {Pid})...", proc.Id);

            try
            {
                proc.Kill(entireProcessTree: true);
                proc.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error al detener FaceService.");
            }
            finally
            {
                proc.Dispose();
            }

            _logger?.LogInformation("FaceService detenido. RAM liberada.");
        }

        // ── Internos ─────────────────────────────────────────────────────────

        private async Task<bool> StartServiceAsync(CancellationToken ct)
        {
            string executablePath = ResolveServicePath();
            if (executablePath == null)
            {
                _logger?.LogError("No se encontró el ejecutable/script del FaceService.");
                return false;
            }

            _logger?.LogInformation("Iniciando FaceService desde: {Path}", executablePath);

            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Si es .py, ejecutar con python
            if (executablePath.EndsWith(".py", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = FindPython();
                psi.Arguments = $"\"{executablePath}\"";

                if (string.IsNullOrEmpty(psi.FileName))
                {
                    _logger?.LogError("Python no encontrado en PATH.");
                    return false;
                }
            }

            try
            {
                _serviceProcess = Process.Start(psi);
                if (_serviceProcess == null)
                    return false;

                _logger?.LogInformation("FaceService iniciado (PID {Pid}). Esperando que esté listo...", _serviceProcess.Id);

                // Esperar hasta que responda al health check (máx 30s para cargar modelo)
                return await WaitForReadyAsync(ct);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al iniciar FaceService.");
                return false;
            }
        }

        private async Task<bool> WaitForReadyAsync(CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.AddSeconds(45);

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                if (_serviceProcess is { HasExited: true })
                {
                    _logger?.LogError("FaceService se cerró inesperadamente.");
                    return false;
                }

                if (await HealthCheckAsync(ct))
                {
                    _logger?.LogInformation("FaceService listo.");
                    return true;
                }

                await Task.Delay(1000, ct);
            }

            _logger?.LogError("FaceService no respondió en tiempo.");
            return false;
        }

        private async Task<bool> HealthCheckAsync(CancellationToken ct)
        {
            try
            {
                var response = await _healthClient.GetAsync($"{_serviceUrl}/api/health", ct);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private void CheckIdle(object _)
        {
            if (!IsRunning) return;

            var idleMinutes = (DateTime.UtcNow - _lastUsed).TotalMinutes;
            if (idleMinutes >= _idleTimeoutMinutes)
            {
                _logger?.LogInformation(
                    "FaceService inactivo por {Min} minutos. Deteniendo para liberar RAM.",
                    (int)idleMinutes);
                StopService();
            }
        }

        /// <summary>
        /// Busca el ejecutable/script del servicio en orden de prioridad:
        /// 1. FaceService.exe (empaquetado con PyInstaller)
        /// 2. run.py (desarrollo)
        /// </summary>
        private string ResolveServicePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Buscar ejecutable empaquetado (producción)
            string exePath = Path.Combine(baseDir, "FaceService", "FaceService.exe");
            if (File.Exists(exePath)) return exePath;

            // 2. Buscar en ruta relativa de desarrollo (src/FaceService/run.py)
            string devPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "FaceService", "run.py"));
            if (File.Exists(devPath)) return devPath;

            // 3. Buscar run.py junto al exe
            string localPy = Path.Combine(baseDir, "FaceService", "run.py");
            if (File.Exists(localPy)) return localPy;

            return null;
        }

        private static string FindPython()
        {
            // Buscar python en PATH
            foreach (var name in new[] { "python", "python3" })
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = name,
                        Arguments = "--version",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit(3000);
                    if (proc is { ExitCode: 0 })
                        return name;
                }
                catch { }
            }
            return null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _idleTimer?.Dispose();
            _idleTimer = null;

            StopService();

            _healthClient?.Dispose();
            _startLock?.Dispose();
        }
    }
}
