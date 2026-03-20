using System;
using System.Collections.Generic;
using System.Globalization;
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

            // SQL COUNT — no carga entidades
            int totalEmpleados = await _empleadoRepo.CountActivosAsync(ct);

            var marcajesHoy = await _marcajeRepo.GetByFechaAsync(hoy, maniana, ct);

            // Single-pass: recorre la colección una sola vez
            var (asistencias, tardanzas) = ContarEntradasYTardanzas(marcajesHoy);
            int ausencias = Math.Max(0, totalEmpleados - asistencias);

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

        // ── KPIs con tendencia (hoy vs ayer) ────────────────────────────────────
        public async Task<Dictionary<string, KpiTendenciaDto>> ObtenerTendenciasAsync(CancellationToken ct = default)
        {
            var hoy     = DateTime.Today;
            var ayer    = hoy.AddDays(-1);
            var maniana = hoy.AddDays(1);

            int totalEmpleados = await _empleadoRepo.CountActivosAsync(ct);

            var marcajesHoy  = await _marcajeRepo.GetByFechaAsync(hoy, maniana, ct);
            var marcajesAyer = await _marcajeRepo.GetByFechaAsync(ayer, hoy, ct);

            var (asistHoy, tardHoy) = ContarEntradasYTardanzas(marcajesHoy);
            var (asistAyer, tardAyer) = ContarEntradasYTardanzas(marcajesAyer);

            int ausHoy  = Math.Max(0, totalEmpleados - asistHoy);
            int ausAyer = Math.Max(0, totalEmpleados - asistAyer);

            return new Dictionary<string, KpiTendenciaDto>
            {
                ["asistencias"] = new KpiTendenciaDto { ValorActual = asistHoy,  ValorAnterior = asistAyer },
                ["tardanzas"]   = new KpiTendenciaDto { ValorActual = tardHoy,   ValorAnterior = tardAyer },
                ["ausencias"]   = new KpiTendenciaDto { ValorActual = ausHoy,    ValorAnterior = ausAyer }
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
            if (items.Count == 0) return items;
            await ResolverNombresEmpleados(items, ct);
            return items;
        }

        // ── Top N empleados más puntuales — SQL GROUP BY server-side ──────────────
        public async Task<List<TardanzaEmpleadoDto>> ObtenerTopPuntualesAsync(
            int topN = 5, int dias = 30, CancellationToken ct = default)
        {
            var items = await _marcajeRepo.GetTopPuntualesAsync(topN, dias, ct);
            if (items.Count == 0) return items;
            await ResolverNombresEmpleados(items, ct);
            return items;
        }

        // ── Resumen semanal (Lun-Vie de la semana actual) ───────────────────────
        public async Task<List<ResumenSemanalDto>> ObtenerResumenSemanalAsync(CancellationToken ct = default)
        {
            var hoy = DateTime.Today;

            int diff  = (7 + (int)hoy.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var lunes = hoy.AddDays(-diff);

            int totalEmpleados = await _empleadoRepo.CountActivosAsync(ct);
            var marcajes       = await _marcajeRepo.GetByFechaAsync(lunes, hoy.AddDays(1), ct);

            // Agrupar por fecha en un solo pase con Dictionary
            var porDia = new Dictionary<DateTime, (int Asist, int Tard)>();
            foreach (var m in marcajes)
            {
                var fecha = m.GetFechaHora().Date;
                if (!m.EsEntrada()) continue;

                porDia.TryGetValue(fecha, out var acc);
                acc.Asist++;
                if (m.EsTardanza()) acc.Tard++;
                porDia[fecha] = acc;
            }

            string[] diasCortos = { "Lun", "Mar", "Mié", "Jue", "Vie" };
            var resultado = new List<ResumenSemanalDto>(5);

            for (int i = 0; i < 5; i++)
            {
                var dia = lunes.AddDays(i);
                if (dia > hoy) break;

                porDia.TryGetValue(dia, out var datos);
                resultado.Add(new ResumenSemanalDto
                {
                    DiaNombre   = diasCortos[i],
                    FechaCorta  = dia.ToString("dd/MM"),
                    Asistencias = datos.Asist,
                    Tardanzas   = datos.Tard,
                    Ausencias   = Math.Max(0, totalEmpleados - datos.Asist),
                    Total       = totalEmpleados
                });
            }
            return resultado;
        }

        // ── Heatmap mensual ─────────────────────────────────────────────────────
        public async Task<List<HeatmapDiaDto>> ObtenerHeatmapMensualAsync(
            int mes = 0, int anio = 0, CancellationToken ct = default)
        {
            var hoy = DateTime.Today;
            if (mes <= 0) mes = hoy.Month;
            if (anio <= 0) anio = hoy.Year;

            var primerDia = new DateTime(anio, mes, 1);
            var ultimoDia = primerDia.AddMonths(1);

            int totalEmpleados = await _empleadoRepo.CountActivosAsync(ct);
            var marcajes       = await _marcajeRepo.GetByFechaAsync(primerDia, ultimoDia, ct);

            // Agrupar por fecha en un solo pase
            var porDia = new Dictionary<DateTime, (int Asist, int Tard)>();
            foreach (var m in marcajes)
            {
                var fecha = m.GetFechaHora().Date;
                if (!m.EsEntrada()) continue;

                porDia.TryGetValue(fecha, out var acc);
                acc.Asist++;
                if (m.EsTardanza()) acc.Tard++;
                porDia[fecha] = acc;
            }

            var resultado = new List<HeatmapDiaDto>();
            for (var dia = primerDia; dia < ultimoDia && dia <= hoy; dia = dia.AddDays(1))
            {
                porDia.TryGetValue(dia, out var datos);
                resultado.Add(new HeatmapDiaDto
                {
                    Fecha       = dia,
                    Asistencias = datos.Asist,
                    Tardanzas   = datos.Tard,
                    Ausencias   = Math.Max(0, totalEmpleados - datos.Asist)
                });
            }
            return resultado;
        }

        // ── Tabla actividad reciente con nombre resuelto ──────────────────────────
        public async Task<List<MarcajeResumenDto>> ObtenerUltimosMarcajesAsync(
            int cantidad = 10, CancellationToken ct = default)
        {
            var marcajes = await _marcajeRepo.GetUltimosAsync(cantidad, ct);
            if (marcajes.Count == 0) return new List<MarcajeResumenDto>();

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

        // ══════════════════════════════════════════════════════════════════════════
        // Métodos privados reutilizables
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Single-pass: recorre la colección UNA sola vez para contar entradas y tardanzas.
        /// Evita múltiples .Count() con predicados distintos sobre la misma lista.
        /// </summary>
        private static (int Asistencias, int Tardanzas) ContarEntradasYTardanzas(IEnumerable<Marcaje> marcajes)
        {
            int asistencias = 0, tardanzas = 0;
            foreach (var m in marcajes)
            {
                if (!m.EsEntrada()) continue;
                asistencias++;
                if (m.EsTardanza()) tardanzas++;
            }
            return (asistencias, tardanzas);
        }

        /// <summary>
        /// Resuelve nombres de empleados en bulk para una lista de DTOs.
        /// Bulk: EmpleadoIds → Empleados (IN query) → UsuarioIds → Usuarios (IN query) → Map.
        /// </summary>
        private async Task ResolverNombresEmpleados(List<TardanzaEmpleadoDto> items, CancellationToken ct)
        {
            var empIds    = items.Select(x => x.EmpleadoId).ToList();
            var empleados = await _empleadoRepo.GetByIdsAsync(empIds, ct);
            var empMap    = empleados.ToDictionary(e => e.GetId(), e => e);

            var usrIds   = empleados.Select(e => e.GetUsuarioId()).Distinct().ToList();
            var usuarios = await _usuarioRepo.GetByIdsAsync(usrIds, ct);
            var usrMap   = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            foreach (var item in items)
            {
                item.NombreEmpleado = empMap.TryGetValue(item.EmpleadoId, out var emp)
                    && usrMap.TryGetValue(emp.GetUsuarioId(), out var nombre)
                        ? nombre
                        : $"#{item.EmpleadoId}";
            }
        }
    }
}
