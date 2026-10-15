Param(
    [Parameter(Mandatory)] [string] $Package
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'install-android-sdk.ps1')
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

'y' | & $env:ANDROID_SDK_MANAGER_PATH $Package
exit $LASTEXITCODE
