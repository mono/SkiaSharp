$errorActionPreference = 'Stop'

if ($IsMacOS) {
    $url = "https://download.oracle.com/java/GA/jdk10/10.0.2/19aef61b38124481863b1413dce1855f/13/openjdk-10.0.2_osx-x64_bin.tar.gz"
} elseif ($IsLinux) {
    $url = "https://download.oracle.com/java/GA/jdk10/10.0.2/19aef61b38124481863b1413dce1855f/13/openjdk-10.0.2_linux-x64_bin.tar.gz"
} else {
    $url = "https://download.java.net/java/GA/jdk10/10.0.2/19aef61b38124481863b1413dce1855f/13/openjdk-10.0.2_windows-x64_bin.tar.gz"
}

$jdk = Join-Path "$HOME" "openjdk"
$jdkTemp = Join-Path "$HOME" "openjdk-temp"
$archive = Join-Path "$jdkTemp" "openjdk.tar.gz"

# download
Write-Host "Downloading OpenJDK to '$archive'..."
New-Item -ItemType Directory -Force -Path "$jdkTemp" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$url", "$archive")

# install
Write-Host "Extracting OpenJDK to '$jdk'..."
New-Item -ItemType Directory -Force -Path "$jdk" | Out-Null
if ($IsMacOS -or $IsLinux) {
    tar -vxzf "$archive" -C "$jdk"
} else {
    tar --force-local -vxzf "$archive" -C "$jdk"
}

if ($IsMacOS) {
    $java_home = Join-Path "$jdk" "jdk-10.0.2/Contents/Home"
} else {
    $java_home = Join-Path "$jdk" "jdk-10.0.2"
}
Write-Host "##vso[task.setvariable variable=JAVA_HOME;]$java_home"
Write-Host "Set environment variable to ($env:JAVA_HOME)"

exit $LASTEXITCODE
