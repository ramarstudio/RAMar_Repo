# Guía de instalación

---

## Requisitos previos

Instala estos tres programas antes de continuar:

**PostgreSQL 15+**

1. Descarga desde [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)
2. Sigue el instalador — cuando pida contraseña para `postgres`, elige una que recuerdes
3. Puerto: déjalo en `5432`

**Python 3.10, 3.11 o 3.12**

1. Descarga desde [python.org/downloads](https://www.python.org/downloads/) — versión **3.12.x**
2. **Importante:** marca la casilla **"Add Python to PATH"** en la primera pantalla
3. Haz clic en "Install Now"

!!! danger "Python 3.13 no es compatible"
    `onnxruntime` (motor de IA) no tiene soporte para Python 3.13 aún. Instala únicamente 3.10, 3.11 o 3.12.

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
    No necesitas crear la base de datos manualmente. La aplicación la crea automáticamente al primer arranque usando Entity Framework.

### 3. Instalar librerías de IA

```bash
cd ../FaceService

python -m venv venv
venv\Scripts\activate

python install.py
```

La primera vez tarda entre **2 y 5 minutos** — descarga el modelo de reconocimiento facial (~600 MB).

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
| "Python no compatible" | Tienes Python 3.13 | Instala Python 3.12 desde python.org |
| "Python no encontrado" | No se marcó "Add to PATH" | Reinstala Python marcando esa casilla |
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
