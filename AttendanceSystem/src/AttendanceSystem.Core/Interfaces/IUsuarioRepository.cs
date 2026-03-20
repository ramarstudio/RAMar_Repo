using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IUsuarioRepository : IRepositoryBase<Usuario>
    {
        Task<Usuario>              GetByIdAsync(int id);
        Task<Usuario>              GetByUsernameAsync(string username);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<List<Usuario>>        GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task                       DeleteAsync(int id);
    }
}
