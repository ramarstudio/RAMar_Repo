using System;

public class Consentimiento
{
    private int id;
    private string metodo;
    private bool aceptado;
    private DateTime fecha_consentimiento;
    private string hash_documento;
    private string ip_origen;
    private int empleadoId;

    public Consentimiento() { }

    // --- GETTERS ---
    public int GetId() => id;
    public string GetMetodo() => metodo;
    public bool GetAceptado() => aceptado;
    public DateTime GetFechaConsentimiento() => fecha_consentimiento;
    public string GetHashDocumento() => hash_documento;
    public string GetIpOrigen() => ip_origen;
    public int GetEmpleadoId() => empleadoId;

    // --- SETTERS ---
    public void SetMetodo(string metodo) { this.metodo = metodo; }
    public void SetAceptado(bool aceptado) { this.aceptado = aceptado; }
    public void SetFechaConsentimiento(DateTime fecha) { this.fecha_consentimiento = fecha; }
    public void SetHashDocumento(string hash) { this.hash_documento = hash; }
    public void SetIpOrigen(string ip) { this.ip_origen = ip; }
    public void SetEmpleadoId(int empleadoId) { this.empleadoId = empleadoId; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EstaAutorizado() => aceptado;

    public void Revocar() { this.aceptado = false; }

    public void Otorgar(string metodo, string ip)
    {
        this.aceptado = true;
        this.metodo = metodo;
        this.ip_origen = ip;
        this.fecha_consentimiento = DateTime.UtcNow;
    }

    public override string ToString() =>
        $"Consentimiento[empleadoId={empleadoId}]: {(aceptado ? "Autorizado" : "Revocado")} — {fecha_consentimiento:dd/MM/yyyy}";
}
