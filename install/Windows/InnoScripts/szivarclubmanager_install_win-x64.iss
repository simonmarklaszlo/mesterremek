[Setup]
AppName=SzivarClub Manager
AppVersion=1.0
DefaultDirName={pf}\SzivarClubManager
DefaultGroupName=SzivarClub Manager
OutputDir=..\Output
OutputBaseFilename=win-x64
Compression=lzma
SolidCompression=yes

[Files]
Source: "..\..\..\SzivarClubManager\bin\Release\net10.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\SzivarClub Manager"; Filename: "{app}\SzivarClubManager.exe"
Name: "{commondesktop}\SzivarClub Manager"; Filename: "{app}\SzivarClubManager.exe"

[Run]
Filename: "{app}\SzivarClubManager.exe"; Description: "Launch app"; Flags: nowait postinstall


