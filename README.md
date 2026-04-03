<div align="center">

# RAMar Software Studio

**Privacidad Computacional y Soluciones de Asistencia Biométrica**

[![Documentación](https://img.shields.io/badge/Documentaci%C3%B3n-Ver%20portal-2e7d32?style=for-the-badge)](https://ramarstudio.github.io/RAMar_Repo/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.10+-3776AB?style=flat-square&logo=python&logoColor=white)](https://www.python.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

</div>

---

## ⚡ Instalación en 3 Pasos (Zero-Touch)

Hemos optimizado todo para que no pierdas tiempo editando archivos de configuración. 

### 1. Preparación
Instala [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [Python 3.12](https://www.python.org/downloads/) (marcando **Add to PATH**) y [PostgreSQL](https://www.postgresql.org/download/windows/).

### 2. Base de Datos
Abre pgAdmin y crea una base de datos vacía llamada `AttendanceSystem`.

### 3. ¡Ejecutar!
Haz doble clic en **`iniciar.bat`**. El asistente te pedirá tu clave de PostgreSQL y configurará todo el sistema por ti.

---

## 🛡️ Sobre el Sistema

El **RAMar Attendance System** es una solución de escritorio corporativa que utiliza **reconocimiento facial de última generación** (InsightFace) para gestionar el control de asistencia.

- **Identificación Instantánea**: Reconocimiento en menos de 1 segundo.
- **Privacidad por Diseño**: No guarda fotos. Solo vectores matemáticos cifrados con AES-256.
- **Operación Local**: Funciona 100% offline. Tus datos biométricos nunca salen de tu empresa.
- **Gestión Integral**: Dashboard, gestión de empleados, reportes de asistencia y auditoría.

---

## 🏗️ Arquitectura Técnica

```
┌──────────────────────────────┐     HTTP localhost:5001  ┌─────────────────────────┐
│   App WPF — C# .NET 8       │ ◄────────────────────►  │  Motor IA — Python      │
│                              │                          │                         │
│   · Interfaz gráfica         │                          │  · FastAPI + Uvicorn    │
│   · Captura de cámara        │                          │  · InsightFace/ArcFace  │
│   · Lógica de negocio        │                          │  · Embedding 512-d      │
│   · Entity Framework Core    │                          │  · Inicio bajo demanda  │
└──────────────┬───────────────┘                          └─────────────────────────┘
               │
               ▼
      ┌─────────────────┐
      │   PostgreSQL     │
      │                  │
      │  · Empleados     │
      │  · Marcajes      │
      │  · Embeddings    │
      └─────────────────┘
```

---

## 📖 Documentación

| Guía | Contenido |
| :--- | :--- |
| [**Manual de Instalación**](https://ramarstudio.github.io/RAMar_Repo/instalacion/guia/) | Paso a paso detallado para nuevos usuarios. |
| [**Requisitos de Sistema**](https://ramarstudio.github.io/RAMar_Repo/instalacion/requisitos/) | Software y hardware necesario. |
| [**Arquitectura de Red**](https://ramarstudio.github.io/RAMar_Repo/arquitectura/motor-biometrico/) | Cómo funciona el motor biométrico. |

---

<div align="center">

> **RAMar Software Studio** — Innovación, privacidad computacional y soluciones corporativas.
> © 2026 Todos los derechos reservados.

</div>
