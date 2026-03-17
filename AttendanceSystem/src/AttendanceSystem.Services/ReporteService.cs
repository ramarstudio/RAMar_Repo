using System;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums; 

namespace AttendanceSystem.Services
{
    public class ReporteService
    {
        private readonly IMarcajeRepository _marcajeRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IHorarioRepository _horarioRepository; 

        public ReporteService(
            IMarcajeRepository marcajeRepository, 
            IEmpleadoRepository empleadoRepository,
            IHorarioRepository horarioRepository)
        {
            _marcajeRepository = marcajeRepository;
            _empleadoRepository = empleadoRepository;
            _horarioRepository = horarioRepository;
        }

        public async Task<ReporteDto> GenerarReporteMensualAsync(int empleadoId, int mes, int anio)
        {
            // 1. Validar empleado
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new ArgumentException("El empleado especificado no existe.");
            }

            var periodoInicio = new DateTime(anio, mes, 1);
            int diasDelMes = DateTime.DaysInMonth(anio, mes);
            var periodoFin = new DateTime(anio, mes, diasDelMes, 23, 59, 59);

            // 2. Traer marcajes y horarios de la base de datos
            var marcajes = await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, periodoInicio, periodoFin);
            var horarios = await _horarioRepository.GetByEmpleadoIdAsync(empleadoId);

            // 3. Calcular métricas básicas
            var entradas = marcajes.Where(m => m.EsEntrada()).ToList();
            int totalAsistencias = entradas.Count;
            int totalTardanzas = entradas.Count(m => m.EsTardanza());
            int sumatoriaTardanzas = entradas.Where(m => m.EsTardanza()).Sum(m => m.GetMinutosTardanza());

            int diasLaborablesReales = 0;

            // Recorremos cada día del mes
            for (int i = 1; i <= diasDelMes; i++)
            {
                var fechaIteracion = new DateTime(anio, mes, i);
                var diaSemanaEnum = ConvertirADiaSemana(fechaIteracion.DayOfWeek);

                var fechaDateOnly = DateOnly.FromDateTime(fechaIteracion);

                // Verificamos si para esa fecha exacta el empleado tenía un horario vigente
                bool debioTrabajar = horarios.Any(h => 
                    h.DiaSemana == diaSemanaEnum && 
                    h.VigenteDesde <= fechaDateOnly && 
                    (h.VigenteHasta == null || h.VigenteHasta >= fechaDateOnly));

                if (debioTrabajar)
                {
                    diasLaborablesReales++;
                }
            }

            // Calculamos faltas reales (Días que debió ir menos los días que sí fue)
            int totalFaltas = Math.Max(0, diasLaborablesReales - totalAsistencias);

            // 5. Retornar DTO
            return new ReporteDto
            {
                EmpleadoId = empleado.GetId(), // O empleado.Id si cambiaste a propiedades
                CodigoEmpleado = empleado.GetCodigo(),
                TotalAsistencias = totalAsistencias,
                TotalTardanzas = totalTardanzas,
                TotalFaltas = totalFaltas,
                SumatoriaMinutosTardanza = sumatoriaTardanzas,
                PeriodoInicio = periodoInicio,
                PeriodoFin = periodoFin
            };
        }

        // --- Método Auxiliar ---
        // Traduce el día del sistema (DayOfWeek de C#) a tu Enum de dominio
        private DiaSemana ConvertirADiaSemana(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => DiaSemana.LUN,
                DayOfWeek.Tuesday => DiaSemana.MAR,
                DayOfWeek.Wednesday => DiaSemana.MIE,
                DayOfWeek.Thursday => DiaSemana.JUE,
                DayOfWeek.Friday => DiaSemana.VIE,
                DayOfWeek.Saturday => DiaSemana.SAB,
                DayOfWeek.Sunday => DiaSemana.DOM,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), $"Día no esperado: {dayOfWeek}")
            };
        }
    }
}