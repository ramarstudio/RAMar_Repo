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

        public BiometricoService(
            HttpClient                 httpClient,
            IConsentimientoRepository  consentimientoRepository,
            IEmpleadoRepository        empleadoRepository,
            IEncryptionService         encryptionService,
            FacialServiceOptions       options,
            ILogger<BiometricoService> logger)
        {
            _httpClient               = httpClient;
            _consentimientoRepository = consentimientoRepository;
            _empleadoRepository       = empleadoRepository;
            _encryptionService        = encryptionService;
            _options                  = options ?? throw new ArgumentNullException(nameof(options));
            _logger                   = logger;
        }

        public async Task<bool> VerificarIdentidadAsync(string base64Image, string codigoEmpleado)
        {
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null || !empleado.TieneEmbedding()) return false;

            var embeddingFacial = empleado.GetEmbeddingFacial();

            float[] vectorConocido;
            try
            {
                var encryptedStr = Encoding.UTF8.GetString(embeddingFacial.GetVectorCifrado());
                var vectorJson   = _encryptionService.Decrypt(encryptedStr);
                vectorConocido   = JsonSerializer.Deserialize<float[]>(vectorJson);
                if (vectorConocido == null) return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descifrar embedding de {Codigo}.", codigoEmpleado);
                return false;
            }

            var requestPayload = new { image_base64 = base64Image, known_embedding = vectorConocido };

            HttpResponseMessage response;
            try
            {
                // Usa la BaseAddress del HttpClient + ruta relativa desde appsettings
                response = await _httpClient.PostAsJsonAsync(_options.VerifyPath, requestPayload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Microservicio Python no disponible en {Path}.", _options.VerifyPath);
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Verificación devolvió {Status}.", response.StatusCode);
                return false;
            }

            PythonMatchResponse matchResult;
            try
            {
                matchResult = await response.Content.ReadFromJsonAsync<PythonMatchResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de verificación.");
                return false;
            }

            if (matchResult == null) return false;

            decimal umbral = embeddingFacial.GetUmbral();
            bool reconocido = matchResult.Match && matchResult.Confidence >= umbral;
            _logger.LogDebug("Verificación {Codigo}: match={M}, confidence={C}, umbral={U}.",
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

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(_options.EncodePath, requestPayload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Microservicio Python no disponible en {Path}.", _options.EncodePath);
                return false;
            }

            if (!response.IsSuccessStatusCode) return false;

            PythonEncodeResponse encodeResult;
            try
            {
                encodeResult = await response.Content.ReadFromJsonAsync<PythonEncodeResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de encode.");
                return false;
            }

            if (encodeResult?.Embedding == null) return false;

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
