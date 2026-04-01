<#
.SYNOPSIS Copy a validated skia-review JSON to output/ai/ for collection and render HTML report.
.EXAMPLE  pwsh persist-skia-review.ps1 /tmp/skiasharp/skia-review/20260320-164500/170.json
#>
param([Parameter(Mandatory, Position = 0)][string]$Path)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) { Write-Host "❌ File not found: $Path"; exit 2 }

$number = [System.IO.Path]::GetFileNameWithoutExtension($Path)
if ($number -notmatch '^\d+$') {
    Write-Host "❌ Cannot extract PR number from filename: $(Split-Path $Path -Leaf)"
    exit 2
}

$destDir = "output/ai/repos/mono-skia/ai-review"
New-Item -ItemType Directory -Force $destDir | Out-Null

Copy-Item $Path "$destDir/$number.json" -Force
Write-Host "✅ Copied to $destDir/$number.json"

# Render HTML report alongside the JSON
$renderScript = Join-Path $PSScriptRoot "render-skia-review.py"
if (Test-Path $renderScript) {
    python3 $renderScript "$destDir/$number.json"
} else {
    Write-Host "⚠️  render-skia-review.py not found — skipping HTML generation"
}
