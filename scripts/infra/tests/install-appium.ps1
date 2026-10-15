param(
    [Parameter(Mandatory)]
    [ValidateSet('Android', 'iOS', 'Mac', 'Windows')]
    [string] $Platform
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

npm install --global appium@3

$driver = switch ($Platform) {
    'Android' { 'uiautomator2' }
    'iOS' { 'xcuitest' }
    'Mac' { 'mac2' }
    'Windows' { 'windows' }
}

appium driver install $driver
