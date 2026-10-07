Param(
    [ValidatePattern('^\d+(\.\d+)*\+\d+$')]
    [string] $Version = '21.0.10+7',
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

$downloadVersion = $Version.Split('+')[0]
$majorVersion = [int] $downloadVersion.Split('.')[0]
$installedJavaHome = [Environment]::GetEnvironmentVariable("JAVA_HOME_${majorVersion}_X64")
if ($installedJavaHome) {
    Write-Host "Using the existing JDK at '$installedJavaHome'..."
    $java_home = $installedJavaHome
} else {
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

    if ($IsMacOS) {
        $ext = "tar.gz"
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$downloadVersion-macOS-x64.tar.gz"
    } elseif ($IsLinux) {
        $ext = "tar.gz"
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$downloadVersion-linux-x64.tar.gz"
    } else {
        $ext = "zip"
        $url = "https://aka.ms/download-jdk/microsoft-jdk-$downloadVersion-windows-x64.zip"
    }

    $jdk = Join-Path "$HOME_DIR" "openjdk"
    if ($InstallDestination) {
        $jdk = $InstallDestination
    }
    Write-Host "Install destination is '$jdk'..."

    $jdkTemp = Join-Path "$HOME_DIR" "openjdk-temp"
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
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to extract OpenJDK (exit code $LASTEXITCODE)."
        }
    } else {
        [System.IO.Compression.ZipFile]::ExtractToDirectory("$archive", "$jdk")
    }

    # set the JAVA_HOME
    if ($IsMacOS) {
        $java_home = Join-Path "$jdk" "jdk-$Version/Contents/Home"
    } else {
        $java_home = Join-Path "$jdk" "jdk-$Version"
    }
}

$javaBin = Join-Path "$java_home" "bin"
$executableExtension = if ($IsMacOS -or $IsLinux) { "" } else { ".exe" }
foreach ($tool in @("java", "javac")) {
    $executable = Join-Path "$javaBin" "$tool$executableExtension"
    if (-not (Test-Path -LiteralPath "$executable" -PathType Leaf)) {
        throw "The selected JDK is missing '$executable'."
    }
    $versionOutput = & "$executable" -version 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "$tool -version failed at '$java_home' (exit code $LASTEXITCODE): $versionOutput"
    }
    Write-Host ($versionOutput -join [Environment]::NewLine)
    $versionPattern = if ($tool -eq "java") { 'version "(\d+)\.' } else { '^javac (\d+)\.' }
    if (($versionOutput -join "`n") -notmatch $versionPattern -or [int] $Matches[1] -ne $majorVersion) {
        throw "Expected JDK $majorVersion at '$java_home', but $tool reported: $versionOutput"
    }
}

Write-Host "##vso[task.setvariable variable=JAVA_HOME;]$java_home"
$env:JAVA_HOME = "$java_home"

# Prepend even if already present later, so an older Java cannot take precedence.
$env:PATH = "$javaBin" + [IO.Path]::PathSeparator + "$env:PATH"
Write-Host "##vso[task.prependpath]$javaBin"

exit 0
