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

            if (IsRunning)
            {
                // Caso 1: proceso vivo y modelo ya cargado → listo
                if (await IsModelReadyAsync(ct))
                    return true;

                // Caso 2: proceso vivo pero modelo aún cargando → esperar
                if (await IsServerRespondingAsync(ct))
                    return await WaitForModelAsync(ct);

                // Caso 3: proceso colgado o caído → reiniciar
                _logger?.LogWarning("FaceService no responde. Reiniciando...");
                StopService();
            }

            await _startLock.WaitAsync(ct);
            try
            {
                // Double-check tras el lock — misma lógica que el bloque exterior.
                // Si mientras esperábamos el lock otro caller ya arrancó el servicio,
                // no intentar arrancar otro proceso en el mismo puerto.
                if (IsRunning)
                {
                    if (await IsModelReadyAsync(ct)) return true;
                    if (await IsServerRespondingAsync(ct)) return await WaitForModelAsync(ct);
                    StopService();
                }

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
                _logger?.LogError("No se encontró run.py del FaceService.");
                throw new InvalidOperationException(
                    "No se encontró el motor de reconocimiento facial.\n\n" +
                    "Ejecute manualmente para reparar:\n" +
                    "  FaceService\\setup_faceservice.bat");
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
            if (executablePath.EndsWith(".py"))
            {
                psi.FileName = FindPython(executablePath);
                psi.Arguments = $"\"{executablePath}\"";

                if (string.IsNullOrEmpty(psi.FileName))
                {
                    _logger?.LogError("Python no encontrado en venv ni en PATH.");
                    throw new InvalidOperationException(
                        "Python no encontrado. Las librerías de IA no están instaladas.\n\n" +
                        "Instale Python 3.12 y luego ejecute:\n" +
                        "  FaceService\\setup_faceservice.bat");
                }
            }

            try
            {
                _serviceProcess = Process.Start(psi);
                if (_serviceProcess == null)
                    return false;

                // Capturar stderr en background para diagnóstico si el proceso muere pronto
                var stderrBuffer = new System.Text.StringBuilder();
                _serviceProcess.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) stderrBuffer.AppendLine(e.Data);
                };
                _serviceProcess.BeginErrorReadLine();

                _logger?.LogInformation("FaceService iniciado (PID {Pid}). Esperando que esté listo...", _serviceProcess.Id);

                bool ready = await WaitForReadyAsync(ct);

                if (!ready && _serviceProcess is { HasExited: true })
                {
                    var stderr = stderrBuffer.ToString();
                    _logger?.LogError("FaceService terminó inesperadamente. Stderr:\n{Stderr}", stderr);

                    // Detectar errores comunes y lanzar excepción con mensaje claro
                    if (stderr.Contains("No module named"))
                    {
                        var missing = stderr.Contains("insightface") ? "insightface" :
                                      stderr.Contains("onnxruntime")  ? "onnxruntime"  :
                                      stderr.Contains("fastapi")      ? "fastapi"      : "una librería";
                        throw new InvalidOperationException(
                            $"Falta la librería de IA '{missing}'.\n\n" +
                            $"Ejecuta en cmd desde la carpeta del proyecto:\n" +
                            $"  cd AttendanceSystem\\src\\FaceService\n" +
                            $"  venv\\Scripts\\activate.bat\n" +
                            $"  python install.py");
                    }

                    if (stderr.Contains("onnxruntime") || stderr.Contains("ONNX"))
                        throw new InvalidOperationException(
                            "Error en el motor de IA (ONNX Runtime).\n" +
                            "Instala Visual C++ Redistributable desde:\n" +
                            "https://aka.ms/vs/17/release/vc_redist.x64.exe");
                }

                return ready;
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al iniciar FaceService.");
                return false;
            }
        }

        // ── Chequeos de estado ────────────────────────────────────────────────

        /// <summary>El servidor está arriba (aunque el modelo aún esté cargando).</summary>
        private async Task<bool> IsServerRespondingAsync(CancellationToken ct)
        {
            try
            {
                var r = await _healthClient.GetAsync($"{_serviceUrl}/api/health", ct);
                return r.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>El servidor está arriba Y el modelo ya terminó de cargar.</summary>
        private async Task<bool> IsModelReadyAsync(CancellationToken ct)
        {
            try
            {
                var r = await _healthClient.GetAsync($"{_serviceUrl}/api/health", ct);
                if (!r.IsSuccessStatusCode) return false;
                var json = await r.Content.ReadAsStringAsync(ct);

                // Si el FaceService reporta un error de carga, fallar inmediatamente
                if (json.Contains("\"status\":\"error\""))
                {
                    var errorMsg = ExtractJsonField(json, "error");
                    throw new InvalidOperationException(
                        $"El motor de reconocimiento facial falló al cargar:\n{errorMsg}\n\n" +
                        "Ejecute para reparar: FaceService\\setup_faceservice.bat");
                }

                return json.Contains("\"model_loaded\":true");
            }
            catch (InvalidOperationException) { throw; }
            catch { return false; }
        }

        private static string ExtractJsonField(string json, string field)
        {
            var key = $"\"{field}\":\"";
            var idx = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return "error desconocido";
            var start = idx + key.Length;
            var end   = json.IndexOf('"', start);
            return end > start ? json.Substring(start, end - start) : "error desconocido";
        }

        /// <summary>
        /// Espera hasta que el modelo termine de cargarse (máx 5 min).
        /// Se usa cuando el proceso ya está vivo pero el modelo aún carga.
        /// </summary>
        private async Task<bool> WaitForModelAsync(CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.AddSeconds(300);
            _logger?.LogInformation("FaceService arriba. Esperando carga del modelo...");

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                if (_serviceProcess is { HasExited: true })
                {
                    _logger?.LogError("FaceService se cerró mientras cargaba el modelo.");
                    return false;
                }
                if (await IsModelReadyAsync(ct))
                {
                    _logger?.LogInformation("Modelo cargado. FaceService listo.");
                    return true;
                }
                await Task.Delay(2000, ct);
            }

            _logger?.LogError("El modelo no terminó de cargar en 5 minutos.");
            return false;
        }

        /// <summary>
        /// Arranque completo: espera servidor (60s) + espera modelo (5 min).
        /// Solo se usa cuando el proceso acaba de ser lanzado.
        /// </summary>
        private async Task<bool> WaitForReadyAsync(CancellationToken ct)
        {
            var serverDeadline = DateTime.UtcNow.AddSeconds(60);

            while (DateTime.UtcNow < serverDeadline && !ct.IsCancellationRequested)
            {
                if (_serviceProcess is { HasExited: true })
                {
                    _logger?.LogError("FaceService se cerró inesperadamente al arrancar.");
                    return false;
                }
                if (await IsServerRespondingAsync(ct))
                {
                    _logger?.LogInformation("FaceService respondiendo. Esperando carga del modelo...");
                    return await WaitForModelAsync(ct);
                }
                await Task.Delay(1000, ct);
            }

            _logger?.LogError("FaceService no arrancó en 60 segundos.");
            return false;
        }

        private async Task<bool> HealthCheckAsync(CancellationToken ct)
            => await IsModelReadyAsync(ct);

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

            // 3. Buscar desde publish/ → ../src/FaceService/run.py
            string publishPath = Path.GetFullPath(Path.Combine(baseDir, "..", "src", "FaceService", "run.py"));
            if (File.Exists(publishPath)) return publishPath;

            // 4. Buscar run.py junto al exe
            string localPy = Path.Combine(baseDir, "FaceService", "run.py");
            if (File.Exists(localPy)) return localPy;

            return null;
        }

        private static string FindPython(string scriptPath)
        {
            // 1. Buscar en el entorno virtual aislado (venv)
            string scriptDir = Path.GetDirectoryName(scriptPath);
            if (!string.IsNullOrEmpty(scriptDir))
            {
                string venvPython = Path.Combine(scriptDir, "venv", "Scripts", "python.exe");
                if (File.Exists(venvPython)) return venvPython;
            }

            // 2. Buscar python global en PATH
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
