using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class DashboardController
    {
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IMarcajeRepository  _marcajeRepo;
        private readonly ISessionManager     _session;

        public DashboardController(
            IEmpleadoRepository empleadoRepo,
            IMarcajeRepository  marcajeRepo,
            ISessionManager     session)
        {
            _empleadoRepo = empleadoRepo;
            _marcajeRepo  = marcajeRepo;
            _session      = session;
        }

        public async Task<DashboardKpiDto> ObtenerKpisAsync(CancellationToken ct = default)
        {
            var hoy     = DateTime.Today;
            var maniana = hoy.AddDays(1);

            // COUNT directo en BD — sin cargar entidades
            var empleados     = await _empleadoRepo.GetAllActivosAsync();
            int totalEmpleados = empleados.Count();

            // Solo marcajes del día — dataset reducido
            var marcajesHoy = await _marcajeRepo.GetByFechaAsync(hoy, maniana, ct);

            int asistencias = marcajesHoy.Count(m => m.EsEntrada());
            int tardanzas   = marcajesHoy.Count(m => m.EsEntrada() && m.EsTardanza());
            double tasa     = totalEmpleados > 0
                ? Math.Round((double)asistencias / totalEmpleados * 100, 1) : 0;

            return new DashboardKpiDto
            {
                TotalEmpleadosActivos = totalEmpleados,
                AsistenciasHoy        = asistencias,
                TardanzasHoy          = tardanzas,
                TotalUsuarios         = totalEmpleados,   // misma fuente — Empleados activos
                TasaAsistenciaHoy     = tasa
            };
        }

        public async Task<List<MarcajeResumenDto>> ObtenerUltimosMarcajesAsync(
            int cantidad = 10, CancellationToken ct = default)
        {
            var marcajes = await _marcajeRepo.GetUltimosAsync(cantidad, ct);
            return marcajes.Select(m => new MarcajeResumenDto
            {
                EmpleadoId = m.GetEmpleadoId().ToString(),
                FechaHora  = m.GetFechaHora().ToString("dd/MM/yyyy HH:mm"),
                Tipo       = m.GetTipo().ToString(),
                EsTardanza = m.EsTardanza() ? "Sí" : "No"
            }).ToList();
        }

        public string ObtenerNombreAdmin()
            => _session.GetCurrentSession()?.Username ?? "Admin";
    }
}
