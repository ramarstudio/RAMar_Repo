using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.GetId());

        builder.Property(u => u.GetUsername())
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("username");

        builder.Property(u => u.GetPassword())
               .IsRequired()
               .HasMaxLength(128) // Hashed password
               .HasColumnName("password");

        builder.Property(u => u.GetNombre())
               .IsRequired()
               .HasMaxLength(150)
               .HasColumnName("nombre");

        builder.Property(u => u.GetActivo())
               .IsRequired()
               .HasDefaultValue(true)
               .HasColumnName("activo");

        builder.Property(u => u.GetFechaCreacion())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_creacion");

        // Relación con Rol (N:1)
        builder.HasOne(u => u.GetRol())
               .WithMany()
               .HasForeignKey("RolId") // Usando backing key ya que no hay RolId expuesto explícitamente pero sí "Rol rol"
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_usuarios_rol");

        builder.HasIndex(u => u.GetUsername())
               .IsUnique()
               .HasDatabaseName("idx_usuarios_username");
    }
}
