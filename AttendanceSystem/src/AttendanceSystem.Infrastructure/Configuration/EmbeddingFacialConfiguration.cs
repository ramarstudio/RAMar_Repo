using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmbeddingFacialConfiguration : IEntityTypeConfiguration<EmbeddingFacial>
{
    public void Configure(EntityTypeBuilder<EmbeddingFacial> builder)
    {
        builder.ToTable("embeddings_faciales");

        builder.HasKey(e => e.GetId());

        builder.Property(e => e.GetVectorCifrado())
               .IsRequired()
               .HasColumnName("vector_cifrado");

        builder.Property(e => e.GetAlgoritmo())
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("algoritmo");

        builder.Property(e => e.GetUmbral())
               .IsRequired()
               .HasColumnType("numeric(5,4)") // ej: 0.9500
               .HasColumnName("umbral");

        builder.Property(e => e.GetVersionModelo())
               .HasMaxLength(20)
               .HasColumnName("version_modelo");

        builder.Property(e => e.GetCreadoEn())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("creado_en");

        builder.Property(e => e.GetActualizadoEn())
               .HasColumnType("timestamp without time zone")
               .HasColumnName("actualizado_en");

        // Relación 1:1 con Empleado
        builder.HasOne<Empleado>()
               .WithMany()
               .HasForeignKey(e => e.GetEmpleadoId())
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_embeddings_empleado");

        builder.HasIndex(e => e.GetEmpleadoId())
               .IsUnique()
               .HasDatabaseName("idx_embeddings_empleado");
    }
}
