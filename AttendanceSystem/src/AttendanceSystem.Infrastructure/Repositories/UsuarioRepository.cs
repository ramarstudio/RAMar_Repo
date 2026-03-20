using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario> GetByIdAsync(int id)
        => await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Property<int>(u, "id") == id);

    public async Task<Usuario> GetByUsernameAsync(string username)
        => await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                EF.Property<string>(u, "username") == username &&
                EF.Property<bool>(u, "activo") == true);

    public async Task<IEnumerable<Usuario>> GetAllAsync()
        => await _context.Usuarios
            .Include("rol")
            .AsNoTracking()
            .ToListAsync();

    // IN (...) query — filtra solo los IDs solicitados, sin cargar la tabla completa
    public async Task<List<Usuario>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids as List<int> ?? ids.ToList();
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => idList.Contains(EF.Property<int>(u, "id")))
            .ToListAsync(ct);
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
