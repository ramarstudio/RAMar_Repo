@echo off
:: ############################################################################
:: # RAMar Software Studio - Attendance System
:: # Script de Arranque Automático (One-Click)
:: ############################################################################
setlocal
chcp 65001 >nul
title RAMar - Control de Asistencia Biométrico

echo.
echo ===========================================================================
echo    RAMar Software Studio - Attendance System
echo    Inicializador Automático Global (Windows)
echo ===========================================================================
echo.

:: 1. Verificación de Entorno (.NET SDK)
echo [1/4] Verificando Requisitos de Software...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR CRITICO] .NET 8 SDK no está instalado.
    echo Por favor, instálalo desde: https://dotnet.microsoft.com/download/dotnet/8.0
    goto :ERROR
)
echo   - OK (.NET 8 SDK detectado)

:: 2. Verificación de Python
python --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR CRITICO] Python no está instalado o no se agregó al PATH.
    echo Por favor, instala Python 3.10+ y asegúrate de marcar "Add Python to PATH".
    goto :ERROR
)
echo   - OK (Python detectado)

:: 3. Configuración de Base de Datos
echo.
echo [2/4] Verificando Base de Datos...
set "APP_DIR=%~dp0AttendanceSystem"
set "CONFIG_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.json"
set "EXAMPLE_PATH=%APP_DIR%\src\AttendanceSystem.App\appsettings.example.json"

if not exist "%CONFIG_PATH%" (
    echo.
    echo ***************************************************************************
    echo                 CONFIGURACION INICIAL (Primera vez)
    echo ***************************************************************************
    echo.
    echo Creando archivo appsettings.json basándose en el ejemplo...
    copy "%EXAMPLE_PATH%" "%CONFIG_PATH%" >nul
    echo.
    echo PASO OBLIGATORIO:
    echo 1. Abre el archivo: AttendanceSystem\src\AttendanceSystem.App\appsettings.json
    echo 2. Busca "CAMBIAR_POR_TU_CONTRASEÑA" y pon tu clave de PostgreSQL.
    echo 3. Guarda el archivo.
    echo.
    echo Pulsa cualquier tecla una vez que hayas guardado el archivo...
    pause >nul
) else (
    echo   - OK (appsettings.json configurado)
)

:: 4. Motor Biométrico (Python Venv)
echo.
echo [3/4] Preparando el Motor de Inteligencia Artificial (Biometría)...
cd /d "%APP_DIR%\src\FaceService"

if not exist "venv\Scripts\python.exe" (
    echo   - Creando entorno virtual aislado (venv)...
    python -m venv venv
)

echo   - Validando/Instalando librerías faciales (InsightFace)...
echo     (Esto puede tardar unos minutos en la primera ejecución)
call venv\Scripts\activate.bat
python install.py
if errorlevel 1 (
    echo [ERROR] Falló la instalación de librerías de IA. 
    goto :ERROR
)

:: 5. Arranque Final
echo.
echo [4/4] Compilando e Iniciando Sistema...
cd /d "%APP_DIR%"
echo.
echo ===========================================================================
echo    SISTEMA INICIANDO... (No cierres esta ventana)
echo ===========================================================================
echo.
dotnet run --project src\AttendanceSystem.App

if errorlevel 1 (
    echo.
    echo [ERROR] La aplicación se cerró inesperadamente. Revisa los mensajes arriba.
    goto :ERROR
)

goto :FIN

:ERROR
echo.
echo El proceso se interrumpió por un error.
pause
exit /b 1

:FIN
pause
exit /b 0
