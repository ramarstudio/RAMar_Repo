using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class AuditoriaController
    {
        private readonly AppDbContext        _context;
        private readonly IUsuarioRepository  _usuarioRepo;

        public AuditoriaController(AppDbContext context, IUsuarioRepository usuarioRepo)
        {
            _context     = context;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<AuditLogFilaDto>> CargarLogsAsync(
            DateTime? desde = null, DateTime? hasta = null,
            string entidadFiltro = null, CancellationToken ct = default)
        {
            var desdeReal = desde ?? DateTime.Today.AddDays(-30);
            var hastaReal = (hasta ?? DateTime.Today).AddDays(1);

            // Query base con filtro de fecha en SQL
            IQueryable<AuditLog> query = _context.AuditLogs
                .AsNoTracking()
                .Where(a => EF.Property<DateTime>(a, "fecha") >= desdeReal
                         && EF.Property<DateTime>(a, "fecha") < hastaReal);

            // Filtro de entidad en SQL — no en memoria
            if (!string.IsNullOrWhiteSpace(entidadFiltro))
                query = query.Where(a => EF.Property<string>(a, "entidad") == entidadFiltro.Trim());

            var logs = await query
                .OrderByDescending(a => EF.Property<DateTime>(a, "fecha"))
                .ToListAsync(ct);

            // Resolver nombres de usuario en bulk
            var userIds  = logs.Select(a => a.GetUsuarioId()).Distinct().ToList();
            var usuarios = await _usuarioRepo.GetByIdsAsync(userIds, ct);
            var userMap  = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            return logs.Select(a => new AuditLogFilaDto
            {
                Id              = a.GetId(),
                Accion          = a.GetAccion(),
                Entidad         = a.GetEntidad(),
                RegistroId      = a.GetRegistroId(),
                DatosAnteriores = a.GetDatosAnteriores() ?? "—",
                DatosNuevos     = a.GetDatosNuevos() ?? "—",
                Motivo          = a.GetMotivo() ?? "—",
                Fecha           = a.GetFecha().ToString("dd/MM/yyyy HH:mm"),
                Usuario         = userMap.TryGetValue(a.GetUsuarioId(), out var n) ? n : $"#{a.GetUsuarioId()}"
            }).ToList();
        }

        /// <summary>
        /// SQL DISTINCT — solo trae los nombres de entidad únicos, no carga toda la tabla.
        /// </summary>
        public async Task<List<string>> ObtenerEntidadesAsync(CancellationToken ct = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Select(a => EF.Property<string>(a, "entidad"))
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync(ct);
        }
    }
}
