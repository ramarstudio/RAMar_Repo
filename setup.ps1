# ############################################################################
# RAMar Software Studio — Setup y Lanzador
# setup.ps1 — Configuracion, verificacion e inicio del sistema
# ############################################################################

param([string]$ScriptDir = $PSScriptRoot)

$Host.UI.RawUI.WindowTitle = "RAMar — Control de Asistencia Biometrico"
$ErrorActionPreference = "Stop"

$AppDir      = Join-Path $ScriptDir "AttendanceSystem"
$FaceDir     = Join-Path $AppDir "src\FaceService"
$AppProject  = Join-Path $AppDir "src\AttendanceSystem.App"
$JsonPath    = Join-Path $AppProject "appsettings.json"
$ExamplePath = Join-Path $AppProject "appsettings.example.json"
$VenvPy      = Join-Path $FaceDir "venv\Scripts\python.exe"
$EnvFile     = Join-Path $FaceDir ".env"
$EnvExample  = Join-Path $FaceDir ".env.example"

# ── Helpers ──────────────────────────────────────────────────────────────────

function Write-Header {
    Write-Host ""
    Write-Host "===========================================================================" -ForegroundColor DarkCyan
    Write-Host "   RAMar Software Studio — Control de Asistencia Biometrico" -ForegroundColor White
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

# ── 2. VERIFICAR PYTHON 3.10–3.12 ────────────────────────────────────────────

Write-Step 2 5 "Verificando Python..."

$pyExe = $null
foreach ($cmd in @("python", "python3", "py")) {
    try {
        $ver = & $cmd --version 2>&1
        if ($ver -match "Python (\d+)\.(\d+)") {
            $pyMajor = [int]$Matches[1]
            $pyMinor = [int]$Matches[2]
            if ($pyMajor -eq 3 -and $pyMinor -in @(10, 11, 12)) {
                $pyExe = $cmd
                $pyFull = "$pyMajor.$pyMinor"
                break
            }
        }
    } catch {}
}

if (-not $pyExe) {
    $verTry = try { & python --version 2>&1 } catch { "no encontrado" }
    Exit-WithError "Python compatible no encontrado. Version detectada: $verTry`n`n         Se requiere Python 3.10, 3.11 o 3.12.`n         Python 3.13+ aun no es compatible con onnxruntime.`n`n         Descarga en: https://www.python.org/downloads/`n         IMPORTANTE: marca 'Add Python to PATH' al instalar."
}

Write-Ok "Python $pyFull compatible."

# ── 3. CONFIGURAR BASE DE DATOS ───────────────────────────────────────────────

Write-Step 3 5 "Configurando base de datos PostgreSQL..."

if (-not (Test-Path $JsonPath)) {

    if (-not (Test-Path $ExamplePath)) {
        Exit-WithError "No se encontro appsettings.example.json en:`n         $ExamplePath"
    }

    Write-Host ""
    Write-Host "  -------------------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "   CONFIGURACION INICIAL — Base de Datos" -ForegroundColor Yellow
    Write-Host "  -------------------------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "   PostgreSQL debe estar instalado y corriendo." -ForegroundColor Gray
    Write-Host "   La base de datos se creara automaticamente si no existe." -ForegroundColor Gray
    Write-Host ""

    # Pedir host (default localhost)
    $dbHost = Read-Host "   Host de PostgreSQL [localhost]"
    if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }

    # Pedir puerto (default 5432)
    $dbPort = Read-Host "   Puerto de PostgreSQL [5432]"
    if ([string]::IsNullOrWhiteSpace($dbPort)) { $dbPort = "5432" }

    # Pedir usuario (default postgres)
    $dbUser = Read-Host "   Usuario de PostgreSQL [postgres]"
    if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "postgres" }

    # Pedir contrasena (con SecureString para no mostrarla en pantalla)
    $dbPassSecure = Read-Host "   Contrasena de PostgreSQL" -AsSecureString
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
        Write-Host "      Intentando crear la base de datos automaticamente..." -ForegroundColor DarkGray
        $env:PGPASSWORD = $dbPass
        $createResult = & $psqlExe -h $dbHost -p $dbPort -U $dbUser -c "SELECT 1 FROM pg_database WHERE datname='AttendanceSystem'" postgres 2>&1
        if ($createResult -match "\(0 rows\)") {
            & $psqlExe -h $dbHost -p $dbPort -U $dbUser -c "CREATE DATABASE ""AttendanceSystem"" ENCODING 'UTF8'" postgres 2>&1 | Out-Null
            Write-Ok "Base de datos 'AttendanceSystem' creada."
        } elseif ($createResult -match "\(1 row\)") {
            Write-Ok "Base de datos 'AttendanceSystem' ya existe."
        } else {
            Write-Warn "No se pudo verificar la base de datos automaticamente. La app la creara al iniciar."
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
