#!/bin/bash

# Stop the script when an error happens
set -e

cleanup() {
    echo "Clearing temp files..."
    rm -rf ./dmg_temp
    rm -rf ./publish/app
    rm -rf ./publish
}

trap cleanup EXIT

echo "Building application for macOS ARM..."
dotnet publish -c Release -r osx-arm64 ./src/Asv.Drones.Gui.Desktop/Asv.Drones.Gui.Desktop.csproj -o ./publish/app -p:UseAppHost=true --self-contained

echo "Creating app bundle for macOS"
./bundle-mac-app.sh

echo "Creationg temporary files for DMG..."
mkdir -p ./dmg_temp/MyApp
cp -R "./publish/Asv Drones Gui.app" ./dmg_temp/MyApp/
ln -s /Applications ./dmg_temp/MyApp/Applications

echo "Creating DMG..."
hdiutil create -volname "Asv Drones Gui" -srcfolder ./dmg_temp/MyApp -ov -format UDZO AsvDronesGui-arm64.dmg

echo "DMG was successfully created!"
