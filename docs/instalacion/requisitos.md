# Requisitos previos

## Sistema operativo

- **Windows 10** u **11** (x64)

---

## Software requerido

| Componente | Versión mínima | Función |
|---|---|---|
| .NET 8 Desktop Runtime | 8.0+ | Ejecutar la aplicación WPF |
| Python | 3.10+ | Motor de reconocimiento facial |
| PostgreSQL | 15+ | Base de datos |

---

## Hardware

| Componente | Requisito |
|---|---|
| **Cámara web** | Cualquier webcam USB o integrada (DirectShow) |
| **RAM** | 4 GB mínimo — 8 GB recomendado |
| **CPU** | x64 con soporte SSE2 |
| **Disco** | ~500 MB (modelos IA + aplicación) |

!!! info "Sobre la memoria RAM"
    El motor de reconocimiento facial consume ~300-500 MB de RAM **solo cuando está activo**. La aplicación lo inicia automáticamente cuando se necesita y lo detiene tras un periodo de inactividad, liberando la memoria.

---

## Puertos de red

| Puerto | Servicio | Acceso |
|---|---|---|
| `5432` | PostgreSQL | Red local |
| `8000` | Motor facial | Solo localhost |

!!! warning "Firewall"
    El puerto `8000` escucha exclusivamente en `127.0.0.1`. No es necesario abrirlo en el firewall — la comunicación es interna al equipo.
