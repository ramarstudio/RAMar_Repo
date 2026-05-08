; ============================================================================
;  RAMar - Control de Asistencia - Inno Setup Script
;
;  Genera un instalador profesional (.exe) que incluye:
;    1. Aplicacion WPF (.NET 8) self-contained
;    2. FaceService (microservicio Python de reconocimiento facial)
;    3. Verificacion de prerequisitos (Python 3.10/3.11/3.12, PostgreSQL)
;    4. Pagina de configuracion de base de datos (genera appsettings.json)
;    5. Post-instalacion: venv + dependencias + pre-descarga del modelo
;
;  Para compilar:
;    .\build_installer.ps1
; ============================================================================

#define MyAppName        "RAMar - Control de Asistencia"
#define MyAppVersion     "1.0.0"
#define MyAppPublisher   "RAMar Software Studio"
#define MyAppURL         "https://github.com/ramarstudio"
#define MyAppExeName     "AttendanceSystem.App.exe"
#define MyAppCopyright   "(c) 2026 RAMar Software Studio"

#define PublishDir       "..\publish\app"
#define FaceServiceDir   "..\src\FaceService"
#define LogoFile         "..\..\logo\logo_ramar.png"
#define IconFile         "..\src\AttendanceSystem.App\Resources\logo_ramar.ico"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppCopyright={#MyAppCopyright}

DefaultDirName={autopf}\RAMar\ControlAsistencia
DefaultGroupName={#MyAppName}

OutputDir=output
OutputBaseFilename=Setup_RAMar_Asistencia_v{#MyAppVersion}

Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

WizardStyle=modern
WizardSizePercent=110,110
WizardImageFile={#LogoFile}
WizardSmallImageFile={#LogoFile}
SetupIconFile={#IconFile}

PrivilegesRequired=admin
MinVersion=10.0

UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Uninstallable=yes
CreateUninstallRegKey=yes

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el &Escritorio"; GroupDescription: "Accesos directos:"; Flags: checkedonce

[Files]
; Aplicacion principal (.NET 8 self-contained)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; FaceService (Python)
Source: "{#FaceServiceDir}\*.py";         DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\*.txt";        DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\*.bat";        DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\.env.example"; DestDir: "{app}\FaceService"; Flags: ignoreversion
Source: "{#FaceServiceDir}\.env.example"; DestDir: "{app}\FaceService"; DestName: ".env"; Flags: ignoreversion onlyifdoesntexist
Source: "{#FaceServiceDir}\app\*";        DestDir: "{app}\FaceService\app"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#FaceServiceDir}\tests\*";      DestDir: "{app}\FaceService\tests"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists(ExpandConstant('{#FaceServiceDir}\tests'))

[Icons]
Name: "{group}\{#MyAppName}";             Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";       Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; WorkingDir: "{app}"

[Run]
; Desbloquear archivos (Mark of the Web) para evitar bloqueos de SmartScreen
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Get-ChildItem -Path '{app}' -Recurse | Unblock-File"""; \
    StatusMsg: "Desbloqueando componentes del sistema..."; \
    Flags: runhidden waituntilterminated; \
    Check: not WizardSilent

; Lanzar la aplicacion al terminar
Filename: "{app}\{#MyAppExeName}"; \
    Description: "Iniciar {#MyAppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\FaceService\venv"
Type: filesandordirs; Name: "{app}\FaceService\models"
Type: filesandordirs; Name: "{app}\FaceService\__pycache__"
Type: filesandordirs; Name: "{app}\FaceService\install.log"
Type: filesandordirs; Name: "{app}\Exportaciones"
Type: dirifempty;     Name: "{app}"

[Code]
// ============================================================================
//  Pascal Script
//  - Pagina de configuracion de base de datos
//  - Verificacion de prerequisitos
//  - Generacion automatica de appsettings.json
//  - Setup de FaceService post-instalacion
// ============================================================================

var
  DbPage: TInputQueryWizardPage;


// ── Deteccion de prerequisitos ───────────────────────────────────────────────

function PythonInstalled: Boolean;
var
  ResultCode: Integer;
begin
  if Exec('py', '-3.12 --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
     and (ResultCode = 0) then begin Result := True; Exit; end;
  if Exec('py', '-3.11 --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
     and (ResultCode = 0) then begin Result := True; Exit; end;
  if Exec('py', '-3.10 --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
     and (ResultCode = 0) then begin Result := True; Exit; end;
  Result := Exec('python', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
            and (ResultCode = 0);
end;


function PostgreSQLInstalled: Boolean;
var
  SubKeys: TArrayOfString;
  I, ResultCode: Integer;
begin
  Result := False;
  if Exec('pg_isready', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
     and (ResultCode = 0) then begin Result := True; Exit; end;
  if RegGetSubkeyNames(HKLM, 'SOFTWARE\PostgreSQL\Installations', SubKeys) then
    if GetArrayLength(SubKeys) > 0 then begin Result := True; Exit; end;
  for I := 20 downto 14 do
    if DirExists('C:\Program Files\PostgreSQL\' + IntToStr(I)) then
    begin Result := True; Exit; end;
end;


// ── Pagina de configuracion de base de datos ─────────────────────────────────

procedure InitializeWizard;
begin
  DbPage := CreateInputQueryPage(wpSelectDir,
    'Configuracion de PostgreSQL',
    'Ingresa los datos de conexion a la base de datos',
    'El instalador generara el archivo de configuracion automaticamente.' + #13#10 +
    'No necesitas editar ningun archivo JSON manualmente.');

  DbPage.Add('Contrasena del usuario "postgres":', True);
  DbPage.Add('Host (dejar en blanco para usar localhost):', False);
  DbPage.Add('Nombre de la base de datos:', False);

  DbPage.Values[1] := 'localhost';
  DbPage.Values[2] := 'ramar_asistencia';
end;


// ── Generacion del appsettings.json ──────────────────────────────────────────

procedure WriteAppSettings;
var
  Password, Host, DbName, ConnStr, Content: String;
begin
  Password := DbPage.Values[0];
  Host     := DbPage.Values[1];
  DbName   := DbPage.Values[2];

  if Trim(Host) = ''   then Host   := 'localhost';
  if Trim(DbName) = '' then DbName := 'ramar_asistencia';

  // Escapar comillas simples para Npgsql (dentro del valor entre comillas)
  StringChange(Password, #39, #39 + #39);

  // Envolver en comillas simples protege contra ; en la contrasena
  ConnStr := 'Host=' + Host + ';Port=5432;Database=' + DbName +
             ';Username=postgres;Password=' + #39 + Password + #39;

  // Escapar para JSON (el ConnStr va como valor string en el JSON)
  StringChange(ConnStr, '\', '\\');
  StringChange(ConnStr, '"', '\"');

  Content :=
    '{' + #13#10 +
    '  "ConnectionStrings": {' + #13#10 +
    '    "DefaultConnection": "' + ConnStr + '"' + #13#10 +
    '  },' + #13#10 +
    '  "FacialService": {' + #13#10 +
    '    "BaseUrl": "http://localhost:5001",' + #13#10 +
    '    "VerifyPath": "/api/verify",' + #13#10 +
    '    "EncodePath": "/api/encode",' + #13#10 +
    '    "IdleTimeoutMinutos": "10"' + #13#10 +
    '  },' + #13#10 +
    '  "Empleado": {' + #13#10 +
    '    "HorarioEntradaHora": "8",' + #13#10 +
    '    "HorarioSalidaHora": "17",' + #13#10 +
    '    "ToleranciaMins": "15"' + #13#10 +
    '  },' + #13#10 +
    '  "Exportacion": {' + #13#10 +
    '    "Carpeta": "Exportaciones"' + #13#10 +
    '  }' + #13#10 +
    '}';

  SaveStringToFile(ExpandConstant('{app}\appsettings.json'), Content, False);
end;


// ── Setup de FaceService (con verificacion de codigo de salida) ──────────────

procedure RunFaceServiceSetup;
var
  ResultCode: Integer;
  FaceBat, LogFile, Msg: String;
begin
  FaceBat := ExpandConstant('{app}\FaceService\setup_faceservice.bat');
  LogFile  := ExpandConstant('{app}\FaceService\install.log');

  if not PythonInstalled then
  begin
    MsgBox(
      'Python 3.10, 3.11 o 3.12 no fue detectado.' + #13#10 + #13#10 +
      'El motor de reconocimiento facial NO fue configurado.' + #13#10 + #13#10 +
      'Para configurarlo despues:' + #13#10 +
      '  1. Instala Python 3.12: winget install Python.Python.3.12' + #13#10 +
      '  2. Ejecuta: ' + FaceBat,
      mbError, MB_OK
    );
    Exit;
  end;

  if not Exec(FaceBat, '', ExpandConstant('{app}\FaceService'),
              SW_SHOW, ewWaitUntilTerminated, ResultCode)
     or (ResultCode <> 0) then
  begin
    Msg :=
      'La instalacion de librerias Python no se completo.' + #13#10 +
      'Codigo de error: ' + IntToStr(ResultCode) + #13#10 + #13#10 +
      'Causas comunes:' + #13#10 +
      '  - Sin conexion a internet' + #13#10 +
      '  - Antivirus bloqueo la instalacion' + #13#10 +
      '  - Disco sin espacio suficiente' + #13#10 + #13#10 +
      'Para reintentar: ' + FaceBat;

    if FileExists(LogFile) then
      Msg := Msg + #13#10 + #13#10 + 'Log: ' + LogFile;

    MsgBox(Msg, mbError, MB_OK);
  end;
end;


// ── Flujo del wizard ─────────────────────────────────────────────────────────

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Msg: String;
  HasWarning: Boolean;
begin
  Result := True;

  // Advertencias de prerequisitos en la pagina de bienvenida
  if CurPageID = wpWelcome then
  begin
    HasWarning := False;
    Msg := 'ATENCION - Se detectaron prerequisitos faltantes:' + #13#10 + #13#10;

    if not PythonInstalled then
    begin
      Msg := Msg + '- Python 3.10/3.11/3.12 no detectado.' + #13#10 +
                   '  Instala con: winget install Python.Python.3.12' + #13#10 + #13#10;
      HasWarning := True;
    end;

    if not PostgreSQLInstalled then
    begin
      Msg := Msg + '- PostgreSQL no detectado.' + #13#10 +
                   '  Descarga: https://www.postgresql.org/download/windows/' + #13#10 + #13#10;
      HasWarning := True;
    end;

    if HasWarning then
    begin
      Msg := Msg + 'Puedes continuar, pero el sistema puede no funcionar' + #13#10 +
                   'hasta instalar los componentes faltantes.';
      MsgBox(Msg, mbInformation, MB_OK);
    end;
  end;

  // Validacion de la pagina de base de datos
  if CurPageID = DbPage.ID then
  begin
    if Trim(DbPage.Values[0]) = '' then
    begin
      MsgBox(
        'La contrasena de PostgreSQL es obligatoria.' + #13#10 + #13#10 +
        'Ingresa la contrasena del usuario "postgres" que' + #13#10 +
        'configuraste al instalar PostgreSQL.',
        mbError, MB_OK
      );
      Result := False;
    end;
  end;
end;


procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // 1. Generar appsettings.json con la contrasena ingresada
    WriteAppSettings;
    // 2. Configurar Python / FaceService
    RunFaceServiceSetup;
  end;
end;
