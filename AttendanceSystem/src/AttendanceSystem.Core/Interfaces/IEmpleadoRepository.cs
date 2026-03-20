using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IEmpleadoRepository : IRepositoryBase<Empleado>
    {
        Task<Empleado>              GetByIdAsync(int id);
        Task<Empleado>              GetByCodigoAsync(string codigo);
        Task<IEnumerable<Empleado>> GetAllActivosAsync();
    }
}
