using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    public class HistorialController
    {
        private readonly IMarcajeService _marcajeService;
        private readonly ISessionManager _sessionManager;

        public HistorialController(IMarcajeService marcajeService, ISessionManager sessionManager)
        {
            _marcajeService = marcajeService;
            _sessionManager = sessionManager;
        }

        public async Task<List<MarcajeFilaDto>> CargarHistorialAsync(DateTime mes)
        {
            if (!_sessionManager.EstaLogueado()) return new List<MarcajeFilaDto>();

            var usuario = _sessionManager.ObtenerUsuarioActual();
            if (usuario == null) return new List<MarcajeFilaDto>();

            var marcajes = await _marcajeService.ObtenerHistorialEmpleadoAsync(usuario.GetId(), mes);
            return MapearFilas(marcajes);
        }

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

        private static List<MarcajeFilaDto> MapearFilas(IEnumerable<Marcaje> marcajes)
            => marcajes.Select(m => new MarcajeFilaDto
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
