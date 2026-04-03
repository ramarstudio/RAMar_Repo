@echo off
:: ############################################################################
:: RAMar Software Studio - Control de Asistencia Biometrico
:: Lanzador principal - delega toda la logica a PowerShell
:: ############################################################################
chcp 65001 >nul
title RAMar - Control de Asistencia Biometrico

:: Verificar que PowerShell este disponible
powershell -NoProfile -Command "exit 0" >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] PowerShell no encontrado. Es requerido por este instalador.
    pause & exit /b 1
)

:: Ejecutar el script PowerShell principal
:: NOTA: no pasar -ScriptDir porque %~dp0 termina en \ y corrompe el argumento
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1"
if %errorlevel% neq 0 (
    echo.
    echo Presiona cualquier tecla para cerrar...
    pause >nul
)
exit /b 0
