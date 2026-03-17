using System.Threading.Tasks;

namespace AttendanceSystem.Core.Interfaces
{
    public interface IBiometricoService
    {
        // Se comunica con tu API en Python enviando el Base64 y el ID esperado
        Task<bool> VerificarIdentidadAsync(string base64Image, string codigoEmpleado);
        
        // Útil para cuando el empleado se registra por primera vez
        Task<bool> RegistrarNuevoRostroAsync(string base64Image, string codigoEmpleado);
    }
}