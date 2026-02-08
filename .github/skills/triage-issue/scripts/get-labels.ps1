<#
.SYNOPSIS
    Fetch GitHub labels for a SkiaSharp repo, optionally filtered by prefix.
.EXAMPLE
    pwsh scripts/get-labels.ps1                         # all labels, grouped
    pwsh scripts/get-labels.ps1 area/                   # labels starting with "area/"
    pwsh scripts/get-labels.ps1 -Json area/             # machine-readable JSON array
    pwsh scripts/get-labels.ps1 -Repo mono/SkiaSharp.Extended backend/
#>
param(
    [Parameter(Position = 0)]
    [string]$Prefix,

    [string]$Repo = 'mono/SkiaSharp',

    [switch]$Json
)

$ErrorActionPreference = 'Stop'

$raw = gh label list --repo $Repo --limit 500 --json name,description 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ gh label list failed: $raw"
    exit 1
}
$labels = $raw | ConvertFrom-Json

if ($Prefix) {
    $filtered = $labels | Where-Object { $_.name.StartsWith($Prefix) } | Sort-Object name

    if ($Json) {
        $values = $filtered | ForEach-Object { $_.name.Substring($Prefix.Length) }
        $values | ConvertTo-Json -Compress
    }
    else {
        foreach ($l in $filtered) {
            $suffix = $l.name.Substring($Prefix.Length)
            $desc = if ($l.description) { "  — $($l.description)" } else { '' }
            Write-Host "$suffix$desc"
        }
        Write-Host "`n$($filtered.Count) labels with prefix '$Prefix'"
    }
}
else {
    if ($Json) {
        $labels | ForEach-Object { $_.name } | Sort-Object | ConvertTo-Json -Compress
    }
    else {
        $groups = $labels | Group-Object {
            $i = $_.name.IndexOf('/')
            if ($i -ge 0) { $_.name.Substring(0, $i + 1) } else { '(no prefix)' }
        } | Sort-Object Name

        foreach ($g in $groups) {
            $values = $g.Group | ForEach-Object {
                $i = $_.name.IndexOf('/')
                if ($i -ge 0) { $_.name.Substring($i + 1) } else { $_.name }
            } | Sort-Object
            Write-Host "$($g.Name) ($($values.Count) labels)"
            $values | ForEach-Object { Write-Host "  $_" }
            Write-Host ''
        }
        Write-Host "$($labels.Count) total labels"
    }
}
