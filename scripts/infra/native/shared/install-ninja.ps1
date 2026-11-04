$ErrorActionPreference = 'Stop'

if ($IsMacOS) {
    brew install ninja
    $ninja = (Get-Command ninja -CommandType Application -ErrorAction Stop).Source
} elseif ($IsLinux) {
    sudo apt install -y ninja-build
    $ninja = (Get-Command ninja -CommandType Application -ErrorAction Stop).Source
} else {
    if (!$env:VS_INSTALL) {
        throw 'Select Visual Studio before configuring Ninja.'
    }
    $ninja = Join-Path $env:VS_INSTALL 'Common7\IDE\CommonExtensions\Microsoft\CMake\Ninja\ninja.exe'
    if (!(Test-Path $ninja -PathType Leaf)) {
        throw "Visual Studio Ninja was not installed: $ninja"
    }
}

$env:NINJA_EXE = $ninja
Write-Host "##vso[task.setvariable variable=NINJA_EXE]$ninja"
if ($env:GITHUB_ENV) {
    "NINJA_EXE=$ninja" | Add-Content $env:GITHUB_ENV
}
& $ninja --version

exit $LASTEXITCODE
