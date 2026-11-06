param(
    [Parameter(Mandatory)]
    [ValidateSet('iOS', 'Android', 'Mac', 'Windows')]
    [string] $Platform
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if ($Platform -eq 'iOS' -and -not $IsMacOS) {
    throw 'The iOS sample tests require macOS.'
}
if ($Platform -eq 'Mac' -and -not $IsMacOS) {
    throw 'The Mac desktop sample tests require macOS.'
}
if ($Platform -eq 'Windows' -and ($IsMacOS -or $IsLinux)) {
    throw 'The Windows desktop sample tests require Windows.'
}

npm install --global appium@3

# This driver pins WDA 16.12.10, before 16.12.11 added a copy phase that
# requires Swift Testing libraries absent from the centrally selected Xcode 26.3.
# Remove the pin only after upstream supports earlier Xcode 26 installations:
# https://github.com/appium/WebDriverAgent/pull/1271
if ($Platform -eq 'iOS') {
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
} elseif ($Platform -eq 'Android') {
    # Keep the driver/server pair deterministic because the runtime job
    # preinstalls these exact server APKs once before starting any sessions.
    appium driver install uiautomator2@8.7.0
} elseif ($Platform -eq 'Mac') {
    appium driver install mac2
} else {
    appium driver install windows
    appium driver run windows install-wad
    $wad = Join-Path ${env:ProgramFiles(x86)} 'Windows Application Driver/WinAppDriver.exe'
    if (-not (Test-Path $wad -PathType Leaf)) {
        throw "WinAppDriver was not installed at the expected path: $wad"
    }
}
