#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Summarizes one SkiaSharp release line without changing release state.

.DESCRIPTION
    Reads the maintenance branch, release branches, public NuGet provenance,
    tags, GitHub Releases, exact-tip internal package builds, and newer commits
    on the matching upstream Skia milestone branch for one A.B line. Detailed
    release validation remains the responsibility of prepare-release.ps1 and
    finish-release.ps1.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidatePattern('^\d+\.\d+$')]
    [string] $Version,

    [switch] $Json
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'AzureDevOps.Common.psm1') -Force

function Get-SkiaSharpReleaseInfoAtCommit([string] $Root, [string] $Commit) {
    $versions = Get-GitFileText -Root $Root -Commit $Commit -Path 'scripts/VERSIONS.txt'
    $versionMatch = [regex]::Match(
        $versions,
        '(?m)^\s*SkiaSharp\s+nuget\s+(?<version>\d+\.\d+\.\d+(?:\.\d+)?)\s*$')
    $milestoneMatch = [regex]::Match(
        $versions,
        '(?m)^\s*libSkiaSharp\s+milestone\s+(?<milestone>\d+)\s*$')
    if (!$versionMatch.Success -or !$milestoneMatch.Success) {
        throw "Could not read the SkiaSharp NuGet version and Skia milestone at $Commit."
    }
    return [pscustomobject] @{
        Version = $versionMatch.Groups['version'].Value
        SkiaMilestone = [int] $milestoneMatch.Groups['milestone'].Value
    }
}

function Get-ReleaseBranches([string] $Root, [string] $Line) {
    $branches = Get-RemoteBranchMap `
        -Root $Root `
        -Remote origin `
        -Pattern "refs/heads/release/$Line.*"
    $result = foreach ($branch in $branches.Keys) {
        $identity = ConvertTo-ReleaseMilestone ($branch -replace '^release/')
        if ($identity -and $branch -match "^release/$([regex]::Escape($Line))\.") {
            [pscustomobject] @{
                Name = $branch
                Sha = $branches[$branch]
                Identity = $identity
            }
        }
    }
    return @($result)
}

function Get-MaintenanceUniqueCommits(
    [string] $Root,
    [string] $MaintenanceSha,
    [string] $ReleaseSha
) {
    if (!$MaintenanceSha -or !$ReleaseSha) {
        return @()
    }
    $output = (Invoke-Git -Root $Root -Arguments @(
        'log', '--cherry-pick', '--right-only', '--no-merges',
        '--format=%H%x1f%s', "$ReleaseSha...$MaintenanceSha"
    )).Output
    $commits = foreach ($line in @($output -split "`r?`n" | Where-Object { $_ })) {
        $parts = $line -split [char] 0x1f, 2
        if ($parts.Count -eq 2) {
            [pscustomobject] @{ Sha = $parts[0]; Subject = $parts[1] }
        }
    }
    return @($commits)
}

function Test-AutomaticVersionBump([string] $Root, [pscustomobject] $Commit) {
    if (!$Commit -or !$Commit.Subject.StartsWith('Bump to the next version (', [StringComparison]::Ordinal)) {
        return $false
    }
    $paths = @(
        (Invoke-Git -Root $Root -Arguments @(
            'diff-tree', '--no-commit-id', '--name-only', '-r', $Commit.Sha
        )).Output -split "`r?`n" | Where-Object { $_ }
    )
    return @($paths | Sort-Object) -join "`n" -eq @(
        'scripts/VERSIONS.txt'
        'scripts/azure-templates-variables.yml'
    ) -join "`n"
}

function Get-MaintenanceDelta(
    [string] $Root,
    [string] $MaintenanceSha,
    [string] $ReleaseSha
) {
    $commits = @(Get-MaintenanceUniqueCommits -Root $Root -MaintenanceSha $MaintenanceSha -ReleaseSha $ReleaseSha)
    $ignoredBump = @($commits | Where-Object {
        Test-AutomaticVersionBump -Root $Root -Commit $_
    } | Select-Object -First 1)
    $queued = @($commits | Where-Object { $_ -notin $ignoredBump })
    return [pscustomobject] @{
        Commits = $commits
        Queued = $queued
        VersionBumpOnly = $commits.Count -eq 1 -and $ignoredBump.Count -eq 1
        Text = if (!$commits.Count) {
            'none'
        } elseif ($commits.Count -eq 1 -and $ignoredBump.Count -eq 1) {
            'version bump only'
        } else {
            "$($queued.Count) queued release commit$(if ($queued.Count -eq 1) { '' } else { 's' })"
        }
    }
}

function Get-NextReleaseIdentity(
    [string] $Line,
    [pscustomobject] $Latest,
    [string] $MaintenanceVersion
) {
    if (!$Latest) {
        return "$Line.0-preview.1"
    }
    $identity = $Latest.Identity
    if ($identity.Channel -eq 'preview') {
        if ($identity.Title -match '-preview\.1$') {
            return "$($identity.Numeric)-preview.2"
        }
        return "$($identity.Numeric)-rc.1"
    }
    if ($identity.Channel -eq 'rc') {
        return "$($identity.Numeric)-stable"
    }
    if ($MaintenanceVersion -match "^$([regex]::Escape($Line))\.\d+(?:\.\d+)?$") {
        return "$MaintenanceVersion-stable"
    }
    return $null
}

function Get-ExpectedSyncBranch(
    [pscustomobject] $Maintenance,
    [int] $TargetMilestone = 0
) {
    if (!$Maintenance) {
        return $null
    }
    if ($Maintenance.Branch -eq 'main') {
        $milestone = if ($TargetMilestone -gt 0) {
            $TargetMilestone
        } else {
            [int] $Maintenance.SkiaMilestone
        }
        if ($milestone -lt 1) {
            throw 'The current main line does not have a valid Skia milestone.'
        }
        return "skia-sync/m$milestone"
    }
    if ($Maintenance.Branch -match '^release/\d+\.\d+\.x$') {
        return "skia-sync/$($Maintenance.Branch.Replace('/', '-'))"
    }
    return $null
}

function Get-SkiaSyncTopology(
    [pscustomobject] $Maintenance,
    [int] $TargetMilestone = 0
) {
    $milestone = if ($TargetMilestone -gt 0) {
        $TargetMilestone
    } else {
        [int] $Maintenance.SkiaMilestone
    }
    $syncBranch = Get-ExpectedSyncBranch `
        -Maintenance $Maintenance `
        -TargetMilestone $milestone
    if (!$syncBranch) {
        return $null
    }
    return [pscustomobject] @{
        Milestone = $milestone
        UpstreamRef = "chrome/m$milestone"
        ParentBaseBranch = $Maintenance.Branch
        SkiaBaseBranch = if ($Maintenance.Branch -eq 'main') {
            'skiasharp'
        } else {
            $Maintenance.Branch
        }
        SyncBranch = $syncBranch
    }
}

# Finds a main-line sync pair that remained open after its milestone moved to a servicing branch.
function Get-LegacyTransitionSyncSelection([pscustomobject] $Maintenance) {
    if (
        !$Maintenance -or
        $Maintenance.Branch -notmatch '^release/\d+\.\d+\.x$' -or
        [int] $Maintenance.SkiaMilestone -lt 1
    ) {
        return $null
    }

    $milestone = [int] $Maintenance.SkiaMilestone
    $headBranch = "skia-sync/m$milestone"
    $parentPullRequests = @(
        Get-GitHubOpenPullRequests `
            -Repository $ReleaseRepository `
            -Head $headBranch `
            -Base $Maintenance.Branch
        Get-GitHubOpenPullRequests `
            -Repository $ReleaseRepository `
            -Head $headBranch `
            -Base main
    )
    $nativePullRequests = @(
        Get-GitHubOpenPullRequests `
            -Repository 'mono/skia' `
            -Head $headBranch `
            -Base $Maintenance.Branch
        Get-GitHubOpenPullRequests `
            -Repository 'mono/skia' `
            -Head $headBranch `
            -Base skiasharp
    )
    if (!$parentPullRequests.Count -and !$nativePullRequests.Count) {
        return $null
    }

    return [pscustomobject] @{
        Topology = [pscustomobject] @{
            Milestone = $milestone
            UpstreamRef = "chrome/m$milestone"
            ParentBaseBranch = $Maintenance.Branch
            SkiaBaseBranch = $Maintenance.Branch
            SyncBranch = $headBranch
        }
        ParentPullRequests = $parentPullRequests
        NativePullRequests = $nativePullRequests
    }
}

function Get-LegacyTransitionIncomingPullRequest(
    [pscustomobject] $Maintenance,
    [pscustomobject] $Selection,
    [string] $SyncBranchSha,
    [string] $SkiaSyncBranchSha,
    [string] $ParentSkiaSha,
    [string] $ParentSyncSkiaSha
) {
    if (!$Selection) {
        return $null
    }
    $incoming = Get-IncomingReleasePullRequest `
        -Maintenance $Maintenance `
        -SyncBranchSha $SyncBranchSha `
        -PullRequests $Selection.ParentPullRequests `
        -Topology $Selection.Topology `
        -SkiaSyncBranchSha $SkiaSyncBranchSha `
        -ParentSkiaSha $ParentSkiaSha `
        -ParentSyncSkiaSha $ParentSyncSkiaSha
    if (!$incoming) {
        return $null
    }

    $wrongParentBases = @(
        $Selection.ParentPullRequests |
            Where-Object { [string] $_.baseRefName -ne $Maintenance.Branch } |
            ForEach-Object { "SkiaSharp PR #$($_.number) targets $($_.baseRefName)" }
    )
    $wrongNativeBases = @(
        $Selection.NativePullRequests |
            Where-Object { [string] $_.baseRefName -ne $Maintenance.Branch } |
            ForEach-Object { "mono/skia PR #$($_.number) targets $($_.baseRefName)" }
    )
    $wrongBases = @($wrongParentBases) + @($wrongNativeBases)
    if ($wrongBases.Count) {
        $existingDetail = [string] $incoming.Message
        $incoming.State = 'inconsistent'
        $incoming.BlocksRelease = $true
        $incoming.Message = (
            "Pre-transition m$($Maintenance.SkiaMilestone) sync remains open after the line moved " +
            "to $($Maintenance.Branch): $($wrongBases -join '; '). Retarget the reciprocal PR pair " +
            "to $($Maintenance.Branch), or close it if the changes are already integrated." +
            $(if ($existingDetail) { " $existingDetail" } else { '' }))
    }
    return $incoming
}

function Select-IncomingReleasePullRequest(
    [pscustomobject] $Expected,
    [pscustomobject] $Legacy
) {
    if ($Legacy -and $Legacy.State -eq 'inconsistent' -and $Legacy.BlocksRelease) {
        return $Legacy
    }
    if ($Expected) {
        return $Expected
    }
    return $Legacy
}

function Get-PendingMainMilestone(
    [string] $Line,
    [string] $MainSha,
    [pscustomobject] $MainReleaseInfo
) {
    $lineMatch = [regex]::Match($Line, '^(?<major>\d+)\.(?<minor>\d+)$')
    $versionMatch = [regex]::Match(
        [string] $MainReleaseInfo.Version,
        '^(?<major>\d+)\.(?<minor>\d+)\.')
    if (!$lineMatch.Success -or !$versionMatch.Success) {
        return $null
    }
    $requestedMajor = [int] $lineMatch.Groups['major'].Value
    $requestedMinor = [int] $lineMatch.Groups['minor'].Value
    $mainMajor = [int] $versionMatch.Groups['major'].Value
    $mainMinor = [int] $versionMatch.Groups['minor'].Value
    $mainMilestone = [int] $MainReleaseInfo.SkiaMilestone
    if (
        $requestedMajor -ne $mainMajor -or
        $mainMinor -ne $mainMilestone -or
        $requestedMinor -ne ($mainMinor + 1) -or
        $requestedMinor -ne ($mainMilestone + 1)
    ) {
        return $null
    }
    return [pscustomobject] @{
        Line = $Line
        Milestone = $requestedMinor
        BaseBranch = 'main'
        BaseSha = $MainSha
        CurrentVersion = $MainReleaseInfo.Version
        CurrentMilestone = $mainMilestone
    }
}

function Get-SkiaUpstreamStatus(
    [string] $Root,
    [pscustomobject] $Maintenance,
    [pscustomobject] $Topology = $null
) {
    $topology = if ($Topology) {
        $Topology
    } else {
        Get-SkiaSyncTopology -Maintenance $Maintenance
    }
    if (!$topology) {
        return $null
    }
    $parentSkiaSha = Get-GitTreeEntrySha `
        -Root $Root `
        -Commit $Maintenance.Sha `
        -Path 'externals/skia'
    $upstreamSha = Get-RemoteBranchSha `
        -Root $Root `
        -Remote 'https://github.com/google/skia.git' `
        -Branch $topology.UpstreamRef
    $skiaBranches = Get-RemoteBranchShas `
        -Root $Root `
        -Remote 'https://github.com/mono/skia.git' `
        -Branches @($topology.SkiaBaseBranch, $topology.SyncBranch)
    $skiaBaseSha = $skiaBranches[$topology.SkiaBaseBranch]
    $syncBranchSha = $skiaBranches[$topology.SyncBranch]
    $compareRef = if ($syncBranchSha) {
        $topology.SyncBranch
    } else {
        $topology.SkiaBaseBranch
    }
    $compareSha = if ($syncBranchSha) { $syncBranchSha } else { $skiaBaseSha }

    if (!$upstreamSha) {
        return [pscustomobject] @{
            Milestone = $topology.Milestone
            UpstreamRef = $topology.UpstreamRef
            UpstreamSha = ''
            ParentBaseBranch = $topology.ParentBaseBranch
            ParentSkiaSha = $parentSkiaSha
            SkiaBaseBranch = $topology.SkiaBaseBranch
            SkiaBaseSha = $skiaBaseSha
            SyncBranch = $topology.SyncBranch
            SyncBranchSha = $syncBranchSha
            CompareRef = $compareRef
            CompareSha = $compareSha
            BehindBy = $null
            HasChanges = $false
            State = 'inconsistent'
            BlocksRelease = $true
            Message = "google/skia branch $($topology.UpstreamRef) does not exist."
        }
    }
    if (!$skiaBaseSha) {
        return [pscustomobject] @{
            Milestone = $topology.Milestone
            UpstreamRef = $topology.UpstreamRef
            UpstreamSha = $upstreamSha
            ParentBaseBranch = $topology.ParentBaseBranch
            ParentSkiaSha = $parentSkiaSha
            SkiaBaseBranch = $topology.SkiaBaseBranch
            SkiaBaseSha = ''
            SyncBranch = $topology.SyncBranch
            SyncBranchSha = $syncBranchSha
            CompareRef = $compareRef
            CompareSha = $compareSha
            BehindBy = $null
            HasChanges = $false
            State = 'inconsistent'
            BlocksRelease = $true
            Message = "mono/skia base branch $($topology.SkiaBaseBranch) does not exist."
        }
    }

    $comparison = Get-GitHubComparison `
        -Repository 'mono/skia' `
        -Base $upstreamSha `
        -Head $compareRef
    if ($null -eq $comparison -or $null -eq $comparison.behind_by) {
        throw "GitHub comparison for $($topology.UpstreamRef)...$compareRef did not report behind_by."
    }
    $behindBy = [int] $comparison.behind_by
    return [pscustomobject] @{
        Milestone = $topology.Milestone
        UpstreamRef = $topology.UpstreamRef
        UpstreamSha = $upstreamSha
        ParentBaseBranch = $topology.ParentBaseBranch
        ParentSkiaSha = $parentSkiaSha
        SkiaBaseBranch = $topology.SkiaBaseBranch
        SkiaBaseSha = $skiaBaseSha
        SyncBranch = $topology.SyncBranch
        SyncBranchSha = $syncBranchSha
        CompareRef = $compareRef
        CompareSha = $compareSha
        BehindBy = $behindBy
        HasChanges = $behindBy -gt 0
        State = if ($behindBy -gt 0) { 'changes available' } else { 'current' }
        BlocksRelease = $behindBy -gt 0
        Message = ''
    }
}

function Get-IncomingReleasePullRequest(
    [pscustomobject] $Maintenance,
    [string] $SyncBranchSha,
    [object[]] $PullRequests,
    [pscustomobject] $Topology = $null,
    [string] $SkiaSyncBranchSha = '',
    [string] $ParentSkiaSha = '',
    [string] $ParentSyncSkiaSha = ''
) {
    if (!$Maintenance) {
        return $null
    }
    $headBranch = if ($Topology) {
        $Topology.SyncBranch
    } else {
        Get-ExpectedSyncBranch -Maintenance $Maintenance
    }
    if (!$headBranch) {
        return $null
    }
    $pullRequests = @($PullRequests | Where-Object { $null -ne $_ })
    if (!$SyncBranchSha -and !$pullRequests.Count) {
        if ($SkiaSyncBranchSha -and $SkiaSyncBranchSha -ne $ParentSkiaSha) {
            return [pscustomobject] @{
                Number = $null
                Title = ''
                Url = ''
                HeadBranch = $headBranch
                HeadSha = ''
                BaseBranch = $Maintenance.Branch
                BaseSha = $Maintenance.Sha
                IsDraft = $false
                MergeState = 'UNKNOWN'
                State = 'inconsistent'
                BlocksRelease = $true
                Message = "mono/skia branch $headBranch is at $SkiaSyncBranchSha, but " +
                    "$($Maintenance.Branch) points to $ParentSkiaSha and no SkiaSharp sync branch or pull request exists."
            }
        }
        return $null
    }
    if ($pullRequests.Count -ne 1) {
        $message = if ($pullRequests.Count) {
            "$($pullRequests.Count) open pull requests use $headBranch -> $($Maintenance.Branch)."
        } else {
            "$headBranch exists at $SyncBranchSha without an open pull request to $($Maintenance.Branch)."
        }
        return [pscustomobject] @{
            Number = $null
            Title = ''
            Url = ''
            HeadBranch = $headBranch
            HeadSha = $SyncBranchSha
            BaseBranch = $Maintenance.Branch
            BaseSha = $Maintenance.Sha
            IsDraft = $false
            MergeState = 'UNKNOWN'
            State = 'inconsistent'
            BlocksRelease = $true
            Message = $message
        }
    }

    $pullRequest = $pullRequests[0]
    $issues = [Collections.Generic.List[string]]::new()
    if (!$SyncBranchSha) {
        $issues.Add("$headBranch does not exist on origin.")
    } elseif ([string] $pullRequest.headRefOid -ne $SyncBranchSha) {
        $issues.Add("PR head $($pullRequest.headRefOid) does not match $headBranch at $SyncBranchSha.")
    } elseif ($SkiaSyncBranchSha -and $ParentSyncSkiaSha -ne $SkiaSyncBranchSha) {
        $issues.Add(
            "Parent sync branch points to mono/skia $ParentSyncSkiaSha, " +
            "expected $SkiaSyncBranchSha.")
    }
    if ([string] $pullRequest.baseRefName -ne $Maintenance.Branch) {
        $issues.Add("PR targets $($pullRequest.baseRefName), expected $($Maintenance.Branch).")
    }
    $isDraft = [bool] $pullRequest.isDraft
    return [pscustomobject] @{
        Number = [int] $pullRequest.number
        Title = [string] $pullRequest.title
        Url = [string] $pullRequest.url
        HeadBranch = [string] $pullRequest.headRefName
        HeadSha = [string] $pullRequest.headRefOid
        BaseBranch = [string] $pullRequest.baseRefName
        BaseSha = [string] $pullRequest.baseRefOid
        IsDraft = $isDraft
        MergeState = [string] $pullRequest.mergeStateStatus
        State = if ($issues.Count) { 'inconsistent' } elseif ($isDraft) { 'draft' } else { 'open' }
        BlocksRelease = $issues.Count -gt 0 -or !$isDraft
        Message = $issues -join ' '
    }
}

function New-ReleaseAuditAction([string] $Kind, [string] $Message, [string] $Command = '') {
    return [pscustomobject] @{
        Kind = $Kind
        Message = $Message
        Command = $Command
    }
}

function Format-ReleasePackageBuild([pscustomobject] $Build) {
    if (!$Build) {
        return '-'
    }
    if (!$Build.Available) {
        return 'unavailable'
    }
    if (!$Build.BuildId) {
        return 'not built'
    }
    if ($Build.State -eq 'green') {
        return "#$($Build.BuildId) green / BAR $($Build.BarId)"
    }
    return "#$($Build.BuildId) $($Build.State)"
}

function New-ReleasePackageBuildAction(
    [pscustomobject] $Build,
    [string] $Purpose
) {
    if (!$Build -or !$Build.Available) {
        $message = if ($Build) { $Build.Message } else { 'The build was not checked.' }
        return New-ReleaseAuditAction `
            -Kind 'build-unavailable' `
            -Message "Internal package build check for $Purpose is unavailable: $message"
    }
    if ($Build.State -eq 'running') {
        return New-ReleaseAuditAction `
            -Kind 'wait-build' `
            -Message "Wait for skiasharp-package build #$($Build.BuildId) for $Purpose to complete: $($Build.Url)"
    }
    if ($Build.State -eq 'not built') {
        return New-ReleaseAuditAction `
            -Kind 'build' `
            -Message "Build $Purpose with skiasharp-package; no exact-tip build exists for $($Build.Branch)@$($Build.Commit)."
    }
    $message = "Fix or rerun skiasharp-package for ${Purpose}: $($Build.Message) $($Build.Url)"
    return New-ReleaseAuditAction `
        -Kind 'fix-build' `
        -Message $message.Trim()
}

function Get-ReleaseAuditState(
    [string] $Line,
    [pscustomobject] $Maintenance,
    [object[]] $Branches,
    [object[]] $Packages,
    [hashtable] $TagShas,
    [hashtable] $GitHubReleases,
    [pscustomobject] $Delta,
    [pscustomobject] $IncomingPullRequest = $null,
    [pscustomobject] $UpstreamSync = $null,
    [pscustomobject] $PendingMilestone = $null,
    [hashtable] $PackageBuilds = $null
) {
    $specific = @($Branches | Where-Object {
        $_.Name -match "^release/$([regex]::Escape($Line))\.\d+(?:\.\d+)?(?:-(?:preview|rc)\.[1-9]\d*)?$"
    } | Sort-Object { $_.Identity.SortKey })
    $latest = $specific | Select-Object -Last 1
    $actions = [Collections.Generic.List[object]]::new()
    $rows = [Collections.Generic.List[object]]::new()
    $publicByBranch = @{}
    $buildChecksEnabled = $null -ne $PackageBuilds

    foreach ($package in @($Packages | Sort-Object Version)) {
        $release = Get-ReleaseIdentity $package.Version
        if (!$publicByBranch.ContainsKey($release.Branch)) {
            $publicByBranch[$release.Branch] = [Collections.Generic.List[object]]::new()
        }
        $publicByBranch[$release.Branch].Add([pscustomobject] @{
            Package = $package
            Release = $release
        })
    }

    foreach ($branch in $specific) {
        $shipments = if ($publicByBranch.ContainsKey($branch.Name)) {
            @($publicByBranch[$branch.Name])
        } else {
            @()
        }
        if (!$shipments.Count) {
            $branchBuild = if ($buildChecksEnabled) { $PackageBuilds[$branch.Name] } else { $null }
            $state = if ($branch -eq $latest) { 'publish release branch' } else { 'superseded' }
            $action = if ($branch -ne $latest) {
                '-'
            } elseif (!$buildChecksEnabled -or ($branchBuild -and $branchBuild.Ready)) {
                'Publish release branch'
            } elseif ($branchBuild -and $branchBuild.State -eq 'running') {
                'Wait for package build'
            } else {
                'Fix package build'
            }
            $rows.Add([pscustomobject] @{
                Identity = $branch.Identity.Title
                Branch = $branch.Name
                BranchSha = $branch.Sha
                NuGet = '-'
                SourceBranch = '-'
                SourceCommit = '-'
                PackageBuild = $branchBuild
                Tag = '-'
                GitHubRelease = '-'
                State = $state
                Action = $action
            })
            continue
        }

        foreach ($shipment in $shipments) {
            $package = $shipment.Package
            $release = $shipment.Release
            $tag = $release.Tag
            $tagSha = $TagShas[$tag]
            $githubRelease = $GitHubReleases[$tag]
            $sourceMatches = $package.Branch -eq $branch.Name -and $package.Commit -eq $branch.Sha
            $releaseConsistent = $githubRelease -and
                $githubRelease.tagName -eq $tag -and
                ([bool] $githubRelease.isPrerelease) -eq ([bool] $release.IsPrerelease)
            $needsFinish = !$tagSha -or $tagSha -ne $package.Commit -or !$releaseConsistent
            $state = if ($needsFinish) {
                'finish shipment'
            } elseif (!$sourceMatches) {
                'public source differs from branch'
            } else {
                'finished'
            }
            $action = if ($needsFinish) {
                'Finish shipment'
            } elseif (!$sourceMatches) {
                'Investigate provenance'
            } else {
                '-'
            }
            $rows.Add([pscustomobject] @{
                Identity = $release.Branch -replace '^release/'
                Branch = $branch.Name
                BranchSha = $branch.Sha
                NuGet = $package.Version
                SourceBranch = $package.Branch
                SourceCommit = $package.Commit
                PackageBuild = $null
                Tag = if ($tagSha) { "$tag@$($tagSha.Substring(0, 12))" } else { "$tag (missing)" }
                GitHubRelease = if ($githubRelease) { if ($releaseConsistent) { 'present' } else { 'inconsistent' } } else { 'missing' }
                State = $state
                Action = $action
            })
            if ($needsFinish) {
                $actions.Add((New-ReleaseAuditAction `
                    -Kind 'finish' `
                    -Message "Finish public shipment $($package.Version)." `
                    -Command "pwsh ./scripts/infra/publishing/finish-release.ps1 -Version $($package.Version) -Mode DryRun"))
            }
            if (!$sourceMatches) {
                $actions.Add((New-ReleaseAuditAction `
                    -Kind 'investigate' `
                    -Message "Investigate public shipment $($package.Version): NuGet names $($package.Branch)@$($package.Commit), but $($branch.Name) is at $($branch.Sha)."))
            }
        }
    }

    foreach ($branchName in $publicByBranch.Keys | Where-Object { $_ -notin $specific.Name }) {
        foreach ($shipment in $publicByBranch[$branchName]) {
            $package = $shipment.Package
            $release = $shipment.Release
            $tagSha = $TagShas[$release.Tag]
            $githubRelease = $GitHubReleases[$release.Tag]
            $releaseConsistent = $githubRelease -and $tagSha -eq $package.Commit -and
                ([bool] $githubRelease.isPrerelease) -eq ([bool] $release.IsPrerelease)
            $needsFinish = !$tagSha -or !$releaseConsistent
            $rows.Add([pscustomobject] @{
                Identity = $release.Branch -replace '^release/'
                Branch = $branchName
                BranchSha = '-'
                NuGet = $package.Version
                SourceBranch = $package.Branch
                SourceCommit = $package.Commit
                PackageBuild = $null
                Tag = if ($tagSha) { "$($release.Tag)@$($tagSha.Substring(0, 12))" } else { "$($release.Tag) (missing)" }
                GitHubRelease = if ($githubRelease) { if ($releaseConsistent) { 'present' } else { 'inconsistent' } } else { 'missing' }
                State = if ($needsFinish) { 'finish shipment' } else { 'finished branch unavailable' }
                Action = if ($needsFinish) { 'Finish shipment' } else { 'Investigate branch' }
            })
            if ($needsFinish) {
                $actions.Add((New-ReleaseAuditAction `
                    -Kind 'finish' `
                    -Message "Finish public shipment $($package.Version)." `
                    -Command "pwsh ./scripts/infra/publishing/finish-release.ps1 -Version $($package.Version) -Mode DryRun"))
            }
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'investigate' `
                -Message "Investigate public shipment $($package.Version): source branch $branchName is not available."))
        }
    }

    $latestShipments = if ($latest -and $publicByBranch.ContainsKey($latest.Name)) {
        @($publicByBranch[$latest.Name])
    } else {
        @()
    }
    $latestPublic = if ($latestShipments.Count) {
        @($latestShipments | Where-Object {
            $_.Package.Branch -eq $latest.Name -and $_.Package.Commit -eq $latest.Sha
        })
    } else {
        @()
    }
    $latestFinished = $latestPublic.Count -gt 0 -and !@($rows | Where-Object {
        $_.Branch -eq $latest.Name -and $_.State -eq 'finish shipment'
    }).Count
    $latestBuild = if ($buildChecksEnabled -and $latest) {
        $PackageBuilds[$latest.Name]
    } else {
        $null
    }
    if ($latest -and !$latestShipments.Count) {
        if (!$buildChecksEnabled -or ($latestBuild -and $latestBuild.Ready)) {
            $buildEvidence = if ($latestBuild) {
                " Exact-tip build #$($latestBuild.BuildId) succeeded with BAR $($latestBuild.BarId)."
            } else {
                ''
            }
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'publish' `
                -Message "Publish packages from $($latest.Name) through the protected BAR-to-NuGet process.$buildEvidence Optionally validate its exact BAR with release-testing first."))
        } else {
            $actions.Add((New-ReleasePackageBuildAction `
                -Build $latestBuild `
                -Purpose $latest.Name))
        }
    }
    if ($UpstreamSync -and $UpstreamSync.BlocksRelease) {
        if ($UpstreamSync.HasChanges) {
            $count = [int] $UpstreamSync.BehindBy
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'sync-skia' `
                -Message "Sync $count newer upstream Skia commit$(if ($count -eq 1) { '' } else { 's' }) from $($UpstreamSync.UpstreamRef) into $($UpstreamSync.ParentBaseBranch) before its next release cut." `
                -Command "gh workflow run auto-skia-sync.lock.yml --repo $ReleaseRepository --ref main -f target=$($UpstreamSync.Milestone)"))
        } else {
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'investigate-upstream' `
                -Message "Investigate upstream Skia state: $($UpstreamSync.Message)"))
        }
    }
    if ($IncomingPullRequest -and $IncomingPullRequest.BlocksRelease) {
        if ($IncomingPullRequest.Number -and $IncomingPullRequest.State -ne 'inconsistent') {
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'merge-sync' `
                -Message "Review and merge incoming Skia sync PR #$($IncomingPullRequest.Number): $($IncomingPullRequest.Title)." `
                -Command "gh pr view $($IncomingPullRequest.Number) --repo $ReleaseRepository --web"))
        } else {
            $message = if ($IncomingPullRequest.Number) {
                "Investigate incoming maintenance PR #$($IncomingPullRequest.Number): $($IncomingPullRequest.Message)"
            } else {
                $IncomingPullRequest.Message
            }
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'investigate-sync' `
                -Message $message))
        }
    } elseif ($PendingMilestone -and $IncomingPullRequest) {
        $actions.Add((New-ReleaseAuditAction `
            -Kind 'review-sync' `
            -Message "Complete incoming milestone PR #$($IncomingPullRequest.Number) before $($PendingMilestone.Line) becomes the active main line: $($IncomingPullRequest.Title)." `
            -Command "gh pr view $($IncomingPullRequest.Number) --repo $ReleaseRepository --web"))
    } elseif (
        $PendingMilestone -and
        $UpstreamSync -and
        $UpstreamSync.State -eq 'current'
    ) {
        $actions.Add((New-ReleaseAuditAction `
            -Kind 'investigate-sync' `
            -Message "$($UpstreamSync.UpstreamRef) is synchronized in mono/skia, but no SkiaSharp pull request is open to activate $($PendingMilestone.Line)."))
    }
    $releaseBlocked = [bool] (
        ($IncomingPullRequest -and $IncomingPullRequest.BlocksRelease) -or
        ($UpstreamSync -and $UpstreamSync.BlocksRelease)
    )
    $maintenanceBuild = if ($buildChecksEnabled -and $Maintenance) {
        $PackageBuilds[$Maintenance.Branch]
    } else {
        $null
    }
    if ($Maintenance -and !$latest) {
        if (!$releaseBlocked) {
            if (!$buildChecksEnabled -or ($maintenanceBuild -and $maintenanceBuild.Ready)) {
                $next = Get-NextReleaseIdentity -Line $Line -Latest $null -MaintenanceVersion $Maintenance.Version
                $actions.Add((New-ReleaseAuditAction `
                    -Kind 'start' `
                    -Message "Start the first release from $($Maintenance.Branch)." `
                    -Command "pwsh ./scripts/infra/publishing/prepare-release.ps1 -Base $($Maintenance.Branch) -Release $next -Mode DryRun"))
            } else {
                $actions.Add((New-ReleasePackageBuildAction `
                    -Build $maintenanceBuild `
                    -Purpose "$($Maintenance.Branch) before its first release cut"))
            }
        }
    } elseif ($Maintenance -and $Delta.Queued.Count) {
        if ($latest -and !$latestPublic.Count) {
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'queued' `
                -Message "$($Delta.Queued.Count) newer maintenance commit(s) are queued until $($latest.Name) is published."))
        } elseif (!$releaseBlocked -and (!$latest -or $latestFinished)) {
            if (!$buildChecksEnabled -or ($maintenanceBuild -and $maintenanceBuild.Ready)) {
                $next = Get-NextReleaseIdentity -Line $Line -Latest $latest -MaintenanceVersion $Maintenance.Version
                if ($next) {
                    $actions.Add((New-ReleaseAuditAction `
                        -Kind 'start' `
                        -Message "Start the next release from $($Maintenance.Branch)." `
                        -Command "pwsh ./scripts/infra/publishing/prepare-release.ps1 -Base $($Maintenance.Branch) -Release $next -Mode DryRun"))
                } else {
                    $actions.Add((New-ReleaseAuditAction `
                        -Kind 'prepare' `
                        -Message "Prepare the next release from $($Maintenance.Branch); its identity cannot be derived safely."))
                }
            } else {
                $actions.Add((New-ReleasePackageBuildAction `
                    -Build $maintenanceBuild `
                    -Purpose "$($Maintenance.Branch) before its next release cut"))
            }
        }
    }
    return [pscustomobject] @{
        Line = $Line
        Maintenance = $Maintenance
        LatestSpecificBranch = $latest
        MaintenanceDelta = $Delta
        IncomingPullRequest = $IncomingPullRequest
        UpstreamSync = $UpstreamSync
        PendingMilestone = $PendingMilestone
        PackageBuilds = $PackageBuilds
        Releases = @($rows)
        Actions = @($actions)
    }
}

function Format-ReleaseAuditField(
    [string] $Label,
    [string] $Value,
    [string] $ValueStyle = ''
) {
    $labelText = '{0,-19}' -f "${Label}:"
    if (!(Test-ReleaseAuditStyling)) {
        return "$labelText $Value"
    }
    return "$($PSStyle.Bold)$labelText$($PSStyle.Reset) $ValueStyle$Value$($PSStyle.Reset)"
}

function Test-ReleaseAuditStyling {
    if ([Console]::IsOutputRedirected -or $null -eq $PSStyle) {
        return $false
    }
    return [string] $PSStyle.OutputRendering -ne 'PlainText'
}

function Get-ReleaseAuditOutputWidth {
    try {
        $width = [int] $Host.UI.RawUI.BufferSize.Width
        if ($width -gt 0) {
            return [Math]::Max(180, [Math]::Min(240, $width - 1))
        }
    } catch {
        # Non-interactive hosts may not expose terminal dimensions.
    }
    return 200
}

function Get-ReleaseAuditTableRows([pscustomobject] $State) {
    return @(
        foreach ($release in $State.Releases) {
            $branchSha = if ($release.BranchSha -eq '-') {
                '-'
            } else {
                $release.BranchSha.Substring(0, 12)
            }
            $state = if (
                $release.Action -eq '-' -or
                $release.Action.Equals($release.State, [StringComparison]::OrdinalIgnoreCase)
            ) {
                $release.State
            } else {
                "$($release.State); $($release.Action)"
            }
            [pscustomobject] [ordered] @{
                Identity = $release.Identity
                'Branch SHA' = $branchSha
                NuGet = $release.NuGet
                'Build / BAR' = (Format-ReleasePackageBuild $release.PackageBuild)
                Tag = $release.Tag
                'GitHub Release' = $release.GitHubRelease
                'State / action' = $state
            }
        }
    )
}

function Write-ReleaseAuditReport([pscustomobject] $State) {
    $maintenance = if ($State.Maintenance) {
        "$($State.Maintenance.Branch)@$($State.Maintenance.Sha.Substring(0, 12)) $($State.Maintenance.Version) ($($State.Maintenance.Label))"
    } else {
        'none'
    }
    $latest = if ($State.LatestSpecificBranch) {
        "$($State.LatestSpecificBranch.Name)@$($State.LatestSpecificBranch.Sha.Substring(0, 12))"
    } else {
        'none'
    }
    $incoming = if ($State.IncomingPullRequest) {
        if ($State.IncomingPullRequest.Number) {
            $headSha = if ($State.IncomingPullRequest.HeadSha.Length -ge 12) {
                $State.IncomingPullRequest.HeadSha.Substring(0, 12)
            } else {
                $State.IncomingPullRequest.HeadSha
            }
            "#$($State.IncomingPullRequest.Number) " +
                "$($State.IncomingPullRequest.HeadBranch)@$headSha " +
                "-> $($State.IncomingPullRequest.BaseBranch) " +
                "($($State.IncomingPullRequest.State), $($State.IncomingPullRequest.MergeState))"
        } else {
            $State.IncomingPullRequest.Message
        }
    } else {
        'none'
    }
    $upstream = if ($State.UpstreamSync) {
        if ($State.UpstreamSync.State -eq 'inconsistent') {
            $State.UpstreamSync.Message
        } else {
            $upstreamSha = $State.UpstreamSync.UpstreamSha.Substring(0, 12)
            $suffix = if ($State.UpstreamSync.HasChanges) {
                "$($State.UpstreamSync.BehindBy) newer commit$(if ($State.UpstreamSync.BehindBy -eq 1) { '' } else { 's' })"
            } else {
                'current'
            }
            "$($State.UpstreamSync.UpstreamRef)@$upstreamSha -> " +
                "mono/skia:$($State.UpstreamSync.CompareRef) ($suffix)"
        }
    } else {
        'none'
    }
    Write-Output (Format-ReleaseAuditField `
        -Label 'Release line' `
        -Value $State.Line `
        -ValueStyle $PSStyle.Foreground.BrightCyan)
    Write-Output (Format-ReleaseAuditField -Label 'Maintenance' -Value $maintenance)
    if ($State.Maintenance) {
        $maintenanceBuild = if ($State.PackageBuilds) {
            $State.PackageBuilds[$State.Maintenance.Branch]
        } else {
            $null
        }
        Write-Output (Format-ReleaseAuditField `
            -Label 'Maintenance build' `
            -Value (Format-ReleasePackageBuild $maintenanceBuild) `
            -ValueStyle $(if ($maintenanceBuild -and $maintenanceBuild.Ready) {
                $PSStyle.Foreground.Green
            } else {
                $PSStyle.Foreground.Yellow
            }))
    }
    if ($State.PendingMilestone) {
        $pendingMilestone = "$($State.PendingMilestone.Line) via m$($State.PendingMilestone.Milestone) -> main " +
            "(current $($State.PendingMilestone.CurrentVersion)/m$($State.PendingMilestone.CurrentMilestone))"
        Write-Output (Format-ReleaseAuditField `
            -Label 'Pending milestone' `
            -Value $pendingMilestone `
            -ValueStyle $PSStyle.Foreground.Yellow)
    }
    Write-Output (Format-ReleaseAuditField -Label 'Latest branch' -Value $latest)
    Write-Output (Format-ReleaseAuditField `
        -Label 'Maintenance delta' `
        -Value $State.MaintenanceDelta.Text `
        -ValueStyle $(if ($State.MaintenanceDelta.Queued.Count) {
            $PSStyle.Foreground.Yellow
        } else {
            $PSStyle.Foreground.Green
        }))
    Write-Output (Format-ReleaseAuditField `
        -Label 'Upstream Skia' `
        -Value $upstream `
        -ValueStyle $(if ($State.UpstreamSync -and $State.UpstreamSync.BlocksRelease) {
            $PSStyle.Foreground.Yellow
        } else {
            $PSStyle.Foreground.Green
        }))
    Write-Output (Format-ReleaseAuditField `
        -Label 'Incoming sync PR' `
        -Value $incoming `
        -ValueStyle $(if ($State.IncomingPullRequest) {
            $PSStyle.Foreground.Yellow
        } else {
            $PSStyle.Foreground.Green
        }))
    Write-Output ''
    $rows = @(Get-ReleaseAuditTableRows -State $State)
    if ($rows.Count) {
        $table = $rows |
            Format-Table -AutoSize -Wrap |
            Out-String -Width (Get-ReleaseAuditOutputWidth)
        Write-Output $table.TrimEnd()
    } else {
        Write-Output 'No specific release branches or public shipments.'
    }
    $nextActionsStyle = if ($State.Actions.Count) {
        $PSStyle.Foreground.Yellow
    } else {
        $PSStyle.Foreground.Green
    }
    if (Test-ReleaseAuditStyling) {
        Write-Output "$($PSStyle.Bold)$($nextActionsStyle)Next actions:$($PSStyle.Reset)"
    } else {
        Write-Output 'Next actions:'
    }
    if (!$State.Actions.Count) {
        Write-Output '1. None.'
    } else {
        $index = 0
        foreach ($action in $State.Actions) {
            $index++
            Write-Output "$index. $($action.Message)"
            if ($action.Command) {
                Write-Output "   $($action.Command)"
            }
        }
    }
}

try {
    $root = Get-GitRepositoryRoot -Path $PSScriptRoot
    $mainSha = $null
    $mainReleaseInfo = $null
    $maintenanceBranch = "release/$Version.x"
    $maintenanceSha = Get-RemoteBranchSha -Root $root -Remote origin -Branch $maintenanceBranch
    if ($maintenanceSha) {
        $maintenanceSha = Get-ResolvedGitCommit `
            -Root $root `
            -Reference $maintenanceBranch
        $releaseInfo = Get-SkiaSharpReleaseInfoAtCommit -Root $root -Commit $maintenanceSha
        $maintenance = [pscustomobject] @{
            Branch = $maintenanceBranch
            Sha = $maintenanceSha
            Version = $releaseInfo.Version
            SkiaMilestone = $releaseInfo.SkiaMilestone
            Label = 'servicing'
        }
    } else {
        $mainSha = Get-ResolvedGitCommit -Root $root -Reference 'main'
        $mainReleaseInfo = Get-SkiaSharpReleaseInfoAtCommit -Root $root -Commit $mainSha
        $maintenance = if ($mainReleaseInfo.Version -match "^$([regex]::Escape($Version))\.") {
            [pscustomobject] @{
                Branch = 'main'
                Sha = $mainSha
                Version = $mainReleaseInfo.Version
                SkiaMilestone = $mainReleaseInfo.SkiaMilestone
                Label = 'current main'
            }
        } else {
            $null
        }
    }
    $pendingMilestone = if (!$maintenance -and $mainReleaseInfo) {
        Get-PendingMainMilestone `
            -Line $Version `
            -MainSha $mainSha `
            -MainReleaseInfo $mainReleaseInfo
    } else {
        $null
    }
    $syncBase = if ($maintenance) {
        $maintenance
    } elseif ($pendingMilestone) {
        [pscustomobject] @{
            Branch = 'main'
            Sha = $pendingMilestone.BaseSha
            Version = $pendingMilestone.CurrentVersion
            SkiaMilestone = $pendingMilestone.CurrentMilestone
            Label = 'current main'
        }
    } else {
        $null
    }
    $syncTopology = if ($syncBase) {
        Get-SkiaSyncTopology `
            -Maintenance $syncBase `
            -TargetMilestone $(if ($pendingMilestone) { $pendingMilestone.Milestone } else { 0 })
    } else {
        $null
    }
    $upstreamSync = if ($syncBase) {
        Get-SkiaUpstreamStatus `
            -Root $root `
            -Maintenance $syncBase `
            -Topology $syncTopology
    } else {
        $null
    }
    $incomingPullRequest = $null
    if ($syncBase) {
        $syncBranch = $syncTopology.SyncBranch
        if ($syncBranch) {
            $syncSha = Get-RemoteBranchSha -Root $root -Remote origin -Branch $syncBranch
            $parentSyncSkiaSha = if ($syncSha) {
                $resolvedSyncSha = Get-ResolvedGitCommit `
                    -Root $root `
                    -Reference $syncBranch
                Get-GitTreeEntrySha `
                    -Root $root `
                    -Commit $resolvedSyncSha `
                    -Path 'externals/skia'
            } else {
                ''
            }
            $syncPullRequests = Get-GitHubOpenPullRequests `
                -Repository $ReleaseRepository `
                -Head $syncBranch `
                -Base $syncBase.Branch
            $incomingPullRequest = Get-IncomingReleasePullRequest `
                -Maintenance $syncBase `
                -SyncBranchSha $syncSha `
                -PullRequests $syncPullRequests `
                -Topology $syncTopology `
                -SkiaSyncBranchSha $upstreamSync.SyncBranchSha `
                -ParentSkiaSha $upstreamSync.ParentSkiaSha `
                -ParentSyncSkiaSha $parentSyncSkiaSha
        }
    }
    $legacyIncomingPullRequest = $null
    if ($maintenance) {
        $legacySelection = Get-LegacyTransitionSyncSelection -Maintenance $maintenance
        if ($legacySelection) {
            $legacySyncBranch = $legacySelection.Topology.SyncBranch
            $legacySyncSha = Get-RemoteBranchSha `
                -Root $root `
                -Remote origin `
                -Branch $legacySyncBranch
            $legacyParentSyncSkiaSha = if ($legacySyncSha) {
                $resolvedLegacySyncSha = Get-ResolvedGitCommit `
                    -Root $root `
                    -Reference $legacySyncBranch
                Get-GitTreeEntrySha `
                    -Root $root `
                    -Commit $resolvedLegacySyncSha `
                    -Path 'externals/skia'
            } else {
                ''
            }
            $legacySkiaBranches = Get-RemoteBranchShas `
                -Root $root `
                -Remote 'https://github.com/mono/skia.git' `
                -Branches @($legacySyncBranch)
            $legacyIncomingPullRequest = Get-LegacyTransitionIncomingPullRequest `
                -Maintenance $maintenance `
                -Selection $legacySelection `
                -SyncBranchSha $legacySyncSha `
                -SkiaSyncBranchSha $legacySkiaBranches[$legacySyncBranch] `
                -ParentSkiaSha $upstreamSync.ParentSkiaSha `
                -ParentSyncSkiaSha $legacyParentSyncSkiaSha
        }
    }
    $incomingPullRequest = Select-IncomingReleasePullRequest `
        -Expected $incomingPullRequest `
        -Legacy $legacyIncomingPullRequest
    $branches = @(Get-ReleaseBranches -Root $root -Line $Version)
    $linePackages = foreach ($publicVersion in Get-NuGetPackageVersions -PackageId 'SkiaSharp') {
        if ($publicVersion -match "^$([regex]::Escape($Version))\.\d+(?:\.\d+)?(?:-(?:preview|rc)\.[1-9]\d*\.\d+(?:\.\d+)?)?$") {
            $source = Get-NuGetPackageSource -PackageId 'SkiaSharp' -PackageVersion $publicVersion
            [pscustomobject] @{ Version = $publicVersion; Branch = $source.Branch; Commit = $source.Commit }
        }
    }
    $githubReleases = Get-GitHubReleaseMap -Repository $ReleaseRepository
    $tagShas = Get-RemoteTagMap -Root $root -Remote origin -Pattern "refs/tags/v$Version.*"
    $latest = @($branches | Where-Object {
        $_.Name -match "^release/$([regex]::Escape($Version))\.\d+(?:\.\d+)?(?:-(?:preview|rc)\.[1-9]\d*)?$"
    } | Sort-Object { $_.Identity.SortKey } | Select-Object -Last 1)
    $packageBuilds = @{}
    if ($maintenance) {
        $packageBuilds[$maintenance.Branch] = Get-ReleasePackageBuild `
            -Branch $maintenance.Branch `
            -Commit $maintenance.Sha
    }
    $latestHasShipment = if ($latest) {
        @($linePackages | Where-Object {
            (Get-ReleaseIdentity $_.Version).Branch -eq $latest.Name
        }).Count -gt 0
    } else {
        $false
    }
    if ($latest -and !$latestHasShipment) {
        $packageBuilds[$latest.Name] = Get-ReleasePackageBuild `
            -Branch $latest.Name `
            -Commit $latest.Sha
    }
    $delta = if ($maintenance -and $latest) {
        Get-MaintenanceDelta -Root $root -MaintenanceSha $maintenance.Sha -ReleaseSha $latest.Sha
    } else {
        [pscustomobject] @{ Commits = @(); Queued = @(); VersionBumpOnly = $false; Text = 'none' }
    }
    $state = Get-ReleaseAuditState `
        -Line $Version `
        -Maintenance $maintenance `
        -Branches $branches `
        -Packages $linePackages `
        -TagShas $tagShas `
        -GitHubReleases $githubReleases `
        -Delta $delta `
        -IncomingPullRequest $incomingPullRequest `
        -UpstreamSync $upstreamSync `
        -PendingMilestone $pendingMilestone `
        -PackageBuilds $packageBuilds
    $buildCheckUnavailable = @($packageBuilds.Values | Where-Object {
        !$_.Available
    }).Count -gt 0
    $exitCode = if ($buildCheckUnavailable) {
        2
    } elseif ($state.Actions.Count) {
        1
    } else {
        0
    }
    if ($Json) {
        [ordered] @{
            line = $state.Line
            maintenance = $state.Maintenance
            latestSpecificBranch = $state.LatestSpecificBranch
            maintenanceDelta = $state.MaintenanceDelta
            incomingPullRequest = $state.IncomingPullRequest
            upstreamSync = $state.UpstreamSync
            pendingMilestone = $state.PendingMilestone
            packageBuilds = $state.PackageBuilds
            releases = $state.Releases
            actions = $state.Actions
            exitCode = $exitCode
        } | ConvertTo-Json -Depth 8
    } else {
        Write-ReleaseAuditReport $state
    }
    exit $exitCode
} catch {
    $failure = [pscustomobject] @{
        line = $Version
        error = $_.Exception.Message
        exitCode = 2
    }
    if ($Json) {
        $failure | ConvertTo-Json -Depth 4
    } else {
        Write-Error "Release-state audit unavailable: $($_.Exception.Message)" -ErrorAction Continue
    }
    exit 2
}
