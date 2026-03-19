using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Services
{
    public class AuditService
    {
        private readonly IAuditRepository _auditRepository;

        //Inyección de dependencias
        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task RegistrarAsync(string accion, string entidad, int registroId, int usuarioId, string anterior, string nuevo, string motivo)
        {
            //Usamos tu método de fábrica estático para crear el log inmutable
            var log = AuditLog.Registrar(
                accion: accion,
                entidad: entidad,
                registroId: registroId,
                usuarioId: usuarioId,
                datosAnteriores: anterior,
                datosNuevos: nuevo,
                motivo: motivo
            );
            
            //Guardamos en base de datos a través del repositorio
            await _auditRepository.AddAsync(log);
        }
    }
}