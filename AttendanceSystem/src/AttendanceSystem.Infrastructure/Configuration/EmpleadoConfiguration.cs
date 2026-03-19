using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");

        builder.HasKey("id");

        builder.Property<string>("codigo")
               .IsRequired()
               .HasMaxLength(20)
               .HasColumnName("codigo");

        builder.Property<DateTime>("horario_entrada")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("horario_entrada");

        builder.Property<DateTime>("horario_salida")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("horario_salida");

        builder.Property<int>("tolerancia")
               .IsRequired()
               .HasDefaultValue(0)
               .HasColumnName("tolerancia_minutos");

        builder.Property<bool>("activo")
               .IsRequired()
               .HasDefaultValue(true)
               .HasColumnName("activo");

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey("usuarioId")
               .IsRequired(true)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_empleados_usuario");

        builder.HasIndex("codigo")
               .IsUnique()
               .HasDatabaseName("idx_empleados_codigo");

        builder.HasIndex("usuarioId")
               .IsUnique()
               .HasDatabaseName("idx_empleados_usuario_id");
    }
}
