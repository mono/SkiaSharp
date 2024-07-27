$ErrorActionPreference = 'Stop'

if ("$env:ANDROID_HOME" -and "$env:ANDROID_SDK_ROOT") {
    if ("$env:ANDROID_HOME" -eq "$env:ANDROID_SDK_ROOT") {
        Write-Host "Android SDK is already installed at $env:ANDROID_HOME."
        Write-Host "Android SDK contents:"
        Get-ChildItem "$env:ANDROID_HOME"
        exit 0
    } else {
        Write-Warning "Android SDK environment variables were different:"
        Write-Warning "ANDROID_SDK_ROOT=$env:ANDROID_SDK_ROOT."
        Write-Warning "ANDROID_HOME=$env:ANDROID_HOME."
        Write-Host "ANDROID_SDK_ROOT Android SDK contents:"
        Get-ChildItem "$env:ANDROID_SDK_ROOT"
        Write-Host "ANDROID_HOME Android SDK contents:"
        Get-ChildItem "$env:ANDROID_HOME"
        exit 1
    }
}

Write-Host "Looking for an existing Android SDK..."

# try use some environment variables
if ("$env:ANDROID_SDK_ROOT") {
    Write-Host "Found environment variable ANDROID_SDK_ROOT=$env:ANDROID_SDK_ROOT."
    $sdk = "$env:ANDROID_SDK_ROOT"
} elseif ("$env:ANDROID_HOME") {
    Write-Host "Found environment variable ANDROID_HOME=$env:ANDROID_HOME."
    $sdk = "$env:ANDROID_HOME"
}

# look in program files from VS
if (-not $sdk -and -not $IsMacOS -and -not $IsLinux) {
    Write-Host "Searching Program Files..."
    $pfsdk = Join-Path "${env:ProgramFiles(x86)}" "Android" "android-sdk"
    $adb = Join-Path "$pfsdk" "platform-tools" "adb.exe"
    if (Test-Path $adb) {
        Write-Host "Found $pfsdk."
        $sdk = $pfsdk
    }
}

# for now just thro as most machines have the SDK somewhere
if (-not $sdk) {
    Write-Error "TODO: actually install the Android SDK"
    exit 1
}

# make sure that the SDK is in ANDROID_HOME and ANDROID_SDK_ROOT
Write-Host "Setting environment variable ANDROID_HOME=$sdk"
Write-Host "Setting environment variable ANDROID_SDK_ROOT=$sdk"
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_ROOT;]$sdk";
Write-Host "##vso[task.setvariable variable=ANDROID_HOME;]$sdk";
$env:ANDROID_SDK_ROOT = $sdk
$env:ANDROID_HOME = $sdk

Write-Host "Android SDK contents:"
Get-ChildItem "$sdk"

exit $LASTEXITCODE
