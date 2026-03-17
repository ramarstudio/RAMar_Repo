using System.Collections.Generic;
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

    public async Task<Usuario> GetByIdAsync(int id)
    {
        return await _context.Usuarios
                             .Include(u => u.GetRol()) // Traer la relación por defecto
                             .FirstOrDefaultAsync(u => u.GetId() == id);
    }

    public async Task<Usuario> GetByUsernameAsync(string username)
    {
        return await _context.Usuarios
                             .Include(u => u.GetRol())
                             .FirstOrDefaultAsync(u => u.GetUsername() == username && u.GetActivo());
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
                             .Include(u => u.GetRol())
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
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            usuario.Desactivar(); // Borrado lógico (soft delete)
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
