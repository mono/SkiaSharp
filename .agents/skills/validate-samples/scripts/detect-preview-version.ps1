# detect-preview-version.ps1
# Detects the preview label and build number from downloaded nupkg files in output/nugets/
# Usage: pwsh .claude/skills/validate-samples/scripts/detect-preview-version.ps1
#   Prints: Preview label, Build number, Full suffix

param(
    [string]$NugetsDir = "output/nugets"
)

$ErrorActionPreference = "Stop"

# Find a preview SkiaSharp package (not NativeAssets, not HarfBuzz)
$pkg = Get-ChildItem "$NugetsDir/SkiaSharp.*.nupkg" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^SkiaSharp\.\d' -and $_.Name -notmatch 'NativeAssets' -and $_.Name -match '-' } |
    Select-Object -First 1

if (-not $pkg) {
    Write-Error "No preview SkiaSharp package found in $NugetsDir. Run 'dotnet cake --target=docs-download-output' first."
    exit 1
}

Write-Host "Found: $($pkg.Name)"

# Extract suffix: SkiaSharp.3.119.4-preview.0.76.nupkg → preview.0.76
$suffix = $pkg.Name -replace '^SkiaSharp\.\d+\.\d+\.\d+-', '' -replace '\.nupkg$', ''

# Split on last dot: preview.0 + 76
$previewLabel = $suffix -replace '\.\d+$', ''
$buildNumber = ($suffix | Select-String '\d+$').Matches[0].Value

Write-Host "Preview label: $previewLabel"
Write-Host "Build number:  $buildNumber"
Write-Host "Full suffix:   $suffix"
