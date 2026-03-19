using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class ReportesController
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly ReporteService      _reporteService;
        private readonly ExportService       _exportService;
        private readonly AppDbContext        _context;

        public ReportesController(
            IEmpleadoRepository empleadoRepo,
            ReporteService      reporteService,
            ExportService       exportService,
            AppDbContext        context)
        {
            _empleadoRepo   = empleadoRepo;
            _reporteService = reporteService;
            _exportService  = exportService;
            _context        = context;
        }

        // ── Lista de empleados activos con nombre del usuario vinculado ───────────
        public async Task<List<EmpleadoSelectorDto>> ObtenerEmpleadosAsync()
        {
            var empleados = await _empleadoRepo.GetAllActivosAsync();
            var empList   = empleados.ToList();

            var usuarioIds = empList.Select(e => e.GetUsuarioId()).Distinct().ToList();

            // Filtrado en BD con IN (...) — no carga todos los usuarios en memoria
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(u => usuarioIds.Contains(EF.Property<int>(u, "id")))
                .ToListAsync();

            var nombresMap = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            return empList
                .Select(e => new EmpleadoSelectorDto
                {
                    Id     = e.GetId(),
                    Codigo = e.GetCodigo(),
                    Nombre = nombresMap.TryGetValue(e.GetUsuarioId(), out var n) ? n : e.GetCodigo()
                })
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        // ── Generar reporte mensual ───────────────────────────────────────────────
        public async Task<(bool Ok, ReporteDto Reporte, string Mensaje)> GenerarReporteAsync(
            int empleadoId, int mes, int anio)
        {
            try
            {
                var dto = await _reporteService.GenerarReporteMensualAsync(empleadoId, mes, anio);
                return (true, dto, "Reporte generado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        // ── Exportar a PDF (async — no bloquea UI) ───────────────────────────────
        public async Task<(bool Ok, string RutaOError)> ExportarPdfAsync(ReporteDto reporte)
        {
            try   { return (true,  await _exportService.ExportarAPdfAsync(reporte)); }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ── Exportar a CSV ───────────────────────────────────────────────────────
        public async Task<(bool Ok, string RutaOError)> ExportarCsvAsync(ReporteDto reporte)
        {
            try   { return (true,  await _exportService.ExportarACsvAsync(reporte)); }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
