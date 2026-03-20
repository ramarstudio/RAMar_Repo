using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.Interfaces;
using MaterialDesignThemes.Wpf;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class AdminShellView : UserControl
    {
        private readonly DashboardView       _dashboardView;
        private readonly UsuariosView        _usuariosView;
        private readonly MarcajesAdminView   _marcajesView;
        private readonly ReportesView        _reportesView;
        private readonly AuditoriaView       _auditoriaView;
        private readonly HorariosView        _horariosView;
        private readonly ConfiguracionView   _configView;

        private readonly AuthController      _authCtrl;
        private readonly ISessionManager     _session;

        private Button _activeBtn;

        private Brush ActiveBg   => (Brush)FindResource("NavActiveBg");
        private Brush ActiveFg   => Brushes.White;
        private Brush InactiveFg => (Brush)FindResource("NavInactiveFg");

        // ── Zoom ────────────────────────────────────────────────────────────────
        private const double ZoomMin  = 0.5;
        private const double ZoomMax  = 2.0;
        private const double ZoomStep = 0.1;
        private double _currentZoom   = 1.0;

        // ── Theme ───────────────────────────────────────────────────────────────
        private static readonly Uri DarkThemeUri  = new("Resources/Styles/DarkTheme.xaml", UriKind.Relative);
        private static readonly Uri LightThemeUri = new("Resources/Styles/LightTheme.xaml", UriKind.Relative);
        private bool _isDarkTheme = true;

        public AdminShellView(
            DashboardController      dashboardCtrl,
            UsuariosController       usuariosCtrl,
            MarcajesAdminController  marcajesCtrl,
            ReportesController       reportesCtrl,
            AuditoriaController      auditoriaCtrl,
            HorariosController       horariosCtrl,
            ConfiguracionController  configCtrl,
            AuthController           authCtrl,
            ISessionManager          session)
        {
            InitializeComponent();

            _dashboardView = new DashboardView(dashboardCtrl);
            _usuariosView  = new UsuariosView(usuariosCtrl);
            _marcajesView  = new MarcajesAdminView(marcajesCtrl);
            _reportesView  = new ReportesView(reportesCtrl);
            _auditoriaView = new AuditoriaView(auditoriaCtrl);
            _horariosView  = new HorariosView(horariosCtrl);
            _configView    = new ConfiguracionView(configCtrl);

            _authCtrl = authCtrl;
            _session  = session;

            _dashboardView.AlertasCalculadas += OnAlertasCalculadas;
        }

        private void AdminShell_Loaded(object sender, RoutedEventArgs e)
        {
            var si = _session.GetCurrentSession();
            if (si != null) txtNombreAdmin.Text = si.Username;

            if (_session.EsRRHH())
            {
                btnNavUsuarios.Visibility = Visibility.Collapsed;
                btnNavConfig.Visibility   = Visibility.Collapsed;
            }

            ActivarSeccion(btnNavDashboard, _dashboardView);
        }

        private void OnAlertasCalculadas(int tardanzas, int ausencias)
        {
            if (tardanzas > 0 || ausencias > 0)
            {
                pnlAlertas.Visibility  = Visibility.Visible;
                txtAlertaTardanzas.Text = $"{tardanzas} tardanza{(tardanzas != 1 ? "s" : "")}";
                txtAlertaAusencias.Text = $"{ausencias} ausencia{(ausencias != 1 ? "s" : "")}";
            }
            else
            {
                pnlAlertas.Visibility = Visibility.Collapsed;
            }
        }

        // ── Navigation ──────────────────────────────────────────────────────────
        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            UserControl view = btn.Tag?.ToString() switch
            {
                "dashboard"     => _dashboardView,
                "usuarios"      => _usuariosView,
                "marcajes"      => _marcajesView,
                "horarios"      => _horariosView,
                "reportes"      => _reportesView,
                "auditoria"     => _auditoriaView,
                "configuracion" => _configView,
                _               => _dashboardView
            };
            ActivarSeccion(btn, view);
        }

        private void ActivarSeccion(Button btn, UserControl view)
        {
            if (_activeBtn != null)
            {
                _activeBtn.Background = Brushes.Transparent;
                _activeBtn.Foreground = InactiveFg;
            }

            btn.Background = ActiveBg;
            btn.Foreground = ActiveFg;
            _activeBtn = btn;

            ShellContent.Content = view;
        }

        // ── Zoom (global — applies to all views) ────────────────────────────────
        private void ContentScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            e.Handled = true;
            ApplyZoom(_currentZoom + (e.Delta > 0 ? ZoomStep : -ZoomStep));
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
            => ApplyZoom(_currentZoom + ZoomStep);

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
            => ApplyZoom(_currentZoom - ZoomStep);

        private void BtnZoomReset_Click(object sender, RoutedEventArgs e)
            => ApplyZoom(1.0);

        private void ApplyZoom(double zoom)
        {
            _currentZoom = Math.Clamp(zoom, ZoomMin, ZoomMax);
            ContentScale.ScaleX = _currentZoom;
            ContentScale.ScaleY = _currentZoom;
            txtZoomLevel.Text = $"{(int)(_currentZoom * 100)}%";
        }

        // ── Theme toggle ────────────────────────────────────────────────────────
        private void TglTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = tglTheme.IsChecked == true;

            // 1. Swap MaterialDesign base theme
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            // 2. Swap custom resource dictionary
            var appResources = Application.Current.Resources.MergedDictionaries;
            var oldTheme = appResources.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("DarkTheme") ||
                                     d.Source.OriginalString.Contains("LightTheme")));

            if (oldTheme != null)
                appResources.Remove(oldTheme);

            appResources.Add(new ResourceDictionary
            {
                Source = _isDarkTheme ? DarkThemeUri : LightThemeUri
            });

            // 3. Update toggle UI
            icoTheme.Kind = _isDarkTheme ? PackIconKind.WeatherNight : PackIconKind.WhiteBalanceSunny;
            txtThemeLabel.Text = _isDarkTheme ? "Modo Oscuro" : "Modo Claro";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
            => _authCtrl.Logout();
    }
}
