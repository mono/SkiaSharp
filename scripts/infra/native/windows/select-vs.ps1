param([string] $InstallationPath)

$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$products = @(
    'Microsoft.VisualStudio.Product.Enterprise'
    'Microsoft.VisualStudio.Product.Professional'
    'Microsoft.VisualStudio.Product.Community'
    'Microsoft.VisualStudio.Product.BuildTools'
)
$selection = @('-prerelease', '-version', '[18.0,19.0)')
$instances = @(& $vswhere -all @selection -products $products -format json | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0) { throw "vswhere exited with code $LASTEXITCODE." }

if ($InstallationPath) {
    $matches = @($instances | Where-Object { $_.installationPath -eq $InstallationPath })
    if ($matches.Count -ne 1) {
        throw "Expected exactly one Visual Studio 2026 installation at $InstallationPath, found $($matches.Count)."
    }
    $instance = $matches[0]
} else {
    foreach ($product in $products) {
        $instance = $instances |
            Where-Object { $_.productId -eq $product } |
            Sort-Object { [version]$_.installationVersion } -Descending |
            Select-Object -First 1
        if ($instance) { break }
    }
    if (!$instance) { throw 'An existing Visual Studio 2026 installation is required.' }
    $InstallationPath = $instance.installationPath
}

$configPath = if ($instance.productId -eq 'Microsoft.VisualStudio.Product.BuildTools') {
    Join-Path $PSScriptRoot 'build-tools.vsconfig'
} else {
    [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../source/.vsconfig'))
}

$env:VS_INSTALL = $InstallationPath
$env:VS_CONFIG = $configPath
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$InstallationPath"
Write-Host "##vso[task.setvariable variable=VS_CONFIG]$configPath"
if ($env:GITHUB_ENV) {
    "VS_INSTALL=$InstallationPath" | Add-Content $env:GITHUB_ENV
    "VS_CONFIG=$configPath" | Add-Content $env:GITHUB_ENV
}
Write-Host "Selected $($instance.productId) at $InstallationPath"
