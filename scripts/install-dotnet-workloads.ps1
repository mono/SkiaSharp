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

# Install official .NET workloads needed by the repo.
$Workloads = @('android', 'macos', 'wasm-tools')
if (!$IsLinux) {
  $Workloads += @('ios', 'tvos', 'maccatalyst', 'maui')
}

Write-Host "Installing .NET workloads: $($Workloads -join ', ')..."

& dotnet workload install `
  @Workloads `
  --source https://api.nuget.org/v3/index.json `
  --skip-sign-check `
  --skip-manifest-update
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Tizen is not an official workload — it uses Samsung's custom install scripts.
# Install AFTER official workloads so Tizen manifest is not overwritten.
Write-Host "Installing Tizen workloads..."
New-Item -ItemType Directory -Force './output/tmp' | Out-Null
if ($IsLinux -or $IsMacOS) {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.sh' -OutFile './output/tmp/workload-install.sh'
  bash output/tmp/workload-install.sh --version "$Tizen"
} else {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile './output/tmp/workload-install.ps1'
  ./output/tmp/workload-install.ps1 -Version "$Tizen"
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
