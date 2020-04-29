Param(
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$version = "13.0.2"
if ($IsMacOS) {
    $ext = "tar.gz"
    $url = "https://download.java.net/java/GA/jdk13.0.2/d4173c853231432d94f001e99d882ca7/8/GPL/openjdk-13.0.2_osx-x64_bin.tar.gz"
} elseif ($IsLinux) {
    $ext = "tar.gz"
    $url = "https://download.java.net/java/GA/jdk13.0.2/d4173c853231432d94f001e99d882ca7/8/GPL/openjdk-13.0.2_linux-x64_bin.tar.gz"
} else {
    $ext = "zip"
    $url = "https://download.java.net/java/GA/jdk13.0.2/d4173c853231432d94f001e99d882ca7/8/GPL/openjdk-13.0.2_windows-x64_bin.zip"
}

$jdk = Join-Path "$HOME" "openjdk"
if ($InstallDestination) {
    $jdk = $InstallDestination
}
Write-Host "Install destination is '$jdk'..."

$jdkTemp = Join-Path "$HOME" "openjdk-temp"
$archive = Join-Path "$jdkTemp" "openjdk.$ext"

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
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$archive", "$jdk")
}

# set the JAVA_HOME
if ($IsMacOS) {
    $java_home = Join-Path "$jdk" "jdk-$version.jdk/Contents/Home"
} else {
    $java_home = Join-Path "$jdk" "jdk-$version"
}
Write-Host "##vso[task.setvariable variable=JAVA_HOME;]$java_home"

# make sure that JAVA_HOME/bin is in the PATH
$javaBin = Join-Path "$java_home" "bin"
Write-Host "##vso[task.setvariable variable=PATH;]$javaBin;$env:PATH";

exit $LASTEXITCODE
