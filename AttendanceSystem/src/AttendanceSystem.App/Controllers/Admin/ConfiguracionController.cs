using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class ConfiguracionController
    {
        private readonly AppDbContext    _context;
        private readonly AuditService   _auditService;
        private readonly ISessionManager _session;

        private const string SecurityPrefix = "Security:";

        public static readonly List<ConfigPresetDto> Presets = new()
        {
            new() { Clave = "MinutosTardanza",       ValorDefault = "15",         TipoDato = "int",     Descripcion = "Minutos de tolerancia antes de marcar como tardanza",      Icono = "ClockAlertOutline" },
            new() { Clave = "JornadaHorasMax",       ValorDefault = "8",          TipoDato = "int",     Descripcion = "Máximo de horas por jornada laboral",                      Icono = "ClockOutline" },
            new() { Clave = "PermitirMarcajeManual", ValorDefault = "false",      TipoDato = "bool",    Descripcion = "Permitir registrar asistencia sin reconocimiento facial",   Icono = "HandBackRightOutline" },
            new() { Clave = "NombreEmpresa",         ValorDefault = "Mi Empresa", TipoDato = "string",  Descripcion = "Nombre de la empresa que aparece en los reportes",          Icono = "Domain" },
            new() { Clave = "DiasRetroMarcaje",      ValorDefault = "0",          TipoDato = "int",     Descripcion = "Días permitidos para registrar marcajes atrasados",          Icono = "CalendarClockOutline" },
            new() { Clave = "NotificarTardanzas",    ValorDefault = "true",       TipoDato = "bool",    Descripcion = "Enviar alerta cuando se detecta una tardanza",               Icono = "BellAlertOutline" },
            new() { Clave = "MaxIntentosFacial",     ValorDefault = "3",          TipoDato = "int",     Descripcion = "Intentos máximos de reconocimiento facial antes de bloquear", Icono = "FaceRecognition" },
            new() { Clave = "HoraCorteNocturno",     ValorDefault = "22:00",      TipoDato = "string",  Descripcion = "Hora límite para marcajes del turno nocturno",               Icono = "WeatherNight" },
        };

        public ConfiguracionController(
            AppDbContext    context,
            AuditService   auditService,
            ISessionManager session)
        {
            _context      = context;
            _auditService = auditService;
            _session      = session;
        }

        public async Task<List<ConfiguracionFilaDto>> CargarConfiguracionesAsync(CancellationToken ct = default)
        {
            // Filtro, proyección y ordenamiento en SQL — no carga toda la tabla
            return await _context.Configuraciones
                .AsNoTracking()
                .Where(c => !EF.Property<string>(c, "clave").StartsWith(SecurityPrefix))
                .OrderBy(c => EF.Property<string>(c, "clave"))
                .Select(c => new ConfiguracionFilaDto
                {
                    Id          = EF.Property<int>(c, "id"),
                    Clave       = EF.Property<string>(c, "clave"),
                    Valor       = EF.Property<string>(c, "valor"),
                    TipoDato    = EF.Property<string>(c, "tipoDato"),
                    Descripcion = EF.Property<string>(c, "descripcion") ?? "—"
                })
                .ToListAsync(ct);
        }

        public async Task<List<ConfigPresetDto>> ObtenerPresetsDisponiblesAsync(CancellationToken ct = default)
        {
            var clavesExistentes = await _context.Configuraciones
                .AsNoTracking()
                .Select(c => EF.Property<string>(c, "clave"))
                .ToListAsync(ct);

            return Presets.Where(p => !clavesExistentes.Contains(p.Clave)).ToList();
        }

        public async Task<(bool Ok, string Mensaje)> ActualizarValorAsync(int id, string nuevoValor)
        {
            if (string.IsNullOrWhiteSpace(nuevoValor))
                return (false, "El valor no puede estar vacío.");

            var config = await _context.Configuraciones
                .FirstOrDefaultAsync(c => EF.Property<int>(c, "id") == id);

            if (config == null)
                return (false, "Configuración no encontrada.");

            if (config.GetClave().StartsWith(SecurityPrefix))
                return (false, "Las claves de seguridad no se pueden modificar desde aquí.");

            string valorAnterior = config.GetValor();
            config.SetValor(nuevoValor.Trim());
            _context.Configuraciones.Update(config);
            await _context.SaveChangesAsync();

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "ACTUALIZAR", entidad: "Configuracion", registroId: id,
                usuarioId: adminId,
                anterior: valorAnterior,
                nuevo: nuevoValor.Trim(),
                motivo: $"Cambio de configuración: {config.GetClave()}");

            return (true, $"'{config.GetClave()}' actualizado correctamente.");
        }

        public async Task<(bool Ok, string Mensaje)> CrearConfiguracionAsync(
            string clave, string valor, string tipoDato, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(valor))
                return (false, "Clave y valor son obligatorios.");

            string claveTrimmed = clave.Trim();

            // SQL EXISTS — no carga toda la tabla para verificar duplicado
            bool existe = await _context.Configuraciones
                .AnyAsync(c => EF.Property<string>(c, "clave") == claveTrimmed);

            if (existe)
                return (false, $"Ya existe la clave '{claveTrimmed}'.");

            var nueva = new Configuracion();
            nueva.SetClave(claveTrimmed);
            nueva.SetValor(valor.Trim());
            nueva.SetTipoDato(string.IsNullOrWhiteSpace(tipoDato) ? "string" : tipoDato.Trim());
            nueva.SetDescripcion(descripcion?.Trim());
            _context.Configuraciones.Add(nueva);
            await _context.SaveChangesAsync();

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "CREAR", entidad: "Configuracion", registroId: 0,
                usuarioId: adminId, anterior: null,
                nuevo: $"{claveTrimmed}={valor}",
                motivo: "Nueva configuración del sistema");

            return (true, $"Configuración '{claveTrimmed}' creada.");
        }

        public async Task<(bool Ok, string Mensaje)> EliminarConfiguracionAsync(int id)
        {
            var config = await _context.Configuraciones
                .FirstOrDefaultAsync(c => EF.Property<int>(c, "id") == id);

            if (config == null)
                return (false, "Configuración no encontrada.");

            if (config.GetClave().StartsWith(SecurityPrefix))
                return (false, "Las claves de seguridad no se pueden eliminar.");

            string clave = config.GetClave();
            _context.Configuraciones.Remove(config);
            await _context.SaveChangesAsync();

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "ELIMINAR", entidad: "Configuracion", registroId: id,
                usuarioId: adminId,
                anterior: $"{clave}={config.GetValor()}",
                nuevo: null,
                motivo: $"Eliminación de configuración: {clave}");

            return (true, $"'{clave}' eliminado correctamente.");
        }
    }
}
