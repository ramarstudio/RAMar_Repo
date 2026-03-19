using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    public class BiometricoService : IBiometricoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConsentimientoRepository _consentimientoRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEncryptionService _encryptionService;

        private readonly string _pythonApiUrl = "http://127.0.0.1:5001/api";

        public BiometricoService(
            HttpClient httpClient,
            IConsentimientoRepository consentimientoRepository,
            IEmpleadoRepository empleadoRepository,
            IEncryptionService encryptionService)
        {
            _httpClient                 = httpClient;
            _consentimientoRepository   = consentimientoRepository;
            _empleadoRepository         = empleadoRepository;
            _encryptionService          = encryptionService;
        }

        public async Task<bool> VerificarIdentidadAsync(string base64Image, string codigoEmpleado)
        {
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null || !empleado.TieneEmbedding()) return false;

            var embeddingFacial = empleado.GetEmbeddingFacial();

            // Descifrar el vector — try/catch previene crash si la clave AES no coincide
            // o si los datos en BD están corruptos.
            float[] vectorConocido;
            try
            {
                var encryptedStr = Encoding.UTF8.GetString(embeddingFacial.GetVectorCifrado());
                var vectorJson   = _encryptionService.Decrypt(encryptedStr);
                vectorConocido   = JsonSerializer.Deserialize<float[]>(vectorJson);
                if (vectorConocido == null) return false;
            }
            catch
            {
                // Clave incorrecta o datos corruptos: no se puede verificar identidad
                return false;
            }

            var requestPayload = new
            {
                image_base64    = base64Image,
                known_embedding = vectorConocido
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/verify", requestPayload);
            }
            catch
            {
                // Microservicio Python no disponible
                return false;
            }

            if (!response.IsSuccessStatusCode) return false;

            PythonMatchResponse matchResult;
            try
            {
                matchResult = await response.Content.ReadFromJsonAsync<PythonMatchResponse>();
            }
            catch
            {
                return false;
            }

            if (matchResult == null) return false;

            decimal umbralRequerido = embeddingFacial.GetUmbral();
            return matchResult.Match && matchResult.Confidence >= umbralRequerido;
        }

        public async Task<bool> RegistrarNuevoRostroAsync(string base64Image, string codigoEmpleado)
        {
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null)
                throw new ArgumentException("Empleado no encontrado.");

            // RN02: consentimiento legal obligatorio
            var consentimiento = await _consentimientoRepository.GetByEmpleadoIdAsync(empleado.GetId());
            if (consentimiento == null || !consentimiento.EstaAutorizado())
                throw new InvalidOperationException(
                    "No se puede registrar biometría sin un consentimiento legal firmado y autorizado.");

            var requestPayload = new { image_base64 = base64Image };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/encode", requestPayload);
            }
            catch
            {
                return false;
            }

            if (!response.IsSuccessStatusCode) return false;

            PythonEncodeResponse encodeResult;
            try
            {
                encodeResult = await response.Content.ReadFromJsonAsync<PythonEncodeResponse>();
            }
            catch
            {
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
