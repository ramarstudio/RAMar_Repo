using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly AppDbContext _context;

    public EmpleadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Empleado> GetByIdAsync(int id)
    {
        return await _context.Empleados
                             .Include(e => e.GetHorarios())
                             .Include(e => e.GetConsentimiento())
                             .Include(e => e.GetEmbeddingFacial())
                             .FirstOrDefaultAsync(e => e.GetId() == id);
    }

    public async Task<Empleado> GetByCodigoAsync(string codigo)
    {
        return await _context.Empleados
                             .Include(e => e.GetHorarios()) // Depende de cuánto gráfico se requiera por default
                             .FirstOrDefaultAsync(e => e.GetCodigo() == codigo && e.GetActivo());
    }

    public async Task<IEnumerable<Empleado>> GetAllActivosAsync()
    {
        return await _context.Empleados
                             .Where(e => e.GetActivo())
                             .ToListAsync();
    }

    public async Task AddAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
        await _context.SaveChangesAsync();
    }
}
