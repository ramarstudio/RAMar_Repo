using System;

namespace AttendanceSystem.Core.Options
{
    public sealed class ExportOptions
    {
        public string Carpeta { get; }

        public ExportOptions(string carpeta = "Exportaciones")
        {
            if (string.IsNullOrWhiteSpace(carpeta))
                throw new ArgumentException("La carpeta de exportación no puede estar vacía.", nameof(carpeta));

            Carpeta = carpeta;
        }
    }
}
