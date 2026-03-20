using System.Threading;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    // Contrato base para todos los repositorios.
    // Centraliza AddAsync/UpdateAsync/SaveAsync con soporte de CancellationToken.
    public interface IRepositoryBase<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
    }
}
