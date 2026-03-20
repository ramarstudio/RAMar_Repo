using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class UsuariosView : UserControl
    {
        private readonly UsuariosController _ctrl;
        private UsuarioFilaDto _filaSeleccionada;

        public UsuariosView(UsuariosController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────
        private async void UsuariosView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await CargarRolesAsync();
                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar usuarios: {ex.Message}", false);
            }
        }

        // ── Carga de datos ────────────────────────────────────────────────────────
        private async Task CargarUsuariosAsync()
        {
            OcultarMensaje();
            var lista = await _ctrl.ObtenerTodosAsync();
            dgUsuarios.ItemsSource = lista;

            bool sinDatos = lista == null || lista.Count == 0;
            txtSinUsuarios.Visibility = sinDatos ? Visibility.Visible  : Visibility.Collapsed;
            dgUsuarios.Visibility     = sinDatos ? Visibility.Collapsed : Visibility.Visible;

            PanelAcciones.Visibility = Visibility.Collapsed;
            _filaSeleccionada = null;
        }

        private async Task CargarRolesAsync()
        {
            var roles = await _ctrl.ObtenerRolesAsync();
            cmbRol.ItemsSource   = roles;
            cmbRol.SelectedIndex = 0;
        }

        // ── Selección en DataGrid ─────────────────────────────────────────────────
        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filaSeleccionada = dgUsuarios.SelectedItem as UsuarioFilaDto;

            if (_filaSeleccionada != null)
            {
                txtAccionesLabel.Text    = $"Seleccionado: {_filaSeleccionada.Username}";
                PanelAcciones.Visibility = Visibility.Visible;
                pbNuevaPass.Clear();
                OcultarMensaje();
            }
            else
            {
                PanelAcciones.Visibility = Visibility.Collapsed;
            }
        }

        // ── Mostrar / ocultar formulario de nuevo usuario ─────────────────────────
        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            bool visible = PanelFormNuevo.Visibility == Visibility.Visible;
            PanelFormNuevo.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
            if (!visible) LimpiarFormulario();
            OcultarMensaje();
        }

        // ── Crear usuario nuevo ───────────────────────────────────────────────────
        private async void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rolSel = cmbRol.SelectedItem as RolSelectorDto;
                if (rolSel == null)
                {
                    MostrarMensaje("Selecciona un rol.", false);
                    return;
                }

                var (ok, msg) = await _ctrl.CrearUsuarioAsync(
                    txtUsername.Text.Trim(),
                    txtNombre.Text.Trim(),
                    pbPassword.Password,
                    rolSel.Id);

                MostrarMensaje(msg, ok);
                if (ok)
                {
                    LimpiarFormulario();
                    PanelFormNuevo.Visibility = Visibility.Collapsed;
                    await CargarUsuariosAsync();
                }
            }
            catch (Exception ex)
            {
                // Extraer el mensaje más profundo (el error real de PostgreSQL)
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                var detalle = inner == ex ? ex.Message : $"{ex.Message} → {inner.Message}";
                MostrarMensaje($"Error: {detalle}", false);
                System.Diagnostics.Debug.WriteLine($"[UsuariosView] {ex}");
            }
        }

        // ── Cancelar formulario ───────────────────────────────────────────────────
        private void BtnCancelarNuevo_Click(object sender, RoutedEventArgs e)
        {
            PanelFormNuevo.Visibility = Visibility.Collapsed;
            LimpiarFormulario();
            OcultarMensaje();
        }

        // ── Activar / desactivar el usuario seleccionado ──────────────────────────
        private async void BtnToggleActivo_Click(object sender, RoutedEventArgs e)
        {
            if (_filaSeleccionada == null) return;
            try
            {
                var (ok, msg) = await _ctrl.ToggleActivoAsync(_filaSeleccionada.Id);
                MostrarMensaje(msg, ok);
                if (ok) await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", false);
            }
        }

        // ── Cambiar contraseña del usuario seleccionado ───────────────────────────
        private async void BtnCambiarPass_Click(object sender, RoutedEventArgs e)
        {
            if (_filaSeleccionada == null) return;
            try
            {
                var (ok, msg) = await _ctrl.CambiarPasswordAsync(
                    _filaSeleccionada.Id, pbNuevaPass.Password);
                MostrarMensaje(msg, ok);
                pbNuevaPass.Clear();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", false);
            }
        }

        // ── Actualizar lista ──────────────────────────────────────────────────────
        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            try { await CargarUsuariosAsync(); }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", false); }
        }

        // ── Helpers UI ────────────────────────────────────────────────────────────
        private void LimpiarFormulario()
        {
            txtUsername.Clear();
            txtNombre.Clear();
            pbPassword.Clear();
            if (cmbRol.Items.Count > 0) cmbRol.SelectedIndex = 0;
        }

        private void MostrarMensaje(string msg, bool ok)
        {
            txtMensaje.Text       = msg;
            txtMensaje.Foreground = ok ? Brushes.Green : Brushes.Red;
            txtMensaje.Visibility = Visibility.Visible;
        }

        private void OcultarMensaje()
            => txtMensaje.Visibility = Visibility.Collapsed;
    }
}
