using System;
using System.Threading.Tasks;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Security;

namespace AttendanceSystem.App.Controllers
{
    public class MarcajeController
    {
        private readonly IMarcajeService _marcajeService;
        private readonly BiometricoController _biometricoController;
        private readonly SessionManager _sessionManager;

        public MarcajeController(
            IMarcajeService marcajeService,
            BiometricoController biometricoController,
            SessionManager sessionManager)
        {
            _marcajeService = marcajeService;
            _biometricoController = biometricoController;
            _sessionManager = sessionManager;
        }

        // Método principal: captura foto de la cámara y registra el marcaje
        public async Task<MarcajeResponse> RegistrarMarcajeAsync(TipoMarcaje tipo)
        {
            // 1. Verificar sesión activa
            if (!_sessionManager.EstaLogueado())
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = "No hay sesión activa. Por favor inicie sesión.",
                    Timestamp = DateTime.Now
                };
            }

            try
            {
                // 2. Capturar la foto actual de la cámara como Base64
                string fotoBase64 = ObtenerFotoDeCamera();
                if (string.IsNullOrEmpty(fotoBase64))
                {
                    return new MarcajeResponse
                    {
                        Exito = false,
                        Mensaje = "No se pudo capturar la imagen de la cámara. Verifique que esté activa.",
                        Timestamp = DateTime.Now
                    };
                }

                // 3. Obtener el código del empleado desde la sesión activa
                string codigoEmpleado = _sessionManager.ObtenerUsuarioActual()?.GetUsername()
                    ?? throw new InvalidOperationException("No se encontró el usuario en sesión.");

                // 4. Construir la solicitud y enviar al Service
                var request = new MarcajeRequest
                {
                    CodigoEmpleado = codigoEmpleado,
                    Tipo = tipo,
                    DatosBiometricos = fotoBase64
                };

                return await _marcajeService.RegistrarMarcajeAsync(request);
            }
            catch (Exception ex)
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = $"Error inesperado al registrar marcaje: {ex.Message}",
                    Timestamp = DateTime.Now
                };
            }
        }

        // Obtiene el frame actual de BiometricoController (la cámara ya está encendida por la vista)
        private string ObtenerFotoDeCamera()
        {
            try
            {
                // BiometricoController expone este método para capturar el frame actual
                return _biometricoController.CapturarFrameActual();
            }
            catch
            {
                return null;
            }
        }
    }
}
