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
        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportarACsvAsync(ReporteDto reporte)
        {
            //1.Crear la carpeta "Exportaciones" dentro de la carpeta donde corre la app
            string carpetaDestino = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exportaciones");
            Directory.CreateDirectory(carpetaDestino);

            //2.Generar un nombre único para el archivo
            string nombreArchivo = $"Reporte_{reporte.CodigoEmpleado}_{reporte.PeriodoInicio:MMyyyy}.csv";
            string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            //3.Construir el contenido del CSV
            var sb = new StringBuilder();
            
            //Fila de encabezados
            sb.AppendLine("Código Empleado,Periodo Inicio,Periodo Fin,Total Asistencias,Total Tardanzas,Minutos de Tardanza,Total Faltas");
            
            //Fila de datos
            sb.AppendLine($"{reporte.CodigoEmpleado},{reporte.PeriodoInicio:dd/MM/yyyy},{reporte.PeriodoFin:dd/MM/yyyy},{reporte.TotalAsistencias},{reporte.TotalTardanzas},{reporte.SumatoriaMinutosTardanza},{reporte.TotalFaltas}");

            //4.Escribir el archivo en el disco
            await File.WriteAllTextAsync(rutaCompleta, sb.ToString(), Encoding.UTF8);

            return rutaCompleta;
        }

        public string ExportarAPdf(ReporteDto reporte)
        {
            string carpetaDestino = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exportaciones");
            Directory.CreateDirectory(carpetaDestino);

            string nombreArchivo = $"Reporte_{reporte.CodigoEmpleado}_{reporte.PeriodoInicio:MMyyyy}.pdf";
            string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            //5.Diseñar el PDF usando QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    //Encabezado del PDF
                    page.Header().Text("Reporte de Asistencia Mensual")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    //Contenido principal
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(20);
                        
                        x.Item().Text($"Empleado: {reporte.CodigoEmpleado}").FontSize(14).SemiBold();
                        x.Item().Text($"Periodo: {reporte.PeriodoInicio:dd/MM/yyyy} al {reporte.PeriodoFin:dd/MM/yyyy}");

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        //Tabla de métricas
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
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

                    //Pie de página
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado por AttendanceSystem el ");
                        x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });
            })
            .GeneratePdf(rutaCompleta);

            return rutaCompleta;
        }
    }
}