#!/usr/bin/env pwsh
#Requires -Version 7.4

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SkiaSharpBranch,

    [Parameter(Mandatory)]
    [string] $SkiaBranch,

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

$repoRoot = git rev-parse --show-toplevel
Set-Location $repoRoot
$skiaGitUrl = (git config -f .gitmodules --get submodule.externals/skia.url).Trim()
if (-not $skiaGitUrl) {
    throw 'Unable to resolve externals/skia from .gitmodules.'
}
git check-ref-format --branch $SkiaSharpBranch | Out-Null
git check-ref-format --branch $SkiaBranch | Out-Null

$currentBranch = git branch --show-current
$currentHead = git rev-parse 'HEAD^{commit}'
$status = @(git status --porcelain=v1 --untracked-files=all)
if ($currentBranch -ne $SkiaSharpBranch) {
    throw "Current branch is $currentBranch; expected $SkiaSharpBranch."
}
if ($status) {
    throw "The SkiaSharp worktree is not clean:`n$($status -join "`n")"
}

$parentUrl = git remote get-url origin
$parentRemoteHead = Get-RemoteBranchSha $parentUrl $SkiaSharpBranch
if ($parentRemoteHead -ne $currentHead) {
    throw "Current checkout is $currentHead, but origin/$SkiaSharpBranch is $parentRemoteHead."
}

$oldGitlinkEntry = git ls-tree HEAD -- externals/skia
$oldGitlinkParts = $oldGitlinkEntry -split '\s+'
if ($oldGitlinkParts.Count -lt 3 -or $oldGitlinkParts[1] -ne 'commit') {
    throw 'The current SkiaSharp commit does not contain the externals/skia gitlink.'
}
$oldGitlink = $oldGitlinkParts[2]

$manifestPath = Join-Path $repoRoot 'cgmanifest.json'
$manifestBefore = Get-Content $manifestPath -Raw | ConvertFrom-Json
$oldManifestSha = @(
    $manifestBefore.registrations |
        Where-Object { $_.component.git.repositoryUrl -eq $skiaGitUrl } |
        ForEach-Object { $_.component.git.commitHash }
)
if ($oldManifestSha.Count -ne 1 -or $oldManifestSha[0] -ne $oldGitlink) {
    throw "cgmanifest Skia SHA does not match the current gitlink $oldGitlink."
}

git submodule update --init -- externals/skia | Out-Null
$skiaRoot = Join-Path $repoRoot 'externals/skia'
Set-Location $skiaRoot
$nativeUrl = git remote get-url origin
git fetch --no-tags origin "+refs/heads/${SkiaBranch}:refs/remotes/origin/${SkiaBranch}" | Out-Null
$mergedSha = git rev-parse "refs/remotes/origin/${SkiaBranch}^{commit}"

$parents = (git rev-list --parents -n 1 $mergedSha) -split '\s+'
if ($parents.Count -ne 3) {
    throw "Skia $SkiaBranch tip $mergedSha is not a two-parent merge commit."
}
if ($oldGitlink -notin $parents[1..2]) {
    throw "Skia $SkiaBranch tip $mergedSha does not merge the parent PR gitlink $oldGitlink."
}

$oldTree = git rev-parse "${oldGitlink}^{tree}"
$mergedTree = git rev-parse "${mergedSha}^{tree}"
if ($mergedTree -ne $oldTree) {
    throw "Merged Skia tree $mergedTree differs from the parent PR's reviewed tree $oldTree."
}

Write-Host "SkiaSharp branch:  $SkiaSharpBranch @ $currentHead"
Write-Host "Skia branch:       $SkiaBranch"
Write-Host "Reviewed commit:   $oldGitlink"
Write-Host "Merged commit:     $mergedSha"
Write-Host "Identical tree:    $mergedTree"

if (-not $Push) {
    Write-Host 'DRY RUN: no files or refs were changed. Re-run with -Push to update the parent PR.'
    exit 0
}

# Re-read both remote tips immediately before changing the worktree.
$parentRemoteNow = Get-RemoteBranchSha $parentUrl $SkiaSharpBranch
$nativeRemoteNow = Get-RemoteBranchSha $nativeUrl $SkiaBranch
if ($parentRemoteNow -ne $currentHead -or $nativeRemoteNow -ne $mergedSha) {
    throw 'A source branch changed after preflight. Run the script without -Push again.'
}

git checkout --detach $mergedSha | Out-Null
Set-Location $repoRoot

$manifest = Get-Content $manifestPath -Raw
$escapedSkiaGitUrl = [regex]::Escape($skiaGitUrl)
$pattern = '(?s)("repositoryUrl"\s*:\s*"' + $escapedSkiaGitUrl + '"\s*,\s*"commitHash"\s*:\s*")[0-9a-fA-F]{40}(")'
$regex = [regex]::new($pattern)
$matches = $regex.Matches($manifest)
if ($matches.Count -ne 1) {
    throw "Expected exactly one paired Skia git registration in cgmanifest.json; found $($matches.Count)."
}
$updatedManifest = $regex.Replace(
    $manifest,
    { param($match) $match.Groups[1].Value + $mergedSha + $match.Groups[2].Value }
)
[System.IO.File]::WriteAllText($manifestPath, $updatedManifest, [System.Text.UTF8Encoding]::new($false))

$changed = @(git diff --name-only | Where-Object { $_ })
$unexpected = @($changed | Where-Object { $_ -notin @('cgmanifest.json', 'externals/skia') })
if ($unexpected -or $changed.Count -eq 0) {
    throw "Expected only cgmanifest.json and externals/skia to change; found: $($changed -join ', ')"
}

$manifestNow = Get-Content $manifestPath -Raw | ConvertFrom-Json
$manifestSha = @(
    $manifestNow.registrations |
        Where-Object { $_.component.git.repositoryUrl -eq $skiaGitUrl } |
        ForEach-Object { $_.component.git.commitHash }
)
Set-Location $skiaRoot
$gitlinkNow = git rev-parse 'HEAD^{commit}'
Set-Location $repoRoot
if ($manifestSha.Count -ne 1 -or $manifestSha[0] -ne $mergedSha -or $gitlinkNow -ne $mergedSha) {
    throw 'The updated gitlink and cgmanifest do not both match the merged Skia SHA.'
}

git add -- externals/skia cgmanifest.json | Out-Null
$staged = @(git diff --cached --name-only | Where-Object { $_ })
if ($staged.Count -eq 0 -or @($staged | Where-Object { $_ -notin @('cgmanifest.json', 'externals/skia') })) {
    throw "Unexpected staged paths: $($staged -join ', ')"
}

git commit -m 'Update Skia reference to merged commit' -m "Skia $SkiaBranch advanced from $oldGitlink to $mergedSha." | Out-Null
$newParentHead = git rev-parse 'HEAD^{commit}'
git push origin "HEAD:refs/heads/$SkiaSharpBranch" | Out-Null

$remoteResult = Get-RemoteBranchSha $parentUrl $SkiaSharpBranch
if ($remoteResult -ne $newParentHead) {
    throw "Remote parent branch is $remoteResult; expected $newParentHead."
}

Write-Host "Updated and pushed $SkiaSharpBranch to $newParentHead."
