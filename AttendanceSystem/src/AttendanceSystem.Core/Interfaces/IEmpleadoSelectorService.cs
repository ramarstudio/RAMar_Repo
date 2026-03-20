using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    // Encapsula la consulta "empleados activos con nombre de usuario vinculado"
    // que estaba duplicada en MarcajesAdminController y ReportesController.
    public interface IEmpleadoSelectorService
    {
        Task<List<EmpleadoSelectorDto>> ObtenerSelectorAsync(CancellationToken ct = default);
    }
}
