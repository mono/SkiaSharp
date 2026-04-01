# Serve the SkiaSharp docs site locally for preview.
#
# Usage:
#   pwsh scripts/serve-site.ps1                     # serve from docs-staging branch
#   pwsh scripts/serve-site.ps1 docs-live            # serve from docs-live branch
#
# Requirements: python3
#
# The script caches the clone in a fixed location and reuses it on
# subsequent runs, pulling the latest changes each time.

param(
    [Parameter(Position = 0)]
    [string]$Branch = "docs-staging",

    [int]$Port = 8080
)

$RepoUrl = "https://github.com/mono/SkiaSharp.git"
$CacheRoot = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-site-preview"
$RepoDir = Join-Path $CacheRoot "repo"
$ServeDir = Join-Path $CacheRoot "serve"

New-Item -ItemType Directory -Path $CacheRoot -Force | Out-Null

if (Test-Path (Join-Path $RepoDir ".git")) {
    # Reuse existing clone — fetch and reset to latest
    Write-Host "Updating cached clone..." -ForegroundColor Cyan
    git -C $RepoDir fetch origin $Branch --depth=1 2>&1
    git -C $RepoDir checkout -B $Branch "origin/$Branch" 2>&1
    git -C $RepoDir reset --hard "origin/$Branch" 2>&1
    git -C $RepoDir clean -fdx 2>&1
} else {
    # Fresh clone
    if (Test-Path $RepoDir) { Remove-Item $RepoDir -Recurse -Force }
    Write-Host "Cloning $Branch branch..." -ForegroundColor Cyan
    git clone --depth=1 --branch $Branch $RepoUrl $RepoDir 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Branch '$Branch' does not exist. Try: docs-staging, docs-live"
        exit 1
    }
}

$sha = (git -C $RepoDir rev-parse --short HEAD).Trim()
Write-Host "Serving $Branch @ $sha" -ForegroundColor Cyan

# Create SkiaSharp/ junction to match GitHub Pages path
New-Item -ItemType Directory -Path $ServeDir -Force | Out-Null
$junction = Join-Path $ServeDir "SkiaSharp"
if (-not (Test-Path $junction)) {
    New-Item -ItemType Junction -Path $junction -Target $RepoDir -Force | Out-Null
}

Write-Host "Open: http://localhost:$Port/SkiaSharp/" -ForegroundColor Green
Write-Host "Cache: $CacheRoot" -ForegroundColor DarkGray
Write-Host ""

Push-Location $ServeDir
try { python3 -m http.server $Port }
finally { Pop-Location }
