Param(
    [string] $LinkId,
    [string] $Version = ''
)

$ErrorActionPreference = 'Stop'

$sdkDir = "${env:ProgramFiles(x86)}\Windows Kits\10\UnionMetadata"

Write-Host "Installed Windows SDKs:"
if (Test-Path (Join-Path "$sdkDir" "Windows.winmd")) {
    Write-Host "10.0.10240.0"
}
Get-ChildItem -Name "$sdkDir\10.0.*"

if ($Version -and ((Test-Path (Join-Path "$sdkDir" "$Version")) -or (Test-Path (Join-Path "$sdkDir" "Windows.winmd")))) {
    Write-Host "Windows SDK $Version already installed."
    exit 0
}

$uri = "https://go.microsoft.com/fwlink/p/?LinkId=$LinkId"

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
$tempDir = Join-Path "$HOME_DIR" "sdksetup-$LinkId-temp"
$installer = Join-Path "$tempDir" "sdksetup-$LinkId.exe"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

Write-Host "Downloading Windows SDK Installer: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile $installer

Write-Host "Installing Windows SDK..."
& $installer /norestart /quiet | Out-Null

Write-Host "Installed Windows SDKs:"
if (Test-Path (Join-Path "$sdkDir" "Windows.winmd")) {
    Write-Host "10.0.10240.0"
}
Get-ChildItem -Name "$sdkDir\10.0.*"

exit $LASTEXITCODE
