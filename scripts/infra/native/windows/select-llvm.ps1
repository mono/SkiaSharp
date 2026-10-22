param([string] $InstallationPath = $env:VS_INSTALL)

$ErrorActionPreference = 'Stop'

if (!$InstallationPath) {
    throw 'Select Visual Studio before selecting LLVM.'
}

$llvm = Join-Path $InstallationPath 'VC\Tools\Llvm\x64'
$clang = Join-Path $llvm 'bin\clang.exe'
if (!(Test-Path $clang -PathType Leaf)) {
    throw "Visual Studio LLVM was not installed: $clang"
}

$env:LLVM_HOME = $llvm
Write-Host "##vso[task.setvariable variable=LLVM_HOME]$llvm"
if ($env:GITHUB_ENV) {
    "LLVM_HOME=$llvm" | Add-Content $env:GITHUB_ENV
}

Write-Host "Selected LLVM at $llvm"
& $clang --version
if ($LASTEXITCODE -ne 0) {
    throw "Visual Studio LLVM exited with code $LASTEXITCODE."
}
