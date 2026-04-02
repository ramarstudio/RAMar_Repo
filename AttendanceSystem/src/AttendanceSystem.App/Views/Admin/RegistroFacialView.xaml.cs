using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class RegistroFacialView : UserControl
    {
        private readonly RegistroFacialController _controller;
        private EmpleadoBiometricoDto _seleccionado;
        private bool _camaraActiva;

        private static readonly SolidColorBrush BrushVerde  = new(Color.FromRgb(0x4C, 0xAF, 0x50));
        private static readonly SolidColorBrush BrushRojo   = new(Color.FromRgb(0xE5, 0x39, 0x35));

        static RegistroFacialView()
        {
            BrushVerde.Freeze();
            BrushRojo.Freeze();
        }

        public RegistroFacialView(RegistroFacialController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        // ── Lifecycle ────────────────────────────────────────────────────────
        private async void View_Loaded(object sender, RoutedEventArgs e)
        {
            try { await CargarEmpleadosAsync(); }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
        }

        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_camaraActiva)
            {
                _controller.ApagarCamara(OnFrameArrived);
                _camaraActiva = false;
            }
        }

        private async System.Threading.Tasks.Task CargarEmpleadosAsync()
        {
            var empleados = await _controller.ObtenerEmpleadosAsync();
            icEmpleados.ItemsSource = empleados;
        }

        // ── Seleccionar empleado ─────────────────────────────────────────────
        private async void BtnSeleccionarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not EmpleadoBiometricoDto emp) return;

            _seleccionado = emp;
            txtEmpleadoNombre.Text = emp.Nombre;
            txtEmpleadoCodigo.Text = $"Código: {emp.Codigo}";
            pnlEmpleadoInfo.Visibility = Visibility.Visible;

            // Estado badge
            if (emp.TieneEmbedding)
            {
                badgeEstado.Background = new SolidColorBrush(Color.FromArgb(0x30, 0x4C, 0xAF, 0x50));
                txtEstado.Text = "Registrado";
                txtEstado.Foreground = BrushVerde;
                btnCapturar.Visibility = Visibility.Collapsed;
                btnReemplazar.Visibility = Visibility.Visible;
            }
            else
            {
                badgeEstado.Background = new SolidColorBrush(Color.FromArgb(0x30, 0xE5, 0x39, 0x35));
                txtEstado.Text = "Sin registro";
                txtEstado.Foreground = BrushRojo;
                btnCapturar.Visibility = Visibility.Visible;
                btnReemplazar.Visibility = Visibility.Collapsed;
            }

            // Consentimiento
            pnlConsentimiento.Visibility = emp.TieneConsentimiento
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Habilitar captura solo si tiene consentimiento
            pnlCaptura.IsEnabled = emp.TieneConsentimiento;
            pnlAcciones.Visibility = Visibility.Visible;

            // Iniciar cámara
            await IniciarCamaraAsync();
        }

        // ── Cámara ───────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task IniciarCamaraAsync()
        {
            if (_camaraActiva) return;

            try
            {
                await _controller.IniciarCamaraAsync(OnFrameArrived);
                _camaraActiva = true;
                pnlPlaceholder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                txtPlaceholder.Text = $"Error de cámara: {ex.Message}";
            }
        }

        private void OnFrameArrived(object sender, BitmapSource frame)
        {
            Dispatcher.BeginInvoke(() => imgCamara.Source = frame);
        }

        // ── Consentimiento ───────────────────────────────────────────────────
        private async void BtnConsentimiento_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null) return;

            btnConsentimiento.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.OtorgarConsentimientoAsync(_seleccionado.Id);
                MostrarMensaje(msg, !ok);

                if (ok)
                {
                    _seleccionado.TieneConsentimiento = true;
                    pnlConsentimiento.Visibility = Visibility.Collapsed;
                    pnlCaptura.IsEnabled = true;
                }
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnConsentimiento.IsEnabled = true; }
        }

        // ── Captura / Registro facial ────────────────────────────────────────
        private async void BtnCapturar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null) return;

            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                var (ok, msg) = await _controller.RegistrarRostroAsync(_seleccionado.Codigo);
                MostrarMensaje(msg, !ok);

                if (ok)
                {
                    _seleccionado.TieneEmbedding = true;
                    badgeEstado.Background = new SolidColorBrush(Color.FromArgb(0x30, 0x4C, 0xAF, 0x50));
                    txtEstado.Text = "Registrado";
                    txtEstado.Foreground = BrushVerde;
                    btnCapturar.Visibility = Visibility.Collapsed;
                    btnReemplazar.Visibility = Visibility.Visible;

                    await CargarEmpleadosAsync();
                }
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        // ── Feedback ─────────────────────────────────────────────────────────
        private void MostrarMensaje(string msg, bool error)
        {
            txtMensaje.Text = msg;
            txtMensaje.Foreground = error ? BrushRojo : BrushVerde;
            txtMensaje.Visibility = Visibility.Visible;
        }
    }
}
