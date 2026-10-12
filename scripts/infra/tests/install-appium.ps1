param(
    [Parameter(Mandatory)]
    [ValidateSet('Android', 'iOS', 'MacCatalyst', 'macOS', 'Windows')]
    [string] $Platform,
    [ValidateSet('', '24', '37')]
    [string] $AndroidApi = ''
)

$ErrorActionPreference = 'Stop'
if ($env:TF_BUILD -ne 'True') {
    throw 'This provisioning entry point is for the explicitly selected CI job, not a shared personal workstation.'
}

function Invoke-Checked([string] $Program, [string[]] $Arguments) {
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Program failed ($LASTEXITCODE): $($Arguments -join ' ')"
    }
}

$env:APPIUM_HOME = Join-Path $env:AGENT_TEMPDIRECTORY 'skiasharp-appium'
New-Item -ItemType Directory -Force $env:APPIUM_HOME | Out-Null
Write-Host "##vso[task.setvariable variable=APPIUM_HOME]$env:APPIUM_HOME"
Invoke-Checked npm @('install', '--global', 'appium@3.6.0')

$driver = switch ($Platform) {
    'Android' {
        if ($AndroidApi -eq '24') { 'uiautomator2@5.0.7' }
        elseif ($AndroidApi -eq '37') { 'uiautomator2@8.5.0' }
        else { throw 'Declare the exact Android API job: driver 6+ does not support API 24.' }
    }
    'iOS' { 'xcuitest@12.8.0' }
    'Windows' { 'windows@6.3.1' }
    default { 'mac2@4.2.0' }
}
Invoke-Checked appium @('driver', 'install', $driver)
Invoke-Checked appium @('driver', 'list', '--installed')

if ($Platform -in @('MacCatalyst', 'macOS')) {
    $wda = Join-Path $env:APPIUM_HOME 'node_modules/appium-mac2-driver/WebDriverAgentMac'
    if (!(Test-Path (Join-Path $wda 'WebDriverAgentMac.xcodeproj'))) {
        throw "Installed Mac2 driver source is missing: $wda"
    }
    Write-Host "##vso[task.setvariable variable=SKIASHARP_APPIUM_MAC_WDA_ROOT]$wda"
    Invoke-Checked appium @('driver', 'doctor', 'mac2')
}
elseif ($Platform -eq 'Windows') {
    $msi = Join-Path $env:AGENT_TEMPDIRECTORY 'WindowsApplicationDriver-1.2.1.msi'
    Invoke-WebRequest 'https://github.com/microsoft/WinAppDriver/releases/download/v1.2.1/WindowsApplicationDriver.msi' -OutFile $msi
    $installer = Start-Process msiexec.exe -ArgumentList @('/i', "`"$msi`"", '/quiet', '/norestart') -Wait -PassThru
    if ($installer.ExitCode -notin @(0, 3010)) {
        throw "WinAppDriver installer failed: $($installer.ExitCode)"
    }
    $wad = Join-Path ${env:ProgramFiles(x86)} 'Windows Application Driver/WinAppDriver.exe'
    if (!(Test-Path $wad)) { throw "WinAppDriver executable is missing: $wad" }
    Write-Host "##vso[task.setvariable variable=SKIASHARP_APPIUM_WINAPPDRIVER]$wad"
    Write-Host 'Windows cases require an interactive unlocked desktop and Developer Mode. The runner checks these; no security/agent-session settings are changed here.'
}
