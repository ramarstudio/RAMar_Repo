using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

// ════════════════════════════════════════════════════════════════
// EJEMPLO DE REPOSITORY — Usar como referencia para el equipo
//
// ¿Qué hace este archivo?
//   Implementa la interfaz IMarcajeRepository que definimos en Core.
//   Contiene el código SQL real (a través de EF Core + LINQ) que
//   interactúa con la base de datos PostgreSQL.
//
// REGLA CLAVE: Los repositorios NO tienen lógica de negocio.
//   Solo guardan y recuperan datos. Nada de calcular tardanzas,
//   verificar consentimientos, ni tomar decisiones. Eso va en Services.
// ════════════════════════════════════════════════════════════════

public class MarcajeRepository : IMarcajeRepository
{
    // ── DEPENDENCIA ──────────────────────────────────────────────
    // AppDbContext es el puente a la BD. Lo recibimos por inyección
    // de dependencias — no lo creamos nosotros (eso lo hace el sistema).
    private readonly AppDbContext _context;

    // ── CONSTRUCTOR ──────────────────────────────────────────────
    // El sistema pasa automáticamente el AppDbContext configurado.
    public MarcajeRepository(AppDbContext context)
    {
        _context = context;
    }

    // ════════════════════════════════════════════════════════════
    // IMPLEMENTACIÓN DE IMarcajeRepository
    // Cada método aquí cumple el contrato definido en la interfaz.
    // ════════════════════════════════════════════════════════════

    // ── GetByIdAsync ─────────────────────────────────────────────
    // Busca un marcaje por su ID primario.
    // FindAsync es más eficiente que FirstOrDefault cuando buscas por PK.
    public async Task<Marcaje> GetByIdAsync(int id)
    {
        return await _context.Marcajes.FindAsync(id);
    }

    // ── GetByEmpleadoIdAsync ──────────────────────────────────────
    // Devuelve todos los marcajes de un empleado en un rango de fechas.
    // Usado para historial mensual, reportes, etc.
    //
    // LINQ equivale a este SQL:
    //   SELECT * FROM marcajes
    //   WHERE empleado_id = @empleadoId
    //     AND fecha_hora BETWEEN @fechaInicio AND @fechaFin
    //   ORDER BY fecha_hora ASC
    public async Task<IEnumerable<Marcaje>> GetByEmpleadoIdAsync(
        int empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        return await _context.Marcajes
            .Where(m => m.GetEmpleadoId() == empleadoId
                     && m.GetFechaHora() >= fechaInicio
                     && m.GetFechaHora() <= fechaFin)
            .OrderBy(m => m.GetFechaHora())
            .ToListAsync();
    }

    // ── GetUltimoMarcajeDelDiaAsync ───────────────────────────────
    // Devuelve el último marcaje del empleado en una fecha.
    // Útil para saber si el sistema debe registrar Entrada o Salida.
    //
    // LINQ equivale a este SQL:
    //   SELECT TOP 1 * FROM marcajes
    //   WHERE empleado_id = @empleadoId
    //     AND DATE(fecha_hora) = DATE(@fecha)
    //   ORDER BY fecha_hora DESC
    public async Task<Marcaje> GetUltimoMarcajeDelDiaAsync(int empleadoId, DateTime fecha)
    {
        return await _context.Marcajes
            .Where(m => m.GetEmpleadoId() == empleadoId
                     && m.GetFechaHora().Date == fecha.Date)    // .Date compara solo la fecha, sin hora
            .OrderByDescending(m => m.GetFechaHora())           // el más reciente primero
            .FirstOrDefaultAsync();                             // toma solo el primero (o null si no hay)
    }

    // ── AddAsync ──────────────────────────────────────────────────
    // Inserta un nuevo marcaje en la BD.
    // EF Core genera el INSERT automáticamente y asigna el Id generado.
    //
    // Siempre termina con SaveChangesAsync() para confirmar el cambio.
    // Sin esto, los cambios quedan en memoria pero NO llegan a la BD.
    public async Task AddAsync(Marcaje marcaje)
    {
        _context.Marcajes.Add(marcaje);         // EF rastrea el objeto como "nuevo"
        await _context.SaveChangesAsync();      // genera el INSERT y lo ejecuta en BD
    }

    // ── UpdateAsync ───────────────────────────────────────────────
    // Actualiza un marcaje existente en la BD.
    // Usar cuando se necesita corregir un marcaje asistido.
    //
    // Update() marca TODOS los campos como modificados.
    // EF genera el UPDATE con todos los campos en el WHERE del Id.
    public async Task UpdateAsync(Marcaje marcaje)
    {
        _context.Marcajes.Update(marcaje);      // EF rastrea el objeto como "modificado"
        await _context.SaveChangesAsync();      // genera el UPDATE y lo ejecuta en BD
    }
}
