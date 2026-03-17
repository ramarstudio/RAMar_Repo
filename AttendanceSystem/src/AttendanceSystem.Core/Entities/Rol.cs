using System;
using AttendanceSystem.Core.Enums;

public class Rol
{
    private int id;
    private RolUsuario rolUsuarioVal;
    private string descripcion;

    public Rol() { }

    // --- GETTERS ---
    public int GetId() => id;
    public RolUsuario GetNombre() => rolUsuarioVal;
    public string GetDescripcion() => descripcion;

    // --- SETTERS ---
    public void SetNombre(RolUsuario rol) { this.rolUsuarioVal = rol; }
    public void SetDescripcion(string descripcion) { this.descripcion = descripcion; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EsAdministrador() => rolUsuarioVal == RolUsuario.Admin;
    public bool EsEmpleado() => rolUsuarioVal == RolUsuario.Empleado;

    public override string ToString() => $"Rol[{id}]: {rolUsuarioVal}";
}