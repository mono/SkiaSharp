[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int] $BuildId,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $Channel,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $AzdoToken,

    [ValidateRange(3, 4)]
    [int] $PublishingInfraVersion = 3,

    [ValidateNotNullOrEmpty()]
    [string] $MaestroApiEndpoint = 'https://maestro.dot.net',

    [ValidateNotNullOrEmpty()]
    [string] $SourceBranch = 'main',

    [string] $DarcPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($Channel)) {
    throw 'A non-empty Maestro channel is required.'
}

if ([string]::IsNullOrWhiteSpace($DarcPath)) {
    $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
    $ci = $true
    $disableConfigureToolsetImport = $true
    . (Join-Path $repoRoot 'eng/common/tools.ps1')
    $DarcPath = Get-Darc
}

if (-not (Test-Path -LiteralPath $DarcPath -PathType Leaf)) {
    throw "Darc executable '$DarcPath' does not exist."
}

$darcArguments = @(
    'add-build-to-channel'
    '--id', $BuildId
    '--publishing-infra-version', $PublishingInfraVersion
    '--channel', $Channel.Trim()
    '--source-branch', $SourceBranch
    '--azdev-pat', $AzdoToken
    '--bar-uri', $MaestroApiEndpoint
    '--ci'
    '--verbose'
)

& $DarcPath @darcArguments
if ($LASTEXITCODE -ne 0) {
    throw "Darc failed to promote BAR build '$BuildId' to '$Channel' with exit code $LASTEXITCODE."
}

Write-Host "BAR build '$BuildId' was promoted to '$Channel'."
