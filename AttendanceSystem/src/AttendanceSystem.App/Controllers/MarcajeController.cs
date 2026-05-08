using System;
using System.Threading.Tasks;
using AttendanceSystem.App.Interfaces;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    public class MarcajeController
    {
        private readonly IMarcajeService       _marcajeService;
        private readonly IBiometricoController _biometricoController;
        private readonly ISessionManager       _sessionManager;
        private readonly IFaceServiceLifecycle _faceServiceLifecycle;

        public MarcajeController(
            IMarcajeService       marcajeService,
            IBiometricoController biometricoController,
            ISessionManager       sessionManager,
            IFaceServiceLifecycle faceServiceLifecycle = null)
        {
            _marcajeService       = marcajeService;
            _biometricoController = biometricoController;
            _sessionManager       = sessionManager;
            _faceServiceLifecycle = faceServiceLifecycle;
        }

        /// <summary>
        /// Pre-calienta el FaceService en background mientras el usuario se posiciona
        /// frente a la cámara. Así cuando presiona el botón, el modelo ya está cargado.
        /// </summary>
        public async Task PrecalentarAsync()
        {
            if (_faceServiceLifecycle == null) return;
            try
            {
                await _faceServiceLifecycle.EnsureRunningAsync();
            }
            catch { /* best-effort, el error se manejará al hacer el marcaje */ }
        }

        /// <summary>
        /// Resetea el timer de inactividad. Llamar periódicamente mientras la vista
        /// de marcaje está activa para evitar que el idle-timeout apague el servicio.
        /// </summary>
        public void MantenervVivo() => _faceServiceLifecycle?.Touch();

        /// <summary>True si el FaceService ya está corriendo y el modelo está listo.</summary>
        public bool FaceServiceActivo => _faceServiceLifecycle?.IsRunning ?? false;

        public async Task<MarcajeResponse> RegistrarMarcajeAsync(TipoMarcaje tipo)
        {
            if (!_sessionManager.EstaLogueado())
                return Fallo("No hay sesión activa. Por favor inicie sesión.");

            try
            {
                string fotoBase64 = _biometricoController.CapturarFrameActual();
                if (string.IsNullOrEmpty(fotoBase64))
                    return Fallo("No se pudo capturar la imagen de la cámara. Verifique que esté activa.");

                string codigoEmpleado = _sessionManager.ObtenerUsuarioActual()?.GetUsername()
                    ?? throw new InvalidOperationException("No se encontró el usuario en sesión.");

                var request = new MarcajeRequest
                {
                    CodigoEmpleado   = codigoEmpleado,
                    Tipo             = tipo,
                    DatosBiometricos = fotoBase64
                };

                return await _marcajeService.RegistrarMarcajeAsync(request);
            }
            catch (Exception ex)
            {
                return Fallo($"Error inesperado: {ex.Message}");
            }
        }

        private static MarcajeResponse Fallo(string mensaje) =>
            new MarcajeResponse { Exito = false, Mensaje = mensaje, Timestamp = DateTime.Now };
    }
}
