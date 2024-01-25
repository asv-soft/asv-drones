@echo off
cd publish

rmdir /S /Q win-arm
rmdir /S /Q win-arm64
rmdir /S /Q win-x64
rmdir /S /Q win-x86
rmdir /S /Q linux-arm
rmdir /S /Q linux-arm64
rmdir /S /Q linux-musl-x64
rmdir /S /Q linux-x64
rmdir /S /Q osx-arm64
rmdir /S /Q osx-x64

cd ../src/Asv.Drones.Gui.Desktop

dotnet publish -c Release -r win-arm -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/win-arm/app
dotnet publish -c Release -r win-arm64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/win-arm64/app
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/win-x64/app
dotnet publish -c Release -r win-x86 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/win-x86/app
dotnet publish -c Release -r linux-arm -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/linux-arm/app
dotnet publish -c Release -r linux-arm64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/linux-arm64/app
dotnet publish -c Release -r linux-musl-x64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/linux-musl-x64/app
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/linux-x64/app
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/osx-arm64/app
dotnet publish -c Release -r osx-x64 -p:PublishSingleFile=true --self-contained true -o ~/../../../publish/osx-x64/app

cd ../../publish/win-arm/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-arm.exe
del /S *.pdb
cd ../../win-arm64/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-arm64.exe
del /S *.pdb
cd ../../win-x64/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-x64.exe
del /S *.pdb
cd ../../win-x86/app
move Asv.Drones.Gui.Desktop.exe asv-drones-win-x86.exe
del /S *.pdb
cd ../../linux-arm/app
move Asv.Drones.Gui.Desktop asv-drones-linux-arm
del /S *.pdb
cd ../../linux-arm64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-arm64
del /S *.pdb
cd ../../linux-musl-x64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-musl-x64
del /S *.pdb
cd ../../linux-x64/app
move Asv.Drones.Gui.Desktop asv-drones-linux-x64
del /S *.pdb
cd ../../osx-arm64/app
move Asv.Drones.Gui.Desktop asv-drones-osx-arm64
del /S *.pdb
cd ../../osx-x64/app
move Asv.Drones.Gui.Desktop asv-drones-osx-x64
del /S *.pdb