using System;
using System.Threading.Tasks;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    /// <summary>
    /// Servicio especializado en el cálculo de tardanzas.
    /// Centraliza toda la lógica de comparación de horarios para
    /// que MarcajeService y otros servicios no repitan este código.
    ///
    /// No persiste en BD — solo calcula y retorna resultados.
    /// </summary>
    public class TardanzaService
    {
        // ── DEPENDENCIA ──────────────────────────────────────────────────────
        // Necesita el repositorio de horarios para consultar el horario vigente.
        private readonly IHorarioRepository _horarioRepository;

        // ── CONSTRUCTOR ──────────────────────────────────────────────────────
        public TardanzaService(IHorarioRepository horarioRepository)
        {
            _horarioRepository = horarioRepository
                ?? throw new ArgumentNullException(nameof(horarioRepository));
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 1: EvaluarTardanzaAsync
        // Método principal: determina si un empleado llegó tarde según su
        // horario vigente del día y su tolerancia configurada.
        //
        // Retorna una tupla con:
        //   esTardanza     → true si superó horario + tolerancia
        //   minutosTarde   → cuántos minutos tarde llegó (0 si es puntual)
        //   horarioBase    → la hora de entrada esperada (para mostrar en UI)
        // ════════════════════════════════════════════════════════════════════
        public async Task<(bool esTardanza, int minutosTarde, DateTime horarioBase)> EvaluarTardanzaAsync(
            Empleado empleado,
            DateTime horaLlegada)
        {
            if (empleado == null)
                throw new ArgumentNullException(nameof(empleado));

            // Convertir el día de la semana actual al enum DiaSemana del sistema.
            var diaSemanaActual = (DiaSemana)horaLlegada.DayOfWeek;

            // Buscar el horario vigente del empleado para este día.
            var horario = await _horarioRepository.GetHorarioVigenteAsync(
                empleado.GetId(),
                diaSemanaActual);

            // Si no tiene horario asignado para hoy, no aplica tardanza.
            if (horario == null)
            {
                return (esTardanza: false, minutosTarde: 0, horarioBase: DateTime.MinValue);
            }

            // Calcular el límite máximo permitido: hora de entrada + tolerancia.
            var limitePermitido = horario.GetEntrada().TimeOfDay
                .Add(TimeSpan.FromMinutes(empleado.GetTolerancia()));

            bool esTardanza = horaLlegada.TimeOfDay > limitePermitido;

            int minutosTarde = 0;
            if (esTardanza)
            {
                var diferencia = horaLlegada.TimeOfDay - horario.GetEntrada().TimeOfDay;
                minutosTarde = (int)diferencia.TotalMinutes;
            }

            return (esTardanza, minutosTarde, horario.GetEntrada());
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 2: EsDiaLaboralAsync
        // Verifica si un empleado tiene horario asignado en una fecha dada.
        // Útil para saber si una ausencia cuenta como falta o es día libre.
        // ════════════════════════════════════════════════════════════════════
        public async Task<bool> EsDiaLaboralAsync(Empleado empleado, DateTime fecha)
        {
            if (empleado == null)
                throw new ArgumentNullException(nameof(empleado));

            var diaSemana = (DiaSemana)fecha.DayOfWeek;

            var horario = await _horarioRepository.GetHorarioVigenteAsync(
                empleado.GetId(),
                diaSemana);

            // Tiene horario vigente ese día → es día laboral.
            return horario != null;
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO 3: ObtenerHorarioVigenteHoy
        // Devuelve el horario vigente del empleado para el día de hoy.
        // Centraliza esta lógica que se repite en varios servicios.
        // ════════════════════════════════════════════════════════════════════
        public async Task<Horario> ObtenerHorarioVigenteHoyAsync(Empleado empleado)
        {
            if (empleado == null)
                throw new ArgumentNullException(nameof(empleado));

            var diaSemanaActual = (DiaSemana)DateTime.Now.DayOfWeek;

            return await _horarioRepository.GetHorarioVigenteAsync(
                empleado.GetId(),
                diaSemanaActual);
        }
    }
}
