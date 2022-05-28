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
  $previewRuntime = 'https://api.nuget.org/v3/index.json'
  $previewEmscripten = 'https://api.nuget.org/v3/index.json'
}

Write-Host "Installing workloads..."
& dotnet workload install `
  android ios tvos macos maccatalyst wasm-tools maui `
  --from-rollback-file $SourceUrl `
  --source https://api.nuget.org/v3/index.json `
  --source $previewFeed `
  --source $previewRuntime `
  --source $previewEmscripten `
  --skip-sign-check

exit $LASTEXITCODE
