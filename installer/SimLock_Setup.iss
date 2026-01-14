; SimLock Installer Script for Inno Setup
; Compile this with Inno Setup to create SimLock_Setup.exe
; Copyright (c) SimWoods Golf. All rights reserved.

#define MyAppName "SimLock"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "SimWoods Golf"
#define MyAppURL "https://simwoodsgolf.com"
#define MyAppExeName "SimLock.Launcher.exe"
#define MyAppCopyright "Copyright (c) 2024 SimWoods Golf"

[Setup]
; Basic installer info
AppId={{8A7B6C5D-4E3F-2A1B-0C9D-8E7F6A5B4C3D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright={#MyAppCopyright}

; Install location
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Update/Upgrade handling
UsePreviousAppDir=yes
UsePreviousTasks=yes
; Note: We kill processes in PrepareToInstall instead of using AppMutex
CloseApplications=force
CloseApplicationsFilter=*.exe

; Allow same version reinstall (downgrade handled by custom code)
; Note: Inno Setup doesn't have AllowDowngrade - our InitializeSetup handles it

; Creates proper Add/Remove Programs entry
CreateUninstallRegKey=yes
Uninstallable=yes
UninstallFilesDir={app}\uninstall

; Output settings
OutputDir=..\installer_output
OutputBaseFilename=SimLock_Setup
SetupIconFile=..\Assets\app.ico
Compression=lzma2
SolidCompression=yes

; Branding (uncomment if wizard images are available)
; WizardImageFile=..\Assets\wizard.bmp
; WizardSmallImageFile=..\Assets\wizard-small.bmp

; Privileges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Visual
WizardStyle=modern
WizardSizePercent=100

; Uninstaller
UninstallDisplayIcon={app}\SimLock.exe
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startupicon"; Description: "Start SimLock Monitor with Windows"; GroupDescription: "Startup:"

[Files]
; Main application files from publish folder
Source: "..\publish\SimLock.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\SimLock.Admin.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\SimLock.Monitor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\SimLock.Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\publish\*.deps.json"; DestDir: "{app}"; Flags: ignoreversion

; Config and Assets from project root
Source: "..\config.json"; DestDir: "{app}"; Flags: ignoreversion onlyifdoesntexist
Source: "..\Assets\app.ico"; DestDir: "{app}\Assets"; Flags: ignoreversion
Source: "..\Assets\PLACEHOLDER.txt"; DestDir: "{app}\Assets"; Flags: ignoreversion skipifsourcedoesntexist
Source: "..\Videos\PLACEHOLDER.txt"; DestDir: "{app}\Videos"; Flags: ignoreversion skipifsourcedoesntexist

[Dirs]
Name: "{app}\Assets"; Permissions: users-modify
Name: "{app}\Videos"; Permissions: users-modify

[Icons]
; Start Menu shortcuts
Name: "{group}\SimLock Admin"; Filename: "{app}\SimLock.Admin.exe"; IconFilename: "{app}\Assets\app.ico"; Comment: "Configure SimLock settings"
Name: "{group}\SimLock Monitor"; Filename: "{app}\SimLock.Monitor.exe"; IconFilename: "{app}\Assets\app.ico"; Comment: "Start SimLock process monitor"
Name: "{group}\Launch SimLock"; Filename: "{app}\SimLock.Launcher.exe"; IconFilename: "{app}\Assets\app.ico"; Comment: "Launch SimLock lock screen"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop shortcut (optional)
Name: "{autodesktop}\SimLock Admin"; Filename: "{app}\SimLock.Admin.exe"; IconFilename: "{app}\Assets\app.ico"; Tasks: desktopicon

; Startup shortcut (common startup for all users since running as admin)
Name: "{commonstartup}\SimLock Monitor"; Filename: "{app}\SimLock.Monitor.exe"; IconFilename: "{app}\Assets\app.ico"; Tasks: startupicon

[Run]
; Run after install
Filename: "{app}\SimLock.Monitor.exe"; Description: "Start SimLock Monitor"; Flags: nowait postinstall skipifsilent
Filename: "{app}\SimLock.Admin.exe"; Description: "Open Admin Panel to configure settings"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Stop monitor before uninstall
Filename: "taskkill.exe"; Parameters: "/F /IM SimLock.Monitor.exe"; Flags: runhidden; RunOnceId: "StopMonitor"
Filename: "taskkill.exe"; Parameters: "/F /IM SimLock.exe"; Flags: runhidden; RunOnceId: "StopSimLock"

[UninstallDelete]
; Clean up folders on uninstall
Type: filesandordirs; Name: "{app}\Assets"
Type: filesandordirs; Name: "{app}\Videos"
; Clean up ProgramData settings folder
Type: filesandordirs; Name: "{commonappdata}\SimLock"

[Code]
var
  DotNetDownloadPage: TDownloadWizardPage;

// Check if .NET 8 Desktop Runtime is installed
function IsDotNet8Installed(): Boolean;
var
  ResultCode: Integer;
begin
  Result := False;

  // Try to run dotnet --list-runtimes and check for WindowsDesktop 8.x
  if Exec('cmd.exe', '/c dotnet --list-runtimes 2>nul | findstr /C:"Microsoft.WindowsDesktop.App 8."', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := (ResultCode = 0);
  end;
end;

// Check if SimLock is already installed
function IsSimLockInstalled(): Boolean;
var
  UninstallKey: String;
begin
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8A7B6C5D-4E3F-2A1B-0C9D-8E7F6A5B4C3D}_is1';
  Result := RegKeyExists(HKLM, UninstallKey) or RegKeyExists(HKCU, UninstallKey);
end;

// Get installed version
function GetInstalledVersion(): String;
var
  UninstallKey: String;
  Version: String;
begin
  Result := '';
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8A7B6C5D-4E3F-2A1B-0C9D-8E7F6A5B4C3D}_is1';

  if RegQueryStringValue(HKLM, UninstallKey, 'DisplayVersion', Version) then
    Result := Version
  else if RegQueryStringValue(HKCU, UninstallKey, 'DisplayVersion', Version) then
    Result := Version;
end;

// Get uninstall string
function GetUninstallString(): String;
var
  UninstallKey: String;
  UninstallStr: String;
begin
  Result := '';
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8A7B6C5D-4E3F-2A1B-0C9D-8E7F6A5B4C3D}_is1';

  if RegQueryStringValue(HKLM, UninstallKey, 'UninstallString', UninstallStr) then
    Result := UninstallStr
  else if RegQueryStringValue(HKCU, UninstallKey, 'UninstallString', UninstallStr) then
    Result := UninstallStr;
end;

// Uninstall existing version
function UninstallExisting(): Boolean;
var
  UninstallStr: String;
  ResultCode: Integer;
begin
  Result := True;
  UninstallStr := GetUninstallString();

  if UninstallStr <> '' then
  begin
    // Add /SILENT to uninstall quietly
    UninstallStr := RemoveQuotes(UninstallStr);
    Result := Exec(UninstallStr, '/SILENT /NORESTART', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

// Download and install .NET 8 silently
function InstallDotNet8(): Boolean;
var
  ResultCode: Integer;
  TempPath: String;
  InstallerPath: String;
begin
  Result := False;
  TempPath := ExpandConstant('{tmp}');
  InstallerPath := TempPath + '\windowsdesktop-runtime-8.0-win-x64.exe';

  // Download .NET 8 runtime
  DotNetDownloadPage.Clear;
  DotNetDownloadPage.Add('https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe', 'windowsdesktop-runtime-8.0-win-x64.exe', '');
  DotNetDownloadPage.Show;

  try
    DotNetDownloadPage.Download;
    DotNetDownloadPage.Hide;

    // Install silently
    WizardForm.StatusLabel.Caption := 'Installing .NET 8 Desktop Runtime...';
    WizardForm.ProgressGauge.Style := npbstMarquee;

    if Exec(InstallerPath, '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      Result := (ResultCode = 0);
    end;

    WizardForm.ProgressGauge.Style := npbstNormal;
  except
    DotNetDownloadPage.Hide;
    Result := False;
  end;
end;

procedure InitializeWizard();
begin
  DotNetDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
  DotNetDownloadPage.ShowBaseNameInsteadOfUrl := True;
end;

// Check for existing installation and prompt user
function InitializeSetup(): Boolean;
var
  InstalledVersion: String;
  Choice: Integer;
begin
  Result := True;

  if IsSimLockInstalled() then
  begin
    InstalledVersion := GetInstalledVersion();

    Choice := MsgBox('SimLock ' + InstalledVersion + ' is already installed.' + #13#10 + #13#10 +
                     'What would you like to do?' + #13#10 + #13#10 +
                     'YES = Update/Reinstall (keeps settings)' + #13#10 +
                     'NO = Uninstall existing version first' + #13#10 +
                     'CANCEL = Exit setup',
                     mbConfirmation, MB_YESNOCANCEL);

    case Choice of
      IDYES:
        begin
          // Continue with installation (will overwrite)
          Result := True;
        end;
      IDNO:
        begin
          // Uninstall first, then continue
          if UninstallExisting() then
            Result := True
          else
          begin
            MsgBox('Failed to uninstall existing version. Please uninstall manually and try again.',
                   mbError, MB_OK);
            Result := False;
          end;
        end;
      IDCANCEL:
        begin
          Result := False;
        end;
    end;
  end;
end;

// Stop running SimLock processes
procedure StopSimLockProcesses();
var
  ResultCode: Integer;
begin
  // Kill monitor process
  Exec('taskkill.exe', '/F /IM SimLock.Monitor.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  // Kill main lock screen
  Exec('taskkill.exe', '/F /IM SimLock.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  // Kill admin panel
  Exec('taskkill.exe', '/F /IM SimLock.Admin.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  // Small delay to ensure processes are fully terminated
  Sleep(500);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  Installed: Boolean;
begin
  Result := '';

  // Stop any running SimLock processes first
  StopSimLockProcesses();

  if not IsDotNet8Installed() then
  begin
    if MsgBox('.NET 8 Desktop Runtime is required.' + #13#10 + #13#10 +
              'Click OK to download and install it automatically.',
              mbInformation, MB_OKCANCEL) = IDOK then
    begin
      Installed := InstallDotNet8();
      if not Installed then
      begin
        Result := '.NET 8 installation failed. Please install it manually from: https://dotnet.microsoft.com/download/dotnet/8.0';
      end;
    end
    else
    begin
      Result := '.NET 8 Desktop Runtime is required to run SimLock.';
    end;
  end;
end;

// Make config.json writable after install
procedure CurStepChanged(CurStep: TSetupStep);
var
  ConfigPath: String;
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    ConfigPath := ExpandConstant('{app}\config.json');
    // Remove read-only attribute if present
    if FileExists(ConfigPath) then
    begin
      Exec('attrib.exe', '-R "' + ConfigPath + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    end;
  end;
end;

// Stop processes before uninstall
function InitializeUninstall(): Boolean;
begin
  Result := True;
  StopSimLockProcesses();
end;
