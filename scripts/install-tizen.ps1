Param(
    [string] $version = "2.4"
)

$errorActionPreference = 'Stop'

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

$ts = "$HOME/tizen-studio"
$tsTemp = "$HOME/tizen-temp"
$url = "http://download.tizen.org/sdk/Installer/tizen-studio_${version}/web-cli_Tizen_Studio_${version}_${platform}.${ext}"
$install = "$tsTemp/tizen-install.$ext"
$packages = "MOBILE-4.0,MOBILE-4.0-NativeAppDevelopment"

# download
Write-Host "Downloading SDK..."
New-Item -ItemType Directory -Force -Path "$tsTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# install
Write-Host "Installing SDK..."
if ($IsMacOS -or $IsLinux) {
    & "bash" "$install" --accept-license --no-java-check "$ts"
} else {
    & "$install" --accept-license --no-java-check "$ts"
}

# install packages
Write-Host "Installing Additional Packages..."
if ($IsMacOS -or $IsLinux) {
    & "bash" "${ts}/package-manager/package-manager-cli.${ext}" install --no-java-check --accept-license "$packages"
} else {
    & "${ts}/package-manager/package-manager-cli.${ext}" install --no-java-check --accept-license "$packages"
}

ls -l $ts

exit $LASTEXITCODE
