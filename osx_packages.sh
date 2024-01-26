#!/bin/bash

export PublishDir="$(pwd)"
export Version=$(grep 'ProductVersion' $PublishDir/../src/Asv.Drones.Gui.Custom.props | sed -e 's/^.*ProductVersion>\([^<]*\)<.*$/\1/')

build_app_package() {
    platform=$1

    mkdir -p asv-drones.app/Contents/MacOS asv-drones.app/Contents/Resources asv-drones.app/Contents/Frameworks

    cp $PublishDir/../src/Asv.Drones.Gui/Assets/icon.icns asv-drones.app/Contents/Resources
    cp -a $PublishDir/$platform/app/. asv-drones.app/Contents/MacOS

    chmod 777 asv-drones.app/Contents/MacOS/asv-drones-$platform

    cat > asv-drones.app/Contents/Info.plist << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>asv-drones-$platform</string>
    <key>CFBundleIconFile</key>
    <string>icon.icns</string>
    <key>CFBundleIdentifier</key>
    <string>com.asv.drones</string>
    <key>CFBundleName</key>
    <string>Asv.Drones</string>
    <key>CFBundleVersion</key>
    <string>$Version</string>
    <key>CFBundleShortVersionString</key>
    <string>$Version</string>
</dict>
</plist>
EOF

    tar -czvf asv-drones-$platform.tar.gz asv-drones.app
    cp asv-drones-$platform.tar.gz $PublishDir/$platform
    rm -r asv-drones.app
    rm -r asv-drones-$platform.tar.gz
}

cd ~

build_app_package osx-arm64
build_app_package osx-x64
