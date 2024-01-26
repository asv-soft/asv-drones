#define MyAppName "Asv.Drones"  
#define MyAppVersion "0.2.4" 
#define MyAppPublisher "Asv.Soft LLC"  
#define MyAppURL "https://www.asv.me"  
#define MyAppExeName "asv-drones-win-x64.exe"  
[Setup]  
AppId={{C5CE850B-61D6-417A-8855-AD02C9BEA9FD}  
AppName={#MyAppName}  
AppVersion={#MyAppVersion}  
AppPublisher={#MyAppPublisher}  
AppPublisherURL={#MyAppURL}  
AppSupportURL={#MyAppURL}  
AppUpdatesURL={#MyAppURL}  
DefaultDirName={autopf}\{#MyAppName}  
DisableProgramGroupPage=yes  
LicenseFile=License  
OutputDir=publish\win-x64  
OutputBaseFilename=asv-drones-win-x64-install  
SetupIconFile=src\Asv.Drones.Gui\Assets\icon.ico  
Compression=lzma  
SolidCompression=yes  
WizardStyle=modern  
[Languages]  
Name: "english"; MessagesFile: "compiler:Default.isl"  
[Tasks]  
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked  
[Files]  
Source: "publish\win-x64\app\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion  
[Icons]  
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"  
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon  
[Run]  
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent  
