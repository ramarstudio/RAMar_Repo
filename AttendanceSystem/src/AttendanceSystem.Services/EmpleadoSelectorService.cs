using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.Services
{
    // Encapsula la consulta "empleados activos con nombre de usuario vinculado".
    // Antes estaba duplicada en MarcajesAdminController y ReportesController.
    public class EmpleadoSelectorService : IEmpleadoSelectorService
    {
        private readonly IEmpleadoRepository             _empleadoRepo;
        private readonly IUsuarioRepository              _usuarioRepo;
        private readonly ILogger<EmpleadoSelectorService> _logger;

        public EmpleadoSelectorService(
            IEmpleadoRepository              empleadoRepo,
            IUsuarioRepository               usuarioRepo,
            ILogger<EmpleadoSelectorService> logger)
        {
            _empleadoRepo = empleadoRepo ?? throw new ArgumentNullException(nameof(empleadoRepo));
            _usuarioRepo  = usuarioRepo  ?? throw new ArgumentNullException(nameof(usuarioRepo));
            _logger       = logger;
        }

        public async Task<List<EmpleadoSelectorDto>> ObtenerSelectorAsync(CancellationToken ct = default)
        {
            var empleados  = await _empleadoRepo.GetAllActivosAsync();
            var empList    = empleados.ToList();

            var usuarioIds = empList.Select(e => e.GetUsuarioId()).Distinct().ToList();

            // IN (...) query — filtra solo los IDs necesarios
            var usuarios   = await _usuarioRepo.GetByIdsAsync(usuarioIds, ct);
            var nombresMap = usuarios.ToDictionary(u => u.GetId(), u => u.GetNombre());

            var selector = empList
                .Select(e => new EmpleadoSelectorDto
                {
                    Id     = e.GetId(),
                    Codigo = e.GetCodigo(),
                    Nombre = nombresMap.TryGetValue(e.GetUsuarioId(), out var n) ? n : e.GetCodigo()
                })
                .OrderBy(e => e.Nombre)
                .ToList();

            _logger.LogDebug("EmpleadoSelector: {Count} empleados activos cargados.", selector.Count);
            return selector;
        }
    }
}
