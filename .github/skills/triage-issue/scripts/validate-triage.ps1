<#
.SYNOPSIS
    Validate an ai-triage JSON file against the triage JSON Schema.
.EXAMPLE
    pwsh scripts/validate-triage.ps1 ai-triage/2794.json
.NOTES
    Exits 0 if valid, 1 if errors found, 2 for usage/file errors.
#>
param(
    [Parameter(Mandatory, Position = 0)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) {
    Write-Error "❌ File not found: $Path"
    exit 2
}

$schemaPath = Join-Path $PSScriptRoot '../references/triage-schema.json'
if (-not (Test-Path $schemaPath)) {
    Write-Error "❌ Schema not found: $schemaPath"
    exit 2
}

$json = Get-Content $Path -Raw
$schema = Get-Content $schemaPath -Raw

if ($json | Test-Json -Schema $schema -ErrorVariable validationErrors -ErrorAction SilentlyContinue) {
    $number = ($json | ConvertFrom-Json).number ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (issue #$number)"
    exit 0
}

# Strip the boilerplate prefix from each error message
$msgs = $validationErrors | ForEach-Object {
    $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
} | Sort-Object -Unique

Write-Host "❌ $($msgs.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$msgs | ForEach-Object { Write-Host "  $_" }
exit 1
