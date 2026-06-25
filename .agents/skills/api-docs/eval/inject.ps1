#!/usr/bin/env pwsh
# =====================================================================
# inject.ps1 - seed known defects into COPIES of the eval fixtures
# ---------------------------------------------------------------------
# Reads eval/mutations.json, copies each fixture into a gitignored output
# tree, applies every find->replace mutation, and writes an answer key.
# Safety: it NEVER writes under docs/ and NEVER edits the fixtures in place.
# =====================================================================
[CmdletBinding()]
param(
    [string]$OutputRoot
)
$ErrorActionPreference = 'Stop'

$EvalDir   = Split-Path -Parent $PSCommandPath
$MutPath   = Join-Path $EvalDir "mutations.json"
$RepoRoot  = (Resolve-Path "$EvalDir/../../../..").Path

if (-not $OutputRoot) { $OutputRoot = Join-Path $RepoRoot "output/eval" }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)

# --- Safety: refuse to write into the real docs submodule -------------
$docsAbs = [IO.Path]::GetFullPath((Join-Path $RepoRoot "docs"))
if ($OutputRoot.StartsWith($docsAbs, [StringComparison]::OrdinalIgnoreCase)) {
    Write-Error "REFUSING to write eval output under docs/ ($OutputRoot). Pick a path outside the submodule."
    exit 1
}

$mut = Get-Content $MutPath -Raw | ConvertFrom-Json
$fixturesRoot = Join-Path $EvalDir $mut.fixturesRoot

$corruptDir = Join-Path $OutputRoot "corrupted"
$controlDir = Join-Path $OutputRoot "controls"
foreach ($d in @($corruptDir, $controlDir)) {
    if (Test-Path $d) { Remove-Item $d -Recurse -Force }
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

function Copy-Fixture([string]$rel, [string]$destRoot) {
    $src = Join-Path $fixturesRoot $rel
    if (-not (Test-Path $src)) { Write-Error "fixture not found: $src"; exit 1 }
    $dest = Join-Path $destRoot $rel
    New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force | Out-Null
    Copy-Item $src $dest -Force
    return $dest
}

# --- Controls: pristine copies (any finding here is a false positive) -
foreach ($rel in $mut.controls) { [void](Copy-Fixture $rel $controlDir) }

# --- Corrupted: copy then apply every mutation for that file ----------
$answerKey = @()
$byFile = $mut.mutations | Group-Object file
foreach ($grp in $byFile) {
    $rel  = $grp.Name
    $dest = Copy-Fixture $rel $corruptDir
    $text = Get-Content $dest -Raw

    foreach ($m in $grp.Group) {
        $occurrences = ([regex]::Matches($text, [regex]::Escape($m.find))).Count
        if ($occurrences -ne 1) {
            Write-Error "mutation $($m.id): 'find' must occur exactly once in $rel (found $occurrences). Fix the fixture or the mutation."
            exit 1
        }
        $idx = $text.IndexOf($m.find)
        $text = $text.Substring(0, $idx) + $m.replace + $text.Substring($idx + $m.find.Length)

        $answerKey += [pscustomobject]@{
            id       = $m.id
            fileBase = [IO.Path]::GetFileName($rel)
            docId    = $m.docId
            field    = $m.field
            class    = $m.class
            detector = $m.detector
        }
    }

    Set-Content $dest $text -NoNewline

    # Corrupted file must stay well-formed; otherwise the linter only sees
    # malformed-xml and the seeded defect can never be matched.
    try { [void]([xml](Get-Content $dest -Raw)) }
    catch { Write-Error "mutation set for $rel produced malformed XML: $($_.Exception.Message)"; exit 1 }
}

$akPath = Join-Path $OutputRoot "answer-key.json"
$answerKey | ConvertTo-Json -Depth 5 | Set-Content $akPath

$lintCount = ($answerKey | Where-Object detector -eq 'lint').Count
$llmCount  = ($answerKey | Where-Object detector -eq 'llm').Count
Write-Host "INJECT | corrupted:$($byFile.Count) files | controls:$($mut.controls.Count) | defects:$($answerKey.Count) (lint:$lintCount llm:$llmCount)"
Write-Host "INJECT | output: $OutputRoot"
Write-Host "INJECT | answer-key: $akPath"
