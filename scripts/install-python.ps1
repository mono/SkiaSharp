Param(
    [string] $Version,
    [string] $Arch = 'x64'
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$HOME_DIR = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }

$destDir = Join-Path "$env:AGENT_TOOLSDIRECTORY" "Python" "$Version" "$Arch"
$completeFile = "$destDir.complete"

if (Test-Path $completeFile) {
    Write-Host "Python $Version ($Arch) already installed."
    exit 0;
} else {
    Write-Host "No matching Python found."
}

Write-Host "Downloading manifest..."

$pythonManifestUri = "https://raw.githubusercontent.com/actions/python-versions/main/versions-manifest.json"
$pythonManifest = Invoke-WebRequest -Uri $pythonManifestUri | ConvertFrom-Json

if ($IsMacOS) {
    $platform = "darwin"
} elseif ($IsLinux) {
    $platform = "linux"
} else {
    $platform = "win32"
}

$downloadUrl = (($pythonManifest
    | Where-Object { $_.version -eq $Version }
    | Select-Object -First 1).files
    | Where-Object { $_.platform -eq $platform -and $_.arch -eq $Arch }
    | Select-Object -First 1).download_url

# download
$tempDir = Join-Path "$HOME_DIR" "python-temp"
$archive = Join-Path "$tempDir" (Split-Path $downloadUrl -Leaf)

Write-Host "Downloading Python package '$downloadUrl' to '$archive'..."
New-Item -ItemType Directory -Force -Path "$tempDir" | Out-Null
(New-Object System.Net.WebClient).DownloadFile("$downloadUrl", "$archive")

# install
Write-Host "Extracting Python to '$destDir'..."
New-Item -ItemType Directory -Force -Path "$destDir" | Out-Null
if ($IsMacOS -or $IsLinux) {
    tar -vxzf "$archive" -C "$destDir"
} else {
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$archive", "$destDir")
}

Write-Host "Extraction complete."
New-Item -ItemType File -Force -Path "$completeFile" | Out-Null

exit $LASTEXITCODE
