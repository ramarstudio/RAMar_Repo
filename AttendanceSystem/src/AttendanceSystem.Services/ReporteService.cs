using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;

namespace AttendanceSystem.Services
{
    public class ReporteService
    {
        private readonly IMarcajeRepository  _marcajeRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IHorarioRepository  _horarioRepository;

        public ReporteService(
            IMarcajeRepository  marcajeRepository,
            IEmpleadoRepository empleadoRepository,
            IHorarioRepository  horarioRepository)
        {
            _marcajeRepository  = marcajeRepository;
            _empleadoRepository = empleadoRepository;
            _horarioRepository  = horarioRepository;
        }

        public async Task<ReporteDto> GenerarReporteMensualAsync(int empleadoId, int mes, int anio)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new ArgumentException("El empleado especificado no existe.");

            var periodoInicio = new DateTime(anio, mes, 1);
            int diasDelMes    = DateTime.DaysInMonth(anio, mes);
            var periodoFin    = new DateTime(anio, mes, diasDelMes, 23, 59, 59);

            var marcajes = await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, periodoInicio, periodoFin);
            var horarios = await _horarioRepository.GetByEmpleadoIdAsync(empleadoId);

            // Materializar una vez para evitar múltiples enumeraciones del IEnumerable
            var entradas     = marcajes.Where(m => m.EsEntrada()).ToList();
            var horariosList = horarios.ToList();

            // Un solo recorrido sobre entradas para calcular tardanzas y minutos
            int totalAsistencias   = entradas.Count;
            int totalTardanzas     = 0;
            int sumatoriaTardanzas = 0;

            foreach (var m in entradas)
            {
                if (m.EsTardanza())
                {
                    totalTardanzas++;
                    sumatoriaTardanzas += m.GetMinutosTardanza();
                }
            }

            // Calcular días laborables: un recorrido por el mes con acceso O(1) al horario
            int diasLaborablesReales = 0;

            for (int i = 1; i <= diasDelMes; i++)
            {
                var fechaIteracion = new DateTime(anio, mes, i);
                var diaSemanaEnum  = ConvertirADiaSemana(fechaIteracion.DayOfWeek);
                var fechaDateOnly  = DateOnly.FromDateTime(fechaIteracion);

                bool debioTrabajar = horariosList.Any(h =>
                    h.GetDia() == diaSemanaEnum &&
                    DateOnly.FromDateTime(h.GetVigenteDesde()) <= fechaDateOnly &&
                    // DateTime.MinValue se usa como centinela de "sin fecha fin"
                    (h.GetVigenteHasta() == DateTime.MinValue ||
                     DateOnly.FromDateTime(h.GetVigenteHasta()) >= fechaDateOnly));

                if (debioTrabajar) diasLaborablesReales++;
            }

            int totalFaltas = Math.Max(0, diasLaborablesReales - totalAsistencias);

            return new ReporteDto
            {
                EmpleadoId               = empleado.GetId(),
                CodigoEmpleado           = empleado.GetCodigo(),
                TotalAsistencias         = totalAsistencias,
                TotalTardanzas           = totalTardanzas,
                TotalFaltas              = totalFaltas,
                SumatoriaMinutosTardanza = sumatoriaTardanzas,
                PeriodoInicio            = periodoInicio,
                PeriodoFin               = periodoFin
            };
        }

        private static DiaSemana ConvertirADiaSemana(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Monday    => DiaSemana.Lunes,
            DayOfWeek.Tuesday   => DiaSemana.Martes,
            DayOfWeek.Wednesday => DiaSemana.Miercoles,
            DayOfWeek.Thursday  => DiaSemana.Jueves,
            DayOfWeek.Friday    => DiaSemana.Viernes,
            DayOfWeek.Saturday  => DiaSemana.Sabado,
            DayOfWeek.Sunday    => DiaSemana.Domingo,
            _                   => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
        };
    }
}
