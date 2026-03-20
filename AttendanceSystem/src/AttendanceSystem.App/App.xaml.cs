using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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

            // LiveCharts2: inicializar antes de mostrar cualquier ventana
            LiveCharts.Configure(config => config
                .AddSkiaSharp()
                .AddDefaultMappers()
                .AddLightTheme());

            // Compatibilidad Npgsql: permite DateTime(Kind=UTC) en timestamp without time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<App>(optional: true)
                .Build();

            // 1. Crear/migrar la BD y obtener las claves de seguridad persistidas
            var connectionString = Environment.GetEnvironmentVariable("AS_DB_CONNECTION")
                ?? _configuration.GetConnectionString("DefaultConnection");
            var securityKeys = InitSecurityKeys(connectionString);

            // 2. Configurar DI con las claves de la BD
            var services = new ServiceCollection();
            ConfigureServices(services, connectionString, securityKeys);
            _serviceProvider = services.BuildServiceProvider();

            // 3. Seed de roles y usuario admin
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hasher  = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                SeedData(context, hasher);
            }

            _serviceProvider.GetRequiredService<MainWindow>().Show();
        }

        /// <summary>
        /// Lee las claves de seguridad de la tabla configuraciones.
        /// Si no existen, las genera y las guarda. Así nunca dependen del appsettings.json.
        /// </summary>
        private SecurityKeys InitSecurityKeys(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new AppDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            string pepper        = GetOrCreateConfig(context, "Security:HashPepper",    "Pepper para hashing de contraseñas",  () => GenerateRandomBase64(32));
            string encryptionKey = GetOrCreateConfig(context, "Security:EncryptionKey", "Clave AES-256 para cifrado",          () => GenerateRandomBase64(32));
            string hmacKey       = GetOrCreateConfig(context, "Security:HmacKey",       "Clave HMAC-SHA256 para integridad",   () => GenerateRandomBase64(32));

            return new SecurityKeys(pepper, encryptionKey, hmacKey);
        }

        private string GetOrCreateConfig(AppDbContext context, string clave, string descripcion, Func<string> generarValor)
        {
            var config = context.Configuraciones
                .AsEnumerable()
                .FirstOrDefault(c => c.GetClave() == clave);

            if (config != null)
                return config.GetValor();

            // No existe → generar y persistir
            string valor = generarValor();
            var nueva = new Configuracion();
            nueva.SetClave(clave);
            nueva.SetValor(valor);
            nueva.SetTipoDato("string");
            nueva.SetDescripcion(descripcion);
            context.Configuraciones.Add(nueva);
            context.SaveChanges();

            return valor;
        }

        private static string GenerateRandomBase64(int sizeBytes)
        {
            byte[] buffer = new byte[sizeBytes];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(buffer); }
            return Convert.ToBase64String(buffer);
        }

        private void ConfigureServices(IServiceCollection services, string connectionString, SecurityKeys keys)
        {
            // ── Logging ──────────────────────────────────────────────────────
            services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug));

            // ── Seguridad y sesión ────────────────────────────────────────────
            services.AddSingleton(new SessionOptions());
            services.AddSingleton<ISessionManager, SessionManager>();

            // ── Base de datos ─────────────────────────────────────────────────
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            // ── Repositorios ──────────────────────────────────────────────────
            services.AddScoped<IUsuarioRepository,       UsuarioRepository>();
            services.AddScoped<IEmpleadoRepository,      EmpleadoRepository>();
            services.AddScoped<IMarcajeRepository,       MarcajeRepository>();
            services.AddScoped<IConsentimientoRepository, ConsentimientoRepository>();
            services.AddScoped<IAuditRepository,         AuditRepository>();
            services.AddScoped<IHorarioRepository,       HorarioRepository>();

            // ── Claves de seguridad (desde la BD, no desde appsettings) ───────
            services.AddSingleton(new EncryptionOptions(keys.EncryptionKey, keys.HmacKey));
            services.AddSingleton(new HashingOptions(keys.Pepper));

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
            services.AddTransient<AuditoriaController>();
            services.AddTransient<HorariosController>();
            services.AddTransient<ConfiguracionController>();

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

        /// <summary>DTO interno para transportar las 3 claves desde InitSecurityKeys a ConfigureServices.</summary>
        private sealed class SecurityKeys
        {
            public string Pepper { get; }
            public string EncryptionKey { get; }
            public string HmacKey { get; }

            public SecurityKeys(string pepper, string encryptionKey, string hmacKey)
            {
                Pepper        = pepper;
                EncryptionKey = encryptionKey;
                HmacKey       = hmacKey;
            }
        }
    }
}
