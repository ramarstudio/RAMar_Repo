using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class DashboardController
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IMarcajeRepository  _marcajeRepo;
        private readonly IUsuarioRepository  _usuarioRepo;
        private readonly ISessionManager     _session;

        public DashboardController(
            IEmpleadoRepository empleadoRepo,
            IMarcajeRepository  marcajeRepo,
            IUsuarioRepository  usuarioRepo,
            ISessionManager     session)
        {
            _empleadoRepo = empleadoRepo;
            _marcajeRepo  = marcajeRepo;
            _usuarioRepo  = usuarioRepo;
            _session      = session;
        }

        // ── KPIs del día ──────────────────────────────────────────────────────────
        public async Task<DashboardKpiDto> ObtenerKpisAsync(CancellationToken ct = default)
        {
            var hoy     = DateTime.Today;
            var maniana = hoy.AddDays(1);

            var empleados      = await _empleadoRepo.GetAllActivosAsync();
            int totalEmpleados = empleados.Count();

            var marcajesHoy = await _marcajeRepo.GetByFechaAsync(hoy, maniana, ct);
            int asistencias = marcajesHoy.Count(m => m.EsEntrada());
            int tardanzas   = marcajesHoy.Count(m => m.EsEntrada() && m.EsTardanza());
            int ausencias   = Math.Max(0, totalEmpleados - asistencias);

            double tasa = totalEmpleados > 0
                ? Math.Round((double)asistencias / totalEmpleados * 100, 1) : 0;

            int marcajes7Dias = await _marcajeRepo.CountByFechaRangoAsync(
                hoy.AddDays(-6), maniana, ct);

            return new DashboardKpiDto
            {
                TotalEmpleadosActivos = totalEmpleados,
                AsistenciasHoy        = asistencias,
                TardanzasHoy          = tardanzas,
                AusenciasHoy          = ausencias,
                TotalUsuarios         = totalEmpleados,
                TasaAsistenciaHoy     = tasa,
                MarcajesUltimos7Dias  = marcajes7Dias
            };
        }

        // ── Gráfico líneas — asistencias diarias N días ───────────────────────────
        public Task<List<AsistenciaDiariaDto>> ObtenerAsistenciasDiariasAsync(
            int dias = 14, CancellationToken ct = default)
            => _marcajeRepo.GetAsistenciasDiariasAsync(dias, ct);

        // ── Gráfico barras — top N empleados con más tardanzas ────────────────────
        public async Task<List<TardanzaEmpleadoDto>> ObtenerTopTardanzasAsync(
            int topN = 5, int dias = 30, CancellationToken ct = default)
        {
            var items = await _marcajeRepo.GetTopTardanzasAsync(topN, dias, ct);
            if (!items.Any()) return items;

            // Resolver nombres: 2 queries bulk — sin N+1
            var empIds    = items.Select(x => x.EmpleadoId).ToList();
            var empleados = await _empleadoRepo.GetByIdsAsync(empIds, ct);

            var usrIds   = empleados.Select(e => e.GetUsuarioId()).Distinct().ToList();
            var usuarios = await _usuarioRepo.GetByIdsAsync(usrIds, ct);
            var usrMap   = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            foreach (var item in items)
            {
                var emp = empleados.FirstOrDefault(e => e.GetId() == item.EmpleadoId);
                item.NombreEmpleado = emp != null && usrMap.TryGetValue(emp.GetUsuarioId(), out var n)
                    ? n
                    : $"#{item.EmpleadoId}";
            }
            return items;
        }

        // ── Tabla actividad reciente con nombre resuelto ──────────────────────────
        public async Task<List<MarcajeResumenDto>> ObtenerUltimosMarcajesAsync(
            int cantidad = 10, CancellationToken ct = default)
        {
            var marcajes = await _marcajeRepo.GetUltimosAsync(cantidad, ct);
            if (!marcajes.Any()) return new List<MarcajeResumenDto>();

            // Resolver códigos de empleado en una sola query
            var empIds    = marcajes.Select(m => m.GetEmpleadoId()).Distinct().ToList();
            var empleados = await _empleadoRepo.GetByIdsAsync(empIds, ct);
            var empMap    = empleados.ToDictionary(e => e.GetId(), e => e.GetCodigo());

            return marcajes.Select(m => new MarcajeResumenDto
            {
                NombreEmpleado = empMap.TryGetValue(m.GetEmpleadoId(), out var cod)
                    ? cod : $"#{m.GetEmpleadoId()}",
                FechaHora  = m.GetFechaHora().ToString("dd/MM/yyyy HH:mm"),
                Tipo       = m.GetTipo().ToString(),
                EsTardanza = m.EsTardanza() ? "Sí" : "No"
            }).ToList();
        }

        public string ObtenerNombreAdmin()
            => _session.GetCurrentSession()?.Username ?? "Admin";
    }
}
