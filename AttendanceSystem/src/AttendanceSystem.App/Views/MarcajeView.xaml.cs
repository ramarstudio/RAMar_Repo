using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.App.Views
{
    public partial class MarcajeView : UserControl
    {
        private readonly MarcajeController _marcajeController;
        private readonly BiometricoController _biometricoController;

        public MarcajeView(MarcajeController marcajeController, BiometricoController biometricoController)
        {
            InitializeComponent();
            _marcajeController = marcajeController;
            _biometricoController = biometricoController;
        }

        // ── Cuando la vista carga: encender cámara y actualizar fecha/hora ──
        private void MarcajeView_Loaded(object sender, RoutedEventArgs e)
        {
            // Actualizar el reloj en pantalla
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM - HH:mm",
                new System.Globalization.CultureInfo("es-ES"));

            // Iniciar la cámara, pasando el método que actualizará el Image de XAML
            try
            {
                _biometricoController.IniciarCamara(OnFrameArrived);
                txtEstadoCamara.Text = "Cámara activa. Posiciónese frente a la cámara.";
                txtEstadoCamara.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            }
            catch (Exception ex)
            {
                txtEstadoCamara.Text = $"Sin cámara: {ex.Message}";
                txtEstadoCamara.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            }
        }

        // ── Cuando la vista se cierra: apagar cámara ──
        private void MarcajeView_Unloaded(object sender, RoutedEventArgs e)
        {
            _biometricoController.ApagarCamara(OnFrameArrived);
        }

        // ── Callback del video: actualiza el Image de XAML con cada frame ──
        private void OnFrameArrived(object sender, BitmapImage frame)
        {
            // Dispatcher garantiza que actualizamos la UI desde el hilo correcto
            Dispatcher.Invoke(() => imgCamara.Source = frame);
        }

        // ─── EVENTOS DE BOTONES ─────────────────────────────────────────────
        // Cada botón solo llama al controlador y muestra el resultado.

        private async void BtnEntrada_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.Entrada);

        private async void BtnSalida_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.Salida);

        private async void BtnBreak_Click(object sender, RoutedEventArgs e)
            => await ProcesarMarcaje(TipoMarcaje.Break);

        // ─── LÓGICA COMPARTIDA DE PROCESAMIENTO ────────────────────────────
        private async System.Threading.Tasks.Task ProcesarMarcaje(TipoMarcaje tipo)
        {
            // Deshabilitar todos los botones mientras se procesa
            SetBotonesHabilitados(false);
            OcultarResultado();
            txtEstadoCamara.Text = "Procesando marcaje...";

            // Delegar al controlador
            MarcajeResponse resultado = await _marcajeController.RegistrarMarcajeAsync(tipo);

            // Mostrar el resultado visualmente
            MostrarResultado(resultado);

            // Re-habilitar botones
            SetBotonesHabilitados(true);
            txtEstadoCamara.Text = "Listo. Puede realizar otro marcaje.";
        }

        private void MostrarResultado(MarcajeResponse resultado)
        {
            panelResultado.Visibility = Visibility.Visible;

            if (resultado.Exito)
            {
                // Fondo verde para éxito
                panelResultado.Background = new SolidColorBrush(Color.FromRgb(213, 245, 227));
                txtResultadoIcono.Text = "✔";
                txtResultadoIcono.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                txtResultadoMensaje.Text = resultado.Mensaje;
                txtResultadoMensaje.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));

                // Si hubo tardanza, mostrar el detalle en rojo
                if (resultado.EsTardanza)
                {
                    txtResultadoTardanza.Text = $"⚠ {resultado.MinutosTardanza} minuto(s) de tardanza";
                    txtResultadoTardanza.Visibility = Visibility.Visible;
                }
                else
                {
                    txtResultadoTardanza.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Fondo rojo para error
                panelResultado.Background = new SolidColorBrush(Color.FromRgb(253, 213, 210));
                txtResultadoIcono.Text = "✘";
                txtResultadoIcono.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                txtResultadoMensaje.Text = resultado.Mensaje;
                txtResultadoMensaje.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                txtResultadoTardanza.Visibility = Visibility.Collapsed;
            }
        }

        private void OcultarResultado()
        {
            panelResultado.Visibility = Visibility.Collapsed;
        }

        private void SetBotonesHabilitados(bool habilitado)
        {
            btnEntrada.IsEnabled = habilitado;
            btnSalida.IsEnabled  = habilitado;
            btnBreak.IsEnabled   = habilitado;
        }
    }
}
