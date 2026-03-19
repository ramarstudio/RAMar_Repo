using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;
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
        private IConfiguration _configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Compatibilidad Npgsql: permite escribir DateTime(Kind=UTC) en columnas timestamp without time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // 0. Cargar appsettings.json para toda la aplicación
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 1. Crear el "Contenedor" donde guardaremos todas nuestras piezas
            var services = new ServiceCollection();
            ConfigureServices(services);

            // 2. Construir el proveedor de servicios
            _serviceProvider = services.BuildServiceProvider();

            // 3. Inicializar base de datos y sembrar datos por defecto
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                context.Database.EnsureCreated();
                SeedData(context, hasher);
            }

            // 4. ¡Pedirle al contenedor que nos dé el MainWindow ya armado y mostrarlo!
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ==========================================
            // A. CAPA DE SEGURIDAD Y CONFIGURACIÓN GLOBAL
            // ==========================================
            // Registrar las opciones de sesión para que puedan ser inyectadas en SessionManager
            // Usamos los valores por defecto definidos en el constructor de SessionOptions
            services.AddSingleton(new SessionOptions());
            services.AddSingleton<SessionManager>(); // Singleton: Solo hay una sesión en toda la app
            services.AddSingleton<HttpClient>(); // HttpClient debe ser Singleton por buenas prácticas
            
          
            // ==========================================
            // B. CAPA DE INFRAESTRUCTURA (REPOSITORIOS / DB)
            // ==========================================
            // Lee la cadena de conexión desde appsettings.json
            // Si hay una variable de entorno AS_DB_CONNECTION, tiene prioridad (útil en servidores)
            var connectionString = Environment.GetEnvironmentVariable("AS_DB_CONNECTION")
                ?? _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Repositorios: Scoped es la opción recomendada para DbContext-based services
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
            services.AddScoped<IMarcajeRepository, MarcajeRepository>();
            services.AddScoped<IConsentimientoRepository, ConsentimientoRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IHorarioRepository, HorarioRepository>();

            // ==========================================
            // C. CAPA DE SERVICIOS (LÓGICA DE NEGOCIO)
            // ==========================================
            services.AddTransient<IAuthService, AuthService>();
            // Lee el pepper de hash desde appsettings.json (sección Security.HashPepper)
            var pepper = Environment.GetEnvironmentVariable("AS_HASH_PEPPER")
                ?? _configuration["Security:HashPepper"]
                ?? "ZGV2X3BlcHBlcl9zYW1wbGU=";
            services.AddSingleton(new HashingOptions(pepper));
            services.AddTransient<IPasswordHasher, PasswordHasher>();
            // Lee la URL del microservicio Python desde appsettings.json (sección FacialService.BaseUrl)
            var facialBaseUrl = _configuration["FacialService:BaseUrl"] ?? "http://localhost:5000";
            services.AddSingleton(new HttpClient { BaseAddress = new Uri(facialBaseUrl) });
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
            services.AddTransient<MarcajeController>();
            services.AddTransient<HistorialController>();

            // Vistas (Pantallas)
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<MarcajeView>();
            services.AddTransient<HistorialView>();
            
            services.AddSingleton<IServiceProvider>(sp => sp);
        }

        private void SeedData(AppDbContext context, IPasswordHasher hasher)
        {
            // Sembrar roles si no existen
            if (!context.Roles.Any())
            {
                var rolAdmin = new Rol();
                rolAdmin.SetNombre(RolUsuario.Admin);
                rolAdmin.SetDescripcion("Administrador del sistema");

                var rolRRHH = new Rol();
                rolRRHH.SetNombre(RolUsuario.RRHH);
                rolRRHH.SetDescripcion("Recursos Humanos");

                var rolEmpleado = new Rol();
                rolEmpleado.SetNombre(RolUsuario.Empleado);
                rolEmpleado.SetDescripcion("Empleado regular");

                context.Roles.AddRange(rolAdmin, rolRRHH, rolEmpleado);
                context.SaveChanges();
            }

            // Sembrar usuario admin por defecto si no existe
            if (!context.Usuarios.Any())
            {
                var rolAdmin = context.Roles.AsEnumerable().First(r => r.GetNombre() == RolUsuario.Admin);

                var admin = new Usuario();
                admin.SetUsername("admin");
                admin.SetPassword(hasher.HashPassword("admin123"));
                admin.SetNombre("Administrador");
                admin.SetActivo(true);
                admin.SetFechaCreacion(DateTime.UtcNow);
                admin.SetRol(rolAdmin);

                context.Usuarios.Add(admin);
                context.SaveChanges();
            }
        }
    }
}