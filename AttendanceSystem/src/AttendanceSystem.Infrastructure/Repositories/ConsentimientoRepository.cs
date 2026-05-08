using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class ConsentimientoRepository : RepositoryBase<Consentimiento>, IConsentimientoRepository
{
    public ConsentimientoRepository(AppDbContext context) : base(context) { }

    public async Task<Consentimiento> GetByEmpleadoIdAsync(int empleadoId)
        => await _context.Consentimientos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Property<int>(c, "empleadoId") == empleadoId);
}
