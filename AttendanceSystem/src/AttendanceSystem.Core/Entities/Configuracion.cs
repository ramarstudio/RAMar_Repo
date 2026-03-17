using System;

public class Configuracion
{
    private int id;
    private string clave;
    private string valor;
    private string tipoDato;
    private string descripcion;

    public Configuracion() { }

    // --- GETTERS ---
    public int GetId() => id;
    public string GetClave() => clave;
    public string GetValor() => valor;
    public string GetTipoDato() => tipoDato;
    public string GetDescripcion() => descripcion;

    // --- SETTERS ---
    public void SetClave(string clave) { this.clave = clave; }
    public void SetValor(string valor) { this.valor = valor; }
    public void SetTipoDato(string tipoDato) { this.tipoDato = tipoDato; }
    public void SetDescripcion(string descripcion) { this.descripcion = descripcion; }

    // --- MÉTODOS DE CONVERSIÓN (negocio) ---

    // Convierte el valor al tipo correspondiente según tipoDato
    public int GetValorEntero() => int.Parse(valor);
    public decimal GetValorDecimal() => decimal.Parse(valor);
    public bool GetValorBooleano() => bool.Parse(valor);

    public override string ToString() => $"Config[{clave}] = {valor} ({tipoDato})";
}
