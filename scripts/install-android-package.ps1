Param(
    [Parameter(Mandatory)] [string] $Package
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1

Write-Host "Installing Android SDK package $Package..."
dotnet android sdk install --package "$Package"
Write-Host "Installation of Android SDK package $Package complete."

exit $LASTEXITCODE
