Param(
  [string] $InstallDir
)

$ErrorActionPreference = 'Stop'

$dotnetSdk = Join-Path "$InstallDir" "sdk"
if (Test-Path $dotnetSdk) {
  $versions = Get-ChildItem $dotnetSdk
  foreach ($version in $versions) {
    $root = Join-Path $version "Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets"
    if (Test-Path $root) {
      if (Test-Path (Join-Path $root Microsoft.WinFx.props)) {
        Move-Item (Join-Path $root Microsoft.WinFx.props) (Join-Path $root Microsoft.WinFX.props)
      }
      if (Test-Path (Join-Path $root Microsoft.WinFx.targets)) {
        Move-Item (Join-Path $root Microsoft.WinFx.targets) (Join-Path $root Microsoft.WinFX.targets)
      }
    }
  }
} else {
  Write-Host "No .NET installed."
}

exit $LASTEXITCODE
