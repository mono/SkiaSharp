Param(
  [string] $Tizen = ''
)

$ErrorActionPreference = 'Stop'

if (!$Tizen) {
  $Tizen = '<latest>'
}

# Workloads are resolved from the installed SDK version.
# `dotnet workload restore` installs workloads required by projects in the repo.
Write-Host "Restoring .NET workloads..."
& dotnet workload restore --verbosity diagnostic
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Tizen is not an official workload — it uses Samsung's custom install scripts.
Write-Host "Installing Tizen workloads..."
New-Item -ItemType Directory -Force './output/tmp' | Out-Null
if ($IsLinux -or $IsMacOS) {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.sh' -OutFile './output/tmp/workload-install.sh'
  bash output/tmp/workload-install.sh --version "$Tizen"
} else {
  Invoke-WebRequest 'https://raw.githubusercontent.com/Samsung/Tizen.NET/main/workload/scripts/workload-install.ps1' -OutFile './output/tmp/workload-install.ps1'
  ./output/tmp/workload-install.ps1 -Version "$Tizen"
}

exit $LASTEXITCODE
