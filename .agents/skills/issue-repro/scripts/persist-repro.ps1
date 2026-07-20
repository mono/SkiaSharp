<#
.SYNOPSIS Persist a validated repro JSON to output/ai/.
.DESCRIPTION Copies the repro JSON to output/ai/repos/mono-SkiaSharp/ai-repro/{number}.json.
.EXAMPLE  pwsh persist-repro.ps1 /tmp/skiasharp/repro/20250101-120000/3400.json
#>
param(
    [Parameter(Mandatory, Position = 0)][string]$Path
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }

$number = [System.IO.Path]::GetFileNameWithoutExtension($Path) -replace '^repro-', ''
if ($number -notmatch '^\d+$') {
    Write-Host "❌ Cannot extract issue number from filename: $(Split-Path $Path -Leaf)"
    Write-Host "   Expected format: repro-{number}.json"
    exit 2
}

$dest = "output/ai/repos/mono-SkiaSharp/ai-repro/$number.json"
New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
Copy-Item $Path $dest -Force
Write-Host "✅ Persisted to $dest"
