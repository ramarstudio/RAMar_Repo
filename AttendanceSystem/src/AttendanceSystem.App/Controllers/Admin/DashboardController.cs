using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Security;

namespace AttendanceSystem.App.Controllers.Admin
{
    // ─── DTOs ────────────────────────────────────────────────────────────────────

    public class DashboardKpiDto
    {
        public int    TotalEmpleadosActivos { get; set; }
        public int    AsistenciasHoy        { get; set; }
        public int    TardanzasHoy          { get; set; }
        public int    TotalUsuarios         { get; set; }
        public double TasaAsistenciaHoy     { get; set; }
    }

    public class MarcajeResumenDto
    {
        public string EmpleadoId { get; set; }
        public string FechaHora  { get; set; }
        public string Tipo       { get; set; }
        public string EsTardanza { get; set; }
    }

    // ─── Controller ──────────────────────────────────────────────────────────────

    public class DashboardController
    {
        private readonly AppDbContext   _context;
        private readonly SessionManager _session;

        public DashboardController(AppDbContext context, SessionManager session)
        {
            _context = context;
            _session = session;
        }

        // ── KPIs del día ─────────────────────────────────────────────────────────
        // CountAsync para empleados y usuarios: una sola instrucción COUNT SQL.
        // Solo los marcajes de hoy se cargan en memoria (dataset pequeño vs. histórico completo).
        public async Task<DashboardKpiDto> ObtenerKpisAsync()
        {
            var hoy     = DateTime.Today;
            var maniana = hoy.AddDays(1);

            // COUNT directo en BD — no carga entidades
            int totalEmpleados = await _context.Empleados
                .CountAsync(e => EF.Property<bool>(e, "activo") == true);

            int totalUsuarios = await _context.Usuarios.CountAsync();

            // Solo marcajes de hoy — dataset reducido al día actual
            var marcajesHoy = await _context.Marcajes
                .AsNoTracking()
                .Where(m =>
                    EF.Property<DateTime>(m, "fechaHora") >= hoy &&
                    EF.Property<DateTime>(m, "fechaHora") < maniana)
                .ToListAsync();

            int asistencias = marcajesHoy.Count(m => m.EsEntrada());
            int tardanzas   = marcajesHoy.Count(m => m.EsEntrada() && m.EsTardanza());
            double tasa     = totalEmpleados > 0
                ? Math.Round((double)asistencias / totalEmpleados * 100, 1)
                : 0;

            return new DashboardKpiDto
            {
                TotalEmpleadosActivos = totalEmpleados,
                AsistenciasHoy        = asistencias,
                TardanzasHoy          = tardanzas,
                TotalUsuarios         = totalUsuarios,
                TasaAsistenciaHoy     = tasa
            };
        }

        // ── Últimos N marcajes — ORDER BY + LIMIT en BD, no en memoria ───────────
        public async Task<List<MarcajeResumenDto>> ObtenerUltimosMarcajesAsync(int cantidad = 10)
        {
            var marcajes = await _context.Marcajes
                .AsNoTracking()
                .OrderByDescending(m => EF.Property<DateTime>(m, "fechaHora"))
                .Take(cantidad)
                .ToListAsync();

            return marcajes.Select(m => new MarcajeResumenDto
            {
                EmpleadoId = m.GetEmpleadoId().ToString(),
                FechaHora  = m.GetFechaHora().ToString("dd/MM/yyyy HH:mm"),
                Tipo       = m.GetTipo().ToString(),
                EsTardanza = m.EsTardanza() ? "Sí" : "No"
            }).ToList();
        }

        public string ObtenerNombreAdmin()
            => _session.GetCurrentSession()?.Username ?? "Admin";
    }
}
