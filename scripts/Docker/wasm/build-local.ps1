Param(
    [string] $Version = "3.1.34",
    [string] $Features = "_wasmeh,st" # any combination of: _wasmeh,simd,st,mt,none
)

$ErrorActionPreference = 'Stop'

$DIR = "$PSScriptRoot"

Push-Location $DIR
try {
    docker build --tag skiasharp-wasm:$Version --build-arg EMSCRIPTEN_VERSION=$Version .
} finally {
    Pop-Location
}

$pwd = (Join-Path $DIR "../../../")
docker run --rm --name skiasharp-wasm-$Version --volume ${pwd}:/work skiasharp-wasm:$Version /bin/bash -c "\
    dotnet tool restore ; \
    dotnet cake --target=externals-wasm --emscriptenVersion=$Version --emscriptenFeatures='$Features'"

exit $LASTEXITCODE
