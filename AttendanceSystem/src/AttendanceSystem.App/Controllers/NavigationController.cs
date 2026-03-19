using System;
using System.Windows.Controls;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.Security;
using AttendanceSystem.App.Views;
using AttendanceSystem.App.Views.Admin;

namespace AttendanceSystem.App.Controllers
{
    public class NavigationController
    {
        private readonly SessionManager _sessionManager;
        private readonly NavigationHelper _navigationHelper;
        private readonly IServiceProvider _serviceProvider; 

        public NavigationController(
            SessionManager sessionManager, 
            NavigationHelper navigationHelper,
            IServiceProvider serviceProvider)
        {
            _sessionManager = sessionManager;
            _navigationHelper = navigationHelper;
            _serviceProvider = serviceProvider;
        }

        public void InicializarContenedor(ContentControl container)
        {
            _navigationHelper.Inicializar(container);
        }

        public void NavegarALogin()
        {
            // Usamos el ServiceProvider para que resuelva la vista y sus controladores automáticamente
            var loginView = (LoginView)_serviceProvider.GetService(typeof(LoginView));
            _navigationHelper.CambiarVista(loginView);
        }

        public void NavegarAlMenuPrincipal()
        {
            if (!_sessionManager.EstaLogueado()) return;

            if (_sessionManager.EsAdministrador())
            {
                var adminShell = (AdminShellView)_serviceProvider.GetService(typeof(AdminShellView));
                _navigationHelper.CambiarVista(adminShell);
            }
            else
            {
                var marcajeView = (MarcajeView)_serviceProvider.GetService(typeof(MarcajeView));
                _navigationHelper.CambiarVista(marcajeView);
            }
        }
    }
}