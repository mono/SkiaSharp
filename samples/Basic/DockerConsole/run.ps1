param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/console"
$outputDir = Join-Path $PSScriptRoot "output"
$outputFile = Join-Path $outputDir "output.png"

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .

Write-Host "Running $tag..."
docker run --rm -v "${outputDir}:/output" $tag $Text --output /output/output.png

if (Test-Path $outputFile) {
    Write-Host "Saved to $outputFile"
    if ($IsMacOS) { open $outputFile }
    elseif ($IsLinux) { xdg-open $outputFile 2>$null }
    else { Start-Process $outputFile }
} else {
    Write-Error "Failed to generate image."
}
