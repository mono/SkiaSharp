param (
    [string] $Platform = ""
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/console"

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .

Write-Host "Running $tag..."
docker run --rm $tag
