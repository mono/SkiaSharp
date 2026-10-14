Param(
    [Parameter(Mandatory)] [ValidateNotNullOrEmpty()] [string] $Package
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'install-android-sdk.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "Android SDK setup failed (exit code $LASTEXITCODE)."
}

$sdkmanager = "$env:ANDROID_SDK_MANAGER_PATH"
if (-not $sdkmanager -or -not (Test-Path -LiteralPath $sdkmanager -PathType Leaf)) {
    throw 'Android SDK setup did not provide an existing ANDROID_SDK_MANAGER_PATH.'
}

$logDirectory = Join-Path $PSScriptRoot '../../../output/logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$licenseFile = Join-Path $logDirectory "android-license-$([Guid]::NewGuid().ToString('N')).txt"
$licenseStream = [IO.File]::Open($licenseFile, [IO.FileMode]::CreateNew)
$licenseStream.Dispose()
try {
    Set-Content -Value 'y' -LiteralPath $licenseFile
    Get-Content -LiteralPath $licenseFile | & $sdkmanager $Package
    $result = $LASTEXITCODE
} finally {
    Remove-Item -LiteralPath $licenseFile
}

exit $result
