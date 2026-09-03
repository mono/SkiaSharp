#!/usr/bin/env pwsh
#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SkiaSharpBaseBranch,

    [Parameter(Mandatory)]
    [string] $SkiaSharpHeadBranch,

    [Parameter(Mandatory)]
    [string] $SkiaBaseBranch,

    [switch] $Push
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

function Get-RemoteBranchSha {
    param([string] $RepositoryUrl, [string] $Branch)

    $result = git ls-remote --heads $RepositoryUrl "refs/heads/$Branch"
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

function Get-SkiaMilestone {
    param([string] $Commit)

    $manifestText = git show "${Commit}:cgmanifest.json" | Out-String
    $manifest = $manifestText | ConvertFrom-Json
    $registrations = @(
        $manifest.registrations | Where-Object {
            $_.component.type -eq 'other' -and
            $_.component.other.name -eq 'skia' -and
            $null -ne $_.chrome_milestone
        }
    )
    if ($registrations.Count -ne 1) {
        throw "Cannot derive the Skia milestone from cgmanifest.json at $Commit."
    }

    $milestone = [int] $registrations[0].chrome_milestone
    if ($milestone -le 0) {
        throw "Invalid Skia milestone '$($registrations[0].chrome_milestone)' in cgmanifest.json at $Commit."
    }
    return $milestone
}

function Get-SkiaSharpProductLine {
    param([string] $Commit)

    $versions = (git show "${Commit}:scripts/VERSIONS.txt" | Out-String).Trim()
    $versionMatch = [regex]::Match($versions, '(?m)^SkiaSharp\s+nuget\s+(?<major>\d+)\.(?<minor>\d+)\.\d+(?:[-+]\S+)?\s*$')
    if (-not $versionMatch.Success) {
        throw "Cannot derive the SkiaSharp product line from scripts/VERSIONS.txt at $Commit."
    }
    return "$($versionMatch.Groups['major'].Value).$($versionMatch.Groups['minor'].Value)"
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

    gh api --method POST "repos/$Repository/git/refs" -f "ref=refs/heads/$Branch" -f "sha=$Sha" | Out-Null
}

$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot
git check-ref-format --branch $SkiaSharpBaseBranch | Out-Null
git check-ref-format --branch $SkiaSharpHeadBranch | Out-Null
git check-ref-format --branch $SkiaBaseBranch | Out-Null

$parentUrl = git remote get-url origin

git fetch --no-tags origin "+refs/heads/${SkiaSharpBaseBranch}:refs/remotes/origin/${SkiaSharpBaseBranch}" | Out-Null
$parentBaseSha = git rev-parse "refs/remotes/origin/${SkiaSharpBaseBranch}^{commit}"

if ($SkiaSharpBaseBranch -match '^release/(?<version>\d+\.\d+)\.x$') {
    Write-Host "Servicing branch: $SkiaSharpBaseBranch"
    Write-Host 'No release branches are needed for a servicing sync.'
    exit 0
}
if ($SkiaSharpBaseBranch -ne 'main') {
    throw "SkiaSharp base branch must be main or release/A.B.x; got $SkiaSharpBaseBranch."
}

git fetch --no-tags origin "+refs/heads/${SkiaSharpHeadBranch}:refs/remotes/origin/${SkiaSharpHeadBranch}" | Out-Null
$parentHeadSha = git rev-parse "refs/remotes/origin/${SkiaSharpHeadBranch}^{commit}"

$baseMilestone = Get-SkiaMilestone $parentBaseSha
$headMilestone = Get-SkiaMilestone $parentHeadSha
if ($headMilestone -lt $baseMilestone) {
    throw "Skia milestone regresses from m$baseMilestone on $SkiaSharpBaseBranch to m$headMilestone on $SkiaSharpHeadBranch."
}
if ($headMilestone -eq $baseMilestone) {
    Write-Host "Same-milestone sync: m$baseMilestone -> m$headMilestone."
    Write-Host 'No release branches are needed.'
    exit 0
}

$baseProductLine = Get-SkiaSharpProductLine $parentBaseSha
$headProductLine = Get-SkiaSharpProductLine $parentHeadSha
if ($headProductLine -eq $baseProductLine) {
    throw "Skia milestone changes from m$baseMilestone to m$headMilestone, but the SkiaSharp product line remains $baseProductLine."
}
$ReleaseBranch = "release/$baseProductLine.x"
git check-ref-format --branch $ReleaseBranch | Out-Null

$nativeUrl = git config --blob "${parentBaseSha}:.gitmodules" --get submodule.externals/skia.url
$parentRepository = Get-GitHubRepository $parentUrl
$nativeRepository = Get-GitHubRepository $nativeUrl

$treeEntry = git ls-tree $parentBaseSha -- externals/skia
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

Write-Host "Milestone bump:     m$baseMilestone -> m$headMilestone ($baseProductLine -> $headProductLine)"
Write-Host "Release branch:     $ReleaseBranch"
Write-Host "$nativeRepository source: $SkiaBaseBranch @ $nativeBaseSha$(if ($nativeExisting) { ' (release branch already exists)' })"
Write-Host "$parentRepository source: $SkiaSharpBaseBranch @ $parentBaseSha$(if ($parentExisting) { ' (release branch already exists)' })"

if (-not $Push) {
    Write-Host 'DRY RUN: no remote refs were created. Re-run with -Push to create them.'
    exit 0
}

# Re-read every source and destination immediately before the first write.
$parentBaseNow = Get-RemoteBranchSha $parentUrl $SkiaSharpBaseBranch
$parentHeadNow = Get-RemoteBranchSha $parentUrl $SkiaSharpHeadBranch
$nativeBaseNow = Get-RemoteBranchSha $nativeUrl $SkiaBaseBranch
if ($parentBaseNow -ne $parentBaseSha -or $parentHeadNow -ne $parentHeadSha -or $nativeBaseNow -ne $nativeBaseSha) {
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
