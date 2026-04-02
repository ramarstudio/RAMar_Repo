# Arquitectura tecnica

El sistema sigue una **arquitectura hibrida de dos procesos** que separa la capa visual de la inteligencia biometrica, comunicandose mediante HTTP local.

---

## Diagrama general

```mermaid
graph TB
    subgraph Desktop["Aplicacion WPF (C# .NET 8)"]
        UI[Interfaz grafica]
        CAM[Captura de camara — DirectShow]
        BL[Logica de negocio]
        EF[Entity Framework Core]
    end

    subgraph Python["Motor biometrico (Python FastAPI)"]
        API[API REST — localhost:8000]
        IF[InsightFace — ArcFace]
        DET[Deteccion de rostro]
        EMB[Generacion de embedding]
    end

    subgraph DB["Base de datos"]
        PG[(PostgreSQL)]
    end

    UI --> CAM
    CAM --> BL
    BL -->|HTTP POST| API
    API --> DET --> EMB
    EMB -->|Vector 512-d| BL
    BL --> EF --> PG
```

---

## Componentes

### App WPF (C# .NET 8)

| Capa | Responsabilidad |
|---|---|
| `AttendanceSystem.App` | Interfaz grafica, vistas, code-behind |
| `AttendanceSystem.Core` | DTOs, interfaces, enums |
| `AttendanceSystem.Services` | Logica de negocio |
| `AttendanceSystem.Infrastructure` | Acceso a datos, Entity Framework |
| `AttendanceSystem.Security` | Autenticacion, cifrado, sesiones |

### Motor biometrico (Python)

| Modulo | Responsabilidad |
|---|---|
| `api/` | Endpoints REST (FastAPI) |
| `core/` | Interfaces abstractas de deteccion y reconocimiento |
| `adapters/` | Implementacion concreta con InsightFace |
| `services/` | Logica de negocio biometrica |

### Base de datos (PostgreSQL)

Tablas principales: empleados, marcajes, horarios, embeddings faciales (cifrados), usuarios, auditoria, configuracion del sistema.

---

## Principios de diseno

- **Abstraccion**: interfaces para detector y reconocedor (intercambiables)
- **Eficiencia**: el motor IA se inicia bajo demanda y se detiene por inactividad
- **Seguridad**: embeddings cifrados con AES-256, comunicacion solo por localhost
- **Separacion de responsabilidades**: cada capa tiene una unica razon de cambio
