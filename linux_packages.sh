#!/bin/bash

export PublishDir="$(pwd)"
export Version=$(grep 'ProductVersion' $PublishDir/../src/Asv.Drones.Gui.Custom.props | sed -e 's/^.*ProductVersion>\([^<]*\)<.*$/\1/')

check_and_install_package() {
    package_name=$1
    if ! dpkg -l | grep -qw $package_name; then
        echo "Package $package_name is not installed. Installing..."
        sudo apt-get update
        sudo apt-get install -y $package_name
    else
        echo "Package $package_name already installed."
    fi
}

build_deb_package() {
    platform=$1
    architecture="all"

    case "$platform" in
        "linux-arm")
            architecture="armhf"  
            ;;
        "linux-arm64")
            architecture="arm64"
            ;;
        "linux-musl-x64" | "linux-x64")
            architecture="amd64"
            ;;
    esac

    mkdir -p asv-drones/DEBIAN asv-drones/usr/bin asv-drones/usr/share/applications asv-drones/usr/share/icons

    cp $PublishDir/../src/Asv.Drones.Gui/Assets/icon.png asv-drones/usr/share/icons/asv-drones.png
    
    cat > asv-drones/usr/share/applications/asv-drones.desktop << EOF
[Desktop Entry]
Name=Asv.Drones
Icon=/usr/share/icons/asv-drones.png
Exec=asv-drones-$platform %u
Type=Application
Terminal=false
Categories=Utility;
EOF
    chmod 755 asv-drones/usr/share/applications/asv-drones.desktop

    cat > asv-drones/DEBIAN/control << EOF
Package: asv-drones
Version: $Version
Section: utils
Priority: optional
Architecture: $architecture
Maintainer: Asv.Soft <me@asv.me>
Description: Open source implementation of ground control station application for ArduPilot and PX4 autopilot.
EOF
    chmod 755 asv-drones/DEBIAN/control

    cp $PublishDir/$platform/app/asv-drones-$platform asv-drones/usr/bin

    chmod 777 asv-drones/usr/bin/asv-drones-$platform

    dpkg-deb --build asv-drones
    sudo -S alien -r asv-drones.deb

    cp asv-drones.deb $PublishDir/$platform/asv-drones-$platform.deb
    cp *.rpm $PublishDir/$platform

    rm -r asv-drones
    rm -f *.deb
    rm -f *.rpm
}

check_and_install_package alien

cd ~

build_deb_package linux-arm
build_deb_package linux-arm64
build_deb_package linux-musl-x64
build_deb_package linux-x64
