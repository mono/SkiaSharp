#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Phase 6: Update all SkiaSharp version files for a Skia milestone update.

.DESCRIPTION
    Performs ALL version updates required by Phase 6 of the update-skia skill:
    - scripts/VERSIONS.txt (milestone, increment, soname, assembly, file, all nuget lines)
    - cgmanifest.json (commitHash, version, chrome_milestone, upstream_merge_commit)
    - scripts/azure-pipelines-variables.yml (if it exists)
    - externals/skia/include/c/sk_types.h (verifies SK_C_INCREMENT is 0)

    Then runs the mandatory verification greps to catch any stale references.

.PARAMETER Current
    The current Skia milestone number (e.g., 119)

.PARAMETER Target
    The target Skia milestone number (e.g., 120)

.EXAMPLE
    pwsh .github/skills/update-skia/scripts/update-versions.ps1 -Current 119 -Target 120
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$Current,

    [Parameter(Mandatory=$true)]
    [int]$Target
)

$ErrorActionPreference = 'Stop'
$repoRoot = git rev-parse --show-toplevel

Write-Host "`n=== Phase 6: Update Version Files (m$Current -> m$Target) ===" -ForegroundColor Cyan

# --- Step 1: VERSIONS.txt ---
Write-Host "`n--- Updating scripts/VERSIONS.txt ---" -ForegroundColor Yellow
$versionsPath = Join-Path $repoRoot 'scripts/VERSIONS.txt'
$content = Get-Content $versionsPath -Raw

# Get current nuget version pattern (e.g., 3.119.4)
$nugetPattern = "3\.$Current\.\d+"
$currentNuget = [regex]::Match($content, $nugetPattern).Value
if (-not $currentNuget) {
    Write-Error "Could not find current nuget version pattern 3.$Current.x in VERSIONS.txt"
}
Write-Host "  Current nuget version: $currentNuget"

# Replace all version patterns
$content = $content -replace "release\s+m$Current", "release     m$Target"
$content = $content -replace "milestone\s+$Current", "milestone   $Target"
$content = $content -replace "increment\s+\d+", "increment   0"
$content = $content -replace "$Current\.0\.0", "$Target.0.0"
$content = $content -replace "3\.$Current\.\d+\.0", "3.$Target.0.0"
$content = $content -replace "3\.$Current\.\d+", "3.$Target.0"

Set-Content $versionsPath $content -NoNewline
Write-Host "  Updated VERSIONS.txt" -ForegroundColor Green

# --- Step 2: cgmanifest.json ---
Write-Host "`n--- Updating cgmanifest.json ---" -ForegroundColor Yellow
$cgPath = Join-Path $repoRoot 'cgmanifest.json'
$cgContent = Get-Content $cgPath -Raw

# Get submodule commit hash
$submoduleHash = git -C (Join-Path $repoRoot 'externals/skia') rev-parse HEAD
Write-Host "  Submodule HEAD: $submoduleHash"

# Get upstream merge commit
$upstreamHash = git -C (Join-Path $repoRoot 'externals/skia') rev-parse "upstream/chrome/m$Target" 2>$null
if (-not $upstreamHash) {
    Write-Warning "  Could not resolve upstream/chrome/m$Target - set upstream_merge_commit manually"
    $upstreamHash = "UNKNOWN"
} else {
    Write-Host "  Upstream m$Target tip: $upstreamHash"
}

# Update cgmanifest.json fields
$cgJson = $cgContent | ConvertFrom-Json

foreach ($reg in $cgJson.registrations) {
    # Update git component (commitHash)
    if ($reg.component.type -eq 'git' -and $reg.component.git.repositoryUrl -match 'mono/skia') {
        $reg.component.git.commitHash = $submoduleHash
        Write-Host "  Updated commitHash" -ForegroundColor Green
    }
    # Update skia other component (version, chrome_milestone, upstream_merge_commit)
    if ($reg.component.type -eq 'other' -and $reg.component.other.name -eq 'skia') {
        $reg.component.other.version = "chrome/m$Target"
    }
    # Handle custom fields at registration level
    if ($null -ne $reg.chrome_milestone) {
        $reg.chrome_milestone = $Target
        Write-Host "  Updated chrome_milestone" -ForegroundColor Green
    }
    if ($null -ne $reg.upstream_merge_commit) {
        $reg.upstream_merge_commit = $upstreamHash
        Write-Host "  Updated upstream_merge_commit" -ForegroundColor Green
    }
}

$cgJson | ConvertTo-Json -Depth 10 | Set-Content $cgPath
Write-Host "  Updated cgmanifest.json" -ForegroundColor Green

# --- Step 3: azure-pipelines-variables.yml ---
$pipelinePath = Join-Path $repoRoot 'scripts/azure-pipelines-variables.yml'
if (Test-Path $pipelinePath) {
    Write-Host "`n--- Updating azure-pipelines-variables.yml ---" -ForegroundColor Yellow
    $pipelineContent = Get-Content $pipelinePath -Raw
    $pipelineContent = $pipelineContent -replace "$Current", "$Target"
    Set-Content $pipelinePath $pipelineContent -NoNewline
    Write-Host "  Updated azure-pipelines-variables.yml" -ForegroundColor Green
} else {
    Write-Host "`n--- scripts/azure-pipelines-variables.yml not found (skipping) ---" -ForegroundColor DarkGray
}

# --- Step 4: Verify SK_C_INCREMENT ---
Write-Host "`n--- Verifying SK_C_INCREMENT ---" -ForegroundColor Yellow
$skTypesPath = Join-Path $repoRoot 'externals/skia/include/c/sk_types.h'
$incrementLine = Select-String -Path $skTypesPath -Pattern 'SK_C_INCREMENT'
if ($incrementLine -match 'SK_C_INCREMENT\s+0') {
    Write-Host "  SK_C_INCREMENT is 0" -ForegroundColor Green
} else {
    Write-Error "SK_C_INCREMENT is NOT 0 — reset it to 0 in $skTypesPath"
}

# --- Step 5: Mandatory Verification ---
Write-Host "`n=== Verification ===" -ForegroundColor Cyan

$staleVersions = Select-String -Path $versionsPath -Pattern "$Current" |
    Where-Object { $_.Line -notmatch '^\s*#' -and $_.Line -notmatch 'HarfBuzz' }

$staleCgmanifest = Select-String -Path $cgPath -Pattern "m$Current|`"$Current`""

$failures = @()

if ($staleVersions) {
    $failures += "VERSIONS.txt still contains '$Current':"
    foreach ($match in $staleVersions) {
        $failures += "  Line $($match.LineNumber): $($match.Line.Trim())"
    }
}

if ($staleCgmanifest) {
    $failures += "cgmanifest.json still contains '$Current':"
    foreach ($match in $staleCgmanifest) {
        $failures += "  Line $($match.LineNumber): $($match.Line.Trim())"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "`n❌ GATE FAILED — stale references found:" -ForegroundColor Red
    foreach ($f in $failures) { Write-Host "  $f" -ForegroundColor Red }
    exit 1
} else {
    Write-Host "`n✅ GATE PASSED — no stale milestone references found" -ForegroundColor Green
    Write-Host "  SK_C_INCREMENT: 0" -ForegroundColor Green
    Write-Host "  VERSIONS.txt: clean" -ForegroundColor Green
    Write-Host "  cgmanifest.json: clean" -ForegroundColor Green
}
