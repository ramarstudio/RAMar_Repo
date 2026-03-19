using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers.Admin;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class DashboardView : UserControl
    {
        private readonly DashboardController _ctrl;

        public DashboardView(DashboardController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────
        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Saludo dinámico según hora del día
            int hora = DateTime.Now.Hour;
            txtSaludo.Text = hora < 12 ? "Buenos días," : hora < 18 ? "Buenas tardes," : "Buenas noches,";

            txtNombreAdmin.Text = _ctrl.ObtenerNombreAdmin();
            txtFechaHoy.Text    = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                                      new System.Globalization.CultureInfo("es-ES"));

            try
            {
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                txtTotalEmpleados.Text = "!";
                txtAsistenciasHoy.Text = "!";
                txtTardanzasHoy.Text   = "!";
                txtTasaAsistencia.Text = "!";
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Botón Actualizar ──────────────────────────────────────────────────────
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try { await CargarDatosAsync(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Carga de KPIs y actividad reciente ───────────────────────────────────
        private async Task CargarDatosAsync()
        {
            txtTotalEmpleados.Text = "…";
            txtAsistenciasHoy.Text = "…";
            txtTardanzasHoy.Text   = "…";
            txtTasaAsistencia.Text = "…";

            var kpis = await _ctrl.ObtenerKpisAsync();

            txtTotalEmpleados.Text = kpis.TotalEmpleadosActivos.ToString();
            txtAsistenciasHoy.Text = kpis.AsistenciasHoy.ToString();
            txtTardanzasHoy.Text   = kpis.TardanzasHoy.ToString();
            txtTasaAsistencia.Text = $"{kpis.TasaAsistenciaHoy}%";

            var recientes = await _ctrl.ObtenerUltimosMarcajesAsync(10);
            dgReciente.ItemsSource         = recientes;
            txtContadorReciente.Text       = $"{recientes.Count} registros";
        }
    }
}
