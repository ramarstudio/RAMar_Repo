<div align="center">

# RAMar Software Studio

**Innovación, Privacidad Computacional y Soluciones Corporativas**

[![Documentación](https://img.shields.io/badge/Documentaci%C3%B3n-Ver%20portal-2e7d32?style=for-the-badge)](https://ramarstudio.github.io/RAMar_Repo/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.13-3776AB?style=flat-square&logo=python&logoColor=white)](https://www.python.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/Licencia-Propietaria-red?style=flat-square)](./LICENSE)

</div>

---

## Sistema de Control de Asistencia Biométrico

Aplicación de escritorio que registra la asistencia del personal mediante **reconocimiento facial** en tiempo real. Opera 100% en la red local, sin internet, sin almacenar fotografías, con respuestas menores a un segundo.

### Características

| | Detalle |
|---|---|
| **Reconocimiento facial** | Identificación en < 1 segundo con InsightFace (ArcFace) |
| **Privacidad** | Cero fotos — solo vectores matemáticos cifrados con AES-256 |
| **Offline** | Funciona sin internet, sin suscripciones, sin datos en la nube |
| **Panel admin** | Dashboard, empleados, horarios, marcajes, reportes, auditoría |
| **Roles** | Empleado · Administrador · SuperAdministrador |

### Arquitectura

```
┌──────────────────────────────┐     HTTP localhost     ┌─────────────────────────┐
│   App WPF — C# .NET 8       │ ◄────────────────────► │  Motor IA — Python      │
│                              │                        │                         │
│   · Interfaz gráfica         │                        │  · FastAPI              │
│   · Captura de cámara        │                        │  · InsightFace/ArcFace  │
│   · Lógica de negocio        │                        │  · Embedding 512-d      │
│   · Entity Framework Core    │                        │  · Inicio bajo demanda  │
└──────────────┬───────────────┘                        └─────────────────────────┘
               │
               ▼
      ┌─────────────────┐
      │   PostgreSQL     │
      │                  │
      │  · Empleados     │
      │  · Marcajes      │
      │  · Embeddings    │
      │  · Auditoría     │
      └─────────────────┘
```

### Estructura del repositorio

```
RAMar_Repo/
├── AttendanceSystem/
│   └── src/
│       ├── AttendanceSystem.App/            # Interfaz WPF
│       ├── AttendanceSystem.Core/           # DTOs, interfaces, enums
│       ├── AttendanceSystem.Services/       # Lógica de negocio
│       ├── AttendanceSystem.Infrastructure/ # Acceso a datos (EF Core)
│       ├── AttendanceSystem.Security/       # Autenticación, cifrado AES
│       └── FaceService/                     # Motor biométrico (Python)
├── Projects/                                # Planeamiento e ingeniería
├── docs/                                    # Código fuente de la documentación
└── mkdocs.yml                               # Configuración del portal web
```

### Inicio rápido

```bash
# 1. Clonar
git clone https://github.com/ramarstudio/RAMar_Repo.git

# 2. Configurar la base de datos PostgreSQL y el archivo .env

# 3. Instalar dependencias de Python
cd AttendanceSystem/src/FaceService && pip install -r requirements.txt

# 4. Compilar y ejecutar
cd AttendanceSystem && dotnet run --project src/AttendanceSystem.App
```

Para la guía completa de instalación, consulta la **[documentación](https://ramarstudio.github.io/RAMar_Repo/instalacion/guia/)**.

---

<div align="center">

**[Documentación completa](https://ramarstudio.github.io/RAMar_Repo/)** · **[Producto](https://ramarstudio.github.io/RAMar_Repo/producto/)** · **[Arquitectura](https://ramarstudio.github.io/RAMar_Repo/arquitectura/)** · **[Ingeniería](https://ramarstudio.github.io/RAMar_Repo/ingenieria/)**

</div>

---

> Este repositorio es de uso exclusivo para almacenamiento, administración y gestión interna.
> No está destinado a distribución pública, reutilización ni referencia externa.
> © RAMar Software Studio. Todos los derechos reservados.
