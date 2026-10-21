$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if (-not $IsMacOS) {
    throw 'The iOS sample tests require macOS.'
}

npm install --global appium@3

# This driver pins WDA 16.12.10, before 16.12.11 added a copy phase that
# requires Swift Testing libraries absent from the centrally selected Xcode 26.3.
# Remove the pin only after upstream supports earlier Xcode 26 installations:
# https://github.com/appium/WebDriverAgent/pull/1271
appium driver install xcuitest@12.13.2

$wda = Join-Path $PSScriptRoot '../../../output/appium/wda'
New-Item (Split-Path $wda -Parent) -ItemType Directory -Force | Out-Null
if (Test-Path $wda) {
    Remove-Item $wda -Recurse -Force
}
appium driver run xcuitest download-wda -- --outdir=$wda --kind=sim --platform=ios

$wdaApp = Join-Path $wda 'WebDriverAgentRunner-Runner.app'
if (-not (Test-Path $wdaApp -PathType Container)) {
    throw "The XCUITest driver did not provide the prebuilt simulator WDA app: $wdaApp"
}
