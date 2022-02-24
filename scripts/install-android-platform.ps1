Param(
    [string] $API
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1
$sdk = "$env:ANDROID_SDK_ROOT"

$apiPath = Join-Path "$sdk" "platforms" "android-$API" "android.jar"
if (Test-Path $apiPath) {
    Write-Host "Android API level $API was already installed."
    exit 0
}

$latest = Join-Path "$sdk" "cmdline-tools" "latest"
if (-not (Test-Path $latest)) {
    $versions = Get-ChildItem (Join-Path "$sdk" "cmdline-tools")
    $latest = ($versions | Select-Object -Last 1)[0]
}

if (-not $IsMacOS -and -not $IsLinux) {
    $ext = ".bat"
}
$sdkmanager = Join-Path "$latest" "bin" "sdkmanager$ext"

Set-Content -Value "y" -Path "yes.txt"
try {
    if ($IsMacOS -or $IsLinux) {
        sh -c "`"$sdkmanager`" `"platforms\;android-$API`" < yes.txt"
    } else {
        cmd /c "`"$sdkmanager`" `"platforms;android-$API`" < yes.txt"
    }
} finally {
    Remove-Item "yes.txt"
}

exit $LASTEXITCODE
