; ============================================================================
;  RAMar - Control de Asistencia — Inno Setup Script
;  
;  Genera un instalador profesional (.exe) que incluye:
;    1. Aplicación WPF (.NET 8) self-contained
;    2. FaceService (microservicio Python de reconocimiento facial)
;    3. Verificación de prerequisitos (Python 3.12, PostgreSQL)
;    4. Post-instalación: entorno virtual + dependencias Python
;
;  Para compilar:
;    - Abrir este archivo con Inno Setup Compiler
;    - O ejecutar: iscc AttendanceSystem_Installer.iss
;    - O usar el script: .\build_installer.ps1
; ============================================================================

#define MyAppName        "RAMar - Control de Asistencia"
#define MyAppVersion     "1.0.0"
#define MyAppPublisher   "RAMar Software Studio"
#define MyAppURL         "https://github.com/ramarstudio"
#define MyAppExeName     "AttendanceSystem.App.exe"
#define MyAppCopyright   "© 2026 RAMar Software Studio"

; Rutas relativas al directorio del script .iss
#define PublishDir       "..\publish\app"
#define FaceServiceDir   "..\src\FaceService"

[Setup]
; Identificador único de la aplicación (NO cambiar entre versiones)
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppCopyright={#MyAppCopyright}

; Directorio de instalación por defecto
DefaultDirName={autopf}\RAMar\ControlAsistencia
DefaultGroupName={#MyAppName}

; Archivos de salida
OutputDir=output
OutputBaseFilename=Setup_RAMar_Asistencia_v{#MyAppVersion}

; Compresión
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

; Apariencia del wizard
WizardStyle=modern
WizardSizePercent=110,110

; Privilegios (requiere administrador para Program Files)
PrivilegesRequired=admin

; Versión mínima de Windows
MinVersion=10.0

; Desinstalador
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Uninstallable=yes
CreateUninstallRegKey=yes

; Arquitectura
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";  Description: "Crear acceso directo en el &Escritorio"; GroupDescription: "Accesos directos:"; Flags: checkedonce
Name: "quicklaunchicon"; Description: "Crear acceso directo en la &barra de tareas"; GroupDescription: "Accesos directos:"; Flags: unchecked

[Files]
; ── Aplicación principal (.NET 8 self-contained) ──────────────────────────────
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; ── FaceService (Python) ──────────────────────────────────────────────────────
; Se copian todos los archivos de FaceService excepto venv/, __pycache__/, .env
Source: "{#FaceServiceDir}\*.py";            DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\*.txt";           DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\.env.example";    DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\app\*";           DestDir: "{app}\FaceService\app"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#FaceServiceDir}\tests\*";         DestDir: "{app}\FaceService\tests"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists(ExpandConstant('{#FaceServiceDir}\tests'))

[Icons]
; ── Menú Inicio ───────────────────────────────────────────────────────────────
Name: "{group}\{#MyAppName}";               Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Desinstalar {#MyAppName}";   Filename: "{uninstallexe}"

; ── Escritorio (si el usuario lo elige) ───────────────────────────────────────
Name: "{autodesktop}\{#MyAppName}";         Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; WorkingDir: "{app}"

[Run]
; ── Post-instalación: Desbloquear archivos (Mark of the Web) ───────────────────
; Evita que Windows Defender o SmartScreen bloqueen las DLLs al abrir la app
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Get-ChildItem -Path '{app}' -Recurse | Unblock-File"""; \
    StatusMsg: "Desbloqueando componentes del sistema..."; \
    Flags: runhidden waituntilterminated; \
    Check: not WizardSilent

; ── Post-instalación: instalar entorno virtual de FaceService ─────────────────
Filename: "{app}\FaceService\setup_faceservice.bat"; \
    Parameters: ""; \
    WorkingDir: "{app}\FaceService"; \
    StatusMsg: "Instalando librerías de Python para FaceService (abriendo consola)..."; \
    Flags: waituntilterminated; \
    Check: PythonInstalled

; ── Lanzar la aplicación al terminar ──────────────────────────────────────────
Filename: "{app}\{#MyAppExeName}"; \
    Description: "Iniciar {#MyAppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallRun]

[UninstallDelete]
; Limpiar archivos generados en runtime
Type: filesandordirs; Name: "{app}\FaceService\venv"
Type: filesandordirs; Name: "{app}\FaceService\__pycache__"
Type: filesandordirs; Name: "{app}\Exportaciones"
Type: dirifempty;     Name: "{app}"

[Code]
// ═══════════════════════════════════════════════════════════════════════════════
//  Pascal Script — Verificación de prerequisitos
// ═══════════════════════════════════════════════════════════════════════════════

function PythonInstalled: Boolean;
var
  ResultCode: Integer;
begin
  // Intenta ejecutar py -3.12 --version
  Result := Exec('py', '-3.12 --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
            and (ResultCode = 0);

  if not Result then
  begin
    // Intenta python --version como fallback
    Result := Exec('python', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
              and (ResultCode = 0);
  end;
end;


function PostgreSQLInstalled: Boolean;
var
  SubKeys: TArrayOfString;
  I, ResultCode: Integer;
begin
  Result := False;

  // Método 1: Comando pg_isready (si está en el PATH)
  if Exec('pg_isready', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0) then
  begin
    Result := True;
    Exit;
  end;

  // Método 2: Buscar en el registro de Windows (funciona con cualquier versión)
  if RegGetSubkeyNames(HKLM, 'SOFTWARE\PostgreSQL\Installations', SubKeys) then
  begin
    if GetArrayLength(SubKeys) > 0 then
    begin
      Result := True;
      Exit;
    end;
  end;

  // Método 3: Buscar carpetas de PostgreSQL en Program Files (versiones 14-20)
  for I := 20 downto 14 do
  begin
    if DirExists('C:\Program Files\PostgreSQL\' + IntToStr(I)) then
    begin
      Result := True;
      Exit;
    end;
  end;
end;


procedure ShowPrerequisiteWarnings;
var
  Msg: String;
  HasWarning: Boolean;
begin
  HasWarning := False;
  Msg := 'ATENCIÓN — Se detectaron prerequisitos faltantes:' + #13#10 + #13#10;

  if not PythonInstalled then
  begin
    Msg := Msg + '⚠ Python 3.12 no fue detectado.' + #13#10
               + '  El motor de reconocimiento facial NO se instalará automáticamente.' + #13#10
               + '  Instálalo con: winget install Python.Python.3.12' + #13#10
               + '  Luego ejecuta manualmente: FaceService\setup_faceservice.bat' + #13#10 + #13#10;
    HasWarning := True;
  end;

  if not PostgreSQLInstalled then
  begin
    Msg := Msg + '⚠ PostgreSQL no fue detectado.' + #13#10
               + '  La base de datos es necesaria para el funcionamiento del sistema.' + #13#10
               + '  Descárgalo de: https://www.postgresql.org/download/windows/' + #13#10 + #13#10;
    HasWarning := True;
  end;

  if HasWarning then
  begin
    Msg := Msg + 'Puedes continuar la instalación, pero el sistema puede no funcionar' + #13#10
               + 'completamente hasta que instales los componentes faltantes.';
    MsgBox(Msg, mbInformation, MB_OK);
  end;
end;


function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  // Mostrar advertencias al salir de la página de bienvenida
  if CurPageID = wpWelcome then
  begin
    ShowPrerequisiteWarnings;
  end;
end;
