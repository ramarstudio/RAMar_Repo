# ============================================================================
#  build_installer.ps1 — Script de Build del Instalador
#
#  Automatiza todo el proceso:
#    1. Publica la aplicación .NET como self-contained
#    2. Compila el script de Inno Setup para generar el .exe instalador
#
#  Uso:
#    .\build_installer.ps1
#    .\build_installer.ps1 -SkipPublish    # Si ya publicaste manualmente
#    .\build_installer.ps1 -InnoSetupPath "C:\ruta\a\ISCC.exe"
#
#  Requisitos:
#    - .NET 8 SDK
#    - Inno Setup 6+ (https://jrsoftware.org/isinfo.php)
# ============================================================================

param(
    [switch]$SkipPublish,
    [string]$InnoSetupPath = "",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
$SolutionDir = Split-Path $ScriptDir -Parent
$PublishDir = Join-Path $SolutionDir "publish\app"
$IssFile = Join-Path $ScriptDir "AttendanceSystem_Installer.iss"
$OutputDir = Join-Path $ScriptDir "output"

# ── Funciones auxiliares ──────────────────────────────────────────────────────

function Write-Header($text) {
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor Cyan
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step($step, $text) {
    Write-Host "  [$step] $text" -ForegroundColor Yellow
}

function Write-Success($text) {
    Write-Host "  ✓ $text" -ForegroundColor Green
}

function Write-Fail($text) {
    Write-Host "  ✗ $text" -ForegroundColor Red
}

# ── Buscar Inno Setup Compiler ────────────────────────────────────────────────

function Find-InnoSetupCompiler {
    # Si se proporcionó una ruta explícita
    if ($InnoSetupPath -and (Test-Path $InnoSetupPath)) {
        return $InnoSetupPath
    }

    # Buscar en rutas comunes
    $searchPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "${env:LocalAppData}\Programs\Inno Setup 6\ISCC.exe"
    )

    foreach ($path in $searchPaths) {
        if (Test-Path $path) {
            return $path
        }
    }

    # Buscar en PATH
    $iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($iscc) {
        return $iscc.Source
    }

    return $null
}

# ══════════════════════════════════════════════════════════════════════════════
#  MAIN
# ══════════════════════════════════════════════════════════════════════════════

Write-Header "RAMar — Build del Instalador"

# ── Paso 1: Publicar la aplicación .NET ──────────────────────────────────────

if (-not $SkipPublish) {
    Write-Step "1/3" "Publicando aplicación .NET ($Configuration, win-x64, self-contained)..."

    $publishArgs = @(
        "publish",
        (Join-Path $SolutionDir "src\AttendanceSystem.App"),
        "-c", $Configuration,
        "-r", "win-x64",
        "--self-contained", "true",
        "-o", $PublishDir
    )

    & dotnet @publishArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Error durante dotnet publish. Código: $LASTEXITCODE"
        exit 1
    }

    Write-Success "Publicación completada en: $PublishDir"
} else {
    Write-Step "1/3" "Publicación omitida (flag -SkipPublish)"
    
    if (-not (Test-Path (Join-Path $PublishDir "AttendanceSystem.App.exe"))) {
        Write-Fail "No se encontró la publicación en: $PublishDir"
        Write-Fail "Ejecuta sin -SkipPublish o publica manualmente primero."
        exit 1
    }
    Write-Success "Usando publicación existente en: $PublishDir"
}

# ── Paso 2: Verificar archivos del FaceService ──────────────────────────────

Write-Step "2/3" "Verificando archivos del FaceService..."

$faceServiceDir = Join-Path $SolutionDir "src\FaceService"
$requiredFiles = @("install.py", "run.py", "requirements.txt", "setup_faceservice.bat")

foreach ($file in $requiredFiles) {
    $filePath = Join-Path $faceServiceDir $file
    if (-not (Test-Path $filePath)) {
        Write-Fail "Archivo faltante: $filePath"
        exit 1
    }
}

Write-Success "Todos los archivos del FaceService verificados."

# ── Paso 3: Compilar instalador con Inno Setup ──────────────────────────────

Write-Step "3/3" "Buscando Inno Setup Compiler..."

$isccPath = Find-InnoSetupCompiler

if (-not $isccPath) {
    Write-Host ""
    Write-Fail "Inno Setup Compiler (ISCC.exe) no encontrado."
    Write-Host ""
    Write-Host "  Opciones:" -ForegroundColor White
    Write-Host "    1. Descarga Inno Setup desde: https://jrsoftware.org/isinfo.php" -ForegroundColor Gray
    Write-Host "    2. O especifica la ruta:" -ForegroundColor Gray
    Write-Host "       .\build_installer.ps1 -InnoSetupPath 'C:\ruta\a\ISCC.exe'" -ForegroundColor Gray
    Write-Host "    3. O abre el archivo .iss directamente con Inno Setup Compiler:" -ForegroundColor Gray
    Write-Host "       $IssFile" -ForegroundColor Gray
    Write-Host ""
    
    # Aún así, la publicación fue exitosa
    Write-Success "La publicación de la app se completó correctamente."
    Write-Host "  Puedes compilar el instalador manualmente abriendo:" -ForegroundColor Yellow
    Write-Host "  $IssFile" -ForegroundColor White
    exit 0
}

Write-Success "Inno Setup encontrado: $isccPath"
Write-Host "  Compilando instalador..."

# Crear directorio de salida
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

& $isccPath $IssFile

if ($LASTEXITCODE -ne 0) {
    Write-Fail "Error durante la compilación de Inno Setup. Código: $LASTEXITCODE"
    exit 1
}

# ── Resultado ────────────────────────────────────────────────────────────────

Write-Header "Build Completado Exitosamente"

$setupExe = Get-ChildItem $OutputDir -Filter "Setup_RAMar_*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($setupExe) {
    $sizeMB = [math]::Round($setupExe.Length / 1MB, 1)
    Write-Host "  Instalador generado:" -ForegroundColor White
    Write-Host "    $($setupExe.FullName)" -ForegroundColor Green
    Write-Host "    Tamaño: $sizeMB MB" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Para probar:" -ForegroundColor White
    Write-Host "    Start-Process '$($setupExe.FullName)'" -ForegroundColor Gray
} else {
    Write-Host "  Revisa la carpeta de salida: $OutputDir" -ForegroundColor Yellow
}

Write-Host ""
