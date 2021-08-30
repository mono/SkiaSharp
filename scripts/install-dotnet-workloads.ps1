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
& dotnet workload install android-aot --source "$SourceUrl"
& dotnet workload install ios --source "$SourceUrl"
& dotnet workload install tvos --source "$SourceUrl"
& dotnet workload install macos --source "$SourceUrl"
& dotnet workload install maccatalyst --source "$SourceUrl"
& dotnet workload install wasm-tools --source "$SourceUrl"
& dotnet workload install maui --source "$SourceUrl"

exit $LASTEXITCODE
