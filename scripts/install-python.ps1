Param(
    [string] $Version,
    [string] $Arch = 'x64'
)

$ErrorActionPreference = 'Stop'

$destDir = Join-Path "$env:AGENT_TOOLSDIRECTORY" "Python" "$Version" "$Arch"
$completeFile = "$destDir.complete"

if (Test-Path $completeFile) {
    Write-Host "Python $Version ($Arch) already installed."
    exit 0;
}

Write-Host "Downloading manifest..."

$uri = "https://raw.githubusercontent.com/actions/python-versions/main/versions-manifest.json"

# Write-Host "Downloading Windows SDK Installer: $uri..."
# .\scripts\download-file.ps1 -Uri $uri -OutFile sdksetup.exe

# Write-Host "Installing Windows SDK..."
# .\sdksetup.exe /norestart /quiet | Out-Null

# Write-Host "Installed Windows SDKs:"
# Get-ChildItem -Name "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"

exit $LASTEXITCODE
