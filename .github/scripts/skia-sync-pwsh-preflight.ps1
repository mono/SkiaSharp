#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

$requiredCommands = @(
    'ConvertFrom-Json',
    'ConvertTo-Json',
    'Get-Content',
    'Join-Path',
    'Select-String',
    'Set-Content',
    'Test-Path',
    'Write-Host'
)

foreach ($command in $requiredCommands) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Required PowerShell command '$command' is unavailable. PSModulePath='$env:PSModulePath'."
    }
}

$repoRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or -not $repoRoot) {
    throw 'PowerShell could not execute git in the repository.'
}

Write-Host "PowerShell $($PSVersionTable.PSVersion) ready at $PSHOME"
Write-Host "Repository root: $repoRoot"
