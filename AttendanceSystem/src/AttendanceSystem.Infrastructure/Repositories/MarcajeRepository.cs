using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;

public class MarcajeRepository : RepositoryBase<Marcaje>, IMarcajeRepository
{
    public MarcajeRepository(AppDbContext context) : base(context) { }

    public async Task<Marcaje> GetByIdAsync(int id)
        => await _context.Marcajes.FindAsync(id);

    public async Task<IEnumerable<Marcaje>> GetByEmpleadoIdAsync(
        int empleadoId, DateTime fechaInicio, DateTime fechaFin)
        => await _context.Marcajes
            .Where(m =>
                EF.Property<int>(m, "empleadoId") == empleadoId &&
                EF.Property<DateTime>(m, "fechaHora") >= fechaInicio &&
                EF.Property<DateTime>(m, "fechaHora") <= fechaFin)
            .OrderBy(m => EF.Property<DateTime>(m, "fechaHora"))
            .AsNoTracking()
            .ToListAsync();

    public async Task<Marcaje> GetUltimoMarcajeDelDiaAsync(int empleadoId, DateTime fecha)
        => await _context.Marcajes
            .Where(m =>
                EF.Property<int>(m, "empleadoId") == empleadoId &&
                EF.Property<DateTime>(m, "fechaHora") >= fecha.Date &&
                EF.Property<DateTime>(m, "fechaHora") < fecha.Date.AddDays(1))
            .OrderByDescending(m => EF.Property<DateTime>(m, "fechaHora"))
            .AsNoTracking()
            .FirstOrDefaultAsync();

    // Conteo por rango de fechas — sin cargar entidades (usado por DashboardController)
    public async Task<int> CountByFechaRangoAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
        => await _context.Marcajes
            .CountAsync(m =>
                EF.Property<DateTime>(m, "fechaHora") >= desde &&
                EF.Property<DateTime>(m, "fechaHora") < hasta, ct);

    // Últimos N marcajes ordenados DESC — usado por Dashboard
    public async Task<List<Marcaje>> GetUltimosAsync(int cantidad, CancellationToken ct = default)
        => await _context.Marcajes
            .AsNoTracking()
            .OrderByDescending(m => EF.Property<DateTime>(m, "fechaHora"))
            .Take(cantidad)
            .ToListAsync(ct);

    // Marcajes del día para KPIs — filtra solo por fecha sin empleadoId
    public async Task<List<Marcaje>> GetByFechaAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
        => await _context.Marcajes
            .AsNoTracking()
            .Where(m =>
                EF.Property<DateTime>(m, "fechaHora") >= desde &&
                EF.Property<DateTime>(m, "fechaHora") < hasta)
            .ToListAsync(ct);

    // Asistencias diarias — proyección mínima (solo DateTime + bool) para gráfico de líneas
    public async Task<List<AsistenciaDiariaDto>> GetAsistenciasDiariasAsync(
        int dias, CancellationToken ct = default)
    {
        var desde = DateTime.Today.AddDays(-(dias - 1));
        var hasta = DateTime.Today.AddDays(1);  // exclusivo

        // Proyección mínima: solo fechaHora y tardanza para Entradas del rango
        var datos = await _context.Marcajes
            .AsNoTracking()
            .Where(m =>
                EF.Property<TipoMarcaje>(m, "tipo") == TipoMarcaje.Entrada &&
                EF.Property<DateTime>(m, "fechaHora") >= desde &&
                EF.Property<DateTime>(m, "fechaHora") <  hasta)
            .Select(m => new
            {
                Fecha    = EF.Property<DateTime>(m, "fechaHora"),
                Tardanza = EF.Property<bool>(m, "tardanza")
            })
            .ToListAsync(ct);

        // Agrupa en memoria: dataset pequeño (días × empleados)
        var result = new List<AsistenciaDiariaDto>(dias);
        for (var day = desde.Date; day < hasta.Date; day = day.AddDays(1))
        {
            var del_dia = datos.Where(d => d.Fecha.Date == day).ToList();
            result.Add(new AsistenciaDiariaDto
            {
                Fecha     = day,
                Total     = del_dia.Count,
                Tardanzas = del_dia.Count(d => d.Tardanza)
            });
        }
        return result;
    }

    // Top N empleados con más tardanzas — GROUP BY server-side en BD
    public async Task<List<TardanzaEmpleadoDto>> GetTopTardanzasAsync(
        int topN, int dias, CancellationToken ct = default)
    {
        var desde = DateTime.Today.AddDays(-(dias - 1));
        var hasta = DateTime.Today.AddDays(1);

        var agrupado = await _context.Marcajes
            .AsNoTracking()
            .Where(m =>
                EF.Property<TipoMarcaje>(m, "tipo") == TipoMarcaje.Entrada &&
                EF.Property<bool>(m, "tardanza")    == true &&
                EF.Property<DateTime>(m, "fechaHora") >= desde &&
                EF.Property<DateTime>(m, "fechaHora") <  hasta)
            .GroupBy(m => EF.Property<int>(m, "empleadoId"))
            .Select(g => new
            {
                EmpleadoId     = g.Key,
                TotalTardanzas = g.Count(),
                MinutosTotales = g.Sum(m => EF.Property<int>(m, "min_tardanza"))
            })
            .OrderByDescending(x => x.TotalTardanzas)
            .Take(topN)
            .ToListAsync(ct);

        // NombreEmpleado se resuelve en el controller (evita JOIN cross-layer en repo)
        return agrupado.Select(x => new TardanzaEmpleadoDto
        {
            EmpleadoId     = x.EmpleadoId,
            NombreEmpleado = string.Empty,
            TotalTardanzas = x.TotalTardanzas,
            MinutosTotales = x.MinutosTotales
        }).ToList();
    }

    // Top N empleados más puntuales — GROUP BY server-side, cuenta entradas sin tardanza
    public async Task<List<TardanzaEmpleadoDto>> GetTopPuntualesAsync(
        int topN, int dias, CancellationToken ct = default)
    {
        var desde = DateTime.Today.AddDays(-(dias - 1));
        var hasta = DateTime.Today.AddDays(1);

        var agrupado = await _context.Marcajes
            .AsNoTracking()
            .Where(m =>
                EF.Property<TipoMarcaje>(m, "tipo") == TipoMarcaje.Entrada &&
                EF.Property<bool>(m, "tardanza")    == false &&
                EF.Property<DateTime>(m, "fechaHora") >= desde &&
                EF.Property<DateTime>(m, "fechaHora") <  hasta)
            .GroupBy(m => EF.Property<int>(m, "empleadoId"))
            .Select(g => new
            {
                EmpleadoId    = g.Key,
                EntradasATiempo = g.Count()
            })
            .OrderByDescending(x => x.EntradasATiempo)
            .Take(topN)
            .ToListAsync(ct);

        return agrupado.Select(x => new TardanzaEmpleadoDto
        {
            EmpleadoId     = x.EmpleadoId,
            NombreEmpleado = string.Empty,
            TotalTardanzas = x.EntradasATiempo, // reutilizado: entradas a tiempo
            MinutosTotales = 0
        }).ToList();
    }
}
