using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class MarcajesAdminController
    {
        private readonly IEmpleadoSelectorService _selectorService;
        private readonly IMarcajeRepository       _marcajeRepo;
        private readonly IMarcajeService          _marcajeService;
        private readonly AuditService             _auditService;
        private readonly ISessionManager          _session;

        public MarcajesAdminController(
            IEmpleadoSelectorService selectorService,
            IMarcajeRepository       marcajeRepo,
            IMarcajeService          marcajeService,
            AuditService             auditService,
            ISessionManager          session)
        {
            _selectorService = selectorService;
            _marcajeRepo     = marcajeRepo;
            _marcajeService  = marcajeService;
            _auditService    = auditService;
            _session         = session;
        }

        // Delegado al servicio compartido — elimina duplicación con ReportesController
        public Task<List<EmpleadoSelectorDto>> ObtenerEmpleadosAsync()
            => _selectorService.ObtenerSelectorAsync();

        public async Task<List<MarcajeAdminFilaDto>> CargarMarcajesAsync(int empleadoId, DateTime mes)
        {
            var inicio   = new DateTime(mes.Year, mes.Month, 1);
            var fin      = inicio.AddMonths(1).AddTicks(-1);
            var marcajes = await _marcajeRepo.GetByEmpleadoIdAsync(empleadoId, inicio, fin);

            return marcajes.Select(m => new MarcajeAdminFilaDto
            {
                MarcajeId = m.GetId(),
                Fecha     = m.GetFechaHora().ToString("dd/MM/yyyy"),
                Hora      = m.GetFechaHora().ToString("HH:mm"),
                Tipo      = m.GetTipo().ToString(),
                Tardanza  = m.EsTardanza() ? "Sí" : "No",
                Minutos   = m.EsTardanza() ? m.GetMinutosTardanza().ToString() : "—",
                Metodo    = m.FueAsistido() ? "Manual" : "Facial"
            }).ToList();
        }

        public async Task<(bool Ok, string Mensaje)> RegistrarMarcajeManualAsync(
            int empleadoId, DateTime fechaHora)
        {
            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            try
            {
                var marcaje = await _marcajeService.RegistrarMarcajeAsistidoAsync(
                    empleadoId, adminId, fechaHora);

                await _auditService.RegistrarAsync(
                    accion: "CREAR", entidad: "Marcaje", registroId: marcaje.GetId(),
                    usuarioId: adminId, anterior: null,
                    nuevo: $"empleadoId={empleadoId}, fechaHora={fechaHora:dd/MM/yyyy HH:mm}",
                    motivo: "Marcaje asistido por administrador");

                return (true, "Marcaje registrado correctamente.");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message
                         ?? ex.InnerException?.Message
                         ?? ex.Message;
                return (false, $"Error: {inner}");
            }
        }

        public string GenerarResumen(List<MarcajeAdminFilaDto> filas)
        {
            int entradas  = filas.Count(f => f.Tipo == "Entrada");
            int tardanzas = filas.Count(f => f.Tardanza == "Sí");
            return $"Total: {filas.Count}   |   Entradas: {entradas}   |   Tardanzas: {tardanzas}";
        }
    }
}
