Param(
  # Pin to a specific workload set version (e.g. "10.0.104")
  [Parameter(Mandatory=$true)]
  [string] $WorkloadSetVersion,
  # Tizen version in "BAND/VERSION" format, e.g., "10.0.100/10.0.123"
  [string] $Tizen = '',
  # Override the default workloads (comma-separated, e.g. "android,maui-android")
  [string] $Workloads = '',
  # Select an installed SDK without changing the repository global.json.
  [string] $SdkVersion = ''
)

$ErrorActionPreference = 'Stop'

$sdkDirectory = $null
try {
  if ($SdkVersion) {
    $sdkDirectory = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-workloads-$([guid]::NewGuid())"
    New-Item -ItemType Directory -Path $sdkDirectory | Out-Null
    @{ sdk = @{ version = $SdkVersion; rollForward = 'disable'; allowPrerelease = $true } } |
      ConvertTo-Json | Set-Content -LiteralPath (Join-Path $sdkDirectory 'global.json')
    $nugetConfigPath = Join-Path $sdkDirectory 'nuget.config'
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot '../../../nuget.config') -Destination $nugetConfigPath
    [xml] $nugetConfig = Get-Content -LiteralPath $nugetConfigPath -Raw
    if (-not ($nugetConfig.configuration.packageSources.add | Where-Object { $_.key -eq 'nuget.org' })) {
      $source = $nugetConfig.CreateElement('add')
      $source.SetAttribute('key', 'nuget.org')
      $source.SetAttribute('value', 'https://api.nuget.org/v3/index.json')
      $nugetConfig.configuration.packageSources.AppendChild($source) | Out-Null
      $nugetConfig.Save($nugetConfigPath)
    }
    Push-Location $sdkDirectory
  }
  if ($SdkVersion) {
    $actualSdk = & dotnet --version
    if ($LASTEXITCODE -ne 0 -or $actualSdk.Trim() -ne $SdkVersion) {
      throw "Expected SDK $SdkVersion, selected $actualSdk"
    }
  }

# Parse Tizen parameter (format: BAND/VERSION)
if ($Tizen -and $Tizen -ne '<latest>') {
  $parts = $Tizen -split '/'
  if ($parts.Length -ne 2) {
    throw "Tizen parameter must be in BAND/VERSION format (e.g., 10.0.100/10.0.123)"
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
  if ($SdkVersion) {
    $sdkParts = ($SdkVersion -split '-')[0] -split '\.'
    $featureBand = "$($sdkParts[0]).$($sdkParts[1]).$([math]::Floor([int]$sdkParts[2] / 100) * 100)"
    if ($SdkVersion -match '-((?:preview|rc)\.\d+)') { $featureBand += "-$($Matches[1])" }
    $selectedManifestDir = Join-Path $dotnetRoot "sdk-manifests/$featureBand/samsung.net.sdk.tizen"
    if ($selectedManifestDir -ne $manifestDir) {
      New-Item -ItemType Directory -Force $selectedManifestDir | Out-Null
      Copy-Item -Force './output/tmp/tizen-manifest/data/*' $selectedManifestDir/
    }
  }
}

# Build workload list
if ($Workloads) {
  $WorkloadList = $Workloads -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
} else {
  $WorkloadList = @('android', 'macos', 'wasm-tools')
  if ($SdkVersion -like '11.*') { $WorkloadList += 'wasm-tools-net10' }
  if ($IsLinux) {
    $WorkloadList += @('maui-android')
  } else {
    $WorkloadList += @('ios', 'tvos', 'maccatalyst', 'maui')
  }
}

# Install official workloads pinned to the workload set version
Write-Host "Installing workloads: $($WorkloadList -join ', ') (workload set $WorkloadSetVersion)..."
& dotnet workload install @WorkloadList --skip-sign-check --version $WorkloadSetVersion
if ($LASTEXITCODE -ne 0) { throw "Could not install workload set $WorkloadSetVersion (exit code $LASTEXITCODE)" }

# Install Tizen separately — it's a third-party workload not part of the
# official workload set, so it can't use --version.
if ($TizenBand) {
  Write-Host "Installing Tizen workload (third-party, no version pin)..."
  & dotnet workload install tizen --skip-sign-check
  if ($LASTEXITCODE -ne 0) { throw "Could not install Tizen workload (exit code $LASTEXITCODE)" }
}

Write-Host "Installed workloads:"
& dotnet workload list
if ($LASTEXITCODE -ne 0) { throw "Could not list installed workloads" }
} finally {
  if ($sdkDirectory) {
    if ((Get-Location).Path -eq $sdkDirectory) { Pop-Location }
    Remove-Item -LiteralPath $sdkDirectory -Recurse -Force
  }
}
