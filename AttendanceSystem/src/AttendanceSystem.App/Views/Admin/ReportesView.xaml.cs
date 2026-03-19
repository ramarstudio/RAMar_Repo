using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.App.Views.Admin
{
    public partial class ReportesView : UserControl
    {
        private readonly ReportesController _ctrl;
        private ReporteDto _reporteActual;

        public ReportesView(ReportesController ctrl)
        {
            InitializeComponent();
            _ctrl = ctrl;
        }

        // ── Carga inicial ─────────────────────────────────────────────────────────
        private async void ReportesView_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarEmpleadosAsync();
            PoblarMeses();
            PoblarAnios();
        }

        // ── Poblar selectores ─────────────────────────────────────────────────────
        private async Task CargarEmpleadosAsync()
        {
            var empleados = await _ctrl.ObtenerEmpleadosAsync();
            cmbEmpleado.ItemsSource = empleados;
        }

        private void PoblarMeses()
        {
            var meses = new[]
            {
                new { Nombre = "Enero",      Valor = 1  },
                new { Nombre = "Febrero",    Valor = 2  },
                new { Nombre = "Marzo",      Valor = 3  },
                new { Nombre = "Abril",      Valor = 4  },
                new { Nombre = "Mayo",       Valor = 5  },
                new { Nombre = "Junio",      Valor = 6  },
                new { Nombre = "Julio",      Valor = 7  },
                new { Nombre = "Agosto",     Valor = 8  },
                new { Nombre = "Septiembre", Valor = 9  },
                new { Nombre = "Octubre",    Valor = 10 },
                new { Nombre = "Noviembre",  Valor = 11 },
                new { Nombre = "Diciembre",  Valor = 12 }
            };
            cmbMes.ItemsSource       = meses;
            cmbMes.DisplayMemberPath = "Nombre";
            cmbMes.SelectedIndex     = DateTime.Now.Month - 1;
        }

        private void PoblarAnios()
        {
            int anioActual = DateTime.Now.Year;
            var anios = new int[5];
            for (int i = 0; i < 5; i++) anios[i] = anioActual - i;
            cmbAnio.ItemsSource   = anios;
            cmbAnio.SelectedIndex = 0;
        }

        // ── Generar reporte ───────────────────────────────────────────────────────
        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            var empSel = cmbEmpleado.SelectedItem as EmpleadoSelectorDto;
            if (empSel == null)
            {
                MostrarMensaje("Selecciona un empleado.", false);
                return;
            }

            dynamic mesSel  = cmbMes.SelectedItem;
            dynamic anioSel = cmbAnio.SelectedItem;
            if (mesSel == null || anioSel == null)
            {
                MostrarMensaje("Selecciona mes y año.", false);
                return;
            }

            int mes  = mesSel.Valor;
            int anio = (int)anioSel;

            OcultarMensaje();
            PanelResultados.Visibility = Visibility.Collapsed;

            var (ok, reporte, msg) = await _ctrl.GenerarReporteAsync(empSel.Id, mes, anio);

            if (!ok)
            {
                MostrarMensaje(msg, false);
                return;
            }

            _reporteActual = reporte;
            MostrarResultados(reporte, empSel.Codigo, mes, anio);
        }

        // ── Mostrar resultados en pantalla ────────────────────────────────────────
        private void MostrarResultados(ReporteDto dto, string codigo, int mes, int anio)
        {
            txtTituloReporte.Text = $"Reporte — Empleado: {codigo}";
            txtPeriodo.Text       = $"{dto.PeriodoInicio:dd/MM/yyyy}  al  {dto.PeriodoFin:dd/MM/yyyy}";

            txtAsistencias.Text = dto.TotalAsistencias.ToString();
            txtTardanzas.Text   = dto.TotalTardanzas.ToString();
            txtFaltas.Text      = dto.TotalFaltas.ToString();
            txtMinutos.Text     = dto.SumatoriaMinutosTardanza.ToString();

            PanelResultados.Visibility = Visibility.Visible;
            MostrarMensaje("Reporte generado correctamente.", true);
        }

        // ── Exportar PDF ──────────────────────────────────────────────────────────
        private async void BtnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_reporteActual == null) return;
            var (ok, rutaOError) = await _ctrl.ExportarPdfAsync(_reporteActual);
            if (ok)
            {
                MostrarMensaje($"PDF guardado en: {rutaOError}", true);
                AbrirCarpeta(rutaOError);
            }
            else
            {
                MostrarMensaje($"Error al exportar PDF: {rutaOError}", false);
            }
        }

        // ── Exportar CSV ──────────────────────────────────────────────────────────
        private async void BtnExportarCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_reporteActual == null) return;
            var (ok, rutaOError) = await _ctrl.ExportarCsvAsync(_reporteActual);
            if (ok)
            {
                MostrarMensaje($"CSV guardado en: {rutaOError}", true);
                AbrirCarpeta(rutaOError);
            }
            else
            {
                MostrarMensaje($"Error al exportar CSV: {rutaOError}", false);
            }
        }

        // ── Abrir carpeta del archivo exportado en el explorador ──────────────────
        private static void AbrirCarpeta(string rutaArchivo)
        {
            try
            {
                string carpeta = Path.GetDirectoryName(rutaArchivo);
                if (!string.IsNullOrEmpty(carpeta))
                    Process.Start("explorer.exe", carpeta);
            }
            catch { /* No crítico: si falla, el archivo sigue guardado */ }
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
