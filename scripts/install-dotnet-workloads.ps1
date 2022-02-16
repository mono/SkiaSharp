Param(
    [string] $InstallDir,
    [string] $SourceUrl
)

$ErrorActionPreference = 'Stop'

$env:DOTNET_ROOT="$InstallDir"

Write-Host "Installing workloads..."
& dotnet workload install `
  android ios tvos macos maccatalyst wasm-tools maui `
  --from-rollback-file $SourceUrl `
  --source https://api.nuget.org/v3/index.json `
  --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json

exit $LASTEXITCODE
