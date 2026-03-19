using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey("id");

        builder.Property<string>("username")
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("username");

        builder.Property<string>("password")
               .IsRequired()
               .HasMaxLength(256)
               .HasColumnName("password");

        builder.Property<string>("name")
               .IsRequired()
               .HasMaxLength(150)
               .HasColumnName("nombre");

        builder.Property<bool>("activo")
               .IsRequired()
               .HasDefaultValue(true)
               .HasColumnName("activo");

        builder.Property<DateTime>("fecha_creacion")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_creacion");

        builder.HasOne<Rol>("rol")
               .WithMany()
               .HasForeignKey("RolId")
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("fk_usuarios_rol");

        builder.HasIndex("username")
               .IsUnique()
               .HasDatabaseName("idx_usuarios_username");
    }
}
