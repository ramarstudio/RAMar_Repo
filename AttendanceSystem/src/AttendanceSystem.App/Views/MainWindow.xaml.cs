using System.Windows;
using AttendanceSystem.App.Controllers;

namespace AttendanceSystem.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(NavigationController navigationController)
        {
            InitializeComponent();
            
            // 1. Le pasamos el contenedor visual al controlador de navegación
            navigationController.InicializarContenedor(MainContainer);
            
            // 2. Le decimos a la app que, apenas arranque, nos lleve a la pantalla de Login
            navigationController.NavegarALogin();
        }
    }
}