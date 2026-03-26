param (
    [string] $Platform = ""
)

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/webapi"
$port = 8080

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
        $response = Invoke-WebRequest -Uri "http://localhost:$port/api/images" -Method Head -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "Server is ready!"
            break
        }
    } catch {
        Write-Host "Waiting for server... ($($i + 1)/$maxRetries)"
    }
}

# Fetch and save the image
Write-Host "Fetching image from http://localhost:$port/api/images ..."
Invoke-WebRequest -Uri "http://localhost:$port/api/images" -OutFile "output.png"
Write-Host "Saved to output.png"

# Stop the container
Write-Host "Stopping container..."
docker stop $containerId | Out-Null
Write-Host "Done."
