using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    public class BiometricoController
    {
        private readonly IBiometricoService _biometricoService;
        private readonly CameraHelper _cameraHelper;

        public BiometricoController(IBiometricoService biometricoService, CameraHelper cameraHelper)
        {
            _biometricoService = biometricoService;
            _cameraHelper = cameraHelper;
        }

        // 1. Método para encender la cámara y vincularla a la Vista (UI)
        public void IniciarCamara(EventHandler<BitmapImage> onFrameArrivedCallback)
        {
            try
            {
                if (_cameraHelper.HayCamarasDisponibles())
                {
                    // Suscribimos la vista al evento para que reciba el video en vivo
                    _cameraHelper.OnFrameArrived += onFrameArrivedCallback;
                    _cameraHelper.IniciarCamara(0); // Inicia la primera cámara que encuentre
                }
                else
                {
                    throw new Exception("No se detectó ninguna cámara web.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al iniciar la cámara: {ex.Message}");
            }
        }

        // 2. Método para apagar la cámara 
        public void ApagarCamara(EventHandler<BitmapImage> onFrameArrivedCallback)
        {
            _cameraHelper.OnFrameArrived -= onFrameArrivedCallback;
            _cameraHelper.DetenerCamara();
        }

        // 3. El método principal: Toma la foto y le pregunta a Python si es el empleado
        public async Task<(bool Exito, string Mensaje)> VerificarRostroAsync(string codigoEmpleado)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpleado))
            {
                return (false, "El código de empleado no puede estar vacío.");
            }

            try
            {
                // Capturamos el frame exacto de este milisegundo en Base64
                string fotoBase64 = _cameraHelper.CapturarFrameEnBase64();

                if (string.IsNullOrEmpty(fotoBase64))
                {
                    return (false, "No se pudo capturar la imagen de la cámara.");
                }

                bool esValido = await _biometricoService.VerificarIdentidadAsync(fotoBase64, codigoEmpleado);

                if (esValido)
                {
                    return (true, "Identidad verificada correctamente.");
                }
                else
                {
                    return (false, "Rostro no reconocido o no coincide con el código.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error en la verificación biométrica: {ex.Message}");
            }
        }

        // 4. Expone el frame actual como Base64 para que MarcajeController lo use al registrar
        public string CapturarFrameActual()
        {
            return _cameraHelper.CapturarFrameEnBase64();
        }
    }
}