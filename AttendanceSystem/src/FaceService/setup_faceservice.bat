@echo off
title FaceService - Configuracion Post-Instalacion
REM ============================================================================
REM  setup_faceservice.bat — Configura el entorno virtual del FaceService
REM
REM  Invocado automaticamente por el instalador Inno Setup.
REM  Tambien puede ejecutarse manualmente para reparar o reinstalar.
REM
REM  Requiere: Python 3.10, 3.11 o 3.12 instalado en el sistema.
REM ============================================================================

echo ============================================================
echo   FaceService — Configuracion Post-Instalacion
echo ============================================================
echo.

set PYTHON_EXE=
set PY_FOUND=0

REM ── Opcion 1: Python Launcher 3.12 ──────────────────────────────────────────
py -3.12 --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=py -3.12
    set PY_FOUND=1
    echo   [OK] Usando Python 3.12 via Python Launcher
    goto :found_python
)

REM ── Opcion 2: Python Launcher 3.11 ──────────────────────────────────────────
py -3.11 --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=py -3.11
    set PY_FOUND=1
    echo   [OK] Usando Python 3.11 via Python Launcher
    goto :found_python
)

REM ── Opcion 3: Python Launcher 3.10 ──────────────────────────────────────────
py -3.10 --version >nul 2>&1
if %ERRORLEVEL%==0 (
    set PYTHON_EXE=py -3.10
    set PY_FOUND=1
    echo   [OK] Usando Python 3.10 via Python Launcher
    goto :found_python
)

REM ── Opcion 4: python en PATH (con validacion de version) ────────────────────
python --version >nul 2>&1
if %ERRORLEVEL%==0 (
    for /f "tokens=2" %%V in ('python --version 2^>^&1') do set PY_VER=%%V
    echo   Python en PATH detectado: %PY_VER%
    for /f "tokens=1,2 delims=." %%A in ("%PY_VER%") do (
        set PY_MAJOR=%%A
        set PY_MINOR=%%B
    )
    if "%PY_MAJOR%"=="3" (
        if "%PY_MINOR%"=="10" ( set PYTHON_EXE=python & set PY_FOUND=1 & echo   [OK] Python 3.10 compatible & goto :found_python )
        if "%PY_MINOR%"=="11" ( set PYTHON_EXE=python & set PY_FOUND=1 & echo   [OK] Python 3.11 compatible & goto :found_python )
        if "%PY_MINOR%"=="12" ( set PYTHON_EXE=python & set PY_FOUND=1 & echo   [OK] Python 3.12 compatible & goto :found_python )
        echo   [!!] Python %PY_VER% en PATH no es compatible con onnxruntime.
        echo        Se requiere 3.10, 3.11 o 3.12.
    )
)

REM ── Sin Python compatible ────────────────────────────────────────────────────
echo.
echo ============================================================
echo   ERROR: No se encontro Python 3.10 / 3.11 / 3.12
echo ============================================================
echo.
echo   Instala Python 3.12 con:
echo     winget install Python.Python.3.12
echo.
echo   O descarga desde:
echo     https://www.python.org/downloads/
echo   (marca "Add Python to PATH" al instalar)
echo.
echo   Luego ejecuta este script de nuevo.
echo ============================================================
exit /b 1

:found_python
echo.
echo   Ejecutando instalador de dependencias...
echo   (esto puede tardar varios minutos la primera vez)
echo.

%PYTHON_EXE% "%~dp0install.py"
set INSTALL_CODE=%ERRORLEVEL%

echo.
if %INSTALL_CODE%==0 (
    echo ============================================================
    echo   FaceService configurado exitosamente.
    echo   El motor de reconocimiento facial esta listo.
    echo ============================================================
) else (
    echo ============================================================
    echo   ERROR durante la configuracion del FaceService.
    echo   Codigo de error: %INSTALL_CODE%
    echo.
    echo   Revisa el log de instalacion en:
    echo     %~dp0install.log
    echo ============================================================
)

exit /b %INSTALL_CODE%
