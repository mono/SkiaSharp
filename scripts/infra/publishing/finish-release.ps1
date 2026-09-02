#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Finishes a SkiaSharp release after its packages appear on NuGet.org.

.DESCRIPTION
    Reads the source commit from the exact public SkiaSharp package, creates the
    immutable exact-version tag, publishes a GitHub-generated Release, opens or
    updates the release-support PR, and dispatches follow-up workflows.

.PARAMETER Version
    A stable version or prerelease identity. A prerelease build revision may be
    omitted when exactly one matching SkiaSharp version exists on NuGet.org.

.PARAMETER Push
    Publishes the tag and release, then dispatches follow-up workflows. Without
    this switch, the script is read-only.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository paths.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }
$root = Get-GitRepositoryRoot
$repository = $ReleaseRepository

# Validates release identity and the title of an existing draft.
function Assert-GitHubRelease([pscustomobject] $Release, [pscustomobject] $GitHubRelease) {
    if ($GitHubRelease.tagName -ne $Release.Tag -or
        [bool] $GitHubRelease.isPrerelease -ne $Release.IsPrerelease) {
        throw "GitHub Release $($Release.Tag) has conflicting metadata."
    }
    if ($GitHubRelease.isDraft) {
        if ($GitHubRelease.name -ne $Release.Title) {
            throw "GitHub Release draft $($Release.Tag) has conflicting metadata."
        }
    }
}

# Creates or resumes one published GitHub Release.
function Publish-GitHubRelease(
    [pscustomobject] $Release,
    [string] $SourceCommit,
    [pscustomobject] $Existing
) {
    if ($Existing -and !$Existing.isDraft) {
        Write-ReleaseStatus ready "GitHub Release $($Release.Tag) is published."
        return
    }
    if (!$writeRemote) {
        $action = if ($Existing) { 'Publish existing draft' } else { 'Create and publish' }
        Write-ReleaseStatus plan "$action GitHub Release $($Release.Tag)."
        return
    }

    if ($Existing) {
        Assert-GitHubRelease $Release $Existing
        $null = Invoke-GitHub `
            -Arguments @('release', 'edit', $Release.Tag, '--repo', $repository, '--verify-tag', '--draft=false') `
            -WriteOutput
    } else {
        $arguments = @(
            'release', 'create', $Release.Tag,
            '--repo', $repository,
            '--title', $Release.Title,
            '--generate-notes',
            '--target', $SourceCommit,
            '--verify-tag'
        )
        if ($Release.IsPrerelease) {
            $arguments += @('--prerelease', '--latest=false')
        }
        $null = Invoke-GitHub -Arguments $arguments -WriteOutput
    }

    $published = Get-GitHubRelease -Repository $repository -Tag $Release.Tag
    if (!$published -or $published.isDraft) {
        throw "GitHub Release $($Release.Tag) was not published."
    }
    Assert-GitHubRelease $Release $published
    if ($published.name -ne $Release.Title) {
        throw "Published GitHub Release $($Release.Tag) has conflicting metadata."
    }
    Write-ReleaseStatus applied "Published GitHub Release $($Release.Tag)."
}

# Dispatches convergent release-note and issue-template follow-up workflows.
function Invoke-ReleaseFollowUpWorkflows([pscustomobject] $Release) {
    if (!$writeRemote) {
        Write-ReleaseStatus plan "Dispatch release-note generation for $($Release.Tag)."
        if (!$Release.IsPrerelease) {
            Write-ReleaseStatus plan 'Dispatch the issue-template version update.'
        }
        return
    }
    $null = Invoke-GitHub `
        -Arguments @(
            'workflow', 'run', 'update-release-notes.lock.yml',
            '--repo', $repository,
            '--ref', 'main',
            '-f', 'source_branch=main',
            '-f', "min_version=$($Release.Numeric)",
            '-f', "max_version=$($Release.Numeric)"
        ) `
        -WriteOutput
    if (!$Release.IsPrerelease) {
        $null = Invoke-GitHub `
            -Arguments @(
                'workflow', 'run', 'auto-update-issue-template-versions.yml',
                '--repo', $repository,
                '--ref', 'main',
                '-f', 'push=true'
            ) `
            -WriteOutput
    }
    Write-ReleaseStatus applied 'Release-note follow-up workflows were dispatched.'
}

# 1. Resolve the exact public release.
# 1.1 Resolve an abbreviated prerelease identity to one public NuGet version.
$requestedVersion = $Version
Write-Host "Finishing $requestedVersion ($mode)"
$Version = Resolve-NuGetPackageVersion -PackageId 'SkiaSharp' -Version $Version
if ($Version -ne $requestedVersion) {
    Write-ReleaseStatus ready "Resolved $requestedVersion to public package version $Version."
}

# 1.2 Parse the public version into its branch and tag identity.
$release = Get-ReleaseIdentity -PublicVersion $Version
# 1.3 Read the source commit directly from the public SkiaSharp nuspec.
$packageSource = Get-NuGetPackageSource -PackageId 'SkiaSharp' -PackageVersion $Version
Write-ReleaseStatus ready "SkiaSharp $Version was built from $($packageSource.Commit) on $($packageSource.Branch)."
if ($packageSource.Branch -ne $release.Branch) {
    Write-ReleaseStatus warning "The package names $($packageSource.Branch), while the version implies $($release.Branch)."
}

# 2. Inspect and converge immutable GitHub state.
# 2.1 Freeze the current release state before applying any action.
$initialRelease = Get-GitHubRelease -Repository $repository -Tag $release.Tag
if ($initialRelease) {
    Assert-GitHubRelease -Release $release -GitHubRelease $initialRelease
}
if ($writeRemote) {
    Enable-GitHubGitAuthentication
}

# 2.2 Ensure the tag points to the package source commit.
Push-ReleaseTag `
    -Root $root `
    -Remote origin `
    -Tag $release.Tag `
    -SourceCommit $packageSource.Commit `
    -Push:$Push

# 3. Create or resume the published GitHub Release.
Publish-GitHubRelease `
    -Release $release `
    -SourceCommit $packageSource.Commit `
    -Existing $initialRelease

# 4. Propose the released line's deterministic support-tier update.
& (Join-Path $PSScriptRoot 'update-release-support.ps1') `
    -Version $Version `
    -Push:$Push

# 5. Dispatch follow-up workflows only after publication.
Invoke-ReleaseFollowUpWorkflows -Release $release
