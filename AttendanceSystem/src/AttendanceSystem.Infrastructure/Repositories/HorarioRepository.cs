using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;

public class HorarioRepository : IHorarioRepository
{
    private readonly AppDbContext _context;

    public HorarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Horario>> GetByEmpleadoIdAsync(int empleadoId)
    {
        // El EF proxy backing field o query sobre Empleado puede requerir explicit join si no podemos usar GetEmpleadoId directamente.
        // Asumiendo EF core translate para el navigation prop del empleado, de lo contrario usaremos EF.Property:
        return await _context.Horarios
                             .Where(h => EF.Property<int>(h, "EmpleadoId") == empleadoId)
                             .ToListAsync();
    }

    public async Task<Horario> GetHorarioVigenteAsync(int empleadoId, DiaSemana dia)
    {
        var now = System.DateTime.UtcNow;
        return await _context.Horarios
                             .Where(h => EF.Property<int>(h, "EmpleadoId") == empleadoId 
                                         && h.GetDia() == dia
                                         && h.GetVigenteDesde() <= now 
                                         && h.GetVigenteHasta() >= now)
                             .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Horario horario)
    {
        _context.Horarios.Add(horario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Horario horario)
    {
        _context.Horarios.Update(horario);
        await _context.SaveChangesAsync();
    }
}
