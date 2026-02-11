Param(
  [string] $WorkloadVersion = '',
  [string] $Tizen = ''
)

$ErrorActionPreference = 'Stop'

if (!$Tizen) {
  $Tizen = '<latest>'
}

# Install .NET workloads needed by the repo.
# Use --version to pin to a specific workload set version for reproducibility.
$Workloads = @('android', 'macos', 'wasm-tools')
if (!$IsLinux) {
  $Workloads += @('ios', 'tvos', 'maccatalyst', 'maui')
}

$versionArgs = @()
if ($WorkloadVersion) {
  $versionArgs = @('--version', $WorkloadVersion)
  Write-Host "Installing .NET workloads (version $WorkloadVersion): $($Workloads -join ', ')..."
} else {
  Write-Host "Installing .NET workloads: $($Workloads -join ', ')..."
}

& dotnet workload install `
  @Workloads `
  @versionArgs `
  --source https://api.nuget.org/v3/index.json `
  --skip-sign-check `
  --verbosity diagnostic
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Tizen is not an official workload — it uses Samsung's custom install scripts.
# Samsung's script LatestVersionMap may not have .NET 10 entries yet,
# so we explicitly pass the target version band (10.0.100) to force correct installation.
Write-Host "Installing Tizen workloads..."
New-Item -ItemType Directory -Force './output/tmp' | Out-Null
if ($IsLinux -or $IsMacOS) {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.sh' -OutFile './output/tmp/workload-install.sh'
  bash output/tmp/workload-install.sh --version "$Tizen" --dotnet-target-version-band "10.0.100"
} else {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile './output/tmp/workload-install.ps1'
  ./output/tmp/workload-install.ps1 -Version "$Tizen" -DotnetTargetVersionBand "10.0.100"
}

exit $LASTEXITCODE
