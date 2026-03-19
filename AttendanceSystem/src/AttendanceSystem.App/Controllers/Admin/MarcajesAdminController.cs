using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Security;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    // ─── DTOs compartidos entre MarcajesAdmin y Reportes ─────────────────────────

    public class EmpleadoSelectorDto
    {
        public int    Id     { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Display => string.IsNullOrEmpty(Nombre) ? Codigo : $"{Nombre}  ({Codigo})";
    }

    public class MarcajeAdminFilaDto
    {
        public int    MarcajeId { get; set; }
        public string Fecha     { get; set; }
        public string Hora      { get; set; }
        public string Tipo      { get; set; }
        public string Tardanza  { get; set; }
        public string Minutos   { get; set; }
        public string Metodo    { get; set; }
    }

    // ─── Controller ──────────────────────────────────────────────────────────────

    public class MarcajesAdminController
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IMarcajeRepository  _marcajeRepo;
        private readonly IMarcajeService     _marcajeService;
        private readonly AuditService        _auditService;
        private readonly SessionManager      _session;
        private readonly AppDbContext        _context;

        public MarcajesAdminController(
            IEmpleadoRepository empleadoRepo,
            IMarcajeRepository  marcajeRepo,
            IMarcajeService     marcajeService,
            AuditService        auditService,
            SessionManager      session,
            AppDbContext        context)
        {
            _empleadoRepo   = empleadoRepo;
            _marcajeRepo    = marcajeRepo;
            _marcajeService = marcajeService;
            _auditService   = auditService;
            _session        = session;
            _context        = context;
        }

        // ── Lista de empleados activos con nombre del usuario vinculado ───────────
        public async Task<List<EmpleadoSelectorDto>> ObtenerEmpleadosAsync()
        {
            var empleados  = await _empleadoRepo.GetAllActivosAsync();
            var empList    = empleados.ToList();

            var usuarioIds = empList.Select(e => e.GetUsuarioId()).Distinct().ToList();

            // Filtrado en BD con IN (...) — no carga todos los usuarios en memoria
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(u => usuarioIds.Contains(EF.Property<int>(u, "id")))
                .ToListAsync();

            var nombresMap = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            return empList
                .Select(e => new EmpleadoSelectorDto
                {
                    Id     = e.GetId(),
                    Codigo = e.GetCodigo(),
                    Nombre = nombresMap.TryGetValue(e.GetUsuarioId(), out var n) ? n : e.GetCodigo()
                })
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        // ── Historial de marcajes de un empleado en un mes ───────────────────────
        public async Task<List<MarcajeAdminFilaDto>> CargarMarcajesAsync(
            int empleadoId, DateTime mes)
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

        // ── Registrar un marcaje asistido (manual por admin) ─────────────────────
        public async Task<(bool Ok, string Mensaje)> RegistrarMarcajeManualAsync(
            int empleadoId, DateTime fechaHora)
        {
            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            try
            {
                var marcaje = await _marcajeService.RegistrarMarcajeAsistidoAsync(
                    empleadoId, adminId, fechaHora);

                await _auditService.RegistrarAsync(
                    accion:     "CREAR",
                    entidad:    "Marcaje",
                    registroId: marcaje.GetId(),
                    usuarioId:  adminId,
                    anterior:   null,
                    nuevo:      $"empleadoId={empleadoId}, fechaHora={fechaHora:dd/MM/yyyy HH:mm}",
                    motivo:     "Marcaje asistido por administrador");

                return (true, "Marcaje registrado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar marcaje: {ex.Message}");
            }
        }

        // ── Texto de resumen ─────────────────────────────────────────────────────
        public string GenerarResumen(List<MarcajeAdminFilaDto> filas)
        {
            int entradas  = filas.Count(f => f.Tipo == "Entrada");
            int tardanzas = filas.Count(f => f.Tardanza == "Sí");
            return $"Total: {filas.Count}   |   Entradas: {entradas}   |   Tardanzas: {tardanzas}";
        }
    }
}
