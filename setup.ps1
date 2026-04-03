# ############################################################################
# RAMar Software Studio - Setup y Lanzador
# setup.ps1 - Configuracion, verificacion e inicio del sistema
# ############################################################################

$Host.UI.RawUI.WindowTitle = "RAMar - Control de Asistencia Biometrico"
$ErrorActionPreference = "Stop"

$ScriptDir   = $PSScriptRoot
$AppDir      = Join-Path $ScriptDir "AttendanceSystem"
$FaceDir     = Join-Path $AppDir "src" | Join-Path -ChildPath "FaceService"
$AppProject  = Join-Path $AppDir "src" | Join-Path -ChildPath "AttendanceSystem.App"
$JsonPath    = Join-Path $AppProject "appsettings.json"
$ExamplePath = Join-Path $AppProject "appsettings.example.json"
$VenvPy      = Join-Path $FaceDir "venv\Scripts\python.exe"
$EnvFile     = Join-Path $FaceDir ".env"
$EnvExample  = Join-Path $FaceDir ".env.example"

# ── Helpers ──────────────────────────────────────────────────────────────────

function Write-Header {
    Write-Host ""
    Write-Host "===========================================================================" -ForegroundColor DarkCyan
    Write-Host "   RAMar Software Studio - Control de Asistencia Biometrico" -ForegroundColor White
    Write-Host "===========================================================================" -ForegroundColor DarkCyan
    Write-Host ""
}

function Write-Step([int]$n, [int]$total, [string]$msg) {
    Write-Host "[$n/$total] $msg" -ForegroundColor Cyan
}

function Write-Ok([string]$msg) {
    Write-Host "      OK - $msg" -ForegroundColor Green
}

function Write-Warn([string]$msg) {
    Write-Host "      AVISO: $msg" -ForegroundColor Yellow
}

function Write-Fail([string]$msg) {
    Write-Host ""
    Write-Host "[ERROR] $msg" -ForegroundColor Red
    Write-Host ""
}

function Exit-WithError([string]$msg) {
    Write-Fail $msg
    Write-Host "Presiona Enter para cerrar..." -ForegroundColor DarkGray
    Read-Host | Out-Null
    exit 1
}

# ── INICIO ───────────────────────────────────────────────────────────────────

Write-Header

# ── 1. VERIFICAR .NET 8 ──────────────────────────────────────────────────────

Write-Step 1 5 "Verificando .NET 8..."

try {
    $dotnetVer = (dotnet --version 2>$null)
    if (-not $dotnetVer) { throw }
    $major = [int]($dotnetVer.Split(".")[0])
    if ($major -lt 8) {
        Exit-WithError ".NET $dotnetVer detectado pero se requiere .NET 8+.`n         Descargalo en: https://dotnet.microsoft.com/download/dotnet/8.0"
    }
    Write-Ok ".NET $dotnetVer detectado."
} catch {
    Exit-WithError ".NET 8 SDK no encontrado.`n         Descargalo en: https://dotnet.microsoft.com/download/dotnet/8.0`n         Marca 'Add to PATH' al instalar."
}

# ── 2. VERIFICAR / INSTALAR PYTHON 3.12 ──────────────────────────────────────

Write-Step 2 5 "Verificando Python..."

# Version objetivo
$PY_TARGET_FULL  = "3.12.10"
$PY_INSTALLER_URL = "https://www.python.org/ftp/python/$PY_TARGET_FULL/python-$PY_TARGET_FULL-amd64.exe"
$PY_INSTALLER_PATH = Join-Path $env:TEMP "python-$PY_TARGET_FULL-amd64.exe"

function Find-CompatiblePython {
    foreach ($cmd in @("python", "python3", "py -3.12", "py -3.11", "py -3.10")) {
        try {
            $ver = & $cmd.Split()[0] $cmd.Split()[1..99] --version 2>&1
            if ($ver -match "Python (\d+)\.(\d+)") {
                $maj = [int]$Matches[1]; $min = [int]$Matches[2]
                if ($maj -eq 3 -and $min -in @(10, 11, 12)) {
                    return @{ Exe = $cmd.Split()[0]; Full = "$maj.$min" }
                }
            }
        } catch {}
    }
    return $null
}

function Install-Python312 {
    Write-Host ""
    Write-Host "      Python compatible no encontrado. Instalando Python $PY_TARGET_FULL automaticamente..." -ForegroundColor Yellow
    Write-Host "      Descargando instalador (~27 MB)..." -ForegroundColor DarkGray

    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $wc = New-Object System.Net.WebClient
        $wc.DownloadFile($PY_INSTALLER_URL, $PY_INSTALLER_PATH)
    } catch {
        Exit-WithError "No se pudo descargar Python. Verifica tu conexion a internet.`n         URL: $PY_INSTALLER_URL`n         O instalalo manualmente desde: https://www.python.org/downloads/"
    }

    Write-Host "      Instalando Python $PY_TARGET_FULL (puede tardar 1-2 minutos)..." -ForegroundColor DarkGray

    # Instalacion silenciosa: solo para el usuario actual, agrega al PATH
    $pyInstallArgs = "/quiet InstallAllUsers=0 PrependPath=1 Include_test=0 Include_doc=0 Include_launcher=1"
    $proc = Start-Process -FilePath $PY_INSTALLER_PATH -ArgumentList $pyInstallArgs -Wait -PassThru

    Remove-Item $PY_INSTALLER_PATH -ErrorAction SilentlyContinue

    if ($proc.ExitCode -ne 0) {
        Exit-WithError "La instalacion de Python fallo (codigo $($proc.ExitCode)).`n         Instalalo manualmente desde: https://www.python.org/downloads/`n         Marca 'Add Python to PATH' al instalar."
    }

    # Refrescar PATH en el proceso actual para encontrar python recien instalado
    $userPath  = [Environment]::GetEnvironmentVariable("PATH", "User")
    $machinePath = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    $env:PATH  = "$userPath;$machinePath"

    Write-Ok "Python $PY_TARGET_FULL instalado correctamente."
}

# Buscar Python compatible ya instalado
$pyFound = Find-CompatiblePython

if (-not $pyFound) {
    Install-Python312
    # Buscar de nuevo despues de instalar
    $pyFound = Find-CompatiblePython
    if (-not $pyFound) {
        Exit-WithError "Python se instalo pero no se encontro en PATH.`n         Cierra esta ventana, abre una nueva y vuelve a ejecutar iniciar.bat."
    }
}

$pyExe  = $pyFound.Exe
$pyFull = $pyFound.Full
Write-Ok "Python $pyFull listo."

# ── 3. CONFIGURAR BASE DE DATOS ───────────────────────────────────────────────

Write-Step 3 5 "Configurando base de datos PostgreSQL..."

if (-not (Test-Path $JsonPath)) {

    if (-not (Test-Path $ExamplePath)) {
        Exit-WithError "No se encontro appsettings.example.json en:`n         $ExamplePath"
    }

    Write-Host ""
    Write-Host "  -------------------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "   CONFIGURACION INICIAL - Base de Datos" -ForegroundColor Yellow
    Write-Host "  -------------------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "   Necesitamos conectar la aplicacion con PostgreSQL." -ForegroundColor Gray
    Write-Host "   Presiona Enter en cada pregunta para usar el valor por defecto." -ForegroundColor Gray
    Write-Host ""

    # Pedir host (default localhost)
    $dbHost = Read-Host "   Servidor (Enter = localhost)"
    if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }

    # Pedir puerto (default 5432)
    $dbPort = Read-Host "   Puerto   (Enter = 5432)"
    if ([string]::IsNullOrWhiteSpace($dbPort)) { $dbPort = "5432" }

    # Pedir usuario (default postgres)
    $dbUser = Read-Host "   Usuario  (Enter = postgres)"
    if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "postgres" }

    # Pedir contrasena (con SecureString para no mostrarla en pantalla)
    $dbPassSecure = Read-Host "   Contrasena de PostgreSQL (la que pusiste al instalar)" -AsSecureString
    $dbPass = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassSecure)
    )

    if ([string]::IsNullOrEmpty($dbPass)) {
        Exit-WithError "La contrasena no puede estar vacia."
    }

    # Intentar crear la base de datos automaticamente via psql
    $psqlExe = $null
    $psqlPaths = @(
        "psql",
        "C:\Program Files\PostgreSQL\16\bin\psql.exe",
        "C:\Program Files\PostgreSQL\15\bin\psql.exe",
        "C:\Program Files\PostgreSQL\14\bin\psql.exe",
        "C:\Program Files\PostgreSQL\17\bin\psql.exe"
    )
    foreach ($p in $psqlPaths) {
        try {
            & $p --version >$null 2>&1
            if ($LASTEXITCODE -eq 0) { $psqlExe = $p; break }
        } catch {}
    }

    if ($psqlExe) {
        Write-Host ""
        Write-Host "      Verificando base de datos..." -ForegroundColor DarkGray
        $env:PGPASSWORD = $dbPass
        $checkResult = & $psqlExe -h $dbHost -p $dbPort -U $dbUser -tAc "SELECT 1 FROM pg_database WHERE datname='AttendanceSystem'" postgres 2>&1
        $dbExists = ($checkResult -join "") -match "1"
        if (-not $dbExists) {
            & $psqlExe -h $dbHost -p $dbPort -U $dbUser postgres -c "CREATE DATABASE AttendanceSystem ENCODING 'UTF8'" 2>&1 | Out-Null
            Write-Ok "Base de datos creada."
        } else {
            Write-Ok "Base de datos ya existe."
        }
        Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
    } else {
        Write-Warn "psql no encontrado. La base de datos se creara automaticamente al iniciar la app."
    }

    # Generar appsettings.json con los datos ingresados
    $json = Get-Content $ExamplePath -Raw -Encoding UTF8
    $connStr = "Host=$dbHost;Port=$dbPort;Database=AttendanceSystem;Username=$dbUser;Password=$dbPass"
    $json = $json -replace "Host=localhost;Port=5432;Database=AttendanceSystem;Username=postgres;Password=CAMBIAR_POR_TU_CONTRASEÑA", $connStr
    Set-Content -Path $JsonPath -Value $json -Encoding UTF8 -NoNewline

    Write-Ok "appsettings.json creado correctamente."
    Write-Host ""

} else {
    Write-Ok "appsettings.json ya existe."
}

# ── 4. PREPARAR MOTOR DE IA ───────────────────────────────────────────────────

Write-Step 4 5 "Preparando motor de reconocimiento facial (IA)..."

# Crear .env si no existe
if (-not (Test-Path $EnvFile)) {
    if (Test-Path $EnvExample) {
        Copy-Item $EnvExample $EnvFile
    } else {
        @"
FACE_HOST=0.0.0.0
FACE_PORT=5001
FACE_DETECTION_MODEL=buffalo_l
FACE_GPU_ID=-1
FACE_SIMILARITY_THRESHOLD=0.60
FACE_MAX_FACES_PER_IMAGE=1
FACE_API_KEY=
FACE_LOG_LEVEL=INFO
"@ | Set-Content $EnvFile -Encoding UTF8
    }
    Write-Ok ".env creado."
}

# Crear venv si no existe
if (-not (Test-Path $VenvPy)) {
    Write-Host "      Creando entorno virtual Python aislado..." -ForegroundColor DarkGray
    Push-Location $FaceDir
    & $pyExe -m venv venv
    if ($LASTEXITCODE -ne 0) { Exit-WithError "No se pudo crear el entorno virtual Python." }

    Write-Host "      Instalando librerias IA (puede tardar 2-5 min la primera vez)..." -ForegroundColor DarkGray
    & "venv\Scripts\python.exe" install.py
    if ($LASTEXITCODE -ne 0) { Exit-WithError "Fallo la instalacion de librerias. Verifica tu conexion a internet." }
    Pop-Location
    Write-Ok "Motor de IA instalado correctamente."
} else {
    # Verificar que insightface este instalado
    $check = & $VenvPy -c "import insightface; print('ok')" 2>&1
    if ($check -ne "ok") {
        Write-Host "      Reparando librerias faltantes..." -ForegroundColor Yellow
        Push-Location $FaceDir
        & "venv\Scripts\python.exe" install.py
        Pop-Location
    } else {
        Write-Ok "Motor de IA listo."
    }
}

# ── 5. LANZAR APLICACION ─────────────────────────────────────────────────────

Write-Step 5 5 "Iniciando aplicacion..."
Write-Host ""
Write-Host "===========================================================================" -ForegroundColor DarkCyan
Write-Host "   Sistema listo. Iniciando RAMar Control de Asistencia..." -ForegroundColor White
Write-Host "   Usuario inicial: admin   /   Contrasena: admin123" -ForegroundColor DarkGray
Write-Host "===========================================================================" -ForegroundColor DarkCyan
Write-Host ""

Push-Location $AppDir
dotnet run --project src\AttendanceSystem.App
$exitCode = $LASTEXITCODE
Pop-Location

if ($exitCode -ne 0) {
    Write-Host ""
    Write-Fail "La aplicacion termino con error."
    Write-Host "  Posibles causas:" -ForegroundColor Yellow
    Write-Host "    1. PostgreSQL no esta corriendo" -ForegroundColor Gray
    Write-Host "    2. La contrasena de la base de datos es incorrecta" -ForegroundColor Gray
    Write-Host "    3. Permisos insuficientes" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Para reconfigurar la base de datos, elimina este archivo y vuelve a ejecutar:" -ForegroundColor Gray
    Write-Host "  $JsonPath" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Presiona Enter para cerrar..." -ForegroundColor DarkGray
    Read-Host | Out-Null
}
