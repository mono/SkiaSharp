Param(
    [string] $SearchRoot = "tests",
    [string] $Destination = "output/logs/testlogs/visualfailures"
)

# Collects the visual-regression matrix's failure artifacts (the *.actual.png /
# *.diff.png pairs the harness writes next to the test binary under
# _visualfailures) into the published test-log tree so a red visual cell can be
# triaged from the build artifacts without decoding the base64 in the TRX.
#
# Safe to run unconditionally: when nothing failed there are no _visualfailures
# directories and this is a no-op.

$ErrorActionPreference = "Stop"

$root = (Get-Location).Path

$dirs = Get-ChildItem -Path $SearchRoot -Recurse -Directory -Filter "_visualfailures" -ErrorAction SilentlyContinue
if (-not $dirs) {
    Write-Host "No _visualfailures directories found under '$SearchRoot'."
    exit 0
}

foreach ($dir in $dirs) {
    $relative = [System.IO.Path]::GetRelativePath($root, $dir.FullName)
    $target = Join-Path $Destination $relative
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item -Path (Join-Path $dir.FullName "*") -Destination $target -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Collected '$relative' -> '$target'."
}

Write-Host "Collected $($dirs.Count) visual-failure director(ies) into '$Destination'."
