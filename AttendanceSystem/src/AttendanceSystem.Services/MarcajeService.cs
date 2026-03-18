using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    /// <summary>
    /// Servicio central del sistema. Orquesta el flujo completo de un marcaje:
    /// validación de consentimiento → verificación biométrica → cálculo de tardanza
    /// → persistencia → auditoría.
    ///
    /// REGLA: Este servicio NO accede a la BD directamente.
    /// Solo coordina repositorios y otros servicios.
    /// </summary>
    public class MarcajeService : IMarcajeService
    {
        // ── DEPENDENCIAS ─────────────────────────────────────────────────────
        private readonly IMarcajeRepository _marcajeRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IHorarioRepository _horarioRepository;
        private readonly IConsentimientoRepository _consentimientoRepository;
        private readonly IBiometricoService _biometricoService;
        private readonly AuditService _auditService;

        // ── CONSTRUCTOR ──────────────────────────────────────────────────────
        // El sistema inyecta automáticamente todas las dependencias.
        public MarcajeService(
            IMarcajeRepository marcajeRepository,
            IEmpleadoRepository empleadoRepository,
            IHorarioRepository horarioRepository,
            IConsentimientoRepository consentimientoRepository,
            IBiometricoService biometricoService,
            AuditService auditService)
        {
            _marcajeRepository = marcajeRepository;
            _empleadoRepository = empleadoRepository;
            _horarioRepository = horarioRepository;
            _consentimientoRepository = consentimientoRepository;
            _biometricoService = biometricoService;
            _auditService = auditService;
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 1: RegistrarMarcajeAsync
        // Flujo principal: el empleado se presenta y el sistema verifica su identidad.
        // ════════════════════════════════════════════════════════════════════
        public async Task<MarcajeResponse> RegistrarMarcajeAsync(MarcajeRequest request)
        {
            // ── PASO 1: Buscar al empleado por su código ─────────────────────
            var empleado = await _empleadoRepository.GetByCodigoAsync(request.CodigoEmpleado);
            if (empleado == null)
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = $"No se encontró el empleado con código '{request.CodigoEmpleado}'.",
                    Timestamp = DateTime.UtcNow
                };
            }

            if (!empleado.EstaActivo())
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = "El empleado no está activo en el sistema.",
                    Timestamp = DateTime.UtcNow
                };
            }

            // ── PASO 2: Verificar que el empleado tiene consentimiento biométrico ─
            // Sin consentimiento legal, no podemos procesar datos biométricos.
            var consentimiento = await _consentimientoRepository.GetByEmpleadoIdAsync(empleado.GetId());
            if (consentimiento == null || !consentimiento.EstaAutorizado())
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = "El empleado no tiene consentimiento biométrico vigente.",
                    Timestamp = DateTime.UtcNow
                };
            }

            // ── PASO 3: Verificar identidad con reconocimiento facial ─────────
            // Llama al microservicio Python con la imagen en Base64.
            bool identidadVerificada = await _biometricoService.VerificarIdentidadAsync(
                request.DatosBiometricos,
                request.CodigoEmpleado);

            if (!identidadVerificada)
            {
                return new MarcajeResponse
                {
                    Exito = false,
                    Mensaje = "No se pudo verificar la identidad del empleado. Intente nuevamente.",
                    Timestamp = DateTime.UtcNow
                };
            }

            // ── PASO 4: Calcular si hay tardanza ─────────────────────────────
            // Obtener el día actual como enum DiaSemana compatible con IHorarioRepository.
            var ahora = DateTime.Now;
            var diaSemanaActual = (DiaSemana)ahora.DayOfWeek;

            var horarioHoy = await _horarioRepository.GetHorarioVigenteAsync(
                empleado.GetId(),
                diaSemanaActual);

            bool esTardanza = false;
            int minutosTardanza = 0;

            // Solo calculamos tardanza en marcajes de Entrada y si tiene horario asignado.
            if (request.Tipo == TipoMarcaje.Entrada && horarioHoy != null)
            {
                esTardanza = empleado.LlegoTarde(ahora);
                if (esTardanza)
                {
                    minutosTardanza = empleado.CalcularMinutosTardanza(ahora);
                }
            }

            // ── PASO 5: Crear y guardar el marcaje ───────────────────────────
            var marcaje = new Marcaje();
            marcaje.SetEmpleadoId(empleado.GetId());
            marcaje.SetTipo(request.Tipo.ToString());
            marcaje.SetFechaHora(ahora);
            marcaje.SetTardanza(esTardanza);
            marcaje.SetMinutosTardanza(minutosTardanza);
            marcaje.SetAsistido(false);     // fue el propio empleado con biometría

            await _marcajeRepository.AddAsync(marcaje);

            // ── PASO 6: Registrar en auditoría ───────────────────────────────
            await _auditService.RegistrarAsync(
                accion: "MARCAJE_FACIAL",
                entidad: "Marcaje",
                registroId: marcaje.GetId(),
                usuarioId: empleado.GetId(),
                anterior: null,
                nuevo: marcaje.ObtenerResumen(),
                motivo: "Marcaje automático por reconocimiento facial"
            );

            // ── PASO 7: Retornar respuesta al controlador ────────────────────
            string mensajeResultado = esTardanza
                ? $"Marcaje registrado con {minutosTardanza} minuto(s) de tardanza."
                : "Marcaje registrado correctamente. ¡Buen día!";

            return new MarcajeResponse
            {
                Exito = true,
                Mensaje = mensajeResultado,
                Timestamp = ahora,
                EsTardanza = esTardanza,
                MinutosTardanza = minutosTardanza
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 2: ObtenerHistorialEmpleadoAsync
        // Devuelve todos los marcajes de un empleado en el mes indicado.
        // Usado en la vista de historial y para generar reportes.
        // ════════════════════════════════════════════════════════════════════
        public async Task<IEnumerable<Marcaje>> ObtenerHistorialEmpleadoAsync(int empleadoId, DateTime mes)
        {
            // Calculamos el primer y último día del mes recibido.
            var inicio = new DateTime(mes.Year, mes.Month, 1);
            var fin = inicio.AddMonths(1).AddTicks(-1);     // último instante del mes

            return await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, inicio, fin);
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 3: RegistrarMarcajeAsistidoAsync
        // Un administrador registra manualmente el marcaje de un empleado.
        // Se usa cuando la biometría falla o el empleado olvidó marcar.
        // ════════════════════════════════════════════════════════════════════
        public async Task<Marcaje> RegistrarMarcajeAsistidoAsync(int empleadoId, int adminId, DateTime fechaHora)
        {
            // ── PASO 1: Verificar que el empleado existe ─────────────────────
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new ArgumentException($"No existe un empleado con ID {empleadoId}.");
            }

            // ── PASO 2: Calcular tardanza con la hora que el admin indicó ────
            bool esTardanza = empleado.LlegoTarde(fechaHora);
            int minutosTardanza = esTardanza ? empleado.CalcularMinutosTardanza(fechaHora) : 0;

            // ── PASO 3: Crear el marcaje marcándolo como asistido ─────────────
            // creadoPorId = adminId deja constancia de quién lo registró.
            var marcaje = new Marcaje();
            marcaje.SetEmpleadoId(empleadoId);
            marcaje.SetTipo(TipoMarcaje.Entrada.ToString());
            marcaje.SetFechaHora(fechaHora);
            marcaje.SetTardanza(esTardanza);
            marcaje.SetMinutosTardanza(minutosTardanza);
            marcaje.SetAsistido(true);              // ← indica que fue manual
            marcaje.SetCreadoPorId(adminId);        // ← quién lo registró

            await _marcajeRepository.AddAsync(marcaje);

            // ── PASO 4: Auditoría obligatoria para marcajes manuales ─────────
            // Es importante dejar trazabilidad de que un admin intervino.
            await _auditService.RegistrarAsync(
                accion: "MARCAJE_ASISTIDO",
                entidad: "Marcaje",
                registroId: marcaje.GetId(),
                usuarioId: adminId,
                anterior: null,
                nuevo: marcaje.ObtenerResumen(),
                motivo: $"Marcaje registrado manualmente por admin ID={adminId}"
            );

            return marcaje;
        }
    }
}
