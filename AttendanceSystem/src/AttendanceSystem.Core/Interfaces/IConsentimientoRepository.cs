using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IConsentimientoRepository
    {
        Task<Consentimiento> GetByEmpleadoIdAsync(int empleadoId);
        Task AddAsync(Consentimiento consentimiento);
        Task UpdateAsync(Consentimiento consentimiento);
    }
}