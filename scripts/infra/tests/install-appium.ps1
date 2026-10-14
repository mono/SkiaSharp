param(
    [Parameter(Mandatory)]
    [ValidateSet('Android', 'iOS', 'Mac', 'Windows')]
    [string] $Platform
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$logs = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../output/logs'))
New-Item -ItemType Directory -Path $logs -Force | Out-Null
Start-Transcript -Path (Join-Path $logs "install-appium-$Platform.log") -Append | Out-Null

function Invoke-InstallerCommand {
    param([string] $Command, [string[]] $Arguments)

    Write-Host "> $Command $($Arguments -join ' ')"
    & $Command @Arguments 2>&1 | ForEach-Object {
        Write-Host $_
        $_
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Command '$Command $($Arguments -join ' ')' failed with exit code $LASTEXITCODE."
    }
}

try {
    if ($env:TF_BUILD -ne 'True' -or [string]::IsNullOrWhiteSpace($env:AGENT_TEMPDIRECTORY) -or
        !(Test-Path $env:AGENT_TEMPDIRECTORY -PathType Container)) {
        throw 'Appium installation is CI-only and requires the job Agent.TempDirectory.'
    }

    $npm = if ($IsWindows) { 'npm.cmd' } else { 'npm' }
    Get-Command node, $npm -ErrorAction Stop | Out-Null
    $nodeVersion = [version](([string](Invoke-InstallerCommand 'node' @('--version'))).TrimStart('v'))
    $npmVersion = [version]([string](Invoke-InstallerCommand $npm @('--version')))
    if (!(($nodeVersion.Major -eq 20 -and $nodeVersion -ge [version]'20.19.0') -or
        ($nodeVersion.Major -eq 22 -and $nodeVersion -ge [version]'22.12.0') -or $nodeVersion.Major -ge 24) -or
        $npmVersion.Major -lt 10) {
        throw 'Appium 3 requires Node ^20.19.0 || ^22.12.0 || >=24.0.0 and npm >=10. Provision Node 22.x with UseNode@1.'
    }
    if ($Platform -in @('iOS', 'Mac')) {
        if ([string]::IsNullOrWhiteSpace($env:DEVELOPER_DIR) -or
            !(Test-Path $env:DEVELOPER_DIR -PathType Container)) {
            throw 'Selected Xcode is not installed. Set DEVELOPER_DIR to its existing developer directory.'
        }
    }

    $identity = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmssfffffff', [Globalization.CultureInfo]::InvariantCulture)
    $root = Join-Path $env:AGENT_TEMPDIRECTORY "skiasharp-appium-$identity-$PID"
    $prefix = Join-Path $root 'cli'
    $env:APPIUM_HOME = Join-Path $root 'drivers'
    $env:npm_config_cache = Join-Path $root 'npm-cache'
    New-Item -ItemType Directory -Path $prefix, $env:APPIUM_HOME, $env:npm_config_cache | Out-Null

    Invoke-InstallerCommand $npm @('install', '--prefix', $prefix, '--global=false', 'appium@3') | Out-Null
    $bin = Join-Path $prefix 'node_modules/.bin'
    $appium = Join-Path $bin $(if ($IsWindows) { 'appium.cmd' } else { 'appium' })
    if (!(Test-Path $appium -PathType Leaf)) {
        throw "Appium CLI installation did not produce '$appium'."
    }
    $env:PATH = "$bin$([IO.Path]::PathSeparator)$env:PATH"
    Write-Host "##vso[task.prependpath]$bin"
    Write-Host "##vso[task.setvariable variable=APPIUM_HOME]$env:APPIUM_HOME"
    Invoke-InstallerCommand $appium @('--version') | Out-Null

    $driver = switch ($Platform) {
        'Android' { 'uiautomator2' }
        'iOS' { 'xcuitest' }
        'Mac' { 'mac2' }
        'Windows' { 'windows' }
    }
    Invoke-InstallerCommand $appium @('driver', 'install', $driver) | Out-Null
    Invoke-InstallerCommand $appium @('driver', 'list', '--installed') | Out-Null
    $driverRoot = Join-Path $env:APPIUM_HOME "node_modules/appium-$driver-driver"
    if (!(Test-Path $driverRoot -PathType Container)) {
        throw "Selected Appium driver '$driver' was not installed under APPIUM_HOME: '$driverRoot'."
    }

    if ($Platform -eq 'Mac') {
        Write-Warning 'Mac2 still requires an interactive desktop and XCTest Accessibility/Automation permissions. This installer does not grant permissions or change Xcode selection.'
        Invoke-InstallerCommand $appium @('driver', 'doctor', 'mac2') | Out-Null
    }
    elseif ($Platform -eq 'Windows') {
        Write-Warning 'WinAppDriver installation requires administrator privileges. Tests still require Developer Mode and an interactive, unlocked Windows desktop; this installer does not configure these.'
        Invoke-InstallerCommand $appium @('driver', 'run', 'windows', 'install-wad') | Out-Null
        $wad = $null
        # The driver hook starts msiexec asynchronously.
        $installation = [Diagnostics.Stopwatch]::StartNew()
        while (!$wad -and $installation.Elapsed.TotalSeconds -lt 60) {
            foreach ($programFiles in @(${env:ProgramFiles(x86)}, $env:ProgramFiles)) {
                if ([string]::IsNullOrWhiteSpace($programFiles)) { continue }
                $candidate = Join-Path $programFiles 'Windows Application Driver/WinAppDriver.exe'
                if (Test-Path $candidate -PathType Leaf) {
                    $wad = $candidate
                    break
                }
            }
            if (!$wad) { Start-Sleep -Milliseconds 250 }
        }
        if (!$wad) {
            throw 'WinAppDriver installation did not produce Windows Application Driver/WinAppDriver.exe under ProgramFiles(x86) or ProgramFiles.'
        }
    }
}
catch {
    Write-Host $_.Exception.Message
    throw
}
finally {
    Stop-Transcript | Out-Null
}
