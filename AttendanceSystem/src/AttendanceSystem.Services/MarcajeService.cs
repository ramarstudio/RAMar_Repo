using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.Services
{
    public class MarcajeService : IMarcajeService
    {
        private readonly IMarcajeRepository      _marcajeRepository;
        private readonly IEmpleadoRepository     _empleadoRepository;
        private readonly IConsentimientoRepository _consentimientoRepository;
        private readonly IBiometricoService      _biometricoService;
        private readonly ITardanzaService        _tardanzaService;
        private readonly AuditService            _auditService;
        private readonly ILogger<MarcajeService> _logger;

        public MarcajeService(
            IMarcajeRepository        marcajeRepository,
            IEmpleadoRepository       empleadoRepository,
            IConsentimientoRepository consentimientoRepository,
            IBiometricoService        biometricoService,
            ITardanzaService          tardanzaService,
            AuditService              auditService,
            ILogger<MarcajeService>   logger)
        {
            _marcajeRepository        = marcajeRepository;
            _empleadoRepository       = empleadoRepository;
            _consentimientoRepository = consentimientoRepository;
            _biometricoService        = biometricoService;
            _tardanzaService          = tardanzaService;
            _auditService             = auditService;
            _logger                   = logger;
        }

        public async Task<MarcajeResponse> RegistrarMarcajeAsync(MarcajeRequest request)
        {
            // 1. Buscar empleado
            var empleado = await _empleadoRepository.GetByCodigoAsync(request.CodigoEmpleado);
            if (empleado == null)
                return Fallo($"No se encontró el empleado '{request.CodigoEmpleado}'.");
            if (!empleado.EstaActivo())
                return Fallo("El empleado no está activo en el sistema.");

            // 2. Verificar consentimiento biométrico
            var consentimiento = await _consentimientoRepository.GetByEmpleadoIdAsync(empleado.GetId());
            if (consentimiento == null || !consentimiento.EstaAutorizado())
                return Fallo("El empleado no tiene consentimiento biométrico vigente.");

            // 3. Verificar identidad facial
            bool identidadVerificada;
            try
            {
                identidadVerificada = await _biometricoService.VerificarIdentidadAsync(
                    request.DatosBiometricos, request.CodigoEmpleado);
            }
            catch (InvalidOperationException ex)
            {
                return Fallo(ex.Message);
            }
            if (!identidadVerificada)
                return Fallo("Rostro no reconocido. La similitud es insuficiente. Intente nuevamente con buena iluminación.");

            // 4. Calcular tardanza usando ITardanzaService (lógica centralizada)
            var ahora = DateTime.Now;
            bool esTardanza   = false;
            int  minutosTarde = 0;

            if (request.Tipo == TipoMarcaje.Entrada)
            {
                var (tardanza, minutos, _) = await _tardanzaService.EvaluarTardanzaAsync(empleado, ahora);
                esTardanza   = tardanza;
                minutosTarde = minutos;
            }

            // 5. Persistir marcaje
            var marcaje = new Marcaje();
            marcaje.SetEmpleadoId(empleado.GetId());
            marcaje.SetTipo(request.Tipo);
            marcaje.SetFechaHora(ahora);
            marcaje.SetTardanza(esTardanza);
            marcaje.SetMinutosTardanza(minutosTarde);
            marcaje.SetAsistido(false);

            await _marcajeRepository.AddAsync(marcaje);
            _logger.LogInformation("Marcaje {Tipo} registrado — empleado {Codigo}.", request.Tipo, request.CodigoEmpleado);

            // 6. Auditoría
            await _auditService.RegistrarAsync(
                accion: "MARCAJE_FACIAL", entidad: "Marcaje",
                registroId: marcaje.GetId(), usuarioId: empleado.GetId(),
                anterior: null,
                nuevo: $"{request.Tipo} – {ahora} – {request.CodigoEmpleado}",
                motivo: "Marcaje automático por reconocimiento facial");

            string mensaje = esTardanza
                ? $"Marcaje registrado con {minutosTarde} minuto(s) de tardanza."
                : "Marcaje registrado correctamente. ¡Buen día!";

            return new MarcajeResponse { Exito = true, Mensaje = mensaje, Timestamp = ahora, EsTardanza = esTardanza, MinutosTardanza = minutosTarde };
        }

        public async Task<IEnumerable<Marcaje>> ObtenerHistorialEmpleadoAsync(int empleadoId, DateTime mes)
        {
            var inicio = new DateTime(mes.Year, mes.Month, 1);
            var fin    = inicio.AddMonths(1).AddTicks(-1);
            return await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, inicio, fin);
        }

        public async Task<Marcaje> RegistrarMarcajeAsistidoAsync(int empleadoId, int adminId, DateTime fechaHora)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new ArgumentException($"No existe empleado con ID {empleadoId}.");

            var (esTardanza, minutosTarde, _) = await _tardanzaService.EvaluarTardanzaAsync(empleado, fechaHora);

            var marcaje = new Marcaje();
            marcaje.SetEmpleadoId(empleadoId);
            marcaje.SetTipo(TipoMarcaje.Entrada);
            marcaje.SetFechaHora(fechaHora);
            marcaje.SetTardanza(esTardanza);
            marcaje.SetMinutosTardanza(minutosTarde);
            marcaje.SetAsistido(true);
            marcaje.SetCreadoPorId(adminId);

            await _marcajeRepository.AddAsync(marcaje);

            await _auditService.RegistrarAsync(
                accion: "MARCAJE_ASISTIDO", entidad: "Marcaje",
                registroId: marcaje.GetId(), usuarioId: adminId,
                anterior: null,
                nuevo: $"Entrada manual – {fechaHora} – empleadoId={empleadoId}",
                motivo: $"Marcaje registrado manualmente por admin ID={adminId}");

            return marcaje;
        }

        private static MarcajeResponse Fallo(string mensaje) =>
            new MarcajeResponse { Exito = false, Mensaje = mensaje, Timestamp = DateTime.UtcNow };
    }
}
