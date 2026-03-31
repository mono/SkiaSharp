param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/console"

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .

Write-Host "Running $tag..."
docker run --rm -v "${pwd}:/tmp/output" $tag $Text --output /tmp/output/output.png

$outputFile = Join-Path $pwd "output.png"
if (Test-Path $outputFile) {
    Write-Host "Saved to $outputFile"
} else {
    Write-Error "Failed to generate image."
}
