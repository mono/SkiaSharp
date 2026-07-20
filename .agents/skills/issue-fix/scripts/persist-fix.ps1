<#
.SYNOPSIS Persist a validated fix JSON to output/ai/.
.DESCRIPTION Copies the fix JSON to output/ai/repos/mono-SkiaSharp/ai-fix/{number}.json.
.EXAMPLE  pwsh persist-fix.ps1 /tmp/skiasharp/fix/20250101-120000/3400.json
#>
param(
    [Parameter(Mandatory, Position = 0)][string]$Path
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }

$number = [System.IO.Path]::GetFileNameWithoutExtension($Path)
if ($number -notmatch '^\d+$') {
    Write-Host "❌ Cannot extract issue number from filename: $(Split-Path $Path -Leaf)"
    Write-Host "   Expected format: {number}.json"
    exit 2
}

$dest = "output/ai/repos/mono-SkiaSharp/ai-fix/$number.json"
New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
Copy-Item $Path $dest -Force
Write-Host "✅ Persisted to $dest"
