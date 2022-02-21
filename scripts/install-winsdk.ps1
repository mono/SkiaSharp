Param(
    [string] $LinkId,
    [string] $Version = ''
)

$ErrorActionPreference = 'Stop'

$sdkDir = "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"

Write-Host "Installed Windows SDKs:"
Get-ChildItem -Name "$sdkDir"

if ($Version -and (Test-Path (Join-Path "$sdkDir" "$Version"))) {
    Write-Host "Windows SDK $Version already installed."
    exit 0
}

$uri = "https://go.microsoft.com/fwlink/p/?LinkId=$LinkId"

Write-Host "Downloading Windows SDK Installer: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile sdksetup.exe

Write-Host "Installing Windows SDK..."
.\sdksetup.exe /norestart /quiet | Out-Null

Write-Host "Installed Windows SDKs:"
Get-ChildItem -Name "$sdkDir"

exit $LASTEXITCODE
