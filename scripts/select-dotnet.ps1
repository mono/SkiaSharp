Param(
  [string] $InstallDir
)

$ErrorActionPreference = 'Stop'

Write-Host "Setting install directory to '$InstallDir'..."
$env:DOTNET_INSTALL_DIR=$InstallDir
$env:DOTNET_ROOT=$InstallDir
$env:DOTNET_MULTILEVEL_LOOKUP=0

Write-Host "##vso[task.setvariable variable=DOTNET_INSTALL_DIR;]$InstallDir";
Write-Host "##vso[task.setvariable variable=DOTNET_ROOT;]$InstallDir";
Write-Host "##vso[task.setvariable variable=DOTNET_MULTILEVEL_LOOKUP;]0";

$env:PATH = "$InstallDir;$env:PATH"
Write-Host "##vso[task.setvariable variable=PATH;]$env:PATH";

Write-Host "Checking all dotnet info..."
& dotnet --info

exit $LASTEXITCODE
