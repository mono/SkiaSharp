$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsMacOS) {
    throw 'The Appium installer tests require macOS.'
}

$installer = Join-Path $PSScriptRoot '../install-appium.ps1'
$state = @{
    Commands = [Collections.Generic.List[string]]::new()
    FailCommand = ''
}

# Shadow both installers so these tests never install packages or change Appium.
function npm {
    $state.Commands.Add("npm $($args -join ' ')")
    if ($state.FailCommand -eq 'npm') {
        throw 'npm fixture failure'
    }
}

function appium {
    $state.Commands.Add("appium $($args -join ' ')")
    if ($state.FailCommand -eq 'appium') {
        throw 'appium fixture failure'
    }
}

foreach ($failCommand in @('', 'npm', 'appium')) {
    $state.FailCommand = $failCommand
    $state.Commands.Clear()
    $failure = $null
    try {
        & $installer
    } catch {
        $failure = $_
    }

    if ($failCommand) {
        if ($null -eq $failure -or $failure.Exception.Message -ne "$failCommand fixture failure") {
            throw "The installer did not propagate the $failCommand failure."
        }
    } elseif ($null -ne $failure) {
        throw $failure
    }

    $expected = @('npm install --global appium@3')
    if ($failCommand -ne 'npm') {
        $expected += 'appium driver install xcuitest@12.13.2'
    }
    if (($state.Commands -join "`n") -cne ($expected -join "`n")) {
        throw "Unexpected installer commands: $($state.Commands -join '; ')"
    }
}

Write-Host 'Appium installer tests passed (pinned driver, npm failure, driver failure; no packages installed).'
