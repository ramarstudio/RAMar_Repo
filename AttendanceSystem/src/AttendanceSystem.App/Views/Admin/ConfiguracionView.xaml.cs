using System;
using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class ConfiguracionView : UserControl
    {
        private readonly ConfiguracionController _controller;

        public ConfiguracionView(ConfiguracionController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        private async void ConfiguracionView_Loaded(object sender, RoutedEventArgs e)
        {
            try { await CargarConfigsAsync(); }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
        }

        private async System.Threading.Tasks.Task CargarConfigsAsync()
        {
            var configs = await _controller.CargarConfiguracionesAsync();
            dgConfig.ItemsSource = configs;
        }

        private void DgConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgConfig.SelectedItem is ConfiguracionFilaDto fila)
            {
                txtClaveSeleccionada.Text = fila.Clave;
                txtNuevoValor.Text = fila.Valor;
                pnlEditar.Visibility = Visibility.Visible;
            }
            else
            {
                pnlEditar.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (dgConfig.SelectedItem is not ConfiguracionFilaDto fila) return;

            btnGuardar.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.ActualizarValorAsync(fila.Id, txtNuevoValor.Text);
                MostrarMensaje(msg, !ok);
                if (ok) await CargarConfigsAsync();
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnGuardar.IsEnabled = true; }
        }

        private void BtnNueva_Click(object sender, RoutedEventArgs e)
        {
            pnlNueva.Visibility = pnlNueva.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnCancelarNueva_Click(object sender, RoutedEventArgs e)
            => pnlNueva.Visibility = Visibility.Collapsed;

        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            string tipo = (cmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "string";

            btnCrear.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.CrearConfiguracionAsync(
                    txtClave.Text, txtValor.Text, tipo, txtDescripcion.Text);
                MostrarMensaje(msg, !ok);
                if (ok)
                {
                    pnlNueva.Visibility = Visibility.Collapsed;
                    txtClave.Text = txtValor.Text = txtDescripcion.Text = "";
                    await CargarConfigsAsync();
                }
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnCrear.IsEnabled = true; }
        }

        private void MostrarMensaje(string msg, bool error)
        {
            txtMensaje.Text = msg;
            txtMensaje.Foreground = error
                ? System.Windows.Media.Brushes.Crimson
                : System.Windows.Media.Brushes.ForestGreen;
            txtMensaje.Visibility = Visibility.Visible;
        }
    }
}
