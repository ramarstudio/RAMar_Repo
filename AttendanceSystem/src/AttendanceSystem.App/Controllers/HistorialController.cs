using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Security;

namespace AttendanceSystem.App.Controllers
{
    // DTO interno para que la vista pueda bindear directamente a la tabla
    // sin exponer la entidad Marcaje (con sus getters) al XAML
    public class MarcajeFilaDto
    {
        public string Fecha      { get; set; }
        public string Hora       { get; set; }
        public string Tipo       { get; set; }
        public string Tardanza   { get; set; }
        public string Minutos    { get; set; }
        public string Metodo     { get; set; }
    }

    public class HistorialController
    {
        private readonly IMarcajeService _marcajeService;
        private readonly SessionManager _sessionManager;

        public HistorialController(IMarcajeService marcajeService, SessionManager sessionManager)
        {
            _marcajeService = marcajeService;
            _sessionManager = sessionManager;
        }

        // Carga el historial del empleado logueado en el mes indicado
        public async Task<List<MarcajeFilaDto>> CargarHistorialAsync(DateTime mes)
        {
            if (!_sessionManager.EstaLogueado())
                return new List<MarcajeFilaDto>();

            // Obtenemos el id del empleado asociado al usuario en sesión
            int empleadoId = _sessionManager.ObtenerUsuarioActual().GetId();

            var marcajes = await _marcajeService.ObtenerHistorialEmpleadoAsync(empleadoId, mes);

            // Convertimos cada Marcaje a un DTO plano que el DataGrid puede mostrar
            return marcajes.Select(m => new MarcajeFilaDto
            {
                Fecha    = m.GetFechaHora().ToString("dd/MM/yyyy"),
                Hora     = m.GetFechaHora().ToString("HH:mm"),
                Tipo     = m.GetTipo().ToString(),
                Tardanza = m.EsTardanza() ? "Sí" : "No",
                Minutos  = m.EsTardanza() ? m.GetMinutosTardanza().ToString() : "—",
                Metodo   = m.FueAsistido() ? "Manual" : "Facial"
            }).ToList();
        }

        // Versión para admin: puede ver el historial de cualquier empleado
        public async Task<List<MarcajeFilaDto>> CargarHistorialAdminAsync(int empleadoId, DateTime mes)
        {
            var marcajes = await _marcajeService.ObtenerHistorialEmpleadoAsync(empleadoId, mes);

            return marcajes.Select(m => new MarcajeFilaDto
            {
                Fecha    = m.GetFechaHora().ToString("dd/MM/yyyy"),
                Hora     = m.GetFechaHora().ToString("HH:mm"),
                Tipo     = m.GetTipo().ToString(),
                Tardanza = m.EsTardanza() ? "Sí" : "No",
                Minutos  = m.EsTardanza() ? m.GetMinutosTardanza().ToString() : "—",
                Metodo   = m.FueAsistido() ? "Manual" : "Facial"
            }).ToList();
        }

        // Texto del resumen para mostrar debajo de la tabla
        public string GenerarResumen(List<MarcajeFilaDto> filas)
        {
            int entradas  = filas.Count(f => f.Tipo == "Entrada");
            int tardanzas = filas.Count(f => f.Tardanza == "Sí");
            return $"Total registros: {filas.Count}   |   Entradas: {entradas}   |   Tardanzas: {tardanzas}";
        }
    }
}
