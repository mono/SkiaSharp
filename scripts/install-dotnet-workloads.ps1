Param(
  # Tizen version in "BAND/VERSION" format, e.g., "10.0.100/10.0.123"
  [string] $Tizen = '',
  # Override the default workloads (comma-separated, e.g. "android,maui-android")
  [string] $Workloads = ''
)

$ErrorActionPreference = 'Stop'

# Parse Tizen parameter (format: BAND/VERSION)
if ($Tizen -and $Tizen -ne '<latest>') {
  $parts = $Tizen -split '/'
  if ($parts.Length -ne 2) {
    Write-Host "##[error]Tizen parameter must be in BAND/VERSION format (e.g., 10.0.100/10.0.123)"
    exit 1
  }
  $TizenBand = $parts[0]
  $TizenVersion = $parts[1]
} else {
  $TizenBand = ''
  $TizenVersion = ''
}

# Use workload-set mode — global.json pins the workload version to ensure
# reproducible builds. The workloadVersion in global.json determines which
# manifest versions are used for all Microsoft workloads.
Write-Host "Configuring workload update mode to 'workload-set'..."
& dotnet workload config --update-mode workload-set
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Build workload list (Microsoft workloads only — Tizen is installed separately)
if ($Workloads) {
  $WorkloadList = $Workloads -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
  # Separate tizen from the list if present
  $HasTizen = $WorkloadList -contains 'tizen'
  $WorkloadList = $WorkloadList | Where-Object { $_ -ne 'tizen' }
} else {
  $HasTizen = [bool]$TizenBand
  $WorkloadList = @('android', 'macos', 'wasm-tools')
  if ($IsLinux) {
    $WorkloadList += @('maui-android')
  } else {
    $WorkloadList += @('ios', 'tvos', 'maccatalyst', 'maui')
  }
}

# Step 1: Install Microsoft workloads using the pinned workload set
if ($WorkloadList.Count -gt 0) {
  Write-Host "Installing Microsoft workloads: $($WorkloadList -join ', ')..."
  & dotnet workload install @WorkloadList --skip-sign-check
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

# Step 2: Install Tizen separately — Tizen is a third-party workload from
# Samsung that is not included in the official workload sets. We install
# the manifest manually and then install the workload after all Microsoft
# workloads are in place.
if ($HasTizen -and $TizenBand -and $TizenVersion) {
  Write-Host "Installing Tizen manifest ($TizenBand/$TizenVersion)..."

  # Get dotnet root (resolve symlinks on Linux/macOS)
  $dotnetPath = (Get-Command dotnet).Source
  if ($IsLinux -or $IsMacOS) {
    $dotnetRoot = & readlink -f $dotnetPath | Split-Path
  } else {
    $dotnetRoot = Split-Path $dotnetPath
  }

  $manifestDir = Join-Path $dotnetRoot "sdk-manifests" $TizenBand "samsung.net.sdk.tizen"
  $manifestName = "samsung.net.sdk.tizen.manifest-$TizenBand"
  $manifestUrl = "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/$($manifestName.ToLower())/$TizenVersion/$($manifestName.ToLower()).$TizenVersion.nupkg"

  Write-Host "  Downloading from $manifestUrl"
  New-Item -ItemType Directory -Force './output/tmp' | Out-Null
  Invoke-WebRequest $manifestUrl -OutFile './output/tmp/tizen-manifest.nupkg'

  Write-Host "  Extracting to $manifestDir"
  New-Item -ItemType Directory -Force $manifestDir | Out-Null
  Expand-Archive -Path './output/tmp/tizen-manifest.nupkg' -DestinationPath './output/tmp/tizen-manifest' -Force
  Copy-Item -Force './output/tmp/tizen-manifest/data/*' $manifestDir/

  Write-Host "Installing Tizen workload..."
  & dotnet workload install tizen --skip-sign-check --skip-manifest-update
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Installed workloads:"
& dotnet workload list
