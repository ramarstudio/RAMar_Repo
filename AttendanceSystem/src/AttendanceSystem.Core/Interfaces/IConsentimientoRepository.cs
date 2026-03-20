using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IConsentimientoRepository : IRepositoryBase<Consentimiento>
    {
        Task<Consentimiento> GetByEmpleadoIdAsync(int empleadoId);
    }
}
