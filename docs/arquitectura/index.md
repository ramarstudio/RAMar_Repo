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
| `AttendanceSystem.Core` | DTOs, interfaces, enums (capa 0 — sin dependencias) |
| `AttendanceSystem.Services` | Logica de negocio, reglas de marcaje |
| `AttendanceSystem.Infrastructure` | Acceso a datos, repositorios, EF Core |
| `AttendanceSystem.Security` | Autenticacion, cifrado AES, gestion de sesiones |

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

<div class="feature-grid" markdown>

<div class="feature-card">
<span class="card-icon">:material-scale-balance:</span>

### ADR: WPF vs Web

Por que se descarto React a favor de una app nativa de escritorio.

[Leer decision](adr-wpf.md){ .md-button }
</div>

<div class="feature-card">
<span class="card-icon">:material-face-recognition:</span>

### Motor biometrico

InsightFace, ArcFace, cifrado y gestion de recursos del servicio Python.

[Ver detalles](motor-biometrico.md){ .md-button }
</div>

</div>

---

<span class="section-label">Principios</span>

## Principios de diseno

- **Abstraccion** — Interfaces para detector y reconocedor facial (intercambiables sin tocar el resto)
- **Eficiencia** — Motor IA bajo demanda, se detiene por inactividad
- **Seguridad** — Embeddings cifrados AES-256, comunicacion solo por localhost
- **Separacion** — Cada capa tiene una unica responsabilidad
- **Flexibilidad** — Cambiar de InsightFace a otro modelo solo requiere un nuevo adapter
