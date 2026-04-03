# Requisitos previos (Solo 3 cosas)

Para que el asistente de una sola ejecución (`iniciar.bat`) funcione sin errores, asegúrate de tener estos tres componentes instalados antes de arrancar.

---

## 1. El Portal de Datos (PostgreSQL)

El sistema usa **PostgreSQL 15+** para almacenar empleados, marcajes y los vectores matemáticos de los rostros.

- [**Descargar PostgreSQL para Windows**](https://www.postgresql.org/download/windows/)
- Durante la instalación, ponle una contraseña que puedas recordar.
- Crea una base de datos vacía llamada **`AttendanceSystem`** (mediante `pgAdmin` o la consola).

---

## 2. El Motor de Aplicación (.NET)

El panel de control está construido sobre **.NET 8**.

- [**Descargar .NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0)
- Instala la versión de **64 bits (x64)**.

---

## 3. El Motor Inteligente (Python)

La biometría facial de clase mundial (InsightFace) corre sobre Python.

- [**Descargar Python 3.10+**](https://www.python.org/downloads/)
- !!! warning "Paso Crítico"
    Al instalar Python, **MARCA OBLIGATORIAMENTE la casilla "Add Python to PATH"** en la primera pantalla del instalador.

---

## ⚡ Librería de compatibilidad (C++ Runtime)

Para que la IA procese los vectores matemáticos a alta velocidad, Windows necesita los binarios de ejecución de C++. La mayoría de computadoras ya los tienen, pero si recibes un **"Error de Biometría"**, esta es la solución más común:

- [**Descargar Visual C++ Redistributable (vc_redist.x64.exe)**](https://aka.ms/vs/17/release/vc_redist.x64.exe)

---

!!! info "Hardware recomendado"
    - **Cámara:** Web USB estándar o integrada con resolución 720p+.
    - **Memoria RAM:** 8 GB recomendado (funciona con 4 GB).
    - **Procesador:** Intel Core i3 / AMD Ryzen 3 o superior.
