using System;
using System.Threading;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface ITardanzaService
    {
        Task<(bool esTardanza, int minutosTarde, DateTime horarioBase)> EvaluarTardanzaAsync(
            Empleado empleado, DateTime horaLlegada, CancellationToken ct = default);

        Task<bool> EsDiaLaboralAsync(
            Empleado empleado, DateTime fecha, CancellationToken ct = default);

        Task<Horario> ObtenerHorarioVigenteHoyAsync(
            Empleado empleado, CancellationToken ct = default);
    }
}
