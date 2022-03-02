Param(
    [string] $Version = 'latest'
)

$ErrorActionPreference = 'Stop'

try {
    nuget
    Write-Host "NuGet already installed."
    exit 0
} catch {
    # no op
}

$uri = "https://dist.nuget.org/win-x86-commandline/$Version/nuget.exe"

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
$destDir = Join-Path "$HOME_DIR" "nuget"
$exe = Join-Path "$destDir" "nuget.exe"
New-Item -ItemType Directory -Force -Path $destDir | Out-Null

Write-Host "Downloading NuGet: $uri..."
.\scripts\download-file.ps1 -Uri $uri -OutFile $exe

Write-Host "##vso[task.setvariable variable=PATH;]$destDir;$env:PATH";
$env:PATH = "$destDir;$env:PATH"

exit $LASTEXITCODE
