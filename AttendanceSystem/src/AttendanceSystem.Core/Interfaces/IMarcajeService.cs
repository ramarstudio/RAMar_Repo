using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IMarcajeService
    {
        Task<MarcajeResponse> RegistrarMarcajeAsync(MarcajeRequest request);
        Task<IEnumerable<Marcaje>> ObtenerHistorialEmpleadoAsync(int empleadoId, DateTime mes);
        Task<Marcaje> RegistrarMarcajeAsistidoAsync(int empleadoId, int adminId, DateTime fechaHora);
    }
}