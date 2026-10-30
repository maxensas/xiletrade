### What is Xiletrade.Updater?
The application that updates Xiletrade from the `Xiletrade_win-x64.7z` archive of the latest GitHub release, published as `Update.exe` with the WPF project.

It also contains the NSIS script of the installer (`Installer/Xiletrade.nsi`), used for the first installation.

## Installer
A web installer : `Xiletrade_win-x64_Setup.exe` does not embed the application, it downloads the `Xiletrade_win-x64.7z` archive of the latest release and installs it.

Its binary does not depend on the application version : the same file serves every release, and its reputation (SmartScreen, based on the hash of the file) keeps growing with the downloads.
A new release of Xiletrade does not require a new installer.

Publication (compatible with immutable releases) :
* `.github/workflows/installer.yml` (manual run) builds the setup and publishes it in an immutable pre-release `installer-<version>`. The version is computed : last `installer-x.y.z` tag + the increment chosen when running it (patch by default, 1.0.0 for the first one).
Run it once, then only when the installer script changes : a new build is a new file whose reputation starts over.
* `.github/workflows/release.yml` attaches the setup of the most recent installer pre-release, unchanged, to every release (digest checked). Same file, same hash : the reputation is kept.

Download link, always the setup of the latest release :
`https://github.com/maxensas/xiletrade/releases/latest/download/Xiletrade_win-x64_Setup.exe`

A pre-release is never returned by `releases/latest` : the application updater does not see it.

`7zr.exe` comes from the latest release of the official 7-Zip repository ([ip7z/7zip](https://github.com/ip7z/7zip)) when the installer is built, checked against the digest of that release. Its version and SHA256 are written in the notes of the installer pre-release.

### Behaviour
* No administrator rights, at any step : per-user installation in `%LOCALAPPDATA%\Programs\Xiletrade`.
* Requires Windows 10 1803 or later (`curl.exe` of Windows is used for the download) and an internet access.
* The archive is downloaded over HTTPS from GitHub, extracted in a temporary folder by 7-Zip (CRC of every file checked), then copied : the installed version is untouched if something fails.
* The installation folder is always named `Xiletrade`, as required by the updater.
* The uninstaller is kept in `%LOCALAPPDATA%\Xiletrade\Uninstall` : an update replaces every file of the installation folder.
* Running the setup on an existing installation replaces it with the latest release, as the updater does : `Data\Config.json` is kept, and saved as `Config.json.bak`.
* Uninstalling removes the whole installation folder, including the files added by later updates.
* Silent mode : `Xiletrade_win-x64_Setup.exe /S`, and `Uninstall.exe /S`.
* Xiletrade logo (`Assets/app-icon.ico`, `Installer/header.bmp`, `Installer/wizard.bmp`) and license of the WPF project (GPLv3).

### Build locally
From `Installer`, with `7zr.exe` taken from an official 7-Zip release :
```
makensis -DINSTALLER_VERSION=1.2.3 -DGITHUB_REPO=maxensas/xiletrade -DSEVENZIP=<path of 7zr.exe> -DOUTFILE=<setup.exe> Xiletrade.nsi
```

## Special thanks
The authors behind the tools used by the installer :
* [NSIS](https://nsis.sourceforge.io) (Nullsoft Scriptable Install System) by Nullsoft and the [NSIS contributors](https://github.com/kichik/nsis)
* [7-Zip](https://www.7-zip.org) by [Igor Pavlov](https://github.com/ip7z/7zip) : `7zr.exe` is embedded in the installer to extract the archive (GNU LGPL, see the [7-Zip license](https://www.7-zip.org/license.txt))
