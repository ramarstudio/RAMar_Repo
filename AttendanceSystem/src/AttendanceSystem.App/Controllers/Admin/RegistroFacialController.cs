using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.App.Helpers;
using AttendanceSystem.Core.DTOs;
using AttendanceSystem.Core.Interfaces;
using AttendanceSystem.Services;

namespace AttendanceSystem.App.Controllers.Admin
{
    public class RegistroFacialController
    {
        private readonly AppDbContext               _context;
        private readonly IConsentimientoRepository  _consentimientoRepo;
        private readonly IBiometricoService         _biometricoService;
        private readonly CameraHelper               _cameraHelper;
        private readonly AuditService               _auditService;
        private readonly ISessionManager            _session;

        public RegistroFacialController(
            AppDbContext               context,
            IConsentimientoRepository  consentimientoRepo,
            IBiometricoService         biometricoService,
            CameraHelper               cameraHelper,
            AuditService               auditService,
            ISessionManager            session)
        {
            _context            = context;
            _consentimientoRepo = consentimientoRepo;
            _biometricoService  = biometricoService;
            _cameraHelper       = cameraHelper;
            _auditService       = auditService;
            _session            = session;
        }

        public async Task<List<EmpleadoBiometricoDto>> ObtenerEmpleadosAsync(CancellationToken ct = default)
        {
            // Consulta con joins en SQL — un solo round-trip a la BD
            var empleados = await _context.Empleados
                .AsNoTracking()
                .Where(e => EF.Property<bool>(e, "activo"))
                .ToListAsync(ct);

            var empleadoIds = empleados.Select(e => e.GetId()).ToList();

            // Verificar embeddings existentes
            var idsConEmbedding = await _context.EmbeddingsFaciales
                .AsNoTracking()
                .Where(ef => empleadoIds.Contains(EF.Property<int>(ef, "empleadoId")))
                .Select(ef => EF.Property<int>(ef, "empleadoId"))
                .ToListAsync(ct);

            // Verificar consentimientos
            var idsConConsentimiento = await _context.Consentimientos
                .AsNoTracking()
                .Where(c => empleadoIds.Contains(EF.Property<int>(c, "empleadoId"))
                         && EF.Property<bool>(c, "aceptado"))
                .Select(c => EF.Property<int>(c, "empleadoId"))
                .ToListAsync(ct);

            // Nombres desde usuarios
            var usuarioIds = empleados.Select(e => e.GetUsuarioId()).ToList();
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(u => usuarioIds.Contains(EF.Property<int>(u, "id")))
                .ToListAsync(ct);

            var nombresPorUsuarioId = usuarios.ToDictionary(
                u => u.GetId(),
                u => u.GetNombre()
            );

            return empleados
                .Select(e => new EmpleadoBiometricoDto
                {
                    Id                  = e.GetId(),
                    Codigo              = e.GetCodigo(),
                    Nombre              = nombresPorUsuarioId.GetValueOrDefault(e.GetUsuarioId(), e.GetCodigo()),
                    TieneEmbedding      = idsConEmbedding.Contains(e.GetId()),
                    TieneConsentimiento = idsConConsentimiento.Contains(e.GetId()),
                })
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        // ── Consentimiento ───────────────────────────────────────────────────
        public async Task<(bool Ok, string Mensaje)> OtorgarConsentimientoAsync(int empleadoId)
        {
            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "id") == empleadoId);
            if (empleado == null)
                return (false, "Empleado no encontrado.");

            var existente = await _consentimientoRepo.GetByEmpleadoIdAsync(empleadoId);
            if (existente != null && existente.EstaAutorizado())
                return (true, "El consentimiento ya está otorgado.");

            if (existente != null)
            {
                existente.Otorgar("digital", "local-admin");
                await _consentimientoRepo.UpdateAsync(existente);
            }
            else
            {
                var nuevo = new Consentimiento();
                nuevo.SetEmpleadoId(empleadoId);
                nuevo.Otorgar("digital", "local-admin");
                await _consentimientoRepo.AddAsync(nuevo);
            }

            int adminId = _session.GetCurrentSession()?.UserId ?? 0;
            await _auditService.RegistrarAsync(
                accion: "CREAR", entidad: "Consentimiento", registroId: empleadoId,
                usuarioId: adminId, anterior: null, nuevo: "digital",
                motivo: $"Consentimiento biométrico otorgado para empleado {empleado.GetCodigo()}");

            return (true, "Consentimiento registrado correctamente.");
        }

        // ── Cámara ───────────────────────────────────────────────────────────
        public void IniciarCamara(EventHandler<BitmapSource> callback)
        {
            _cameraHelper.OnFrameArrived += callback;
            _cameraHelper.IniciarCamara(0);
        }

        public void ApagarCamara(EventHandler<BitmapSource> callback)
        {
            _cameraHelper.OnFrameArrived -= callback;
            _cameraHelper.DetenerCamara();
        }

        // ── Registro facial ──────────────────────────────────────────────────
        public async Task<(bool Ok, string Mensaje)> RegistrarRostroAsync(string codigoEmpleado)
        {
            string fotoBase64 = _cameraHelper.CapturarFrameEnBase64();
            if (string.IsNullOrEmpty(fotoBase64))
                return (false, "No se pudo capturar la imagen de la cámara.");

            try
            {
                bool exito = await _biometricoService.RegistrarNuevoRostroAsync(fotoBase64, codigoEmpleado);
                if (!exito)
                    return (false, "No se pudo registrar el rostro. Verifique que el servicio facial esté activo.");

                int adminId = _session.GetCurrentSession()?.UserId ?? 0;
                await _auditService.RegistrarAsync(
                    accion: "CREAR", entidad: "EmbeddingFacial", registroId: 0,
                    usuarioId: adminId, anterior: null, nuevo: codigoEmpleado,
                    motivo: $"Registro facial de empleado {codigoEmpleado}");

                return (true, "Rostro registrado exitosamente.");
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar rostro: {ex.Message}");
            }
        }
    }
}
