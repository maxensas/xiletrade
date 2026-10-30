; Xiletrade web installer (NSIS 3). Part of the Xiletrade.Updater project.
;
; The setup does not embed the application : it downloads the 7z archive of the latest GitHub release and installs it.
; Its binary does not depend on the application version : the same file serves every release, so its reputation
; (SmartScreen, based on the hash of the file) keeps growing. Rebuild it only when this script changes :
; a new build is a new file, its reputation starts over.
;
; Built by .github/workflows/installer.yml (manual run) :
;   makensis -DINSTALLER_VERSION=1.2.3 -DGITHUB_REPO=owner/repo -DSEVENZIP=<path of 7zr.exe> -DOUTFILE=<setup.exe> Xiletrade.nsi
; Published in an immutable pre-release (tag installer-<version>, version computed by the workflow),
; then attached unchanged to every release of the application by release.yml.
;
; No administrator rights, at any step : per-user installation in %LOCALAPPDATA%\Programs\Xiletrade.
; Requirements : Windows 10 1803 or later (curl.exe of Windows), internet access.
; Coexistence with the updater (Xiletrade.Updater) :
; - the installation folder must be named 'Xiletrade' (checked by the updater) : enforced.
; - an update replaces every file of the installation folder : the uninstaller is kept outside of it.

Unicode true
ManifestDPIAware true
RequestExecutionLevel user
SetCompressor /SOLID lzma

; Version of the installer itself, not of the application : computed by installer.yml (tag installer-<version>).
!ifndef INSTALLER_VERSION
  !define INSTALLER_VERSION "0.0.0"
!endif

!ifndef GITHUB_REPO
  !define GITHUB_REPO "maxensas/xiletrade"
!endif
!ifndef SEVENZIP
  !error "SEVENZIP is required : path of 7zr.exe (official 7-Zip console, 7z format only)"
!endif
!ifndef OUTFILE
  !define OUTFILE "Xiletrade_win-x64_Setup.exe"
!endif

!define APP_NAME "Xiletrade"
!define APP_FOLDER "Xiletrade"
!define APP_EXE "Xiletrade.exe"
!define APP_PUBLISHER "maxensas"
!define APP_URL "https://github.com/${GITHUB_REPO}"
!define ARCHIVE "Xiletrade_win-x64.7z"
!define ARCHIVE_URL "https://github.com/${GITHUB_REPO}/releases/latest/download/${ARCHIVE}"
!define APP_REGKEY "Software\Xiletrade"
!define UNINST_REGKEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\Xiletrade"
!define UNINST_DIR "$LOCALAPPDATA\Xiletrade\Uninstall"
!define UNINST_EXE "Uninstall.exe"
; Copy of the updater run during an update (see Xiletrade.Updater, Util/Relocation.cs).
!define UPDATER_DIR "$LOCALAPPDATA\Xiletrade\Updater"
!define CONFIG_DIR "Data"
!define CONFIG_FILE "Config.json"

Name "${APP_NAME}"
OutFile "${OUTFILE}"
InstallDir "$LOCALAPPDATA\Programs\${APP_FOLDER}"
InstallDirRegKey HKCU "${APP_REGKEY}" "InstallDir"
BrandingText "${APP_NAME}"

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

Var PreviousDir
Var Curl

;--------------------------------
; Interface

!define MUI_ABORTWARNING
; Xiletrade logo : setup and uninstaller icons, page header, welcome and finish pages
!define MUI_ICON "../Assets/app-icon.ico"
!define MUI_UNICON "../Assets/app-icon.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_RIGHT
!define MUI_HEADERIMAGE_BITMAP "header.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "wizard.bmp"

!insertmacro MUI_PAGE_WELCOME
; License of the WPF project (GPLv3)
!insertmacro MUI_PAGE_LICENSE "../../../licenses/LICENSE_Xiletrade"
!define MUI_PAGE_CUSTOMFUNCTION_PRE DirectoryPre
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE DirectoryLeave
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
; the 'show readme' checkbox is used for the desktop shortcut
!define MUI_FINISHPAGE_SHOWREADME ""
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_SHOWREADME_TEXT "$(MsgDesktopShortcut)"
!define MUI_FINISHPAGE_SHOWREADME_FUNCTION CreateDesktopShortcut
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
; Languages of the application : the one matching the Windows display language is used, English otherwise.

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "Spanish"
!insertmacro MUI_LANGUAGE "PortugueseBR"
!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_LANGUAGE "Korean"
!insertmacro MUI_LANGUAGE "Japanese"
!insertmacro MUI_LANGUAGE "Thai"
!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "TradChinese"

; English
LangString MsgFolderName ${LANG_ENGLISH} "The installation folder must be named '${APP_FOLDER}' so that the application can update itself. The installation path has been corrected. Please check it."
LangString MsgAppRunning ${LANG_ENGLISH} "${APP_NAME} is running. Close it, then click Retry."
LangString MsgDesktopShortcut ${LANG_ENGLISH} "Create a desktop shortcut"
LangString MsgCurlMissing ${LANG_ENGLISH} "This setup requires Windows 10 version 1803 or later."
LangString MsgDownloading ${LANG_ENGLISH} "Downloading the latest version of ${APP_NAME}, please wait..."
LangString MsgDownloadFailed ${LANG_ENGLISH} "Unable to download ${APP_NAME}. Check your internet connection, then run the setup again."
LangString MsgExtracting ${LANG_ENGLISH} "Extracting files..."
LangString MsgExtractFailed ${LANG_ENGLISH} "The downloaded archive is invalid. Run the setup again."
LangString MsgCopyFailed ${LANG_ENGLISH} "Unable to copy the files into the installation folder."

; French
LangString MsgFolderName ${LANG_FRENCH} "Le dossier d'installation doit s'appeler « ${APP_FOLDER} » pour que l'application puisse se mettre à jour. Le chemin a été corrigé. Veuillez le vérifier."
LangString MsgAppRunning ${LANG_FRENCH} "${APP_NAME} est en cours d'exécution. Fermez l'application, puis cliquez sur « Réessayer »."
LangString MsgDesktopShortcut ${LANG_FRENCH} "Créer un raccourci sur le bureau"
LangString MsgCurlMissing ${LANG_FRENCH} "Cette installation nécessite Windows 10, version 1803 ou ultérieure."
LangString MsgDownloading ${LANG_FRENCH} "Téléchargement de la dernière version de ${APP_NAME}. Veuillez patienter..."
LangString MsgDownloadFailed ${LANG_FRENCH} "Impossible de télécharger ${APP_NAME}. Vérifiez votre connexion Internet, puis relancez l'installation."
LangString MsgExtracting ${LANG_FRENCH} "Extraction des fichiers..."
LangString MsgExtractFailed ${LANG_FRENCH} "L'archive téléchargée est invalide. Veuillez relancer l'installation."
LangString MsgCopyFailed ${LANG_FRENCH} "Impossible de copier les fichiers dans le dossier d'installation."

; German
LangString MsgFolderName ${LANG_GERMAN} "Der Installationsordner muss „${APP_FOLDER}“ heißen, damit sich die Anwendung selbst aktualisieren kann. Der Installationspfad wurde korrigiert. Bitte überprüfen Sie ihn."
LangString MsgAppRunning ${LANG_GERMAN} "${APP_NAME} wird ausgeführt. Schließen Sie die Anwendung und klicken Sie anschließend auf „Wiederholen“."
LangString MsgDesktopShortcut ${LANG_GERMAN} "Verknüpfung auf dem Desktop erstellen"
LangString MsgCurlMissing ${LANG_GERMAN} "Für diese Installation ist Windows 10, Version 1803 oder höher, erforderlich."
LangString MsgDownloading ${LANG_GERMAN} "Die neueste Version von ${APP_NAME} wird heruntergeladen. Bitte warten..."
LangString MsgDownloadFailed ${LANG_GERMAN} "${APP_NAME} konnte nicht heruntergeladen werden. Überprüfen Sie Ihre Internetverbindung und starten Sie die Installation erneut."
LangString MsgExtracting ${LANG_GERMAN} "Dateien werden entpackt..."
LangString MsgExtractFailed ${LANG_GERMAN} "Das heruntergeladene Archiv ist ungültig. Starten Sie die Installation erneut."
LangString MsgCopyFailed ${LANG_GERMAN} "Die Dateien konnten nicht in den Installationsordner kopiert werden."

; Spanish
LangString MsgFolderName ${LANG_SPANISH} "La carpeta de instalación debe llamarse «${APP_FOLDER}» para que la aplicación pueda actualizarse. La ruta de instalación se ha corregido. Compruébela."
LangString MsgAppRunning ${LANG_SPANISH} "${APP_NAME} se está ejecutando. Cierre la aplicación y haga clic en «Reintentar»."
LangString MsgDesktopShortcut ${LANG_SPANISH} "Crear un acceso directo en el escritorio"
LangString MsgCurlMissing ${LANG_SPANISH} "Esta instalación requiere Windows 10, versión 1803 o posterior."
LangString MsgDownloading ${LANG_SPANISH} "Descargando la última versión de ${APP_NAME}. Espere, por favor..."
LangString MsgDownloadFailed ${LANG_SPANISH} "No se pudo descargar ${APP_NAME}. Compruebe su conexión a Internet y vuelva a ejecutar el instalador."
LangString MsgExtracting ${LANG_SPANISH} "Extrayendo archivos..."
LangString MsgExtractFailed ${LANG_SPANISH} "El archivo comprimido descargado no es válido. Vuelva a ejecutar el instalador."
LangString MsgCopyFailed ${LANG_SPANISH} "No se pudieron copiar los archivos en la carpeta de instalación."

; Portuguese (Brazil)
LangString MsgFolderName ${LANG_PORTUGUESEBR} "A pasta de instalação deve se chamar '${APP_FOLDER}' para que o aplicativo possa se atualizar. O caminho de instalação foi corrigido. Verifique-o."
LangString MsgAppRunning ${LANG_PORTUGUESEBR} "${APP_NAME} está em execução. Feche o aplicativo e clique em $\"Repetir$\"."
LangString MsgDesktopShortcut ${LANG_PORTUGUESEBR} "Criar um atalho na área de trabalho"
LangString MsgCurlMissing ${LANG_PORTUGUESEBR} "Esta instalação requer o Windows 10, versão 1803 ou posterior."
LangString MsgDownloading ${LANG_PORTUGUESEBR} "Baixando a versão mais recente do ${APP_NAME}. Aguarde..."
LangString MsgDownloadFailed ${LANG_PORTUGUESEBR} "Não foi possível baixar ${APP_NAME}. Verifique sua conexão com a internet e execute o instalador novamente."
LangString MsgExtracting ${LANG_PORTUGUESEBR} "Extraindo arquivos..."
LangString MsgExtractFailed ${LANG_PORTUGUESEBR} "O arquivo compactado baixado é inválido. Execute o instalador novamente."
LangString MsgCopyFailed ${LANG_PORTUGUESEBR} "Não foi possível copiar os arquivos para a pasta de instalação."

; Russian
LangString MsgFolderName ${LANG_RUSSIAN} "Папка установки должна называться «${APP_FOLDER}», чтобы приложение могло обновляться самостоятельно. Путь установки исправлен. Проверьте его."
LangString MsgAppRunning ${LANG_RUSSIAN} "${APP_NAME} запущено. Закройте приложение и нажмите «Повторить»."
LangString MsgDesktopShortcut ${LANG_RUSSIAN} "Создать ярлык на рабочем столе"
LangString MsgCurlMissing ${LANG_RUSSIAN} "Для установки требуется Windows 10 версии 1803 или новее."
LangString MsgDownloading ${LANG_RUSSIAN} "Загрузка последней версии ${APP_NAME}. Пожалуйста, подождите..."
LangString MsgDownloadFailed ${LANG_RUSSIAN} "Не удалось загрузить ${APP_NAME}. Проверьте подключение к интернету и запустите установщик снова."
LangString MsgExtracting ${LANG_RUSSIAN} "Извлечение файлов..."
LangString MsgExtractFailed ${LANG_RUSSIAN} "Загруженный архив повреждён. Запустите установщик снова."
LangString MsgCopyFailed ${LANG_RUSSIAN} "Не удалось скопировать файлы в папку установки."

; Korean
LangString MsgFolderName ${LANG_KOREAN} "애플리케이션이 자동으로 업데이트될 수 있도록 설치 폴더 이름은 '${APP_FOLDER}'이어야 합니다. 설치 경로가 수정되었으니 확인해 주세요."
LangString MsgAppRunning ${LANG_KOREAN} "${APP_NAME}이(가) 실행 중입니다. 애플리케이션을 종료한 후 [다시 시도]를 클릭하세요."
LangString MsgDesktopShortcut ${LANG_KOREAN} "바탕 화면에 바로 가기 만들기"
LangString MsgCurlMissing ${LANG_KOREAN} "이 설치 프로그램을 사용하려면 Windows 10 버전 1803 이상이 필요합니다."
LangString MsgDownloading ${LANG_KOREAN} "${APP_NAME}의 최신 버전을 다운로드하는 중입니다. 잠시 기다려 주세요..."
LangString MsgDownloadFailed ${LANG_KOREAN} "${APP_NAME}을(를) 다운로드할 수 없습니다. 인터넷 연결을 확인한 후 설치 프로그램을 다시 실행하세요."
LangString MsgExtracting ${LANG_KOREAN} "파일 압축을 푸는 중..."
LangString MsgExtractFailed ${LANG_KOREAN} "다운로드한 압축 파일이 유효하지 않습니다. 설치 프로그램을 다시 실행하세요."
LangString MsgCopyFailed ${LANG_KOREAN} "설치 폴더에 파일을 복사할 수 없습니다."

; Japanese
LangString MsgFolderName ${LANG_JAPANESE} "アプリケーションが自動更新できるように、インストール先フォルダーの名前は「${APP_FOLDER}」である必要があります。インストール先のパスを修正しましたので、ご確認ください。"
LangString MsgAppRunning ${LANG_JAPANESE} "${APP_NAME} が実行中です。アプリケーションを終了してから [再試行] をクリックしてください。"
LangString MsgDesktopShortcut ${LANG_JAPANESE} "デスクトップにショートカットを作成する"
LangString MsgCurlMissing ${LANG_JAPANESE} "このセットアップを実行するには、Windows 10 バージョン 1803 以降が必要です。"
LangString MsgDownloading ${LANG_JAPANESE} "${APP_NAME} の最新バージョンをダウンロードしています。しばらくお待ちください..."
LangString MsgDownloadFailed ${LANG_JAPANESE} "${APP_NAME} をダウンロードできませんでした。インターネット接続を確認してから、インストーラーを再度実行してください。"
LangString MsgExtracting ${LANG_JAPANESE} "ファイルを展開しています..."
LangString MsgExtractFailed ${LANG_JAPANESE} "ダウンロードしたアーカイブが無効です。インストーラーを再度実行してください。"
LangString MsgCopyFailed ${LANG_JAPANESE} "インストール先フォルダーにファイルをコピーできませんでした。"

; Thai
LangString MsgFolderName ${LANG_THAI} "โฟลเดอร์ติดตั้งต้องมีชื่อว่า '${APP_FOLDER}' เพื่อให้แอปพลิเคชันอัปเดตตัวเองได้ ระบบได้แก้ไขพาธการติดตั้งแล้ว โปรดตรวจสอบ"
LangString MsgAppRunning ${LANG_THAI} "${APP_NAME} กำลังทำงานอยู่ โปรดปิดแอปพลิเคชัน แล้วคลิก $\"ลองอีกครั้ง$\""
LangString MsgDesktopShortcut ${LANG_THAI} "สร้างทางลัดบนเดสก์ท็อป"
LangString MsgCurlMissing ${LANG_THAI} "การติดตั้งนี้ต้องใช้ Windows 10 เวอร์ชัน 1803 ขึ้นไป"
LangString MsgDownloading ${LANG_THAI} "กำลังดาวน์โหลด ${APP_NAME} เวอร์ชันล่าสุด โปรดรอสักครู่..."
LangString MsgDownloadFailed ${LANG_THAI} "ไม่สามารถดาวน์โหลด ${APP_NAME} ได้ โปรดตรวจสอบการเชื่อมต่ออินเทอร์เน็ต แล้วเรียกใช้โปรแกรมติดตั้งอีกครั้ง"
LangString MsgExtracting ${LANG_THAI} "กำลังแตกไฟล์..."
LangString MsgExtractFailed ${LANG_THAI} "ไฟล์บีบอัดที่ดาวน์โหลดไม่ถูกต้อง โปรดเรียกใช้โปรแกรมติดตั้งอีกครั้ง"
LangString MsgCopyFailed ${LANG_THAI} "ไม่สามารถคัดลอกไฟล์ไปยังโฟลเดอร์ติดตั้งได้"

; Simplified Chinese
LangString MsgFolderName ${LANG_SIMPCHINESE} "安装文件夹必须命名为“${APP_FOLDER}”，应用程序才能自动更新。安装路径已更正，请检查。"
LangString MsgAppRunning ${LANG_SIMPCHINESE} "${APP_NAME} 正在运行。请将其关闭，然后单击“重试”。"
LangString MsgDesktopShortcut ${LANG_SIMPCHINESE} "创建桌面快捷方式"
LangString MsgCurlMissing ${LANG_SIMPCHINESE} "此安装程序需要 Windows 10 版本 1803 或更高版本。"
LangString MsgDownloading ${LANG_SIMPCHINESE} "正在下载 ${APP_NAME} 的最新版本，请稍候..."
LangString MsgDownloadFailed ${LANG_SIMPCHINESE} "无法下载 ${APP_NAME}。请检查网络连接，然后重新运行安装程序。"
LangString MsgExtracting ${LANG_SIMPCHINESE} "正在解压文件..."
LangString MsgExtractFailed ${LANG_SIMPCHINESE} "下载的压缩包无效。请重新运行安装程序。"
LangString MsgCopyFailed ${LANG_SIMPCHINESE} "无法将文件复制到安装文件夹。"

; Traditional Chinese
LangString MsgFolderName ${LANG_TRADCHINESE} "安裝資料夾必須命名為「${APP_FOLDER}」，應用程式才能自動更新。安裝路徑已修正，請確認。"
LangString MsgAppRunning ${LANG_TRADCHINESE} "${APP_NAME} 正在執行。請將其關閉，然後按一下「重試」。"
LangString MsgDesktopShortcut ${LANG_TRADCHINESE} "建立桌面捷徑"
LangString MsgCurlMissing ${LANG_TRADCHINESE} "此安裝程式需要 Windows 10 版本 1803 或更新版本。"
LangString MsgDownloading ${LANG_TRADCHINESE} "正在下載 ${APP_NAME} 的最新版本，請稍候..."
LangString MsgDownloadFailed ${LANG_TRADCHINESE} "無法下載 ${APP_NAME}。請檢查網路連線，然後重新執行安裝程式。"
LangString MsgExtracting ${LANG_TRADCHINESE} "正在解壓縮檔案..."
LangString MsgExtractFailed ${LANG_TRADCHINESE} "下載的壓縮檔無效。請重新執行安裝程式。"
LangString MsgCopyFailed ${LANG_TRADCHINESE} "無法將檔案複製到安裝資料夾。"

;--------------------------------
; Version information of the setup file

VIProductVersion "${INSTALLER_VERSION}.0"
VIFileVersion "${INSTALLER_VERSION}.0"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${INSTALLER_VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${INSTALLER_VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${APP_NAME} Setup"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "${APP_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © ${APP_PUBLISHER}"

;--------------------------------
; Functions

Function .onInit
  ; curl.exe of Windows (since Windows 10 1803) : 64-bit one through Sysnative, the setup being a 32-bit process
  ${If} ${FileExists} "$WINDIR\Sysnative\curl.exe"
    StrCpy $Curl "$WINDIR\Sysnative\curl.exe"
  ${ElseIf} ${FileExists} "$SYSDIR\curl.exe"
    StrCpy $Curl "$SYSDIR\curl.exe"
  ${Else}
    MessageBox MB_OK|MB_ICONSTOP "$(MsgCurlMissing)" /SD IDOK
    Abort
  ${EndIf}
  ReadRegStr $PreviousDir HKCU "${APP_REGKEY}" "InstallDir"
FunctionEnd

; The updater refuses to work on a folder that is not named exactly 'Xiletrade' (case sensitive).
; $R0 = 1 when $INSTDIR has been corrected.
Function EnsureFolderName
  Push $0
  ${GetFileName} "$INSTDIR" $0
  ${If} $0 S== "${APP_FOLDER}"
    StrCpy $R0 0
  ${Else}
    StrCpy $INSTDIR "$INSTDIR\${APP_FOLDER}"
    StrCpy $R0 1
  ${EndIf}
  Pop $0
FunctionEnd

; Upgrade : the folder of the installed version is kept, the page is skipped.
Function DirectoryPre
  ${If} $PreviousDir != ""
    Abort
  ${EndIf}
FunctionEnd

Function DirectoryLeave
  Call EnsureFolderName
  ${If} $R0 = 1
    ; shows the corrected path in the folder field (control 1019) and stays on the page
    FindWindow $0 "#32770" "" $HWNDPARENT
    GetDlgItem $0 $0 1019
    SendMessage $0 ${WM_SETTEXT} 0 "STR:$INSTDIR"
    MessageBox MB_OK|MB_ICONINFORMATION "$(MsgFolderName)"
    Abort
  ${EndIf}
FunctionEnd

; A running executable cannot be opened for writing : waits for the application to be closed.
!macro ENSURE_APP_CLOSED UN
Function ${UN}EnsureAppClosed
  Exch $0 ; executable path
  Push $1
  ${DoWhile} ${FileExists} "$0"
    ClearErrors
    FileOpen $1 "$0" a
    ${IfNot} ${Errors}
      FileClose $1
      ${Break}
    ${EndIf}
    MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION "$(MsgAppRunning)" /SD IDCANCEL IDRETRY retry
    Abort
    retry:
  ${Loop}
  Pop $1
  Pop $0
FunctionEnd
!macroend
!insertmacro ENSURE_APP_CLOSED ""
!insertmacro ENSURE_APP_CLOSED "un."

Function CreateDesktopShortcut
  CreateShortcut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}"
FunctionEnd

;--------------------------------
; Installation

Section "Install"
  Call EnsureFolderName ; silent installation (/D=...) : corrected without message
  Push "$INSTDIR\${APP_EXE}"
  Call EnsureAppClosed

  InitPluginsDir
  SetOutPath "$PLUGINSDIR" ; working folder outside of the installation folder

  ; Download of the latest release (no 'Mark of the Web' : the extracted files are not checked by SmartScreen).
  DetailPrint "$(MsgDownloading)"
  nsExec::ExecToLog '"$Curl" -fsSL --retry 3 --retry-delay 2 -o "$PLUGINSDIR\${ARCHIVE}" "${ARCHIVE_URL}"'
  Pop $0
  ${If} $0 != 0
    DetailPrint "curl : $0"
    MessageBox MB_OK|MB_ICONSTOP "$(MsgDownloadFailed)" /SD IDOK
    Abort
  ${EndIf}

  ; Extraction into a temporary folder (7-Zip checks the CRC of every file) : nothing installed is touched on failure.
  DetailPrint "$(MsgExtracting)"
  File "/oname=$PLUGINSDIR\7zr.exe" "${SEVENZIP}"
  nsExec::ExecToLog '"$PLUGINSDIR\7zr.exe" x "$PLUGINSDIR\${ARCHIVE}" -o"$PLUGINSDIR\extract" -y -bso0 -bsp0'
  Pop $0
  ; the archive contains the 'Xiletrade' folder
  StrCpy $1 "$PLUGINSDIR\extract\${APP_FOLDER}"
  ${If} $0 != 0
  ${OrIfNot} ${FileExists} "$1\${APP_EXE}"
    DetailPrint "7-Zip : $0"
    MessageBox MB_OK|MB_ICONSTOP "$(MsgExtractFailed)" /SD IDOK
    Abort
  ${EndIf}
  Delete "$PLUGINSDIR\${ARCHIVE}"

  ; Same behaviour as the updater : the previous version is removed, the user configuration is kept.
  ${If} ${FileExists} "$INSTDIR\${CONFIG_DIR}\${CONFIG_FILE}"
    CopyFiles /SILENT "$INSTDIR\${CONFIG_DIR}\${CONFIG_FILE}" "$PLUGINSDIR"
  ${EndIf}
  ${If} $PreviousDir != ""
  ${AndIf} $PreviousDir == $INSTDIR
    RMDir /r "$INSTDIR"
  ${EndIf}

  CreateDirectory "$INSTDIR"
  ClearErrors
  CopyFiles /SILENT "$1\*.*" "$INSTDIR"
  ${If} ${Errors}
  ${OrIfNot} ${FileExists} "$INSTDIR\${APP_EXE}"
    MessageBox MB_OK|MB_ICONSTOP "$(MsgCopyFailed)" /SD IDOK
    Abort
  ${EndIf}

  ; the package only ships a Config.json when its format changed : the user one is restored otherwise, and always kept as .bak
  ${If} ${FileExists} "$PLUGINSDIR\${CONFIG_FILE}"
    CreateDirectory "$INSTDIR\${CONFIG_DIR}"
    ${IfNot} ${FileExists} "$INSTDIR\${CONFIG_DIR}\${CONFIG_FILE}"
      CopyFiles /SILENT "$PLUGINSDIR\${CONFIG_FILE}" "$INSTDIR\${CONFIG_DIR}\${CONFIG_FILE}"
    ${EndIf}
    CopyFiles /SILENT "$PLUGINSDIR\${CONFIG_FILE}" "$INSTDIR\${CONFIG_DIR}\${CONFIG_FILE}.bak"
  ${EndIf}

  ; the application works on its current folder : shortcuts and launch from the installation folder
  SetOutPath "$INSTDIR"
  CreateShortcut "$SMPROGRAMS\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}"

  CreateDirectory "${UNINST_DIR}"
  WriteUninstaller "${UNINST_DIR}\${UNINST_EXE}"

  ; version of the installed application, read from its executable
  ${GetFileVersion} "$INSTDIR\${APP_EXE}" $2
  WriteRegStr HKCU "${APP_REGKEY}" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "${UNINST_REGKEY}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKCU "${UNINST_REGKEY}" "DisplayVersion" "$2"
  WriteRegStr HKCU "${UNINST_REGKEY}" "DisplayIcon" "$INSTDIR\${APP_EXE}"
  WriteRegStr HKCU "${UNINST_REGKEY}" "Publisher" "${APP_PUBLISHER}"
  WriteRegStr HKCU "${UNINST_REGKEY}" "URLInfoAbout" "${APP_URL}"
  WriteRegStr HKCU "${UNINST_REGKEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${UNINST_REGKEY}" "UninstallString" '"${UNINST_DIR}\${UNINST_EXE}"'
  WriteRegStr HKCU "${UNINST_REGKEY}" "QuietUninstallString" '"${UNINST_DIR}\${UNINST_EXE}" /S'
  WriteRegDWORD HKCU "${UNINST_REGKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINST_REGKEY}" "NoRepair" 1
  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "${UNINST_REGKEY}" "EstimatedSize" "$0"
SectionEnd

;--------------------------------
; Uninstallation ($INSTDIR is the folder of the uninstaller)

Section "Uninstall"
  ReadRegStr $0 HKCU "${APP_REGKEY}" "InstallDir"
  ${GetFileName} "$0" $1
  ; safety : only a folder named 'Xiletrade' recorded by this installer is removed
  ${If} $0 != ""
  ${AndIf} $1 S== "${APP_FOLDER}"
    Push "$0\${APP_EXE}"
    Call un.EnsureAppClosed
    ; files added by later updates are unknown to the uninstaller : the whole folder is removed
    RMDir /r "$0"
  ${EndIf}

  Delete "$SMPROGRAMS\${APP_NAME}.lnk"
  Delete "$DESKTOP\${APP_NAME}.lnk"
  RMDir /r "${UPDATER_DIR}"

  DeleteRegKey HKCU "${UNINST_REGKEY}"
  DeleteRegKey HKCU "${APP_REGKEY}"

  Delete "$INSTDIR\${UNINST_EXE}"
  RMDir "$INSTDIR"
  RMDir "$LOCALAPPDATA\${APP_FOLDER}" ; only when empty
SectionEnd
