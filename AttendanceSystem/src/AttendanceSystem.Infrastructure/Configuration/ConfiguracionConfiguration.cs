using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.ToTable("configuraciones");

        builder.HasKey("id");

        builder.Property<string>("clave")
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("clave");

        builder.Property<string>("valor")
               .IsRequired()
               .HasColumnName("valor");

        builder.Property<string>("tipoDato")
               .IsRequired()
               .HasMaxLength(20)
               .HasColumnName("tipo_dato");

        builder.Property<string>("descripcion")
               .HasMaxLength(250)
               .HasColumnName("descripcion");

        builder.HasIndex("clave")
               .IsUnique()
               .HasDatabaseName("idx_configuraciones_clave");
    }
}
