Param(
    [Parameter(Mandatory = $true)]
    [string] $InstallationPath,
    [ValidateSet('x86', 'x64')]
    [string] $Architecture = 'x64'
)

$ErrorActionPreference = 'Stop'

function Get-ToolsetVersion {
    $build = Join-Path $InstallationPath 'VC\Auxiliary\Build'
    $version = (Get-Content (Join-Path $build 'Microsoft.VCToolsVersion.default.txt') -Raw).Trim()
    $parsed = [Version]::Parse($version)
    # Match the v143 selection in windows-shared.cake, not the VS 2026 default.
    if ($parsed.Major -ne 14 -or $parsed.Minor -lt 30 -or $parsed.Minor -ge 50) {
        $version = (Get-Content (Join-Path $build 'Microsoft.VCToolsVersion.v143.default.txt') -Raw).Trim()
    }
    return $version
}

function Test-SpectreLibraries([string] $Version) {
    $libraries = Join-Path $InstallationPath "VC\Tools\MSVC\$Version\lib\spectre\$Architecture"
    foreach ($name in @('libcmt.lib', 'libcpmt.lib', 'libvcruntime.lib')) {
        if (!(Test-Path (Join-Path $libraries $name) -PathType Leaf)) {
            return $false
        }
    }
    return $true
}

$version = Get-ToolsetVersion
Write-Host "Checking MSVC $version Spectre libraries for $Architecture in $InstallationPath"
if (Test-SpectreLibraries $version) {
    Write-Host 'Matching Spectre libraries are already installed.'
    exit 0
}

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$instances = & $vswhere -all -prerelease -products '*' -format json
if ($LASTEXITCODE -ne 0) {
    throw "vswhere failed with exit code $LASTEXITCODE."
}
$instance = @($instances | ConvertFrom-Json | Where-Object { $_.installationPath -eq $InstallationPath })
if ($instance.Count -ne 1) {
    throw "Could not identify the selected Visual Studio installation: $InstallationPath"
}

$catalogPath = Join-Path $env:ProgramData "Microsoft\VisualStudio\Packages\_Instances\$($instance[0].instanceId)\catalog.json"
$catalog = Get-Content $catalogPath -Raw | ConvertFrom-Json
$parsed = [Version]::Parse($version)
$prefix = [Regex]::Escape("Microsoft.VisualStudio.Component.VC.$($parsed.Major).$($parsed.Minor).")
$components = @($catalog.packages |
    Where-Object { $_.type -eq 'Component' -and $_.id -match "^$prefix\d+\.\d+\.x86\.x64\.Spectre$" } |
    Select-Object -ExpandProperty id -Unique)
if ($components.Count -ne 1) {
    throw "The selected Visual Studio catalogue has no unique matching Spectre component for MSVC $version."
}

$installer = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\setup.exe"
Write-Host "Installing $($components[0]) into $InstallationPath"
$process = Start-Process -FilePath $installer -ArgumentList @(
    'modify', '--installPath', "`"$InstallationPath`"",
    '--add', $components[0], '--quiet', '--norestart'
) -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Visual Studio Spectre component installation exited with code $($process.ExitCode)."
}

$version = Get-ToolsetVersion
if (!(Test-SpectreLibraries $version)) {
    throw "Visual Studio installation completed without matching MSVC $version Spectre libraries for $Architecture."
}
Write-Host "Verified MSVC $version Spectre libraries for $Architecture."
