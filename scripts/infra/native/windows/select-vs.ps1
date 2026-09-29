$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

Write-Host "Installed Visual Studio Versions:"
& $vswhere -all -version '[17.0,18.0)' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath

Write-Host "Setting Environment Variables..."
$installationPath = & $vswhere -latest -version '[17.0,18.0)' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $installationPath) {
    throw "Could not find Visual Studio 2022 with the MSVC C++ build tools installed."
}
Write-Host "##vso[task.prependpath]$installationPath\MSBuild\Current\Bin"
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$installationPath"
$env:VS_INSTALL = $installationPath
Write-Host "Selected VS $installationPath"

exit $LASTEXITCODE
