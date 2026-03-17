using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.ToTable("configuraciones");

        builder.HasKey(c => c.GetId());

        builder.Property(c => c.GetClave())
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("clave");

        builder.Property(c => c.GetValor())
               .IsRequired()
               .HasColumnName("valor");

        builder.Property(c => c.GetTipoDato())
               .IsRequired()
               .HasMaxLength(20) // ej: "int", "boolean", "string"
               .HasColumnName("tipo_dato");

        builder.Property(c => c.GetDescripcion())
               .HasMaxLength(250)
               .HasColumnName("descripcion");

        builder.HasIndex(c => c.GetClave())
               .IsUnique()
               .HasDatabaseName("idx_configuraciones_clave");
    }
}
