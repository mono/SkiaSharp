Param(
    [string] $Version = "16/pre"
)

$ErrorActionPreference = 'Stop'

Write-Host "Downloading Visual Studio..."
Invoke-WebRequest -UseBasicParsing `
    -Uri "https://aka.ms/vs/$Version/vs_community.exe" `
    -OutFile vs_community.exe

Write-Host "Installing Visual Studio..."
./vs_community.exe --passive --norestart --wait `
  --add Microsoft.VisualStudio.Workload.NetCrossPlat `
  --add Microsoft.VisualStudio.Workload.NetCoreTools `
  --add Microsoft.VisualStudio.Workload.ManagedDesktop `
  --add Microsoft.VisualStudio.Workload.Universal `
  | Out-Null

Remove-Item vs_community.exe

Write-Host "Installed Visual Studio Versions:"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
& $vswhere -all -prerelease -property installationPath

exit $LASTEXITCODE
