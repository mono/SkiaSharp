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
Invoke-WebRequest -Uri $SourceUrl -OutFile rollback.json
& dotnet workload update --from-rollback-file rollback.json --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install android-aot --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install ios --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install tvos --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install macos --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install maccatalyst --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install wasm-tools --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json
& dotnet workload install maui --skip-manifest-update --source https://aka.ms/dotnet6/nuget/index.json

exit $LASTEXITCODE
