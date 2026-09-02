#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SkiaSharpBaseBranch,

    [Parameter(Mandatory)]
    [string] $SkiaBaseBranch,

    [switch] $Push
)

$ErrorActionPreference = 'Stop'

function Invoke-Native {
    param(
        [Parameter(Mandatory)]
        [string] $Command,

        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    $output = @(& $Command @Arguments 2>&1)
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "$Command $($Arguments -join ' ') failed:`n$($output -join "`n")"
    }
    return ($output -join "`n").Trim()
}

function Get-RemoteBranchSha {
    param([string] $RepositoryUrl, [string] $Branch)

    $result = Invoke-Native git @('ls-remote', '--heads', $RepositoryUrl, "refs/heads/$Branch")
    if (-not $result) {
        return $null
    }
    return ($result -split '\s+')[0]
}

function Get-GitHubRepository {
    param([string] $RepositoryUrl)

    $match = [regex]::Match($RepositoryUrl, '(?i)github\.com[/:](?<repository>[^/:\s]+/[^/\s]+?)(?:\.git)?$')
    if (-not $match.Success) {
        throw "Cannot derive a GitHub owner/repository from $RepositoryUrl."
    }
    return $match.Groups['repository'].Value
}

function Assert-Destination {
    param(
        [string] $Repository,
        [string] $RepositoryUrl,
        [string] $Branch,
        [string] $ExpectedSha
    )

    $actual = Get-RemoteBranchSha $RepositoryUrl $Branch
    if ($actual -and $actual -ne $ExpectedSha) {
        throw "$Repository $Branch already points to $actual; expected $ExpectedSha. Existing release branches are never moved."
    }
    return $actual
}

function New-RemoteBranch {
    param([string] $Repository, [string] $Branch, [string] $Sha)

    $null = Invoke-Native gh @(
        'api', '--method', 'POST',
        "repos/$Repository/git/refs",
        '-f', "ref=refs/heads/$Branch",
        '-f', "sha=$Sha"
    )
}

$repoRoot = Invoke-Native git @('rev-parse', '--show-toplevel')
Set-Location $repoRoot
$null = Invoke-Native git @('check-ref-format', '--branch', $SkiaSharpBaseBranch)
$null = Invoke-Native git @('check-ref-format', '--branch', $SkiaBaseBranch)

$parentUrl = Invoke-Native git @('remote', 'get-url', 'origin')

$null = Invoke-Native git @(
    'fetch', '--no-tags', 'origin',
    "+refs/heads/${SkiaSharpBaseBranch}:refs/remotes/origin/${SkiaSharpBaseBranch}"
)
$parentBaseSha = Invoke-Native git @(
    'rev-parse', "refs/remotes/origin/${SkiaSharpBaseBranch}^{commit}"
)

if ($SkiaSharpBaseBranch -match '^release/(?<version>\d+\.\d+)\.x$') {
    Write-Host "Servicing branch: $SkiaSharpBaseBranch"
    Write-Host 'No release branches are needed for a servicing sync.'
    exit 0
}
if ($SkiaSharpBaseBranch -ne 'main') {
    throw "SkiaSharp base branch must be main or release/A.B.x; got $SkiaSharpBaseBranch."
}

$versions = Invoke-Native git @('show', "${parentBaseSha}:scripts/VERSIONS.txt")
$versionMatch = [regex]::Match(
    $versions,
    '(?m)^SkiaSharp\s+nuget\s+(?<major>\d+)\.(?<minor>\d+)\.\d+(?:[-+]\S+)?\s*$'
)
if (-not $versionMatch.Success) {
    throw "Cannot derive the current SkiaSharp product line from scripts/VERSIONS.txt at $parentBaseSha."
}
$ReleaseBranch = "release/$($versionMatch.Groups['major'].Value).$($versionMatch.Groups['minor'].Value).x"
$null = Invoke-Native git @('check-ref-format', '--branch', $ReleaseBranch)

$nativeUrl = Invoke-Native git @(
    'config', '--blob', "${parentBaseSha}:.gitmodules",
    '--get', 'submodule.externals/skia.url'
)
$parentRepository = Get-GitHubRepository $parentUrl
$nativeRepository = Get-GitHubRepository $nativeUrl

$treeEntry = Invoke-Native git @('ls-tree', $parentBaseSha, '--', 'externals/skia')
$treeParts = $treeEntry -split '\s+'
if ($treeParts.Count -lt 3 -or $treeParts[1] -ne 'commit') {
    throw "$parentBaseSha does not contain the externals/skia gitlink."
}
$nativeBaseSha = $treeParts[2]
$nativeBranchSha = Get-RemoteBranchSha $nativeUrl $SkiaBaseBranch
if (-not $nativeBranchSha) {
    throw "mono/skia branch $SkiaBaseBranch does not exist."
}
if ($nativeBranchSha -ne $nativeBaseSha) {
    throw "SkiaSharp $SkiaSharpBaseBranch points to mono/skia $nativeBaseSha, but $SkiaBaseBranch points to $nativeBranchSha."
}

$nativeExisting = Assert-Destination $nativeRepository $nativeUrl $ReleaseBranch $nativeBaseSha
$parentExisting = Assert-Destination $parentRepository $parentUrl $ReleaseBranch $parentBaseSha

Write-Host "Release branch:     $ReleaseBranch"
Write-Host "$nativeRepository source: $SkiaBaseBranch @ $nativeBaseSha$(if ($nativeExisting) { ' (release branch already exists)' })"
Write-Host "$parentRepository source: $SkiaSharpBaseBranch @ $parentBaseSha$(if ($parentExisting) { ' (release branch already exists)' })"

if (-not $Push) {
    Write-Host 'DRY RUN: no remote refs were created. Re-run with -Push to create them.'
    exit 0
}

# Re-read every source and destination immediately before the first write.
$parentBaseNow = Get-RemoteBranchSha $parentUrl $SkiaSharpBaseBranch
$nativeBaseNow = Get-RemoteBranchSha $nativeUrl $SkiaBaseBranch
if ($parentBaseNow -ne $parentBaseSha -or $nativeBaseNow -ne $nativeBaseSha) {
    throw 'A source branch changed after preflight. Run the script without -Push again.'
}
$nativeExisting = Assert-Destination $nativeRepository $nativeUrl $ReleaseBranch $nativeBaseSha
$parentExisting = Assert-Destination $parentRepository $parentUrl $ReleaseBranch $parentBaseSha

if (-not $nativeExisting) {
    New-RemoteBranch $nativeRepository $ReleaseBranch $nativeBaseSha
    Write-Host "Created $nativeRepository $ReleaseBranch."
}
if (-not $parentExisting) {
    New-RemoteBranch $parentRepository $ReleaseBranch $parentBaseSha
    Write-Host "Created $parentRepository $ReleaseBranch."
}

$nativeResult = Get-RemoteBranchSha $nativeUrl $ReleaseBranch
$parentResult = Get-RemoteBranchSha $parentUrl $ReleaseBranch
if ($nativeResult -ne $nativeBaseSha -or $parentResult -ne $parentBaseSha) {
    throw 'Release branch verification failed after creation.'
}

Write-Host "Verified $ReleaseBranch in both repositories."
