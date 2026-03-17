using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");

        builder.HasKey(e => e.GetId());

        builder.Property(e => e.GetCodigo())
               .IsRequired()
               .HasMaxLength(20)
               .HasColumnName("codigo");

        builder.Property(e => e.GetHorarioEntrada())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("horario_entrada");

        builder.Property(e => e.GetHorarioSalida())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("horario_salida");

        builder.Property(e => e.GetTolerancia())
               .IsRequired()
               .HasDefaultValue(0)
               .HasColumnName("tolerancia_minutos");

        builder.Property(e => e.GetActivo())
               .IsRequired()
               .HasDefaultValue(true)
               .HasColumnName("activo");

        // Relación 1:1 con Usuario (Usuario -> Empleado)
        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(e => e.GetUsuarioId())
               .IsRequired(true)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_empleados_usuario");

        // Índices
        builder.HasIndex(e => e.GetCodigo())
               .IsUnique()
               .HasDatabaseName("idx_empleados_codigo");
               
        builder.HasIndex(e => e.GetUsuarioId())
               .IsUnique()
               .HasDatabaseName("idx_empleados_usuario_id");
    }
}
