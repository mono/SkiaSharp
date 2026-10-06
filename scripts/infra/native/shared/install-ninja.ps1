$ErrorActionPreference = 'Stop'

if (Get-Command ninja -ErrorAction SilentlyContinue) {
    ninja --version
    exit $LASTEXITCODE
} elseif ($IsMacOS) {
    brew install ninja
} elseif ($IsLinux) {
    sudo apt install -y ninja-build
} else {
    choco install ninja
}

exit $LASTEXITCODE
