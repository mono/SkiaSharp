Param(
    [string] $Version = "11.1.0",
    [string] $InstallDestination = "C:\Program Files\LLVM"
)

$ErrorActionPreference = 'Stop'

& clang --version

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

$url = "https://github.com/llvm/llvm-project/releases/download/llvmorg-${Version}/LLVM-${Version}-win64.exe"

$llvmTemp = "$HOME_DIR/llvm-temp"
$install = "$llvmTemp/llvm.exe"

# download
Write-Host "Downloading LLVM..."
New-Item -ItemType Directory -Force -Path "$llvmTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# install
Write-Host "Installing LLVM..."
& $install /S /D=$InstallDestination

# echo version
dir "C:\Program Files\"
dir "C:\Program Files\LLVM\"
dir "C:\Program Files\LLVM\bin\"
& "C:\Program Files\LLVM\bin\clang.exe" --version

# make sure that LLVM is in LLVM_HOME
Write-Host "##vso[task.setvariable variable=LLVM_HOME;]$InstallDestination";

exit $LASTEXITCODE
