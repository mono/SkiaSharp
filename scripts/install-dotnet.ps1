Param(
    [string] $Version,
    [string] $InstallDir,
    [string] $FeedUrl = "https://dotnetbuilds.blob.core.windows.net/public"
)

$ErrorActionPreference = 'Stop'

if ($IsMacOS -or $IsLinux) {
  $url = "https://dot.net/v1/dotnet-install.sh"
} else {
  $url = "https://dot.net/v1/dotnet-install.ps1"
}

Write-Host "Downloading .NET Installer..."
Invoke-WebRequest `
  -Uri "$url" `
  -OutFile (Split-Path $url -Leaf)

Write-Host "Installing .NET $Version..."
if ($IsMacOS) {
  & sh dotnet-install.sh --version "$Version" --install-dir "$InstallDir" --azure-feed "$FeedUrl" --verbose
} elseif ($IsLinux) {
  & bash dotnet-install.sh --version "$Version" --install-dir "$InstallDir" --azure-feed "$FeedUrl" --verbose
} else {
  .\dotnet-install.ps1 -Version "$Version" -InstallDir "$InstallDir" -AzureFeed "$FeedUrl" -Verbose
}

Write-Host "Installed .NET Versions:"
& dotnet --list-sdks

exit $LASTEXITCODE
