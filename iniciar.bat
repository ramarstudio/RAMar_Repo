@echo off
:: ############################################################################
:: RAMar Software Studio — Sistema de Control de Asistencia Biometrico
:: Instalador y lanzador automatico (Zero-Touch)
::
:: Requisitos previos:
::   - .NET 8 SDK          https://dotnet.microsoft.com/download/dotnet/8.0
::   - Python 3.10, 3.11 o 3.12   https://www.python.org/downloads/
::     (marcar "Add Python to PATH" al instalar)
::   - PostgreSQL corriendo con base de datos "AttendanceSystem" creada
:: ############################################################################
setlocal enabledelayedexpansion
chcp 65001 >nul
title RAMar — Control de Asistencia Biometrico

echo.
echo ===========================================================================
echo    RAMar Software Studio — Control de Asistencia Biometrico
echo ===========================================================================
echo.

:: ── PERMISOS ────────────────────────────────────────────────────────────────
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [AVISO] Sin permisos de Administrador. Si algo falla, ejecuta como Admin.
    echo.
)

:: ── 1. VERIFICAR .NET 8 ─────────────────────────────────────────────────────
echo [1/5] Verificando .NET 8...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] .NET 8 SDK no encontrado.
    echo         Descargalo en: https://dotnet.microsoft.com/download/dotnet/8.0
    echo         Instala y vuelve a ejecutar este archivo.
    pause & exit /b 1
)
for /f "tokens=*" %%v in ('dotnet --version 2^>nul') do set DOTNET_VER=%%v
echo       OK — .NET %DOTNET_VER% detectado.

:: ── 2. VERIFICAR PYTHON 3.10–3.12 ───────────────────────────────────────────
echo [2/5] Verificando Python...
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Python no encontrado en PATH.
    echo         Instala Python 3.10, 3.11 o 3.12 desde https://www.python.org/downloads/
    echo         IMPORTANTE: marca "Add Python to PATH" durante la instalacion.
    pause & exit /b 1
)

:: Obtener version mayor.menor
for /f "tokens=2" %%v in ('python --version 2^>^&1') do set PY_FULL=%%v
for /f "tokens=1,2 delims=." %%a in ("%PY_FULL%") do (
    set PY_MAJOR=%%a
    set PY_MINOR=%%b
)

if "%PY_MAJOR%" neq "3" (
    echo [ERROR] Se requiere Python 3. Version detectada: %PY_FULL%
    pause & exit /b 1
)

:: Verificar que sea 3.10, 3.11 o 3.12
set PY_OK=0
if "%PY_MINOR%"=="10" set PY_OK=1
if "%PY_MINOR%"=="11" set PY_OK=1
if "%PY_MINOR%"=="12" set PY_OK=1

if "%PY_OK%"=="0" (
    echo.
    echo [ERROR] Python %PY_FULL% no es compatible.
    echo         Este sistema requiere Python 3.10, 3.11 o 3.12.
    echo         Python 3.13+ aun no tiene soporte completo para onnxruntime.
    echo         Descarga una version compatible: https://www.python.org/downloads/
    pause & exit /b 1
)
echo       OK — Python %PY_FULL% compatible.

:: ── 3. CONFIGURAR appsettings.json ──────────────────────────────────────────
echo [3/5] Verificando configuracion de base de datos...
set "APP_DIR=%~dp0AttendanceSystem"
set "JSON_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.json"
set "EXAMPLE_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.example.json"

if not exist "%JSON_PATH%" (
    if not exist "%EXAMPLE_PATH%" (
        echo [ERROR] No se encontro appsettings.example.json en:
        echo         %EXAMPLE_PATH%
        pause & exit /b 1
    )
    echo.
    echo -------------------------------------------------------------------------
    echo   PRIMERA CONFIGURACION — Base de Datos PostgreSQL
    echo -------------------------------------------------------------------------
    echo   Necesitas una base de datos PostgreSQL con el nombre: AttendanceSystem
    echo   Si no la tienes, crea en pgAdmin: CREATE DATABASE "AttendanceSystem";
    echo.
    set /p "DB_PASS=   Ingresa la contrasena de PostgreSQL (usuario postgres): "
    if "!DB_PASS!"=="" (
        echo [ERROR] La contrasena no puede estar vacia.
        pause & exit /b 1
    )
    copy "%EXAMPLE_PATH%" "%JSON_PATH%" >nul
    powershell -NoProfile -Command "(Get-Content '%JSON_PATH%') -replace 'CAMBIAR_POR_TU_CONTRASEÑA', '!DB_PASS!' | Set-Content -Encoding UTF8 '%JSON_PATH%'"
    if errorlevel 1 (
        echo [ERROR] No se pudo escribir la configuracion.
        pause & exit /b 1
    )
    echo       OK — appsettings.json creado correctamente.
) else (
    echo       OK — appsettings.json ya existe.
)

:: ── 4. PREPARAR MOTOR DE IA (Python venv) ───────────────────────────────────
echo [4/5] Preparando motor de reconocimiento facial (IA)...
set "FACE_DIR=%APP_DIR%\src\FaceService"
set "VENV_DIR=%FACE_DIR%\venv"
set "VENV_PY=%VENV_DIR%\Scripts\python.exe"
set "ENV_FILE=%FACE_DIR%\.env"

:: Crear .env si no existe
if not exist "%ENV_FILE%" (
    echo       Creando archivo de configuracion del motor IA...
    copy "%FACE_DIR%\.env.example" "%ENV_FILE%" >nul 2>&1
    if errorlevel 1 (
        echo FACE_HOST=0.0.0.0> "%ENV_FILE%"
        echo FACE_PORT=5001>> "%ENV_FILE%"
        echo FACE_DETECTION_MODEL=buffalo_l>> "%ENV_FILE%"
        echo FACE_GPU_ID=-1>> "%ENV_FILE%"
        echo FACE_SIMILARITY_THRESHOLD=0.60>> "%ENV_FILE%"
        echo FACE_MAX_FACES_PER_IMAGE=1>> "%ENV_FILE%"
        echo FACE_API_KEY=>> "%ENV_FILE%"
        echo FACE_LOG_LEVEL=INFO>> "%ENV_FILE%"
    )
    echo       OK — .env creado.
)

:: Crear venv si no existe o esta corrupto
if not exist "%VENV_PY%" (
    echo       Creando entorno virtual Python aislado...
    cd /d "%FACE_DIR%"
    python -m venv venv
    if errorlevel 1 (
        echo [ERROR] No se pudo crear el entorno virtual.
        pause & exit /b 1
    )
    echo       OK — Entorno virtual creado.
    echo       Instalando librerias (esto puede tardar unos minutos la primera vez)...
    call "%VENV_DIR%\Scripts\activate.bat"
    python install.py
    if errorlevel 1 (
        echo [ERROR] Fallo la instalacion de librerias.
        echo         Verifica tu conexion a internet e intenta de nuevo.
        pause & exit /b 1
    )
    echo       OK — Librerias instaladas correctamente.
) else (
    :: Verificar que insightface este instalado
    "%VENV_PY%" -c "import insightface" >nul 2>&1
    if errorlevel 1 (
        echo       Reinstalando librerias faltantes...
        call "%VENV_DIR%\Scripts\activate.bat"
        python install.py
    ) else (
        echo       OK — Motor de IA ya esta instalado.
    )
)

:: ── 5. LANZAR APLICACION ────────────────────────────────────────────────────
echo [5/5] Iniciando aplicacion...
echo.
echo ===========================================================================
echo    Sistema listo. Iniciando RAMar Control de Asistencia...
echo    (Usuario inicial: admin / Contrasena: admin123)
echo ===========================================================================
echo.
cd /d "%APP_DIR%"
dotnet run --project src\AttendanceSystem.App

if errorlevel 1 (
    echo.
    echo [ERROR] La aplicacion no pudo iniciar.
    echo         Posibles causas:
    echo           1. PostgreSQL no esta corriendo
    echo           2. La contrasena de la base de datos es incorrecta
    echo           3. La base de datos "AttendanceSystem" no existe
    echo.
    echo         Para reconfigurar, elimina el archivo:
    echo         %JSON_PATH%
    echo         y vuelve a ejecutar iniciar.bat
)

pause
exit /b 0
