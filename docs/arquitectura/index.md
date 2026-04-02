# Arquitectura técnica

El sistema usa una **arquitectura híbrida de dos procesos** que separa la interfaz gráfica de la inteligencia biométrica.

```mermaid
graph TB
    subgraph WPF["App WPF — C# .NET 8"]
        UI[Interfaz gráfica]
        CAM[Captura DirectShow]
        BL[Lógica de negocio]
        EF[Entity Framework Core]
    end

    subgraph PY["Motor biométrico — Python"]
        API[FastAPI :8000]
        IF[InsightFace / ArcFace]
    end

    subgraph DB["Base de datos"]
        PG[(PostgreSQL)]
    end

    UI --> CAM --> BL
    BL -->|HTTP POST| API --> IF
    IF -->|Embedding| BL
    BL --> EF --> PG
```

---

## Capas de la aplicación WPF

| Proyecto | Responsabilidad |
|---|---|
| `AttendanceSystem.App` | Interfaz gráfica, vistas XAML |
| `AttendanceSystem.Core` | DTOs, interfaces, enums |
| `AttendanceSystem.Services` | Lógica de negocio |
| `AttendanceSystem.Infrastructure` | Acceso a datos, EF Core |
| `AttendanceSystem.Security` | Autenticación, cifrado AES, sesiones |

## Motor biométrico (Python)

| Módulo | Responsabilidad |
|---|---|
| `api/` | Endpoints REST (FastAPI) |
| `core/` | Interfaces abstractas |
| `adapters/` | Implementación InsightFace |
| `services/` | Pipeline biométrico |

---

## Principios de diseño

- **Abstracción** — Interfaces intercambiables para detector y reconocedor
- **Eficiencia** — Motor IA bajo demanda, se detiene por inactividad
- **Seguridad** — Embeddings cifrados AES-256, solo localhost
- **Separación** — Cada capa tiene una única responsabilidad
