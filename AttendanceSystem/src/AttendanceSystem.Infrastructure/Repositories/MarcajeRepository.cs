using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
}
