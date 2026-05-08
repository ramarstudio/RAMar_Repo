# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**RAMar** is a biometric attendance system for Windows. It combines a C# .NET 8 WPF desktop app with a Python FastAPI microservice for real-time facial recognition via InsightFace (ArcFace). All processing is offline; no photos are stored — only AES-256-GCM encrypted 512-dimensional embeddings.

## Architecture

```
WPF App (C# .NET 8)  ←→  HTTP localhost:5001  ←→  FastAPI (Python 3.10-3.12)
         ↓                                                     ↓
    PostgreSQL 15+                                     InsightFace (ArcFace)
```

### .NET Solution — `AttendanceSystem/AttendanceSystem.sln`

| Project | Role |
|---|---|
| `AttendanceSystem.App` | WPF entry point, DI wiring, navigation, views |
| `AttendanceSystem.Core` | DTOs, entities, interfaces, domain options |
| `AttendanceSystem.Infrastructure` | EF Core DbContext, Npgsql, migrations, repos |
| `AttendanceSystem.Security` | AES-256-GCM encryption, auth logic |
| `AttendanceSystem.Services` | Business logic layer |

**Startup flow** (`App.xaml.cs`): validates `appsettings.json` → loads config → loads AES keys from DB → configures DI → runs EF migrations + seed (roles, admin user) → shows `LoginView` → role-based nav to `AdminShellView`.

### Python Microservice — `AttendanceSystem/src/FaceService/`

| File | Role |
|---|---|
| `run.py` | Uvicorn entry point |
| `app/main.py` | FastAPI app, lifespan (model load/cleanup) |
| `app/config.py` | Pydantic Settings; all vars prefixed `FACE_` |
| `app/adapters/insightface_engine.py` | Lazy-loads InsightFace model |
| `app/api/routes.py` | `/api/` endpoints: embed + similarity |
| `app/core/similarity.py` | Cosine distance |

Single Uvicorn worker only — InsightFace must not be loaded N times.

### Key Database Entities

`Rol`, `Usuario`, `Empleado`, `Horario`, `Consentimiento`, `EmbeddingFacial`, `Marcaje`, `AuditLog`, `Configuracion`

EF Core migrations run automatically on startup; no manual migration step needed in development.

## Development Commands

### Run the WPF app
```powershell
dotnet run --project AttendanceSystem\src\AttendanceSystem.App
```

### Build the solution
```powershell
dotnet build AttendanceSystem\AttendanceSystem.sln
```

### First-time configuration
```powershell
cd AttendanceSystem\src\AttendanceSystem.App
copy appsettings.example.json appsettings.json
# Edit appsettings.json — set PostgreSQL password
```

### Set up and run the Python FaceService
```powershell
cd AttendanceSystem\src\FaceService
python install.py        # creates venv, installs insightface + deps
# then to run manually:
venv\Scripts\python run.py
```

`install.py` detects Python 3.13+ and falls back to `py -3.12`; downloads the Windows insightface wheel from the community repo if pip install fails.

### Build the Windows installer
```powershell
.\AttendanceSystem\installer\build_installer.ps1
# Requires Inno Setup 6+ installed
# Publishes self-contained .NET app + calls setup_faceservice.bat
```

### Documentation (MkDocs)
```powershell
mkdocs serve   # local preview at http://127.0.0.1:8000
mkdocs build   # static site
```

Docs are auto-deployed to GitHub Pages via `.github/workflows/deploy-docs.yml`.

## Configuration

**`appsettings.json`** (copy from `appsettings.example.json`, never commit with real credentials):
- PostgreSQL connection string
- Database initialization flags

**`AttendanceSystem/src/FaceService/.env`** (copy from `.env.example`):
- `FACE_HOST`, `FACE_PORT` (default `0.0.0.0:5001`)
- `FACE_DETECTION_MODEL=buffalo_l`
- `FACE_SIMILARITY_THRESHOLD=0.60`
- `FACE_MAX_FACES_PER_IMAGE=1`

Default admin login: `admin` / `admin123`

## Key Constraints

- **FaceService must use a single Uvicorn worker** — multi-worker would load InsightFace multiple times and exhaust GPU/RAM.
- **No photo storage** — only encrypted embeddings (`EmbeddingFacial`). Treat embedding bytes as sensitive data.
- **Windows only** — WPF + Inno Setup + Windows-specific insightface wheel.
- **Python 3.10–3.12** — InsightFace wheels are not available for 3.13+; `install.py` enforces this.
- AES-256-GCM keys are loaded from the database at startup; the app cannot start if the DB is unreachable.
