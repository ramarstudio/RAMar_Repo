# Requisitos previos

## Sistema operativo

- **Windows 10** u **11** (x64)

## Software requerido

| Componente | Version minima | Uso |
|---|---|---|
| .NET 8 Desktop Runtime | 8.0+ | Ejecutar la aplicacion WPF |
| Python | 3.10+ | Motor de reconocimiento facial |
| PostgreSQL | 15+ | Base de datos de empleados, marcajes y embeddings |

## Hardware

| Componente | Requisito |
|---|---|
| **Camara web** | Cualquier webcam USB o integrada compatible con DirectShow |
| **RAM** | 4 GB minimo (8 GB recomendado cuando el motor IA esta activo) |
| **CPU** | Procesador x64 con soporte SSE2 |
| **Disco** | ~500 MB para modelos de IA + aplicacion |

!!! info "Sobre la memoria RAM"
    El motor de reconocimiento facial consume aproximadamente 300-500 MB de RAM cuando esta activo. La aplicacion lo inicia automaticamente solo cuando se necesita y lo detiene tras un periodo de inactividad.

## Puertos de red

| Puerto | Servicio |
|---|---|
| `5432` | PostgreSQL |
| `8000` | Motor facial (solo localhost) |

!!! warning "Firewall"
    El puerto `8000` solo escucha en `localhost`. No es necesario abrirlo en el firewall ya que la comunicacion es interna al equipo.
