Param(
  [string] $SourceUrl,
  [string] $InstallDir,
  [string] $Tizen = '<latest>',
  [boolean] $IsPreview = $false
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

$Workloads = 'android','ios','tvos','macos','maccatalyst','wasm-tools','wasm-tools-net6','maui'
if ($IsLinux) {
  $Workloads = 'android','macos','wasm-tools','wasm-tools-net6'
}

Write-Host "Installing .NET workloads..."
& dotnet workload install `
  @Workloads `
  --from-rollback-file $SourceUrl `
  --source https://api.nuget.org/v3/index.json `
  --source $feed1 `
  --source $feed2 `
  --source $feed3 `
  --skip-sign-check

Write-Host "Installing Tizen workloads..."
if ($IsLinux -or $IsMacOS) {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.sh' -OutFile 'workload-install.sh'
  bash workload-install.sh --version "$Tizen"
} else {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile 'workload-install.ps1'
  ./workload-install.ps1 -Version "$Tizen"
}

exit $LASTEXITCODE
