using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IAuditRepository
    {
        Task<AuditLog> GetByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetByUsuarioIdAsync(int usuarioId);
        Task AddAsync(AuditLog auditLog);
        // Los AuditLogs son inmutables, por lo que no llevan métodos Update o Delete
    }
}