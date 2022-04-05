Param(
    [string] $Version = '21.07'
)

$ErrorActionPreference = 'Stop'

try {
    7z --help
    Write-Host "7-zip already installed."
    exit 0
} catch {
    # no op
}

$uri = "https://www.7-zip.org/a/7z$($Version.Replace('.', ''))-x64.msi"

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
$tempDir = Join-Path "$HOME_DIR" "7zip-temp"
$installer = Join-Path "$tempDir" "7zip.msi"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

Write-Host "Downloading 7-zip Installer: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile $installer

$p = "$env:BUILD_SOURCESDIRECTORY\output\logs\install-logs"
New-Item -ItemType Directory -Force -Path $p | Out-Null

msiexec /i $installer /norestart /quiet /l* $p\7zip-install.log

Write-Host "##vso[task.setvariable variable=PATH;]C:\Program Files\7-Zip;$env:PATH";

exit $LASTEXITCODE
