Param(
    [string] $Version = "16/pre"
)

$ErrorActionPreference = 'Stop'

$startTime = Get-Date

Write-Host "Downloading Visual Studio Installer..."
Invoke-WebRequest -UseBasicParsing `
    -Uri "https://aka.ms/vs/install/latest/vs_setup.exe" `
    -OutFile "$env:TEMP\dd_vs_setup.exe"

Write-Host "Updating the Visual Studio Installer..."
$exitCode = & "$env:TEMP\dd_vs_setup.exe" --update --quiet --wait | Out-Null

Write-Host "Exit code: $exitCode"

Write-Host "Downloading Visual Studio ($Version)..."
Invoke-WebRequest -UseBasicParsing `
    -Uri "https://aka.ms/vs/$Version/vs_community.exe" `
    -OutFile "$env:TEMP\dd_vs_community.exe"

Write-Host "Installing Visual Studio..."
$exitCode = & "$env:TEMP\dd_vs_community.exe" --quiet --norestart --wait `
  --add Microsoft.VisualStudio.Workload.NetCrossPlat `
  --add Microsoft.VisualStudio.Workload.NetCoreTools `
  --add Microsoft.VisualStudio.Workload.ManagedDesktop `
  --add Microsoft.VisualStudio.Workload.Universal `
  | Out-Null

Write-Host "Exit code: $exitCode"

$vsLogs = 'output\vs-logs'
New-Item -ItemType Directory -Force -Path "$vsLogs" | Out-Null
Get-ChildItem "$env:TEMP\dd_*" |
  Where-Object { $_.CreationTime -gt $startTime } |
  Copy-Item -Destination "$vsLogs"

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

Write-Host "Setting Environment Variables..."
$installationPath = & $vswhere -latest -prerelease -property installationPath
Write-Host "##vso[task.prependpath]$installationPath\MSBuild\Current\Bin"
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$installationPath"

Write-Host "Installed Visual Studio Versions:"
& $vswhere -all -prerelease -property installationPath

exit $LASTEXITCODE
