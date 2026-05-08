using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IEmpleadoRepository : IRepositoryBase<Empleado>
    {
        Task<Empleado>              GetByIdAsync(int id);
        Task<Empleado>              GetByCodigoAsync(string codigo);
        Task<Empleado>              GetByCodigoConEmbeddingAsync(string codigo);
        Task<Empleado>              GetByUsuarioIdAsync(int usuarioId);
        Task<List<Empleado>>        GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<IEnumerable<Empleado>> GetAllActivosAsync();
        Task<int>                   CountActivosAsync(CancellationToken ct = default);
    }
}
