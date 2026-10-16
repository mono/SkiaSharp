$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if (-not $IsMacOS) {
    throw 'The iOS sample tests require macOS.'
}

npm install --global appium@3
appium driver install xcuitest
