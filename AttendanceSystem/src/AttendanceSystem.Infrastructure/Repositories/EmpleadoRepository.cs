using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class EmpleadoRepository : RepositoryBase<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(AppDbContext context) : base(context) { }

    public async Task<Empleado> GetByIdAsync(int id)
        => await _context.Empleados
            .Include("horarios")
            .Include("embeddingFacial")
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "id") == id);

    public async Task<Empleado> GetByCodigoAsync(string codigo)
        => await _context.Empleados
            .Include("horarios")
            .Include("embeddingFacial")
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                EF.Property<string>(e, "codigo").ToLower() == codigo.ToLower() &&
                EF.Property<bool>(e, "activo") == true);

    // Consulta ligera para operaciones biométricas: solo carga embeddingFacial,
    // sin horarios. Evita que Update() marque todos los horarios como Modified.
    public async Task<Empleado> GetByCodigoConEmbeddingAsync(string codigo)
        => await _context.Empleados
            .Include("embeddingFacial")
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                EF.Property<string>(e, "codigo").ToLower() == codigo.ToLower() &&
                EF.Property<bool>(e, "activo") == true);

    public async Task<Empleado> GetByUsuarioIdAsync(int usuarioId)
        => await _context.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "usuarioId") == usuarioId);

    public async Task<List<Empleado>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids as List<int> ?? ids.ToList();
        return await _context.Empleados
            .AsNoTracking()
            .Where(e => idList.Contains(EF.Property<int>(e, "id")))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Empleado>> GetAllActivosAsync()
        => await _context.Empleados
            .Where(e => EF.Property<bool>(e, "activo") == true)
            .AsNoTracking()
            .ToListAsync();

    public Task<int> CountActivosAsync(CancellationToken ct = default)
        => _context.Empleados
            .CountAsync(e => EF.Property<bool>(e, "activo") == true, ct);
}
