using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views
{
    public partial class HistorialView : UserControl
    {
        private readonly HistorialController _historialController;

        public HistorialView(HistorialController historialController)
        {
            InitializeComponent();
            _historialController = historialController;
        }

        // ── Cuando la vista carga: mostrar el mes actual ──
        private async void HistorialView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Establecer el mes actual en el DatePicker
                // Esto dispara SelectedDateChanged, que llama CargarHistorial automáticamente
                dpMes.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            catch (Exception ex)
            {
                txtSubtitulo.Text = "Error al cargar historial.";
                txtResumen.Text   = ex.Message;
            }
        }

        // ── Cada vez que el usuario cambia el mes seleccionado ──
        private async void DpMes_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpMes.SelectedDate == null) return;
            try
            {
                await CargarHistorial(dpMes.SelectedDate.Value);
            }
            catch (Exception ex)
            {
                txtSubtitulo.Text = "Error al cargar historial.";
                txtResumen.Text   = ex.Message;
            }
        }

        // ─── Método compartido de carga ────────────────────────────────────
        private async System.Threading.Tasks.Task CargarHistorial(DateTime mes)
        {
            // Mostrar estado de carga
            txtSubtitulo.Text = "Cargando registros...";
            txtResumen.Text   = "Cargando...";
            txtSinDatos.Visibility = Visibility.Collapsed;
            dgHistorial.Visibility = Visibility.Collapsed;

            // Delegar al controlador
            List<MarcajeFilaDto> filas = await _historialController.CargarHistorialAsync(mes);

            if (filas == null || filas.Count == 0)
            {
                // Sin datos: mostrar mensaje amigable
                txtSinDatos.Visibility = Visibility.Visible;
                txtSubtitulo.Text = $"Sin registros para {mes:MMMM yyyy}";
                txtResumen.Text   = "Sin registros en el período seleccionado.";
            }
            else
            {
                // Bindear la lista directamente al DataGrid
                dgHistorial.ItemsSource = filas;
                dgHistorial.Visibility  = Visibility.Visible;

                txtSubtitulo.Text = $"Mostrando registros de {mes:MMMM yyyy}";
                txtResumen.Text   = _historialController.GenerarResumen(filas);
            }
        }

        private void DgHistorial_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Fecha": e.Column.Header = "Fecha"; e.Column.Width = new DataGridLength(120); break;
                case "Hora": e.Column.Header = "Hora"; e.Column.Width = new DataGridLength(85); break;
                case "Tipo": e.Column.Header = "Tipo"; e.Column.Width = new DataGridLength(110); break;
                case "Tardanza": e.Column.Header = "Tardanza"; e.Column.Width = new DataGridLength(95); break;
                case "Minutos": e.Column.Header = "Minutos"; e.Column.Width = new DataGridLength(95); break;
                case "Metodo": e.Column.Header = "Método"; e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star); break;
                default: e.Cancel = true; break;
            }
        }
    }
}
