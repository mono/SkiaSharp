Param(
    [string] $LinkId
)

$ErrorActionPreference = 'Stop'

Write-Host "Installed Windows SDKs:"
Get-ChildItem -Name "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"

$uri = "https://go.microsoft.com/fwlink/p/?LinkId=$LinkId"

Write-Host "Downloading Windows SDK Installer: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile sdksetup.exe

Write-Host "Installing Windows SDK..."
.\sdksetup.exe /norestart /quiet | Out-Null

Write-Host "Installed Windows SDKs:"
Get-ChildItem -Name "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"

exit $LASTEXITCODE
