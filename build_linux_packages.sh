#!/bin/bash

cd publish

export PublishDir="$(pwd)"
export Version=$(grep 'ProductVersion' $PublishDir/../src/Asv.Drones.Gui.Custom.props | sed -e 's/^.*ProductVersion>\([^<]*\)<.*$/\1/')

echo $Version

build_deb_package() {
    platform=$1
    architecture="all"

    rm $PublishDir/$platform/asv-drones-$platform.deb

    if [ "$platform" = "linux-arm" ]
    then
    architecture=arm64
    elif [ "$platform" = "linux-arm64" ]
    then
    architecture=arm64
    elif [ "$platform" = "linux-musl-x64" ]
    then
    architecture=amd64
    elif [ "$platform" = "linux-x64" ]
    then
    architecture=amd64
    fi

    mkdir -p asv-drones/DEBIAN asv-drones/usr/bin asv-drones/usr/share/applications asv-drones/usr/share/icons
    cp icon.png asv-drones/usr/share/icons/asv-drones.png
    
    echo "[Desktop Entry]
Name=Asv.Drones
Icon=/usr/share/icons/asv-drones.png
Exec=asv-drones %u
Type=Application
Terminal=false
Categories=Utility;" > asv-drones/usr/share/applications/asv-drones.desktop
    chmod 0755 asv-drones/usr/share/applications/asv-drones.desktop

    echo "Package: asv-drones
Version: $Version
Section: utils
Priority: optional
Architecture: $architecture
Maintainer: Asv.Soft <me@asv.me>
Description: Open source implementation of ground control station application for ArduPilot and PX4 autopilot." > asv-drones/DEBIAN/control
    chmod 0755 asv-drones/DEBIAN/control

    cp $PublishDir/$platform/app/asv-drones-$platform asv-drones/usr/bin
    find $PublishDir/$platform/app -maxdepth 1 -type f ! -name "asv-drones-$platform" -exec cp {} asv-drones/usr/lib \;
    dpkg-deb --build asv-drones
    cp asv-drones.deb $PublishDir/$platform/asv-drones-$platform.deb
    rm -r asv-drones
    rm asv-drones.deb
}

cd ~

cp $PublishDir/../src/Asv.Drones.Gui/Assets/icon.ico ./
convert icon.ico icon.png
rm icon.ico

build_deb_package linux-arm
build_deb_package linux-arm64
build_deb_package linux-musl-x64
build_deb_package linux-x64

rm icon.png
