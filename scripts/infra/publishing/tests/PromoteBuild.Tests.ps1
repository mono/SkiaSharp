$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/publishing/promote-build.ps1'
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-promotion-tests-$([Guid]::NewGuid().ToString('N'))"
$capturePath = Join-Path $testRoot 'arguments.txt'

try {
    New-Item $testRoot -ItemType Directory -Force | Out-Null

    if ($IsWindows) {
        $fakeDarc = Join-Path $testRoot 'darc.cmd'
        @'
@echo off
for %%A in (%*) do echo %%~A>>"%DARC_CAPTURE%"
exit /b %DARC_EXIT_CODE%
'@ | Set-Content $fakeDarc
    } else {
        $fakeDarc = Join-Path $testRoot 'darc'
        @'
#!/bin/sh
: > "$DARC_CAPTURE"
for argument in "$@"; do
    printf '%s\n' "$argument" >> "$DARC_CAPTURE"
done
exit "$DARC_EXIT_CODE"
'@ | Set-Content $fakeDarc
        & chmod +x $fakeDarc
    }

    $env:DARC_CAPTURE = $capturePath
    $env:DARC_EXIT_CODE = '0'

    & $scriptPath `
        -BuildId 327797 `
        -Channel 'General Testing' `
        -AzdoToken 'test-token' `
        -DarcPath $fakeDarc

    $actualArguments = @(Get-Content $capturePath)
    $expectedArguments = @(
        'add-build-to-channel'
        '--id'
        '327797'
        '--publishing-infra-version'
        '3'
        '--channel'
        'General Testing'
        '--source-branch'
        'main'
        '--azdev-pat'
        'test-token'
        '--bar-uri'
        'https://maestro.dot.net'
        '--ci'
        '--verbose'
    )

    if (($actualArguments -join "`n") -ne ($expectedArguments -join "`n")) {
        throw "Unexpected Darc arguments.`nExpected:`n$($expectedArguments -join "`n")`nActual:`n$($actualArguments -join "`n")"
    }
    if ($actualArguments -contains '--default-channels') {
        throw 'Explicit testing promotion must not depend on default channel mappings.'
    }

    $env:DARC_EXIT_CODE = '19'
    $failurePropagated = $false
    try {
        & $scriptPath `
            -BuildId 327798 `
            -Channel 'General Testing' `
            -AzdoToken 'test-token' `
            -DarcPath $fakeDarc
    } catch {
        $failurePropagated = $_.Exception.Message -like "*exit code 19*"
    }

    if (-not $failurePropagated) {
        throw 'A failed Darc promotion did not fail the build.'
    }

    Write-Host 'BAR promotion tests passed.'
} finally {
    Remove-Item Env:DARC_CAPTURE -ErrorAction Ignore
    Remove-Item Env:DARC_EXIT_CODE -ErrorAction Ignore
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
