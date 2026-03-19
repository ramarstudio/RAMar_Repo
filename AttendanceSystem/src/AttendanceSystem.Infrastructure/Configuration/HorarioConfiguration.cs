using AttendanceSystem.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class HorarioConfiguration : IEntityTypeConfiguration<Horario>
{
    public void Configure(EntityTypeBuilder<Horario> builder)
    {
        builder.ToTable("horarios");

        builder.HasKey("id");

        builder.Property<DiaSemana>("dia")
               .IsRequired()
               .HasConversion<string>()
               .HasColumnName("dia_semana");

        builder.Property<DateTime>("entrada")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("hora_entrada");

        builder.Property<DateTime>("salida")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("hora_salida");

        builder.Property<DateTime>("vigente_desde")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("vigente_desde");

        builder.Property<DateTime>("vigente_hasta")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("vigente_hasta");

        builder.HasOne<Empleado>()
               .WithMany("horarios")
               .HasForeignKey("empleadoId")
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_horarios_empleado");

        builder.HasIndex("empleadoId")
               .HasDatabaseName("idx_horarios_empleado");
    }
}
