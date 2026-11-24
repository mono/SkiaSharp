Param(
    [Parameter(Mandatory)] [string[]] $Package
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'install-android-sdk.ps1')

$arguments = @('android', 'sdk', 'install', '--home', $env:ANDROID_SDK_ROOT)
foreach ($item in ($Package | Select-Object -Unique)) {
    $arguments += @('--package', $item)
}
dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "Android package installation failed: $($Package -join ', ')" }
