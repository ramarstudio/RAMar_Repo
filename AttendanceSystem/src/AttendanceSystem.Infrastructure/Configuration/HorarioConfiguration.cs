using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HorarioConfiguration : IEntityTypeConfiguration<Horario>
{
    public void Configure(EntityTypeBuilder<Horario> builder)
    {
        builder.ToTable("horarios");

        builder.HasKey(h => h.GetId());

        builder.Property(h => h.GetDia())
               .IsRequired()
               .HasColumnName("dia_semana");

        builder.Property(h => h.GetEntrada())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("hora_entrada");

        builder.Property(h => h.GetSalida())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("hora_salida");

        builder.Property(h => h.GetVigenteDesde())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("vigente_desde");

        builder.Property(h => h.GetVigenteHasta())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("vigente_hasta");

        // Relación N:1 con Empleado
        builder.HasOne(h => h.GetEmpleado())
               .WithMany(e => e.GetHorarios())
               .HasForeignKey("EmpleadoId") // Usar backing field / abstract mapping ya que el horarioget no tiene un GetEmpleadoId sino GetEmpleado, pero internamente existe empleadoId
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_horarios_empleado");

        builder.HasIndex("EmpleadoId")
               .HasDatabaseName("idx_horarios_empleado");
    }
}
