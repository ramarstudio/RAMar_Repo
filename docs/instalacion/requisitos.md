# Requisitos previos

## Sistema operativo

- **Windows 10** u **11** (x64)

---

## Software requerido

| Componente | Versión | Descarga | Función |
|---|---|---|---|
| .NET 8 SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) | Compilar y ejecutar la aplicación WPF |
| Python | 3.10 – 3.13 | [python.org](https://www.python.org/downloads/) | Motor de reconocimiento facial (InsightFace) |
| PostgreSQL | 15+ | [postgresql.org](https://www.postgresql.org/download/windows/) | Base de datos |

!!! warning "Python: marcar 'Add to PATH'"
    Al instalar Python, asegúrate de marcar la casilla **"Add Python to PATH"** en la primera pantalla del instalador. Sin esto, la aplicación no podrá iniciar el motor facial automáticamente.

!!! info "Instalación de dependencias Python"
    El proyecto incluye un script `install.py` que detecta tu versión de Python y descarga automáticamente el wheel correcto de `insightface` para Windows. Usar `python install.py` en vez de `pip install -r requirements.txt`.

---

## Hardware

| Componente | Requisito |
|---|---|
| **Cámara web** | Cualquier webcam USB o integrada (DirectShow) |
| **RAM** | 4 GB mínimo — 8 GB recomendado |
| **CPU** | x64 con soporte SSE2 |
| **Disco** | ~1 GB (aplicación + modelos IA + dependencias Python) |

!!! info "Sobre la memoria RAM"
    El motor de reconocimiento facial consume ~400-500 MB de RAM **solo cuando está activo**. La aplicación lo inicia automáticamente cuando se necesita y lo detiene tras 10 minutos de inactividad, liberando la memoria completamente.

---

## Puertos de red

| Puerto | Servicio | Acceso |
|---|---|---|
| `5432` | PostgreSQL | Red local |
| `5001` | Motor facial (FaceService) | Solo localhost |

!!! warning "Firewall"
    El puerto `5001` escucha exclusivamente en `127.0.0.1`. No es necesario abrirlo en el firewall — la comunicación es interna al equipo. La aplicación C# arranca y detiene el servicio Python automáticamente.

---

## Conexión a internet

| Cuándo | Para qué | Obligatorio |
|---|---|---|
| Solo la primera vez | Descargar modelos InsightFace (~600 MB) | Sí |
| Después | El sistema funciona 100% offline | No |
