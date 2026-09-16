#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Summarizes one SkiaSharp release line without changing release state.

.DESCRIPTION
    Reads the maintenance branch, release branches, public NuGet provenance,
    tags, and GitHub Releases for one A.B line. Detailed release validation
    remains the responsibility of prepare-release.ps1 and finish-release.ps1.
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

function Get-ExpectedSyncBranch([pscustomobject] $Maintenance) {
    if (!$Maintenance) {
        return $null
    }
    if ($Maintenance.Branch -eq 'main') {
        $milestone = [int] $Maintenance.SkiaMilestone
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

function Get-IncomingReleasePullRequest(
    [pscustomobject] $Maintenance,
    [string] $SyncBranchSha,
    [object[]] $PullRequests
) {
    if (!$Maintenance) {
        return $null
    }
    $headBranch = Get-ExpectedSyncBranch -Maintenance $Maintenance
    if (!$headBranch) {
        return $null
    }
    $pullRequests = @($PullRequests | Where-Object { $null -ne $_ })
    if (!$SyncBranchSha -and !$pullRequests.Count) {
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

function Get-ReleaseAuditState(
    [string] $Line,
    [pscustomobject] $Maintenance,
    [object[]] $Branches,
    [object[]] $Packages,
    [hashtable] $TagShas,
    [hashtable] $GitHubReleases,
    [pscustomobject] $Delta,
    [pscustomobject] $IncomingPullRequest = $null
) {
    $specific = @($Branches | Where-Object {
        $_.Name -match "^release/$([regex]::Escape($Line))\.\d+(?:\.\d+)?(?:-(?:preview|rc)\.[1-9]\d*)?$"
    } | Sort-Object { $_.Identity.SortKey })
    $latest = $specific | Select-Object -Last 1
    $actions = [Collections.Generic.List[object]]::new()
    $rows = [Collections.Generic.List[object]]::new()
    $publicByBranch = @{}

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
            $state = if ($branch -eq $latest) { 'publish release branch' } else { 'superseded' }
            $action = if ($branch -eq $latest) { 'Publish release branch' } else { '-' }
            $rows.Add([pscustomobject] @{
                Identity = $branch.Identity.Title
                Branch = $branch.Name
                BranchSha = $branch.Sha
                NuGet = '-'
                SourceBranch = '-'
                SourceCommit = '-'
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
                !$githubRelease.isDraft -and
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
                !$githubRelease.isDraft -and
                ([bool] $githubRelease.isPrerelease) -eq ([bool] $release.IsPrerelease)
            $needsFinish = !$tagSha -or !$releaseConsistent
            $rows.Add([pscustomobject] @{
                Identity = $release.Branch -replace '^release/'
                Branch = $branchName
                BranchSha = '-'
                NuGet = $package.Version
                SourceBranch = $package.Branch
                SourceCommit = $package.Commit
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
    if ($latest -and !$latestShipments.Count) {
        $actions.Add((New-ReleaseAuditAction `
            -Kind 'publish' `
            -Message "Publish packages from $($latest.Name) through the protected BAR-to-NuGet process. Optionally validate its exact BAR with release-testing first."))
    }
    if ($IncomingPullRequest -and $IncomingPullRequest.BlocksRelease) {
        if ($IncomingPullRequest.Number -and $IncomingPullRequest.State -ne 'inconsistent') {
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'merge-sync' `
                -Message "Review and merge incoming maintenance PR #$($IncomingPullRequest.Number): $($IncomingPullRequest.Title)." `
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
    }
    if ($Maintenance -and !$latest) {
        if (!$IncomingPullRequest -or !$IncomingPullRequest.BlocksRelease) {
            $next = Get-NextReleaseIdentity -Line $Line -Latest $null -MaintenanceVersion $Maintenance.Version
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'start' `
                -Message "Start the first release from $($Maintenance.Branch)." `
                -Command "pwsh ./scripts/infra/publishing/prepare-release.ps1 -Base $($Maintenance.Branch) -Release $next -Mode DryRun"))
        }
    } elseif ($Maintenance -and $Delta.Queued.Count) {
        if ($latest -and !$latestPublic.Count) {
            $actions.Add((New-ReleaseAuditAction `
                -Kind 'queued' `
                -Message "$($Delta.Queued.Count) newer maintenance commit(s) are queued until $($latest.Name) is published."))
        } elseif ((!$IncomingPullRequest -or !$IncomingPullRequest.BlocksRelease) -and (!$latest -or $latestFinished)) {
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
        }
    }
    return [pscustomobject] @{
        Line = $Line
        Maintenance = $Maintenance
        LatestSpecificBranch = $latest
        MaintenanceDelta = $Delta
        IncomingPullRequest = $IncomingPullRequest
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
    Write-Output (Format-ReleaseAuditField `
        -Label 'Release line' `
        -Value $State.Line `
        -ValueStyle $PSStyle.Foreground.BrightCyan)
    Write-Output (Format-ReleaseAuditField -Label 'Maintenance' -Value $maintenance)
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
    $incomingPullRequest = $null
    if ($maintenance) {
        $syncBranch = Get-ExpectedSyncBranch -Maintenance $maintenance
        if ($syncBranch) {
            $syncSha = Get-RemoteBranchSha -Root $root -Remote origin -Branch $syncBranch
            $syncPullRequests = Get-GitHubOpenPullRequests `
                -Repository $ReleaseRepository `
                -Head $syncBranch `
                -Base $maintenance.Branch
            $incomingPullRequest = Get-IncomingReleasePullRequest `
                -Maintenance $maintenance `
                -SyncBranchSha $syncSha `
                -PullRequests $syncPullRequests
        }
    }
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
        -IncomingPullRequest $incomingPullRequest
    $exitCode = if ($state.Actions.Count) { 1 } else { 0 }
    if ($Json) {
        [ordered] @{
            line = $state.Line
            maintenance = $state.Maintenance
            latestSpecificBranch = $state.LatestSpecificBranch
            maintenanceDelta = $state.MaintenanceDelta
            incomingPullRequest = $state.IncomingPullRequest
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
        Write-Error "Release-state audit unavailable: $($_.Exception.Message)"
    }
    exit 2
}
