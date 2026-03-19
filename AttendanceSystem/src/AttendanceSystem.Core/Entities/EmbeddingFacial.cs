using System;

public class EmbeddingFacial
{
    private int id;
    private byte[] vectorCifrado;
    private string algoritmo;
    private decimal umbral;
    private string versionModelo;
    private DateTime creadoEn;
    private DateTime? actualizadoEn;
    private int empleadoId;

    public EmbeddingFacial() { }

    // --- GETTERS ---
    public int GetId() => id;
    public byte[] GetVectorCifrado() => vectorCifrado;
    public string GetAlgoritmo() => algoritmo;
    public decimal GetUmbral() => umbral;
    public string GetVersionModelo() => versionModelo;
    public DateTime GetCreadoEn() => creadoEn;
    public DateTime? GetActualizadoEn() => actualizadoEn;
    public int GetEmpleadoId() => empleadoId;

    // --- SETTERS ---
    public void SetVectorCifrado(byte[] vector) { this.vectorCifrado = vector; }
    public void SetAlgoritmo(string algoritmo) { this.algoritmo = algoritmo; }
    public void SetUmbral(decimal umbral) { this.umbral = umbral; }
    public void SetVersionModelo(string version) { this.versionModelo = version; }
    public void SetEmpleadoId(int empleadoId) { this.empleadoId = empleadoId; }

    public void SetCreadoEn(DateTime fecha) { this.creadoEn = fecha; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool ConfianzaSuficiente(decimal nivelConfianza) => nivelConfianza >= umbral;

    public bool TieneVectorCargado() => vectorCifrado != null && vectorCifrado.Length > 0;

    public void MarcarActualizado() { this.actualizadoEn = DateTime.UtcNow; }

    public override string ToString() =>
        $"Embedding[empleadoId={empleadoId}]: v{versionModelo} — Umbral: {umbral}";
}
