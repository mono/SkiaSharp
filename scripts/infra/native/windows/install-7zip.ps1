Param(
    [string] $Version = '26.02',
    [string] $Sha256 = 'db407a4f6d4999e5c7bc00ce8a882be94717b56e7fa68140fe3f12605d91643e'
)

$ErrorActionPreference = 'Stop'

$sevenZipPath = (Get-Command 7z -ErrorAction SilentlyContinue).Source
if (-not $sevenZipPath) {
    $installedPath = Join-Path $env:ProgramFiles '7-Zip\7z.exe'
    if (Test-Path $installedPath) {
        $sevenZipPath = $installedPath
    }
}

if ($sevenZipPath) {
    & $sevenZipPath --help
    Write-Host "7-zip already installed."
    $sevenZipDirectory = Split-Path $sevenZipPath
    Write-Host "##vso[task.setvariable variable=PATH;]$sevenZipDirectory;$env:PATH"
    exit 0
}

$uri = "https://github.com/ip7z/7zip/releases/download/$Version/7z$($Version.Replace('.', ''))-x64.msi"

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
$tempDir = Join-Path "$HOME_DIR" "7zip-temp"
$installer = Join-Path "$tempDir" "7zip.msi"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

Write-Host "Downloading 7-zip Installer: $uri..."
.\scripts\infra\native\shared\download-file.ps1 -Uri $uri -OutFile $installer

$actualSha256 = (Get-FileHash -Algorithm SHA256 -Path $installer).Hash.ToLowerInvariant()
if ($actualSha256 -ne $Sha256.ToLowerInvariant()) {
    throw "7-Zip $Version SHA-256 mismatch: expected $Sha256, got $actualSha256"
}

$p = "$env:BUILD_SOURCESDIRECTORY\output\logs\install-logs"
New-Item -ItemType Directory -Force -Path $p | Out-Null

msiexec /i $installer /norestart /quiet /l* $p\7zip-install.log

Write-Host "##vso[task.setvariable variable=PATH;]C:\Program Files\7-Zip;$env:PATH";

exit $LASTEXITCODE
