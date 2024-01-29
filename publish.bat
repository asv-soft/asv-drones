cd publish

for /d %%i in (".\*") do (
    rmdir /s /q "%%i"
)

cd ../src/Asv.Drones.Gui.Desktop

dotnet publish -c Release -r win-arm --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true  -o ~/../../../publish/win-arm/app
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true  -o ~/../../../publish/win-arm64/app
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/win-x64/app
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/win-x86/app

dotnet publish -c Release -r linux-arm --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/linux-arm/app
dotnet publish -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/linux-arm64/app
dotnet publish -c Release -r linux-musl-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/linux-musl-x64/app
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/linux-x64/app

dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/osx-arm64/app
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ~/../../../publish/osx-x64/app

cd ../../publish
del /S *.pdb

cd win-arm/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-arm.exe
cd ../../win-arm64/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-arm64.exe
cd ../../win-x64/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-x64.exe
cd ../../win-x86/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-x86.exe

cd ../../linux-arm/app
move Asv.Drones.Gui.Desktop asv-drones-linux-arm
cd ../../linux-arm64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-arm64 
cd ../../linux-musl-x64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-musl-x64
cd ../../linux-x64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-x64

cd ../../osx-arm64/app
move Asv.Drones.Gui.Desktop asv-drones-osx-arm64
cd ../../osx-x64/app
move Asv.Drones.Gui.Desktop asv-drones-osx-x64
cd ../../..

setlocal enabledelayedexpansion

set "xmlFile=src\Asv.Drones.Gui.Custom.props"

for /f "tokens=2 delims=<> " %%a in ('findstr /i "<ProductVersion>" "%xmlFile%"') do (
    set "productVersion=%%a"
)

set "issFile=win-arm-install.iss"

set "tempFile=%temp%\temp.iss"

for /f "tokens=*" %%a in ('type "%issFile%"') do (
    set "line=%%a"

    echo !line! | findstr /C:"#define MyAppVersion" > nul

    if !errorlevel! == 0 (
        echo #define MyAppVersion "%productVersion%" >> "%tempFile%"
    ) else (
        echo !line! >> "%tempFile%"
    )
)

move /y "%tempFile%" "%issFile%" > nul

set "issFile=win-arm64-install.iss"

set "tempFile=%temp%\temp.iss"

for /f "tokens=*" %%a in ('type "%issFile%"') do (
    set "line=%%a"

    echo !line! | findstr /C:"#define MyAppVersion" > nul

    if !errorlevel! == 0 (
        echo #define MyAppVersion "%productVersion%" >> "%tempFile%"
    ) else (
        echo !line! >> "%tempFile%"
    )
)

move /y "%tempFile%" "%issFile%" > nul

set "issFile=win-x64-install.iss"

set "tempFile=%temp%\temp.iss"

for /f "tokens=*" %%a in ('type "%issFile%"') do (
    set "line=%%a"

    echo !line! | findstr /C:"#define MyAppVersion" > nul

    if !errorlevel! == 0 (
        echo #define MyAppVersion "%productVersion%" >> "%tempFile%"
    ) else (
        echo !line! >> "%tempFile%"
    )
)

move /y "%tempFile%" "%issFile%" > nul

set "issFile=win-x86-install.iss"

set "tempFile=%temp%\temp.iss"

for /f "tokens=*" %%a in ('type "%issFile%"') do (
    set "line=%%a"

    echo !line! | findstr /C:"#define MyAppVersion" > nul

    if !errorlevel! == 0 (
        echo #define MyAppVersion "%productVersion%" >> "%tempFile%"
    ) else (
        echo !line! >> "%tempFile%"
    )
)

move /y "%tempFile%" "%issFile%" > nul

endlocal

cd publish

iscc ../win-arm-install.iss
iscc ../win-arm64-install.iss
iscc ../win-x86-install.iss
iscc ../win-x64-install.iss


wsl sed -i 's/\r//' linux_packages.sh
wsl sed -i 's/\r//' osx_packages.sh

wsl ../linux_packages.sh

wsl ../osx_packages.sh