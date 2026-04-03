# Guía de instalación

---

## Guía usuario

Para administradores y usuarios finales sin conocimientos técnicos.

### Paso 1 — Instalar los tres programas base

Instala en este orden:

**PostgreSQL 15+**

1. Descarga desde [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)
2. Ejecuta el instalador y sigue los pasos
3. Cuando te pida contraseña para el usuario `postgres`, elige una que recuerdes bien
4. Puerto: déjalo en `5432`
5. Finaliza la instalación

**Python 3.12**

1. Descarga desde [python.org/downloads](https://www.python.org/downloads/) — busca la versión **3.12.x**
2. Ejecuta el instalador
3. **Importante:** en la primera pantalla, marca la casilla **"Add Python to PATH"**
4. Haz clic en "Install Now"

**.NET 8 SDK**

1. Descarga desde [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Descarga el **SDK x64**
3. Ejecuta el instalador y finaliza

---

### Paso 2 — Descargar el sistema

1. Ve a [github.com/ramarstudio/RAMar_Repo](https://github.com/ramarstudio/RAMar_Repo)
2. Haz clic en el botón verde **Code** → **Download ZIP**
3. Extrae el ZIP en una carpeta de tu elección, por ejemplo `C:\RAMar`

---

### Paso 3 — Ejecutar iniciar.bat

1. Entra a la carpeta donde extrajiste el sistema
2. Busca el archivo **`iniciar.bat`**
3. Haz **doble clic**

Se abrirá una ventana negra. El asistente verificará los programas instalados y te pedirá la contraseña de PostgreSQL que configuraste en el Paso 1.

!!! info "Lo que hace el asistente automáticamente"
    - Crea la base de datos `AttendanceSystem` si no existe
    - Crea el archivo de configuración con tu contraseña
    - Instala las librerías de inteligencia artificial (solo la primera vez, tarda 2–5 min)
    - Inicia la aplicación

---

### Paso 4 — Primer inicio de sesión

Cuando la aplicación abra, ingresa con:

- **Usuario:** `admin`
- **Contraseña:** `admin123`

!!! tip "Primeros pasos recomendados"
    1. Ve a **Usuarios** → crea los empleados del sistema
    2. Ve a **Registro Facial** → otorga consentimiento y captura el rostro de cada empleado
    3. Ve a **Horarios** → configura los horarios de entrada y salida
    4. Ve a **Configuración** → ajusta la tolerancia de tardanzas y otros parámetros

---

### Solución de problemas comunes

| Problema | Causa probable | Solución |
|---|---|---|
| "Python no compatible" | Tienes Python 3.13+ | Instala Python 3.12 desde python.org |
| "Python no encontrado" | No se marcó "Add to PATH" | Reinstala Python marcando esa casilla |
| "No se pudo conectar a la base de datos" | PostgreSQL no está corriendo | Abre Servicios de Windows y verifica que `postgresql` esté iniciado |
| "Error de biometría" | Falta Visual C++ | Descarga e instala [vc_redist.x64.exe](https://aka.ms/vs/17/release/vc_redist.x64.exe) |
| "Cámara no abre" | Otra app la está usando | Cierra Zoom, Teams u otra app que use la cámara |
| "Error de reconocimiento facial" | Poca iluminación | Asegúrate de tener luz frontal al capturar el rostro |

!!! note "Para reconfigurar la base de datos"
    Si cambiaste la contraseña de PostgreSQL o necesitas reconectar, elimina el archivo:
    ```
    AttendanceSystem\src\AttendanceSystem.App\appsettings.json
    ```
    y vuelve a ejecutar `iniciar.bat`.

---

## Guía técnica

Para desarrolladores y personal de TI.

### Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

### Configurar appsettings.json manualmente

Si prefieres configurar sin el asistente:

```bash
cd AttendanceSystem/src/AttendanceSystem.App
copy appsettings.example.json appsettings.json
```

Edita `appsettings.json` y reemplaza la cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AttendanceSystem;Username=postgres;Password=TU_CONTRASEÑA"
  }
}
```

### Crear la base de datos

La aplicación usa `EnsureCreated()` de Entity Framework Core. La base de datos y todas las tablas se crean automáticamente al primer arranque. Solo necesitas que exista PostgreSQL corriendo.

Si prefieres crearla manualmente:

```sql
CREATE DATABASE "AttendanceSystem" ENCODING 'UTF8';
```

### Instalar dependencias Python

```bash
cd AttendanceSystem/src/FaceService

# Crear entorno virtual
python -m venv venv
venv\Scripts\activate   # Windows
# source venv/bin/activate  # Linux/Mac

# Instalar dependencias
# Windows (usa wheels comunitarios para insightface):
python install.py

# Linux/Mac:
pip install -r requirements.txt
```

!!! warning "Versiones compatibles"
    | Librería | Versión |
    |---|---|
    | Python | 3.10, 3.11, 3.12 |
    | onnxruntime | >=1.18.0, <1.21.0 |
    | numpy | >=1.24.0, <2.0.0 |
    | insightface | ==0.7.3 |

    Estas restricciones están documentadas en `requirements.txt` y son forzadas por `install.py`.

### Compilar y ejecutar la aplicación

```bash
cd AttendanceSystem
dotnet build
dotnet run --project src/AttendanceSystem.App
```

### Variables de entorno del FaceService

El servicio Python se configura mediante `.env` en `AttendanceSystem/src/FaceService/`:

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

Copia `.env.example` como `.env` y ajusta según el entorno. El `iniciar.bat` lo crea automáticamente.

### Ciclo de vida del servicio Python

El `FaceServiceManager` en la aplicación WPF gestiona el proceso Python automáticamente:

- **Inicio bajo demanda**: se lanza cuando se necesita verificación o registro biométrico
- **Health check**: verifica `GET /api/health` antes de cada operación
- **Apagado por inactividad**: se detiene tras N minutos sin uso (configurable en `appsettings.json`)
- **Reinicio automático**: si el proceso muere inesperadamente, se relanza en la siguiente operación

### Estructura del repositorio

```
RAMar_Repo/
├── iniciar.bat                    # Lanzador principal (doble clic)
├── setup.ps1                      # Lógica de instalación (PowerShell)
├── AttendanceSystem/
│   ├── AttendanceSystem.sln
│   └── src/
│       ├── AttendanceSystem.App/         # WPF + controladores + vistas
│       │   └── appsettings.example.json  # Plantilla de configuración
│       ├── AttendanceSystem.Core/        # DTOs, interfaces, enums
│       ├── AttendanceSystem.Services/    # Lógica de negocio
│       ├── AttendanceSystem.Infrastructure/  # EF Core, repositorios
│       ├── AttendanceSystem.Security/    # AES-256, sesiones
│       └── FaceService/                  # Microservicio Python
│           ├── install.py                # Instalador de librerías
│           ├── requirements.txt          # Dependencias Python
│           ├── .env.example              # Plantilla de variables
│           └── run.py                    # Punto de entrada
├── docs/                          # Fuente de esta documentación
└── mkdocs.yml                     # Configuración del portal
```
