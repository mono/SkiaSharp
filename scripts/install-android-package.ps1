Param(
    [Parameter(Mandatory)] [string] $Package
)

$ErrorActionPreference = 'Stop'

.\scripts\install-android-sdk.ps1

$sdkmanager = "$env:ANDROID_SDK_MANAGER_PATH"

Set-Content -Value "y" -Path "yes.txt"
try {
    if ($IsMacOS -or $IsLinux) {
        sh -c "'$sdkmanager' '$Package' < yes.txt"
    } else {
        cmd /c "`"$sdkmanager`" `"$Package`" < yes.txt"
    }
} finally {
    Remove-Item "yes.txt"
}

exit $LASTEXITCODE
