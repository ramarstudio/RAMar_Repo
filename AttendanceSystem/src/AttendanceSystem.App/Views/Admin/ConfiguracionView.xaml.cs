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
        private ConfiguracionFilaDto _seleccionada;

        public ConfiguracionView(ConfiguracionController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        private async void ConfiguracionView_Loaded(object sender, RoutedEventArgs e)
        {
            try { await CargarTodoAsync(); }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
        }

        private async System.Threading.Tasks.Task CargarTodoAsync()
        {
            var configs = await _controller.CargarConfiguracionesAsync();
            icConfigs.ItemsSource = configs;

            bool hayConfigs = configs.Count > 0;
            lblActivos.Visibility = hayConfigs ? Visibility.Visible : Visibility.Collapsed;
            pnlVacio.Visibility = hayConfigs ? Visibility.Collapsed : Visibility.Visible;

            var presets = await _controller.ObtenerPresetsDisponiblesAsync();
            icPresets.ItemsSource = presets;
            lblSugeridos.Visibility = presets.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Agregar preset con un clic ───────────────────────────────────────
        private async void BtnAgregarPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ConfigPresetDto preset) return;

            btn.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.CrearConfiguracionAsync(
                    preset.Clave, preset.ValorDefault, preset.TipoDato, preset.Descripcion);
                MostrarMensaje(msg, !ok);
                if (ok) await CargarTodoAsync();
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btn.IsEnabled = true; }
        }

        // ── Eliminar parámetro ───────────────────────────────────────────────
        private async void BtnEliminarConfig_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ConfiguracionFilaDto fila) return;

            var result = MessageBox.Show(
                $"¿Eliminar el parámetro \"{fila.Descripcion}\"?\n\nEsto se puede volver a agregar desde los parámetros disponibles.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var (ok, msg) = await _controller.EliminarConfiguracionAsync(fila.Id);
                MostrarMensaje(msg, !ok);
                if (ok)
                {
                    pnlEditar.Visibility = Visibility.Collapsed;
                    _seleccionada = null;
                    await CargarTodoAsync();
                }
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
        }

        // ── Editar desde tarjeta ─────────────────────────────────────────────
        private void BtnEditarConfig_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ConfiguracionFilaDto fila) return;

            _seleccionada = fila;
            txtDescripcionSeleccionada.Text = fila.Descripcion;

            if (fila.EsBool)
            {
                pnlEditorTexto.Visibility = Visibility.Collapsed;
                pnlEditorBool.Visibility = Visibility.Visible;
                toggleBool.IsChecked = fila.ValorBool;
                txtToggleLabel.Text = fila.ValorBool ? "Activado" : "Desactivado";
            }
            else
            {
                pnlEditorTexto.Visibility = Visibility.Visible;
                pnlEditorBool.Visibility = Visibility.Collapsed;
                txtNuevoValor.Text = fila.Valor;
            }

            pnlEditar.Visibility = Visibility.Visible;
            pnlEditar.BringIntoView();
        }

        private async void ToggleBool_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionada == null) return;

            bool nuevoValor = toggleBool.IsChecked == true;
            txtToggleLabel.Text = nuevoValor ? "Activado" : "Desactivado";

            try
            {
                var (ok, msg) = await _controller.ActualizarValorAsync(
                    _seleccionada.Id, nuevoValor.ToString().ToLower());
                MostrarMensaje(msg, !ok);
                if (ok) await CargarTodoAsync();
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionada == null) return;

            btnGuardar.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.ActualizarValorAsync(
                    _seleccionada.Id, txtNuevoValor.Text);
                MostrarMensaje(msg, !ok);
                if (ok)
                {
                    pnlEditar.Visibility = Visibility.Collapsed;
                    _seleccionada = null;
                    await CargarTodoAsync();
                }
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnGuardar.IsEnabled = true; }
        }

        private void BtnCancelarEditar_Click(object sender, RoutedEventArgs e)
        {
            pnlEditar.Visibility = Visibility.Collapsed;
            _seleccionada = null;
        }

        // ── Feedback ─────────────────────────────────────────────────────────
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
