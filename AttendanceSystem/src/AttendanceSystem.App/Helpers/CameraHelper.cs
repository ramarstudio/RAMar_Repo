using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace AttendanceSystem.App.Helpers
{
    public class CameraHelper
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private Bitmap _lastFrame;

        // Evento que la Vista (MarcajeView) escuchará para mostrar el video en vivo
        public event EventHandler<BitmapImage> OnFrameArrived;

        public CameraHelper()
        {
            // Busca todas las cámaras web conectadas a la PC
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public bool HayCamarasDisponibles()
        {
            return _videoDevices != null && _videoDevices.Count > 0;
        }

        public void IniciarCamara(int indiceDispositivo = 0)
        {
            if (!HayCamarasDisponibles())
                throw new InvalidOperationException("No se detectó ninguna cámara web en el equipo.");

            // Detener si ya hay una corriendo
            DetenerCamara();

            // Iniciar la cámara seleccionada
            _videoSource = new VideoCaptureDevice(_videoDevices[indiceDispositivo].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
            _videoSource.Start();
        }

        public void DetenerCamara()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.WaitForStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // 1. Guardamos el último frame capturado (lo clonamos para evitar bloqueos)
            if (_lastFrame != null) _lastFrame.Dispose();
            _lastFrame = (Bitmap)eventArgs.Frame.Clone();

            // 2. Convertimos el formato para que WPF pueda dibujarlo en pantalla
            BitmapImage wpfImage = ConvertirBitmapABitmapImage(_lastFrame);

            // 3. Avisamos a la interfaz gráfica que hay un nuevo cuadro listo.
            // Usamos Dispatcher porque la cámara corre en un hilo secundario y la UI en el principal.
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnFrameArrived?.Invoke(this, wpfImage);
            });
        }

        // Este es el método que usará tu BiometricoController para mandar la foto a Python
        public string CapturarFrameEnBase64()
        {
            if (_lastFrame == null) return string.Empty;

            using (var ms = new MemoryStream())
            {
                // Guardamos el frame actual en memoria con formato JPEG para comprimirlo
                // y evitar enviar un Base64 absurdamente grande a Python
                lock (_lastFrame) 
                {
                    using (var bitmapClone = new Bitmap(_lastFrame))
                    {
                        bitmapClone.Save(ms, ImageFormat.Jpeg);
                    }
                }
                
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        // Convierte System.Drawing.Bitmap (AForge) a System.Windows.Media.Imaging.BitmapImage (WPF)
        private BitmapImage ConvertirBitmapABitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Vital para prevenir errores al cruzar hilos (Thread-safe)
                
                return bitmapImage;
            }
        }
    }
}