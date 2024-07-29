Param(
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

function Get-SdkManager {
    param (
        [string] $SdkPath,
        [string] $Indent = "    "
    )

    Write-Host "${Indent}Looking for the SDK Manager in $SdkPath..."

    if (-not $SdkPath -or -not (Test-Path $SdkPath)) {
        Write-Host "${Indent}No SDK Manager found."
        return ""
    }

    $cmdline = Join-Path "$SdkPath" "cmdline-tools"
    if (Test-Path $cmdline) {
        Write-Host "${Indent}  Found command line tools:"
        $cmdline | Get-ChildItem | ForEach-Object {
            Write-Host "${Indent}    $_"
        }
    }

    Write-Host "${Indent}  Selecting the latest command line tools..."

    $sdkExt = if (-not $IsMacOS -and -not $IsLinux) { ".bat" } else { "" }
    $versions = Get-ChildItem (Join-Path "$SdkPath" "cmdline-tools")
    $latest = ($versions | Select-Object -Last 1)[0]
    Write-Host "${Indent}  Latest command line tools found at $latest."
    $sdkmanager = Join-Path "$latest" "bin" "sdkmanager$sdkExt"

    if (-not (Test-Path $sdkmanager)) {
        Write-Host "${Indent}No SDK Manager found."
        return ""
    }

    Write-Host "${Indent}Found the SDK Manager at $sdkmanager."

    return $sdkmanager
}

function Get-AndroidSdk {
    param (
        [string] $SdkPath,
        [string] $PathType,
        [string] $Indent = "  "
    )

    Write-Host "${Indent}Looking for the Android SDK in $SdkPath ($PathType)..."

    if (-not $SdkPath -or -not (Test-Path $SdkPath)) {
        Write-Host "${Indent}No Android SDK found."
        return ""
    }

    $sdkmanager = Get-SdkManager -SdkPath "$SdkPath"
    if (-not (Test-Path $sdkmanager)) {
        Write-Host "${Indent}No SDK Manager found, not going to use this one."
        return ""
    }

    Write-Host "${Indent}Using the Android SDK at $SdkPath."

    return "$SdkPath"
}

Write-Host "Looking for an existing Android SDK..."

# try use some environment variables
$sdk = Get-AndroidSdk -SdkPath "$env:ANDROID_SDK_ROOT" -PathType "ANDROID_SDK_ROOT"
if (-not $sdk -and "$env:ANDROID_HOME") {
    $sdk = Get-AndroidSdk -SdkPath "$env:ANDROID_HOME" -PathType "ANDROID_HOME"
}
# look in program files from VS
if (-not $sdk -and -not $IsMacOS -and -not $IsLinux) {
    $pfsdk = Join-Path "${env:ProgramFiles(x86)}" "Android" "android-sdk"
    $sdk = Get-AndroidSdk -SdkPath "$pfsdk" -PathType "Program Files"
}

# Nothing was found, so we need to install the SDK
if (-not $sdk) {
    Write-Host "No Android SDK found, will download and install the latest..."

    $sdk = "$HOME_DIR/android-sdk"
    if ($InstallDestination) {
        $sdk = $InstallDestination
    }
    Write-Host "  Install destination is $sdk."

    # detect
    Write-Host "  Getting the latest command line tool info..."
    $repoUrl = "https://dl.google.com/android/repository/repository2-1.xml"
    [xml] $repoXml = Invoke-WebRequest -Uri $repoUrl
    $stable = $repoXml.DocumentElement.remotePackage | Where-Object { $_.path -eq "cmdline-tools;latest" -and $_.channelRef.ref -eq "channel-0" }
    $platform = if ($IsMacOS) { "macosx" } elseif ($IsLinux) { "linux" } else { "windows" }
    $archive = $stable.archives.archive | Where-Object { $_."host-os" -eq $platform }
    $filename = $archive.complete.url
    Write-Host "  Latest command line tools are $filename..."
    $url = "https://dl.google.com/android/repository/$filename"

    $sdkTemp = "$HOME_DIR/android-sdk-temp"
    $zip = "$sdkTemp/$filename"

    # download
    if (-not (Test-Path $zip)) {
        Write-Host "  Downloading SDK ($url) to '$zip'..."
        New-Item -ItemType Directory -Force -Path "$sdkTemp" | Out-Null
        (New-Object System.Net.WebClient).DownloadFile("$url", "$zip")
    }

    # extract
    if (Test-Path "$sdkTemp/extracted") {
        Write-Host "  Removing old extracted SDK from $sdkTemp/extracted..."
        Remove-Item -Recurse -Force "$sdkTemp/extracted"
    }
    if (Test-Path "$sdk") {
        Write-Host "  Removing old SDK from $sdk..."
        Remove-Item -Recurse -Force "$sdk"
    }
    Write-Host "  Extracting SDK to $sdkTemp/extracted..."
    try {
        if ($IsLinux -or $IsMacOS) {
            unzip -q "$zip" -d "$sdkTemp/extracted"
            if (-not $?) {
                throw "  Failed to extract the SDK."
            }
        } else {
            [System.IO.Compression.ZipFile]::ExtractToDirectory("$zip", "$sdkTemp/extracted")
        }
    } catch {
        Write-Host "  Failed to extract the SDK, deleting the file as it may be corrupt..."
        Remove-Item -Force "$zip"
        exit 1
    }
    Write-Host "  Moving command line tools to $sdk/cmdline-tools/latest..."
    New-Item -ItemType Directory -Force -Path "$sdk/cmdline-tools/latest" | Out-Null
    Move-Item "${sdkTemp}/extracted/cmdline-tools/*" "$sdk/cmdline-tools/latest/"

    Write-Host "Installation complete."
}

$sdkmanager = Get-SdkManager -SdkPath "$sdk" -Indent ""

Write-Host "Using Android SDK at $sdk."

Write-Host ""
Write-Host "Setting environment variables..."
# make sure that the SDK is in:
# - ANDROID_HOME and ANDROID_SDK_ROOT for native things
# - AndroidSdkDirectory for .NET for Android
# - ANDROID_SDK_MANAGER_PATH for the path to the SDK Manager
Write-Host "Setting environment variable ANDROID_HOME=$sdk"
Write-Host "Setting environment variable ANDROID_SDK_ROOT=$sdk"
Write-Host "Setting environment variable AndroidSdkDirectory=$sdk"
Write-Host "Setting environment variable ANDROID_SDK_MANAGER_PATH=$sdkmanager"
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_ROOT;]$sdk";
Write-Host "##vso[task.setvariable variable=ANDROID_HOME;]$sdk";
Write-Host "##vso[task.setvariable variable=AndroidSdkDirectory;]$sdk";
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_MANAGER_PATH;]$sdkmanager";
$env:ANDROID_SDK_ROOT = $sdk
$env:ANDROID_HOME = $sdk
$env:AndroidSdkDirectory = $sdk
$env:ANDROID_SDK_MANAGER_PATH = $sdkmanager

exit $LASTEXITCODE
