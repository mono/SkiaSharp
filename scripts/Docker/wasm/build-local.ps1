Param(
    [string] $Version = "3.1.56",
    [string] $Features = "_wasmeh,st" # any combination of: _wasmeh,simd,st,mt
)

$ErrorActionPreference = 'Stop'

$DIR = "$PSScriptRoot"

# Use linux/amd64 platform for Docker — older emscripten versions don't publish arm64 binaries
$PlatformArgs = @()
$arch = if ($IsLinux) { uname -m } elseif ($IsMacOS) { uname -m } else { $env:PROCESSOR_ARCHITECTURE }
if ($arch -eq "arm64" -or $arch -eq "aarch64" -or $arch -eq "ARM64") {
    $PlatformArgs = @("--platform", "linux/amd64")
}

Push-Location $DIR
try {
    docker build @PlatformArgs --tag skiasharp-wasm:$Version --build-arg EMSCRIPTEN_VERSION=$Version .
} finally {
    Pop-Location
}

$FeaturesArg = ""
if ($Features) {
    $FeaturesArg = "--emscriptenFeatures='$Features'"
}

$pwd = (Resolve-Path (Join-Path $DIR "../../../")).Path
docker run --rm @PlatformArgs --name skiasharp-wasm-$Version --volume ${pwd}:/work skiasharp-wasm:$Version /bin/bash -c "\
    dotnet tool restore ; \
    dotnet cake --target=externals-wasm --emscriptenVersion=$Version $FeaturesArg"

exit $LASTEXITCODE
