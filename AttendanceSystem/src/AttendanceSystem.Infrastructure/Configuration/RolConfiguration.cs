using AttendanceSystem.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey("id");

        builder.Property<RolUsuario>("rolUsuarioVal")
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasColumnName("nombre");

        builder.Property<string>("descripcion")
               .HasMaxLength(250)
               .HasColumnName("descripcion");

        builder.HasIndex("rolUsuarioVal")
               .IsUnique()
               .HasDatabaseName("idx_roles_nombre");
    }
}
