Param(
    [string] $InstallDir,
    [string] $SourceUrl
)

$ErrorActionPreference = 'Stop'

$env:DOTNET_ROOT="$InstallDir"

Write-Host "Checking current workloads..."
& dotnet workload list
& dotnet workload search

Write-Host "Installing workloads..."
& dotnet workload install `
  android ios tvos macos maccatalyst wasm-tools maui `
  --from-rollback-file $SourceUrl `
  --source https://aka.ms/dotnet6/nuget/index.json `
  --source https://api.nuget.org/v3/index.json

exit $LASTEXITCODE
