using System;
using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class HorariosView : UserControl
    {
        private readonly HorariosController _controller;

        public HorariosView(HorariosController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        private async void HorariosView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var empleados = await _controller.ObtenerSelectorAsync();
                cmbEmpleado.ItemsSource     = empleados;
                cmbEmpleadoForm.ItemsSource = empleados;

                cmbDia.Items.Clear();
                foreach (DiaSemana d in Enum.GetValues(typeof(DiaSemana)))
                    cmbDia.Items.Add(d);
                cmbDia.SelectedIndex = 0;

                dpVigDesde.SelectedDate = DateTime.Today;
                dpVigHasta.SelectedDate = DateTime.Today.AddYears(1);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}", true);
            }
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmpleado.SelectedItem is not Core.DTOs.EmpleadoSelectorDto emp) return;

            btnCargar.IsEnabled = false;
            try
            {
                var horarios = await _controller.CargarHorariosAsync(emp.Id);
                dgHorarios.ItemsSource = horarios;
            }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnCargar.IsEnabled = true; }
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            pnlFormulario.Visibility = pnlFormulario.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => pnlFormulario.Visibility = Visibility.Collapsed;

        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            if (cmbEmpleadoForm.SelectedItem is not Core.DTOs.EmpleadoSelectorDto emp)
            { MostrarMensaje("Seleccione un empleado.", true); return; }

            if (cmbDia.SelectedItem is not DiaSemana dia)
            { MostrarMensaje("Seleccione un día.", true); return; }

            if (!TimeSpan.TryParse(txtEntrada.Text, out var entrada))
            { MostrarMensaje("Hora de entrada inválida (HH:mm).", true); return; }

            if (!TimeSpan.TryParse(txtSalida.Text, out var salida))
            { MostrarMensaje("Hora de salida inválida (HH:mm).", true); return; }

            if (dpVigDesde.SelectedDate == null || dpVigHasta.SelectedDate == null)
            { MostrarMensaje("Seleccione las fechas de vigencia.", true); return; }

            btnCrear.IsEnabled = false;
            try
            {
                var (ok, msg) = await _controller.CrearHorarioAsync(
                    emp.Id, dia, entrada, salida,
                    dpVigDesde.SelectedDate.Value, dpVigHasta.SelectedDate.Value);

                MostrarMensaje(msg, !ok);
                if (ok)
                {
                    pnlFormulario.Visibility = Visibility.Collapsed;
                    // Recargar si el mismo empleado está seleccionado
                    if (cmbEmpleado.SelectedItem is Core.DTOs.EmpleadoSelectorDto empActual && empActual.Id == emp.Id)
                    {
                        dgHorarios.ItemsSource = await _controller.CargarHorariosAsync(emp.Id);
                    }
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
