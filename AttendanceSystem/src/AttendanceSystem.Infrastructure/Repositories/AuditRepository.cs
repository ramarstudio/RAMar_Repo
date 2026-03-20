using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class AuditRepository : RepositoryBase<AuditLog>, IAuditRepository
{
    public AuditRepository(AppDbContext context) : base(context) { }

    public async Task<AuditLog> GetByIdAsync(int id)
        => await _context.AuditLogs.FindAsync(id);

    public async Task<IEnumerable<AuditLog>> GetByUsuarioIdAsync(int usuarioId)
        => await _context.AuditLogs
            .Where(a => a.GetUsuarioId() == usuarioId)
            .OrderByDescending(a => a.GetFecha())
            .ToListAsync();
}
