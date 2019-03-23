$errorActionPreference = 'Stop'

$jdk = Join-Path "$HOME" "openjdk"
$jdkTemp = Join-Path "$HOME" "openjdk-temp"
$url = "https://download.java.net/java/GA/jdk10/10.0.2/19aef61b38124481863b1413dce1855f/13/openjdk-10.0.2_windows-x64_bin.tar.gz"
$archive = Join-Path "$jdkTemp" "openjdk.tar.gz"

# download
Write-Host "Downloading OpenJDK to '$archive'..."
New-Item -ItemType Directory -Force -Path "$jdkTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$archive")

# install
Write-Host "Extracting OpenJDK to '$jdk'..."
New-Item -ItemType Directory -Force -Path "$jdk" | Out-Null
tar --force-local -vxzf "$archive" -C "$jdk"

exit $LASTEXITCODE
