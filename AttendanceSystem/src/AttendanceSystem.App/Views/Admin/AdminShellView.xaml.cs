using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.Interfaces;

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

        private static readonly Brush ActiveBg   = new SolidColorBrush(Color.FromRgb(0x27, 0x5E, 0x87));
        private static readonly Brush ActiveFg   = Brushes.White;
        private static readonly Brush InactiveFg = new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6));

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

            // Suscribirse a las alertas que el dashboard calcula después de cargar sus KPIs
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

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
            => _authCtrl.Logout();
    }
}
