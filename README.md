<div align="center">

# RAMar Software Studio

**Sistema de Control de Asistencia Biométrico**

[![Documentación](https://img.shields.io/badge/Documentación-Ver%20portal-2e7d32?style=for-the-badge)](https://ramarstudio.github.io/RAMar_Repo/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.10--3.12-3776AB?style=flat-square&logo=python&logoColor=white)](https://www.python.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/Licencia-Propietaria-red?style=flat-square)](./LICENSE)

</div>

---

Aplicación de escritorio Windows que registra la asistencia del personal mediante **reconocimiento facial en tiempo real**. Opera 100% en red local, sin internet, sin almacenar fotografías, con respuestas menores a un segundo.

---

## Características

| | Detalle |
|---|---|
| **Reconocimiento facial** | Identificación en < 1 s con InsightFace ArcFace (embedding 512-D) |
| **Privacidad total** | Cero fotos — solo vectores matemáticos cifrados con AES-256-GCM |
| **100% offline** | Sin internet, sin suscripciones, sin datos en la nube |
| **Panel administrativo** | Dashboard, empleados, horarios, marcajes, reportes, auditoría |
| **Roles** | Empleado · RRHH · Administrador · SuperAdministrador |
| **Configurable** | Tolerancia de tardanzas, horarios, parámetros del sistema |
| **Seguro** | Claves de seguridad persistidas en BD, nunca en archivos de configuración |

---

## Arquitectura

```
┌─────────────────────────────────┐    HTTP localhost:5001   ┌──────────────────────────┐
│   Aplicación WPF — C# .NET 8   │ ◄─────────────────────► │   Motor IA — Python      │
│                                 │                          │                          │
│   · Panel administrativo        │                          │   · FastAPI + Uvicorn    │
│   · Captura de cámara           │                          │   · InsightFace / ArcFace│
│   · Lógica de negocio           │                          │   · Embedding 512-D      │
│   · Entity Framework Core       │                          │   · Inicio bajo demanda  │
│   · Cifrado AES-256-GCM         │                          │   · Auto-stop inactivo   │
└──────────────┬──────────────────┘                          └──────────────────────────┘
               │
               ▼
      ┌──────────────────┐
      │    PostgreSQL     │
      │                  │
      │  · Empleados     │
      │  · Marcajes      │
      │  · Embeddings    │
      │  · Auditoría     │
      │  · Configuración │
      └──────────────────┘
```

---

## Instalación rápida

### Requisitos previos

| Software | Versión | Descarga |
|---|---|---|
| .NET SDK | 8.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Python | 3.10, 3.11 o **3.12** | [python.org](https://www.python.org/downloads/) |
| PostgreSQL | 15+ | [postgresql.org](https://www.postgresql.org/download/windows/) |

**Hardware mínimo:** 8 GB RAM · 4 GB libres en disco · Cámara 720p · Windows 10 64-bit

> ⚠️ **Python 3.13+ no es compatible** con `onnxruntime`. Si ya tienes 3.13, instala 3.12 a la par con `winget install Python.Python.3.12` y usa `py -3.12 -m venv venv` al crear el entorno virtual.

### Ejecutar

```cmd
:: 1. Clonar
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo

:: 2. Configurar base de datos
cd AttendanceSystem\src\AttendanceSystem.App
copy appsettings.example.json appsettings.json
:: Editar appsettings.json y poner tu contraseña de PostgreSQL

:: 3. Instalar librerías de IA (usar cmd, no PowerShell)
cd ..\FaceService
python -m venv venv          :: Si tienes 3.13+: py -3.12 -m venv venv
venv\Scripts\activate.bat
python install.py

:: 4. Correr la aplicación
cd ..\..\..
dotnet run --project src\AttendanceSystem.App
```

**Login inicial:** usuario `admin` / contraseña `admin123`

---

## Estructura del repositorio

```
RAMar_Repo/
├── iniciar.bat                         # Lanzador principal
├── setup.ps1                           # Instalador automático (PowerShell)
├── AttendanceSystem/
│   └── src/
│       ├── AttendanceSystem.App/        # WPF: vistas, controladores, DI
│       ├── AttendanceSystem.Core/       # DTOs, interfaces, entidades
│       ├── AttendanceSystem.Services/   # Lógica de negocio
│       ├── AttendanceSystem.Infrastructure/ # EF Core, repositorios
│       ├── AttendanceSystem.Security/   # AES-256, sesiones, hashing
│       └── FaceService/                 # Microservicio Python (FastAPI)
│           ├── install.py               # Instalador de librerías
│           ├── requirements.txt         # Dependencias con versiones acotadas
│           └── .env.example             # Plantilla de variables de entorno
└── docs/                                # Fuente de la documentación web
```

---

## Documentación

**[ramarstudio.github.io/RAMar_Repo](https://ramarstudio.github.io/RAMar_Repo/)**

| Sección | Contenido |
|---|---|
| [Instalación — Usuario final](https://ramarstudio.github.io/RAMar_Repo/instalacion/guia/#guia-usuario) | Guía paso a paso sin conocimientos técnicos |
| [Instalación — Técnico](https://ramarstudio.github.io/RAMar_Repo/instalacion/guia/#guia-tecnica) | Configuración manual, variables de entorno |
| [Requisitos del sistema](https://ramarstudio.github.io/RAMar_Repo/instalacion/requisitos/) | Hardware, software, espacio en disco |
| [Arquitectura técnica](https://ramarstudio.github.io/RAMar_Repo/arquitectura/) | Diseño del sistema, decisiones de arquitectura |
| [Motor biométrico](https://ramarstudio.github.io/RAMar_Repo/arquitectura/motor-biometrico/) | Cómo funciona el reconocimiento facial |
| [Ingeniería](https://ramarstudio.github.io/RAMar_Repo/ingenieria/) | Metodología, requisitos MoSCoW, diagramas |

---

<div align="center">

© 2026 RAMar Software Studio — Uso exclusivo interno. Todos los derechos reservados.

</div>
