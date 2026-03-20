using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IMarcajeRepository : IRepositoryBase<Marcaje>
    {
        Task<Marcaje>              GetByIdAsync(int id);
        Task<IEnumerable<Marcaje>> GetByEmpleadoIdAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
        Task<Marcaje>              GetUltimoMarcajeDelDiaAsync(int empleadoId, DateTime fecha);

        // Consultas optimizadas para Dashboard — solo COUNT o proyecciones mínimas
        Task<int>           CountByFechaRangoAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
        Task<List<Marcaje>> GetUltimosAsync(int cantidad, CancellationToken ct = default);
        Task<List<Marcaje>> GetByFechaAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);

        // Consultas analíticas para gráficos del dashboard
        Task<List<AsistenciaDiariaDto>>  GetAsistenciasDiariasAsync(int dias, CancellationToken ct = default);
        Task<List<TardanzaEmpleadoDto>>  GetTopTardanzasAsync(int topN, int dias, CancellationToken ct = default);
        Task<List<TardanzaEmpleadoDto>>  GetTopPuntualesAsync(int topN, int dias, CancellationToken ct = default);
    }
}
