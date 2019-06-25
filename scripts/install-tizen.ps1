Param(
    [string] $Version = "3.2"
)

$ErrorActionPreference = 'Stop'

if ($IsMacOS) {
    $platform = "macos-64"
    $ext = "bin"
} elseif ($IsLinux) {
    $platform = "ubuntu-64"
    $ext = "bin"
} else {
    $platform = "windows-64"
    $ext = "exe"
}

$ts = Join-Path "$HOME" "tizen-studio"
$tsTemp = Join-Path "$HOME" "tizen-temp"
$url = "http://download.tizen.org/sdk/Installer/tizen-studio_${Version}/web-cli_Tizen_Studio_${Version}_${platform}.${ext}"
$install = Join-Path "$tsTemp" "tizen-install.$ext"
$packages = "MOBILE-4.0,MOBILE-4.0-NativeAppDevelopment"

# download
Write-Host "Downloading SDK to '$install'..."
New-Item -ItemType Directory -Force -Path "$tsTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# install
Write-Host "Installing SDK to '$ts'..."
if ($IsMacOS -or $IsLinux) {
    & "bash" "$install" --accept-license --no-java-check "$ts"
} else {
    & "$install" --accept-license --no-java-check "$ts"
}

# install packages
Write-Host "Installing Additional Packages: '$packages'..."
$packMan = Join-Path (Join-Path "$ts" "package-manager") "package-manager-cli.${ext}"
if ($IsMacOS -or $IsLinux) {
    & "bash" "$packMan" install --no-java-check --accept-license "$packages"
} else {
    & "$packMan" install --no-java-check --accept-license "$packages"
}

# make sure that Tizen Studio is in TIZEN_STUDIO_HOME
Write-Host "##vso[task.setvariable variable=TIZEN_STUDIO_HOME;]$ts";

exit $LASTEXITCODE
