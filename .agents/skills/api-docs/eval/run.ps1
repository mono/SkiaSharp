#!/usr/bin/env pwsh
# =====================================================================
# run.ps1 - one-command deterministic self-test of the eval harness
# ---------------------------------------------------------------------
# inject seeded defects -> lint corrupted + controls -> score the lint
# detector subset. Gates on full recall and precision so a regression in
# docs-tool.ps1 (a check that stops firing, or a new false positive)
# fails CI. The LLM-detector classes are scored separately during the
# model bake-off (see README.md) and are not part of this gate.
# =====================================================================
[CmdletBinding()]
param(
    [double]$MinRecall = 1.0,
    [double]$MinPrecision = 1.0
)
$ErrorActionPreference = 'Stop'

$EvalDir  = Split-Path -Parent $PSCommandPath
$RepoRoot = (Resolve-Path "$EvalDir/../../../..").Path
$Tool     = Join-Path $EvalDir "../scripts/docs-tool.ps1"
$OutRoot  = Join-Path $RepoRoot "output/eval"
$FindFile = Join-Path $OutRoot "lint-findings.txt"

& "$EvalDir/inject.ps1"

# Lint corrupted + controls into a single findings file (paths disambiguate).
$lines = @()
$lines += & $Tool lint (Join-Path $OutRoot "corrupted")
$lines += & $Tool lint (Join-Path $OutRoot "controls")
$lines | Set-Content $FindFile

& "$EvalDir/score.ps1" -Findings $FindFile -Detector lint -MinRecall $MinRecall -MinPrecision $MinPrecision
exit $LASTEXITCODE
