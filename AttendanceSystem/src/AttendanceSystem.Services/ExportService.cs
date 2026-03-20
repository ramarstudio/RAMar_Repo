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

        // Paleta consistente con el tema oscuro de la app
        private const string BrandDark   = "#0D1B2A";
        private const string BrandBlue   = "#1E3A5F";
        private const string AccentBlue  = "#7EB8DA";
        private const string PastelGreen = "#81C995";
        private const string PastelOrange = "#F4A97D";
        private const string PastelPink  = "#E8879B";
        private const string PastelPurple = "#B8A9D4";
        private const string TextLight   = "#E8EEF2";
        private const string TextMuted   = "#6B8DA6";
        private const string TableBorder = "#2A3746";

        public ExportService(ExportOptions options, ILogger<ExportService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                        // ── HEADER ──────────────────────────────────────────────
                        page.Header().Column(col =>
                        {
                            // Banner con gradiente simulado
                            col.Item().Background(BrandDark).Padding(20).Row(row =>
                            {
                                row.RelativeItem().Column(inner =>
                                {
                                    inner.Item().Text("RAMar")
                                        .FontSize(24).Bold().FontColor(AccentBlue);
                                    inner.Item().Text("Reporte de Asistencia Mensual")
                                        .FontSize(14).FontColor(TextMuted);
                                });
                                row.ConstantItem(140).AlignRight().AlignMiddle().Column(inner =>
                                {
                                    inner.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy}")
                                        .FontSize(9).FontColor(TextMuted).AlignRight();
                                    inner.Item().Text($"{DateTime.Now:HH:mm}")
                                        .FontSize(9).FontColor(TextMuted).AlignRight();
                                });
                            });

                            // Línea de acento
                            col.Item().Height(3).Background(AccentBlue);
                        });

                        // ── CONTENIDO ────────────────────────────────────────────
                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Spacing(16);

                            // Datos del empleado
                            col.Item().Background(Colors.Grey.Lighten4).Padding(16).Row(row =>
                            {
                                row.RelativeItem().Column(inner =>
                                {
                                    inner.Item().Text("EMPLEADO").FontSize(8).Bold()
                                        .FontColor(Colors.Grey.Medium).LetterSpacing(0.5f);
                                    inner.Item().Text(reporte.CodigoEmpleado)
                                        .FontSize(16).Bold().FontColor(BrandDark);
                                });
                                row.RelativeItem().Column(inner =>
                                {
                                    inner.Item().Text("PERIODO").FontSize(8).Bold()
                                        .FontColor(Colors.Grey.Medium).LetterSpacing(0.5f);
                                    inner.Item().Text($"{reporte.PeriodoInicio:dd/MM/yyyy}  —  {reporte.PeriodoFin:dd/MM/yyyy}")
                                        .FontSize(12).FontColor(Colors.Grey.Darken2);
                                });
                            });

                            // KPIs en grid de 4 columnas
                            col.Item().Row(row =>
                            {
                                row.Spacing(10);
                                GenerarKpiCard(row.RelativeItem(), "Días Asistidos",
                                    reporte.TotalAsistencias.ToString(), PastelGreen);
                                GenerarKpiCard(row.RelativeItem(), "Tardanzas",
                                    reporte.TotalTardanzas.ToString(), PastelOrange);
                                GenerarKpiCard(row.RelativeItem(), "Faltas",
                                    reporte.TotalFaltas.ToString(), PastelPink);
                                GenerarKpiCard(row.RelativeItem(), "Min. Tardanza",
                                    reporte.SumatoriaMinutosTardanza.ToString(), PastelPurple);
                            });

                            // Separador
                            col.Item().PaddingVertical(4).LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            // Tabla detallada
                            col.Item().Text("Detalle de Métricas")
                                .FontSize(13).Bold().FontColor(BrandDark);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(3);
                                });

                                // Header de tabla
                                table.Header(header =>
                                {
                                    header.Cell().Background(BrandDark).Padding(8)
                                        .Text("Métrica").FontSize(10).Bold().FontColor(AccentBlue);
                                    header.Cell().Background(BrandDark).Padding(8)
                                        .Text("Valor").FontSize(10).Bold().FontColor(AccentBlue).AlignCenter();
                                    header.Cell().Background(BrandDark).Padding(8)
                                        .Text("Observación").FontSize(10).Bold().FontColor(AccentBlue);
                                });

                                // Filas de datos
                                AgregarFila(table, "Días Asistidos", reporte.TotalAsistencias.ToString(),
                                    "Cantidad de días con registro de entrada", false);
                                AgregarFila(table, "Total Tardanzas", reporte.TotalTardanzas.ToString(),
                                    "Entradas registradas después de la hora asignada", true);
                                AgregarFila(table, "Minutos Acum. Tardanza", $"{reporte.SumatoriaMinutosTardanza} min",
                                    "Sumatoria de todos los minutos de tardanza", false);
                                AgregarFila(table, "Días de Falta", reporte.TotalFaltas.ToString(),
                                    "Días laborables sin registro de asistencia", true);

                                // Resumen
                                double tasaPuntualidad = reporte.TotalAsistencias > 0
                                    ? Math.Round((double)(reporte.TotalAsistencias - reporte.TotalTardanzas)
                                        / reporte.TotalAsistencias * 100, 1) : 0;

                                AgregarFila(table, "Tasa de Puntualidad", $"{tasaPuntualidad}%",
                                    "Porcentaje de asistencias sin tardanza", false);
                            });

                            // Nota al pie del contenido
                            col.Item().PaddingTop(12).Text(text =>
                            {
                                text.Span("Nota: ").Bold().FontSize(9).FontColor(Colors.Grey.Medium);
                                text.Span("Este reporte es generado automáticamente por el sistema RAMar. " +
                                          "Los datos reflejan el estado al momento de la generación.")
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                        });

                        // ── FOOTER ──────────────────────────────────────────────
                        page.Footer().Height(30).Background(Colors.Grey.Lighten4)
                            .Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text("RAMar · Control de Asistencia")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                                text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
                                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
                }).GeneratePdf(ruta);

                _logger.LogInformation("PDF exportado: {Ruta}", ruta);
                return ruta;
            }, ct);
        }

        private static void GenerarKpiCard(IContainer container, string titulo, string valor, string color)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2)
                .Column(col =>
            {
                col.Item().Height(4).Background(color);
                col.Item().Padding(12).Column(inner =>
                {
                    inner.Item().Text(valor).FontSize(24).Bold().FontColor(color);
                    inner.Item().Text(titulo).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void AgregarFila(TableDescriptor table, string metrica, string valor,
            string observacion, bool alternar)
        {
            var bg = alternar ? Colors.Grey.Lighten4 : Colors.White;

            table.Cell().Background(bg).Padding(8).Text(metrica)
                .FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
            table.Cell().Background(bg).Padding(8).AlignCenter()
                .Text(valor).FontSize(10).Bold().FontColor(BrandDark);
            table.Cell().Background(bg).Padding(8).Text(observacion)
                .FontSize(9).FontColor(Colors.Grey.Medium);
        }
    }
}
