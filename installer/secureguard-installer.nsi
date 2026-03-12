; SecureGuard DLP Agent Installer
; NSIS (Nullsoft Scriptable Install System) Script
; Requirements: NSIS 3.x with AccessControl plugin

!include "MUI2.nsh"
!include "ServiceLib.nsh"
!include "LogicLib.nsh"
!include "nsDialogs.nsh"

;--------------------------------
; General Attributes

Name "SecureGuard DLP Agent"
OutFile "SecureGuardSetup.exe"
InstallDir "$PROGRAMFILES64\SecureGuard"
InstallDirRegKey HKLM "Software\SecureGuard" "InstallDir"
RequestExecutionLevel admin
Unicode True

;--------------------------------
; Version Information

VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "SecureGuard DLP Agent"
VIAddVersionKey "CompanyName" "SecureGuard Security"
VIAddVersionKey "FileDescription" "SecureGuard DLP Agent Installer"
VIAddVersionKey "FileVersion" "1.0.0.0"
VIAddVersionKey "ProductVersion" "1.0.0.0"
VIAddVersionKey "LegalCopyright" "Copyright 2024 SecureGuard Security"

;--------------------------------
; Interface Configuration

!define MUI_ABORTWARNING
!define MUI_ICON "assets\secureguard.ico"
!define MUI_UNICON "assets\secureguard.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP "assets\wizard_banner.bmp"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "assets\header.bmp"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_DIRECTORY
Page custom ServerConfigPage ServerConfigPageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
; Language

!insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Variables

Var ServerUrl
Var ProxyPort
Var AgentId
Var Dialog
Var ServerUrlInput
Var ProxyPortInput
Var AgentIdInput

;--------------------------------
; Server Configuration Page

Function ServerConfigPage
  nsDialogs::Create 1018
  Pop $Dialog

  ${If} $Dialog == error
    Abort
  ${EndIf}

  ${NSD_CreateLabel} 0 0 100% 20u "SecureGuard Server Configuration"
  ${NSD_CreateLabel} 0 30u 100% 12u "Server URL:"
  ${NSD_CreateText} 0 44u 100% 14u "http://your-server:5000"
  Pop $ServerUrlInput

  ${NSD_CreateLabel} 0 70u 100% 12u "Proxy Port:"
  ${NSD_CreateText} 0 84u 100% 14u "8877"
  Pop $ProxyPortInput

  ${NSD_CreateLabel} 0 110u 100% 12u "Agent ID (unique per machine):"
  ${NSD_CreateText} 0 124u 100% 14u "AGENT-$COMPUTERNAME"
  Pop $AgentIdInput

  nsDialogs::Show
FunctionEnd

Function ServerConfigPageLeave
  ${NSD_GetText} $ServerUrlInput $ServerUrl
  ${NSD_GetText} $ProxyPortInput $ProxyPort
  ${NSD_GetText} $AgentIdInput $AgentId
FunctionEnd

;--------------------------------
; Installer Sections

Section "SecureGuard Agent" SecMain
  SectionIn RO

  ; Set output path to installation directory
  SetOutPath "$INSTDIR"

  ; Copy agent files
  File /r "publish\*.*"

  ; Create logs directory
  CreateDirectory "$INSTDIR\logs"

  ; Write configuration file
  FileOpen $0 "$INSTDIR\appsettings.json" w
  FileWrite $0 '{'
  FileWrite $0 '$\r$\n  "Logging": {'
  FileWrite $0 '$\r$\n    "LogLevel": {'
  FileWrite $0 '$\r$\n      "Default": "Information"'
  FileWrite $0 '$\r$\n    }'
  FileWrite $0 '$\r$\n  },'
  FileWrite $0 '$\r$\n  "Agent": {'
  FileWrite $0 '$\r$\n    "AgentId": "$AgentId",'
  FileWrite $0 '$\r$\n    "ServerUrl": "$ServerUrl",'
  FileWrite $0 '$\r$\n    "ProxyPort": $ProxyPort,'
  FileWrite $0 '$\r$\n    "ProxyEnabled": true,'
  FileWrite $0 '$\r$\n    "FileMonitorEnabled": true,'
  FileWrite $0 '$\r$\n    "ProcessMonitorEnabled": true,'
  FileWrite $0 '$\r$\n    "MonitoredPaths": ["C:\\\\Users", "C:\\\\Documents"],'
  FileWrite $0 '$\r$\n    "WhitelistedIps": ["127.0.0.1", "::1"]'
  FileWrite $0 '$\r$\n  }'
  FileWrite $0 '$\r$\n}'
  FileClose $0

  ; Install Root CA certificate
  DetailPrint "Installing Root CA certificate..."
  ExecWait '"$INSTDIR\SecureGuard.Agent.exe" --install-ca' $0
  ${If} $0 != 0
    DetailPrint "Root CA installation will be completed on first run"
  ${EndIf}

  ; Set system proxy
  DetailPrint "Configuring system proxy..."
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyServer" "127.0.0.1:$ProxyPort"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyEnable" 1
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyOverride" "localhost;127.0.0.1;<local>"
  ; Notify Windows of proxy change
  ExecWait 'rundll32.exe wininet.dll,InternetSetOptionExW'

  ; Install Windows Service
  DetailPrint "Installing Windows Service..."
  ExecWait '"$SYSDIR\sc.exe" create "SecureGuardAgent" binPath= "$\"$INSTDIR\SecureGuard.Agent.exe$\" --windows-service" DisplayName= "SecureGuard DLP Agent" start= auto obj= LocalSystem' $0
  ${If} $0 == 0
    DetailPrint "Service installed successfully"
  ${Else}
    DetailPrint "Service already exists, updating..."
    ExecWait '"$SYSDIR\sc.exe" config "SecureGuardAgent" binPath= "$\"$INSTDIR\SecureGuard.Agent.exe$\" --windows-service" start= auto'
  ${EndIf}

  ; Set service description
  ExecWait '"$SYSDIR\sc.exe" description "SecureGuardAgent" "SecureGuard Data Loss Prevention Agent"'

  ; Start the service
  DetailPrint "Starting SecureGuard Agent service..."
  ExecWait '"$SYSDIR\sc.exe" start "SecureGuardAgent"' $0
  ${If} $0 == 0
    DetailPrint "Service started successfully"
  ${Else}
    DetailPrint "Service start scheduled for next reboot"
  ${EndIf}

  ; Create registry keys for uninstaller
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "DisplayName" "SecureGuard DLP Agent"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "DisplayIcon" "$INSTDIR\SecureGuard.Agent.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "Publisher" "SecureGuard Security"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "DisplayVersion" "1.0.0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "InstallLocation" "$INSTDIR"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard" "NoRepair" 1
  WriteRegStr HKLM "Software\SecureGuard" "InstallDir" "$INSTDIR"

  ; Write uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; Create Start Menu shortcuts
  CreateDirectory "$SMPROGRAMS\SecureGuard"
  CreateShortcut "$SMPROGRAMS\SecureGuard\Uninstall SecureGuard.lnk" "$INSTDIR\uninstall.exe"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

  ; Stop and remove the service
  DetailPrint "Stopping SecureGuard Agent service..."
  ExecWait '"$SYSDIR\sc.exe" stop "SecureGuardAgent"'
  Sleep 2000
  ExecWait '"$SYSDIR\sc.exe" delete "SecureGuardAgent"'

  ; Remove Root CA certificate
  DetailPrint "Removing Root CA certificate..."
  ExecWait '"$INSTDIR\SecureGuard.Agent.exe" --uninstall-ca'

  ; Remove system proxy settings
  DetailPrint "Removing proxy configuration..."
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyEnable" 0
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyServer"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Internet Settings" "ProxyOverride"

  ; Remove files
  RMDir /r "$INSTDIR"

  ; Remove Start Menu shortcuts
  Delete "$SMPROGRAMS\SecureGuard\Uninstall SecureGuard.lnk"
  RMDir "$SMPROGRAMS\SecureGuard"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SecureGuard"
  DeleteRegKey HKLM "Software\SecureGuard"

  DetailPrint "SecureGuard DLP Agent uninstalled successfully"

SectionEnd
