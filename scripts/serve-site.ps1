# Serve the SkiaSharp docs site locally for preview.
#
# Usage:
#   pwsh scripts/serve-site.ps1                     # serve from docs-staging branch
#   pwsh scripts/serve-site.ps1 docs-live            # serve from docs-live branch
#
# Requirements: python3

param(
    [Parameter(Position = 0)]
    [string]$Branch = "docs-staging",

    [int]$Port = 8080
)

$RepoUrl = "https://github.com/mono/SkiaSharp.git"
$TempDir = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-preview-$([System.Guid]::NewGuid().ToString('N').Substring(0,8))"
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

Write-Host "Cloning $Branch branch..." -ForegroundColor Cyan
git clone --depth=1 --branch $Branch $RepoUrl (Join-Path $TempDir "repo") 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Branch '$Branch' does not exist. Try: docs-staging, docs-live"
    Remove-Item $TempDir -Recurse -Force -ErrorAction SilentlyContinue
    exit 1
}

# Create SkiaSharp/ junction to match GitHub Pages path
$ServeDir = Join-Path $TempDir "serve"
New-Item -ItemType Directory -Path $ServeDir -Force | Out-Null
New-Item -ItemType Junction -Path (Join-Path $ServeDir "SkiaSharp") -Target (Join-Path $TempDir "repo") -Force | Out-Null

Write-Host "Serving $Branch branch" -ForegroundColor Cyan
Write-Host "Open: http://localhost:$Port/SkiaSharp/" -ForegroundColor Green
Write-Host ""

try {
    Push-Location $ServeDir
    python3 -m http.server $Port
}
finally {
    Pop-Location
    Remove-Item $TempDir -Recurse -Force -ErrorAction SilentlyContinue
}
