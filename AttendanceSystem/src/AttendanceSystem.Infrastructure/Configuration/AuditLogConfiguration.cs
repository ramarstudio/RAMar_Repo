using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.GetId());

        builder.Property(a => a.GetAccion())
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("accion");

        builder.Property(a => a.GetEntidad())
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("entidad");

        builder.Property(a => a.GetRegistroId())
               .IsRequired()
               .HasColumnName("registro_id");

        builder.Property(a => a.GetDatosAnteriores())
               .HasColumnType("jsonb") // Ideal para PostgreSQL si guardamos JSON
               .HasColumnName("datos_anteriores");

        builder.Property(a => a.GetDatosNuevos())
               .HasColumnType("jsonb")
               .HasColumnName("datos_nuevos");

        builder.Property(a => a.GetMotivo())
               .HasMaxLength(500)
               .HasColumnName("motivo");

        builder.Property(a => a.GetFecha())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha");

        builder.Property(a => a.GetUsuarioId())
               .IsRequired()
               .HasColumnName("usuario_id");

        // Relación con Usuario
        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(a => a.GetUsuarioId())
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_auditlogs_usuario");

        // Índices para búsquedas comunes
        builder.HasIndex(a => a.GetEntidad())
               .HasDatabaseName("idx_auditlogs_entidad");
               
        builder.HasIndex(a => a.GetUsuarioId())
               .HasDatabaseName("idx_auditlogs_usuario_id");
               
        builder.HasIndex(a => a.GetFecha())
               .HasDatabaseName("idx_auditlogs_fecha");
    }
}
