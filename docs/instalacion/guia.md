# Guía de instalación

---

## Requisitos previos

Instala estos tres programas antes de continuar:

**PostgreSQL 15+**

1. Descarga desde [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)
2. Sigue el instalador — cuando pida contraseña para `postgres`, elige una que recuerdes
3. Puerto: déjalo en `5432`

**Python — versión requerida: 3.10, 3.11 o 3.12**

Abre **cmd** y verifica tu versión actual:

```cmd
python --version
```

Según el resultado, sigue el camino correspondiente:

=== "✅ Tengo 3.10, 3.11 o 3.12"

    No necesitas hacer nada. Continúa con el siguiente requisito.

=== "⚠️ Tengo 3.13 o superior"

    `onnxruntime` (motor de IA) **no tiene soporte para Python 3.13** todavía. No desinstales tu 3.13 — instala 3.12 **a la par**:

    ```cmd
    winget install Python.Python.3.12
    ```

    Cierra y reabre la terminal. Verifica que el launcher lo detecte:

    ```cmd
    py -3.12 --version
    ```

    Debe mostrar `Python 3.12.x`. Si muestra error, descarga el instalador manualmente desde [python.org](https://www.python.org/downloads/release/python-31210/) y marca **"Add Python to PATH"**.

    El paso 3 usa `py -3.12` para crear el venv con la versión correcta, ignorando tu 3.13.

=== "❌ No tengo Python"

    Instala Python 3.12 directamente:

    ```cmd
    winget install Python.Python.3.12
    ```

    O descarga desde [python.org](https://www.python.org/downloads/) (busca **3.12.x**) — marca **"Add Python to PATH"** durante la instalación.

    Cierra y reabre la terminal antes de continuar.

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

**Crea el entorno virtual según tu versión de Python:**

=== "✅ Tengo 3.10, 3.11 o 3.12"

    ```cmd
    python -m venv venv
    ```

=== "⚠️ Tengo 3.13+ (forzar 3.12)"

    ```cmd
    py -3.12 -m venv venv
    ```

    Esto crea el venv usando Python 3.12 aunque tu versión por defecto sea 3.13. Una vez creado el venv, `python` dentro de él siempre será 3.12.

**Verifica antes de continuar:**

```cmd
venv\Scripts\python.exe --version
```

Debe mostrar `Python 3.12.x`. Si muestra 3.13, elimina la carpeta `venv` y vuelve a crearla con `py -3.12 -m venv venv`.

Luego, activa el entorno e instala las dependencias.

!!! warning "Usa `cmd`, no PowerShell"
    El comando `activate` no funciona en PowerShell si la ejecución de scripts está bloqueada. Abre **cmd** (no PowerShell) y ejecuta:

```cmd
venv\Scripts\activate.bat
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
