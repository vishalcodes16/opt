#define MyAppName "Vishal X Opt"
#define MyAppVersion "1.0.0"
#define MyAppExeName "VishalXOpt.exe"
[Setup]
AppId={{6F5B5C21-7E0A-4B2D-A32C-7B20A7C8B7F0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\VishalXOpt
DefaultGroupName=Vishal X Opt
OutputDir=.\out
OutputBaseFilename=VishalXOpt-v1.0.0-Setup
ArchitecturesInstallIn64BitMode=x64
Compression=lzma
SolidCompression=yes
[Files]
Source: "..\publish\VishalXOpt.exe"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
Name: "{group}\Vishal X Opt"; Filename: "{app}\VishalXOpt.exe"
Name: "{autodesktop}\Vishal X Opt"; Filename: "{app}\VishalXOpt.exe"; Tasks: desktopicon
[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"