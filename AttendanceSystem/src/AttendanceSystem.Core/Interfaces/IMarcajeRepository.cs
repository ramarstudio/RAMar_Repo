using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IMarcajeRepository
    {
        Task<Marcaje> GetByIdAsync(int id);
        Task<IEnumerable<Marcaje>> GetByEmpleadoIdAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
        Task<Marcaje> GetUltimoMarcajeDelDiaAsync(int empleadoId, DateTime fecha);
        Task AddAsync(Marcaje marcaje);
        Task UpdateAsync(Marcaje marcaje);
    }
}