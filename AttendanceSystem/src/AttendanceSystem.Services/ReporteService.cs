using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IMarcajeRepository      _marcajeRepository;
        private readonly IEmpleadoRepository     _empleadoRepository;
        private readonly IHorarioRepository      _horarioRepository;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(
            IMarcajeRepository      marcajeRepository,
            IEmpleadoRepository     empleadoRepository,
            IHorarioRepository      horarioRepository,
            ILogger<ReporteService> logger)
        {
            _marcajeRepository  = marcajeRepository;
            _empleadoRepository = empleadoRepository;
            _horarioRepository  = horarioRepository;
            _logger             = logger;
        }

        public async Task<ReporteDto> GenerarReporteMensualAsync(
            int empleadoId, int mes, int anio, CancellationToken ct = default)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                throw new ArgumentException($"Empleado con ID {empleadoId} no existe.");

            var periodoInicio = new DateTime(anio, mes, 1);
            int diasDelMes    = DateTime.DaysInMonth(anio, mes);
            var periodoFin    = new DateTime(anio, mes, diasDelMes, 23, 59, 59);

            var marcajes = await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, periodoInicio, periodoFin);
            var horarios = await _horarioRepository.GetByEmpleadoIdAsync(empleadoId);

            var entradas     = marcajes.Where(m => m.EsEntrada()).ToList();
            var horariosList = horarios.ToList();

            // Un solo recorrido sobre entradas para totalizar tardanzas
            int totalAsistencias   = entradas.Count;
            int totalTardanzas     = 0;
            int sumatoriaTardanzas = 0;

            foreach (var m in entradas)
            {
                if (!m.EsTardanza()) continue;
                totalTardanzas++;
                sumatoriaTardanzas += m.GetMinutosTardanza();
            }

            // Días laborables: preprocesar horarios por día para lookup O(1)
            var horariosPorDia = horariosList
                .GroupBy(h => h.GetDia())
                .ToDictionary(g => g.Key, g => g.ToList());

            int diasLaborablesReales = 0;
            for (int i = 1; i <= diasDelMes; i++)
            {
                var fecha         = new DateTime(anio, mes, i);
                var diaSemanaEnum = ConvertirADiaSemana(fecha.DayOfWeek);

                if (!horariosPorDia.TryGetValue(diaSemanaEnum, out var horariosDelDia))
                    continue;

                var fechaDateOnly = DateOnly.FromDateTime(fecha);
                bool debioTrabajar = horariosDelDia.Any(h =>
                    DateOnly.FromDateTime(h.GetVigenteDesde()) <= fechaDateOnly &&
                    (h.GetVigenteHasta() == DateTime.MinValue ||
                     DateOnly.FromDateTime(h.GetVigenteHasta()) >= fechaDateOnly));

                if (debioTrabajar) diasLaborablesReales++;
            }

            int totalFaltas = Math.Max(0, diasLaborablesReales - totalAsistencias);

            _logger.LogInformation(
                "Reporte generado — empleado {Id}: {A} asist., {T} tard., {F} faltas.",
                empleadoId, totalAsistencias, totalTardanzas, totalFaltas);

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

        private static DiaSemana ConvertirADiaSemana(DayOfWeek d) => d switch
        {
            DayOfWeek.Monday    => DiaSemana.Lunes,
            DayOfWeek.Tuesday   => DiaSemana.Martes,
            DayOfWeek.Wednesday => DiaSemana.Miercoles,
            DayOfWeek.Thursday  => DiaSemana.Jueves,
            DayOfWeek.Friday    => DiaSemana.Viernes,
            DayOfWeek.Saturday  => DiaSemana.Sabado,
            DayOfWeek.Sunday    => DiaSemana.Domingo,
            _                   => throw new ArgumentOutOfRangeException(nameof(d))
        };
    }
}
