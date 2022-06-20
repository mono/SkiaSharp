Param(
    [string] $Version,
    [string] $TestPath = "2022\Preview",
    [boolean] $RemoveExisting = $false
)

$ErrorActionPreference = 'Stop'

$fullPath = "C:\Program Files\Microsoft Visual Studio\$TestPath\Common7\IDE\devenv.exe"
if (Test-Path $fullPath) {
  Write-Host "Visual Studio $($TestPath.Replace('\',' ')) ($Version) already installed."
  exit 0
}

$temp = "$env:TEMP"
if ("$env:AGENT_TEMPDIRECTORY") {
  $temp = "$env:AGENT_TEMPDIRECTORY"
}

$startTime = Get-Date

if ($RemoveExisting) {
  Write-Host "Removing previous Visual Studio..."
  & 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\InstallCleanup.exe' -f
}

Write-Host "Downloading Visual Studio Installer..."
Invoke-WebRequest -UseBasicParsing `
  -Uri "https://aka.ms/vs/install/latest/vs_setup.exe" `
  -OutFile "$temp\dd_vs_setup.exe"

Write-Host "Updating the Visual Studio Installer..."
$exitCode = & "$temp\dd_vs_setup.exe" --update --quiet --wait | Out-Null

Write-Host "Exit code: $exitCode"

Write-Host "Downloading Visual Studio ($Version)..."
Invoke-WebRequest -UseBasicParsing `
  -Uri "https://aka.ms/vs/$Version/vs_community.exe" `
  -OutFile "$temp\dd_vs_community.exe"

Write-Host "Installing Visual Studio..."
$exitCode = & "$temp\dd_vs_community.exe" --quiet --norestart --wait `
  --includeRecommended `
  --add Microsoft.VisualStudio.Workload.NetCrossPlat `
  --add Microsoft.VisualStudio.Workload.NetCoreTools `
  --add Microsoft.VisualStudio.Workload.ManagedDesktop `
  --add Microsoft.VisualStudio.Workload.Universal `
  | Out-Null

Write-Host "Exit code: $exitCode"

$vsLogs = 'output\logs\vs-logs'
New-Item -ItemType Directory -Force -Path "$vsLogs" | Out-Null
Get-ChildItem "$temp\dd_*" |
  Where-Object { $_.CreationTime -gt $startTime } |
  Copy-Item -Destination "$vsLogs"

exit $LASTEXITCODE
