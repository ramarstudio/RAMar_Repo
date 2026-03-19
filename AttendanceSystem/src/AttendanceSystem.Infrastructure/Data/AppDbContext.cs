using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Horario> Horarios { get; set; }
    public DbSet<Consentimiento> Consentimientos { get; set; }
    public DbSet<EmbeddingFacial> EmbeddingsFaciales { get; set; }
    public DbSet<Marcaje> Marcajes { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Configuracion> Configuraciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Acceder a campos privados en lugar de propiedades públicas
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.ApplyConfiguration(new RolConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new EmpleadoConfiguration());
        modelBuilder.ApplyConfiguration(new HorarioConfiguration());
        modelBuilder.ApplyConfiguration(new ConsentimientoConfiguration());
        modelBuilder.ApplyConfiguration(new MarcajeConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionConfiguration());
        modelBuilder.ApplyConfiguration(new EmbeddingFacialConfiguration());
    }
}
