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

        public event Action<int, int> AlertasCalculadas;

        // ── Colores de gráficos (se resuelven en cada carga según el tema activo) ──
        private SKColor PastelBlue, PastelGreen, PastelOrange, PastelPink,
                        PastelPurple, PastelTeal, TextMuted, GridLine, CardBg;

        public DashboardView(DashboardController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        private SKColor ThemeColor(string resourceKey, byte r, byte g, byte b)
        {
            if (TryFindResource(resourceKey) is SolidColorBrush brush)
            {
                var c = brush.Color;
                return new SKColor(c.R, c.G, c.B, c.A);
            }
            return new SKColor(r, g, b);
        }

        private void ResolveThemeColors()
        {
            PastelBlue   = ThemeColor("AccentBlue",    126, 184, 218);
            PastelGreen  = ThemeColor("PastelGreen",   129, 201, 149);
            PastelOrange = ThemeColor("PastelOrange",  244, 169, 125);
            PastelPink   = ThemeColor("PastelPink",    232, 135, 155);
            PastelPurple = ThemeColor("PastelPurple",  184, 169, 212);
            PastelTeal   = ThemeColor("PastelTeal",    126, 200, 184);
            TextMuted    = ThemeColor("TextSubtle",    107, 141, 166);
            GridLine     = ThemeColor("BorderSubtle",   42,  55,  70);
            CardBg       = ThemeColor("CardBg",         26,  35,  50);
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            ResolveThemeColors();
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
            ResolveThemeColors();
            try { await CargarDatosAsync(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task CargarDatosAsync()
        {
            txtTotalEmpleados.Text = "…";
            txtAsistenciasHoy.Text = "…";
            txtTardanzasHoy.Text   = "…";
            txtAusenciasHoy.Text   = "…";
            txtTasaAsistencia.Text = "…";
            txtMarcajes7Dias.Text  = "…";

            var kpis       = await _ctrl.ObtenerKpisAsync();
            var tendencias = await _ctrl.ObtenerTendenciasAsync();
            var diarias    = await _ctrl.ObtenerAsistenciasDiariasAsync(14);
            var tardanzas  = await _ctrl.ObtenerTopTardanzasAsync(5, 30);
            var puntuales  = await _ctrl.ObtenerTopPuntualesAsync(5, 30);
            var recientes  = await _ctrl.ObtenerUltimosMarcajesAsync(10);
            var semana     = await _ctrl.ObtenerResumenSemanalAsync();
            var heatmap    = await _ctrl.ObtenerHeatmapMensualAsync();

            txtTotalEmpleados.Text = kpis.TotalEmpleadosActivos.ToString();
            txtAsistenciasHoy.Text = kpis.AsistenciasHoy.ToString();
            txtTardanzasHoy.Text   = kpis.TardanzasHoy.ToString();
            txtAusenciasHoy.Text   = kpis.AusenciasHoy.ToString();
            txtTasaAsistencia.Text = $"{kpis.TasaAsistenciaHoy}%";
            txtMarcajes7Dias.Text  = kpis.MarcajesUltimos7Dias.ToString();

            double gaugeWidth = barGauge.Parent is Grid gaugeParent
                ? gaugeParent.ActualWidth * (kpis.TasaAsistenciaHoy / 100.0) : 0;
            barGauge.Width = Math.Max(0, gaugeWidth);

            AplicarTendencia(tendencias["asistencias"], badgeTendAsist, iconTendAsist, txtTendAsist, true);
            AplicarTendencia(tendencias["tardanzas"],   badgeTendTard,  iconTendTard,  txtTendTard,  false);
            AplicarTendencia(tendencias["ausencias"],   badgeTendAus,   iconTendAus,   txtTendAus,   false);

            icSemana.ItemsSource = semana;
            ConfigurarHeatmap(heatmap, kpis.TotalEmpleadosActivos);
            ConfigurarGraficoLinea(diarias);
            ConfigurarGraficoDonut(kpis);
            ConfigurarGraficoBarras(tardanzas);
            ConfigurarGraficoPuntuales(puntuales);

            dgReciente.ItemsSource   = recientes;
            txtContadorReciente.Text = $"{recientes.Count} registros";

            AlertasCalculadas?.Invoke(kpis.TardanzasHoy, kpis.AusenciasHoy);
        }

        // ── AutoGeneratingColumn ─────────────────────────────────────────────────
        private void DgReciente_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NombreEmpleado": e.Column.Header = "Empleado";     e.Column.Width = new DataGridLength(140); break;
                case "FechaHora":     e.Column.Header = "Fecha / Hora"; e.Column.Width = new DataGridLength(150); break;
                case "Tipo":          e.Column.Width = new DataGridLength(120); break;
                case "EsTardanza":    e.Column.Header = "Estado";       e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star); break;
            }
        }

        // ── Tendencias ────────────────────────────────────────────────────────────
        private void AplicarTendencia(KpiTendenciaDto tend, Border badge,
            MaterialDesignThemes.Wpf.PackIcon icon, TextBlock txt, bool subirEsBueno)
        {
            if (tend.Igual) { badge.Visibility = Visibility.Collapsed; return; }

            badge.Visibility = Visibility.Visible;
            bool esBueno = subirEsBueno ? tend.Subio : tend.Bajo;

            var goodBg = WpfColor("BadgeGreenBg", 0x1A, 0x30, 0x25);
            var badBg  = WpfColor("BadgeRedBg",   0x2D, 0x1A, 0x22);
            badge.Background = new SolidColorBrush(esBueno ? goodBg : badBg);

            icon.Kind = tend.Subio
                ? MaterialDesignThemes.Wpf.PackIconKind.TrendingUp
                : MaterialDesignThemes.Wpf.PackIconKind.TrendingDown;

            var goodFg = WpfColor("PastelGreen", 0x81, 0xC9, 0x95);
            var badFg  = WpfColor("PastelPink",  0xE8, 0x87, 0x9B);
            var color = new SolidColorBrush(esBueno ? goodFg : badFg);

            icon.Foreground = color;
            txt.Foreground  = color;
            txt.Text        = tend.DiferenciaTexto;
        }

        // ── Heatmap — theme-aware ────────────────────────────────────────────────
        private Color WpfColor(string key, byte r, byte g, byte b)
        {
            if (TryFindResource(key) is SolidColorBrush brush) return brush.Color;
            return Color.FromRgb(r, g, b);
        }

        private void ConfigurarHeatmap(List<HeatmapDiaDto> datos, int totalEmpleados)
        {
            icHeatmap.Items.Clear();

            // Resolve colors from current theme
            var badgeRedBg    = WpfColor("BadgeRedBg",    0x3D, 0x1F, 0x28);
            var badgeOrangeBg = WpfColor("BadgeOrangeBg", 0x2D, 0x25, 0x18);
            var badgeGreenBg  = WpfColor("BadgeGreenBg",  0x1A, 0x35, 0x25);
            var cardAltBg     = WpfColor("CardAltBg",     0x1E, 0x27, 0x38);
            var accentBlue    = WpfColor("AccentBlue",    0x7E, 0xB8, 0xDA);
            var textPrimary   = WpfColor("TextPrimary",   0xE8, 0xEE, 0xF2);
            var textDim       = WpfColor("TextDim",       0x3A, 0x4A, 0x58);

            foreach (var dia in datos)
            {
                Color bgColor;
                string tooltip;

                if (dia.Asistencias == 0 && totalEmpleados > 0)
                {
                    bgColor = badgeRedBg;
                    tooltip = $"{dia.Fecha:dd/MM}: Sin asistencias";
                }
                else if (dia.Tardanzas > 0)
                {
                    bgColor = badgeOrangeBg;
                    tooltip = $"{dia.Fecha:dd/MM}: {dia.Asistencias} asist, {dia.Tardanzas} tard";
                }
                else
                {
                    bgColor = badgeGreenBg;
                    tooltip = $"{dia.Fecha:dd/MM}: {dia.Asistencias} asistencias";
                }

                bool esHoy   = dia.Fecha.Date == DateTime.Today;
                bool esFinde = dia.Fecha.DayOfWeek == DayOfWeek.Saturday || dia.Fecha.DayOfWeek == DayOfWeek.Sunday;

                var border = new Border
                {
                    Width        = 32,
                    Height       = 32,
                    CornerRadius = new CornerRadius(6),
                    Margin       = new Thickness(2),
                    Background   = esFinde
                        ? new SolidColorBrush(cardAltBg)
                        : new SolidColorBrush(bgColor),
                    BorderBrush     = esHoy ? new SolidColorBrush(accentBlue) : null,
                    BorderThickness = esHoy ? new Thickness(2) : new Thickness(0),
                    ToolTip         = esFinde ? $"{dia.Fecha:dd/MM}: Fin de semana" : tooltip,
                    Child = new TextBlock
                    {
                        Text                = dia.Fecha.Day.ToString(),
                        FontSize            = 10,
                        FontWeight          = esHoy ? FontWeights.Bold : FontWeights.Normal,
                        Foreground          = esFinde
                            ? new SolidColorBrush(textDim)
                            : new SolidColorBrush(textPrimary),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center
                    }
                };
                icHeatmap.Items.Add(border);
            }
        }

        // ── Gráfico líneas — pastel ───────────────────────────────────────────────
        private void ConfigurarGraficoLinea(List<AsistenciaDiariaDto> datos)
        {
            var asistencias = datos.Select(d => (double)d.Total).ToArray();
            var tardanzas   = datos.Select(d => (double)d.Tardanzas).ToArray();
            var labels      = datos.Select(d => d.Fecha.ToString("dd/MM")).ToArray();

            chartLinea.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values         = asistencias,
                    Name           = "Asistencias",
                    Stroke         = new SolidColorPaint(PastelBlue) { StrokeThickness = 2.5f },
                    Fill           = new SolidColorPaint(PastelBlue.WithAlpha(25)),
                    GeometryFill   = new SolidColorPaint(PastelBlue),
                    GeometryStroke = new SolidColorPaint(CardBg) { StrokeThickness = 2 },
                    GeometrySize   = 8,
                    LineSmoothness = 0.5
                },
                new LineSeries<double>
                {
                    Values         = tardanzas,
                    Name           = "Tardanzas",
                    Stroke         = new SolidColorPaint(PastelOrange) { StrokeThickness = 2f },
                    Fill           = new SolidColorPaint(PastelOrange.WithAlpha(18)),
                    GeometryFill   = new SolidColorPaint(PastelOrange),
                    GeometryStroke = new SolidColorPaint(CardBg) { StrokeThickness = 2 },
                    GeometrySize   = 7,
                    LineSmoothness = 0.5
                }
            };

            var axisPaint     = new SolidColorPaint(TextMuted);
            var separatorPaint = new SolidColorPaint(GridLine);

            chartLinea.XAxes = new[] { new Axis { Labels = labels, LabelsPaint = axisPaint, TextSize = 10, SeparatorsPaint = separatorPaint } };
            chartLinea.YAxes = new[] { new Axis { LabelsPaint = axisPaint, TextSize = 10, SeparatorsPaint = separatorPaint } };
            chartLinea.LegendPosition  = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartLinea.LegendTextPaint = new SolidColorPaint(TextMuted);
        }

        // ── Donut — pastel ────────────────────────────────────────────────────────
        private void ConfigurarGraficoDonut(DashboardKpiDto kpis)
        {
            int presentes = Math.Max(0, kpis.AsistenciasHoy - kpis.TardanzasHoy);
            int tardanzas = kpis.TardanzasHoy;
            int ausencias = kpis.AusenciasHoy;

            if (presentes == 0 && tardanzas == 0 && ausencias == 0)
            {
                chartDonut.Series = new ISeries[] { new PieSeries<double> { Values = new double[] { 1 }, Name = "Sin datos", Fill = new SolidColorPaint(GridLine) } };
                return;
            }

            var series = new List<ISeries>();
            if (presentes > 0) series.Add(new PieSeries<double> { Values = new[] { (double)presentes }, Name = "A tiempo",  Fill = new SolidColorPaint(PastelGreen),  InnerRadius = 50 });
            if (tardanzas > 0) series.Add(new PieSeries<double> { Values = new[] { (double)tardanzas }, Name = "Tardanzas", Fill = new SolidColorPaint(PastelOrange), InnerRadius = 50 });
            if (ausencias > 0) series.Add(new PieSeries<double> { Values = new[] { (double)ausencias }, Name = "Ausencias", Fill = new SolidColorPaint(PastelPink),   InnerRadius = 50 });

            chartDonut.Series         = series;
            chartDonut.LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom;
            chartDonut.LegendTextPaint = new SolidColorPaint(TextMuted);
        }

        // ── Barras tardanzas — pastel naranja ─────────────────────────────────────
        private void ConfigurarGraficoBarras(List<TardanzaEmpleadoDto> datos)
        {
            if (!datos.Any()) { chartBarras.Series = Array.Empty<ISeries>(); return; }

            chartBarras.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values      = datos.Select(d => (double)d.TotalTardanzas).ToArray(),
                    Name        = "Tardanzas",
                    Fill        = new SolidColorPaint(PastelOrange.WithAlpha(180)),
                    Stroke      = new SolidColorPaint(PastelOrange) { StrokeThickness = 1 },
                    MaxBarWidth = 40, Rx = 4, Ry = 4
                }
            };
            chartBarras.XAxes = new[] { new Axis { Labels = datos.Select(d => d.NombreEmpleado).ToArray(), LabelsPaint = new SolidColorPaint(TextMuted), TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.Transparent) } };
            chartBarras.YAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(TextMuted), TextSize = 10, SeparatorsPaint = new SolidColorPaint(GridLine) } };
            chartBarras.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        }

        // ── Barras puntuales — pastel verde ───────────────────────────────────────
        private void ConfigurarGraficoPuntuales(List<TardanzaEmpleadoDto> datos)
        {
            if (!datos.Any()) { chartPuntuales.Series = Array.Empty<ISeries>(); return; }

            chartPuntuales.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values      = datos.Select(d => (double)d.TotalTardanzas).ToArray(),
                    Name        = "A tiempo",
                    Fill        = new SolidColorPaint(PastelGreen.WithAlpha(180)),
                    Stroke      = new SolidColorPaint(PastelGreen) { StrokeThickness = 1 },
                    MaxBarWidth = 40, Rx = 4, Ry = 4
                }
            };
            chartPuntuales.XAxes = new[] { new Axis { Labels = datos.Select(d => d.NombreEmpleado).ToArray(), LabelsPaint = new SolidColorPaint(TextMuted), TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.Transparent) } };
            chartPuntuales.YAxes = new[] { new Axis { LabelsPaint = new SolidColorPaint(TextMuted), TextSize = 10, SeparatorsPaint = new SolidColorPaint(GridLine) } };
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
