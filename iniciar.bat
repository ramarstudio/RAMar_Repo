@echo off
chcp 65001 >nul
title RAMar - Control de Asistencia Biometrico

echo =======================================================
echo    RAMar Software Studio - Attendance System
echo    Inicializador Automatico Global
echo =======================================================
echo.

cd /d "%~dp0\AttendanceSystem"

:: 1. Verificación de AppSettings
echo [1/3] Verificando configuracion de Base de Datos...
if not exist "src\AttendanceSystem.App\appsettings.json" (
    echo.
    echo *******************************************************
    echo                 PRIMERA EJECUCION
    echo *******************************************************
    echo.
    echo Se acaba de crear tu archivo de configuracion local...
    copy "src\AttendanceSystem.App\appsettings.example.json" "src\AttendanceSystem.App\appsettings.json" >nul
    echo.
    echo PASO REQUERIDO:
    echo 1. Ve a la carpeta: AttendanceSystem\src\AttendanceSystem.App
    echo 2. Abre el archivo "appsettings.json"
    echo 3. Cambia "CAMBIAR_POR_TU_CONTRASEÑA" por la contrasena de tu PostgreSQL.
    echo 4. Guarda el archivo y vuelve a esta ventana.
    echo.
    echo Presiona cualquier tecla UNA VEZ que hayas guardado el archivo...
    pause >nul
) else (
    echo   - OK (appsettings.json encontrado)
)

:: 2. Verificación e Instalación de Python
echo.
echo [2/3] Preparando el Motor Biometrico (Python)...
cd src\FaceService

:: Verificar si existe Python en la línea de comandos
python --version >nul 2>&1
if errorlevel 1 (
    echo.
    echo [ERROR CRITICO] Python no esta instalado o no se agrego al PATH.
    echo Por favor, instala Python 3.10+ y asegurate de marcar "Add Python to PATH".
    pause
    exit /b
)

if not exist "venv\Scripts\python.exe" (
    echo   - Creando entorno virtual de Python por primera vez...
    python -m venv venv
)

echo   - Instalando/Verificando dependencias (esto puede tardar si es la primera vez)...
call venv\Scripts\activate.bat
python install.py
if errorlevel 1 (
    echo.
    echo [ERROR] Ocurrio un problema instalando las dependencias.
    pause
    exit /b
)
cd ..\..

:: 3. Ejecución de .NET
echo.
echo [3/3] Compilando e Iniciando la Aplicacion Principal...
echo.
dotnet run --project src\AttendanceSystem.App

if errorlevel 1 (
    echo.
    echo [ERROR] La aplicacion encontro un problema al ejecutarse (revisa errores arriba).
    pause
    exit /b
)
