using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

// Implementación base genérica para repositorios EF Core.
// Centraliza AddAsync, UpdateAsync y SaveAsync eliminando duplicación
// en cada repositorio concreto.
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly AppDbContext _context;

    protected RepositoryBase(AppDbContext context)
    {
        _context = context ?? throw new System.ArgumentNullException(nameof(context));
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Desadjuntar cualquier instancia previamente trackeada con el mismo Id
        // para evitar: "cannot be tracked because another instance with the same key value"
        var entry = _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Entity != entity &&
                _context.Entry(e.Entity).Property("id").CurrentValue.Equals(
                    _context.Entry(entity).Property("id").CurrentValue));
        if (entry != null)
            entry.State = EntityState.Detached;

        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
