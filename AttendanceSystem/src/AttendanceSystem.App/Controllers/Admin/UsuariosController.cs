using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Core.Enums;
using AttendanceSystem.Security;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    // ─── DTOs ────────────────────────────────────────────────────────────────────

    public class UsuarioFilaDto
    {
        public int    Id            { get; set; }
        public string Username      { get; set; }
        public string Nombre        { get; set; }
        public string Rol           { get; set; }
        public string Estado        { get; set; }
        public string FechaCreacion { get; set; }
    }

    public class RolSelectorDto
    {
        public int        Id    { get; set; }
        public RolUsuario Valor { get; set; }
        public string     Nombre => Valor.ToString();
    }

    // ─── Controller ──────────────────────────────────────────────────────────────

    public class UsuariosController
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IPasswordHasher    _hasher;
        private readonly AuditService       _auditService;
        private readonly SessionManager     _session;
        private readonly AppDbContext       _context;

        public UsuariosController(
            IUsuarioRepository usuarioRepo,
            IPasswordHasher    hasher,
            AuditService       auditService,
            SessionManager     session,
            AppDbContext       context)
        {
            _usuarioRepo  = usuarioRepo;
            _hasher       = hasher;
            _auditService = auditService;
            _session      = session;
            _context      = context;
        }

        // ── Listar todos los usuarios ────────────────────────────────────────────
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

        // ── Roles disponibles para el selector del formulario ────────────────────
        public async Task<List<RolSelectorDto>> ObtenerRolesAsync()
        {
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            return roles.Select(r => new RolSelectorDto
            {
                Id    = r.GetId(),
                Valor = r.GetNombre()
            }).ToList();
        }

        // ── Crear usuario nuevo + empleado vinculado automáticamente ─────────────
        // Cada usuario del sistema de asistencia es también un empleado.
        // Si no se crea el registro Empleado, el usuario no aparecerá en los
        // selectores de Marcajes ni Reportes.
        public async Task<(bool Ok, string Mensaje)> CrearUsuarioAsync(
            string username, string nombre, string password, int rolId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(nombre)   ||
                string.IsNullOrWhiteSpace(password))
                return (false, "Todos los campos son obligatorios.");

            // Verificar username único
            var existente = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => EF.Property<string>(u, "username") == username.Trim());
            if (existente != null)
                return (false, $"Ya existe un usuario con username '{username}'.");

            // Cargar rol sin tracking para evitar conflictos de estado
            var rol = await _context.Roles
                .FirstOrDefaultAsync(r => EF.Property<int>(r, "id") == rolId);
            if (rol == null)
                return (false, "Rol no encontrado.");

            // ── Crear Usuario ────────────────────────────────────────────────────
            var nuevo = new Usuario();
            nuevo.SetUsername(username.Trim());
            nuevo.SetNombre(nombre.Trim());
            nuevo.SetPassword(_hasher.HashPassword(password));
            nuevo.SetActivo(true);
            nuevo.SetFechaCreacion(DateTime.UtcNow);
            nuevo.SetRol(rol);

            _context.Usuarios.Add(nuevo);
            await _context.SaveChangesAsync();   // ← ID asignado aquí por BD

            // ── Crear Empleado vinculado automáticamente ─────────────────────────
            // Necesario para que aparezca en selectores de Marcajes y Reportes.
            // Se usan valores por defecto: entrada 08:00, salida 17:00, tolerancia 15 min.
            var empleadoExistente = await _context.Empleados
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "usuarioId") == nuevo.GetId());

            if (empleadoExistente == null)
            {
                var baseDate        = DateTime.Today;
                var empleado        = new Empleado();
                empleado.SetCodigo(username.Trim().ToLowerInvariant());
                empleado.SetUsuarioId(nuevo.GetId());
                empleado.SetActivo(true);
                empleado.SetTolerancia(15);
                empleado.SetHorarioEntrada(baseDate.AddHours(8));
                empleado.SetHorarioSalida(baseDate.AddHours(17));

                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();
            }

            // ── Auditoría ────────────────────────────────────────────────────────
            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "CREAR", entidad: "Usuario", registroId: nuevo.GetId(),
                usuarioId: adminId, anterior: null,
                nuevo: $"username={username}, rol={rol.GetNombre()}",
                motivo: "Alta de usuario por administrador");

            return (true, $"Usuario '{username}' creado correctamente.");
        }

        // ── Activar / desactivar (soft-delete) ──────────────────────────────────
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
                usuarioId: adminId, anterior: anterior, nuevo: nuevo,
                motivo: "Cambio de estado por administrador");

            return (true, $"Usuario '{usuario.GetUsername()}' → {nuevo}.");
        }

        // ── Cambiar contraseña ───────────────────────────────────────────────────
        public async Task<(bool Ok, string Mensaje)> CambiarPasswordAsync(
            int usuarioId, string nuevaPassword)
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
                usuarioId: adminId, anterior: "***", nuevo: "***",
                motivo: "Reset de contraseña por administrador");

            return (true, "Contraseña actualizada correctamente.");
        }
    }
}
