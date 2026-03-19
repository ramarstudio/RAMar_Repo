using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class ConsentimientoConfiguration : IEntityTypeConfiguration<Consentimiento>
{
    public void Configure(EntityTypeBuilder<Consentimiento> builder)
    {
        builder.ToTable("consentimientos");

        builder.HasKey("id");

        builder.Property<string>("metodo")
               .HasMaxLength(50)
               .HasColumnName("metodo");

        builder.Property<bool>("aceptado")
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("aceptado");

        builder.Property<DateTime>("fecha_consentimiento")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_consentimiento");

        builder.Property<string>("hash_documento")
               .HasMaxLength(256)
               .HasColumnName("hash_documento");

        builder.Property<string>("ip_origen")
               .HasMaxLength(45)
               .HasColumnName("ip_origen");

        builder.HasOne<Empleado>()
               .WithMany()
               .HasForeignKey("empleadoId")
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_consentimientos_empleado");

        builder.HasIndex("empleadoId")
               .IsUnique()
               .HasDatabaseName("idx_consentimientos_empleado");
    }
}
