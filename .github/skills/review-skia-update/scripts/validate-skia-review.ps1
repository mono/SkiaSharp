<#
.SYNOPSIS Validate a skia-review JSON file against skia-review-schema.json.
.EXAMPLE  pwsh scripts/validate-skia-review.ps1 /tmp/skiasharp/skia-review/20260320-164500/170.json
.NOTES    Exits 0=valid, 1=fixable (retry), 2=fatal. Requires PowerShell 7.4+.
#>
#requires -Version 7.4
param([Parameter(Mandatory, Position = 0)] [string]$Path)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }
$schemaPath = Join-Path $PSScriptRoot '../references/skia-review-schema.json'
if (-not (Test-Path $schemaPath)) { Write-Host "❌ Schema not found: $schemaPath"; exit 2 }

$json = Get-Content $Path -Raw
$review = $json | ConvertFrom-Json -Depth 50
$errors = @()

# --- Schema validation ---
if (-not ($json | Test-Json -SchemaFile $schemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue)) {
    $errors += $schemaErrors | ForEach-Object {
        $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
    } | Sort-Object -Unique
}

# --- Logical checks ---

# Top-level summary and recommendations
if (-not $review.summary -or $review.summary.Length -lt 50) {
    $errors += "Top-level summary is too short (need 50+ chars)"
}
if (-not $review.recommendations -or $review.recommendations.Count -eq 0) {
    $errors += "Top-level recommendations array is empty (need at least 1)"
}

# Per-section summary and recommendations
foreach ($section in @('generatedFiles', 'upstreamIntegrity', 'interopIntegrity', 'depsAudit')) {
    if (-not $review.$section.summary) {
        $errors += "$section.summary is missing or empty"
    }
    if (-not $review.$section.recommendations) {
        $errors += "$section.recommendations is missing"
    }
}

# REVIEW_REQUIRED sections must have at least one item
foreach ($section in @('upstreamIntegrity', 'interopIntegrity', 'depsAudit')) {
    if ($review.$section.status -eq 'REVIEW_REQUIRED') {
        $hasItems = ($review.$section.added -and $review.$section.added.Count -gt 0) -or
                    ($review.$section.removed -and $review.$section.removed.Count -gt 0) -or
                    ($review.$section.changed -and $review.$section.changed.Count -gt 0)
        if (-not $hasItems) {
            $errors += "$section.status is REVIEW_REQUIRED but added/removed/changed are all empty"
        }
    }
}

# Source file items must have path and summary; diff fields must contain actual content
$fileRefPattern = '^see '
foreach ($section in @('upstreamIntegrity', 'interopIntegrity')) {
    foreach ($category in @('added', 'removed', 'changed')) {
        $items = $review.$section.$category
        if ($items) {
            foreach ($item in $items) {
                if (-not $item.path) { $errors += "$section.$category item missing 'path'" }
                if (-not $item.summary) { $errors += "$section.$category item '$($item.path)' missing 'summary'" }
                # Diff fields must contain actual diff content, not file references
                foreach ($diffField in @('diff', 'oldDiff', 'newDiff', 'patchDiff')) {
                    $val = $item.$diffField
                    if ($val -and $val -match $fileRefPattern) {
                        $errors += "$section.$category '$($item.path)' $diffField contains a file reference instead of actual diff content"
                    }
                }
            }
        }
    }
}

# Dep items must have name and summary
foreach ($category in @('added', 'removed', 'changed')) {
    $items = $review.depsAudit.$category
    if ($items) {
        foreach ($item in $items) {
            if (-not $item.name) { $errors += "depsAudit.$category item missing 'name'" }
            if (-not $item.summary) { $errors += "depsAudit.$category item '$($item.name)' missing 'summary'" }
        }
    }
}

# Risk assessment consistency
$expectedHigh = ($review.generatedFiles.status -eq 'FAIL') -or
                ($review.upstreamIntegrity.status -eq 'REVIEW_REQUIRED')
if ($expectedHigh -and $review.riskAssessment -ne 'HIGH') {
    $errors += "riskAssessment should be HIGH (generated=$($review.generatedFiles.status), upstream=$($review.upstreamIntegrity.status))"
}

$expectedMedium = ($review.depsAudit.status -eq 'REVIEW_REQUIRED') -or
                  ($review.interopIntegrity.status -eq 'REVIEW_REQUIRED')
if (-not $expectedHigh -and $expectedMedium -and $review.riskAssessment -eq 'LOW') {
    $errors += "riskAssessment should be MEDIUM or HIGH, not LOW"
}

# If all sections PASS, riskAssessment must be LOW
$allPass = ($review.generatedFiles.status -eq 'PASS') -and
           ($review.upstreamIntegrity.status -eq 'PASS') -and
           ($review.interopIntegrity.status -eq 'PASS') -and
           ($review.depsAudit.status -eq 'PASS')
if ($allPass -and $review.riskAssessment -ne 'LOW') {
    $errors += "riskAssessment should be LOW when all sections are PASS"
}

# No absolute paths
$absPathPattern = '(/Users/|/home/|C:\\Users\\)'
if ($json -match $absPathPattern) {
    $matches = [regex]::Matches($json, '"[^"]*(?:/Users/|/home/|C:\\Users\\)[^"]*"')
    foreach ($m in $matches) {
        $errors += "Absolute path found: $($m.Value)"
    }
}

# SHA format
foreach ($shaField in @('prHead', 'base', 'upstream')) {
    $sha = $review.meta.shas.$shaField
    if ($sha -and $sha -notmatch '^[0-9a-f]{40}$') {
        $errors += "meta.shas.$shaField is not a valid 40-char hex SHA: $sha"
    }
}

if ($errors.Count -eq 0) {
    $pr = $review.meta.skiaPrNumber ?? '?'
    $risk = $review.riskAssessment ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (PR #$pr, risk: $risk)"
    exit 0
}
Write-Host "❌ $($errors.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$errors | ForEach-Object { Write-Host "  $_" }
exit 1
