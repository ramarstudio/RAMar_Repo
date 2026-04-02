using System.Threading;
using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    /// <summary>
    /// Abstracción del ciclo de vida del microservicio de reconocimiento facial.
    /// Permite que la capa Services arranque/detenga el servicio Python
    /// sin acoplarse a la implementación concreta (FaceServiceManager).
    /// </summary>
    public interface IFaceServiceLifecycle
    {
        /// <summary>
        /// Asegura que el servicio esté corriendo y listo para recibir peticiones.
        /// Si ya está activo, solo hace health check. Si no, lo arranca.
        /// </summary>
        Task<bool> EnsureRunningAsync(CancellationToken ct = default);

        /// <summary>
        /// Marca actividad — resetea el timer de auto-stop por inactividad.
        /// </summary>
        void Touch();

        /// <summary>
        /// Indica si el servicio está corriendo actualmente.
        /// </summary>
        bool IsRunning { get; }
    }
}
