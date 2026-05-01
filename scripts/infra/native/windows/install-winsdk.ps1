Param(
    [string] $LinkId,
    [string] $Version = ''
)

$ErrorActionPreference = 'Stop'

$sdkLibDir = "${env:ProgramFiles(x86)}\Windows Kits\10\Lib"
$sdkWinmdDir = "${env:ProgramFiles(x86)}\Windows Kits\10\UnionMetadata"

$installedLibs = @()
$installedWinmds = @()
$installedSdks = @()

$installedLibs += (Get-ChildItem -Name "$sdkLibDir")
if (Test-Path (Join-Path "$sdkWinmdDir" "Windows.winmd")) {
    $installedWinmds += @("10.0.10240.0")
}
$installedWinmds += (Get-ChildItem -Name "$sdkWinmdDir\10.0.*")
$installedSdks = $installedLibs | Where-Object { $installedWinmds -contains $_ }

if ($Version -and ($installedSdks -contains $Version)) {
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

exit $LASTEXITCODE
