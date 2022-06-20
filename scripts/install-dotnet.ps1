Param(
  [string] $Version,
  [string] $InstallDir,
  [string] $FeedUrl = "https://dotnetbuilds.blob.core.windows.net/public"
)

$ErrorActionPreference = 'Stop'

$dotnetDll = Join-Path "$InstallDir" "sdk" "$Version" "dotnet.dll"
if (Test-Path $dotnetDll) {
  Write-Host ".NET already installed."
} else {
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
}

if (-not $env:PATH.Contains($InstallDir)) {
  $env:PATH = "$InstallDir" + [IO.Path]::PathSeparator + "$env:PATH"
  Write-Host "##vso[task.setvariable variable=PATH;]$env:PATH"
}

Write-Host "Checking all dotnet info..."
& dotnet --info

exit $LASTEXITCODE
