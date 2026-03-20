using System;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AttendanceSystem.Services
{
    public class TardanzaService : ITardanzaService
    {
        private readonly IHorarioRepository     _horarioRepository;
        private readonly ILogger<TardanzaService> _logger;

        public TardanzaService(IHorarioRepository horarioRepository, ILogger<TardanzaService> logger)
        {
            _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
            _logger            = logger            ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool esTardanza, int minutosTarde, DateTime horarioBase)> EvaluarTardanzaAsync(
            Empleado empleado, DateTime horaLlegada, CancellationToken ct = default)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));

            var diaSemana = (DiaSemana)horaLlegada.DayOfWeek;
            var horario   = await _horarioRepository.GetHorarioVigenteAsync(empleado.GetId(), diaSemana);

            if (horario == null)
            {
                _logger.LogDebug("Empleado {Id}: sin horario vigente para {Dia}.", empleado.GetId(), diaSemana);
                return (false, 0, DateTime.MinValue);
            }

            var limitePermitido = horario.GetEntrada().TimeOfDay
                .Add(TimeSpan.FromMinutes(empleado.GetTolerancia()));

            bool esTardanza  = horaLlegada.TimeOfDay > limitePermitido;
            int minutosTarde = 0;

            if (esTardanza)
            {
                minutosTarde = (int)(horaLlegada.TimeOfDay - horario.GetEntrada().TimeOfDay).TotalMinutes;
                _logger.LogInformation("Empleado {Id}: tardanza de {Min} min.", empleado.GetId(), minutosTarde);
            }

            return (esTardanza, minutosTarde, horario.GetEntrada());
        }

        public async Task<bool> EsDiaLaboralAsync(Empleado empleado, DateTime fecha, CancellationToken ct = default)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            var diaSemana = (DiaSemana)fecha.DayOfWeek;
            var horario   = await _horarioRepository.GetHorarioVigenteAsync(empleado.GetId(), diaSemana);
            return horario != null;
        }

        public async Task<Horario> ObtenerHorarioVigenteHoyAsync(Empleado empleado, CancellationToken ct = default)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            var diaSemana = (DiaSemana)DateTime.Now.DayOfWeek;
            return await _horarioRepository.GetHorarioVigenteAsync(empleado.GetId(), diaSemana);
        }
    }
}
