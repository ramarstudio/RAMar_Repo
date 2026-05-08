@echo off
REM ============================================================================
REM  setup_faceservice.bat — Configura el entorno virtual del FaceService
REM
REM  Este script es invocado automáticamente por el instalador de Inno Setup
REM  después de copiar los archivos. También puede ejecutarse manualmente.
REM
REM  Requiere: Python 3.10, 3.11 o 3.12 instalado en el sistema.
REM ============================================================================

echo ============================================================
echo   FaceService — Configuracion Post-Instalacion
echo ============================================================

REM Intentar encontrar Python compatible
set PYTHON_EXE=

REM Opción 1: Python Launcher con 3.12
py -3.12 --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=py -3.12
    echo   Usando Python 3.12 via Python Launcher
    goto :found_python
)

REM Opción 2: Python Launcher con 3.11
py -3.11 --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=py -3.11
    echo   Usando Python 3.11 via Python Launcher
    goto :found_python
)

REM Opción 3: python en PATH
python --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=python
    echo   Usando Python en PATH
    goto :found_python
)

echo   ERROR: No se encontro Python compatible.
echo   Instala Python 3.12: winget install Python.Python.3.12
echo   Luego ejecuta este script de nuevo.
exit /b 1

:found_python
echo.
echo   Ejecutando install.py...
echo.

%PYTHON_EXE% "%~dp0install.py"

if %ERRORLEVEL%==0 (
    echo.
    echo ============================================================
    echo   FaceService configurado exitosamente.
    echo ============================================================
) else (
    echo.
    echo ============================================================
    echo   ERROR durante la configuracion del FaceService.
    echo   Revisa los mensajes anteriores para mas detalles.
    echo ============================================================
)

exit /b %ERRORLEVEL%
