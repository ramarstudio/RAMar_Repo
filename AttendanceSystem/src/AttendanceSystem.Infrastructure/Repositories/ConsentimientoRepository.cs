using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class ConsentimientoRepository : IConsentimientoRepository
{
    private readonly AppDbContext _context;

    public ConsentimientoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Consentimiento> GetByEmpleadoIdAsync(int empleadoId)
    {
        return await _context.Consentimientos
                             .FirstOrDefaultAsync(c => c.GetEmpleadoId() == empleadoId);
    }

    public async Task AddAsync(Consentimiento consentimiento)
    {
        _context.Consentimientos.Add(consentimiento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Consentimiento consentimiento)
    {
        _context.Consentimientos.Update(consentimiento);
        await _context.SaveChangesAsync();
    }
}
