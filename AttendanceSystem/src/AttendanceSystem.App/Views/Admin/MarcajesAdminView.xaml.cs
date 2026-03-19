using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AttendanceSystem.App.Controllers.Admin;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class MarcajesAdminView : UserControl
    {
        private readonly MarcajesAdminController _ctrl;

        public MarcajesAdminView(MarcajesAdminController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────
        private async void MarcajesAdminView_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarEmpleadosAsync();
            dpMes.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        // ── Cargar lista de empleados en los ComboBoxes ───────────────────────────
        private async Task CargarEmpleadosAsync()
        {
            var empleados = await _ctrl.ObtenerEmpleadosAsync();
            cmbEmpleado.ItemsSource       = empleados;
            cmbEmpleadoManual.ItemsSource = empleados;
        }

        // ── Buscar marcajes ───────────────────────────────────────────────────────
        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var empSel = cmbEmpleado.SelectedItem as EmpleadoSelectorDto;
            if (empSel == null)
            {
                MostrarMensaje("Selecciona un empleado.", false);
                return;
            }
            if (dpMes.SelectedDate == null)
            {
                MostrarMensaje("Selecciona un mes.", false);
                return;
            }

            await CargarMarcajesAsync(empSel.Id, dpMes.SelectedDate.Value);
        }

        private async Task CargarMarcajesAsync(int empleadoId, DateTime mes)
        {
            OcultarMensaje();
            txtSinMarcajes.Text       = "Cargando...";
            txtSinMarcajes.Visibility = Visibility.Visible;
            dgMarcajes.Visibility     = Visibility.Collapsed;

            var filas = await _ctrl.CargarMarcajesAsync(empleadoId, mes);

            if (filas == null || filas.Count == 0)
            {
                txtSinMarcajes.Text = $"Sin marcajes para el período seleccionado.";
                txtResumen.Text     = "Sin datos.";
            }
            else
            {
                dgMarcajes.ItemsSource    = filas;
                dgMarcajes.Visibility     = Visibility.Visible;
                txtSinMarcajes.Visibility = Visibility.Collapsed;
                txtResumen.Text           = _ctrl.GenerarResumen(filas);
            }
        }

        // ── Mostrar / ocultar formulario de marcaje manual ────────────────────────
        private void BtnMostrarFormManual_Click(object sender, RoutedEventArgs e)
        {
            bool visible = PanelMarcajeManual.Visibility == Visibility.Visible;
            PanelMarcajeManual.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
            if (!visible)
            {
                dpFechaMarcaje.SelectedDate = DateTime.Today;
                txtHoraMarcaje.Text         = DateTime.Now.ToString("HH:mm");
            }
            OcultarMensaje();
        }

        // ── Confirmar marcaje manual ──────────────────────────────────────────────
        private async void BtnConfirmarManual_Click(object sender, RoutedEventArgs e)
        {
            var empSel = cmbEmpleadoManual.SelectedItem as EmpleadoSelectorDto;
            if (empSel == null)
            {
                MostrarMensaje("Selecciona un empleado.", false);
                return;
            }
            if (dpFechaMarcaje.SelectedDate == null)
            {
                MostrarMensaje("Selecciona una fecha.", false);
                return;
            }
            if (!TimeSpan.TryParse(txtHoraMarcaje.Text.Trim(), out var hora))
            {
                MostrarMensaje("Formato de hora inválido. Usa HH:mm (ej: 08:30).", false);
                return;
            }

            var fechaHora = dpFechaMarcaje.SelectedDate.Value.Date + hora;
            var (ok, msg) = await _ctrl.RegistrarMarcajeManualAsync(empSel.Id, fechaHora);

            MostrarMensaje(msg, ok);
            if (ok)
            {
                PanelMarcajeManual.Visibility = Visibility.Collapsed;
                // Refrescar tabla si el empleado marcado coincide con el filtro activo
                var filtroEmp = cmbEmpleado.SelectedItem as EmpleadoSelectorDto;
                if (filtroEmp?.Id == empSel.Id && dpMes.SelectedDate != null)
                    await CargarMarcajesAsync(empSel.Id, dpMes.SelectedDate.Value);
            }
        }

        // ── Cancelar formulario manual ────────────────────────────────────────────
        private void BtnCancelarManual_Click(object sender, RoutedEventArgs e)
        {
            PanelMarcajeManual.Visibility = Visibility.Collapsed;
            OcultarMensaje();
        }

        // ── Helpers UI ────────────────────────────────────────────────────────────
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
