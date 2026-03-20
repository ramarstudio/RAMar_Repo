using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;

public class HorarioRepository : RepositoryBase<Horario>, IHorarioRepository
{
    public HorarioRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Horario>> GetByEmpleadoIdAsync(int empleadoId)
        => await _context.Horarios
            .Where(h => EF.Property<int>(h, "empleadoId") == empleadoId)
            .ToListAsync();

    public async Task<Horario> GetHorarioVigenteAsync(int empleadoId, DiaSemana dia)
    {
        var now = DateTime.UtcNow;
        return await _context.Horarios
            .Where(h =>
                EF.Property<int>(h, "empleadoId")      == empleadoId &&
                EF.Property<DiaSemana>(h, "dia")       == dia &&
                EF.Property<DateTime>(h, "vigente_desde") <= now &&
                EF.Property<DateTime>(h, "vigente_hasta") >= now)
            .FirstOrDefaultAsync();
    }
}
