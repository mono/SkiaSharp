Param(
    [string] $Version = "17/pre"
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

$vsLogs = 'output\logs\vs-logs'
New-Item -ItemType Directory -Force -Path "$vsLogs" | Out-Null
Get-ChildItem "$env:TEMP\dd_*" |
  Where-Object { $_.CreationTime -gt $startTime } |
  Copy-Item -Destination "$vsLogs"

dir 'C:\Program Files\Microsoft Visual Studio\2022\'
dir 'C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\'
dir 'C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Xamarin\'
dir 'C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Xamarin\Android\'

exit $LASTEXITCODE
