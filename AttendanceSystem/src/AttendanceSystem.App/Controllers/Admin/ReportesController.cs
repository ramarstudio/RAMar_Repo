using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class ReportesController
    {
        private readonly IEmpleadoSelectorService _selectorService;
        private readonly IReporteService          _reporteService;
        private readonly IExportService           _exportService;

        public ReportesController(
            IEmpleadoSelectorService selectorService,
            IReporteService          reporteService,
            IExportService           exportService)
        {
            _selectorService = selectorService;
            _reporteService  = reporteService;
            _exportService   = exportService;
        }

        // Delegado al servicio compartido — elimina duplicación con MarcajesAdminController
        public Task<List<EmpleadoSelectorDto>> ObtenerEmpleadosAsync()
            => _selectorService.ObtenerSelectorAsync();

        public async Task<(bool Ok, ReporteDto Reporte, string Mensaje)> GenerarReporteAsync(
            int empleadoId, int mes, int anio)
        {
            try
            {
                var dto = await _reporteService.GenerarReporteMensualAsync(empleadoId, mes, anio);
                return (true, dto, "Reporte generado correctamente.");
            }
            catch (System.Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool Ok, string RutaOError)> ExportarPdfAsync(ReporteDto reporte)
        {
            try   { return (true,  await _exportService.ExportarAPdfAsync(reporte)); }
            catch (System.Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Ok, string RutaOError)> ExportarCsvAsync(ReporteDto reporte)
        {
            try   { return (true,  await _exportService.ExportarACsvAsync(reporte)); }
            catch (System.Exception ex) { return (false, ex.Message); }
        }
    }
}
