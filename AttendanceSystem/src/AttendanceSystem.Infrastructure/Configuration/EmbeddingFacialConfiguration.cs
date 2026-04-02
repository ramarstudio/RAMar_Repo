using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class EmbeddingFacialConfiguration : IEntityTypeConfiguration<EmbeddingFacial>
{
    public void Configure(EntityTypeBuilder<EmbeddingFacial> builder)
    {
        builder.ToTable("embeddings_faciales");

        builder.HasKey("id");

        builder.Property<byte[]>("vectorCifrado")
               .IsRequired()
               .HasColumnName("vector_cifrado");

        builder.Property<string>("algoritmo")
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("algoritmo");

        builder.Property<decimal>("umbral")
               .IsRequired()
               .HasColumnType("numeric(5,4)")
               .HasColumnName("umbral");

        builder.Property<string>("versionModelo")
               .HasMaxLength(20)
               .HasColumnName("version_modelo");

        builder.Property<DateTime>("creadoEn")
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("creado_en");

        builder.Property<DateTime?>("actualizadoEn")
               .HasColumnType("timestamp without time zone")
               .HasColumnName("actualizado_en");

        builder.HasOne<Empleado>()
               .WithOne("embeddingFacial")
               .HasForeignKey<EmbeddingFacial>("empleadoId")
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_embeddings_empleado");

        builder.HasIndex("empleadoId")
               .IsUnique()
               .HasDatabaseName("idx_embeddings_empleado");
    }
}
