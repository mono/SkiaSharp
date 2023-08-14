$ErrorActionPreference = 'Stop'

if ($IsMacOS) {
    brew install ninja
} elseif ($IsLinux) {
    sudo apt install -y ninja-build
} else {
    choco install ninja
}

exit $LASTEXITCODE
