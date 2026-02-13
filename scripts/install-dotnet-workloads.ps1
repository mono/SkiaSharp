Param(
  [string] $Tizen = ''
)

$ErrorActionPreference = 'Stop'

if (!$Tizen) {
  $Tizen = '<latest>'
}

# Force manifest update mode — .NET 10 defaults to workload-set mode which
# auto-downloads a workload set from NuGet and ignores individually installed
# workloads (like Samsung Tizen). Manifest mode lets all workloads coexist.
Write-Host "Configuring workload update mode to 'manifests'..."
& dotnet workload config --update-mode manifests
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Install Tizen BEFORE official workloads — Samsung's script uses
# 'dotnet workload install tizen --skip-manifest-update' internally,
# which must run before official workloads change the workload state.
Write-Host "Installing Tizen workloads..."

# Download and extract the Tizen manifest directly instead of using Samsung's script.
# Samsung's script hardcodes nuget.org URLs which are blocked in CI.
$dotnetPath = (Get-Command dotnet).Source
# Resolve symlinks to get the actual dotnet root
if ($IsLinux -or $IsMacOS) {
  $dotnetRoot = & readlink -f $dotnetPath | Split-Path
} else {
  $dotnetRoot = Split-Path $dotnetPath
}
$sdkVersion = & dotnet --version
$versionBand = "10.0.100"
$manifestDir = Join-Path $dotnetRoot "sdk-manifests" $versionBand "samsung.net.sdk.tizen"
$manifestName = "samsung.net.sdk.tizen.manifest-$versionBand"
$manifestUrl = "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/$($manifestName.ToLower())/$Tizen/$($manifestName.ToLower()).$Tizen.nupkg"

Write-Host "Downloading Tizen manifest from $manifestUrl..."
New-Item -ItemType Directory -Force './output/tmp' | Out-Null
$nupkgPath = "./output/tmp/tizen-manifest.nupkg"
Invoke-WebRequest $manifestUrl -OutFile $nupkgPath

Write-Host "Extracting manifest to $manifestDir..."
New-Item -ItemType Directory -Force $manifestDir | Out-Null
if ($IsLinux -or $IsMacOS) {
  unzip -o -qq $nupkgPath -d ./output/tmp/tizen-manifest
  Copy-Item -Force ./output/tmp/tizen-manifest/data/* $manifestDir/
} else {
  Expand-Archive -Path $nupkgPath -DestinationPath ./output/tmp/tizen-manifest -Force
  Copy-Item -Force ./output/tmp/tizen-manifest/data/* $manifestDir/
}

Write-Host "Installing Tizen workload..."
& dotnet workload install tizen --skip-manifest-update --skip-sign-check
if ($LASTEXITCODE -ne 0) {
  Write-Host "##[error]Tizen workload installation failed with exit code $LASTEXITCODE"
  exit $LASTEXITCODE
}
Write-Host "Done installing Tizen workload $Tizen"

# Diagnostic: show manifest directory for debugging
$dotnetRoot = (Get-Command dotnet).Source | Split-Path
$manifestDir = Join-Path $dotnetRoot "sdk-manifests" "10.0.100"
Write-Host "Manifest directory contents:"
if (Test-Path $manifestDir) {
  Get-ChildItem $manifestDir -Recurse -Filter "WorkloadManifest.json" | ForEach-Object { Write-Host "  $($_.FullName)" }
}

# Show installed workloads
Write-Host "Installed workloads:"
& dotnet workload list

# Install official .NET workloads needed by the repo.
$Workloads = @('android', 'macos', 'wasm-tools')
if (!$IsLinux) {
  $Workloads += @('ios', 'tvos', 'maccatalyst', 'maui')
}

Write-Host "Installing .NET workloads: $($Workloads -join ', ')..."

& dotnet workload install `
  @Workloads `
  --skip-sign-check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Verify Tizen is still visible after official workload install
Write-Host "Workloads after install:"
& dotnet workload list
