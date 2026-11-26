#!/usr/bin/env pwsh
# Runs the native artifact tests (tests-native) inside the same Linux image CI uses, so Windows and
# macOS hosts also run the Linux-only cases. Extra arguments are forwarded to Cake.
$ErrorActionPreference = 'Stop'

$image = 'skiasharp-native-tests'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../..'))

docker build --tag $image $PSScriptRoot
if ($LASTEXITCODE -ne 0) { throw "docker build exited with code $LASTEXITCODE." }

docker run --rm --name $image --volume "${root}:/work" --workdir /work $image `
    /bin/sh -c 'dotnet tool restore && dotnet cake --target=tests-native "$@"' _ @args
if ($LASTEXITCODE -ne 0) { throw "Native tests exited with code $LASTEXITCODE." }
