Param(
    [string] $Version,
    [string] $InstallDir
)

$ErrorActionPreference = 'Stop'

if ($IsMacOS -or $IsLinux) {
  $url = "https://dot.net/v1/dotnet-install.sh"
} else {
  $url = "https://dot.net/v1/dotnet-install.ps1"
}

Write-Host "Downloading .NET Installer..."
Invoke-WebRequest `
  -Uri "$install" `
  -OutFile (Split-Path $url -Leaf)

Write-Host "Installing .NET $Version..."
if ($IsMacOS) {
  & sh dotnet-install.sh --version $DOTNET_VERSION_PREVIEW --install-dir "$InstallDir" --verbose
} elseif ($IsLinux) {
  & bash dotnet-install.sh --version $DOTNET_VERSION_PREVIEW --install-dir "$InstallDir" --verbose
} else {
  .\dotnet-install.ps1 -Version $env:DOTNET_VERSION_PREVIEW -InstallDir "$InstallDir" -Verbose
}

Write-Host "Installed .NET Versions:"
& dotnet --list-sdks

exit $LASTEXITCODE
