using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.App.Interfaces;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.App.Views
{
    public partial class MarcajeView : UserControl
    {
        private readonly MarcajeController      _marcajeController;
        private readonly IBiometricoController  _biometricoController;
        private readonly AuthController         _authController;
        private readonly DispatcherTimer        _keepAliveTimer;

        // Brushes estáticos y congelados: creados una sola vez, reutilizados en cada evento.
        // Freeze() permite usarlos desde cualquier hilo sin InvalidOperationException.
        private static readonly SolidColorBrush BrushVerde      = FreezeBrush(39,  174, 96);
        private static readonly SolidColorBrush BrushRojo       = FreezeBrush(231, 76,  60);
        private static readonly SolidColorBrush BrushFondoVerde = FreezeBrush(213, 245, 227);
        private static readonly SolidColorBrush BrushFondoRojo  = FreezeBrush(253, 213, 210);

        private static SolidColorBrush FreezeBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        public MarcajeView(MarcajeController marcajeController,
                           IBiometricoController biometricoController,
                           AuthController authController)
        {
            InitializeComponent();
            _marcajeController    = marcajeController;
            _biometricoController = biometricoController;
            _authController       = authController;

            // Timer que resetea el idle-timeout del FaceService cada 3 minutos
            // mientras la vista de marcaje está abierta.
            _keepAliveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(3) };
            _keepAliveTimer.Tick += (_, _) => _marcajeController.MantenervVivo();
        }

        // ── Carga: encender cámara + pre-calentar FaceService ─────────────────
        private async void MarcajeView_Loaded(object sender, RoutedEventArgs e)
        {
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM - HH:mm",
                new System.Globalization.CultureInfo("es-ES"));

            // Pre-calentar FaceService en background mientras la cámara inicia.
            // Así el modelo ya está cargado cuando el usuario presione el botón.
            _ = _marcajeController.PrecalentarAsync();
            _keepAliveTimer.Start();

            try
            {
                await _biometricoController.IniciarCamaraAsync(OnFrameArrived);
                txtEstadoCamara.Text       = "Cámara activa. Posiciónese frente a la cámara.";
                txtEstadoCamara.Foreground = BrushVerde;
            }
            catch (Exception ex)
            {
                txtEstadoCamara.Text       = $"Sin cámara: {ex.Message}";
                txtEstadoCamara.Foreground = BrushRojo;
            }
        }

        // ── Descarga: apagar cámara + detener keep-alive ───────────────────────
        private void MarcajeView_Unloaded(object sender, RoutedEventArgs e)
        {
            _keepAliveTimer.Stop();
            _biometricoController.ApagarCamara(OnFrameArrived);
        }

        // ── Callback del video ─────────────────────────────────────────────────
        private void OnFrameArrived(object sender, BitmapSource frame)
        {
            Dispatcher.Invoke(() => imgCamara.Source = frame);
        }

        // ── Botones ────────────────────────────────────────────────────────────
        private async void BtnEntrada_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.Entrada);

        private async void BtnSalida_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.Salida);

        private async void BtnBreak_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.BreakInicio);

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            _authController.Logout();
        }

        // ── Lógica compartida ──────────────────────────────────────────────────
        private async System.Threading.Tasks.Task ProcesarMarcaje(TipoMarcaje tipo)
        {
            SetBotonesHabilitados(false);
            OcultarResultado();

            // Si FaceService aún está iniciando (primera vez o tras idle-timeout),
            // avisarle al usuario en lugar de mostrar solo "procesando".
            txtEstadoCamara.Text = _marcajeController.FaceServiceActivo
                ? "Procesando marcaje..."
                : "Iniciando motor de reconocimiento facial, espere...";

            MarcajeResponse resultado = await _marcajeController.RegistrarMarcajeAsync(tipo);
            MostrarResultado(resultado);

            SetBotonesHabilitados(true);
            txtEstadoCamara.Text = resultado.Exito
                ? "Listo. Puede realizar otro marcaje."
                : "Listo.";
        }

        private void MostrarResultado(MarcajeResponse resultado)
        {
            panelResultado.Visibility = Visibility.Visible;

            if (resultado.Exito)
            {
                panelResultado.Background      = BrushFondoVerde;
                txtResultadoIcono.Text         = "✔";
                txtResultadoIcono.Foreground   = BrushVerde;
                txtResultadoMensaje.Text       = resultado.Mensaje;
                txtResultadoMensaje.Foreground = BrushVerde;

                if (resultado.EsTardanza)
                {
                    txtResultadoTardanza.Text       = $"⚠ {resultado.MinutosTardanza} minuto(s) de tardanza";
                    txtResultadoTardanza.Visibility = Visibility.Visible;
                }
                else
                {
                    txtResultadoTardanza.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                panelResultado.Background      = BrushFondoRojo;
                txtResultadoIcono.Text         = "✘";
                txtResultadoIcono.Foreground   = BrushRojo;
                txtResultadoMensaje.Text       = resultado.Mensaje;
                txtResultadoMensaje.Foreground = BrushRojo;
                txtResultadoTardanza.Visibility = Visibility.Collapsed;
            }
        }

        private void OcultarResultado()
            => panelResultado.Visibility = Visibility.Collapsed;

        private void SetBotonesHabilitados(bool habilitado)
        {
            btnEntrada.IsEnabled = habilitado;
            btnSalida.IsEnabled  = habilitado;
            btnBreak.IsEnabled   = habilitado;
        }
    }
}
