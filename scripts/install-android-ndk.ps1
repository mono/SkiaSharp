$errorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

# download ndk
Write-Host "Downloading Android NDK..."
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\android-ndk-temp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile(
    "https://dl.google.com/android/repository/android-ndk-r15c-windows-x86_64.zip",
    "$env:USERPROFILE\android-ndk-temp\android-ndk.zip")

# unzip ndk
Write-Host "Extracting Android NDK..."
[System.IO.Compression.ZipFile]::ExtractToDirectory(
    "$env:USERPROFILE\android-ndk-temp\android-ndk.zip",
    "$env:USERPROFILE\android-ndk-temp")
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\android-ndk" | Out-Null

Write-Host "Moving Android NDK..."
Move-Item "$env:USERPROFILE\android-ndk-temp\android-ndk-r15c" "$env:USERPROFILE\android-ndk"

exit $LASTEXITCODE
