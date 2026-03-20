using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IHorarioRepository : IRepositoryBase<Horario>
    {
        Task<IEnumerable<Horario>> GetByEmpleadoIdAsync(int empleadoId);
        Task<Horario>              GetHorarioVigenteAsync(int empleadoId, DiaSemana dia);
    }
}
