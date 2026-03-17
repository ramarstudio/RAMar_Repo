using System;
using System.Collections.Generic;

public class Empleado
{
    private int id;
    private string codigo;
    private DateTime horario_entrada;
    private DateTime horario_salida;
    private int tolerancia;
    private bool activo;
    private int usuarioId;
    private List<Horario> horarios;
    private EmbeddingFacial embeddingFacial;
    private Consentimiento consentimiento;
    private List<Marcaje> marcajes;

    public Empleado() { }

    // --- GETTERS ---
    public int GetId() => id;
    public string GetCodigo() => codigo;
    public DateTime GetHorarioEntrada() => horario_entrada;
    public DateTime GetHorarioSalida() => horario_salida;
    public int GetTolerancia() => tolerancia;
    public bool GetActivo() => activo;
    public int GetUsuarioId() => usuarioId;
    public List<Horario> GetHorarios() => horarios;
    public EmbeddingFacial GetEmbeddingFacial() => embeddingFacial;
    public Consentimiento GetConsentimiento() => consentimiento;
    public List<Marcaje> GetMarcajes() => marcajes;

    // --- SETTERS ---
    public void SetCodigo(string codigo) { this.codigo = codigo; }
    public void SetHorarioEntrada(DateTime entrada) { this.horario_entrada = entrada; }
    public void SetHorarioSalida(DateTime salida) { this.horario_salida = salida; }
    public void SetTolerancia(int tolerancia) { this.tolerancia = tolerancia; }
    public void SetActivo(bool activo) { this.activo = activo; }
    public void SetUsuarioId(int usuarioId) { this.usuarioId = usuarioId; }
    public void SetHorarios(List<Horario> horarios) { this.horarios = horarios; }
    public void SetEmbeddingFacial(EmbeddingFacial embedding) { this.embeddingFacial = embedding; }
    public void SetConsentimiento(Consentimiento consentimiento) { this.consentimiento = consentimiento; }
    public void SetMarcajes(List<Marcaje> marcajes) { this.marcajes = marcajes; }

    // --- MÉTODOS DE NEGOCIO ---
    public bool EstaActivo() => activo;
    public void Activar() { this.activo = true; }
    public void Desactivar() { this.activo = false; }

    public bool LlegoTarde(DateTime horaActual)
    {
        return horaActual.TimeOfDay > horario_entrada.TimeOfDay.Add(TimeSpan.FromMinutes(tolerancia));
    }

    public int CalcularMinutosTardanza(DateTime horaActual)
    {
        var diferencia = horaActual.TimeOfDay - horario_entrada.TimeOfDay;
        return diferencia.TotalMinutes > 0 ? (int)diferencia.TotalMinutes : 0;
    }

    public bool TieneConsentimientoValido() => consentimiento != null && consentimiento.EstaAutorizado();
    public bool TieneEmbedding() => embeddingFacial != null && embeddingFacial.TieneVectorCargado();

    public override string ToString() => $"Empleado[{codigo}]: usuarioId={usuarioId}";
}