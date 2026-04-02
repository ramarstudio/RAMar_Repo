using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Options;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.Services
{
    public class BiometricoService : IBiometricoService
    {
        private readonly HttpClient                  _httpClient;
        private readonly IConsentimientoRepository   _consentimientoRepository;
        private readonly IEmpleadoRepository         _empleadoRepository;
        private readonly IEncryptionService          _encryptionService;
        private readonly FacialServiceOptions        _options;
        private readonly ILogger<BiometricoService>  _logger;
        private readonly IFaceServiceLifecycle       _faceServiceLifecycle;

        public BiometricoService(
            HttpClient                 httpClient,
            IConsentimientoRepository  consentimientoRepository,
            IEmpleadoRepository        empleadoRepository,
            IEncryptionService         encryptionService,
            FacialServiceOptions       options,
            ILogger<BiometricoService> logger,
            IFaceServiceLifecycle      faceServiceLifecycle = null)
        {
            _httpClient               = httpClient;
            _consentimientoRepository = consentimientoRepository;
            _empleadoRepository       = empleadoRepository;
            _encryptionService        = encryptionService;
            _options                  = options ?? throw new ArgumentNullException(nameof(options));
            _logger                   = logger;
            _faceServiceLifecycle     = faceServiceLifecycle;
        }

        public async Task<bool> VerificarIdentidadAsync(string base64Image, string codigoEmpleado)
        {
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null)
                throw new InvalidOperationException($"Empleado '{codigoEmpleado}' no encontrado.");
            if (!empleado.TieneEmbedding())
                throw new InvalidOperationException($"El empleado '{codigoEmpleado}' no tiene un rostro registrado. Pida al administrador que registre su biometría.");

            var embeddingFacial = empleado.GetEmbeddingFacial();

            float[] vectorConocido;
            try
            {
                var encryptedStr = Encoding.UTF8.GetString(embeddingFacial.GetVectorCifrado());
                var vectorJson   = _encryptionService.Decrypt(encryptedStr);
                vectorConocido   = JsonSerializer.Deserialize<float[]>(vectorJson);
                if (vectorConocido == null)
                    throw new InvalidOperationException("El vector facial almacenado está corrupto.");
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descifrar embedding de {Codigo}.", codigoEmpleado);
                throw new InvalidOperationException($"Error al descifrar datos biométricos: {ex.Message}");
            }

            var requestPayload = new { image_base64 = base64Image, known_embedding = vectorConocido };

            // Arrancar FaceService bajo demanda si no está activo
            if (!await EnsureFaceServiceAsync())
                throw new InvalidOperationException("No se pudo iniciar el servicio facial.");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(_options.VerifyPath, requestPayload);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Microservicio Python no disponible en {Path}.", _options.VerifyPath);
                throw new InvalidOperationException($"Servicio facial no disponible: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException("Tiempo de espera agotado al conectar con el servicio facial.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Verificación devolvió {Status}: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Servicio facial respondió con error ({response.StatusCode}): {errorBody}");
            }

            PythonMatchResponse matchResult;
            try
            {
                matchResult = await response.Content.ReadFromJsonAsync<PythonMatchResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de verificación.");
                throw new InvalidOperationException($"Error al procesar respuesta del servicio: {ex.Message}");
            }

            if (matchResult == null)
                throw new InvalidOperationException("El servicio facial devolvió una respuesta vacía.");

            decimal umbral = embeddingFacial.GetUmbral();
            bool reconocido = matchResult.Match && matchResult.Confidence >= umbral;
            _logger.LogInformation("Verificación {Codigo}: match={M}, confidence={C}, umbral={U}.",
                codigoEmpleado, matchResult.Match, matchResult.Confidence, umbral);
            return reconocido;
        }

        public async Task<bool> RegistrarNuevoRostroAsync(string base64Image, string codigoEmpleado)
        {
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null)
                throw new ArgumentException("Empleado no encontrado.", nameof(codigoEmpleado));

            var consentimiento = await _consentimientoRepository.GetByEmpleadoIdAsync(empleado.GetId());
            if (consentimiento == null || !consentimiento.EstaAutorizado())
                throw new InvalidOperationException(
                    "No se puede registrar biometría sin consentimiento legal firmado y autorizado.");

            var requestPayload = new { image_base64 = base64Image };

            // Arrancar FaceService bajo demanda si no está activo
            if (!await EnsureFaceServiceAsync())
                throw new InvalidOperationException(
                    "No se pudo iniciar el servicio facial. Verifique que Python esté instalado y que la carpeta FaceService exista.");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(_options.EncodePath, requestPayload);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Microservicio Python no disponible en {Path}.", _options.EncodePath);
                throw new InvalidOperationException(
                    $"No se pudo conectar al servicio facial en {_options.EncodePath}. Error: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException(
                    "Tiempo de espera agotado al conectar con el servicio facial. El servicio puede estar iniciándose.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("FaceService respondió {Code}: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException(
                    $"El servicio facial respondió con error ({response.StatusCode}): {errorBody}");
            }

            PythonEncodeResponse encodeResult;
            try
            {
                encodeResult = await response.Content.ReadFromJsonAsync<PythonEncodeResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de encode.");
                throw new InvalidOperationException($"Error al procesar la respuesta del servicio facial: {ex.Message}");
            }

            if (encodeResult?.Embedding == null)
                throw new InvalidOperationException(
                    "El servicio facial no detectó un rostro en la imagen. Asegúrese de estar frente a la cámara.");

            var vectorJson    = JsonSerializer.Serialize(encodeResult.Embedding);
            var vectorCifrado = Encoding.UTF8.GetBytes(_encryptionService.Encrypt(vectorJson));

            var nuevoEmbedding = new EmbeddingFacial();
            nuevoEmbedding.SetEmpleadoId(empleado.GetId());
            nuevoEmbedding.SetVectorCifrado(vectorCifrado);
            nuevoEmbedding.SetAlgoritmo("AES-256-GCM");
            nuevoEmbedding.SetUmbral(0.60m);
            nuevoEmbedding.SetVersionModelo("v1.0");
            nuevoEmbedding.SetCreadoEn(DateTime.UtcNow);

            empleado.SetEmbeddingFacial(nuevoEmbedding);
            await _empleadoRepository.UpdateAsync(empleado);

            _logger.LogInformation("Embedding registrado para empleado {Codigo}.", codigoEmpleado);
            return true;
        }


        /// <summary>
        /// Arranca el FaceService Python si no está activo.
        /// Devuelve true si está listo, false si no se pudo arrancar.
        /// </summary>
        private async Task<bool> EnsureFaceServiceAsync()
        {
            if (_faceServiceLifecycle == null)
            {
                // Sin lifecycle manager (tests o desarrollo sin Python) → asumir que está corriendo
                return true;
            }

            _faceServiceLifecycle.Touch();

            if (_faceServiceLifecycle.IsRunning)
                return true;

            _logger.LogInformation("FaceService no está activo. Iniciando bajo demanda...");

            bool ready = await _faceServiceLifecycle.EnsureRunningAsync();
            if (!ready)
            {
                _logger.LogError("No se pudo iniciar el FaceService. Verifique que Python esté instalado.");
                return false;
            }

            _logger.LogInformation("FaceService iniciado correctamente.");
            return true;
        }

        private sealed class PythonEncodeResponse
        {
            public float[] Embedding { get; set; }
        }

        private sealed class PythonMatchResponse
        {
            public bool    Match      { get; set; }
            public decimal Confidence { get; set; }
        }
    }
}
