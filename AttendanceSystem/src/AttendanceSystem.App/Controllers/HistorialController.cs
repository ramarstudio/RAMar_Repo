using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Security;

namespace AttendanceSystem.App.Controllers
{
    public class MarcajeFilaDto
    {
        public string Fecha    { get; set; }
        public string Hora     { get; set; }
        public string Tipo     { get; set; }
        public string Tardanza { get; set; }
        public string Minutos  { get; set; }
        public string Metodo   { get; set; }
    }

    public class HistorialController
    {
        private readonly IMarcajeService _marcajeService;
        private readonly SessionManager  _sessionManager;

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

            // Null-check explícito antes de llamar GetId() para evitar NullReferenceException
            var usuario = _sessionManager.ObtenerUsuarioActual();
            if (usuario == null) return new List<MarcajeFilaDto>();

            int empleadoId = usuario.GetId();
            var marcajes   = await _marcajeService.ObtenerHistorialEmpleadoAsync(empleadoId, mes);

            return MapearFilas(marcajes);
        }

        // Versión para admin: puede ver el historial de cualquier empleado
        public async Task<List<MarcajeFilaDto>> CargarHistorialAdminAsync(int empleadoId, DateTime mes)
        {
            var marcajes = await _marcajeService.ObtenerHistorialEmpleadoAsync(empleadoId, mes);
            return MapearFilas(marcajes);
        }

        public string GenerarResumen(List<MarcajeFilaDto> filas)
        {
            int entradas  = filas.Count(f => f.Tipo == "Entrada");
            int tardanzas = filas.Count(f => f.Tardanza == "Sí");
            return $"Total registros: {filas.Count}   |   Entradas: {entradas}   |   Tardanzas: {tardanzas}";
        }

        // Proyección compartida entre las dos variantes del historial
        private static List<MarcajeFilaDto> MapearFilas(IEnumerable<Marcaje> marcajes)
        {
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
    }
}
