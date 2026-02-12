<#
.SYNOPSIS Validate an ai-triage JSON file against triage-schema.json.
.EXAMPLE  pwsh scripts/validate-triage.ps1 ai-triage/2794.json
.NOTES    Exits 0=valid, 1=fixable (retry), 2=fatal. Requires PowerShell 7.5+.
#>
#requires -Version 7.5
param([Parameter(Mandatory, Position = 0)] [string]$Path)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }
$schemaPath = Join-Path $PSScriptRoot '../references/triage-schema.json'
if (-not (Test-Path $schemaPath)) { Write-Host "❌ Schema not found: $schemaPath"; exit 2 }

$json = Get-Content $Path -Raw
$triage = $json | ConvertFrom-Json -Depth 50
$errors = @()

# --- Schema validation ---
if (-not ($json | Test-Json -SchemaFile $schemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue)) {
    $errors += $schemaErrors | ForEach-Object {
        $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
    } | Sort-Object -Unique
}

# --- Extra checks beyond schema ---

# codeInvestigation should have entries for bugs (warning only — mechanical conversions may lack this)
if ($triage.classification.type.value -eq 'type/bug' -and
    (-not $triage.analysis.codeInvestigation -or $triage.analysis.codeInvestigation.Count -eq 0)) {
    Write-Host "⚠️  Warning: Bug issue has no codeInvestigation entries (recommended for bugs)"
}

# bugSignals should exist for bugs (warning only)
if ($triage.classification.type.value -eq 'type/bug' -and -not $triage.evidence.bugSignals) {
    Write-Host "⚠️  Warning: Bug issue has no evidence.bugSignals (recommended for bugs)"
}

# Absolute path check in codeInvestigation
$absPathPattern = '(/Users/|/home/|C:\\Users\\)'
if ($triage.analysis.codeInvestigation) {
    foreach ($ci in $triage.analysis.codeInvestigation) {
        if ($ci.file -match $absPathPattern) {
            $errors += "codeInvestigation file '$($ci.file)' contains absolute path — use relative path from repo root"
        }
    }
}

if ($errors.Count -eq 0) {
    $number = $triage.meta.number ?? '?'
    $type = $triage.classification.type.value ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (issue #$number, type: $type)"
    exit 0
}
Write-Host "❌ $($errors.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$errors | ForEach-Object { Write-Host "  $_" }
exit 1
