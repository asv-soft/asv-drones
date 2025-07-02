; Main constants - define following constants as you want them displayed in your installation wizard
!define PRODUCT_NAME "Asv.Drones"
!define PRODUCT_PUBLISHER "Cursir"

; Following constants you should never change
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!include "MUI.nsh"
!include "nsDialogs.nsh"

!define MUI_ABORTWARNING
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Wizard pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Name "Asv Drones "
OutFile "AsvDronesInstaller.exe"
InstallDir "$PROGRAMFILES\Asv\Asv-Drones"
ShowInstDetails show
ShowUnInstDetails show

; Following lists the files you want to include, go through this list carefully!
Section "Core Files (required)" SEC01
  SetOutPath "$INSTDIR"
  ; Check dir in yml where your project is stored
  File /r "./publish/app\\*.*"
SectionEnd

; Section for shortcuts
Section "Start Menu and Desktop Shortcuts" SEC02
  ; Create a shortcut on the desktop
  CreateShortcut "$DESKTOP\Asv Drones.lnk" "$INSTDIR\Asv.Drones.Desktop.exe" "" "$INSTDIR\Asv.Drones.Desktop.exe" 0
  ; Create a shortcut in the Start menu
  CreateShortcut "$SMPROGRAMS\Asv Drones\Asv Drones.lnk" "$INSTDIR\Asv.Drones.Desktop.exe" "" "$INSTDIR\Asv.Drones.Desktop.exe" 0
SectionEnd

Section -Post
  ;Following lines will make uninstaller work - do not change anything, unless you really want to.
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
SectionEnd

; Replace the following strings to suite your needs
Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "Application was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove Asv Drones and all of its components?" IDYES +2
  Abort
FunctionEnd

; Remove any file that you have added above - removing uninstallation and folders last.
; Note: if there is any file changed or added to these folders, they will not be removed. Also, parent folder (which in my example 
; is company name ZWare) will not be removed if there is any other application installed in it.
Section Uninstall
  ; Remove the installed files
  Delete "$INSTDIR\*.*"
  
  ; Remove the installation directory
  RMDir /r "$INSTDIR"
  
  ; Remove the shortcuts
    Delete "$DESKTOP\Asv Drones.lnk"
    Delete "$SMPROGRAMS\Asv Drones\Asv Drones.lnk"
    RMDir "$SMPROGRAMS\Asv Drones"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  SetAutoClose true
SectionEnd