using System;

public class Usuario
{
    private int id;
    private string username;
    private string password;
    private string name;
    private bool activo;
    private DateTime fecha_creacion;
    private Rol rol;

    public Usuario() { }

    // --- GETTERS ---
    public int GetId() => id;
    public string GetUsername() => username;
    public string GetPassword() => password;
    public string GetNombre() => name;
    public bool GetActivo() => activo;
    public DateTime GetFechaCreacion() => fecha_creacion;
    public Rol GetRol() => rol;

    // --- SETTERS ---
    public void SetUsername(string username) { this.username = username; }
    public void SetPassword(string password) { this.password = password; }
    public void SetNombre(string name) { this.name = name; }
    public void SetActivo(bool activo) { this.activo = activo; }
    public void SetRol(Rol rol) { this.rol = rol; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EstaActivo() => activo;

    public bool EsAdministrador() => rol != null && rol.EsAdministrador();

    public void Activar() { this.activo = true; }
    public void Desactivar() { this.activo = false; }

    public override string ToString() => $"Usuario[{id}]: {username} ({rol?.GetNombre()})";
}
