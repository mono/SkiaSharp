<#
.SYNOPSIS Push output/ai/ contents to the data-cache branch.
.DESCRIPTION Copies all files from output/ai/ to .data-cache/, then commits and pushes.
             Run this manually or as a CI step after skills have written their outputs.
.EXAMPLE  pwsh scripts/persist-to-cache.ps1
.EXAMPLE  pwsh scripts/persist-to-cache.ps1 -NoPush   # copy only, no git operations
#>
#requires -Version 7.0
param([switch]$NoPush)
$ErrorActionPreference = 'Stop'

$source = 'output/ai'
$dest = '.data-cache'

if (-not (Test-Path $source)) {
    Write-Host "ℹ️  No output/ai/ directory — nothing to persist"
    exit 0
}

if (-not (Test-Path $dest)) {
    Write-Host "❌ .data-cache/ not found — is the data-cache worktree set up?"
    exit 2
}

# Copy all files from output/ai/ to .data-cache/, preserving structure
$files = Get-ChildItem $source -Recurse -File
if ($files.Count -eq 0) {
    Write-Host "ℹ️  output/ai/ is empty — nothing to persist"
    exit 0
}

$resolvedSource = (Resolve-Path $source).Path
foreach ($file in $files) {
    $relativePath = [System.IO.Path]::GetRelativePath($resolvedSource, $file.FullName)
    $destPath = Join-Path $dest $relativePath
    New-Item -ItemType Directory -Force (Split-Path $destPath) | Out-Null
    Copy-Item $file.FullName $destPath -Force
    Write-Host "  📄 $relativePath"
}
Write-Host "✅ Copied $($files.Count) file(s) to $dest"

if ($NoPush) {
    Write-Host "ℹ️  Push skipped (-NoPush)"
    exit 0
}

Push-Location $dest
try {
    git add -A
    git diff --cached --quiet 2>&1
    if ($LASTEXITCODE -gt 1) {
        Write-Host "❌ git diff --cached --quiet failed (exit code $LASTEXITCODE)"
        exit 1
    }
    if ($LASTEXITCODE -eq 0) {
        Write-Host "ℹ️  No changes to commit"
        exit 0
    }

    git commit -m "ai: persist output from $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ git commit failed (exit code $LASTEXITCODE)"
        exit 1
    }

    $pushed = $false
    for ($i = 0; $i -lt 3; $i++) {
        git push 2>&1
        if ($LASTEXITCODE -eq 0) { $pushed = $true; break }
        if ($i -lt 2) {
            Write-Host "⚠️  Push failed (attempt $($i + 1)/3), rebasing..."
            git pull --rebase
            if ($LASTEXITCODE -ne 0) {
                Write-Host "❌ git pull --rebase failed (exit code $LASTEXITCODE)"
                exit 1
            }
        }
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
