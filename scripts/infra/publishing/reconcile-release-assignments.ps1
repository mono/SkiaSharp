#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reconciles merged pull requests and linked issues to shipped release milestones.

.PARAMETER Version
    One or more released numeric SkiaSharp versions, such as 4.153.0 or
    4.153.0.1.

.PARAMETER Repository
    The GitHub repository whose release assignments are maintained.

.PARAMETER Mode
    DryRun reports planned assignment changes, Push applies them, and Check
    quietly returns whether the calculated plan has work remaining.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string[]] $Version,

    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository = 'mono/SkiaSharp',

    [ValidateSet('DryRun', 'Push', 'Check')]
    [string] $Mode = 'DryRun'
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$writeRemote = $Mode -eq 'Push'
$isCheck = $Mode -eq 'Check'
$mode = $Mode.ToLowerInvariant()
$root = Get-GitRepositoryRoot
$releaseVersions = @(
    foreach ($value in $Version) {
        foreach ($item in $value -split ',') {
            if (![string]::IsNullOrWhiteSpace($item)) {
                $item.Trim()
            }
        }
    }
)
if (!$releaseVersions.Count -or @(
    $releaseVersions | Where-Object { $_ -notmatch '^\d+\.\d+\.\d+(?:\.\d+)?$' }
).Count) {
    throw '-Version must contain one or more numeric release versions.'
}

# Reads one pull request.
function Get-GitHubPullRequest([string] $Repository, [int] $Number) {
    return Invoke-GitHubJsonWithRetry -Arguments @('api', "repos/$Repository/pulls/$Number")
}

# Reads issues that GitHub records as closed by one pull request.
function Get-GitHubClosingIssues([string] $Repository, [int] $PullRequest) {
    $owner, $name = $Repository.Split('/', 2)
    $query = @'
query($owner: String!, $name: String!, $number: Int!, $endCursor: String) {
  repository(owner: $owner, name: $name) {
    pullRequest(number: $number) {
      closingIssuesReferences(first: 100, after: $endCursor) {
        pageInfo { hasNextPage endCursor }
        nodes {
          number
          repository { nameWithOwner }
        }
      }
    }
  }
}
'@
    $data = Invoke-GitHubJsonWithRetry -Arguments @(
        'api', 'graphql',
        '--paginate',
        '--slurp',
        '-f', "query=$query",
        '-F', "owner=$owner",
        '-F', "name=$name",
        '-F', "number=$PullRequest"
    )
    $numbers = foreach ($page in @($data)) {
        foreach ($node in @($page.data.repository.pullRequest.closingIssuesReferences.nodes)) {
            if ([string] $node.repository.nameWithOwner -eq $Repository) {
                [int] $node.number
            }
        }
    }
    return @($numbers | Sort-Object -Unique)
}

# Reads local pull requests that GitHub records as closing one issue.
function Get-GitHubClosingPullRequests([string] $Repository, [int] $Issue) {
    $owner, $name = $Repository.Split('/', 2)
    $query = @'
query($owner: String!, $name: String!, $number: Int!, $endCursor: String) {
  repository(owner: $owner, name: $name) {
    issue(number: $number) {
      closedByPullRequestsReferences(first: 100, after: $endCursor) {
        pageInfo { hasNextPage endCursor }
        nodes {
          number
          repository { nameWithOwner }
        }
      }
    }
  }
}
'@
    $data = Invoke-GitHubJsonWithRetry -Arguments @(
        'api', 'graphql',
        '--paginate',
        '--slurp',
        '-f', "query=$query",
        '-F', "owner=$owner",
        '-F', "name=$name",
        '-F', "number=$Issue"
    )
    $numbers = foreach ($page in @($data)) {
        foreach ($node in @($page.data.repository.issue.closedByPullRequestsReferences.nodes)) {
            if ([string] $node.repository.nameWithOwner -eq $Repository) {
                [int] $node.number
            }
        }
    }
    return @($numbers | Sort-Object -Unique)
}

# Parses every exact shipped tag into its release identity.
function Get-ShippedReleases([string[]] $Tags) {
    $result = foreach ($tag in $Tags) {
        $match = [regex]::Match(
            $tag,
            '^v(?<title>\d+\.\d+\.\d+(?:\.\d+)?-(?:preview|rc)\.\d+)\.\d+(?:\.\d+)?$')
        if (!$match.Success) {
            $match = [regex]::Match($tag, '^v(?<title>\d+\.\d+\.\d+(?:\.\d+)?)$')
        }
        if (!$match.Success) {
            continue
        }
        $release = ConvertTo-ReleaseMilestone $match.Groups['title'].Value
        if ($release) {
            [pscustomobject] @{
                Title = $release.Title
                Tag = $tag
                NumericKey = $release.NumericKey
                SortKey = $release.SortKey
            }
        }
    }
    return @($result | Sort-Object SortKey, Tag)
}

# Combines immutable shipped identities and extant release branches for one numeric line.
function Get-ReleaseMilestones(
    [string] $Root,
    [string] $Version,
    [object[]] $ShippedReleases
) {
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
    $byTitle = @{}
    foreach ($release in @($all) + @($ShippedReleases)) {
        if (!$byTitle.ContainsKey($release.Title)) {
            $byTitle[$release.Title] = ConvertTo-ReleaseMilestone $release.Title
        }
    }
    $selected = @($byTitle.Values | Where-Object {
        $_.Title -eq $Version -or $_.Title.StartsWith("$Version-") -or $_.Title.StartsWith("$Version.")
    } | Sort-Object SortKey)
    if ($selected.Count -eq 0) {
        throw "No release branches or shipped tags match $Version."
    }
    return $selected
}

# Refreshes remote release tags immediately before a write.
function Get-CurrentRemoteReleaseTags([string] $Root) {
    return Get-RemoteReleaseTags -Root $Root
}

# Tests whether two remote tag snapshots contain the same release tags.
function Test-ReleaseTagsEqual([string[]] $Left, [string[]] $Right) {
    return @(Compare-Object @($Left) @($Right)).Count -eq 0
}

# Finds the nearest branch commit shared with a semantically earlier shipped release.
function Get-PreviousShippedBoundary(
    [string] $Root,
    [object[]] $Releases,
    [object] $CurrentRelease
) {
    $end = (Invoke-Git -Root $Root -Arguments @(
        'rev-parse',
        "refs/tags/$($CurrentRelease.Tag)`^{commit}"
    )).Output
    $candidates = foreach ($release in $Releases) {
        if ($release.SortKey -ge $CurrentRelease.SortKey) {
            continue
        }
        $start = (Invoke-Git -Root $Root -Arguments @(
            'merge-base',
            "refs/tags/$($release.Tag)",
            "refs/tags/$($CurrentRelease.Tag)"
        )).Output
        if (!$start) {
            continue
        }
        $distance = (Invoke-Git -Root $Root -Arguments @(
            'rev-list',
            '--count',
            '--first-parent',
            "$start..$end"
        )).Output
        [pscustomobject] @{
            Title = $release.Title
            Tag = $release.Tag
            Start = $start
            End = $end
            Distance = [long] $distance
            SortKey = $release.SortKey
        }
    }
    return $candidates |
        Sort-Object `
            @{ Expression = 'Distance'; Ascending = $true },
            @{ Expression = 'SortKey'; Descending = $true },
            @{ Expression = 'Tag'; Descending = $true } |
        Select-Object -First 1
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

# Extracts pull requests reachable from this tag but no semantically earlier shipped tag.
function Get-ReleasePullRequests(
    [string] $Root,
    [object[]] $Releases,
    [object] $CurrentRelease
) {
    $arguments = @(
        'log',
        '--format=%s',
        "refs/tags/$($CurrentRelease.Tag)"
    )
    $earlier = @($Releases | Where-Object SortKey -lt $CurrentRelease.SortKey)
    if ($earlier.Count -gt 0) {
        $arguments += '--not'
        $arguments += @($earlier | ForEach-Object { "refs/tags/$($_.Tag)" })
    }
    $output = (Invoke-Git -Root $Root -Arguments $arguments).Output
    $numbers = foreach ($subject in @($output -split "`r?`n")) {
        $match = [regex]::Match($subject, '\(#(?<number>\d+)\)$')
        if (!$match.Success) {
            $match = [regex]::Match($subject, '^Merge pull request #(?<number>\d+)\b')
        }
        if ($match.Success) {
            [int] $match.Groups['number'].Value
        }
    }
    return @($numbers | Sort-Object -Unique)
}

# Maps every shipped pull request to its lowest semantic release identity.
function Get-ReleasePullRequestOwners([string] $Root, [object[]] $Releases) {
    $result = @{}
    $seenIdentities = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($release in @($Releases | Sort-Object SortKey, Tag)) {
        if (!$seenIdentities.Add($release.Title)) {
            continue
        }
        $identityReleases = @($Releases |
            Where-Object Title -eq $release.Title |
            Sort-Object SortKey, Tag)
        $ownerRelease = $identityReleases | Select-Object -First 1
        foreach ($exactRelease in $identityReleases) {
            foreach ($pullRequest in Get-ReleasePullRequests `
                -Root $Root `
                -Releases $Releases `
                -CurrentRelease $exactRelease) {
                if (!$result.ContainsKey($pullRequest)) {
                    $result[$pullRequest] = $ownerRelease
                }
            }
        }
    }
    return $result
}

# Selects the lowest semantic release among all shipped PRs that close one issue.
function Get-LinkedIssueOwner(
    [string] $Repository,
    [int] $Issue,
    [int] $ViaPullRequest,
    [hashtable] $PullRequestOwners
) {
    $closingPullRequests = [System.Collections.Generic.HashSet[int]]::new()
    $null = $closingPullRequests.Add($ViaPullRequest)
    foreach ($pullRequest in Get-GitHubClosingPullRequests -Repository $Repository -Issue $Issue) {
        $null = $closingPullRequests.Add($pullRequest)
    }
    return @(
        $closingPullRequests |
            Where-Object { $PullRequestOwners.ContainsKey($_) } |
            ForEach-Object { $PullRequestOwners[$_] } |
            Sort-Object SortKey, Tag
    ) | Select-Object -First 1
}

# Re-fetches changed tags and verifies that a target still owns one item.
function Test-LiveReleaseAssignmentOwnership(
    [string] $Root,
    [string] $Repository,
    [string[]] $Tags,
    [string] $TargetMilestone,
    [string] $Kind,
    [int] $Number,
    [object] $ViaPullRequest
) {
    $null = Invoke-Git -Root $Root -Arguments @('fetch', 'origin', '--prune', '--tags')
    $releases = @(Get-ShippedReleases -Tags $Tags)
    $pullRequestOwners = Get-ReleasePullRequestOwners -Root $Root -Releases $releases
    if ($Kind -eq 'issue') {
        $owner = Get-LinkedIssueOwner `
            -Repository $Repository `
            -Issue $Number `
            -ViaPullRequest ([int] $ViaPullRequest) `
            -PullRequestOwners $pullRequestOwners
        return $owner -and $owner.Title -eq $TargetMilestone
    }
    return (
        $pullRequestOwners.ContainsKey($Number) -and
        $pullRequestOwners[$Number].Title -eq $TargetMilestone
    )
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
    [string[]] $Tags
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
        if (
            $currentRelease.SortKey -lt $targetRelease.SortKey -and
            ($currentState -eq 'closed' -or $currentTag)
        ) {
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
            FromMilestoneNumber = if ($Milestones.ContainsKey($CurrentMilestone)) {
                [int] $Milestones[$CurrentMilestone].number
            } else {
                $null
            }
            ToMilestone = $TargetMilestone
            ToMilestoneNumber = [int] $Milestones[$TargetMilestone].number
        }
    }
}

# Restores the live assignment that was observed immediately before a mutation.
function Restore-PlannedReleaseAssignment([string] $Repository, [object] $Item) {
    $description = "Restore $($Item.Kind) #$($Item.Number)"
    if ($Item.FromMilestoneNumber) {
        Set-GitHubItemMilestone `
            -Repository $Repository `
            -Number $Item.Number `
            -MilestoneNumber $Item.FromMilestoneNumber `
            -MilestoneTitle $Item.FromMilestone `
            -Description "$description to $($Item.FromMilestone)" `
            -Push
        return
    }

    $null = Invoke-GitHubMutation `
        -Arguments @(
            'api',
            "repos/$Repository/issues/$($Item.Number)",
            '-X',
            'PATCH',
            '-F',
            'milestone=null'
        ) `
        -Description "$description to no milestone" `
        -Push
    $actual = Get-GitHubIssue -Repository $Repository -Number $Item.Number
    if ($actual.milestone) {
        throw "GitHub item #$($Item.Number) milestone restoration could not be verified."
    }
    Write-ReleaseStatus applied "$description to no milestone verified."
}

# Revalidates a planned assignment immediately before a remote mutation.
function Set-PlannedReleaseAssignment(
    [string] $Root,
    [string] $Repository,
    [object] $Item,
    [hashtable] $Milestones,
    [string[]] $PlanningTags,
    [switch] $Push
) {
    $ownershipPullRequest = if ($Item.ViaPullRequest) { [int] $Item.ViaPullRequest } else { [int] $Item.Number }
    $ownershipDescription = if ($Item.Kind -eq 'issue') {
        "issue #$($Item.Number) across its shipped closing pull requests"
    } else {
        "pull request #$ownershipPullRequest"
    }
    $liveTags = $PlanningTags
    if ($Push) {
        $live = Get-GitHubIssue -Repository $Repository -Number $Item.Number
        $liveTags = Get-CurrentRemoteReleaseTags -Root $Root
        if (
            !(Test-ReleaseTagsEqual -Left $PlanningTags -Right $liveTags) -and
            !(Test-LiveReleaseAssignmentOwnership `
                -Root $Root `
                -Repository $Repository `
                -Tags $liveTags `
                -TargetMilestone $Item.ToMilestone `
                -Kind $Item.Kind `
                -Number $Item.Number `
                -ViaPullRequest $Item.ViaPullRequest)
        ) {
            throw (
                "Release ownership changed after planning: $($Item.ToMilestone) no longer owns " +
                "$ownershipDescription.")
        }
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
            -Tags $liveTags
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

    $sourceRelease = ConvertTo-ReleaseMilestone $Item.FromMilestone
    $targetRelease = ConvertTo-ReleaseMilestone $Item.ToMilestone
    $postWriteTags = if ($Push) { Get-CurrentRemoteReleaseTags -Root $Root } else { $liveTags }
    if (
        $Push -and
        !(Test-ReleaseTagsEqual -Left $liveTags -Right $postWriteTags) -and
        !(Test-LiveReleaseAssignmentOwnership `
            -Root $Root `
            -Repository $Repository `
            -Tags $postWriteTags `
            -TargetMilestone $Item.ToMilestone `
            -Kind $Item.Kind `
            -Number $Item.Number `
            -ViaPullRequest $Item.ViaPullRequest)
    ) {
        $postWriteItem = Get-GitHubIssue -Repository $Repository -Number $Item.Number
        if ([string] $postWriteItem.milestone.title -ne $Item.ToMilestone) {
            throw (
                "Release ownership changed during mutation, but $($Item.Kind) #$($Item.Number) " +
                'also changed again; no automatic restoration was attempted.')
        }
        Restore-PlannedReleaseAssignment -Repository $Repository -Item $Item
        throw (
            "Release ownership changed during mutation: $($Item.ToMilestone) no longer owns " +
            "$ownershipDescription. The assignment was restored.")
    }
    if ($Push -and $sourceRelease -and $targetRelease -and $sourceRelease.SortKey -lt $targetRelease.SortKey) {
        $postWriteMilestones = Get-GitHubMilestoneMap -Repository $Repository
        $postWritePlan = Get-ReleaseAssignmentPlan `
            -Kind $Item.Kind `
            -Number $Item.Number `
            -ViaPullRequest $Item.ViaPullRequest `
            -CurrentMilestone $Item.FromMilestone `
            -TargetMilestone $Item.ToMilestone `
            -Milestones $postWriteMilestones `
            -Tags $postWriteTags
        if ($postWritePlan.Status -eq 'blocked') {
            $postWriteItem = Get-GitHubIssue -Repository $Repository -Number $Item.Number
            if ([string] $postWriteItem.milestone.title -ne $Item.ToMilestone) {
                throw (
                    "$($postWritePlan.Warning) The item changed again after mutation; " +
                    'no automatic restoration was attempted.')
            }
            Restore-PlannedReleaseAssignment -Repository $Repository -Item $Item
            throw "$($postWritePlan.Warning) The concurrent change was detected after mutation and the assignment was restored."
        }
    }
}

# 1. Reconcile shipped commits, pull requests, and linked issues.
if (!$isCheck) {
    Write-ReleaseStatus start "Release assignment reconciliation for $($releaseVersions -join ', ') ($mode)."
}

# 1.1 Refresh release refs and identify shipped milestones in release order.
$null = Invoke-Git -Root $root -Arguments @('fetch', 'origin', '--prune', '--tags')
$tags = Get-RemoteReleaseTags -Root $root
$warnings = [System.Collections.Generic.List[string]]::new()
$shippedReleases = @(Get-ShippedReleases -Tags $tags)
$pullRequestOwners = Get-ReleasePullRequestOwners -Root $root -Releases $shippedReleases
# 1.2 Roll unshipped milestones forward and inspect each shipped tag range once.
$effectiveTitles = [System.Collections.Generic.List[string]]::new()
foreach ($releaseVersion in $releaseVersions) {
    $branches = @(Get-ReleaseMilestones `
        -Root $root `
        -Version $releaseVersion `
        -ShippedReleases $shippedReleases)
    foreach ($title in @(Get-EffectiveMilestoneTitles -Branches $branches -Tags $tags)) {
        if ($title) {
            $effectiveTitles.Add($title)
        }
    }
}
$targetTitles = @($effectiveTitles | Select-Object -Unique)
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
    $currentRelease = $shippedReleases | Where-Object Tag -eq $currentTag | Select-Object -First 1
    if (!$currentRelease) {
        $warnings.Add("Release milestone $targetTitle has no shipped release identity.")
        continue
    }
    if (!$milestones.ContainsKey($targetTitle)) {
        $warnings.Add("Milestone $targetTitle does not exist.")
        continue
    }
    $boundary = Get-PreviousShippedBoundary `
        -Root $root `
        -Releases $shippedReleases `
        -CurrentRelease $currentRelease
    if (!$boundary) {
        $warnings.Add("Release boundary before $targetTitle is missing.")
        continue
    }
    foreach ($pullRequest in @(
        $pullRequestOwners.Keys |
            Where-Object { $pullRequestOwners[$_].Title -eq $targetTitle } |
            Sort-Object
    )) {
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
            -Tags $tags
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
            $issueOwner = Get-LinkedIssueOwner `
                -Repository $Repository `
                -Issue $linked `
                -ViaPullRequest $pullRequest `
                -PullRequestOwners $pullRequestOwners
            if (!$issueOwner -or $issueOwner.Title -ne $targetTitle) {
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
                -Tags $tags
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

if ($isCheck) {
    if ($warnings.Count -eq 0 -and $operations.Count -eq 0) {
        exit 0
    }
    Write-Output "Release assignments: $($operations.Count) pending, $($warnings.Count) warning(s)."
    foreach ($warning in $warnings) {
        Write-Output $warning
    }
    foreach ($operation in $operations) {
        Write-Output "$($operation.Kind) #$($operation.Number): $($operation.FromMilestone) -> $($operation.ToMilestone)"
    }
    exit 1
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
        -Root $root `
        -Repository $Repository `
        -Item $item `
        -Milestones $milestones `
        -PlanningTags $tags `
        -Push:$writeRemote
}
if ($warnings.Count -eq 0) {
    Write-ReleaseStatus checked (
        "Reconciliation: $($operations.Count) assignment(s), $correct already correct; " +
        'commits not reachable from shipped tags were not inspected.')
}

Write-ReleaseStatus complete "Release assignment reconciliation completed ($mode)."
