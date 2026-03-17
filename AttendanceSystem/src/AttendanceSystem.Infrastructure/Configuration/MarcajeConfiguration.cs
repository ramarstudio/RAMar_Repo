using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ════════════════════════════════════════════════════════════════
// EJEMPLO DE CONFIGURATION — Usar como referencia para el equipo
//
// ¿Qué hace este archivo?
//   Define cómo se mapea la clase Marcaje a la tabla "marcajes"
//   en PostgreSQL. EF Core lee estas reglas al arrancar la app.
//
// ¿Cómo se usa?
//   AppDbContext llama: modelBuilder.ApplyConfiguration(new MarcajeConfiguration())
//   Eso es todo. EF Core hace el resto automáticamente.
// ════════════════════════════════════════════════════════════════

public class MarcajeConfiguration : IEntityTypeConfiguration<Marcaje>
{
    public void Configure(EntityTypeBuilder<Marcaje> builder)
    {
        // ── 1. NOMBRE DE TABLA ───────────────────────────────────
        // Por defecto EF Core usaría "Marcaje" (nombre de la clase).
        // Aquí lo sobreescribimos al nombre real de la BD.
        builder.ToTable("marcajes");

        // ── 2. CLAVE PRIMARIA ────────────────────────────────────
        // Indica cuál atributo es el Id de la tabla.
        builder.HasKey(m => m.GetId());

        // ── 3. PROPIEDADES (columnas) ────────────────────────────

        // "tipo" — obligatorio, máximo 20 caracteres
        builder.Property(m => m.GetTipo())
               .IsRequired()
               .HasMaxLength(20)
               .HasColumnName("tipo");           // nombre exacto de columna en BD

        // "fecha_hora" — obligatorio, sin zona horaria (recomendado para PostgreSQL)
        builder.Property(m => m.GetFechaHora())
               .IsRequired()
               .HasColumnType("timestamp without time zone")
               .HasColumnName("fecha_hora");

        // "tardanza" — obligatorio, valor por defecto false
        builder.Property(m => m.GetTardanza())
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("tardanza");

        // "min_tardanza" — opcional (puede ser 0 o null)
        builder.Property(m => m.GetMinutosTardanza())
               .HasColumnName("min_tardanza");

        // "asistido" — obligatorio, valor por defecto false
        builder.Property(m => m.GetAsistido())
               .IsRequired()
               .HasDefaultValue(false)
               .HasColumnName("asistido");

        // ── 4. CLAVES FORÁNEAS Y RELACIONES ──────────────────────

        // Relación con Empleado (N:1) — OBLIGATORIA
        // Un marcaje SIEMPRE pertenece a un empleado.
        // Si el empleado es eliminado, sus marcajes también (CASCADE).
        builder.HasOne<Empleado>()
               .WithMany()                          // Empleado tiene muchos Marcajes
               .HasForeignKey(m => m.GetEmpleadoId())
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("fk_marcajes_empleado");

        // Relación con Usuario como "creadoPor" (N:1) — OPCIONAL (nullable)
        // Solo existe cuando un admin registró el marcaje manualmente.
        // Si el admin es eliminado, el campo queda en NULL (SET NULL).
        builder.HasOne<Usuario>()
               .WithMany()
               .HasForeignKey(m => m.GetCreadoPorId())
               .IsRequired(false)                   // ← permite NULL
               .OnDelete(DeleteBehavior.SetNull)
               .HasConstraintName("fk_marcajes_usuario_admin");

        // ── 5. ÍNDICES ───────────────────────────────────────────
        // Aceleran las búsquedas más frecuentes en la app.
        // La consulta más común: "dame los marcajes del empleado X en el mes Y"
        builder.HasIndex(m => new { EmpleadoId = m.GetEmpleadoId(), FechaHora = m.GetFechaHora() })
               .HasDatabaseName("idx_marcajes_empleado_fecha");
    }
}
