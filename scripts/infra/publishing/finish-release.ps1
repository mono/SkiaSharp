#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Finishes a SkiaSharp release after its packages appear on NuGet.org.

.DESCRIPTION
    Reads the source commit from the exact public SkiaSharp package, creates the
    immutable exact-version tag, publishes a marked GitHub Release, and dispatches
    follow-up workflows in one pushed run.

.PARAMETER Version
    A stable version or prerelease identity. A prerelease build revision may be
    omitted when exactly one matching SkiaSharp version exists on NuGet.org.

.PARAMETER Push
    Publishes the tag and release, then dispatches follow-up workflows. Without
    this switch, the script is read-only.
#>

param(
    [Parameter(Mandatory)]
    [string] $Version,

    [switch] $Push
)

Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$repository = $ReleaseRepository
$summaryStart = $ReleaseSummaryStartMarker
$summaryEnd = $ReleaseSummaryEndMarker
$generatedStart = $ReleaseGeneratedStartMarker
$generatedEnd = $ReleaseGeneratedEndMarker

# Requires the public-version and managed-region markers to be complete and ordered.
function Assert-ManagedBody([string] $Body) {
    $markers = @($summaryStart, $summaryEnd, $generatedStart, $generatedEnd)
    $positions = @()
    foreach ($marker in $markers) {
        if ([regex]::Matches($Body, [regex]::Escape($marker)).Count -ne 1) {
            throw "GitHub Release body must contain exactly one $marker."
        }
        $positions += $Body.IndexOf($marker, [StringComparison]::Ordinal)
    }
    for ($index = 1; $index -lt $positions.Count; $index++) {
        if ($positions[$index - 1] -ge $positions[$index]) {
            throw 'GitHub Release body markers are out of order.'
        }
    }
}

# Validates release identity and, for drafts, the managed publication contract.
function Assert-GitHubRelease([pscustomobject] $Release, [pscustomobject] $GitHubRelease) {
    if ($GitHubRelease.tagName -ne $Release.Tag -or
        [bool] $GitHubRelease.isPrerelease -ne $Release.IsPrerelease) {
        throw "GitHub Release $($Release.Tag) has conflicting metadata."
    }
    if ($GitHubRelease.isDraft) {
        if ($GitHubRelease.name -ne $Release.Title) {
            throw "GitHub Release draft $($Release.Tag) has conflicting metadata."
        }
        Assert-ManagedBody $GitHubRelease.body
    }
}

# Creates or resumes one marked, published GitHub Release.
function Publish-GitHubRelease(
    [pscustomobject] $Release,
    [string] $SourceCommit,
    [pscustomobject] $Existing,
    [switch] $DryRun
) {
    if ($Existing -and !$Existing.isDraft) {
        Write-ReleaseStatus ready "GitHub Release $($Release.Tag) is published."
        return
    }
    if ($DryRun) {
        $action = if ($Existing) { 'Publish existing draft' } else { 'Create and publish' }
        Write-ReleaseStatus plan "$action GitHub Release $($Release.Tag)."
        return
    }

    if ($Existing) {
        Assert-GitHubRelease $Release $Existing
        gh release edit $Release.Tag --repo $repository --verify-tag --draft=false | Out-Host
    } else {
        # Generate GitHub's notes and wrap them in regions owned by Finish and the summary updater.
        $generated = Invoke-GitHubJson -Arguments @(
            'api',
            '--method',
            'POST',
            "repos/$repository/releases/generate-notes",
            '--field',
            "tag_name=$($Release.Tag)",
            '--field',
            "target_commitish=$SourceCommit"
        )
        $bodyPath = [System.IO.Path]::GetTempFileName()
        $body = (
            "$summaryStart`n`n$summaryEnd`n`n" +
            "$generatedStart`n$($generated.body.Trim())`n$generatedEnd`n")
        try {
            Set-Content $bodyPath $body -NoNewline
            $arguments = @(
                'release', 'create', $Release.Tag,
                '--repo', $repository,
                '--title', $Release.Title,
                '--notes-file', $bodyPath,
                '--target', $SourceCommit,
                '--verify-tag'
            )
            if ($Release.IsPrerelease) {
                $arguments += @('--prerelease', '--latest=false')
            }
            gh @arguments | Out-Host
        } finally {
            Remove-Item $bodyPath -Force -ErrorAction SilentlyContinue
        }
    }

    $published = Get-GitHubRelease -Repository $repository -Tag $Release.Tag
    if (!$published -or $published.isDraft) {
        throw "GitHub Release $($Release.Tag) was not published."
    }
    Assert-GitHubRelease $Release $published
    if ($published.name -ne $Release.Title) {
        throw "Published GitHub Release $($Release.Tag) has conflicting metadata."
    }
    Assert-ManagedBody $published.body
    Write-ReleaseStatus applied "Published GitHub Release $($Release.Tag)."
}

# Dispatches convergent release-note and issue-template follow-up workflows.
function Invoke-ReleaseFollowUpWorkflows([pscustomobject] $Release, [switch] $DryRun) {
    if ($DryRun) {
        Write-ReleaseStatus plan "Dispatch release-note generation for $($Release.Tag)."
        Write-ReleaseStatus ready (
            'The GitHub Release summary converges automatically after publication or reviewed notes merge.')
        if (!$Release.IsPrerelease) {
            Write-ReleaseStatus plan 'Dispatch the issue-template version update.'
        }
        return
    }
    gh workflow run update-release-notes.lock.yml `
        --repo $repository `
        --ref main `
        -f source_branch=main `
        -f min_version=$($Release.Numeric) `
        -f max_version=$($Release.Numeric)
    if (!$Release.IsPrerelease) {
        gh workflow run auto-update-issue-template-versions.yml `
            --repo $repository `
            --ref main
    }
    Write-ReleaseStatus applied 'Release-note follow-up workflows were dispatched.'
}

# 1. Resolve the exact public release.
# 1.1 Resolve an abbreviated prerelease identity to one public NuGet version.
$requestedVersion = $Version
Write-Host "Finishing $requestedVersion ($(if ($Push) { 'push' } else { 'dry run' }))"
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
if ($Push) {
    Enable-GitHubGitAuthentication
}

# 2.2 Ensure the tag points to the package source commit.
Push-ReleaseTag -Remote origin -Tag $release.Tag -SourceCommit $packageSource.Commit -Push:$Push

# 3. Create or resume the published GitHub Release.
Publish-GitHubRelease `
    -Release $release `
    -SourceCommit $packageSource.Commit `
    -Existing $initialRelease `
    -DryRun:(-not $Push)

# 4. Dispatch follow-up workflows only after publication.
Invoke-ReleaseFollowUpWorkflows -Release $release -DryRun:(-not $Push)
