# Guía paso a paso

Hemos optimizado el proyecto para que **cualquier persona pueda arrancarlo con un par de clics**. El script automatizado hará todo el trabajo pesado por ti (crear entornos virtuales, descargar IA, configurar rutas, etc.).

## 1. Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

---

## 2. Instalar software requerido

Si es un equipo nuevo, instala esto primero:

- [**.NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0) — descargar "SDK" x64
- [**Python 3.10 – 3.13**](https://www.python.org/downloads/) — al instalar, **marca estrictamente "Add Python to PATH"**
- [**PostgreSQL 15+**](https://www.postgresql.org/download/windows/) — al instalar, recuerda la contraseña del usuario `postgres`

---

## 3. Configurar PostgreSQL

En `pgAdmin` o `psql`, simplemente crea una base de datos vacía. El sistema construirá todas las tablas por sí solo.

```sql
CREATE DATABASE AttendanceSystem;
```

---

## 4. Un solo clic: Ejecutar `iniciar.bat`

Dirígete a la carpeta `RAMar_Repo` inicial y haz **doble clic en el archivo `iniciar.bat`**.

El asistente interactivo de consola hará lo siguiente:
1. **Detectará** si es la primera vez que se lanza.
2. **Generará** tu `appsettings.json` y se pausará indicándote que pongas la contraseña de tu base de datos allí.
3. **Creará** el entorno virtual para aislar Python sin ensuciar tu sistema.
4. **Instalará** automáticamente todas las librerías faciales compatibles con tu versión exacta de Python.
5. **Compilará** y ejecutará el software en C#.

!!! tip "Ejecuciones futuras"
    Las próximas veces que lo abras, el script `iniciar.bat` verá que todo está listo y arrancará el panel en menos de 3 segundos.

---

## Primer uso en la App

1. Inicia sesión con las credenciales maestras por defecto:

    | Campo | Valor |
    |---|---|
    | **Usuario** | `admin` |
    | **Contraseña** | `admin123` |

2. **Cambia la contraseña inmediatamente** desde el panel para asegurar tu instancia.
3. Registra a tu primer empleado y escanea su rostro en el **Registro Facial**.
4. ¡Listo! El reloj biométrico ya está en funcionamiento.

!!! info "La descarga IA"
    Si la pantalla se queda cargando un momento al abrir por primera vez la cámara biométrica, no te preocupes. Está descargando los motores matemáticos `InsightFace` (~600 MB silenciosos a internet).
