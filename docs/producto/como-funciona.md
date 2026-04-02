---
icon: material/cog
---

# Como funciona

<span class="section-label">Flujo principal</span>

## Marcaje de asistencia

<div class="diagram-box">

```mermaid
sequenceDiagram
    participant E as Empleado
    participant C as Camara
    participant W as App WPF (C#)
    participant P as Motor IA (Python)
    participant D as PostgreSQL

    E->>C: Se acerca al kiosco
    C->>W: Stream de video en vivo
    W->>P: Envia frame capturado
    P->>P: Detecta rostro + genera embedding
    P->>W: Retorna vector 512-d
    W->>D: Busca embedding mas similar
    D->>W: Match encontrado
    W->>E: Pantalla de confirmacion
    W->>D: Registra marcaje
```

</div>

!!! tip "Rendimiento"
    El proceso completo — desde la captura del frame hasta la confirmacion en pantalla — toma menos de **1 segundo**.

---

<span class="section-label">Proceso inicial</span>

## Registro de empleados

Antes de marcar asistencia, un administrador registra el rostro de cada empleado:

<ul class="step-list">
<li>El admin selecciona al empleado desde el panel</li>
<li>Se activa la camara en vivo</li>
<li>El empleado se posiciona frente a la camara</li>
<li>El admin hace clic en <strong>Capturar rostro</strong></li>
<li>El sistema genera un embedding y lo almacena cifrado</li>
</ul>

A partir de ese momento, el empleado puede marcar asistencia con su rostro.

---

<span class="section-label">Eventos</span>

## Tipos de marcaje

| Tipo | Descripcion | Icono |
|---|---|---|
| **Entrada** | Ingreso al inicio de la jornada | :material-login: |
| **Salida** | Salida al finalizar la jornada | :material-logout: |
| **Break inicio** | Inicio de una pausa o receso | :material-coffee: |
| **Break fin** | Fin de la pausa | :material-coffee-off: |

El sistema detecta automaticamente **tardanzas** comparando la hora de marcaje contra el horario asignado, aplicando los minutos de tolerancia configurados por el admin.

---

<span class="section-label">Eficiencia</span>

## Gestion de recursos

El motor de inteligencia artificial **no esta siempre activo**. Se inicia automaticamente cuando se necesita y se detiene tras un periodo de inactividad.

<div class="diagram-box">

```mermaid
stateDiagram-v2
    [*] --> Dormido
    Dormido --> Iniciando: Modulo biometrico requerido
    Iniciando --> Activo: Modelos cargados en RAM
    Activo --> Dormido: Sin actividad (timeout)
    Activo --> Activo: Verificando rostros
    Dormido --> [*]: App se cierra
```

</div>

| Estado | RAM consumida | CPU |
|---|---|---|
| **Dormido** | 0 MB | 0% |
| **Activo** | ~300-500 MB | < 5% por verificacion |
