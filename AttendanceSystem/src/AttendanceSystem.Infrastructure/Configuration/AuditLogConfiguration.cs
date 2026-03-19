using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey("id");

        builder.Property<string>("accion")
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("accion");

        builder.Property<string>("entidad")
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("entidad");

        builder.Property<int>("registroId")
               .IsRequired()
               .HasColumnName("registro_id");

        builder.Property<string>("datosAnteriores")
               .HasColumnType("jsonb")
               .HasColumnName("datos_anteriores");

        builder.Property<string>("datosNuevos")
               .HasColumnType("jsonb")
               .HasColumnName("datos_nuevos");

        builder.Property<string>("motivo")
               .HasMaxLength(500)
               .HasColumnName("motivo");

        builder.Property<DateTime>("fecha")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha");

        builder.Property<int>("usuarioId")
               .IsRequired()
               .HasColumnName("usuario_id");

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey("usuarioId")
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_auditlogs_usuario");

        builder.HasIndex("entidad")
               .HasDatabaseName("idx_auditlogs_entidad");

        builder.HasIndex("usuarioId")
               .HasDatabaseName("idx_auditlogs_usuario_id");

        builder.HasIndex("fecha")
               .HasDatabaseName("idx_auditlogs_fecha");
    }
}
