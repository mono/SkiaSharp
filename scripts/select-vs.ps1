$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

Write-Host "Installed Visual Studio Versions:"
& $vswhere -all -prerelease -property installationPath

Write-Host "Setting Environment Variables..."
$installationPath = & $vswhere -latest -prerelease -property installationPath
Write-Host "##vso[task.prependpath]$installationPath\MSBuild\Current\Bin"
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$installationPath"
$env:VS_INSTALL = $installationPath
Write-Host "Selected VS $installationPath"

exit $LASTEXITCODE
