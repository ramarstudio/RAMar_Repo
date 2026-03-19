using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AttendanceSystem.Services
{
    public class ExportService
    {
        // Ruta única calculada una sola vez: evita llamadas repetidas a Path.Combine + AppDomain
        private static readonly string CarpetaExportaciones =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exportaciones");

        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportarACsvAsync(ReporteDto reporte)
        {
            Directory.CreateDirectory(CarpetaExportaciones);

            string rutaCompleta = Path.Combine(CarpetaExportaciones,
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

            await File.WriteAllTextAsync(rutaCompleta, sb.ToString(), Encoding.UTF8);
            return rutaCompleta;
        }

        // PDF ahora es async: se ejecuta en el thread pool para no bloquear el dispatcher WPF.
        public Task<string> ExportarAPdfAsync(ReporteDto reporte)
        {
            Directory.CreateDirectory(CarpetaExportaciones);

            string rutaCompleta = Path.Combine(CarpetaExportaciones,
                $"Reporte_{reporte.CodigoEmpleado}_{reporte.PeriodoInicio:MMyyyy}.pdf");

            // Task.Run mueve la generación del PDF al ThreadPool,
            // liberando el dispatcher WPF durante la operación bloqueante.
            return Task.Run(() =>
            {
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
                }).GeneratePdf(rutaCompleta);

                return rutaCompleta;
            });
        }
    }
}
