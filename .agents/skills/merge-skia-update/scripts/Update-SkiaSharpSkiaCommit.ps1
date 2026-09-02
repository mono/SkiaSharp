#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $SkiaSharpBranch,

    [Parameter(Mandatory)]
    [string] $SkiaBranch,

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

$repoRoot = Invoke-Native git @('rev-parse', '--show-toplevel')
Set-Location $repoRoot
$null = Invoke-Native git @('check-ref-format', '--branch', $SkiaSharpBranch)
$null = Invoke-Native git @('check-ref-format', '--branch', $SkiaBranch)

$currentBranch = Invoke-Native git @('branch', '--show-current')
$currentHead = Invoke-Native git @('rev-parse', 'HEAD^{commit}')
$status = Invoke-Native git @('status', '--porcelain=v1', '--untracked-files=all')
if ($currentBranch -ne $SkiaSharpBranch) {
    throw "Current branch is $currentBranch; expected $SkiaSharpBranch."
}
if ($status) {
    throw "The SkiaSharp worktree is not clean:`n$status"
}

$parentUrl = Invoke-Native git @('remote', 'get-url', 'origin')
$parentRemoteHead = Get-RemoteBranchSha $parentUrl $SkiaSharpBranch
if ($parentRemoteHead -ne $currentHead) {
    throw "Current checkout is $currentHead, but origin/$SkiaSharpBranch is $parentRemoteHead."
}

$oldGitlinkEntry = Invoke-Native git @('ls-tree', 'HEAD', '--', 'externals/skia')
$oldGitlinkParts = $oldGitlinkEntry -split '\s+'
if ($oldGitlinkParts.Count -lt 3 -or $oldGitlinkParts[1] -ne 'commit') {
    throw 'The current SkiaSharp commit does not contain the externals/skia gitlink.'
}
$oldGitlink = $oldGitlinkParts[2]

$manifestPath = Join-Path $repoRoot 'cgmanifest.json'
$manifestBefore = Get-Content $manifestPath -Raw | ConvertFrom-Json
$oldManifestSha = @(
    $manifestBefore.registrations |
        Where-Object { $_.component.git.repositoryUrl -eq 'https://github.com/mono/skia.git' } |
        ForEach-Object { $_.component.git.commitHash }
)
if ($oldManifestSha.Count -ne 1 -or $oldManifestSha[0] -ne $oldGitlink) {
    throw "cgmanifest mono/skia SHA does not match the current gitlink $oldGitlink."
}

$null = Invoke-Native git @('submodule', 'update', '--init', '--', 'externals/skia')
$skiaRoot = Join-Path $repoRoot 'externals/skia'
Set-Location $skiaRoot
$nativeUrl = Invoke-Native git @('remote', 'get-url', 'origin')
$null = Invoke-Native git @(
    'fetch', '--no-tags', 'origin',
    "+refs/heads/${SkiaBranch}:refs/remotes/origin/${SkiaBranch}"
)
$mergedSha = Invoke-Native git @(
    'rev-parse', "refs/remotes/origin/${SkiaBranch}^{commit}"
)

$parents = (Invoke-Native git @('rev-list', '--parents', '-n', '1', $mergedSha)) -split '\s+'
if ($parents.Count -ne 3) {
    throw "mono/skia $SkiaBranch tip $mergedSha is not a two-parent merge commit."
}
if ($oldGitlink -notin $parents[1..2]) {
    throw "mono/skia $SkiaBranch tip $mergedSha does not merge the parent PR gitlink $oldGitlink."
}

$oldTree = Invoke-Native git @('rev-parse', "${oldGitlink}^{tree}")
$mergedTree = Invoke-Native git @('rev-parse', "${mergedSha}^{tree}")
if ($mergedTree -ne $oldTree) {
    throw "Merged mono/skia tree $mergedTree differs from the parent PR's reviewed tree $oldTree."
}

Write-Host "SkiaSharp branch:  $SkiaSharpBranch @ $currentHead"
Write-Host "mono/skia branch:  $SkiaBranch"
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

$null = Invoke-Native git @('checkout', '--detach', $mergedSha)
Set-Location $repoRoot

$manifest = Get-Content $manifestPath -Raw
$pattern = '(?s)("repositoryUrl"\s*:\s*"https://github\.com/mono/skia\.git"\s*,\s*"commitHash"\s*:\s*")[0-9a-fA-F]{40}(")'
$regex = [regex]::new($pattern)
$matches = $regex.Matches($manifest)
if ($matches.Count -ne 1) {
    throw "Expected exactly one mono/skia git registration in cgmanifest.json; found $($matches.Count)."
}
$updatedManifest = $regex.Replace(
    $manifest,
    { param($match) $match.Groups[1].Value + $mergedSha + $match.Groups[2].Value }
)
[System.IO.File]::WriteAllText($manifestPath, $updatedManifest, [System.Text.UTF8Encoding]::new($false))

$changedOutput = Invoke-Native git @('diff', '--name-only')
$changed = @($changedOutput -split "`n" | Where-Object { $_ })
$unexpected = @($changed | Where-Object { $_ -notin @('cgmanifest.json', 'externals/skia') })
if ($unexpected -or $changed.Count -eq 0) {
    throw "Expected only cgmanifest.json and externals/skia to change; found: $($changed -join ', ')"
}

$manifestNow = Get-Content $manifestPath -Raw | ConvertFrom-Json
$manifestSha = @(
    $manifestNow.registrations |
        Where-Object { $_.component.git.repositoryUrl -eq 'https://github.com/mono/skia.git' } |
        ForEach-Object { $_.component.git.commitHash }
)
Set-Location $skiaRoot
$gitlinkNow = Invoke-Native git @('rev-parse', 'HEAD^{commit}')
Set-Location $repoRoot
if ($manifestSha.Count -ne 1 -or $manifestSha[0] -ne $mergedSha -or $gitlinkNow -ne $mergedSha) {
    throw 'The updated gitlink and cgmanifest do not both match the merged mono/skia SHA.'
}

$null = Invoke-Native git @('add', '--', 'externals/skia', 'cgmanifest.json')
$stagedOutput = Invoke-Native git @('diff', '--cached', '--name-only')
$staged = @($stagedOutput -split "`n" | Where-Object { $_ })
if ($staged.Count -eq 0 -or @($staged | Where-Object { $_ -notin @('cgmanifest.json', 'externals/skia') })) {
    throw "Unexpected staged paths: $($staged -join ', ')"
}

$null = Invoke-Native git @(
    'commit',
    '-m', 'Update Skia reference to merged commit',
    '-m', "mono/skia $SkiaBranch advanced from $oldGitlink to $mergedSha."
)
$newParentHead = Invoke-Native git @('rev-parse', 'HEAD^{commit}')
$null = Invoke-Native git @('push', 'origin', "HEAD:refs/heads/$SkiaSharpBranch")

$remoteResult = Get-RemoteBranchSha $parentUrl $SkiaSharpBranch
if ($remoteResult -ne $newParentHead) {
    throw "Remote parent branch is $remoteResult; expected $newParentHead."
}

Write-Host "Updated and pushed $SkiaSharpBranch to $newParentHead."
