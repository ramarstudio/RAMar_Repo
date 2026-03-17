using System;
using AttendanceSystem.Core.Enums;

public class Marcaje
{
    private int id;
    private TipoMarcaje tipo;
    private DateTime fechaHora;
    private bool tardanza;
    private int min_tardanza;
    private bool asistido;
    private int empleadoId;
    private int? creadoPorId;  // nullable: solo cuando fue asistido por un admin

    public Marcaje() { }

    // --- GETTERS ---
    public int GetId() => id;
    public TipoMarcaje GetTipo() => tipo;
    public DateTime GetFechaHora() => fechaHora;
    public bool GetTardanza() => tardanza;
    public int GetMinutosTardanza() => min_tardanza;
    public bool GetAsistido() => asistido;
    public int GetEmpleadoId() => empleadoId;
    public int? GetCreadoPorId() => creadoPorId;

    // --- SETTERS ---
    public void SetTipo(TipoMarcaje tipo) { this.tipo = tipo; }
    public void SetFechaHora(DateTime fechaHora) { this.fechaHora = fechaHora; }
    public void SetTardanza(bool tardanza) { this.tardanza = tardanza; }
    public void SetMinutosTardanza(int minutos) { this.min_tardanza = minutos; }
    public void SetAsistido(bool asistido) { this.asistido = asistido; }
    public void SetEmpleadoId(int empleadoId) { this.empleadoId = empleadoId; }
    public void SetCreadoPorId(int? usuarioId) { this.creadoPorId = usuarioId; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EsTardanza() => tardanza;
    public bool FueAsistido() => asistido;
    public bool EsEntrada() => tipo == TipoMarcaje.Entrada;
    public bool EsSalida() => tipo == TipoMarcaje.Salida;
    public bool EsBreak() => tipo == TipoMarcaje.BreakInicio || tipo == TipoMarcaje.BreakFin;

    public string ObtenerResumen() =>
        $"[{tipo}] empleadoId={empleadoId} — {fechaHora:dd/MM/yyyy HH:mm}" +
        (tardanza ? $" ⚠ Tardanza: {min_tardanza} min" : "");

    public override string ToString() => ObtenerResumen();
}