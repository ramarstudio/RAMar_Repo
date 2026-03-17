using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        // PK
        builder.HasKey(r => r.GetId());

        builder.Property(r => r.GetNombre())
               .IsRequired()
               .HasConversion<string>() // Lo guardamos como string en BD, ej. 'Admin' o 'Empleado'
               .HasMaxLength(30)
               .HasColumnName("nombre");

        builder.Property(r => r.GetDescripcion())
               .HasMaxLength(250)
               .HasColumnName("descripcion");

        // Índices
        builder.HasIndex(r => r.GetNombre())
               .IsUnique()
               .HasDatabaseName("idx_roles_nombre");
    }
}
