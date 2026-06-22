#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Apply SkiaSharp release version edits: set the prerelease label and/or bump
    the package versions on an integration branch.

.DESCRIPTION
    Applies only the version edits a SkiaSharp release needs. It does NOT touch
    milestone/soname/increment values, cgmanifest.json, or sk_types.h — only the
    two files a release version change needs:

    - scripts/azure-templates-variables.yml
        * SKIASHARP_VERSION   (when -SkiaSharpVersion is given)
        * PREVIEW_LABEL       (when -PreviewLabel is given)
    - scripts/VERSIONS.txt    (only when bumping versions)
        * SkiaSharp    file   -> {SkiaSharpVersion}.0
        * SkiaSharp*   nuget  -> {SkiaSharpVersion}
        * HarfBuzzSharp file  -> {HarfBuzzSharpVersion}
        * HarfBuzzSharp* nuget-> {HarfBuzzSharpVersion}

    Assembly versions are NEVER changed (they are intentionally pinned to
    major.minor.0.0 / 1.0.0.0 and must stay stable across patch releases).

    Two independent operations, usable together or alone:
      1. Set label only (Step 3 — create release branch):
             -PreviewLabel stable
      2. Bump versions (Step 5 — bump integration branch after a release):
             -SkiaSharpVersion 4.148.1 -HarfBuzzSharpVersion 14.2.1 -PreviewLabel preview.0

    A bump requires BOTH -SkiaSharpVersion and -HarfBuzzSharpVersion. The caller
    decides the exact HarfBuzzSharp version (4-digit X.Y.Z.N normally; reset to
    3-digit on a native HarfBuzz upgrade), so this script never guesses it.

.PARAMETER SkiaSharpVersion
    Target SkiaSharp nuget version, e.g. 4.148.1. Requires -HarfBuzzSharpVersion.

.PARAMETER HarfBuzzSharpVersion
    Target HarfBuzzSharp nuget version, e.g. 14.2.1 or 8.3.1.7. Requires
    -SkiaSharpVersion.

.PARAMETER PreviewLabel
    Value for PREVIEW_LABEL: 'stable', 'preview.N', 'rc.N', or 'preview.0'.

.PARAMETER DryRun
    Show the planned changes and run the verification gate against the would-be
    result, but do not write any files.

.EXAMPLE
    # Step 3 — cut a stable release branch
    pwsh .agents/skills/release-branch/scripts/bump-version.ps1 -PreviewLabel stable

.EXAMPLE
    # Step 5 — bump the integration branch to the next patch after a release
    pwsh .agents/skills/release-branch/scripts/bump-version.ps1 `
        -SkiaSharpVersion 4.148.1 -HarfBuzzSharpVersion 14.2.1 -PreviewLabel preview.0
#>

param(
    [string]$SkiaSharpVersion,
    [string]$HarfBuzzSharpVersion,
    [string]$PreviewLabel,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$repoRoot = git rev-parse --show-toplevel

$doBump  = $PSBoundParameters.ContainsKey('SkiaSharpVersion') -or $PSBoundParameters.ContainsKey('HarfBuzzSharpVersion')
$doLabel = $PSBoundParameters.ContainsKey('PreviewLabel')

if (-not $doBump -and -not $doLabel) {
    Write-Error "Nothing to do. Provide -PreviewLabel and/or (-SkiaSharpVersion AND -HarfBuzzSharpVersion)."
}
if ($doBump -and (-not $SkiaSharpVersion -or -not $HarfBuzzSharpVersion)) {
    Write-Error "A version bump requires BOTH -SkiaSharpVersion and -HarfBuzzSharpVersion."
}

# --- Validate formats -------------------------------------------------------
if ($doBump) {
    if ($SkiaSharpVersion -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "SkiaSharpVersion '$SkiaSharpVersion' must be X.Y.Z (e.g. 4.148.1)."
    }
    if ($HarfBuzzSharpVersion -notmatch '^\d+(\.\d+){2,3}$') {
        Write-Error "HarfBuzzSharpVersion '$HarfBuzzSharpVersion' must be 3- or 4-part (e.g. 14.2.1 or 8.3.1.7)."
    }
}
if ($doLabel -and $PreviewLabel -notmatch '^(stable|preview\.\d+|rc\.\d+)$') {
    Write-Error "PreviewLabel '$PreviewLabel' must be 'stable', 'preview.N', or 'rc.N'."
}

$versionsPath = Join-Path $repoRoot 'scripts/VERSIONS.txt'
$pipelinePath = Join-Path $repoRoot 'scripts/azure-templates-variables.yml'
foreach ($p in @($versionsPath, $pipelinePath)) {
    if (-not (Test-Path $p)) {
        Write-Error "Expected file not found: $p. If it was renamed, update this script."
    }
}

$mode = if ($DryRun) { ' (dry run)' } else { '' }
Write-Host "`n=== Release version update$mode ===" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# scripts/VERSIONS.txt
# ---------------------------------------------------------------------------
$assemblyBefore = $null
if ($doBump) {
    Write-Host "`n--- scripts/VERSIONS.txt ---" -ForegroundColor Yellow
    $lines = Get-Content $versionsPath
    $assemblyBefore = ($lines | Where-Object { $_ -match '^SkiaSharp\s+assembly\s' })

    $skiaFileVer = "$SkiaSharpVersion.0"
    $newLines = foreach ($line in $lines) {
        if     ($line -match '^(SkiaSharp\s+file\s+)\S+\s*$')         { ($Matches[1] + $skiaFileVer) }
        elseif ($line -match '^(SkiaSharp\S*\s+nuget\s+)\S+\s*$')     { ($Matches[1] + $SkiaSharpVersion) }
        elseif ($line -match '^(HarfBuzzSharp\s+file\s+)\S+\s*$')     { ($Matches[1] + $HarfBuzzSharpVersion) }
        elseif ($line -match '^(HarfBuzzSharp\S*\s+nuget\s+)\S+\s*$') { ($Matches[1] + $HarfBuzzSharpVersion) }
        else { $line }
    }

    if ($DryRun) {
        $diff = Compare-Object $lines $newLines | Where-Object SideIndicator -eq '=>'
        $diff | ForEach-Object { Write-Host "  + $($_.InputObject)" -ForegroundColor DarkGray }
    } else {
        Set-Content $versionsPath $newLines
    }
    Write-Host "  SkiaSharp     -> $SkiaSharpVersion (file $skiaFileVer)" -ForegroundColor Green
    Write-Host "  HarfBuzzSharp -> $HarfBuzzSharpVersion" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# scripts/azure-templates-variables.yml
# ---------------------------------------------------------------------------
Write-Host "`n--- scripts/azure-templates-variables.yml ---" -ForegroundColor Yellow
$pipeline = Get-Content $pipelinePath -Raw
if ($doBump) {
    $pipeline = [regex]::Replace($pipeline, '(?m)^(\s*SKIASHARP_VERSION:\s*)\S+\s*$', "`${1}$SkiaSharpVersion")
    Write-Host "  SKIASHARP_VERSION -> $SkiaSharpVersion" -ForegroundColor Green
}
if ($doLabel) {
    $pipeline = [regex]::Replace($pipeline, "(?m)^(\s*PREVIEW_LABEL:\s*).*$", "`${1}'$PreviewLabel'")
    Write-Host "  PREVIEW_LABEL -> '$PreviewLabel'" -ForegroundColor Green
}
if (-not $DryRun) {
    Set-Content $pipelinePath $pipeline -NoNewline
}

# ---------------------------------------------------------------------------
# Verification gate
# ---------------------------------------------------------------------------
Write-Host "`n=== Verification ===" -ForegroundColor Cyan
$failures = @()

# Re-read effective content (in-memory for dry run, from disk otherwise).
if ($DryRun) {
    $versionsNow = if ($doBump) { $newLines } else { Get-Content $versionsPath }
    $pipelineNow = $pipeline
} else {
    $versionsNow = Get-Content $versionsPath
    $pipelineNow = Get-Content $pipelinePath -Raw
}

if ($doBump) {
    $skiaFileVer = "$SkiaSharpVersion.0"

    $fileLine = $versionsNow | Where-Object { $_ -match '^SkiaSharp\s+file\s+(\S+)' } | Select-Object -First 1
    if ($fileLine -notmatch "^SkiaSharp\s+file\s+$([regex]::Escape($skiaFileVer))\s*$") {
        $failures += "SkiaSharp file version is not '$skiaFileVer': $fileLine"
    }

    $hbFileLine = $versionsNow | Where-Object { $_ -match '^HarfBuzzSharp\s+file\s+(\S+)' } | Select-Object -First 1
    if ($hbFileLine -notmatch "^HarfBuzzSharp\s+file\s+$([regex]::Escape($HarfBuzzSharpVersion))\s*$") {
        $failures += "HarfBuzzSharp file version is not '$HarfBuzzSharpVersion': $hbFileLine"
    }

    $badSkia = $versionsNow | Where-Object { $_ -match '^SkiaSharp\S*\s+nuget\s+(\S+)' -and $Matches[1] -ne $SkiaSharpVersion }
    foreach ($b in $badSkia) { $failures += "Stale SkiaSharp nuget line: $($b.Trim())" }

    $badHb = $versionsNow | Where-Object { $_ -match '^HarfBuzzSharp\S*\s+nuget\s+(\S+)' -and $Matches[1] -ne $HarfBuzzSharpVersion }
    foreach ($b in $badHb) { $failures += "Stale HarfBuzzSharp nuget line: $($b.Trim())" }

    # Assembly versions must be untouched.
    $assemblyAfter = ($versionsNow | Where-Object { $_ -match '^SkiaSharp\s+assembly\s' })
    if ($assemblyBefore -and ($assemblyAfter -ne $assemblyBefore)) {
        $failures += "SkiaSharp assembly line changed (must stay pinned): '$assemblyAfter'"
    }

    $pipeVer = [regex]::Match($pipelineNow, '(?m)^\s*SKIASHARP_VERSION:\s*(?<v>\S+)\s*$').Groups['v'].Value
    if ($pipeVer -ne $SkiaSharpVersion) {
        $failures += "azure-templates-variables.yml SKIASHARP_VERSION is '$pipeVer', expected '$SkiaSharpVersion'."
    }
}

if ($doLabel) {
    $labelNow = [regex]::Match($pipelineNow, "(?m)^\s*PREVIEW_LABEL:\s*'?(?<l>[^'\r\n]*?)'?\s*$").Groups['l'].Value
    if ($labelNow -ne $PreviewLabel) {
        $failures += "azure-templates-variables.yml PREVIEW_LABEL is '$labelNow', expected '$PreviewLabel'."
    }
}

if ($failures.Count -gt 0) {
    Write-Host "`n❌ GATE FAILED:" -ForegroundColor Red
    foreach ($f in $failures) { Write-Host "  $f" -ForegroundColor Red }
    exit 1
}

Write-Host "`n✅ GATE PASSED" -ForegroundColor Green
if ($doBump)  {
    Write-Host "  SkiaSharp nuget/file = $SkiaSharpVersion / $SkiaSharpVersion.0" -ForegroundColor Green
    Write-Host "  HarfBuzzSharp nuget/file = $HarfBuzzSharpVersion" -ForegroundColor Green
    Write-Host "  SKIASHARP_VERSION = $SkiaSharpVersion (assembly versions unchanged)" -ForegroundColor Green
}
if ($doLabel) { Write-Host "  PREVIEW_LABEL = '$PreviewLabel'" -ForegroundColor Green }
if ($DryRun)  { Write-Host "`n(dry run — no files written)" -ForegroundColor Yellow }
