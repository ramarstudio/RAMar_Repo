using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace AttendanceSystem.App.Helpers
{
    public class CameraHelper : IDisposable
    {
        // volatile garantiza que el flag sea visible entre hilos sin necesidad de lock
        private volatile bool _running;
        private volatile bool _disposed;

        private VideoCapture _capture;
        private Thread _captureThread;
        private Mat _lastFrame = new Mat();
        private readonly object _frameLock = new object();

        public event EventHandler<BitmapSource> OnFrameArrived;

        public bool HayCamarasDisponibles()
        {
            try
            {
                using var test = new VideoCapture(0);
                return test.IsOpened();
            }
            catch { return false; }
        }

        public async System.Threading.Tasks.Task IniciarCamaraAsync(int index = 0)
        {
            DetenerCamara();

            // Mover la inicialización de hardware (DShow) a un hilo secundario.
            // Esto previene los 5 segundos de congelamiento de la ventana de WPF.
            var cap = await System.Threading.Tasks.Task.Run(() => new VideoCapture(index));
            if (!cap.IsOpened())
            {
                cap.Dispose();
                throw new InvalidOperationException("No se pudo abrir la cámara. Verifique que esté conectada.");
            }

            _capture = cap;
            _running = true;

            _captureThread = new Thread(CaptureLoop)
            {
                IsBackground = true,
                Name = "CameraCapture"
            };
            _captureThread.Start();
        }

        private void CaptureLoop()
        {
            using var frame = new Mat();

            while (_running)
            {
                try
                {
                    // Copia local del puntero: evita race condition si DetenerCamara
                    // nulifica _capture mientras este hilo está leyendo.
                    var cap = _capture;
                    if (cap == null || !cap.Read(frame) || frame.Empty())
                    {
                        Thread.Sleep(33);
                        continue;
                    }

                    lock (_frameLock)
                    {
                        frame.CopyTo(_lastFrame);
                    }

                    var bmp = MatToBitmapSource(frame);
                    Application.Current?.Dispatcher.BeginInvoke(
                        () => OnFrameArrived?.Invoke(this, bmp));

                    Thread.Sleep(33); // ~30 fps
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void DetenerCamara()
        {
            _running = false;

            // Esperar a que el thread termine antes de liberar _capture
            _captureThread?.Join(500);
            _captureThread = null;

            // Snapshot del puntero antes de nulificar: garantiza que CaptureLoop
            // no acceda a un objeto ya dispuesto.
            var cap = _capture;
            _capture = null;
            cap?.Release();
            cap?.Dispose();
        }

        // Captura el frame actual como JPEG en Base64 para enviar al microservicio Python.
        // Se llama solo al momento del marcaje, no en cada frame del preview.
        public string CapturarFrameEnBase64()
        {
            lock (_frameLock)
            {
                if (_lastFrame == null || _lastFrame.Empty()) return string.Empty;
                Cv2.ImEncode(".jpg", _lastFrame, out var buffer,
                    new ImageEncodingParam(ImwriteFlags.JpegQuality, 75));
                return Convert.ToBase64String(buffer);
            }
        }

        private static BitmapSource MatToBitmapSource(Mat mat)
        {
            // [Optimización Extrema de Memoria y CPU]
            // Leemos los bytes directamente desde la memoria no administrada de OpenCV (puntero IntPtr).
            // Esto evita la recodificación a .bmp (Cv2.ImEncode) que consume muchísima CPU.
            // Además, evita instanciar arreglos de bytes y MemoryStreams 30 veces por segundo,
            // aliviando drásticamente el Garbage Collector (Gen 0) y eliminando futuros stutters.
            
            int width = mat.Width;
            int height = mat.Height;
            int stride = (int)mat.Step(); // Bytes por fila
            int bufferSize = height * stride;

            var bmp = BitmapSource.Create(
                width,
                height,
                96, // DPI X estándar
                96, // DPI Y estándar
                System.Windows.Media.PixelFormats.Bgr24, // Las webcams con OpenCV usan 24-bits (BGR)
                null,
                mat.Data,
                bufferSize,
                stride);

            bmp.Freeze(); // Sigue siendo obligatorio congelar para usar cross-thread
            return bmp;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            DetenerCamara();
            lock (_frameLock)
            {
                _lastFrame?.Dispose();
                _lastFrame = null;
            }
        }
    }
}
