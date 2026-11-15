Param(
    [Parameter(Mandatory)] [string[]] $API
)

$ErrorActionPreference = 'Stop'

$packages = @($API -split ',' | ForEach-Object { "platforms;android-$($_.Trim())" })
& (Join-Path $PSScriptRoot 'install-android-package.ps1') -Package $packages
