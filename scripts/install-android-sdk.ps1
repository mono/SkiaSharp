Param(
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

Write-Host "Looking for an existing Android SDK..."

# try use some environment variables
if ("$env:ANDROID_SDK_ROOT") {
    Write-Host "Found environment variable ANDROID_SDK_ROOT=$env:ANDROID_SDK_ROOT, checking for command line tools..."
    $latest = Join-Path "$env:ANDROID_SDK_ROOT" "cmdline-tools" "latest"
    if (Test-Path $latest) {
        $sdk = "$env:ANDROID_SDK_ROOT"
        Write-Host "Found command line tools in $latest."
    } else {
        Write-Host "No command line tools found, not going to use this one."
    }
}
if (-not $sdk -and "$env:ANDROID_HOME") {
    Write-Host "Found environment variable ANDROID_HOME=$env:ANDROID_HOME, checking for command line tools..."
    $latest = Join-Path "$env:ANDROID_HOME" "cmdline-tools" "latest"
    if (Test-Path $latest) {
        $sdk = "$env:ANDROID_HOME"
        Write-Host "Found command line tools in $latest."
    } else {
        Write-Host "No command line tools found, not going to use this one."
    }
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

# Nothing was found, so we need to install the SDK
if (-not $sdk) {
    # detect
    Write-Host "Getting the latest command line tool info..."
    $repoUrl = "https://dl.google.com/android/repository/repository2-1.xml"
    [xml] $repoXml = Invoke-WebRequest -Uri $repoUrl
    $stable = $repoXml.DocumentElement.remotePackage | Where-Object { $_.path -eq "cmdline-tools;latest" -and $_.channelRef.ref -eq "channel-0" }
    $platform = if ($IsMacOS) { "macosx" } elseif ($IsLinux) { "linux" } else { "windows" }
    $archive = $stable.archives.archive | Where-Object { $_."host-os" -eq $platform }
    $filename = $archive.complete.url
    Write-Host "Latest command line tools are $filename..."
    $url = "https://dl.google.com/android/repository/$filename"

    $sdk = "$HOME_DIR/android-sdk"
    if ($InstallDestination) {
        $sdk = $InstallDestination
    }
    Write-Host "Install destination is '$sdk'."

    $sdkTemp = "$HOME_DIR/android-sdk-temp"
    $zip = "$sdkTemp/android-sdk-cmdline-tools.zip"

    # download
    # if (-not (Test-Path $zip)) {
    Write-Host "Downloading SDK ($url) to '$zip'..."
    New-Item -ItemType Directory -Force -Path "$sdkTemp" | Out-Null
    (New-Object System.Net.WebClient).DownloadFile("$url", "$zip")
    # }

    # extract
    if (Test-Path "$sdkTemp/extracted") {
        Write-Host "Removing old extracted SDK from $sdkTemp/extracted..."
        Remove-Item -Recurse -Force "$sdkTemp/extracted"
    }
    if (Test-Path "$sdk") {
        Write-Host "Removing old SDK from $sdk..."
        Remove-Item -Recurse -Force "$sdk"
    }
    Write-Host "Extracting SDK to $sdkTemp/extracted..."
    if ($IsLinux -or $IsMacOS) {
        unzip "$zip" -d "$sdkTemp/extracted"
        Write-Host "Moving SDK..."
        New-Item -ItemType Directory -Force -Path "$sdk/cmdline-tools/latest" | Out-Null
        Move-Item "${sdkTemp}/extracted/cmdline-tools/*" "$sdk/cmdline-tools/latest/"
    } else {
        [System.IO.Compression.ZipFile]::ExtractToDirectory("$zip", "$sdkTemp/extracted")
        Write-Host "Moving SDK..."
        New-Item -ItemType Directory -Force -Path "$sdk/cmdline-tools/latest" | Out-Null
        Move-Item "${sdkTemp}/extracted/cmdline-tools/*" "$sdk/cmdline-tools/latest/"
    }
}

# make sure that the SDK is in ANDROID_HOME and ANDROID_SDK_ROOT
Write-Host "Setting environment variable ANDROID_HOME=$sdk"
Write-Host "Setting environment variable ANDROID_SDK_ROOT=$sdk"
Write-Host "##vso[task.setvariable variable=ANDROID_SDK_ROOT;]$sdk";
Write-Host "##vso[task.setvariable variable=ANDROID_HOME;]$sdk";
$env:ANDROID_SDK_ROOT = $sdk
$env:ANDROID_HOME = $sdk

exit $LASTEXITCODE
