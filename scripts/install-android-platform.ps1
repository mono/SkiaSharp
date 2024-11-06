Param(
    [Parameter(Mandatory)] [string[]] $API
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1

$sdk = "$env:ANDROID_SDK_ROOT"
$sdkmanager = "$env:ANDROID_SDK_MANAGER_PATH"

$apis = $API -split ','
Write-Host "Installing $($apis.Count) Android API levels $API..."

# install each of the APIs
foreach ($API in $apis) {
    Write-Host "Installing Android API level $API..."

    # check if already installed
    $apiPath = Join-Path "$sdk" "platforms" "android-$API" "android.jar"
    if (Test-Path $apiPath) {
        Write-Host "Android API level $API was already installed."
        continue
    }

    # install
    Set-Content -Value "y" -Path "yes.txt"
    try {
        Write-Host "Installing Android API level $API..."
        if ($IsMacOS -or $IsLinux) {
            sh -c "'$sdkmanager' 'platforms;android-$API' < yes.txt"
        } else {
            cmd /c "`"$sdkmanager`" `"platforms;android-$API`" < yes.txt"
        }
        if (!$?) {
            Write-Host "Failed to install Android API level $API."
            exit 1
        }
        Write-Host "Installation of Android API level $API complete."
    } finally {
        Remove-Item "yes.txt"
    }
}

exit $LASTEXITCODE
