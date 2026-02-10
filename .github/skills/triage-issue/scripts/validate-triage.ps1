<#
.SYNOPSIS
    Validate an ai-triage JSON file against the triage JSON Schema (v2.1).
.EXAMPLE
    pwsh scripts/validate-triage.ps1 ai-triage/2794.json
.NOTES
    Exits 0 if valid, 1 if errors found, 2 for usage/file errors.
    Checks: schema validation + unique action IDs + dependsOn integrity.
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
$triage = $json | ConvertFrom-Json -Depth 50
$errors = @()

# --- Schema validation ---
if (-not ($json | Test-Json -SchemaFile $schemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue)) {
    $errors += $schemaErrors | ForEach-Object {
        $_.Exception.Message -replace '^The JSON is not valid with the schema: ', ''
    } | Sort-Object -Unique
}

# --- Extra checks (not expressible in JSON Schema) ---
$actions = $triage.output.actions
if ($actions -and $actions.Count -gt 0) {
    # Unique action IDs
    $ids = $actions | ForEach-Object { $_.id }
    $dupes = $ids | Group-Object | Where-Object { $_.Count -gt 1 }
    foreach ($d in $dupes) {
        $errors += "Duplicate action id: '$($d.Name)'"
    }

    # dependsOn referential integrity
    $idSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$ids)
    foreach ($a in $actions) {
        if ($a.dependsOn -and -not $idSet.Contains($a.dependsOn)) {
            $errors += "Action '$($a.id)' dependsOn '$($a.dependsOn)' which does not exist"
        }
    }

    # dependsOn ordering: referenced action must appear before referencing action
    for ($i = 0; $i -lt $actions.Count; $i++) {
        $dep = $actions[$i].dependsOn
        if ($dep) {
            $depIndex = -1
            for ($j = 0; $j -lt $actions.Count; $j++) {
                if ($actions[$j].id -eq $dep) { $depIndex = $j; break }
            }
            if ($depIndex -ge $i) {
                $errors += "Action '$($actions[$i].id)' depends on '$dep' but it appears later (index $depIndex >= $i)"
            }
        }
    }
}

# --- fieldRationale coverage is enforced by the schema via allOf/if-then/contains rules ---

if ($errors.Count -eq 0) {
    $number = $triage.meta.number ?? $triage.number ?? '?'
    Write-Host "✅ $(Split-Path $Path -Leaf) is valid (issue #$number)"
    exit 0
}

Write-Host "❌ $($errors.Count) validation error(s) in $(Split-Path $Path -Leaf):`n"
$errors | ForEach-Object { Write-Host "  $_" }
exit 1
