param([string] $InstallationPath)

$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

Write-Host "Installed Visual Studio Versions:"
& $vswhere -all -prerelease -property installationPath

Write-Host "Setting Environment Variables..."
$selection = @('-all', '-products', '*', '-version', '[18.0,19.0)', '-requires',
    'Microsoft.VisualStudio.Component.VC.Tools.x86.x64')
$instances = @(& $vswhere @selection -property installationPath)
if ($LASTEXITCODE -ne 0) { throw "vswhere exited with code $LASTEXITCODE." }
if ($InstallationPath) {
    if ($instances -notcontains $InstallationPath) { throw "No VS 2026 C++ installation at $InstallationPath." }
} else {
    $InstallationPath = & $vswhere @selection -latest -property installationPath
    if ($LASTEXITCODE -ne 0 -or !$InstallationPath) { throw 'Visual Studio 2026 C++ tools are required.' }
}
Write-Host "##vso[task.prependpath]$installationPath\MSBuild\Current\Bin"
Write-Host "##vso[task.setvariable variable=VS_INSTALL]$installationPath"
$env:VS_INSTALL = $installationPath
Write-Host "Selected VS $installationPath"

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
}
