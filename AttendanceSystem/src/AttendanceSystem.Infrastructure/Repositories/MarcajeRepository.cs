using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class MarcajeRepository : IMarcajeRepository
{
    private readonly AppDbContext _context;

    public MarcajeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Marcaje> GetByIdAsync(int id)
    {
        return await _context.Marcajes.FindAsync(id);
    }

    // EF.Property con los nombres de campo mapeados en MarcajeConfiguration
    // → se traduce a SQL: WHERE empleado_id = X AND fecha_hora BETWEEN inicio AND fin
    public async Task<IEnumerable<Marcaje>> GetByEmpleadoIdAsync(
        int empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        return await _context.Marcajes
            .Where(m =>
                EF.Property<int>(m, "empleadoId") == empleadoId &&
                EF.Property<DateTime>(m, "fechaHora") >= fechaInicio &&
                EF.Property<DateTime>(m, "fechaHora") <= fechaFin)
            .OrderBy(m => EF.Property<DateTime>(m, "fechaHora"))
            .AsNoTracking()
            .ToListAsync();
    }

    // SQL: WHERE empleado_id = X AND DATE(fecha_hora) = DATE(@fecha) ORDER BY fecha_hora DESC LIMIT 1
    public async Task<Marcaje> GetUltimoMarcajeDelDiaAsync(int empleadoId, DateTime fecha)
    {
        return await _context.Marcajes
            .Where(m =>
                EF.Property<int>(m, "empleadoId") == empleadoId &&
                EF.Property<DateTime>(m, "fechaHora") >= fecha.Date &&
                EF.Property<DateTime>(m, "fechaHora") < fecha.Date.AddDays(1))
            .OrderByDescending(m => EF.Property<DateTime>(m, "fechaHora"))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Marcaje marcaje)
    {
        _context.Marcajes.Add(marcaje);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Marcaje marcaje)
    {
        _context.Marcajes.Update(marcaje);
        await _context.SaveChangesAsync();
    }
}
