using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class DashboardView : UserControl
    {
        private readonly DashboardController _ctrl;

        public DashboardView(DashboardController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────
        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            int hora = DateTime.Now.Hour;
            txtSaludo.Text      = hora < 12 ? "Buenos días," : hora < 18 ? "Buenas tardes," : "Buenas noches,";
            txtNombreAdmin.Text = _ctrl.ObtenerNombreAdmin();
            txtFechaHoy.Text    = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                                      new System.Globalization.CultureInfo("es-ES"));
            try
            {
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MostrarError();
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Botón Actualizar ──────────────────────────────────────────────────────
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try { await CargarDatosAsync(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Carga completa — secuencial para evitar conflictos de DbContext ───────
        private async Task CargarDatosAsync()
        {
            // Loading state
            txtTotalEmpleados.Text = "…";
            txtAsistenciasHoy.Text = "…";
            txtTardanzasHoy.Text   = "…";
            txtAusenciasHoy.Text   = "…";
            txtTasaAsistencia.Text = "…";
            txtMarcajes7Dias.Text  = "…";

            var kpis      = await _ctrl.ObtenerKpisAsync();
            var diarias   = await _ctrl.ObtenerAsistenciasDiariasAsync(14);
            var tardanzas = await _ctrl.ObtenerTopTardanzasAsync(5, 30);
            var recientes = await _ctrl.ObtenerUltimosMarcajesAsync(10);

            // KPI cards
            txtTotalEmpleados.Text = kpis.TotalEmpleadosActivos.ToString();
            txtAsistenciasHoy.Text = kpis.AsistenciasHoy.ToString();
            txtTardanzasHoy.Text   = kpis.TardanzasHoy.ToString();
            txtAusenciasHoy.Text   = kpis.AusenciasHoy.ToString();
            txtTasaAsistencia.Text = $"{kpis.TasaAsistenciaHoy}%";
            txtMarcajes7Dias.Text  = kpis.MarcajesUltimos7Dias.ToString();

            // Gráficos
            ConfigurarGraficoLinea(diarias);
            ConfigurarGraficoDonut(kpis);
            ConfigurarGraficoBarras(tardanzas);

            // Tabla actividad reciente
            dgReciente.ItemsSource   = recientes;
            txtContadorReciente.Text = $"{recientes.Count} registros";
        }

        // ── Gráfico de líneas — asistencias y tardanzas diarias ───────────────────
        private void ConfigurarGraficoLinea(List<AsistenciaDiariaDto> datos)
        {
            var asistencias = datos.Select(d => (double)d.Total).ToArray();
            var tardanzas   = datos.Select(d => (double)d.Tardanzas).ToArray();
            var labels      = datos.Select(d => d.Fecha.ToString("dd/MM")).ToArray();

            chartLinea.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values        = asistencias,
                    Name          = "Asistencias",
                    Stroke        = new SolidColorPaint(new SKColor(21, 101, 192)) { StrokeThickness = 2.5f },
                    Fill          = new SolidColorPaint(new SKColor(21, 101, 192, 30)),
                    GeometryFill  = new SolidColorPaint(new SKColor(21, 101, 192)),
                    GeometryStroke= new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    GeometrySize  = 8,
                    LineSmoothness= 0.5
                },
                new LineSeries<double>
                {
                    Values        = tardanzas,
                    Name          = "Tardanzas",
                    Stroke        = new SolidColorPaint(new SKColor(230, 81, 0)) { StrokeThickness = 2f },
                    Fill          = new SolidColorPaint(new SKColor(230, 81, 0, 20)),
                    GeometryFill  = new SolidColorPaint(new SKColor(230, 81, 0)),
                    GeometryStroke= new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    GeometrySize  = 7,
                    LineSmoothness= 0.5
                }
            };

            chartLinea.XAxes = new[]
            {
                new Axis
                {
                    Labels    = labels,
                    LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)),
                    TextSize  = 10,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80))
                }
            };

            chartLinea.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)),
                    TextSize  = 10,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80))
                }
            };

            chartLinea.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartLinea.LegendTextPaint = new SolidColorPaint(new SKColor(69, 90, 100));
        }

        // ── Gráfico donut — distribución asistencias/tardanzas/ausencias hoy ──────
        private void ConfigurarGraficoDonut(DashboardKpiDto kpis)
        {
            int presentes  = Math.Max(0, kpis.AsistenciasHoy - kpis.TardanzasHoy);
            int tardanzas  = kpis.TardanzasHoy;
            int ausencias  = kpis.AusenciasHoy;

            // Si no hay datos muestra placeholder
            if (presentes == 0 && tardanzas == 0 && ausencias == 0)
            {
                chartDonut.Series = new ISeries[]
                {
                    new PieSeries<double>
                    {
                        Values  = new double[] { 1 },
                        Name    = "Sin datos",
                        Fill    = new SolidColorPaint(new SKColor(200, 200, 200))
                    }
                };
                return;
            }

            var series = new List<ISeries>();
            if (presentes > 0)
                series.Add(new PieSeries<double>
                {
                    Values          = new double[] { presentes },
                    Name            = "A tiempo",
                    Fill            = new SolidColorPaint(new SKColor(46, 125, 50)),
                    InnerRadius     = 50,
                    MaxRadialColumnWidth = 30
                });
            if (tardanzas > 0)
                series.Add(new PieSeries<double>
                {
                    Values          = new double[] { tardanzas },
                    Name            = "Tardanzas",
                    Fill            = new SolidColorPaint(new SKColor(230, 81, 0)),
                    InnerRadius     = 50,
                    MaxRadialColumnWidth = 30
                });
            if (ausencias > 0)
                series.Add(new PieSeries<double>
                {
                    Values          = new double[] { ausencias },
                    Name            = "Ausencias",
                    Fill            = new SolidColorPaint(new SKColor(173, 20, 87)),
                    InnerRadius     = 50,
                    MaxRadialColumnWidth = 30
                });

            chartDonut.Series        = series;
            chartDonut.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartDonut.LegendTextPaint = new SolidColorPaint(new SKColor(69, 90, 100));
        }

        // ── Gráfico de barras — top 5 empleados con más tardanzas ─────────────────
        private void ConfigurarGraficoBarras(List<TardanzaEmpleadoDto> datos)
        {
            if (!datos.Any())
            {
                chartBarras.Series = Array.Empty<ISeries>();
                return;
            }

            var valores = datos.Select(d => (double)d.TotalTardanzas).ToArray();
            var labels  = datos.Select(d => d.NombreEmpleado).ToArray();

            chartBarras.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values  = valores,
                    Name    = "Tardanzas",
                    Fill    = new SolidColorPaint(new SKColor(230, 81, 0, 200)),
                    Stroke  = new SolidColorPaint(new SKColor(230, 81, 0)) { StrokeThickness = 1 },
                    MaxBarWidth = 40,
                    Rx = 4,
                    Ry = 4
                }
            };

            chartBarras.XAxes = new[]
            {
                new Axis
                {
                    Labels    = labels,
                    LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)),
                    TextSize  = 10,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Transparent)
                }
            };

            chartBarras.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)),
                    TextSize  = 10,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80))
                }
            };

            chartBarras.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        }

        private void MostrarError()
        {
            txtTotalEmpleados.Text = "!";
            txtAsistenciasHoy.Text = "!";
            txtTardanzasHoy.Text   = "!";
            txtAusenciasHoy.Text   = "!";
            txtTasaAsistencia.Text = "!";
            txtMarcajes7Dias.Text  = "!";
        }
    }
}
