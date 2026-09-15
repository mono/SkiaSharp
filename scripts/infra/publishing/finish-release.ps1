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

.PARAMETER Mode
    DryRun is read-only, Apply writes the proposed support update locally, and
    Push publishes the tag, release, support PR, and follow-up workflows. Check
    quietly validates durable public release completion without making changes.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [ValidateSet('DryRun', 'Apply', 'Push', 'Check')]
    [string] $Mode = 'DryRun'
)

# 0. Initialize shared helpers, execution mode, and repository paths.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$writeRemote = $Mode -eq 'Push'
$isCheck = $Mode -eq 'Check'
$modeDescription = $Mode.ToLowerInvariant()
$root = Get-GitRepositoryRoot -Path $PSScriptRoot
$repository = $ReleaseRepository

# Validates release identity.
function Assert-GitHubRelease([pscustomobject] $Release, [pscustomobject] $GitHubRelease) {
    if ($GitHubRelease.tagName -ne $Release.Tag -or
        [bool] $GitHubRelease.isPrerelease -ne $Release.IsPrerelease) {
        throw "GitHub Release $($Release.Tag) has conflicting metadata."
    }
}

# Creates one published GitHub Release.
function Publish-GitHubRelease(
    [pscustomobject] $Release,
    [string] $SourceCommit,
    [string] $PreviousTag,
    [pscustomobject] $Existing
) {
    if ($Existing) {
        Write-ReleaseStatus ready "GitHub Release $($Release.Tag) is published."
        return
    }
    if (!$writeRemote) {
        Write-ReleaseStatus plan "Create and publish GitHub Release $($Release.Tag)."
        return
    }

    $arguments = @(
        'release', 'create', $Release.Tag,
        '--repo', $repository,
        '--title', $Release.Title,
        '--generate-notes',
        '--target', $SourceCommit,
        '--verify-tag'
    )
    if ($PreviousTag) {
        $arguments += @('--notes-start-tag', $PreviousTag)
    }
    if ($Release.IsPrerelease) {
        $arguments += @('--prerelease', '--latest=false')
    }
    $null = Invoke-GitHub -Arguments $arguments -WriteOutput

    $published = Get-GitHubRelease -Repository $repository -Tag $Release.Tag
    if (!$published) {
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
                '-f', 'mode=Push'
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
        -Mode $Mode
}

# 1. Resolve the exact public release.
# 1.1 Resolve an abbreviated prerelease identity to one public NuGet version.
$requestedVersion = $Version
if ($isCheck) {
    try {
        $findings = [System.Collections.Generic.List[string]]::new()
        $publication = Get-NuGetPublicationReceipt -Version $Version
        if ($publication.State -ne 'complete') {
            [Console]::Error.WriteLine($publication.Message)
            if ($publication.State -eq 'unavailable') {
                exit 2
            }
            exit 1
        }
        $Version = $publication.Value.Version
        $release = $publication.Value.Release
        $packageSource = [pscustomobject] @{
            Branch = $publication.Value.Branch
            Commit = $publication.Value.Commit
        }
        if ($packageSource.Branch -ne $release.Branch) {
            $findings.Add("SkiaSharp $Version names $($packageSource.Branch), expected $($release.Branch).")
        }
        $tagSha = Get-RemoteTagSha -Root $root -Remote origin -Tag $release.Tag
        if (!$tagSha) {
            $findings.Add("Missing tag $($release.Tag).")
        } elseif ($tagSha -ne $packageSource.Commit) {
            $findings.Add("$($release.Tag) points to $tagSha, expected $($packageSource.Commit).")
        }
        $githubRelease = Get-GitHubRelease -Repository $repository -Tag $release.Tag
        if (!$githubRelease) {
            $findings.Add("Missing GitHub Release $($release.Tag).")
        } else {
            try {
                Assert-GitHubRelease -Release $release -GitHubRelease $githubRelease
                if ($githubRelease.isDraft) {
                    $findings.Add("GitHub Release $($release.Tag) is still a draft.")
                }
                if ([string] $githubRelease.targetCommitish -ne $packageSource.Commit) {
                    $findings.Add("GitHub Release $($release.Tag) targets $($githubRelease.targetCommitish), expected $($packageSource.Commit).")
                }
            } catch {
                $findings.Add($_.Exception.Message)
            }
        }
        $support = (Get-GitFileText `
            -Root $root `
            -Commit 'HEAD' `
            -Path 'scripts/infra/docs/versions.json') | ConvertFrom-Json
        $line = ($release.Numeric -split '\.')[0..1] -join '.'
        $tiers = if ($release.IsPrerelease) { @($support.support.preview) + @($support.support.stable) } else { @($support.support.stable) }
        if ($tiers -notcontains $line) {
            $findings.Add("$line is missing from the committed release support tier.")
        }
        if ($findings.Count) {
            $findings | Write-Output
            exit 1
        }
        exit 0
    } catch {
        $message = $_.Exception.Message
        if ($message -match '404|not found|must match exactly one public NuGet version; found none') {
            [Console]::Error.WriteLine("SkiaSharp $Version is not public on NuGet.org.")
            exit 1
        }
        if ($message -match 'must match exactly one public NuGet version; found') {
            [Console]::Error.WriteLine("Release finish check incomplete: $message")
            exit 1
        }
        [Console]::Error.WriteLine("Release finish check unavailable: $message")
        exit 2
    }
}
Write-Host "Finishing $requestedVersion ($modeDescription)"
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
$releaseTags = Get-RemoteReleaseTags -Root $root
$previousTag = Get-PreviousShippedTag -Tag $release.Tag -Tags $releaseTags
if ($previousTag) {
    Write-ReleaseStatus ready "GitHub-generated notes will start after $previousTag."
} else {
    Write-ReleaseStatus warning 'No previous exact shipped tag exists; generated notes will use repository history.'
}

# 2.2 Ensure the tag points to the package source commit.
Push-ReleaseTag `
    -Root $root `
    -Remote origin `
    -Tag $release.Tag `
    -SourceCommit $packageSource.Commit `
    -Push:$writeRemote

# 3. Create or resume the published GitHub Release.
Publish-GitHubRelease `
    -Release $release `
    -SourceCommit $packageSource.Commit `
    -PreviousTag $previousTag `
    -Existing $initialRelease

# 4. Propose the released line's deterministic support-tier update.
Update-ReleaseSupport `
    -Release $release

# 5. Dispatch follow-up workflows only after publication.
Invoke-ReleaseFollowUpWorkflows -Release $release
