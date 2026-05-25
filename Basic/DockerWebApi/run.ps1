param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/webapi"
$port = 8080
$outputFile = Join-Path $pwd "output.png"

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .

Write-Host "Running $tag on port $port..."
$containerId = docker run --rm -d -p "${port}:8080" $tag
Write-Host "Container: $containerId"

# Wait for the server to be ready
$maxRetries = 30
for ($i = 0; $i -lt $maxRetries; $i++) {
    Start-Sleep -Seconds 1
    try {
        Invoke-WebRequest -Uri "http://localhost:$port/health" -ErrorAction Stop | Out-Null
        Write-Host "Server is ready!"
        break
    } catch {
        Write-Host "Waiting for server... ($($i + 1)/$maxRetries)"
    }
}

# Fetch and save the image
$uri = "http://localhost:$port/api/images/$([uri]::EscapeDataString($Text))"
Write-Host "Fetching image from $uri ..."
Invoke-WebRequest -Uri $uri -OutFile $outputFile
Write-Host "Saved to $outputFile"

# Stop the container
Write-Host "Stopping container..."
docker stop $containerId | Out-Null

Write-Host "Done."
