param([string] $InstallationPath)

$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$products = @(
    'Microsoft.VisualStudio.Product.Community'
    'Microsoft.VisualStudio.Product.Professional'
    'Microsoft.VisualStudio.Product.Enterprise'
    'Microsoft.VisualStudio.Product.BuildTools'
)
$selection = @('-prerelease', '-products') + $products + @('-version', '[18.0,19.0)')

Write-Host "Installed Visual Studio Versions:"
& $vswhere -all @selection -property installationPath

Write-Host "Selecting Visual Studio 2026..."
$instances = @(& $vswhere -all @selection -property installationPath)
if ($LASTEXITCODE -ne 0) { throw "vswhere exited with code $LASTEXITCODE." }
if ($InstallationPath) {
    if ($instances -notcontains $InstallationPath) { throw "No Visual Studio 2026 installation at $InstallationPath." }
} else {
    $InstallationPath = & $vswhere @selection -latest -property installationPath
    if ($LASTEXITCODE -ne 0 -or !$InstallationPath) { throw 'An existing Visual Studio 2026 installation is required.' }
}

$msbuild = Join-Path $InstallationPath 'MSBuild\Current\Bin'
$env:VS_INSTALL = $InstallationPath
Write-Host "##vso[task.prependpath]$msbuild"
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$InstallationPath"
if ($env:GITHUB_ENV) {
    "VS_INSTALL=$InstallationPath" | Add-Content $env:GITHUB_ENV
}
if ($env:GITHUB_PATH) {
    $msbuild | Add-Content $env:GITHUB_PATH
}
Write-Host "Selected Visual Studio 2026 at $InstallationPath"
