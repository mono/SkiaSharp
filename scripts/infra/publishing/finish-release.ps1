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
    Apply or Push, the script is read-only.

.PARAMETER Apply
    Writes the proposed release-support update locally without committing,
    pushing, publishing, or dispatching workflows.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [switch] $Apply,

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository paths.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
if ($Apply -and $Push) {
    throw 'Apply and Push are mutually exclusive.'
}
$writeRemote = $Push
$mode = if ($Push) { 'push' } elseif ($Apply) { 'local apply' } else { 'dry run' }
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

# Updates support membership from an exact released version using PowerShell's JSON model.
function Get-UpdatedReleaseSupport([string] $Text, [pscustomobject] $Release) {
    $document = $Text | ConvertFrom-Json
    if (!$document.PSObject.Properties['support']) {
        throw 'versions.json does not contain a support block.'
    }
    $support = $document.support
    $stable = if ($null -eq $support.stable) {
        @()
    } else {
        @($support.stable | ForEach-Object { [string] $_ })
    }
    $preview = if ($null -eq $support.preview) {
        @()
    } else {
        @($support.preview | ForEach-Object { [string] $_ })
    }

    $parts = @($Release.Numeric.Split('.'))
    $line = "$($parts[0]).$($parts[1])"
    $changed = $false
    if ($Release.IsPrerelease) {
        if ($preview -notcontains $line) {
            $support.preview = @($preview) + $line
            $changed = $true
        }
    } else {
        if ($stable -notcontains $line) {
            $support.stable = @($stable) + $line
            $changed = $true
        }
        if ($preview -contains $line) {
            $support.preview = @($preview | Where-Object { $_ -ne $line })
            $changed = $true
        }
    }
    if (!$changed) {
        return $Text
    }

    $newline = if ($Text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $hasFinalNewline = $Text.EndsWith("`n", [StringComparison]::Ordinal)
    $updated = $document | ConvertTo-Json -Depth 100
    $updated = $updated.Replace("`r`n", "`n").Replace("`n", $newline)
    if ($hasFinalNewline) {
        $updated += $newline
    }
    return $updated
}

# Proposes the released line's support update through the shared automation-PR path.
function Update-ReleaseSupport([pscustomobject] $Release) {
    $path = 'scripts/infra/docs/versions.json'
    $parts = @($Release.Numeric.Split('.'))
    $line = "$($parts[0]).$($parts[1])"
    $original = [IO.File]::ReadAllText((Join-Path $root $path))
    $updated = Get-UpdatedReleaseSupport `
        -Text $original `
        -Release $Release
    $action = if ($Release.IsPrerelease) {
        "Add $line to the preview support tier after publishing its preview/RC release."
    } else {
        "Promote $line to the stable support tier after publishing its stable release."
    }
    $body = @"
## Description

$action Existing supported lines are retained because ending support remains an explicit maintainer decision.

**Related issues**

N/A.

**Required skia PR**

None.

**Areas affected**

- [x] Build, packaging, or CI
- [x] Documentation or samples

## Changes

None - release support metadata only.

## Testing

The publishing tests cover preview, RC, stable promotion, idempotency, multiple supported lines, and preservation of unrelated configuration.

## Checklist

- [x] Tests added or updated
- [x] ``Changes`` above lists all public API and behavioral changes (None)
- [x] New/changed public API? N/A
- [x] Native change? N/A
"@
    Publish-AutomationFilePullRequest `
        -Root $root `
        -Repository $repository `
        -Branch "automation/update-release-support-$line" `
        -BaseBranch main `
        -Files ([ordered] @{ $path = $updated }) `
        -CommitMessage "Update $line release support tier" `
        -Title "Update $line release support tier" `
        -Body $body `
        -Description 'release-support' `
        -Apply:$Apply `
        -Push:$Push
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
Update-ReleaseSupport `
    -Release $release

# 5. Dispatch follow-up workflows only after publication.
Invoke-ReleaseFollowUpWorkflows -Release $release
