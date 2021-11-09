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
  --source https://aka.ms/dotnet6/nuget/index.json `
  --source https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-aspnetcore-7c57ecbd-3/nuget/v3/index.json `
  --source https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-runtime-4822e3c3-5/nuget/v3/index.json `
  --source https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-windowsdesktop-59fea7da-4/nuget/v3/index.json `
  --source https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-emsdk-1ec2e17f-4/nuget/v3/index.json

exit $LASTEXITCODE
