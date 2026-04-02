---
icon: material/sitemap
---

# Arquitectura tecnica

<span class="section-label">Vista general</span>

El sistema sigue una **arquitectura hibrida de dos procesos** que separa la capa visual de la inteligencia biometrica, comunicandose via HTTP en localhost.

<div class="diagram-box">

```mermaid
graph TB
    subgraph Desktop["App WPF — C# .NET 8"]
        UI[Interfaz grafica]
        CAM[Captura DirectShow]
        BL[Logica de negocio]
        EF[Entity Framework Core]
    end

    subgraph Python["Motor biometrico — Python FastAPI"]
        API[REST API :8000]
        IF[InsightFace / ArcFace]
    end

    subgraph DB["Almacenamiento"]
        PG[(PostgreSQL)]
    end

    UI --> CAM --> BL
    BL -->|HTTP POST| API --> IF
    IF -->|Embedding| BL
    BL --> EF --> PG
```

</div>

---

<span class="section-label">Capas del sistema</span>

## Aplicacion WPF (C#)

| Proyecto | Responsabilidad |
|---|---|
| `AttendanceSystem.App` | Interfaz grafica, vistas XAML, code-behind |
| `AttendanceSystem.Core` | DTOs, interfaces, enums — capa sin dependencias |
| `AttendanceSystem.Services` | Logica de negocio, reglas de marcaje |
| `AttendanceSystem.Infrastructure` | Acceso a datos, repositorios, EF Core |
| `AttendanceSystem.Security` | Autenticacion, cifrado AES, sesiones |

## Motor biometrico (Python)

| Modulo | Responsabilidad |
|---|---|
| `api/` | Endpoints REST con FastAPI |
| `core/` | Interfaces abstractas de deteccion y reconocimiento |
| `adapters/` | Implementaciones concretas (InsightFace) |
| `services/` | Orquestacion del pipeline biometrico |

---

<span class="section-label">Decisiones</span>

## Documentos de arquitectura

<div class="grid cards" markdown>

-   :material-scale-balance:{ .lg .middle } **ADR: WPF vs Web**

    ---

    Por que se descarto React a favor de una app nativa de escritorio.

    [:octicons-arrow-right-24: Leer decision](adr-wpf.md)

-   :material-face-recognition:{ .lg .middle } **Motor biometrico**

    ---

    InsightFace, ArcFace, cifrado y gestion de recursos del servicio Python.

    [:octicons-arrow-right-24: Ver detalles](motor-biometrico.md)

</div>

---

<span class="section-label">Principios</span>

## Principios de diseno

- **Abstraccion** — Interfaces para detector y reconocedor facial (intercambiables)
- **Eficiencia** — Motor IA bajo demanda, se detiene por inactividad
- **Seguridad** — Embeddings cifrados AES-256, comunicacion solo localhost
- **Separacion** — Cada capa tiene una unica responsabilidad
- **Flexibilidad** — Cambiar de modelo solo requiere un nuevo adapter
