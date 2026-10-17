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
