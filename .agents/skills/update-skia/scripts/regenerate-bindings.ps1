#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Phase 7: Regenerate bindings with automatic HarfBuzz revert and diff summary.

.DESCRIPTION
    Performs ALL steps of Phase 7 of the update-skia skill:
    1. Runs pwsh ./utils/generate.ps1 to regenerate all bindings
    2. IMMEDIATELY reverts HarfBuzz generated bindings (HarfBuzz updates are always separate)
    3. Reports the diff summary of what changed in SkiaSharp bindings
    4. Lists any NEW functions that may need C# wrappers

.EXAMPLE
    pwsh .claude/skills/update-skia/scripts/regenerate-bindings.ps1
#>

$ErrorActionPreference = 'Stop'
$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot

Write-Host "`n=== Phase 7: Regenerate Bindings ===" -ForegroundColor Cyan

# --- Step 1: Run generator ---
Write-Host "`n--- Running binding generator ---" -ForegroundColor Yellow
& pwsh ./utils/generate.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Binding generation failed"
}
Write-Host "  Generation complete" -ForegroundColor Green

# --- Step 2: IMMEDIATELY revert HarfBuzz (before anything else) ---
Write-Host "`n--- Reverting HarfBuzz generated bindings ---" -ForegroundColor Yellow
Write-Host "  HarfBuzz updates are ALWAYS separate from Skia milestone updates." -ForegroundColor DarkGray
Write-Host "  The generator picks up API changes that need hand-written delegate proxies." -ForegroundColor DarkGray

$harfbuzzFile = 'binding/HarfBuzzSharp/HarfBuzzApi.generated.cs'
$harfbuzzDiff = git diff --stat $harfbuzzFile
if ($harfbuzzDiff) {
    git checkout HEAD -- $harfbuzzFile
    Write-Host "  Reverted $harfbuzzFile" -ForegroundColor Green
} else {
    Write-Host "  No HarfBuzz changes to revert" -ForegroundColor Green
}

# --- Step 3: Report SkiaSharp binding changes ---
Write-Host "`n--- Binding diff summary ---" -ForegroundColor Yellow
$bindingDiff = git diff --stat 'binding/'
if ($bindingDiff) {
    Write-Host $bindingDiff
} else {
    Write-Host "  No changes to bindings (C API signatures unchanged)" -ForegroundColor Green
}

# --- Step 4: Check for NEW functions needing C# wrappers ---
Write-Host "`n--- New generated functions (may need C# wrappers) ---" -ForegroundColor Yellow
$newFunctions = git diff 'binding/SkiaSharp/SkiaApi.generated.cs' |
    Select-String '^\+.*internal static' |
    ForEach-Object { $_.Line.Trim().TrimStart('+').Trim() }

if ($newFunctions) {
    Write-Host "  Found $($newFunctions.Count) new function(s):" -ForegroundColor Yellow
    foreach ($fn in $newFunctions) {
        Write-Host "    $fn" -ForegroundColor Yellow
    }
    Write-Host "`n  Check if each needs a C# wrapper in binding/SkiaSharp/*.cs" -ForegroundColor Yellow
} else {
    Write-Host "  No new functions found" -ForegroundColor Green
}

Write-Host "`n✅ Phase 7 complete" -ForegroundColor Green
