Param(
    [string] $Package
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1
$sdk = "$env:ANDROID_SDK_ROOT"

$latest = Join-Path "$sdk" "cmdline-tools" "latest"
if (-not (Test-Path $latest)) {
    $versions = Get-ChildItem (Join-Path "$sdk" "cmdline-tools")
    $latest = ($versions | Select-Object -Last 1)[0]
}

if (-not $IsMacOS -and -not $IsLinux) {
    $ext = ".bat"
}
$sdkmanager = Join-Path "$latest" "bin" "sdkmanager$ext"

Write-Host "y`r`ny" | & $sdkmanager "$Package"

exit $LASTEXITCODE
