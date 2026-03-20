using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IReporteService
    {
        Task<ReporteDto> GenerarReporteMensualAsync(
            int empleadoId, int mes, int anio, CancellationToken ct = default);
    }
}
