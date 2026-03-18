using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AttendanceSystem.App.Helpers
{
    public class NavigationHelper
    {
        private ContentControl _mainContainer;

        // Se llama una sola vez al arrancar la app para vincular el contenedor
        public void Inicializar(ContentControl container)
        {
            _mainContainer = container;
        }

        // Método encargado de inyectar la nueva vista con una transición elegante
        public void CambiarVista(UserControl nuevaVista)
        {
            if (_mainContainer == null) return;

            // 1. Creamos la animación de salida (desvanece la vista actual)
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            
            // 2. Cuando termina de desvanecerse, cambiamos el contenido y hacemos la animación de entrada
            fadeOut.Completed += (s, e) =>
            {
                _mainContainer.Content = nuevaVista;
                
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
                _mainContainer.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            // 3. Iniciamos el proceso
            _mainContainer.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}