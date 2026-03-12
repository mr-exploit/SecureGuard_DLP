; SecureGuard DLP - NSIS Installer Script
; Requires NSIS 3.x (https://nsis.sourceforge.io)

!define PRODUCT_NAME "SecureGuard Agent"
!define PRODUCT_VERSION "1.0.0"
!define PRODUCT_PUBLISHER "SecureGuard DLP"
!define PRODUCT_INSTALL_DIR "$PROGRAMFILES64\SecureGuard"
!define SERVICE_NAME "SecureGuardAgent"
!define SERVICE_DISPLAY "SecureGuard Agent"
!define SERVICE_DESC "SecureGuard Data Loss Prevention Agent"
!define PROXY_HOST "127.0.0.1"
!define PROXY_PORT "8877"

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "SecureGuard-Setup-${PRODUCT_VERSION}.exe"
InstallDir "${PRODUCT_INSTALL_DIR}"
RequestExecutionLevel admin

!include "MUI2.nsh"
!include "ServiceLib.nsh"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

Section "Main" SEC_MAIN
  SetOutPath "$INSTDIR"

  ; Copy agent files
  File /r "..\src\Agent\SecureGuard.Agent\bin\Release\net8.0-windows\publish\*.*"

  ; Install Root CA certificate
  ExecWait 'certutil -addstore "Root" "$INSTDIR\SecureGuardCA.crt"' $0
  DetailPrint "Root CA installation result: $0"

  ; Install Windows Service
  ExecWait '"$INSTDIR\SecureGuard.Agent.exe" install' $0
  DetailPrint "Service install result: $0"

  ; Configure system proxy via registry
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" \
    "ProxyServer" "${PROXY_HOST}:${PROXY_PORT}"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" \
    "ProxyEnable" 1
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" \
    "ProxyOverride" "localhost;127.0.0.1;<local>"
  DetailPrint "System proxy configured: ${PROXY_HOST}:${PROXY_PORT}"

  ; Start the service
  ExecWait 'net start "${SERVICE_NAME}"' $0
  DetailPrint "Service start result: $0"

  ; Write uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; Add to Programs and Features
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" \
    "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" \
    "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" \
    "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" \
    "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Section "Uninstall"
  ; Stop and remove service
  ExecWait 'net stop "${SERVICE_NAME}"' $0
  ExecWait '"$INSTDIR\SecureGuard.Agent.exe" uninstall' $0

  ; Remove proxy settings
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyEnable" 0
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyServer"

  ; Remove Root CA
  ExecWait 'certutil -delstore "Root" "SecureGuard Root CA"' $0

  ; Remove files
  RMDir /r "$INSTDIR"

  ; Remove registry entries
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
SectionEnd
