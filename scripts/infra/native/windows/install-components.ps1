Param(
    [string] $InstallationPath = $env:VS_INSTALL
)

$ErrorActionPreference = 'Stop'

if (!$InstallationPath) {
    throw 'Select an existing Visual Studio 2026 instance before installing components.'
}

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$products = @(
    'Microsoft.VisualStudio.Product.Community'
    'Microsoft.VisualStudio.Product.Professional'
    'Microsoft.VisualStudio.Product.Enterprise'
    'Microsoft.VisualStudio.Product.BuildTools'
)
$selection = @('-prerelease', '-products') + $products + @('-version', '[18.0,19.0)')
$instances = @(& $vswhere -all @selection -format json | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0) { throw "vswhere exited with code $LASTEXITCODE." }
$matches = @($instances | Where-Object { $_.installationPath -eq $InstallationPath })
if ($matches.Count -ne 1) {
    throw "Expected exactly one Visual Studio 2026 product at $InstallationPath, found $($matches.Count)."
}
$instance = $matches[0]

$config = if ($instance.productId -eq 'Microsoft.VisualStudio.Product.BuildTools') {
    Join-Path $PSScriptRoot 'build-tools.vsconfig'
} else {
    [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../source/.vsconfig'))
}
$components = (Get-Content $config -Raw | ConvertFrom-Json).components
$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
$arguments = @(
    'modify', '--installPath', "`"$InstallationPath`"",
    '--config', "`"$config`"", '--quiet', '--norestart'
)
Write-Host "Installing components from $config into $InstallationPath"
$process = Start-Process -FilePath $installer -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Visual Studio component installation exited with code $($process.ExitCode)."
}

$installed = & $vswhere -all @selection -requires $components -property installationPath
if ($LASTEXITCODE -ne 0 -or @($installed) -notcontains $InstallationPath) {
    throw "Could not verify all configured components in $InstallationPath (vswhere exit code $LASTEXITCODE)."
}
Write-Host 'Verified all configured Visual Studio components.'

$env:VS_INSTALL = $InstallationPath
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$InstallationPath"
if ($env:GITHUB_ENV) { "VS_INSTALL=$InstallationPath" | Add-Content $env:GITHUB_ENV }

$tools = @{
    LLVM_HOME = Join-Path $InstallationPath 'VC\Tools\Llvm\x64'
    NINJA_EXE = Join-Path $InstallationPath 'Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja\ninja.exe'
}
foreach ($name in $tools.Keys) {
    $path = $tools[$name]
    $executable = if ($name -eq 'LLVM_HOME') { Join-Path $path 'bin\clang.exe' } else { $path }
    if (!(Test-Path $executable)) { throw "Missing VS Installer tool: $executable" }
    [Environment]::SetEnvironmentVariable($name, $path, 'Process')
    Write-Host "##vso[task.setvariable variable=$name]$path"
    if ($env:GITHUB_ENV) { "$name=$path" | Add-Content $env:GITHUB_ENV }
}

$cmake = Join-Path $InstallationPath 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin'
if (!(Test-Path (Join-Path $cmake 'cmake.exe'))) { throw "Missing VS Installer tool: $cmake\cmake.exe" }
Write-Host "##vso[task.prependpath]$cmake"
if ($env:GITHUB_PATH) { $cmake | Add-Content $env:GITHUB_PATH }
