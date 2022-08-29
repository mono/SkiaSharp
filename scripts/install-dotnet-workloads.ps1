Param(
  [string] $SourceUrl,
  [string] $InstallDir,
  [boolean] $IsPreview = $true
)

$ErrorActionPreference = 'Stop'

$previewFeed = 'https://api.nuget.org/v3/index.json'
$previewRuntime = 'https://api.nuget.org/v3/index.json'
$previewEmscripten = 'https://api.nuget.org/v3/index.json'
if ($IsPreview) {
  $previewFeed = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json'
  $previewRuntime = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-runtime-531f715f/nuget/v3/index.json'
  $previewEmscripten = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-emsdk-3f6c45a2/nuget/v3/index.json'
}

Write-Host "Installing .NET workloads..."
& dotnet workload install `
  android ios tvos macos maccatalyst wasm-tools maui `
  --from-rollback-file $SourceUrl `
  --source https://api.nuget.org/v3/index.json `
  --source $previewFeed `
  --source $previewRuntime `
  --source $previewEmscripten `
  --skip-sign-check

Write-Host "Installing Tizen workloads..."
Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile 'workload-install.ps1'
./workload-install.ps1

exit $LASTEXITCODE
