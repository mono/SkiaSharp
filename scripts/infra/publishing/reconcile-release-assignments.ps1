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

[CmdletBinding()]
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
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
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

# Reads the publication time for every published GitHub Release.
function Get-GitHubPublishedReleaseMap([string] $Repository) {
    $pages = Invoke-GitHubJsonWithRetry -Arguments @(
        'api',
        '--paginate',
        '--slurp',
        "repos/$Repository/releases?per_page=100"
    )
    $result = @{}
    foreach ($release in Expand-GitHubPages $pages) {
        $tag = [string] $release.tag_name
        if (!$release.draft -and $release.published_at -and $tag) {
            $result[$tag] = [datetimeoffset] $release.published_at
        }
    }
    return $result
}

# Combines exact shipped tags with their publication chronology.
function Get-ShippedReleases(
    [object[]] $Branches,
    [string[]] $Tags,
    [hashtable] $PublishedReleases
) {
    $result = foreach ($branch in $Branches) {
        $tag = Get-ShippedTag -Title $branch.Title -Tags $Tags
        if ($tag) {
            [pscustomobject] @{
                Title = $branch.Title
                Tag = $tag
                PublishedAt = if ($PublishedReleases.ContainsKey($tag)) {
                    [datetimeoffset] $PublishedReleases[$tag]
                } else {
                    $null
                }
            }
        }
    }
    return @($result | Sort-Object `
        @{ Expression = { $null -eq $_.PublishedAt }; Ascending = $true },
        PublishedAt,
        Tag)
}

# Extracts every merged pull request through one first-parent release history.
function Get-ReleasePullRequestsThrough([string] $Root, [string] $End) {
    $output = (Invoke-Git -Root $Root -Arguments @(
        'log',
        '--format=%s',
        '--first-parent',
        $End
    )).Output
    $numbers = foreach ($subject in @($output -split "`r?`n")) {
        $match = [regex]::Match($subject, '\(#(?<number>\d+)\)$')
        if ($match.Success) {
            [int] $match.Groups['number'].Value
        }
    }
    return @($numbers | Sort-Object -Unique)
}

# Selects pull requests first seen in one shipment across parallel release histories.
function Get-FirstShippedPullRequests(
    [string] $Root,
    [object[]] $Releases,
    [string] $CurrentTag,
    [datetimeoffset] $CurrentPublishedAt
) {
    $previouslyShipped = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($release in @($Releases | Where-Object PublishedAt)) {
        if ($release.PublishedAt -ge $CurrentPublishedAt) {
            break
        }
        foreach ($pullRequest in Get-ReleasePullRequestsThrough `
            -Root $Root `
            -End "refs/tags/$($release.Tag)") {
            $null = $previouslyShipped.Add($pullRequest)
        }
    }
    $firstShipped = @(
        Get-ReleasePullRequestsThrough -Root $Root -End "refs/tags/$CurrentTag" |
            Where-Object { !$previouslyShipped.Contains($_) }
    )
    $firstShippedSet = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($pullRequest in $firstShipped) {
        $null = $firstShippedSet.Add($pullRequest)
    }
    $ambiguousReleases = foreach ($release in @($Releases | Where-Object {
        !$_.PublishedAt -or
        ($_.Tag -ne $CurrentTag -and $_.PublishedAt -eq $CurrentPublishedAt)
    })) {
        $claimsTargetPullRequest = $false
        foreach ($pullRequest in Get-ReleasePullRequestsThrough `
            -Root $Root `
            -End "refs/tags/$($release.Tag)") {
            if ($firstShippedSet.Contains($pullRequest)) {
                $claimsTargetPullRequest = $true
                break
            }
        }
        if ($claimsTargetPullRequest) {
            [pscustomobject] @{
                Tag = $release.Tag
                Reason = if ($release.PublishedAt) {
                    "has the same published timestamp as $CurrentTag"
                } else {
                    'has no published GitHub Release timestamp'
                }
            }
        }
    }
    return [pscustomobject] @{
        PullRequests = $firstShipped
        Ambiguities = @($ambiguousReleases)
    }
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

# Plans one assignment and blocks forward moves out of earlier closed or shipped milestones.
function Get-ReleaseAssignmentPlan(
    [string] $Kind,
    [int] $Number,
    [object] $ViaPullRequest,
    [string] $CurrentMilestone,
    [string] $TargetMilestone,
    [hashtable] $Milestones,
    [string[]] $Tags,
    [hashtable] $PublishedReleases
) {
    if ($CurrentMilestone -eq $TargetMilestone) {
        return [pscustomobject] @{ Status = 'correct'; Operation = $null; Warning = $null }
    }

    $currentRelease = ConvertTo-ReleaseMilestone $CurrentMilestone
    $targetRelease = ConvertTo-ReleaseMilestone $TargetMilestone
    if ($currentRelease -and $targetRelease) {
        $currentState = if ($Milestones.ContainsKey($CurrentMilestone)) {
            [string] $Milestones[$CurrentMilestone].state
        } else {
            ''
        }
        $currentTag = Get-ShippedTag -Title $currentRelease.Title -Tags $Tags
        if ($currentState -eq 'closed' -or $currentTag) {
            $targetTag = Get-ShippedTag -Title $targetRelease.Title -Tags $Tags
            $targetPublished = if ($targetTag -and $PublishedReleases.ContainsKey($targetTag)) {
                [datetimeoffset] $PublishedReleases[$targetTag]
            } else {
                $null
            }
            $currentPublished = if ($currentTag -and $PublishedReleases.ContainsKey($currentTag)) {
                [datetimeoffset] $PublishedReleases[$currentTag]
            } else {
                $null
            }
            $targetShippedEarlier = $targetPublished -and $currentPublished -and
                $targetPublished -lt $currentPublished
            if ($targetShippedEarlier) {
                return [pscustomobject] @{
                    Status = 'assign'
                    Warning = $null
                    Operation = [pscustomobject] @{
                        Kind = $Kind
                        Number = $Number
                        ViaPullRequest = $ViaPullRequest
                        FromMilestone = $CurrentMilestone
                        ToMilestone = $TargetMilestone
                        ToMilestoneNumber = [int] $Milestones[$TargetMilestone].number
                    }
                }
            }
            $via = if ($ViaPullRequest) { " via pull request #$ViaPullRequest" } else { '' }
            $reason = if ($currentTag) { "shipped as $currentTag" } else { 'is closed' }
            return [pscustomobject] @{
                Status = 'blocked'
                Operation = $null
                Warning = (
                    "Refusing to move $Kind #$Number$via from earlier milestone " +
                    "$CurrentMilestone ($reason) to $TargetMilestone.")
            }
        }
    }

    return [pscustomobject] @{
        Status = 'assign'
        Warning = $null
        Operation = [pscustomobject] @{
            Kind = $Kind
            Number = $Number
            ViaPullRequest = $ViaPullRequest
            FromMilestone = $CurrentMilestone
            ToMilestone = $TargetMilestone
            ToMilestoneNumber = [int] $Milestones[$TargetMilestone].number
        }
    }
}

# Revalidates a planned assignment immediately before a remote mutation.
function Set-PlannedReleaseAssignment(
    [string] $Repository,
    [object] $Item,
    [hashtable] $Milestones,
    [string[]] $Tags,
    [hashtable] $PublishedReleases,
    [switch] $Push
) {
    if ($Push) {
        $live = Get-GitHubIssue -Repository $Repository -Number $Item.Number
        $liveMilestones = $Milestones.Clone()
        $liveMilestoneTitle = [string] $live.milestone.title
        if ($liveMilestoneTitle) {
            $liveMilestones[$liveMilestoneTitle] = $live.milestone
        }
        $livePlan = Get-ReleaseAssignmentPlan `
            -Kind $Item.Kind `
            -Number $Item.Number `
            -ViaPullRequest $Item.ViaPullRequest `
            -CurrentMilestone $liveMilestoneTitle `
            -TargetMilestone $Item.ToMilestone `
            -Milestones $liveMilestones `
            -Tags $Tags `
            -PublishedReleases $PublishedReleases
        if ($livePlan.Status -eq 'correct') {
            Write-ReleaseStatus checked (
                "$($Item.Kind) #$($Item.Number) is already assigned to $($Item.ToMilestone).")
            return
        }
        if ($livePlan.Status -eq 'blocked') {
            throw $livePlan.Warning
        }
        $Item = $livePlan.Operation
    }

    $description = "Assign $($Item.Kind) #$($Item.Number) to $($Item.ToMilestone)"
    Set-GitHubItemMilestone `
        -Repository $Repository `
        -Number $Item.Number `
        -MilestoneNumber $Item.ToMilestoneNumber `
        -MilestoneTitle $Item.ToMilestone `
        -Description $description `
        -Push:$Push
}

# 1. Reconcile shipped commits, pull requests, and linked issues.
Write-ReleaseStatus start "Release assignment reconciliation for $Version ($mode)."

# 1.1 Refresh release refs and identify shipped milestones in release order.
$null = Invoke-Git -Root $root -Arguments @('fetch', 'origin', '--prune', '--tags')
$tags = Get-RemoteReleaseTags -Root $root
$branchSet = Get-ReleaseBranches -Root $root -Version $Version
$branches = @($branchSet.Selected)
$warnings = [System.Collections.Generic.List[string]]::new()
$publishedReleases = Get-GitHubPublishedReleaseMap -Repository $Repository
$shippedReleases = @(Get-ShippedReleases `
    -Branches $branchSet.All `
    -Tags $tags `
    -PublishedReleases $publishedReleases)

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
    $currentRelease = $shippedReleases |
        Where-Object { $_.Tag -eq $currentTag -and $_.PublishedAt } |
        Select-Object -First 1
    if (!$currentRelease) {
        $warnings.Add("Release milestone $targetTitle has no published shipment chronology.")
        continue
    }
    if (!$milestones.ContainsKey($targetTitle)) {
        $warnings.Add("Milestone $targetTitle does not exist.")
        continue
    }
    $shipmentPlan = Get-FirstShippedPullRequests `
        -Root $root `
        -Releases $shippedReleases `
        -CurrentTag $currentTag `
        -CurrentPublishedAt $currentRelease.PublishedAt
    foreach ($ambiguity in $shipmentPlan.Ambiguities) {
        $warnings.Add(
            "Shipped tag $($ambiguity.Tag) $($ambiguity.Reason) and overlaps $currentTag.")
    }
    foreach ($pullRequest in $shipmentPlan.PullRequests) {
        if (!$seenPullRequests.Add($pullRequest)) {
            continue
        }
        $pull = Get-GitHubIssue -Repository $Repository -Number $pullRequest
        $current = [string] $pull.milestone.title
        $pullPlan = Get-ReleaseAssignmentPlan `
            -Kind 'pull-request' `
            -Number $pullRequest `
            -ViaPullRequest $null `
            -CurrentMilestone $current `
            -TargetMilestone $targetTitle `
            -Milestones $milestones `
            -Tags $tags `
            -PublishedReleases $publishedReleases
        if ($pullPlan.Status -eq 'correct') {
            $correct++
        } elseif ($pullPlan.Status -eq 'blocked') {
            $warnings.Add($pullPlan.Warning)
        } else {
            $operations.Add($pullPlan.Operation)
        }
        foreach ($linked in Get-LinkedIssues -Repository $Repository -PullRequest $pullRequest) {
            if (!$seenIssues.Add($linked)) {
                continue
            }
            $issue = Get-GitHubIssue -Repository $Repository -Number $linked
            $linkedCurrent = [string] $issue.milestone.title
            $issuePlan = Get-ReleaseAssignmentPlan `
                -Kind 'issue' `
                -Number $linked `
                -ViaPullRequest $pullRequest `
                -CurrentMilestone $linkedCurrent `
                -TargetMilestone $targetTitle `
                -Milestones $milestones `
                -Tags $tags `
                -PublishedReleases $publishedReleases
            if ($issuePlan.Status -eq 'correct') {
                $correct++
            } elseif ($issuePlan.Status -eq 'blocked') {
                $warnings.Add($issuePlan.Warning)
            } else {
                $operations.Add($issuePlan.Operation)
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
}
foreach ($item in $operations) {
    Set-PlannedReleaseAssignment `
        -Repository $Repository `
        -Item $item `
        -Milestones $milestones `
        -Tags $tags `
        -PublishedReleases $publishedReleases `
        -Push:$Push
}
if ($warnings.Count -eq 0) {
    Write-ReleaseStatus checked (
        "Reconciliation: $($operations.Count) assignment(s), $correct already correct; " +
        'commits after the final shipped branch were not inspected.')
}

Write-ReleaseStatus complete "Release assignment reconciliation completed ($mode)."
