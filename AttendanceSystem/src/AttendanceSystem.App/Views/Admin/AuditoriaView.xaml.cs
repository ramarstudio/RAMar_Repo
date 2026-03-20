using System;
using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers.Admin;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class AuditoriaView : UserControl
    {
        private readonly AuditoriaController _controller;

        public AuditoriaView(AuditoriaController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        private async void AuditoriaView_Loaded(object sender, RoutedEventArgs e)
        {
            dpDesde.SelectedDate = DateTime.Today.AddDays(-30);
            dpHasta.SelectedDate = DateTime.Today;

            try
            {
                var entidades = await _controller.ObtenerEntidadesAsync();
                cmbEntidad.Items.Clear();
                cmbEntidad.Items.Add("Todas");
                foreach (var ent in entidades) cmbEntidad.Items.Add(ent);
                cmbEntidad.SelectedIndex = 0;

                await CargarLogsAsync();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al cargar: {ex.Message}", true);
            }
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            btnBuscar.IsEnabled = false;
            try { await CargarLogsAsync(); }
            catch (Exception ex) { MostrarMensaje($"Error: {ex.Message}", true); }
            finally { btnBuscar.IsEnabled = true; }
        }

        private async System.Threading.Tasks.Task CargarLogsAsync()
        {
            string entidad = cmbEntidad.SelectedItem?.ToString();
            if (entidad == "Todas") entidad = null;

            var logs = await _controller.CargarLogsAsync(
                dpDesde.SelectedDate, dpHasta.SelectedDate, entidad);

            dgAudit.ItemsSource = logs;
            txtContador.Text = $"{logs.Count} registro{(logs.Count != 1 ? "s" : "")} encontrado{(logs.Count != 1 ? "s" : "")}";

            txtVacio.Visibility = logs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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
