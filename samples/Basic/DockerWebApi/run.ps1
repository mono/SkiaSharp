param (
    [string] $Platform = "",
    [string] $Text = "SkiaSharp"
)

$ErrorActionPreference = 'Stop'

if (-not $Platform) {
    $Platform = if ($IsLinux -or $IsMacOS) { "linux" } else { "windows" }
}

$dockerfile = "$Platform.Dockerfile"
$tag = "skiasharpsample/webapi"
$port = 8080
$outputFile = Join-Path $pwd "output.png"

Write-Host "Building $tag from $dockerfile..."
docker build --tag $tag --file $dockerfile .
if ($LASTEXITCODE -ne 0) { throw "Docker build failed with exit code $LASTEXITCODE" }

Write-Host "Running $tag on port $port..."
$containerId = docker run -d -p "${port}:8080" $tag
if ($LASTEXITCODE -ne 0) { throw "Docker run failed with exit code $LASTEXITCODE" }
Write-Host "Container: $containerId"

$failed = $false
try {
    # Wait for the server to be ready
    $maxRetries = 30
    $ready = $false
    for ($i = 0; $i -lt $maxRetries; $i++) {
        Start-Sleep -Seconds 1
        try {
            Invoke-WebRequest -Uri "http://localhost:$port/health" -ErrorAction Stop | Out-Null
            Write-Host "Server is ready!"
            $ready = $true
            break
        } catch {
            Write-Host "Waiting for server... ($($i + 1)/$maxRetries)"
        }
    }
    if (-not $ready) { throw "Server did not become ready after $maxRetries seconds" }

    # Fetch and save the image
    $uri = "http://localhost:$port/api/images/$([uri]::EscapeDataString($Text))"
    Write-Host "Fetching image from $uri ..."
    Invoke-WebRequest -Uri $uri -OutFile $outputFile
    Write-Host "Saved to $outputFile"
} catch {
    $failed = $true
    Write-Host "Container logs:"
    docker logs $containerId 2>&1 | Write-Host
    throw
} finally {
    Write-Host "Stopping container..."
    docker stop $containerId 2>$null | Out-Null
    docker rm -f $containerId 2>$null | Out-Null
}

Write-Host "Done."
