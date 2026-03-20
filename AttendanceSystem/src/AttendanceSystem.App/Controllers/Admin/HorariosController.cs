using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class HorariosController
    {
        private readonly IHorarioRepository          _horarioRepo;
        private readonly IEmpleadoRepository         _empleadoRepo;
        private readonly IEmpleadoSelectorService    _selectorService;
        private readonly AuditService                _auditService;
        private readonly ISessionManager             _session;

        public HorariosController(
            IHorarioRepository       horarioRepo,
            IEmpleadoRepository      empleadoRepo,
            IEmpleadoSelectorService selectorService,
            AuditService             auditService,
            ISessionManager          session)
        {
            _horarioRepo     = horarioRepo;
            _empleadoRepo    = empleadoRepo;
            _selectorService = selectorService;
            _auditService    = auditService;
            _session         = session;
        }

        public Task<List<EmpleadoSelectorDto>> ObtenerSelectorAsync()
            => _selectorService.ObtenerSelectorAsync();

        public async Task<List<HorarioFilaDto>> CargarHorariosAsync(int empleadoId, CancellationToken ct = default)
        {
            var horarios = await _horarioRepo.GetByEmpleadoIdAsync(empleadoId);
            var emp      = await _empleadoRepo.GetByIdAsync(empleadoId);
            string nombre = emp?.GetCodigo() ?? $"#{empleadoId}";

            return horarios.Select(h => new HorarioFilaDto
            {
                Id           = h.GetId(),
                EmpleadoId   = empleadoId,
                Empleado     = nombre,
                Dia          = h.GetDia().ToString(),
                Entrada      = h.GetEntrada().ToString("HH:mm"),
                Salida       = h.GetSalida().ToString("HH:mm"),
                VigenteDesde = h.GetVigenteDesde().ToString("dd/MM/yyyy"),
                VigenteHasta = h.GetVigenteHasta().ToString("dd/MM/yyyy")
            }).ToList();
        }

        public async Task<(bool Ok, string Mensaje)> CrearHorarioAsync(
            int empleadoId, DiaSemana dia,
            TimeSpan entrada, TimeSpan salida,
            DateTime vigDesde, DateTime vigHasta)
        {
            if (entrada >= salida)
                return (false, "La hora de entrada debe ser antes que la de salida.");

            if (vigDesde >= vigHasta)
                return (false, "La fecha 'desde' debe ser anterior a 'hasta'.");

            var baseDate = new DateTime(2000, 1, 1);
            var horario = new Horario();
            horario.SetEmpleadoId(empleadoId);
            horario.SetDia(dia);
            horario.SetEntrada(baseDate.Add(entrada));
            horario.SetSalida(baseDate.Add(salida));
            horario.SetVigenteDesde(vigDesde);
            horario.SetVigenteHasta(vigHasta);

            await _horarioRepo.AddAsync(horario);

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "CREAR", entidad: "Horario", registroId: 0,
                usuarioId: adminId, anterior: null,
                nuevo: $"Emp={empleadoId}, Día={dia}, {entrada:hh\\:mm}-{salida:hh\\:mm}",
                motivo: "Asignación de horario");

            return (true, "Horario creado correctamente.");
        }

        public async Task<(bool Ok, string Mensaje)> EliminarHorarioAsync(int horarioId)
        {
            // Para eliminar necesitamos acceso directo al context
            // Usamos el repo base
            var horarios = await _horarioRepo.GetByEmpleadoIdAsync(0); // workaround
            return (false, "No implementado aún.");
        }
    }
}
