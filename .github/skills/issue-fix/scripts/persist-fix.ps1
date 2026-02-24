<#
.SYNOPSIS Persist a validated fix JSON to data-cache.
.DESCRIPTION Copies the fix JSON to the data-cache worktree and optionally commits+pushes.
             In benchmark mode ($env:SKIASHARP_BENCHMARK or -NoPush), git operations are skipped.
.EXAMPLE  pwsh persist-fix.ps1 /tmp/skiasharp/fix/3400.json
.EXAMPLE  pwsh persist-fix.ps1 /tmp/skiasharp/fix/3400.json -NoPush
#>
param(
    [Parameter(Mandatory, Position = 0)][string]$Path,
    [switch]$NoPush
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }

$number = [System.IO.Path]::GetFileNameWithoutExtension($Path)
if ($number -notmatch '^\d+$') {
    Write-Host "❌ Cannot extract issue number from filename: $(Split-Path $Path -Leaf)"
    Write-Host "   Expected format: {number}.json"
    exit 2
}

$dest = ".data-cache/repos/mono-SkiaSharp/ai-fix/$number.json"
New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
Copy-Item $Path $dest -Force
Write-Host "✅ Copied to $dest"

if ($NoPush -or $env:SKIASHARP_BENCHMARK) {
    Write-Host "ℹ️  Push skipped (benchmark mode)"
    exit 0
}

Push-Location .data-cache
try {
    git add "repos/mono-SkiaSharp/ai-fix/$number.json"

    git diff --cached --quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "ℹ️  No changes to commit"
        exit 0
    }

    git commit -m "ai-fix: fix #$number"

    $pushed = $false
    for ($i = 0; $i -lt 3; $i++) {
        git push 2>&1
        if ($LASTEXITCODE -eq 0) { $pushed = $true; break }
        Write-Host "⚠️  Push failed (attempt $($i + 1)/3), rebasing..."
        git pull --rebase
    }

    if ($pushed) {
        Write-Host "✅ Pushed to data-cache"
    } else {
        Write-Host "❌ Push failed after 3 attempts"
        exit 1
    }
} finally {
    Pop-Location
}
