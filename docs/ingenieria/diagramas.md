# Diagramas

Antes de escribir código, se mapearon las interacciones entre los actores del sistema y sus límites.

---

## Diagrama de casos de uso

Este diagrama define cómo cada actor de la empresa interactúa con el sistema, delimitando las capacidades de cada rol.

![Diagrama de casos de uso](../assets/diagrama.jpeg)

!!! note "Sobre el rol del empleado"
    El empleado opera en modo **pasivo-rápido**: solo se acerca a la cámara y recibe confirmación. No interactúa con teclados, botones ni menús.

---

## Diagrama de arquitectura

```mermaid
graph TB
    subgraph Cliente["Kiosco (PC Windows)"]
        WPF[App WPF — C# .NET 8]
        PY[Motor IA — Python FastAPI]
        WPF -->|HTTP localhost| PY
    end

    subgraph Servidor["Servidor local"]
        DB[(PostgreSQL)]
    end

    WPF -->|Entity Framework Core| DB
    PY -->|InsightFace + ArcFace| WPF
```

---

## Flujo de marcaje

```mermaid
sequenceDiagram
    actor E as Empleado
    participant K as Kiosco (WPF)
    participant IA as Motor IA (Python)
    participant DB as PostgreSQL

    E->>K: Se acerca a la cámara
    K->>IA: Envía frame capturado
    IA->>IA: Detecta rostro y genera embedding
    IA-->>K: Vector 512-d
    K->>DB: Busca match por similitud coseno
    DB-->>K: Empleado identificado
    K->>DB: Registra marcaje (entrada/salida)
    K-->>E: Pantalla de confirmación
```
