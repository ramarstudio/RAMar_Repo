using System;

public class AuditLog
{
    private int id;
    private string accion;
    private string entidad;
    private int registroId;
    private string datosAnteriores;
    private string datosNuevos;
    private string motivo;
    private DateTime fecha;
    private int usuarioId;

    public AuditLog() { }

    // --- GETTERS ---
    public int GetId() => id;
    public string GetAccion() => accion;
    public string GetEntidad() => entidad;
    public int GetRegistroId() => registroId;
    public string GetDatosAnteriores() => datosAnteriores;
    public string GetDatosNuevos() => datosNuevos;
    public string GetMotivo() => motivo;
    public DateTime GetFecha() => fecha;
    public int GetUsuarioId() => usuarioId;

    // --- SETTERS ---
    // AuditLog es INMUTABLE después de creado: no tiene setters individuales.
    // Solo se puede construir con el método de fábrica Registrar().

    // --- MÉTODO DE FÁBRICA ---
    public static AuditLog Registrar(
        string accion,
        string entidad,
        int registroId,
        int usuarioId,
        string datosAnteriores = null,
        string datosNuevos = null,
        string motivo = null)
    {
        var log = new AuditLog();
        log.accion = accion;
        log.entidad = entidad;
        log.registroId = registroId;
        log.usuarioId = usuarioId;
        log.datosAnteriores = datosAnteriores;
        log.datosNuevos = datosNuevos;
        log.motivo = motivo;
        log.fecha = DateTime.UtcNow;
        return log;
    }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EsModificacion() => accion == "MODIFICAR";
    public bool EsEliminacion() => accion == "ELIMINAR";
    public bool EsMarcajeAsistido() => accion == "MARCAJE_ASISTIDO";
    public bool TieneMotivo() => !string.IsNullOrEmpty(motivo);

    public override string ToString() =>
        $"[{fecha:dd/MM/yyyy HH:mm}] {accion} en {entidad}[{registroId}] por usuarioId={usuarioId}";
}
