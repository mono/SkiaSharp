Param(
    [string] $Version = "10.0.0"
)

$ErrorActionPreference = 'Stop'

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
& $install /S

# make sure that LLVM is in LLVM_HOME
Write-Host "##vso[task.setvariable variable=LLVM_HOME;]C:\Program Files\LLVM";

exit $LASTEXITCODE
