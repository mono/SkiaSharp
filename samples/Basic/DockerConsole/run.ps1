param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

$ErrorActionPreference = 'Stop'

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/console"

# Windows containers need Windows-style mount paths
if ($Platform -eq "windows") {
    $containerOutput = "C:\output"
} else {
    $containerOutput = "/tmp/output"
}

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .
if ($LASTEXITCODE -ne 0) { throw "Docker build failed with exit code $LASTEXITCODE" }

Write-Host "Running $tag..."
docker run --rm -v "${pwd}:${containerOutput}" $tag $Text --output "$containerOutput/output.png"
if ($LASTEXITCODE -ne 0) { throw "Docker run failed with exit code $LASTEXITCODE" }

$outputFile = Join-Path $pwd "output.png"
if (Test-Path $outputFile) {
    Write-Host "Saved to $outputFile"
} else {
    throw "Failed to generate image."
}
