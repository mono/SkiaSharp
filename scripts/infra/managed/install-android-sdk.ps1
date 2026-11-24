Param(
    [string] $InstallDestination = $null
)

$ErrorActionPreference = 'Stop'

$sdk = $InstallDestination
if (-not $sdk) { $sdk = $env:ANDROID_SDK_ROOT }
if (-not $sdk) { $sdk = $env:ANDROID_HOME }
if (-not $sdk) {
    $discovered = dotnet android sdk find
    if ($LASTEXITCODE -ne 0) { throw 'Android SDK discovery failed.' }
    $sdk = ($discovered -join "`n").Trim()
}
if (-not $sdk) {
    $homeDir = if ($env:HOME) { $env:HOME } else { $env:USERPROFILE }
    $sdk = Join-Path $homeDir 'android-sdk'
}
$sdk = [IO.Path]::GetFullPath($sdk)

$info = dotnet android sdk info --home "$sdk" --format json
if ($LASTEXITCODE -ne 0) { throw "Failed to read Android SDK information at $sdk." }
$info = ($info -join "`n") | ConvertFrom-Json
if (-not $info.HasCmdlineTools) {
    dotnet android sdk download --home "$sdk"
    if ($LASTEXITCODE -ne 0) { throw "Android SDK download failed at $sdk." }
}

Write-Host "Using Android SDK at $sdk."
foreach ($variable in @{
    ANDROID_HOME = $sdk
    ANDROID_SDK_ROOT = $sdk
    AndroidSdkDirectory = $sdk
}.GetEnumerator()) {
    Write-Host "##vso[task.setvariable variable=$($variable.Key);]$($variable.Value)"
    Set-Item "Env:$($variable.Key)" $variable.Value
}
