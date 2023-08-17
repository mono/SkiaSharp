Param(
    [string] $Version = "r25c",
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

if ($IsMacOS) {
    $platform = "darwin.dmg"
} elseif ($IsLinux) {
    $platform = "linux.zip"
} else {
    $platform = "windows.zip"
}

$url = "https://dl.google.com/android/repository/android-ndk-${Version}-${platform}"

$ndk = "$HOME_DIR/android-ndk"
if ($InstallDestination) {
    $ndk = $InstallDestination
}
Write-Host "Install destination is '$ndk'."

$ndkTemp = "$HOME_DIR/android-ndk-temp"
$install = "$ndkTemp/android-ndk-${platform}"

# download
Write-Host "Downloading NDK ($url)..."
New-Item -ItemType Directory -Force -Path "$ndkTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# extract
Write-Host "Extracting NDK..."
if ($IsMacOS) {
    hdiutil attach $install
    $ndkSource = (Get-ChildItem "/Volumes/Android NDK ${Version}/AndroidNDK*.app/Contents/NDK").Fullname
    New-Item -ItemType Directory -Force -Path "$ndk" | Out-Null
    cp -r "$ndkSource/" "$ndk"
    hdiutil detach "/Volumes/Android NDK ${Version}"
} elseif ($IsLinux) {
    unzip "$install" -d "$ndkTemp"
    Write-Host "Moving NDK..."
    Move-Item "${ndkTemp}\android-ndk-${Version}" "$ndk"
} else {
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$install", "$ndkTemp")
    Write-Host "Moving NDK..."
    Move-Item "${ndkTemp}\android-ndk-${Version}" "$ndk"
}

# make sure that NDK is in ANDROID_NDK_HOME
Write-Host "##vso[task.setvariable variable=ANDROID_NDK_HOME;]$ndk";

exit $LASTEXITCODE
