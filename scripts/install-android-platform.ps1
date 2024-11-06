Param(
    [Parameter(Mandatory)] [string[]] $API
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1

$sdk = "$env:ANDROID_SDK_ROOT"

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
    Write-Host "Installing Android API level $API..."
    dotnet android sdk install --package "platforms;android-$API"
    Write-Host "Installation of Android API level $API complete."
}

exit $LASTEXITCODE
