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

        public MarcajeController(
            IMarcajeService       marcajeService,
            IBiometricoController biometricoController,
            ISessionManager       sessionManager)
        {
            _marcajeService       = marcajeService;
            _biometricoController = biometricoController;
            _sessionManager       = sessionManager;
        }

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
