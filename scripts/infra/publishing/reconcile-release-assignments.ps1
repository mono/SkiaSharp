#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reconciles merged pull requests and linked issues to shipped release milestones.

.PARAMETER Version
    The released numeric SkiaSharp version, such as 4.153.0 or 4.153.0.1.

.PARAMETER Repository
    The GitHub repository whose release assignments are maintained.

.PARAMETER Push
    Performs GitHub milestone assignments. Without this switch, the script is
    read-only and reports exact skipped mutations.
#>

param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:\.\d+)?$')]
    [string] $Version,

    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository = 'mono/SkiaSharp',

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'ReleaseMilestones.Common.psm1') -ArgumentList ([bool] $Push) -Force
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }
$root = Get-GitRepositoryRoot

# 1. Reconcile shipped commits, pull requests, and linked issues.
Write-ReleaseStatus start "Release assignment reconciliation for $Version ($mode)."

# 1.1 Refresh release refs and calculate the exact shipped branch/tag boundaries.
$null = Invoke-Git -Root $root -Arguments @('fetch', 'origin', '--prune')
$tags = Get-RemoteReleaseTags -Root $root -NumericVersion $Version
$branchSet = Get-ReleaseBranches -Root $root -Version $Version
$branches = @($branchSet.Selected)
$previous = Get-PreviousStableBranch -Branches $branchSet.All -Version $Version
$warnings = [System.Collections.Generic.List[string]]::new()
if (!$previous) {
    $warnings.Add("No previous stable release boundary exists for $Version.")
}

$mergeBases = @{}
foreach ($branch in $branches) {
    $mergeBases[$branch.Name] = (Invoke-Git -Root $root -Arguments @(
        'merge-base',
        'origin/main',
        "origin/$($branch.Name)"
    )).Output
    if (!$mergeBases[$branch.Name]) {
        $warnings.Add("No merge-base exists for $($branch.Name).")
    }
}
$previousBase = if ($previous) {
    (Invoke-Git -Root $root -Arguments @('merge-base', 'origin/main', "origin/$($previous.Name)")).Output
} else {
    $null
}
if ($previous -and !$previousBase) {
    $warnings.Add("No merge-base exists for previous boundary $($previous.Name).")
}

# 1.2 Roll unshipped branch milestones forward and collect required assignments.
$effective = @(Get-EffectiveMilestoneTitles -Branches $branches -Tags $tags)
for ($index = 1; $index -lt $branches.Count; $index++) {
    $previousEffective = $effective[$index - 1]
    $currentEffective = $effective[$index]
    if ($previousEffective -and $currentEffective -and $previousEffective -ne $currentEffective -and
        $mergeBases[$branches[$index - 1].Name] -eq $mergeBases[$branches[$index].Name]) {
        $warnings.Add(
            "Release boundaries for $previousEffective and $currentEffective resolve to the same commit.")
    }
}
$milestones = Get-GitHubMilestoneMap -Repository $Repository
$operations = [System.Collections.Generic.List[object]]::new()
$correct = 0
for ($index = 0; $index -lt $branches.Count; $index++) {
    $targetTitle = $effective[$index]
    if (!$targetTitle) {
        continue
    }
    if (!$milestones.ContainsKey($targetTitle)) {
        $warnings.Add("Milestone $targetTitle does not exist.")
        continue
    }
    $start = if ($index -eq 0) { $previousBase } else { $mergeBases[$branches[$index - 1].Name] }
    $end = $mergeBases[$branches[$index].Name]
    if (!$start -or !$end) {
        $warnings.Add("Release boundaries are missing for $targetTitle.")
        continue
    }
    $ancestor = Invoke-Git -Root $root -Arguments @('merge-base', '--is-ancestor', $start, $end) -AllowFailure
    if ($ancestor.ExitCode -ne 0) {
        $warnings.Add("Release boundaries for $targetTitle are ambiguous: $start is not an ancestor of $end.")
        continue
    }
    foreach ($pullRequest in Get-ReleasePullRequests -Root $root -Start $start -End $end) {
        $pull = Get-GitHubIssue -Repository $Repository -Number $pullRequest
        $current = [string] $pull.milestone.title
        if ($current -eq $targetTitle) {
            $correct++
        } else {
            $operations.Add([pscustomobject] @{
                Kind = 'pull-request'
                Number = $pullRequest
                ViaPullRequest = $null
                FromMilestone = $current
                ToMilestone = $targetTitle
                ToMilestoneNumber = [int] $milestones[$targetTitle].number
            })
        }
        foreach ($linked in Get-LinkedIssues -Repository $Repository -PullRequest $pullRequest) {
            $issue = Get-GitHubIssue -Repository $Repository -Number $linked
            $linkedCurrent = [string] $issue.milestone.title
            if ($linkedCurrent -eq $targetTitle) {
                $correct++
            } else {
                $operations.Add([pscustomobject] @{
                    Kind = 'issue'
                    Number = $linked
                    ViaPullRequest = $pullRequest
                    FromMilestone = $linkedCurrent
                    ToMilestone = $targetTitle
                    ToMilestoneNumber = [int] $milestones[$targetTitle].number
                })
            }
        }
    }
}

foreach ($warning in $warnings) {
    Write-Warning $warning
}

# 1.3 Block unsafe writes, otherwise apply each unambiguous assignment.
if ($warnings.Count -gt 0) {
    if ($writeRemote) {
        throw "Reconciliation is blocked by $($warnings.Count) release-boundary or milestone warning(s)."
    }
    Write-ReleaseStatus blocked "Reconciliation has $($warnings.Count) warning(s); no mutation can be applied safely."
    foreach ($item in $operations) {
        $description = "Assign $($item.Kind) #$($item.Number) to $($item.ToMilestone)"
        Set-GitHubItemMilestone `
            -Repository $Repository `
            -Number $item.Number `
            -MilestoneNumber $item.ToMilestoneNumber `
            -MilestoneTitle $item.ToMilestone `
            -Description $description
    }
} else {
    foreach ($item in $operations) {
        $description = "Assign $($item.Kind) #$($item.Number) to $($item.ToMilestone)"
        Set-GitHubItemMilestone `
            -Repository $Repository `
            -Number $item.Number `
            -MilestoneNumber $item.ToMilestoneNumber `
            -MilestoneTitle $item.ToMilestone `
            -Description $description
    }
    Write-ReleaseStatus checked (
        "Reconciliation: $($operations.Count) assignment(s), $correct already correct; " +
        'commits after the final shipped branch were not inspected.')
}

Write-ReleaseStatus complete "Release assignment reconciliation completed ($mode)."
