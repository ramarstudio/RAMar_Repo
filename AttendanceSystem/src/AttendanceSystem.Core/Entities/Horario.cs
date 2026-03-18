using System;
using AttendanceSystem.Core.Enums;

public class Horario
{
    private int id;
    private DiaSemana dia;
    private DateTime entrada;
    private DateTime salida;
    private DateTime vigente_desde;
    private DateTime vigente_hasta;
    private int empleadoId;

    public Horario() { }

    // --- GETTERS ---
    public int GetId() => id;
    public DiaSemana GetDia() => dia;
    public DateTime GetEntrada() => entrada;
    public DateTime GetSalida() => salida;
    public DateTime GetVigenteDesde() => vigente_desde;
    public DateTime GetVigenteHasta() => vigente_hasta;
    public int GetEmpleadoId() => empleadoId;

    // --- SETTERS ---
    public void SetDia(DiaSemana dia) { this.dia = dia; }
    public void SetEntrada(DateTime entrada) { this.entrada = entrada; }
    public void SetSalida(DateTime salida) { this.salida = salida; }
    public void SetVigenteDesde(DateTime fecha) { this.vigente_desde = fecha; }
    public void SetVigenteHasta(DateTime fecha) { this.vigente_hasta = fecha; }
    public void SetEmpleadoId(int empleadoId) { this.empleadoId = empleadoId; }

    // --- MÉTODOS DE NEGOCIO ---

    // Verifica si este horario está vigente en la fecha indicada
    public bool EstaVigente(DateTime fecha)
    {
        return fecha >= vigente_desde && fecha <= vigente_hasta;
    }

    // Verifica si la hora dada está dentro del turno laboral
    public bool EstaEnTurno(DateTime hora)
    {
        return hora.TimeOfDay >= entrada.TimeOfDay && hora.TimeOfDay <= salida.TimeOfDay;
    }

    public override string ToString() => $"Horario[{dia}]: {entrada:HH:mm} - {salida:HH:mm}";
}