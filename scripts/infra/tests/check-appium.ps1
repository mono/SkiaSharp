param(
    [Parameter(Mandatory)]
    [ValidateSet('Android', 'iOS', 'Mac', 'Windows')]
    [string] $Platform
)

$ErrorActionPreference = 'Stop'
Get-Command appium -ErrorAction Stop | Out-Null
$installedJson = & appium driver list --installed --json
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect preinstalled Appium drivers.' }
$installed = $installedJson | ConvertFrom-Json
$driver = switch ($Platform) {
    'Android' { 'uiautomator2' }
    'iOS' { 'xcuitest' }
    'Windows' { 'windows' }
    default { 'mac2' }
}
$entry = $installed.PSObject.Properties[$driver]
if ($null -eq $entry) {
    throw "Appium driver '$driver' is not installed. Provision the agent before selecting this job."
}
if ($Platform -in @('iOS', 'Mac')) {
    if ([string]::IsNullOrWhiteSpace($env:DEVELOPER_DIR) -or !(Test-Path $env:DEVELOPER_DIR -PathType Container)) {
        throw 'Selected Xcode is not installed. Set DEVELOPER_DIR to its existing developer directory.'
    }
}
if ($Platform -eq 'Mac') {
    if ([string]::IsNullOrWhiteSpace($env:SKIASHARP_APPIUM_MAC_WDA_ROOT) -or
        !(Test-Path (Join-Path $env:SKIASHARP_APPIUM_MAC_WDA_ROOT 'WebDriverAgentMac.xcodeproj'))) {
        throw 'Set SKIASHARP_APPIUM_MAC_WDA_ROOT to the preinstalled Mac2 WebDriverAgentMac source.'
    }
    & appium driver doctor mac2
    if ($LASTEXITCODE -ne 0) { throw 'Preinstalled Mac2 prerequisites failed doctor checks.' }
}
elseif ($Platform -eq 'Windows') {
    if ([string]::IsNullOrWhiteSpace($env:SKIASHARP_APPIUM_WINAPPDRIVER) -or
        !(Test-Path $env:SKIASHARP_APPIUM_WINAPPDRIVER -PathType Leaf)) {
        throw 'Set SKIASHARP_APPIUM_WINAPPDRIVER to the preinstalled WinAppDriver executable.'
    }
}
