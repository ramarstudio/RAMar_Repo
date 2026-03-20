using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Options;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class UsuariosController
    {
        private readonly IUsuarioRepository  _usuarioRepo;
        private readonly IPasswordHasher     _hasher;
        private readonly AuditService        _auditService;
        private readonly ISessionManager     _session;
        private readonly AppDbContext        _context;
        private readonly EmpleadoDefaultOptions _empleadoDefaults;

        public UsuariosController(
            IUsuarioRepository     usuarioRepo,
            IPasswordHasher        hasher,
            AuditService           auditService,
            ISessionManager        session,
            AppDbContext           context,
            EmpleadoDefaultOptions empleadoDefaults)
        {
            _usuarioRepo      = usuarioRepo;
            _hasher           = hasher;
            _auditService     = auditService;
            _session          = session;
            _context          = context;
            _empleadoDefaults = empleadoDefaults;
        }

        public async Task<List<UsuarioFilaDto>> ObtenerTodosAsync()
        {
            var usuarios = await _usuarioRepo.GetAllAsync();
            return usuarios.Select(u => new UsuarioFilaDto
            {
                Id            = u.GetId(),
                Username      = u.GetUsername(),
                Nombre        = u.GetNombre(),
                Rol           = u.GetRol()?.GetNombre().ToString() ?? "—",
                Estado        = u.EstaActivo() ? "Activo" : "Inactivo",
                FechaCreacion = u.GetFechaCreacion().ToString("dd/MM/yyyy")
            }).ToList();
        }

        public async Task<List<RolSelectorDto>> ObtenerRolesAsync()
        {
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            return roles.Select(r => new RolSelectorDto
            {
                Id    = r.GetId(),
                Valor = r.GetNombre()
            }).ToList();
        }

        public async Task<(bool Ok, string Mensaje)> CrearUsuarioAsync(
            string username, string nombre, string password, int rolId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(nombre)   ||
                string.IsNullOrWhiteSpace(password))
                return (false, "Todos los campos son obligatorios.");

            var existente = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => EF.Property<string>(u, "username") == username.Trim());
            if (existente != null)
                return (false, $"Ya existe un usuario con username '{username}'.");

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Property<int>(r, "id") == rolId);
            if (rol == null)
                return (false, "Rol no encontrado.");

            // ── Crear Usuario ────────────────────────────────────────────────
            var nuevo = new Usuario();
            nuevo.SetUsername(username.Trim());
            nuevo.SetNombre(nombre.Trim());
            nuevo.SetPassword(_hasher.HashPassword(password));
            nuevo.SetActivo(true);
            nuevo.SetFechaCreacion(DateTime.UtcNow);
            nuevo.SetRol(rol);

            _context.Usuarios.Add(nuevo);
            _context.Entry(nuevo).Property("RolId").CurrentValue = rolId;
            await _context.SaveChangesAsync();

            // ── Crear Empleado vinculado — valores leídos desde configuración ─
            var empleadoExistente = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "usuarioId") == nuevo.GetId());

            if (empleadoExistente == null)
            {
                var baseDate = DateTime.Today;
                var empleado = new Empleado();
                empleado.SetCodigo(username.Trim().ToLowerInvariant());
                empleado.SetUsuarioId(nuevo.GetId());
                empleado.SetActivo(true);
                empleado.SetTolerancia(_empleadoDefaults.ToleranciaMins);
                empleado.SetHorarioEntrada(baseDate.AddHours(_empleadoDefaults.HorarioEntradaHora));
                empleado.SetHorarioSalida(baseDate.AddHours(_empleadoDefaults.HorarioSalidaHora));

                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();
            }

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "CREAR", entidad: "Usuario", registroId: nuevo.GetId(),
                usuarioId: adminId, anterior: null,
                nuevo: JsonSerializer.Serialize(new { username, rol = rol.GetNombre().ToString() }),
                motivo: "Alta de usuario por administrador");

            return (true, $"Usuario '{username}' creado correctamente.");
        }

        public async Task<(bool Ok, string Mensaje)> ToggleActivoAsync(int usuarioId)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
            if (usuario == null) return (false, "Usuario no encontrado.");

            string anterior = usuario.EstaActivo() ? "Activo" : "Inactivo";
            if (usuario.EstaActivo()) usuario.Desactivar(); else usuario.Activar();
            string nuevo = usuario.EstaActivo() ? "Activo" : "Inactivo";

            await _usuarioRepo.UpdateAsync(usuario);

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "ACTUALIZAR", entidad: "Usuario", registroId: usuarioId,
                usuarioId: adminId,
                anterior: JsonSerializer.Serialize(anterior),
                nuevo: JsonSerializer.Serialize(nuevo),
                motivo: "Cambio de estado por administrador");

            return (true, $"Usuario '{usuario.GetUsername()}' → {nuevo}.");
        }

        public async Task<(bool Ok, string Mensaje)> CambiarPasswordAsync(int usuarioId, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return (false, "La nueva contraseña no puede estar vacía.");

            var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
            if (usuario == null) return (false, "Usuario no encontrado.");

            usuario.SetPassword(_hasher.HashPassword(nuevaPassword));
            await _usuarioRepo.UpdateAsync(usuario);

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "ACTUALIZAR", entidad: "Usuario", registroId: usuarioId,
                usuarioId: adminId,
                anterior: JsonSerializer.Serialize("***"),
                nuevo: JsonSerializer.Serialize("***"),
                motivo: "Reset de contraseña por administrador");

            return (true, "Contraseña actualizada correctamente.");
        }
    }
}
