Param(
  [string] $SourceUrl = '',
  [string] $InstallDir = '',
  [string] $Tizen = '',
  [boolean] $IsPreview = $false
)

$ErrorActionPreference = 'Stop'

if (!$Tizen) {
  $Tizen = '<latest>'
}

$feed1 = 'https://api.nuget.org/v3/index.json'
$feed2 = 'https://api.nuget.org/v3/index.json'
$feed3 = 'https://api.nuget.org/v3/index.json'
if ($IsPreview) {
  $feed1 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json'
  $feed2 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json'
  $feed3 = 'https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json'
}

$Workloads = 'android','macos','wasm-tools'
if (!$IsLinux) {
  $Workloads += 'ios','tvos','maccatalyst','maui'
}
if ($IsPreview) {
  $Workloads += 'wasm-tools-net7'
} else {
  $Workloads += 'wasm-tools-net6'
}

if ($SourceUrl) {
  $Rollback = '--from-rollback-file',"$SourceUrl"
} elseif ($IsPreview) {
  Write-Error "A preview workload install was requested, but no rollback file was provided. Specify the -SourceUrl."
  exit 1
}

Write-Host "Installing .NET workloads..."
& dotnet workload install `
  @Workloads `
  @Rollback `
  --source https://api.nuget.org/v3/index.json `
  --source $feed1 `
  --source $feed2 `
  --source $feed3 `
  --skip-sign-check

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
