<#
.SYNOPSIS Persist a validated triage JSON to output/ai/.
.DESCRIPTION Copies the triage JSON to output/ai/repos/mono-SkiaSharp/ai-triage/{number}.json.
.EXAMPLE  pwsh persist-triage.ps1 /tmp/skiasharp/triage/20260320-164500/3400.json
#>
param(
    [Parameter(Mandatory, Position = 0)][string]$Path
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }

$number = [System.IO.Path]::GetFileNameWithoutExtension($Path) -replace '^triage-', ''
if ($number -notmatch '^\d+$') {
    Write-Host "❌ Cannot extract issue number from filename: $(Split-Path $Path -Leaf)"
    Write-Host "   Expected format: triage-{number}.json"
    exit 2
}

$dest = "output/ai/repos/mono-SkiaSharp/ai-triage/$number.json"
New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
Copy-Item $Path $dest -Force
Write-Host "✅ Persisted to $dest"
