using System;
using System.Windows.Controls;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.App.Views;
using AttendanceSystem.App.Views.Admin;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.App.Controllers
{
    // Ruta de navegación basada en Func<T> en lugar de IServiceProvider.
    // Esto elimina el anti-patrón Service Locator y hace el router testeable.
    public class NavigationController
    {
        private readonly ISessionManager    _sessionManager;
        private readonly NavigationHelper   _navigationHelper;
        private readonly Func<LoginView>    _loginViewFactory;
        private readonly Func<AdminShellView> _adminShellViewFactory;
        private readonly Func<MarcajeView>  _marcajeViewFactory;

        public NavigationController(
            ISessionManager      sessionManager,
            NavigationHelper     navigationHelper,
            Func<LoginView>      loginViewFactory,
            Func<AdminShellView> adminShellViewFactory,
            Func<MarcajeView>    marcajeViewFactory)
        {
            _sessionManager        = sessionManager;
            _navigationHelper      = navigationHelper;
            _loginViewFactory      = loginViewFactory;
            _adminShellViewFactory = adminShellViewFactory;
            _marcajeViewFactory    = marcajeViewFactory;
        }

        public void InicializarContenedor(ContentControl container)
            => _navigationHelper.Inicializar(container);

        public void NavegarALogin()
            => _navigationHelper.CambiarVista(_loginViewFactory());

        public void NavegarAlMenuPrincipal()
        {
            if (!_sessionManager.EstaLogueado()) return;

            if (_sessionManager.EsAdministrador() || _sessionManager.EsRRHH())
                _navigationHelper.CambiarVista(_adminShellViewFactory());
            else
                _navigationHelper.CambiarVista(_marcajeViewFactory());
        }
    }
}
