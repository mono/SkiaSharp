Param(
    [string] $Version = "2.12.45"
)

$ErrorActionPreference = 'Stop'

$uri = "https://xamarin.azureedge.net/GTKforWindows/Windows/gtk-sharp-$Version.msi"

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
$tempDir = Join-Path "$HOME_DIR" "gtk-sharp-temp"
$installer = Join-Path "$tempDir" "gtk-sharp.msi"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

Write-Host "Downloading GTK# Installer: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile $installer

$p = "$env:BUILD_SOURCESDIRECTORY\output\logs\install-logs"
New-Item -ItemType Directory -Force -Path $p | Out-Null

msiexec /i $installer /norestart /quiet /l* $p\gtk-sharp-install.log

exit $LASTEXITCODE
