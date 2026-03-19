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

    // FindAsync usa la PK directamente — sin expression tree, sin riesgo de traducción
    public async Task<Empleado> GetByIdAsync(int id)
    {
        return await _context.Empleados
            .Include("horarios")
            .Include("embeddingFacial")
            .Include("consentimiento")
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "id") == id);
    }

    // EF.Property accede al campo mapeado por nombre — traducible a SQL
    public async Task<Empleado> GetByCodigoAsync(string codigo)
    {
        return await _context.Empleados
            .Include("horarios")
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                EF.Property<string>(e, "codigo") == codigo &&
                EF.Property<bool>(e, "activo") == true);
    }

    // Filtrado SQL directo sobre el campo mapeado — O(n) en BD, no en memoria
    public async Task<IEnumerable<Empleado>> GetAllActivosAsync()
    {
        return await _context.Empleados
            .Where(e => EF.Property<bool>(e, "activo") == true)
            .AsNoTracking()
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
