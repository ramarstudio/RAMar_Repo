using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConsentimientoConfiguration : IEntityTypeConfiguration<Consentimiento>
{
    public void Configure(EntityTypeBuilder<Consentimiento> builder)
    {
        builder.ToTable("consentimientos");

        builder.HasKey(c => c.GetId());

        builder.Property(c => c.GetMetodo())
               .HasMaxLength(50)
               .HasColumnName("metodo");

        builder.Property(c => c.GetAceptado())
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("aceptado");

        builder.Property(c => c.GetFechaConsentimiento())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_consentimiento");

        builder.Property(c => c.GetHashDocumento())
               .HasMaxLength(256)
               .HasColumnName("hash_documento");

        builder.Property(c => c.GetIpOrigen())
               .HasMaxLength(45)
               .HasColumnName("ip_origen");

        // Relación 1:1 o M:1 con Empleado
        builder.HasOne<Empleado>()
               .WithMany()
               .HasForeignKey(c => c.GetEmpleadoId())
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_consentimientos_empleado");

        builder.HasIndex(c => c.GetEmpleadoId())
               .IsUnique() // Asumiendo Consentimiento activo único por empleado
               .HasDatabaseName("idx_consentimientos_empleado");
    }
}
