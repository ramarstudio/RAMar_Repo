# Como funciona

## Flujo de marcaje

```mermaid
sequenceDiagram
    participant E as Empleado
    participant W as App WPF
    participant P as Motor IA
    participant D as PostgreSQL

    E->>W: Se acerca a la camara
    W->>P: Envia frame capturado
    P->>P: Detecta rostro + embedding
    P->>W: Vector 512 dimensiones
    W->>D: Busca match por similitud
    D->>W: Empleado identificado
    W->>D: Registra marcaje
    W->>E: Pantalla de confirmacion
```

El proceso completo toma menos de **1 segundo**.

---

## Registro de empleados

Antes de marcar asistencia, un administrador registra el rostro de cada empleado:

1. El admin selecciona al empleado desde el panel
2. Se activa la camara en vivo
3. El empleado se posiciona frente a la camara
4. El admin hace clic en **Capturar rostro**
5. El sistema genera un embedding y lo almacena cifrado

---

## Tipos de marcaje

| Tipo | Descripcion |
|---|---|
| **Entrada** | Ingreso al inicio de la jornada |
| **Salida** | Salida al finalizar la jornada |
| **Break inicio** | Inicio de pausa o receso |
| **Break fin** | Fin de la pausa |

El sistema detecta **tardanzas** automaticamente comparando la hora de marcaje con el horario asignado al empleado.

---

## Gestion de recursos

El motor de IA **no esta siempre activo**. Se inicia cuando se necesita y se detiene por inactividad:

```mermaid
stateDiagram-v2
    [*] --> Dormido
    Dormido --> Activo: Biometria requerida
    Activo --> Dormido: Sin actividad
    Activo --> Activo: Verificando rostros
    Dormido --> [*]: App se cierra
```

| Estado | RAM | CPU |
|---|---|---|
| Dormido | 0 MB | 0% |
| Activo | ~300-500 MB | < 5% por verificacion |
