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
        // Sub-vistas: creadas aquí (no vía DI), son internas al shell.
        // Cada una recibe su controlador ya resuelto por DI.
        private readonly DashboardView     _dashboardView;
        private readonly UsuariosView      _usuariosView;
        private readonly MarcajesAdminView _marcajesView;
        private readonly ReportesView      _reportesView;

        private readonly AuthController  _authCtrl;
        private readonly ISessionManager _session;

        // Referencia al botón activo para aplicar/quitar highlight
        private Button _activeBtn;

        // Colores del estado activo en el sidebar
        private static readonly Brush ActiveBg   = new SolidColorBrush(Color.FromRgb(0x27, 0x5E, 0x87));
        private static readonly Brush ActiveFg   = Brushes.White;
        private static readonly Brush InactiveFg = new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6));

        public AdminShellView(
            DashboardController     dashboardCtrl,
            UsuariosController      usuariosCtrl,
            MarcajesAdminController marcajesCtrl,
            ReportesController      reportesCtrl,
            AuthController          authCtrl,
            ISessionManager         session)
        {
            InitializeComponent();

            _dashboardView = new DashboardView(dashboardCtrl);
            _usuariosView  = new UsuariosView(usuariosCtrl);
            _marcajesView  = new MarcajesAdminView(marcajesCtrl);
            _reportesView  = new ReportesView(reportesCtrl);

            _authCtrl = authCtrl;
            _session  = session;
        }

        // ── Inicialización ────────────────────────────────────────────────────────
        private void AdminShell_Loaded(object sender, RoutedEventArgs e)
        {
            var si = _session.GetCurrentSession();
            if (si != null) txtNombreAdmin.Text = si.Username;

            // Navegar al dashboard por defecto
            ActivarSeccion(btnNavDashboard, _dashboardView);
        }

        // ── Routing de navegación ─────────────────────────────────────────────────
        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            UserControl view = btn.Tag?.ToString() switch
            {
                "dashboard" => _dashboardView,
                "usuarios"  => _usuariosView,
                "marcajes"  => _marcajesView,
                "reportes"  => _reportesView,
                _           => _dashboardView
            };
            ActivarSeccion(btn, view);
        }

        // ── Swap de contenido + highlight del botón activo ────────────────────────
        private void ActivarSeccion(Button btn, UserControl view)
        {
            // Quitar highlight del botón anterior
            if (_activeBtn != null)
            {
                _activeBtn.Background = Brushes.Transparent;
                _activeBtn.Foreground = InactiveFg;
            }

            // Aplicar highlight al nuevo botón activo
            btn.Background = ActiveBg;
            btn.Foreground = ActiveFg;
            _activeBtn = btn;

            ShellContent.Content = view;
        }

        // ── Logout ────────────────────────────────────────────────────────────────
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
            => _authCtrl.Logout();
    }
}
