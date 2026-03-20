using System.Text.Json;
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
            // jsonb requiere JSON válido — envolvemos strings planos como JSON string
            static string ToJson(string s) => s == null ? null : JsonSerializer.Serialize(s);

            var log = AuditLog.Registrar(
                accion: accion,
                entidad: entidad,
                registroId: registroId,
                usuarioId: usuarioId,
                datosAnteriores: ToJson(anterior),
                datosNuevos: ToJson(nuevo),
                motivo: motivo
            );
            
            //Guardamos en base de datos a través del repositorio
            await _auditRepository.AddAsync(log);
        }
    }
}