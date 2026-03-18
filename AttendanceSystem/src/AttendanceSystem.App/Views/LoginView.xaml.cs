using System.Windows;
using System.Windows.Controls;
using AttendanceSystem.App.Controllers;

namespace AttendanceSystem.App.Views
{
    public partial class LoginView : UserControl
    {
        private readonly AuthController _authController;

        public LoginView(AuthController authController)
        {
            InitializeComponent();
            _authController = authController;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Ocultamos errores previos y deshabilitamos el botón para evitar doble envío
            txtError.Visibility = Visibility.Collapsed;
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Validando...";

            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Delegamos la validación al controlador
            var resultado = await _authController.LoginAsync(username, password);

            if (!resultado.Exito)
            {
                // Si falla, mostramos el mensaje de error que nos manda el controlador
                txtError.Text = resultado.Mensaje;
                txtError.Visibility = Visibility.Visible;
            }

            // Restauramos el botón
            btnLogin.IsEnabled = true;
            btnLogin.Content = "Ingresar al Sistema";
        }
    }
}