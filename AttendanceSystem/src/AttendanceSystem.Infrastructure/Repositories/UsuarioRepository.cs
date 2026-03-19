using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    // Server-side filter by PK — O(1) index lookup, no full-table scan
    public async Task<Usuario> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Property<int>(u, "id") == id);
    }

    // Server-side filter on indexed column — O(log n) index seek
    public async Task<Usuario> GetByUsernameAsync(string username)
    {
        return await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "username") == username &&
                EF.Property<bool>(u, "activo") == true);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => EF.Property<int>(u, "id") == id);
        if (usuario != null)
        {
            usuario.Desactivar();
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
