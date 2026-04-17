<#
.SYNOPSIS Persist a validated triage JSON to output/ai/ and render report wrappers.
.DESCRIPTION
    Copies the triage JSON to output/ai/repos/mono-SkiaSharp/ai-triage/{number}.json
    and generates sidecar {number}.md and {number}.html files that present the same
    data in human-friendly formats.
.EXAMPLE
    pwsh persist-triage.ps1 /tmp/skiasharp/triage/20260320-164500/3400.json
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

$destDir = "output/ai/repos/mono-SkiaSharp/ai-triage"
$dest = Join-Path $destDir "$number.json"
New-Item -ItemType Directory -Force $destDir | Out-Null
Copy-Item $Path $dest -Force
Write-Host "✅ Copied JSON to $dest"

$renderScript = Join-Path $PSScriptRoot "render-triage-report.py"
if (Test-Path $renderScript) {
    python3 $renderScript $dest
} else {
    Write-Host "⚠️  render-triage-report.py not found — skipping Markdown/HTML generation"
}
