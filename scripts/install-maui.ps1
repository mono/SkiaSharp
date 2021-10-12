Param(
    [string] $InstallDir
)

$ErrorActionPreference = 'Stop'

$env:DOTNET_ROOT="$InstallDir"

Write-Host "Installing maui-check..."
& dotnet tool update --global redth.net.maui.check

Write-Host "Installing Maui..."
& maui-check `
  --verbose --ci --fix --non-interactive `
  --skip androidsdk `
  --skip xcode `
  --skip vswin `
  --skip vsmac `
  --skip edgewebview2

exit $LASTEXITCODE
