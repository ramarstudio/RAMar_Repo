---
icon: material/clipboard-check
---

# Requisitos previos

## Sistema operativo

- **Windows 10** u **11** (x64)

---

## Software requerido

| Componente | Version minima | Funcion |
|---|---|---|
| .NET 8 Desktop Runtime | 8.0+ | Ejecutar la aplicacion WPF |
| Python | 3.10+ | Motor de reconocimiento facial |
| PostgreSQL | 15+ | Base de datos |

<div class="tech-stack">
<span class="tech-pill">:material-microsoft-windows: Windows 10/11</span>
<span class="tech-pill">:material-language-csharp: .NET 8</span>
<span class="tech-pill">:material-language-python: Python 3.10+</span>
<span class="tech-pill">:material-database: PostgreSQL 15+</span>
</div>

---

## Hardware

| Componente | Requisito |
|---|---|
| **Camara web** | Cualquier webcam USB o integrada (DirectShow) |
| **RAM** | 4 GB minimo — 8 GB recomendado |
| **CPU** | x64 con soporte SSE2 |
| **Disco** | ~500 MB (modelos IA + aplicacion) |

!!! info "Sobre la memoria RAM"
    El motor de reconocimiento facial consume ~300-500 MB de RAM **solo cuando esta activo**. La aplicacion lo inicia automaticamente cuando se necesita y lo detiene tras un periodo de inactividad, liberando la memoria.

---

## Puertos de red

| Puerto | Servicio | Acceso |
|---|---|---|
| `5432` | PostgreSQL | Red local |
| `8000` | Motor facial | Solo localhost |

!!! warning "Firewall"
    El puerto `8000` escucha exclusivamente en `127.0.0.1`. No es necesario abrirlo en el firewall — la comunicacion es interna al equipo.
