using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AttendanceSystem.App.Interfaces
{
    // Abstracción de la cámara + verificación facial.
    // Permite inyectar un mock en tests sin inicializar hardware real.
    public interface IBiometricoController
    {
        Task IniciarCamaraAsync(EventHandler<BitmapSource> onFrameArrivedCallback);
        void ApagarCamara(EventHandler<BitmapSource> onFrameArrivedCallback);
        Task<(bool Exito, string Mensaje)> VerificarRostroAsync(string codigoEmpleado);
        string CapturarFrameActual();
    }
}
