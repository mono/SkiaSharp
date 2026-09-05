#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reconciles merged pull requests and linked issues to shipped release milestones.

.PARAMETER Version
    The released numeric SkiaSharp version, such as 4.153.0 or 4.153.0.1.

.PARAMETER Repository
    The GitHub repository whose release assignments are maintained. Defaults to
    the runtime or configured repository identity.

.PARAMETER Push
    Performs GitHub milestone assignments. Without this switch, the script is
    read-only and reports exact skipped mutations.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:\.\d+)?$')]
    [string] $Version,

    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository,

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$Repository = Resolve-PublishingRepository $Repository
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }
$root = Get-GitRepositoryRoot

# Reads one pull request.
function Get-GitHubPullRequest([string] $Repository, [int] $Number) {
    return Invoke-GitHubJsonWithRetry -Arguments @('api', "repos/$Repository/pulls/$Number")
}

# Reads issues that GitHub records as closed by one pull request.
function Get-GitHubClosingIssues([string] $Repository, [int] $PullRequest) {
    $owner, $name = $Repository.Split('/', 2)
    $query = @'
query($owner: String!, $name: String!, $number: Int!) {
  repository(owner: $owner, name: $name) {
    pullRequest(number: $number) {
      closingIssuesReferences(first: 50) {
        nodes { number }
      }
    }
  }
}
'@
    $data = Invoke-GitHubJsonWithRetry -Arguments @(
        'api', 'graphql',
        '-f', "query=$query",
        '-F', "owner=$owner",
        '-F', "name=$name",
        '-F', "number=$PullRequest"
    )
    $nodes = @($data.data.repository.pullRequest.closingIssuesReferences.nodes)
    return @($nodes | ForEach-Object { [int] $_.number })
}

# Enumerates release branches and selects those in one numeric release line.
function Get-ReleaseBranches([string] $Root, [string] $Version) {
    $output = (Invoke-Git -Root $Root -Arguments @(
        'for-each-ref',
        '--format=%(refname:strip=3)',
        'refs/remotes/origin/release/'
    )).Output
    $all = @(
        foreach ($line in @($output -split "`r?`n")) {
            if ($line) {
                $parsed = ConvertTo-ReleaseMilestone $line
                if ($parsed) {
                    $parsed
                }
            }
        }
    )
    $selected = @($all | Where-Object {
        $_.Title -eq $Version -or $_.Title.StartsWith("$Version-") -or $_.Title.StartsWith("$Version.")
    } | Sort-Object SortKey)
    if ($selected.Count -eq 0) {
        throw "No release branches match $Version."
    }
    return [pscustomobject] @{ Selected = $selected; All = $all }
}

# Finds the latest shipped stable branch before the requested numeric line.
function Get-PreviousStableBranch([object[]] $Branches, [string] $Version, [string[]] $Tags) {
    $target = ConvertTo-ReleaseMilestone $Version
    $candidates = $Branches |
        Where-Object { !$_.Channel -and $_.NumericKey -lt $target.NumericKey } |
        Sort-Object NumericKey -Descending
    foreach ($candidate in $candidates) {
        if (Get-ShippedTag -Title $candidate.Title -Tags $Tags) {
            return $candidate
        }
    }
    return $null
}

# Rolls each unshipped branch forward to the next branch that was shipped.
function Get-EffectiveMilestoneTitles([object[]] $Branches, [string[]] $Tags) {
    $result = [System.Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $Branches.Count; $index++) {
        $effective = $null
        for ($candidate = $index; $candidate -lt $Branches.Count; $candidate++) {
            if (Get-ShippedTag -Title $Branches[$candidate].Title -Tags $Tags) {
                $effective = $Branches[$candidate].Title
                break
            }
        }
        $result.Add($effective)
    }
    return $result.ToArray()
}

# Extracts merged pull request numbers from one first-parent release range.
function Get-ReleasePullRequests([string] $Root, [string] $Start, [string] $End) {
    $output = (Invoke-Git -Root $Root -Arguments @(
        'log',
        '--format=%s',
        '--first-parent',
        "$Start..$End"
    )).Output
    $numbers = foreach ($subject in @($output -split "`r?`n")) {
        $match = [regex]::Match($subject, '\(#(?<number>\d+)\)$')
        if ($match.Success) {
            [int] $match.Groups['number'].Value
        }
    }
    return @($numbers | Sort-Object -Unique)
}

# Combines GitHub closing references with closing keywords in the pull request body.
function Get-LinkedIssues([string] $Repository, [int] $PullRequest) {
    $numbers = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($number in Get-GitHubClosingIssues -Repository $Repository -PullRequest $PullRequest) {
        $null = $numbers.Add($number)
    }
    $pull = Get-GitHubPullRequest -Repository $Repository -Number $PullRequest
    $pattern = '(?i)(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\s*:?\s+#(?<number>\d+)'
    foreach ($match in [regex]::Matches([string] $pull.body, $pattern)) {
        $null = $numbers.Add([int] $match.Groups['number'].Value)
    }
    return @($numbers | Sort-Object)
}

# 1. Reconcile shipped commits, pull requests, and linked issues.
Write-ReleaseStatus start "Release assignment reconciliation for $Version ($mode)."

# 1.1 Refresh release refs and identify shipped milestones in release order.
$null = Invoke-Git -Root $root -Arguments @('fetch', 'origin', '--prune', '--tags')
$tags = Get-RemoteReleaseTags -Root $root
$branchSet = Get-ReleaseBranches -Root $root -Version $Version
$branches = @($branchSet.Selected)
$previous = Get-PreviousStableBranch -Branches $branchSet.All -Version $Version -Tags $tags
$warnings = [System.Collections.Generic.List[string]]::new()
$previousTag = if ($previous) { Get-ShippedTag -Title $previous.Title -Tags $tags } else { $null }
if (!$previous) {
    $warnings.Add("No previous stable release boundary exists for $Version.")
} elseif (!$previousTag) {
    $warnings.Add("Previous stable release $($previous.Title) has no exact shipped tag.")
}

# 1.2 Roll unshipped milestones forward and inspect each shipped tag range once.
$effective = @(Get-EffectiveMilestoneTitles -Branches $branches -Tags $tags)
$targetTitles = @($effective | Where-Object { $_ } | Select-Object -Unique)
$milestones = Get-GitHubMilestoneMap -Repository $Repository
$operations = [System.Collections.Generic.List[object]]::new()
$seenPullRequests = [System.Collections.Generic.HashSet[int]]::new()
$seenIssues = [System.Collections.Generic.HashSet[int]]::new()
$correct = 0
foreach ($targetTitle in $targetTitles) {
    $currentTag = Get-ShippedTag -Title $targetTitle -Tags $tags
    if (!$currentTag) {
        $warnings.Add("Release milestone $targetTitle has no exact shipped tag.")
        continue
    }
    if (!$milestones.ContainsKey($targetTitle)) {
        $warnings.Add("Milestone $targetTitle does not exist.")
        $previousTag = $currentTag
        continue
    }
    if (!$previousTag) {
        $warnings.Add("Release boundary before $targetTitle is missing.")
        $previousTag = $currentTag
        continue
    }
    $start = (Invoke-Git -Root $root -Arguments @(
        'merge-base',
        "refs/tags/$previousTag",
        "refs/tags/$currentTag"
    )).Output
    $end = (Invoke-Git -Root $root -Arguments @('rev-parse', "refs/tags/$currentTag`^{commit}")).Output
    if (!$start -or !$end) {
        $warnings.Add("Release boundaries from $previousTag to $currentTag are missing.")
        $previousTag = $currentTag
        continue
    }
    foreach ($pullRequest in Get-ReleasePullRequests -Root $root -Start $start -End $end) {
        if (!$seenPullRequests.Add($pullRequest)) {
            continue
        }
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
            if (!$seenIssues.Add($linked)) {
                continue
            }
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
    $previousTag = $currentTag
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
}
foreach ($item in $operations) {
    $description = "Assign $($item.Kind) #$($item.Number) to $($item.ToMilestone)"
    Set-GitHubItemMilestone `
        -Repository $Repository `
        -Number $item.Number `
        -MilestoneNumber $item.ToMilestoneNumber `
        -MilestoneTitle $item.ToMilestone `
        -Description $description `
        -Push:$Push
}
if ($warnings.Count -eq 0) {
    Write-ReleaseStatus checked (
        "Reconciliation: $($operations.Count) assignment(s), $correct already correct; " +
        'commits after the final shipped branch were not inspected.')
}

Write-ReleaseStatus complete "Release assignment reconciliation completed ($mode)."
