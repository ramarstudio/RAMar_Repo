using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    // Asumimos que la Persona A creará esta interfaz en AttendanceSystem.Security
    public interface IEncryptionService
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] encryptedData);
    }

    public class BiometricoService : IBiometricoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConsentimientoRepository _consentimientoRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEncryptionService _encryptionService;
        
        // URL de tu microservicio Python local
        private readonly string _pythonApiUrl = "http://127.0.0.1:5001/api";

        public BiometricoService(
            HttpClient httpClient, 
            IConsentimientoRepository consentimientoRepository,
            IEmpleadoRepository empleadoRepository,
            IEncryptionService encryptionService)
        {
            _httpClient = httpClient;
            _consentimientoRepository = consentimientoRepository;
            _empleadoRepository = empleadoRepository;
            _encryptionService = encryptionService;
        }

        public async Task<bool> VerificarIdentidadAsync(string base64Image, string codigoEmpleado)
        {
            // 1. Buscar al empleado y su vector facial cifrado
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null || !empleado.TieneEmbedding())
            {
                return false; // No existe o no tiene rostro registrado
            }

            // 2. Descifrar el vector facial guardado en la BD
            var embeddingFacial = empleado.GetEmbeddingFacial(); 
            // CORRECCIÓN: Usar el getter para obtener el vector
            var vectorCifrado = embeddingFacial.GetVectorCifrado(); 
            var vectorDescifradoBytes = _encryptionService.Decrypt(vectorCifrado);
            
            // Convertimos los bytes descifrados de vuelta a un arreglo de floats para Python
            var vectorConocido = JsonSerializer.Deserialize<float[]>(Encoding.UTF8.GetString(vectorDescifradoBytes));

            // 3. Enviar la imagen actual y el vector conocido a Python para que los compare
            var requestPayload = new 
            { 
                image_base64 = base64Image,
                known_embedding = vectorConocido 
            };

            var response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/verify", requestPayload);
            
            if (!response.IsSuccessStatusCode) return false;

            var matchResult = await response.Content.ReadFromJsonAsync<PythonMatchResponse>();

            // 4. Validar si Python dice que es la misma persona y si supera el umbral de confianza del empleado
            // CORRECCIÓN: Usar el getter para el umbral
            decimal umbralRequerido = embeddingFacial.GetUmbral(); 
            
            return matchResult != null && 
                   matchResult.Match && 
                   matchResult.Confidence >= umbralRequerido;
        }

        public async Task<bool> RegistrarNuevoRostroAsync(string base64Image, string codigoEmpleado)
        {
            // 1. Validar existencia del empleado
            var empleado = await _empleadoRepository.GetByCodigoAsync(codigoEmpleado);
            if (empleado == null) throw new ArgumentException("Empleado no encontrado.");

            // 2. REGLA DE NEGOCIO CRÍTICA (RN02): Verificar Consentimiento Legal
            var consentimiento = await _consentimientoRepository.GetByEmpleadoIdAsync(empleado.GetId());
            if (consentimiento == null || !consentimiento.EstaAutorizado())
            {
                throw new InvalidOperationException("No se puede registrar biometría sin un consentimiento legal firmado y autorizado.");
            }

            // 3. Pedirle a Python que analice la foto y extraiga el vector matemático (Embedding)
            var requestPayload = new { image_base64 = base64Image };
            var response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/encode", requestPayload);
            
            if (!response.IsSuccessStatusCode) return false; // Puede fallar si no hay rostro en la foto

            var encodeResult = await response.Content.ReadFromJsonAsync<PythonEncodeResponse>();
            if (encodeResult == null || encodeResult.Embedding == null) return false;

            // 4. Convertir el arreglo de floats a bytes y cifrarlo (Seguridad)
            var vectorBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(encodeResult.Embedding));
            var vectorCifrado = _encryptionService.Encrypt(vectorBytes);

            // 5. Crear la nueva entidad EmbeddingFacial y asignarla al empleado
            var nuevoEmbedding = new EmbeddingFacial();
            
            // CORRECCIÓN: Como tienes clases estilo Java, usamos los setters en lugar de la inicialización {}
            nuevoEmbedding.SetEmpleadoId(empleado.GetId());
            nuevoEmbedding.SetVectorCifrado(vectorCifrado);
            nuevoEmbedding.SetAlgoritmo("AES-256-GCM");
            nuevoEmbedding.SetUmbral(0.60m); // 60% de coincidencia mínima por defecto
            nuevoEmbedding.SetVersionModelo("v1.0");
            nuevoEmbedding.SetCreadoEn(DateTime.UtcNow);

            empleado.SetEmbeddingFacial(nuevoEmbedding);
            
            // 6. Guardar en BD
            await _empleadoRepository.UpdateAsync(empleado);

            return true;
        }

        // --- Clases Auxiliares (DTOs) para leer las respuestas de Python ---
        private class PythonEncodeResponse
        {
            public float[] Embedding { get; set; }
        }

        private class PythonMatchResponse
        {
            public bool Match { get; set; }
            public decimal Confidence { get; set; }
        }
    }
}