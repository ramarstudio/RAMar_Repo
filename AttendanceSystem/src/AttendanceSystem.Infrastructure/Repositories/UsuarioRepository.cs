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

    public async Task<Usuario> GetByIdAsync(int id)
    {
        var lista = await _context.Usuarios.Include("rol").ToListAsync();
        return lista.FirstOrDefault(u => u.GetId() == id);
    }

    public async Task<Usuario> GetByUsernameAsync(string username)
    {
        var lista = await _context.Usuarios.Include("rol").ToListAsync();
        return lista.FirstOrDefault(u => u.GetUsername() == username && u.GetActivo());
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios.Include("rol").ToListAsync();
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
        var lista = await _context.Usuarios.ToListAsync();
        var usuario = lista.FirstOrDefault(u => u.GetId() == id);
        if (usuario != null)
        {
            usuario.Desactivar();
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
