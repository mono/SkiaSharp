$ErrorActionPreference = 'Stop'

if ("$env:ANDROID_HOME" -and "$env:ANDROID_SDK_ROOT") {
    Write-Host "Android SDK is already installed."
    exit 0
}

# try use some environment variables
if ("$env:ANDROID_SDK_ROOT") {
    $sdk = "$env:ANDROID_SDK_ROOT"
} elseif ("$env:ANDROID_HOME") {
    $sdk = "$env:ANDROID_HOME"
}

# look in program files from VS
if (-not $IsMacOS -and -not $IsLinux) {
    $pfsdk = Join-Path "${env:ProgramFiles(x86)}" "Android" "android-sdk"
    $adb = Join-Path "$pfsdk" "platform-tools" "adb.exe"
    if (Test-Path $adb) {
        $sdk = $pfsdk
    }
}

# for now just thro as most machines have the SDK somewhere
if (-not $sdk) {
    Write-Error "TODO: actually install the Android SDK"
    exit 1
}

# make sure that the SDK is in ANDROID_HOME and ANDROID_SDK_ROOT
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_ROOT;]$sdk";
Write-Host "##vso[task.setvariable variable=ANDROID_HOME;]$sdk";
$env:ANDROID_SDK_ROOT = $sdk
$env:ANDROID_HOME = $sdk

exit $LASTEXITCODE
