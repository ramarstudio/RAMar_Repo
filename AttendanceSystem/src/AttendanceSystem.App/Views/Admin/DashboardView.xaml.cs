using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        /// <summary>Se dispara cuando los KPIs terminan de cargar, con (tardanzas, ausencias) del día.</summary>
        public event Action<int, int> AlertasCalculadas;

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
            try { await CargarDatosAsync(); }
            catch (Exception ex)
            {
                MostrarError();
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try { await CargarDatosAsync(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Carga completa ──────────────────────────────────────────────────────
        private async Task CargarDatosAsync()
        {
            txtTotalEmpleados.Text = "…";
            txtAsistenciasHoy.Text = "…";
            txtTardanzasHoy.Text   = "…";
            txtAusenciasHoy.Text   = "…";
            txtTasaAsistencia.Text = "…";
            txtMarcajes7Dias.Text  = "…";

            // Todas las queries secuenciales (mismo DbContext scope)
            var kpis       = await _ctrl.ObtenerKpisAsync();
            var tendencias = await _ctrl.ObtenerTendenciasAsync();
            var diarias    = await _ctrl.ObtenerAsistenciasDiariasAsync(14);
            var tardanzas  = await _ctrl.ObtenerTopTardanzasAsync(5, 30);
            var puntuales  = await _ctrl.ObtenerTopPuntualesAsync(5, 30);
            var recientes  = await _ctrl.ObtenerUltimosMarcajesAsync(10);
            var semana     = await _ctrl.ObtenerResumenSemanalAsync();
            var heatmap    = await _ctrl.ObtenerHeatmapMensualAsync();

            // KPI cards
            txtTotalEmpleados.Text = kpis.TotalEmpleadosActivos.ToString();
            txtAsistenciasHoy.Text = kpis.AsistenciasHoy.ToString();
            txtTardanzasHoy.Text   = kpis.TardanzasHoy.ToString();
            txtAusenciasHoy.Text   = kpis.AusenciasHoy.ToString();
            txtTasaAsistencia.Text = $"{kpis.TasaAsistenciaHoy}%";
            txtMarcajes7Dias.Text  = kpis.MarcajesUltimos7Dias.ToString();

            // Gauge bar
            double gaugeWidth = barGauge.Parent is Grid gaugeParent
                ? gaugeParent.ActualWidth * (kpis.TasaAsistenciaHoy / 100.0)
                : 0;
            barGauge.Width = Math.Max(0, gaugeWidth);

            // Tendencias
            AplicarTendencia(tendencias["asistencias"], badgeTendAsist, iconTendAsist, txtTendAsist, true);
            AplicarTendencia(tendencias["tardanzas"],   badgeTendTard,  iconTendTard,  txtTendTard,  false);
            AplicarTendencia(tendencias["ausencias"],   badgeTendAus,   iconTendAus,   txtTendAus,   false);

            // Resumen semanal
            icSemana.ItemsSource = semana;

            // Heatmap
            ConfigurarHeatmap(heatmap, kpis.TotalEmpleadosActivos);

            // Gráficos
            ConfigurarGraficoLinea(diarias);
            ConfigurarGraficoDonut(kpis);
            ConfigurarGraficoBarras(tardanzas);
            ConfigurarGraficoPuntuales(puntuales);

            // Tabla reciente
            dgReciente.ItemsSource   = recientes;
            txtContadorReciente.Text = $"{recientes.Count} registros";

            // Notificar alertas al shell (evita query concurrente en el mismo DbContext)
            AlertasCalculadas?.Invoke(kpis.TardanzasHoy, kpis.AusenciasHoy);
        }

        // ── Tendencias con flechitas ────────────────────────────────────────────
        private void AplicarTendencia(KpiTendenciaDto tend, Border badge,
            MaterialDesignThemes.Wpf.PackIcon icon, TextBlock txt, bool subirEsBueno)
        {
            if (tend.Igual)
            {
                badge.Visibility = Visibility.Collapsed;
                return;
            }

            badge.Visibility = Visibility.Visible;
            bool esBueno = subirEsBueno ? tend.Subio : tend.Bajo;

            badge.Background = new SolidColorBrush(esBueno
                ? Color.FromRgb(0xE8, 0xF5, 0xE9)
                : Color.FromRgb(0xFF, 0xEB, 0xEE));

            icon.Kind = tend.Subio
                ? MaterialDesignThemes.Wpf.PackIconKind.TrendingUp
                : MaterialDesignThemes.Wpf.PackIconKind.TrendingDown;

            var color = esBueno
                ? new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))
                : new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));

            icon.Foreground = color;
            txt.Foreground  = color;
            txt.Text        = tend.DiferenciaTexto;
        }

        // ── Heatmap mensual ─────────────────────────────────────────────────────
        private void ConfigurarHeatmap(List<HeatmapDiaDto> datos, int totalEmpleados)
        {
            icHeatmap.Items.Clear();

            foreach (var dia in datos)
            {
                // Determinar color según proporción
                Color bgColor;
                string tooltip;

                if (dia.Asistencias == 0 && totalEmpleados > 0)
                {
                    bgColor = Color.FromRgb(0xEF, 0x9A, 0x9A); // Rojo claro — sin asistencias
                    tooltip = $"{dia.Fecha:dd/MM}: Sin asistencias";
                }
                else if (dia.Tardanzas > 0)
                {
                    double ratio = (double)dia.Tardanzas / Math.Max(1, dia.Asistencias);
                    if (ratio > 0.3)
                    {
                        bgColor = Color.FromRgb(0xFF, 0xE0, 0x82); // Amarillo fuerte
                        tooltip = $"{dia.Fecha:dd/MM}: {dia.Asistencias} asist, {dia.Tardanzas} tard";
                    }
                    else
                    {
                        bgColor = Color.FromRgb(0xFF, 0xF9, 0xC4); // Amarillo suave
                        tooltip = $"{dia.Fecha:dd/MM}: {dia.Asistencias} asist, {dia.Tardanzas} tard";
                    }
                }
                else
                {
                    double ratio = totalEmpleados > 0
                        ? (double)dia.Asistencias / totalEmpleados : 1;
                    bgColor = ratio > 0.8
                        ? Color.FromRgb(0xA5, 0xD6, 0xA7) // Verde fuerte
                        : Color.FromRgb(0xC8, 0xE6, 0xC9); // Verde suave
                    tooltip = $"{dia.Fecha:dd/MM}: {dia.Asistencias} asistencias";
                }

                bool esHoy = dia.Fecha.Date == DateTime.Today;
                bool esFinde = dia.Fecha.DayOfWeek == DayOfWeek.Saturday || dia.Fecha.DayOfWeek == DayOfWeek.Sunday;

                var border = new Border
                {
                    Width        = 32,
                    Height       = 32,
                    CornerRadius = new CornerRadius(4),
                    Margin       = new Thickness(2),
                    Background   = esFinde
                        ? new SolidColorBrush(Color.FromRgb(0xEC, 0xEF, 0xF1))
                        : new SolidColorBrush(bgColor),
                    BorderBrush     = esHoy ? new SolidColorBrush(Color.FromRgb(0x1A, 0x23, 0x7E)) : null,
                    BorderThickness = esHoy ? new Thickness(2) : new Thickness(0),
                    ToolTip         = esFinde ? $"{dia.Fecha:dd/MM}: Fin de semana" : tooltip,
                    Child = new TextBlock
                    {
                        Text                = dia.Fecha.Day.ToString(),
                        FontSize            = 10,
                        FontWeight          = esHoy ? FontWeights.Bold : FontWeights.Normal,
                        Foreground          = esFinde
                            ? new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5))
                            : new SolidColorBrush(Color.FromRgb(0x37, 0x47, 0x4F)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center
                    }
                };

                icHeatmap.Items.Add(border);
            }
        }

        // ── Gráfico de líneas ───────────────────────────────────────────────────
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

            chartLinea.XAxes = new[] { new Axis { Labels = labels, LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80)) } };
            chartLinea.YAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80)) } };
            chartLinea.LegendPosition  = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartLinea.LegendTextPaint = new SolidColorPaint(new SKColor(69, 90, 100));
        }

        // ── Gráfico donut ───────────────────────────────────────────────────────
        private void ConfigurarGraficoDonut(DashboardKpiDto kpis)
        {
            int presentes = Math.Max(0, kpis.AsistenciasHoy - kpis.TardanzasHoy);
            int tardanzas = kpis.TardanzasHoy;
            int ausencias = kpis.AusenciasHoy;

            if (presentes == 0 && tardanzas == 0 && ausencias == 0)
            {
                chartDonut.Series = new ISeries[] { new PieSeries<double> { Values = new double[] { 1 }, Name = "Sin datos", Fill = new SolidColorPaint(new SKColor(200, 200, 200)) } };
                return;
            }

            var series = new List<ISeries>();
            if (presentes > 0) series.Add(new PieSeries<double> { Values = new[] { (double)presentes }, Name = "A tiempo", Fill = new SolidColorPaint(new SKColor(46, 125, 50)), InnerRadius = 50 });
            if (tardanzas > 0) series.Add(new PieSeries<double> { Values = new[] { (double)tardanzas }, Name = "Tardanzas", Fill = new SolidColorPaint(new SKColor(230, 81, 0)), InnerRadius = 50 });
            if (ausencias > 0) series.Add(new PieSeries<double> { Values = new[] { (double)ausencias }, Name = "Ausencias", Fill = new SolidColorPaint(new SKColor(173, 20, 87)), InnerRadius = 50 });

            chartDonut.Series         = series;
            chartDonut.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartDonut.LegendTextPaint = new SolidColorPaint(new SKColor(69, 90, 100));
        }

        // ── Gráfico barras tardanzas ────────────────────────────────────────────
        private void ConfigurarGraficoBarras(List<TardanzaEmpleadoDto> datos)
        {
            if (!datos.Any()) { chartBarras.Series = Array.Empty<ISeries>(); return; }

            chartBarras.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = datos.Select(d => (double)d.TotalTardanzas).ToArray(),
                    Name   = "Tardanzas",
                    Fill   = new SolidColorPaint(new SKColor(230, 81, 0, 200)),
                    Stroke = new SolidColorPaint(new SKColor(230, 81, 0)) { StrokeThickness = 1 },
                    MaxBarWidth = 40, Rx = 4, Ry = 4
                }
            };
            chartBarras.XAxes = new[] { new Axis { Labels = datos.Select(d => d.NombreEmpleado).ToArray(), LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.Transparent) } };
            chartBarras.YAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80)) } };
            chartBarras.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        }

        // ── Gráfico barras puntuales ────────────────────────────────────────────
        private void ConfigurarGraficoPuntuales(List<TardanzaEmpleadoDto> datos)
        {
            if (!datos.Any()) { chartPuntuales.Series = Array.Empty<ISeries>(); return; }

            chartPuntuales.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = datos.Select(d => (double)d.TotalTardanzas).ToArray(), // Reused: count of on-time entries
                    Name   = "A tiempo",
                    Fill   = new SolidColorPaint(new SKColor(46, 125, 50, 200)),
                    Stroke = new SolidColorPaint(new SKColor(46, 125, 50)) { StrokeThickness = 1 },
                    MaxBarWidth = 40, Rx = 4, Ry = 4
                }
            };
            chartPuntuales.XAxes = new[] { new Axis { Labels = datos.Select(d => d.NombreEmpleado).ToArray(), LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.Transparent) } };
            chartPuntuales.YAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(new SKColor(69, 90, 100)), TextSize = 10, SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200, 80)) } };
            chartPuntuales.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
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
