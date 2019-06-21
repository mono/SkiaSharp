Param(
    [string] $Version = "r20"
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

if ($IsMacOS) {
    $platform = "darwin-x86_64"
} elseif ($IsLinux) {
    $platform = "linux-x86_64"
} else {
    $platform = "windows-x86_64"
}

$url = "https://dl.google.com/android/repository/android-ndk-${Version}-${platform}.zip"
$ndk = "$HOME/android-ndk"
$ndkTemp = "$HOME/android-ndk-temp"
$install = "$ndkTemp/android-ndk.zip"

# download
Write-Host "Downloading NDK..."
New-Item -ItemType Directory -Force -Path "$ndkTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$install")

# extract
Write-Host "Extracting NDK..."
if ($IsMacOS -or $IsLinux) {
    # use the native command to preserve permissions
    unzip "$install" -d "$ndkTemp"
}else {
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$install", "$ndkTemp")
}

# move / rename
Write-Host "Moving NDK..."
Move-Item "${ndkTemp}\android-ndk-${Version}" "$ndk"

exit $LASTEXITCODE
