$errorActionPreference = 'Stop'

$url = "http://download.tizen.org/sdk/Installer/tizen-studio_2.4/web-cli_Tizen_Studio_2.4_windows-64.exe"
$ts = "$env:USERPROFILE\tizen-studio"
$install = "$env:USERPROFILE\tizen-temp\tizen-install.exe"
$packages = "MOBILE-4.0,MOBILE-4.0-NativeAppDevelopment"

# download tizen
Write-Host "Downloading Tizen SDK..."
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\tizen-temp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# install tizen
& "$install" --accept-license --no-java-check "$ts"

# install packages
& "$ts\package-manager\package-manager-cli.exe" install --no-java-check --accept-license $packages

exit $LASTEXITCODE
