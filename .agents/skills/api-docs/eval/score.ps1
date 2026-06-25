#!/usr/bin/env pwsh
# =====================================================================
# score.ps1 - score reviewer/linter findings against the seeded answer key
# ---------------------------------------------------------------------
# Findings are read from a file of contract lines:
#     SEVERITY | class | file | docId | message
# Lines whose file path contains '/controls/' are treated as control
# findings (any of them is a false positive, since controls are pristine).
# Lines on '/corrupted/' files are matched to the answer key by
# (file basename, docId, class). Produces a JSON + Markdown scorecard.
# =====================================================================
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$Findings,
    [string]$AnswerKey,
    [string]$OutDir,
    [ValidateSet('lint', 'llm', 'all')] [string]$Detector = 'all',
    [double]$MinRecall = 0.0,
    [double]$MinPrecision = 0.0
)
$ErrorActionPreference = 'Stop'

$EvalDir  = Split-Path -Parent $PSCommandPath
$RepoRoot = (Resolve-Path "$EvalDir/../../../..").Path
if (-not $AnswerKey) { $AnswerKey = Join-Path $RepoRoot "output/eval/answer-key.json" }
if (-not $OutDir)    { $OutDir    = Join-Path $RepoRoot "output/eval/report" }
New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

$key = @(Get-Content $AnswerKey -Raw | ConvertFrom-Json)
if ($Detector -ne 'all') { $key = @($key | Where-Object detector -eq $Detector) }

# --- Parse contract lines --------------------------------------------
$parsed = @()
foreach ($line in Get-Content $Findings) {
    if ($line -notmatch '^\s*(CRITICAL|IMPORTANT|MINOR)\s*\|') { continue }
    $parts = $line.Split('|') | ForEach-Object { $_.Trim() }
    if ($parts.Count -lt 4) { continue }
    $file = $parts[2]
    $parsed += [pscustomobject]@{
        sev       = $parts[0]
        class     = $parts[1]
        file      = $file
        fileBase  = [IO.Path]::GetFileName($file)
        docId     = $parts[3]
        isControl = ($file -match '[\\/]controls[\\/]')
    }
}

$controlFindings = @($parsed | Where-Object isControl)
$corruptFindings = @($parsed | Where-Object { -not $_.isControl })

# --- Match answer-key entries ----------------------------------------
$usedFinding = New-Object 'System.Collections.Generic.HashSet[int]'
$results = foreach ($k in $key) {
    $matchIdx = -1
    for ($i = 0; $i -lt $corruptFindings.Count; $i++) {
        if ($usedFinding.Contains($i)) { continue }
        $f = $corruptFindings[$i]
        if ($f.fileBase -eq $k.fileBase -and $f.docId -eq $k.docId -and $f.class -eq $k.class) {
            $matchIdx = $i; break
        }
    }
    if ($matchIdx -ge 0) { [void]$usedFinding.Add($matchIdx) }
    [pscustomobject]@{ id = $k.id; class = $k.class; detector = $k.detector; docId = $k.docId; matched = ($matchIdx -ge 0) }
}

$extra = for ($i = 0; $i -lt $corruptFindings.Count; $i++) {
    if (-not $usedFinding.Contains($i)) { $corruptFindings[$i] }
}

# --- Metrics ----------------------------------------------------------
$total    = $results.Count
$matched  = @($results | Where-Object matched).Count
$recall   = if ($total) { [math]::Round($matched / $total, 4) } else { 1.0 }
$ctrlFP   = $controlFindings.Count
$precDen  = $matched + $ctrlFP
$precision = if ($precDen) { [math]::Round($matched / $precDen, 4) } else { 1.0 }

$byClass = $results | Group-Object class | ForEach-Object {
    $m = @($_.Group | Where-Object matched).Count
    [pscustomobject]@{ class = $_.Name; matched = $m; total = $_.Count; recall = [math]::Round($m / $_.Count, 4) }
}
$byDetector = $results | Group-Object detector | ForEach-Object {
    $m = @($_.Group | Where-Object matched).Count
    [pscustomobject]@{ detector = $_.Name; matched = $m; total = $_.Count; recall = [math]::Round($m / $_.Count, 4) }
}

$report = [pscustomobject]@{
    detectorFilter = $Detector
    recall         = $recall
    precision      = $precision
    matched        = $matched
    total          = $total
    controlFP      = $ctrlFP
    extra          = $extra.Count
    byClass        = $byClass
    byDetector     = $byDetector
    missed         = @($results | Where-Object { -not $_.matched } | Select-Object id, class, detector, docId)
    controlFindings = @($controlFindings | Select-Object class, fileBase, docId)
    extraFindings  = @($extra | Select-Object class, fileBase, docId)
}

$jsonPath = Join-Path $OutDir "scorecard.json"
$report | ConvertTo-Json -Depth 6 | Set-Content $jsonPath

# --- Markdown scorecard ----------------------------------------------
$md = @()
$md += "# api-docs eval scorecard"
$md += ""
$md += "- **Detector filter:** ``$Detector``"
$md += "- **Recall:** $matched / $total = **$recall**"
$md += "- **Precision (vs controls):** **$precision**  (control false positives: $ctrlFP)"
$md += "- **Extra (unmatched, on corrupted files):** $($extra.Count)"
$md += ""
$md += "## Recall by class"
$md += "| class | matched | total | recall |"
$md += "|---|---|---|---|"
foreach ($c in ($byClass | Sort-Object class)) { $md += "| $($c.class) | $($c.matched) | $($c.total) | $($c.recall) |" }
$md += ""
$md += "## Recall by detector"
$md += "| detector | matched | total | recall |"
$md += "|---|---|---|---|"
foreach ($d in ($byDetector | Sort-Object detector)) { $md += "| $($d.detector) | $($d.matched) | $($d.total) | $($d.recall) |" }
if ($report.missed.Count) {
    $md += ""; $md += "## Missed defects"
    foreach ($m in $report.missed) { $md += "- ``$($m.id)`` ($($m.class), $($m.detector)) — $($m.docId)" }
}
if ($ctrlFP) {
    $md += ""; $md += "## Control false positives"
    foreach ($c in $report.controlFindings) { $md += "- $($c.class) — $($c.fileBase) — $($c.docId)" }
}
if ($extra.Count) {
    $md += ""; $md += "## Extra findings (unmatched, on corrupted files)"
    foreach ($e in $report.extraFindings) { $md += "- $($e.class) — $($e.fileBase) — $($e.docId)" }
}
$mdPath = Join-Path $OutDir "scorecard.md"
$md -join "`n" | Set-Content $mdPath

Write-Host "SCORE | detector:$Detector | recall:$matched/$total=$recall | precision:$precision | controlFP:$ctrlFP | extra:$($extra.Count)"
Write-Host "SCORE | report: $mdPath"

$fail = $false
if ($recall -lt $MinRecall) { Write-Host "SCORE | FAIL recall $recall < $MinRecall"; $fail = $true }
if ($precision -lt $MinPrecision) { Write-Host "SCORE | FAIL precision $precision < $MinPrecision"; $fail = $true }
if ($fail) { exit 1 }
