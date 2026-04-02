# Arquitectura tecnica

El sistema usa una **arquitectura hibrida de dos procesos** que separa la interfaz grafica de la inteligencia biometrica.

```mermaid
graph TB
    subgraph WPF["App WPF — C# .NET 8"]
        UI[Interfaz grafica]
        CAM[Captura DirectShow]
        BL[Logica de negocio]
        EF[Entity Framework Core]
    end

    subgraph PY["Motor biometrico — Python"]
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

## Capas de la aplicacion WPF

| Proyecto | Responsabilidad |
|---|---|
| `AttendanceSystem.App` | Interfaz grafica, vistas XAML |
| `AttendanceSystem.Core` | DTOs, interfaces, enums |
| `AttendanceSystem.Services` | Logica de negocio |
| `AttendanceSystem.Infrastructure` | Acceso a datos, EF Core |
| `AttendanceSystem.Security` | Autenticacion, cifrado AES, sesiones |

## Motor biometrico (Python)

| Modulo | Responsabilidad |
|---|---|
| `api/` | Endpoints REST (FastAPI) |
| `core/` | Interfaces abstractas |
| `adapters/` | Implementacion InsightFace |
| `services/` | Pipeline biometrico |

---

## Principios de diseno

- **Abstraccion** — Interfaces intercambiables para detector y reconocedor
- **Eficiencia** — Motor IA bajo demanda, se detiene por inactividad
- **Seguridad** — Embeddings cifrados AES-256, solo localhost
- **Separacion** — Cada capa tiene una unica responsabilidad

---

**Ver tambien:** [WPF vs Web (ADR)](adr-wpf.md) | [Motor biometrico](motor-biometrico.md)
