Param(
  [string] $SourceUrl,
  [string] $InstallDir,
  [string] $Tizen = '<latest>',
  [boolean] $IsPreview = $true
)

$ErrorActionPreference = 'Stop'

$feed1 = 'https://api.nuget.org/v3/index.json'
$feed2 = 'https://api.nuget.org/v3/index.json'
$feed3 = 'https://api.nuget.org/v3/index.json'
if ($IsPreview) {
  $feed1 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json'
  $feed2 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json'
  $feed3 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json'
}

Write-Host "Installing .NET workloads..."
& dotnet workload install `
  android ios tvos macos maccatalyst wasm-tools maui `
  --from-rollback-file $SourceUrl `
  --source https://api.nuget.org/v3/index.json `
  --source $feed1 `
  --source $feed2 `
  --source $feed3 `
  --skip-sign-check

Write-Host "Installing Tizen workloads..."
Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile 'workload-install.ps1'
./workload-install.ps1 -Version "$Tizen"

exit $LASTEXITCODE
