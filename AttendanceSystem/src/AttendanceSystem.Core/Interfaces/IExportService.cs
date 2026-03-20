using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IExportService
    {
        Task<string> ExportarACsvAsync(ReporteDto reporte, CancellationToken ct = default);
        Task<string> ExportarAPdfAsync(ReporteDto reporte, CancellationToken ct = default);
    }
}
