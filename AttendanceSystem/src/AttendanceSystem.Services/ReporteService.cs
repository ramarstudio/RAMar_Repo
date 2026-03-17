using System;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    public class ReporteService
    {
        private readonly IMarcajeRepository _marcajeRepository;
        private readonly IEmpleadoRepository _empleadoRepository;

        // Inyección de dependencias
        public ReporteService(IMarcajeRepository marcajeRepository, IEmpleadoRepository empleadoRepository)
        {
            _marcajeRepository = marcajeRepository;
            _empleadoRepository = empleadoRepository;
        }

        public async Task<ReporteDto> GenerarReporteMensualAsync(int empleadoId, int mes, int anio)
        {
            // 1. Validar que el empleado exista y obtener su código
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
            {
                throw new ArgumentException("El empleado especificado no existe.");
            }

            // 2. Definir el rango de fechas para la consulta (primer y último día del mes)
            var periodoInicio = new DateTime(anio, mes, 1);
            int diasDelMes = DateTime.DaysInMonth(anio, mes);
            var periodoFin = new DateTime(anio, mes, diasDelMes, 23, 59, 59);

            // 3. Obtener el historial de marcajes del repositorio
            var marcajes = await _marcajeRepository.GetByEmpleadoIdAsync(empleadoId, periodoInicio, periodoFin);

            // 4. Procesar la lógica de negocio (Filtramos solo por "Entradas" para evitar contar salidas como días extra)
            var entradas = marcajes.Where(m => m.EsEntrada()).ToList();

            int totalAsistencias = entradas.Count;
            int totalTardanzas = entradas.Count(m => m.EsTardanza());
            int sumatoriaTardanzas = entradas.Where(m => m.EsTardanza()).Sum(m => m.GetMinutosTardanza());

            // Nota: Para un cálculo de faltas 100% real, aquí podrías inyectar IHorarioRepository 
            // para saber exactamente cuántos días le tocaba venir. Por ahora usamos un estimado estándar (ej. 22 días laborables).
            int diasLaborablesEstimados = 22;
            int totalFaltas = Math.Max(0, diasLaborablesEstimados - totalAsistencias);

            // 5. Ensamblar y retornar el DTO
            return new ReporteDto
            {
                EmpleadoId = empleado.GetId(),
                CodigoEmpleado = empleado.GetCodigo(),
                TotalAsistencias = totalAsistencias,
                TotalTardanzas = totalTardanzas,
                TotalFaltas = totalFaltas,
                SumatoriaMinutosTardanza = sumatoriaTardanzas,
                PeriodoInicio = periodoInicio,
                PeriodoFin = periodoFin
            };
        }
    }
}