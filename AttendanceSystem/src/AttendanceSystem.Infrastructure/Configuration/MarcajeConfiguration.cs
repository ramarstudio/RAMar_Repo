using AttendanceSystem.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class MarcajeConfiguration : IEntityTypeConfiguration<Marcaje>
{
    public void Configure(EntityTypeBuilder<Marcaje> builder)
    {
        builder.ToTable("marcajes");

        builder.HasKey("id");

        builder.Property<TipoMarcaje>("tipo")
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20)
               .HasColumnName("tipo");

        builder.Property<DateTime>("fechaHora")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_hora");

        builder.Property<bool>("tardanza")
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("tardanza");

        builder.Property<int>("min_tardanza")
               .HasColumnName("min_tardanza");

        builder.Property<bool>("asistido")
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("asistido");

        builder.HasOne<Empleado>()
               .WithMany()
               .HasForeignKey("empleadoId")
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_marcajes_empleado");

        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey("creadoPorId")
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("fk_marcajes_usuario_admin");

        builder.HasIndex("empleadoId", "fechaHora")
               .HasDatabaseName("idx_marcajes_empleado_fecha");
    }
}
