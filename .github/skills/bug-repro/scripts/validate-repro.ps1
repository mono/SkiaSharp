<#
.SYNOPSIS Validate a bug-repro JSON file against repro-schema.json (v1.0).
.EXAMPLE  pwsh scripts/validate-repro.ps1 ai-repro/2997.json
.NOTES    Exits 0=valid, 1=fixable (retry), 2=fatal. Requires PowerShell 7.5+.
#>
#requires -Version 7.5
param([Parameter(Mandatory, Position = 0)] [string]$Path)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }
$schemaPath = Join-Path $PSScriptRoot '../references/repro-schema.json'
if (-not (Test-Path $schemaPath)) { Write-Host "❌ Schema not found: $schemaPath"; exit 2 }

$json = Get-Content $Path -Raw
$repro = $json | ConvertFrom-Json -Depth 50
$errors = @()

# --- Schema validation (suppress oneOf null-branch noise) ---
if (-not ($json | Test-Json -SchemaFile $schemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue)) {
    $errors += $schemaErrors | ForEach-Object {
        $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
    } | Where-Object {
        $_ -notmatch 'should be "null"' -and
        $_ -notmatch 'is not valid under any of the given schemas' -and
        $_ -notmatch "Expected.*at '/conclusion'" -and
        $_ -notmatch "Expected.*at '/reproductionSteps/\d+/result'" -and
        $_ -notmatch 'should contain at least 1 matching items' -and
        $_ -notmatch "match one of the values specified by the enum at '/conclusion'"
    } | Sort-Object -Unique
}

# --- Extra checks beyond schema ---
$steps = $repro.reproductionSteps
$absPathPattern = '(/Users/|/home/|C:\\Users\\)'
if ($steps) {
    for ($i = 0; $i -lt $steps.Count; $i++) {
        if ($steps[$i].stepNumber -ne ($i + 1)) {
            $errors += "Step [$i] has stepNumber $($steps[$i].stepNumber), expected $($i + 1)"
        }
    }
    foreach ($s in $steps) {
        $n = $s.stepNumber
        if ($s.output -and $s.output.Length -gt 4096) {
            $errors += "Step $n output is $($s.output.Length) chars (max 4096)"
        }
        if ($s.output -and $s.output -match $absPathPattern) {
            $errors += "Step $n output contains absolute path — redact usernames"
        }
        if ($s.command -and $s.command -match $absPathPattern) {
            $errors += "Step $n command contains absolute path — redact usernames"
        }
    }
}

# Stack trace length
if ($repro.errorMessages.stackTrace -and $repro.errorMessages.stackTrace.Length -gt 5000) {
    $errors += "stackTrace is $($repro.errorMessages.stackTrace.Length) chars (max 5000)"
}

# Conclusion ↔ step-result consistency
$conclusion = $repro.conclusion
$results = @($steps | Where-Object { $_.result } | ForEach-Object { $_.result })
switch ($conclusion) {
    'reproduced' {
        if ($results -notcontains 'failure' -and $results -notcontains 'wrong-output') {
            $errors += "Conclusion is 'reproduced' but no step has result 'failure' or 'wrong-output'"
        }
    }
    'wrong-output' {
        if ($results -notcontains 'wrong-output') {
            $errors += "Conclusion is 'wrong-output' but no step has result 'wrong-output'"
        }
    }
    'not-reproduced' {
        if ($results -notcontains 'success') {
            $errors += "Conclusion is 'not-reproduced' but no step has result 'success'"
        }
        if ($results -contains 'failure' -or $results -contains 'wrong-output') {
            $errors += "Conclusion is 'not-reproduced' but step(s) have result 'failure'/'wrong-output' — if the reported behavior was observed, conclusion should be 'reproduced' (put editorial notes in 'notes' field, not conclusion)"
        }
    }
    { $_ -in 'needs-platform', 'needs-hardware', 'partial', 'inconclusive' } {
        if (-not $repro.blockers -or $repro.blockers.Count -eq 0) {
            $errors += "Conclusion is '$conclusion' but blockers is null or empty"
        }
    }
}

if ($errors.Count -eq 0) {
    $number = $repro.meta.number ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (issue #$number, conclusion: $conclusion)"
    exit 0
}
Write-Host "❌ $($errors.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$errors | ForEach-Object { Write-Host "  $_" }
exit 1
