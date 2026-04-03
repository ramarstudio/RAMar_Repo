@echo off
:: ############################################################################
:: # RAMar Software Studio - Attendance System
:: # Script de Configuración y Arranque Automático (Zero-Touch)
:: ############################################################################
setlocal enabledelayedexpansion
chcp 65001 >nul
title RAMar - Control de Asistencia Biométrico (Instalador)

echo.
echo ===========================================================================
echo    RAMar Software Studio - Attendance System
echo    Configurador Automático de Proyecto
echo ===========================================================================
echo.

:: 0. Verificación de Permisos (Opcional pero recomendado)
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [INFO] Ejecutando con permisos de Administrador.
) else (
    echo [AVISO] No se detectaron permisos de Administrador. 
    echo Si el proceso falla, intenta darle clic derecho "Ejecutar como administrador".
)

:: 1. Verificación de Requisitos (Software Base)
echo.
echo [1/4] Verificando Requisitos de Software...

set "DOTNET_OK=0"
dotnet --version >nul 2>&1 && set "DOTNET_OK=1"
if "!DOTNET_OK!"=="0" (
    echo [ERROR] .NET 8 SDK no está instalado.
    echo Descárgalo aquí: https://dotnet.microsoft.com/download/dotnet/8.0
    pause & exit /b 1
)
echo   - OK (.NET 8 SDK detectado)

set "PYTHON_OK=0"
python --version >nul 2>&1 && set "PYTHON_OK=1"
if "!PYTHON_OK!"=="0" (
    echo [ERROR] Python no está instalado o no se agregó al PATH.
    echo Instala Python 3.10+ y MARCA "Add Python to PATH".
    pause & exit /b 1
)
echo   - OK (Python detectado)

:: 2. Configuración Inteligente de appsettings.json
echo.
echo [2/4] Configurando Base de Datos (PostgreSQL)...
set "APP_DIR=%~dp0AttendanceSystem"
set "JSON_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.json"
set "EXAMPLE_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.example.json"

if not exist "%JSON_PATH%" (
    echo.
    echo ***************************************************************************
    echo   ASISTENTE DE CONFIGURACION DE BASE DE DATOS
    echo ***************************************************************************
    echo.
    echo Parece que es la primera vez que inicias el sistema.
    echo Necesitamos conectar la base de datos de PostgreSQL.
    echo.
    set /p "DB_PASS=>> Ingrese la CONTRASEÑA de su base de datos PostgreSQL: "
    
    if "%DB_PASS%"=="" (
        echo [ERROR] La contraseña no puede estar vacía.
        pause & exit /b 1
    )

    echo.
    echo Creando configuracion local...
    copy "%EXAMPLE_PATH%" "%JSON_PATH%" >nul

    :: Usar PowerShell para reemplazar la contraseña en el JSON de forma segura
    powershell -Command "(gc '%JSON_PATH%') -replace 'CAMBIAR_POR_TU_CONTRASEÑA', '%DB_PASS%' | Out-File -encoding utf8 '%JSON_PATH%'"
    
    if errorlevel 1 (
        echo [ERROR] Falló al escribir la contraseña en el archivo.
        pause & exit /b 1
    )
    echo ✅ Base de datos configurada con éxito.
) else (
    echo   - OK (Ya existe appsettings.json configurado)
)

:: 3. Preparación de Motor IA (Python Venv)
echo.
echo [3/4] Preparando Inteligencia Artificial (Biometría)...
cd /d "%APP_DIR%\src\FaceService"

if not exist "venv\Scripts\python.exe" (
    echo   - Creando entorno virtual aislado (venv)...
    python -m venv venv
)

echo   - Instalando librerías especializadas (InsightFace)...
call venv\Scripts\activate.bat
python install.py
if errorlevel 1 (
    echo [ERROR] Falló la instalación de IA. Revisa tu conexión a internet.
    pause & exit /b 1
)

:: 4. Lanzamiento Final
echo.
echo [4/4] Compilando e Iniciando Sistema Principal...
echo ===========================================================================
echo.
cd /d "%APP_DIR%"
dotnet run --project src\AttendanceSystem.App

if errorlevel 1 (
    echo.
    echo [ERROR] El servidor no pudo iniciar. 
    echo Verifica que PostgreSQL esté corriendo y la contraseña ingresada.
    pause & exit /b 1
)

pause
exit /b 0
