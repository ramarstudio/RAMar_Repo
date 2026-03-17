using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.Enums; // Asumiendo que DíaSemana está aquí

namespace AttendanceSystem.Core.Interfaces
{
    public interface IHorarioRepository
    {
        Task<IEnumerable<Horario>> GetByEmpleadoIdAsync(int empleadoId);
        Task<Horario> GetHorarioVigenteAsync(int empleadoId, DiaSemana dia);
        Task AddAsync(Horario horario);
        Task UpdateAsync(Horario horario);
    }
}