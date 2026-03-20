using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    // Los AuditLogs son inmutables (append-only), por eso no heredan UpdateAsync del base.
    // Solo heredan AddAsync y SaveAsync de IRepositoryBase<AuditLog>.
    public interface IAuditRepository : IRepositoryBase<AuditLog>
    {
        Task<AuditLog>              GetByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetByUsuarioIdAsync(int usuarioId);
    }
}
