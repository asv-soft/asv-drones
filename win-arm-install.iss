#define MyAppName "Asv.Drones"    
#define MyAppVersion "0.2.2" 
#define MyAppPublisher "Asv.Soft LLC"    
#define MyAppURL "https://www.asv.me"    
#define MyAppExeName "asv-drones-win-arm.exe"    
[Setup]    
AppId={{AD0A5B4D-D17A-4301-A387-892193920F9D}    
AppName={#MyAppName}    
AppVersion={#MyAppVersion}    
AppPublisher={#MyAppPublisher}    
AppPublisherURL={#MyAppURL}    
AppSupportURL={#MyAppURL}    
AppUpdatesURL={#MyAppURL}    
DefaultDirName={autopf}\AsvDrones    
DisableProgramGroupPage=yes    
LicenseFile=License    
OutputDir=publish\win-arm    
OutputBaseFilename=asv-drones-win-arm-install    
SetupIconFile=src\Asv.Drones.Gui\Assets\icon.ico    
Compression=lzma    
SolidCompression=yes    
WizardStyle=modern    
[Languages]    
Name: "english"; MessagesFile: "compiler:Default.isl"    
[Tasks]    
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked    
[Files]    
Source: "publish\win-arm\app\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion    
[Icons]    
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"    
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon    
[Run]    
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent    
