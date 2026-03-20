using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.App.Interfaces;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    public class BiometricoController : IBiometricoController
    {
        private readonly IBiometricoService _biometricoService;
        private readonly CameraHelper       _cameraHelper;

        public BiometricoController(IBiometricoService biometricoService, CameraHelper cameraHelper)
        {
            _biometricoService = biometricoService;
            _cameraHelper      = cameraHelper;
        }

        public void IniciarCamara(EventHandler<BitmapSource> onFrameArrivedCallback)
        {
            if (!_cameraHelper.HayCamarasDisponibles())
                throw new InvalidOperationException("No se detectó ninguna cámara web.");

            _cameraHelper.OnFrameArrived += onFrameArrivedCallback;
            _cameraHelper.IniciarCamara(0);
        }

        public void ApagarCamara(EventHandler<BitmapSource> onFrameArrivedCallback)
        {
            _cameraHelper.OnFrameArrived -= onFrameArrivedCallback;
            _cameraHelper.DetenerCamara();
        }

        public async Task<(bool Exito, string Mensaje)> VerificarRostroAsync(string codigoEmpleado)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpleado))
                return (false, "El código de empleado no puede estar vacío.");

            try
            {
                string fotoBase64 = _cameraHelper.CapturarFrameEnBase64();
                if (string.IsNullOrEmpty(fotoBase64))
                    return (false, "No se pudo capturar la imagen de la cámara.");

                bool esValido = await _biometricoService.VerificarIdentidadAsync(fotoBase64, codigoEmpleado);
                return esValido
                    ? (true,  "Identidad verificada correctamente.")
                    : (false, "Rostro no reconocido o no coincide con el código.");
            }
            catch (Exception ex)
            {
                return (false, $"Error en la verificación biométrica: {ex.Message}");
            }
        }

        public string CapturarFrameActual() => _cameraHelper.CapturarFrameEnBase64();
    }
}
