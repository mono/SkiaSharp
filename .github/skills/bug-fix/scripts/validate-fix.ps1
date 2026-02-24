<#
.SYNOPSIS Validate a bug-fix JSON file against fix-schema.json (v1.0).
.EXAMPLE  pwsh scripts/validate-fix.ps1 ai-fix/2997.json
.NOTES    Exits 0=valid, 1=fixable (retry), 2=fatal. Requires PowerShell 7.5+.
#>
#requires -Version 7.5
param([Parameter(Mandatory, Position = 0)] [string]$Path)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }
$schemaPath = Join-Path $PSScriptRoot '../references/fix-schema.json'
if (-not (Test-Path $schemaPath)) { Write-Host "❌ Schema not found: $schemaPath"; exit 2 }

$json = Get-Content $Path -Raw
$fix = $json | ConvertFrom-Json -Depth 50
$errors = @()

# --- Schema validation ---
if (-not ($json | Test-Json -SchemaFile $schemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue)) {
    $errors += $schemaErrors | ForEach-Object {
        $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
    } | Sort-Object -Unique
}

# --- Extra checks beyond schema ---

# If status is 'fixed', verification should be passed or not-applicable
if ($fix.status -eq 'fixed') {
    if ($fix.verification.reproScenario -eq 'failed') {
        $errors += "Status is 'fixed' but verification.reproScenario is 'failed'"
    }
    if ($fix.tests.result -eq 'fail') {
        $errors += "Status is 'fixed' but tests.result is 'fail'"
    }
}

# If regressionTestAdded, testsAdded should have entries
if ($fix.tests.regressionTestAdded -and (-not $fix.tests.testsAdded -or $fix.tests.testsAdded.Count -eq 0)) {
    $errors += "regressionTestAdded is true but testsAdded is missing or empty"
}

# changes.files should have entries when status is 'fixed'
if ($fix.status -eq 'fixed' -and (-not $fix.changes.files -or $fix.changes.files.Count -eq 0)) {
    $errors += "Status is 'fixed' but changes.files is empty"
}

# Absolute path check
$absPathPattern = '(/Users/|/home/|C:\\Users\\)'
if ($fix.changes.files) {
    foreach ($f in $fix.changes.files) {
        if ($f.path -match $absPathPattern) {
            $errors += "File path '$($f.path)' contains absolute path — use relative path from repo root"
        }
    }
}

if ($errors.Count -eq 0) {
    $number = $fix.meta.number ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (issue #$number, status: $($fix.status))"
    exit 0
}
Write-Host "❌ $($errors.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$errors | ForEach-Object { Write-Host "  $_" }
exit 1
