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
New-Item -ItemType Directory -Force './output/tmp' | Out-Null
$tizenInstallFailed = $false
$tizenErrorOutput = ""
if ($IsLinux -or $IsMacOS) {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.sh' -OutFile './output/tmp/workload-install.sh'
  $tizenOutput = bash output/tmp/workload-install.sh --version "$Tizen" 2>&1
  $tizenExitCode = $LASTEXITCODE
  Write-Host $tizenOutput
  # Check for failure indicators in output (Samsung script may not set exit code properly)
  if ($tizenOutput -match "failed|error|not found" -and $tizenOutput -notmatch "0 Error") {
    $tizenInstallFailed = $true
    $tizenErrorOutput = $tizenOutput
  }
} else {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile './output/tmp/workload-install.ps1'
  try {
    ./output/tmp/workload-install.ps1 -Version "$Tizen"
  } catch {
    $tizenInstallFailed = $true
    $tizenErrorOutput = $_.Exception.Message
  }
  $tizenExitCode = $LASTEXITCODE
}
Write-Host "Done installing Tizen workload $Tizen"

if ($tizenExitCode -ne 0) {
  Write-Host "##[error]Tizen workload installation failed with exit code $tizenExitCode"
  exit $tizenExitCode
}
if ($tizenInstallFailed) {
  Write-Host "##[error]Tizen workload installation failed: $tizenErrorOutput"
  exit 1
}

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
