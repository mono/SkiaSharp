Param(
  # Pin to a specific workload set version (e.g. "10.0.104")
  [Parameter(Mandatory=$true)]
  [string] $WorkloadSetVersion,
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

# Install Tizen manifest if specified — Tizen is a third-party workload from
# Samsung that is not included in any official workload set, so we install its
# manifest manually before installing workloads.
if ($TizenBand -and $TizenVersion) {
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
}

# Build workload list
if ($Workloads) {
  $WorkloadList = $Workloads -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
} else {
  $WorkloadList = @('android', 'macos', 'wasm-tools')
  if ($IsLinux) {
    $WorkloadList += @('maui-android')
  } else {
    $WorkloadList += @('ios', 'tvos', 'maccatalyst', 'maui')
  }
}

# Install official workloads pinned to the workload set version
Write-Host "Installing workloads: $($WorkloadList -join ', ') (workload set $WorkloadSetVersion)..."
& dotnet workload install @WorkloadList --skip-sign-check --version $WorkloadSetVersion
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Install Tizen separately — it's a third-party workload not part of the
# official workload set, so it can't use --version.
if ($TizenBand) {
  Write-Host "Installing Tizen workload (third-party, no version pin)..."
  & dotnet workload install tizen --skip-sign-check
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Installed workloads:"
& dotnet workload list
