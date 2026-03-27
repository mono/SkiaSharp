param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/console"
$outputFile = Join-Path $PSScriptRoot "output.png"

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .

Write-Host "Running $tag..."
if ($Platform -eq "windows") {
    docker run --rm -v "${PSScriptRoot}:C:\work" $tag $Text --output C:\work\output.png
} else {
    docker run --rm -v "${PSScriptRoot}:/work" $tag $Text --output /work/output.png
}

if (Test-Path $outputFile) {
    Write-Host "Saved to $outputFile"
    if ($IsMacOS) { open $outputFile }
    elseif ($IsLinux) { xdg-open $outputFile 2>$null }
    else { Start-Process $outputFile }
} else {
    Write-Error "Failed to generate image."
}
