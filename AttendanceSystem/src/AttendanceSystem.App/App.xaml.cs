using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;
using AttendanceSystem.Security;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.App.Views;



namespace AttendanceSystem.App
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Crear el "Contenedor" donde guardaremos todas nuestras piezas
            var services = new ServiceCollection();
            ConfigureServices(services);

            // 2. Construir el proveedor de servicios
            _serviceProvider = services.BuildServiceProvider();

            // 3. ¡Pedirle al contenedor que nos dé el MainWindow ya armado y mostrarlo!
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ==========================================
            // A. CAPA DE SEGURIDAD Y CONFIGURACIÓN GLOBAL
            // ==========================================
            services.AddSingleton<SessionManager>(); // Singleton: Solo hay una sesión en toda la app
            services.AddSingleton<HttpClient>(); // HttpClient debe ser Singleton por buenas prácticas
            
          
            // ==========================================
            // B. CAPA DE INFRAESTRUCTURA (REPOSITORIOS)
            // ==========================================
            // Usamos Scoped o Transient para que se conecten a la BD cuando sea necesario
            // services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            // services.AddTransient<IEmpleadoRepository, EmpleadoRepository>();
            // services.AddTransient<IMarcajeRepository, MarcajeRepository>();
            // services.AddTransient<IConsentimientoRepository, ConsentimientoRepository>();
            // services.AddTransient<IAuditRepository, AuditRepository>();
            // services.AddTransient<IHorarioRepository, HorarioRepository>();

            // ==========================================
            // C. CAPA DE SERVICIOS (LÓGICA DE NEGOCIO)
            // ==========================================
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IBiometricoService, BiometricoService>();
            services.AddTransient<AuditService>();
            services.AddTransient<ReporteService>();
            services.AddTransient<ExportService>();

            // ==========================================
            // D. CAPA DE PRESENTACIÓN (APP)
            // ==========================================
            // Helpers
            services.AddSingleton<NavigationHelper>();
            services.AddTransient<CameraHelper>();

            // Controladores
            services.AddSingleton<NavigationController>(); // Singleton para que todos usen el mismo ruteador
            services.AddTransient<AuthController>();
            services.AddTransient<BiometricoController>();

            // Vistas (Pantallas)
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            
            services.AddSingleton<IServiceProvider>(sp => sp); 
        }
    }
}