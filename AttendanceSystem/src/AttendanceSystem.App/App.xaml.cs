using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Core.Options;
using AttendanceSystem.Services;
using AttendanceSystem.Security;
using AttendanceSystem.App.Controllers;
using AttendanceSystem.App.Controllers.Admin;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.App.Interfaces;
using AttendanceSystem.App.Views;
using AttendanceSystem.App.Views.Admin;

namespace AttendanceSystem.App
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private IConfiguration   _configuration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Compatibilidad Npgsql: permite DateTime(Kind=UTC) en timestamp without time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<App>(optional: true)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                context.Database.EnsureCreated();
                SeedData(context, hasher);
            }

            _serviceProvider.GetRequiredService<MainWindow>().Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ── Logging ──────────────────────────────────────────────────────
            services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

            // ── Seguridad y sesión ────────────────────────────────────────────
            services.AddSingleton(new SessionOptions());
            // Registrado como ISessionManager para que todos los controllers dependan de la abstracción
            services.AddSingleton<ISessionManager, SessionManager>();

            // ── Base de datos ─────────────────────────────────────────────────
            var connectionString = Environment.GetEnvironmentVariable("AS_DB_CONNECTION")
                ?? _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            // ── Repositorios ──────────────────────────────────────────────────
            services.AddScoped<IUsuarioRepository,       UsuarioRepository>();
            services.AddScoped<IEmpleadoRepository,      EmpleadoRepository>();
            services.AddScoped<IMarcajeRepository,       MarcajeRepository>();
            services.AddScoped<IConsentimientoRepository, ConsentimientoRepository>();
            services.AddScoped<IAuditRepository,         AuditRepository>();
            services.AddScoped<IHorarioRepository,       HorarioRepository>();

            // ── Opciones de configuración ──────────────────────────────────────
            var encryptionKey = Environment.GetEnvironmentVariable("AS_ENCRYPTION_KEY")
                ?? _configuration["Security:EncryptionKey"];
            var hmacKey = Environment.GetEnvironmentVariable("AS_HMAC_KEY")
                ?? _configuration["Security:HmacKey"];
            services.AddSingleton(new EncryptionOptions(encryptionKey, hmacKey));

            var pepper = Environment.GetEnvironmentVariable("AS_HASH_PEPPER")
                ?? _configuration["Security:HashPepper"]
                ?? "ZGV2X3BlcHBlcl9zYW1wbGU=";
            services.AddSingleton(new HashingOptions(pepper));

            // FacialServiceOptions — URL relativa leída desde appsettings (elimina hardcoded)
            var facialBaseUrl   = _configuration["FacialService:BaseUrl"]   ?? "http://localhost:5001";
            var facialVerifyPath = _configuration["FacialService:VerifyPath"] ?? "/api/verify";
            var facialEncodePath = _configuration["FacialService:EncodePath"] ?? "/api/encode";
            services.AddSingleton(new FacialServiceOptions(facialVerifyPath, facialEncodePath));
            services.AddSingleton(new HttpClient { BaseAddress = new Uri(facialBaseUrl) });

            // EmpleadoDefaultOptions — valores por defecto al crear empleados
            int entradaHora   = int.TryParse(_configuration["Empleado:HorarioEntradaHora"], out var eh) ? eh : 8;
            int salidaHora    = int.TryParse(_configuration["Empleado:HorarioSalidaHora"],  out var sh) ? sh : 17;
            int toleranciaMins = int.TryParse(_configuration["Empleado:ToleranciaMins"],    out var tm) ? tm : 15;
            services.AddSingleton(new EmpleadoDefaultOptions(entradaHora, salidaHora, toleranciaMins));

            // ExportOptions — carpeta de exportación configurable
            var carpetaExport = _configuration["Exportacion:Carpeta"] ?? "Exportaciones";
            services.AddSingleton(new ExportOptions(carpetaExport));

            // ── Servicios de negocio ───────────────────────────────────────────
            services.AddTransient<IAuthService,    AuthService>();
            services.AddTransient<IMarcajeService, MarcajeService>();
            services.AddTransient<IEncryptionService, EncryptionService>();
            services.AddTransient<IPasswordHasher,    PasswordHasher>();
            services.AddTransient<IBiometricoService, BiometricoService>();
            services.AddTransient<ITardanzaService,   TardanzaService>();
            services.AddTransient<IReporteService,    ReporteService>();
            services.AddTransient<IExportService,     ExportService>();
            services.AddTransient<IEmpleadoSelectorService, EmpleadoSelectorService>();
            services.AddTransient<AuditService>();

            // ── Capa de presentación ──────────────────────────────────────────
            services.AddSingleton<NavigationHelper>();
            services.AddTransient<CameraHelper>();

            // IBiometricoController — abstracción para MarcajeController
            services.AddTransient<IBiometricoController, BiometricoController>();

            // Factories Func<T> para NavigationController — elimina Service Locator
            services.AddSingleton<Func<LoginView>>(sp      => () => sp.GetRequiredService<LoginView>());
            services.AddSingleton<Func<AdminShellView>>(sp => () => sp.GetRequiredService<AdminShellView>());
            services.AddSingleton<Func<MarcajeView>>(sp    => () => sp.GetRequiredService<MarcajeView>());

            // Controladores
            services.AddSingleton<NavigationController>();
            services.AddTransient<AuthController>();
            services.AddTransient<MarcajeController>();
            services.AddTransient<HistorialController>();

            // Controladores admin
            services.AddTransient<DashboardController>();
            services.AddTransient<UsuariosController>();
            services.AddTransient<MarcajesAdminController>();
            services.AddTransient<ReportesController>();

            // Vistas
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<MarcajeView>();
            services.AddTransient<HistorialView>();
            services.AddTransient<AdminShellView>();
        }

        private void SeedData(AppDbContext context, IPasswordHasher hasher)
        {
            if (!context.Roles.Any())
            {
                var rolAdmin    = new Rol(); rolAdmin.SetNombre(RolUsuario.Admin);    rolAdmin.SetDescripcion("Administrador del sistema");
                var rolRRHH     = new Rol(); rolRRHH.SetNombre(RolUsuario.RRHH);      rolRRHH.SetDescripcion("Recursos Humanos");
                var rolEmpleado = new Rol(); rolEmpleado.SetNombre(RolUsuario.Empleado); rolEmpleado.SetDescripcion("Empleado regular");
                context.Roles.AddRange(rolAdmin, rolRRHH, rolEmpleado);
                context.SaveChanges();
            }

            if (!context.Usuarios.Any())
            {
                var rolAdmin = context.Roles.AsEnumerable().First(r => r.GetNombre() == RolUsuario.Admin);
                var admin    = new Usuario();
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
