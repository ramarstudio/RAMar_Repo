# Guía de instalación

---

## Requisitos previos

Instala estos tres programas antes de continuar:

**PostgreSQL 15+**

1. Descarga desde [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)
2. Sigue el instalador — cuando pida contraseña para `postgres`, elige una que recuerdes
3. Puerto: déjalo en `5432`

**Python 3.12** (obligatorio — 3.13+ no es compatible)

Abre una terminal y verifica si ya tienes una versión compatible:

```cmd
python --version
```

| Resultado | Qué hacer |
|---|---|
| `Python 3.10.x`, `3.11.x` o `3.12.x` | ✅ Ya estás listo, salta al siguiente requisito |
| `Python 3.13.x` o superior | ⚠️ Versión incompatible — instala 3.12 **a la par** (ver abajo) |
| `"Python was not found"` o error | ❌ No tienes Python — instálalo (ver abajo) |

**Instalar Python 3.12:**

```cmd
winget install Python.Python.3.12
```

O descarga manualmente desde [python.org/downloads](https://www.python.org/downloads/) (busca **3.12.x**) — marca **"Add Python to PATH"** durante la instalación.

Después de instalar, **cierra y reabre la terminal**.

!!! danger "Python 3.13+ no es compatible"
    `onnxruntime` (motor de IA) no soporta Python 3.13 aún. Si ya tienes 3.13, no lo desinstales — instala 3.12 junto a él. El paso 3 te muestra cómo forzar la versión correcta.

**.NET 8 SDK**

1. Descarga desde [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Descarga el **SDK x64**
3. Ejecuta el instalador

---

## Instalación paso a paso

### 1. Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

### 2. Configurar la base de datos

```bash
cd AttendanceSystem/src/AttendanceSystem.App
copy appsettings.example.json appsettings.json
```

Abre `appsettings.json` y reemplaza `CAMBIAR_POR_TU_CONTRASEÑA` con la contraseña que pusiste al instalar PostgreSQL:

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=AttendanceSystem;Username=postgres;Password=TU_CONTRASEÑA"
```

!!! info "La base de datos se crea sola"
    En el repositorio **solo existe el archivo `appsettings.example.json`**. Es obligatorio que lo copies/renombres a `appsettings.json` y modifiques únicamente la contraseña. 
    Al correr la aplicación por primera vez, **Entity Framework usará esa contraseña para crear automáticamente** toda la base de datos y sus tablas de forma transparente. No necesitas saber ni usar comandos SQL.

### 3. Instalar librerías de IA

```bash
cd ../FaceService
```

Verifica qué versión de Python usarás:

```cmd
python --version
```

**Según el resultado, elige cómo crear el entorno virtual:**

=== "Python 3.10–3.12 (o recién instalado)"

    Tu `python` ya apunta a una versión compatible. Crea el entorno directamente:

    ```cmd
    python -m venv venv
    ```

=== "Python 3.13+ (necesitas forzar 3.12)"

    Si `python --version` muestra 3.13+, usa el **Python Launcher** (`py`) para forzar la versión 3.12 que instalaste a la par:

    ```cmd
    py -3.12 -m venv venv
    ```

    !!! tip "¿Cómo funciona `py -3.12`?"
        Windows instala un programa llamado **Python Launcher** (`py.exe`) que permite elegir entre varias versiones de Python instaladas. `py -3.12` le dice que use específicamente la 3.12, ignorando la 3.13.

Luego, activa el entorno e instala las dependencias:

```cmd
venv\Scripts\activate
python install.py
```

La primera vez tarda entre **2 y 5 minutos** — descarga el modelo de reconocimiento facial (~600 MB).

!!! note "Verificación rápida"
    Después de activar el venv, ejecuta `python --version` — debe mostrar 3.10, 3.11 o 3.12 sin importar qué versión tengas instalada globalmente.

### 4. Correr la aplicación

```bash
cd ../../..
dotnet run --project src/AttendanceSystem.App
```

---

## Primer inicio de sesión

- **Usuario:** `admin`
- **Contraseña:** `admin123`

!!! tip "Primeros pasos recomendados"
    1. Ve a **Usuarios** → crea los empleados del sistema
    2. Ve a **Registro Facial** → captura el rostro de cada empleado
    3. Ve a **Horarios** → configura los horarios de entrada y salida
    4. Ve a **Configuración** → ajusta tolerancias y parámetros

---

## Solución de problemas

| Problema | Causa probable | Solución |
|---|---|---|
| La app no abre y sale error de base de datos | PostgreSQL no está corriendo | Abre **Servicios** de Windows → inicia `postgresql` |
| "Python no compatible" | Tienes Python 3.13+ | Instala 3.12 a la par: `winget install Python.Python.3.12` y crea el venv con `py -3.12 -m venv venv` |
| "Python no encontrado" | Python no está instalado o no está en PATH | Ejecuta `winget install Python.Python.3.12`, cierra y reabre la terminal |
| Error de biometría / ONNX | Falta Visual C++ | Instala [vc_redist.x64.exe](https://aka.ms/vs/17/release/vc_redist.x64.exe) |
| "Cámara no abre" | Otra app la está usando | Cierra Zoom, Teams u otra app con la cámara |

!!! note "Reconfigurar la base de datos"
    Si cambiaste la contraseña de PostgreSQL, edita directamente:
    ```
    AttendanceSystem/src/AttendanceSystem.App/appsettings.json
    ```

---

## Variables de entorno del FaceService (opcional)

El servicio de IA se configura con un archivo `.env` en `AttendanceSystem/src/FaceService/`:

```bash
copy .env.example .env
```

```env
FACE_HOST=0.0.0.0
FACE_PORT=5001
FACE_DETECTION_MODEL=buffalo_l
FACE_GPU_ID=-1          # -1 = CPU, 0 = primera GPU
FACE_SIMILARITY_THRESHOLD=0.60
FACE_MAX_FACES_PER_IMAGE=1
FACE_API_KEY=           # vacío = sin autenticación
FACE_LOG_LEVEL=INFO
```

Si no creas el `.env`, el servicio usa estos valores por defecto automáticamente.
