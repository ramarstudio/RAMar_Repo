using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Options;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AttendanceSystem.Services
{
    public class ExportService : IExportService
    {
        private readonly string              _carpetaExportaciones;
        private readonly ILogger<ExportService> _logger;

        public ExportService(ExportOptions options, ILogger<ExportService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Carpeta calculada una sola vez en construcción
            _carpetaExportaciones = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                options?.Carpeta ?? "Exportaciones");

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportarACsvAsync(ReporteDto reporte, CancellationToken ct = default)
        {
            Directory.CreateDirectory(_carpetaExportaciones);

            string ruta = Path.Combine(_carpetaExportaciones,
                $"Reporte_{reporte.CodigoEmpleado}_{reporte.PeriodoInicio:MMyyyy}.csv");

            var sb = new StringBuilder();
            sb.AppendLine("Código Empleado,Periodo Inicio,Periodo Fin,Total Asistencias,Total Tardanzas,Minutos de Tardanza,Total Faltas");
            sb.AppendLine($"{reporte.CodigoEmpleado}," +
                          $"{reporte.PeriodoInicio:dd/MM/yyyy}," +
                          $"{reporte.PeriodoFin:dd/MM/yyyy}," +
                          $"{reporte.TotalAsistencias}," +
                          $"{reporte.TotalTardanzas}," +
                          $"{reporte.SumatoriaMinutosTardanza}," +
                          $"{reporte.TotalFaltas}");

            await File.WriteAllTextAsync(ruta, sb.ToString(), Encoding.UTF8, ct);
            _logger.LogInformation("CSV exportado: {Ruta}", ruta);
            return ruta;
        }

        // PDF en ThreadPool para no bloquear el dispatcher WPF
        public Task<string> ExportarAPdfAsync(ReporteDto reporte, CancellationToken ct = default)
        {
            Directory.CreateDirectory(_carpetaExportaciones);

            string ruta = Path.Combine(_carpetaExportaciones,
                $"Reporte_{reporte.CodigoEmpleado}_{reporte.PeriodoInicio:MMyyyy}.pdf");

            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("Reporte de Asistencia Mensual")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                        {
                            col.Spacing(20);
                            col.Item().Text($"Empleado: {reporte.CodigoEmpleado}").FontSize(14).SemiBold();
                            col.Item().Text($"Periodo: {reporte.PeriodoInicio:dd/MM/yyyy} al {reporte.PeriodoFin:dd/MM/yyyy}");
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                });
                                table.Header(header =>
                                {
                                    header.Cell().Text("Métrica").SemiBold();
                                    header.Cell().Text("Valor").SemiBold();
                                });
                                table.Cell().Text("Días Asistidos");
                                table.Cell().Text(reporte.TotalAsistencias.ToString());
                                table.Cell().Text("Total Tardanzas");
                                table.Cell().Text(reporte.TotalTardanzas.ToString());
                                table.Cell().Text("Minutos Acumulados de Tardanza");
                                table.Cell().Text($"{reporte.SumatoriaMinutosTardanza} min");
                                table.Cell().Text("Días de Falta");
                                table.Cell().Text(reporte.TotalFaltas.ToString());
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Generado por AttendanceSystem el ");
                            x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                        });
                    });
                }).GeneratePdf(ruta);

                _logger.LogInformation("PDF exportado: {Ruta}", ruta);
                return ruta;
            }, ct);
        }
    }
}
