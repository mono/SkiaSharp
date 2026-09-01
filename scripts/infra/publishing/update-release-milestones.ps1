#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reconciles shipped release assignments and advances SkiaSharp release milestones.

.PARAMETER Version
    The released numeric SkiaSharp version, such as 4.153.0 or 4.153.0.1.

.PARAMETER Count
    The number of Chromium milestones whose release milestones are maintained.

.PARAMETER Repository
    The GitHub repository whose milestones are maintained.

.PARAMETER Push
    Performs remote mutations. Without this switch, all checks run and exact mutation commands are logged as skipped.
#>

param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:\.\d+)?$')]
    [string] $Version,

    [ValidateRange(1, 20)]
    [int] $Count = 3,

    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository = 'mono/SkiaSharp',

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and milestone settings.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }

$script:ScheduleUrl = 'https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={0}'
$script:RequiredScheduleFields = @('branch_point', 'earliest_beta', 'early_stable_cut', 'early_stable', 'stable_cut',
    'stable_date')
$script:MoveSettleAttempts = 5
$script:MoveSettleDelaySeconds = 2

# Reads all repository milestones and rejects duplicate titles.
function Get-GitHubMilestoneMap([string] $Repository) {
    $pages = Invoke-GitHubJsonWithRetry @('api', '--paginate', '--slurp',
        "repos/$Repository/milestones?state=all&per_page=100")
    $result = @{}
    foreach ($milestone in Expand-GitHubPages $pages) {
        $title = [string] $milestone.title
        if ($result.ContainsKey($title)) {
            throw "Multiple milestones are named $title."
        }
        $result[$title] = $milestone
    }
    return $result
}

# Reads one issue or pull request through the issues endpoint.
function Get-GitHubIssue([string] $Repository, [int] $Number) {
    return Invoke-GitHubJsonWithRetry @('api', "repos/$Repository/issues/$Number")
}

# Reads one pull request.
function Get-GitHubPullRequest([string] $Repository, [int] $Number) {
    return Invoke-GitHubJsonWithRetry @('api', "repos/$Repository/pulls/$Number")
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
    $data = Invoke-GitHubJsonWithRetry @('api', 'graphql', '-f', "query=$query", '-F', "owner=$owner", '-F',
        "name=$name", '-F', "number=$PullRequest")
    $nodes = @($data.data.repository.pullRequest.closingIssuesReferences.nodes)
    return @($nodes | ForEach-Object { [int] $_.number })
}

# Reads all open issues and pull requests assigned to one milestone.
function Get-OpenMilestoneItems([string] $Repository, [int] $MilestoneNumber) {
    $pages = Invoke-GitHubJsonWithRetry @('api', '--paginate', '--slurp',
        "repos/$Repository/issues?milestone=$MilestoneNumber&state=open&per_page=100")
    return @(Expand-GitHubPages $pages | ForEach-Object {
        [pscustomobject] @{
            Number = [int] $_.number
            Title = [string] $_.title
            Url = [string] $_.html_url
            Kind = if ($_.PSObject.Properties['pull_request']) { 'pull-request' } else { 'issue' }
        }
    })
}

# Parses a release branch or milestone title into its shipping-order identity.
function ConvertTo-ReleaseMilestone([string] $Value) {
    $match = [regex]::Match($Value,
        '^(?:release/)?(?<numeric>\d+\.\d+\.\d+(?:\.\d+)?)(?:-(?<channel>preview|rc)\.(?<iteration>\d+))?$')
    if (!$match.Success) {
        return $null
    }
    $numericText = $match.Groups['numeric'].Value
    $parts = @($numericText.Split('.') | ForEach-Object { [int] $_ })
    $hotfix = if ($parts.Count -eq 4) { $parts[3] } else { 0 }
    $channel = if ($match.Groups['channel'].Success) { $match.Groups['channel'].Value } else { $null }
    $iteration = if ($match.Groups['iteration'].Success) { [int] $match.Groups['iteration'].Value } else { 0 }
    $channelRank = switch ($channel) {
        'preview' { 0 }
        'rc' { 1 }
        default { 2 }
    }
    $title = $numericText
    if ($channel) {
        $title += "-$channel.$iteration"
    }
    return [pscustomobject] @{
        Name = if ($Value.StartsWith('release/')) { $Value } else { "release/$title" }
        Title = $title
        Numeric = $parts
        NumericKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}' -f $parts[0], $parts[1], $parts[2], $hotfix
        Channel = $channel
        Iteration = $iteration
        SortKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}.{4:D2}.{5:D10}' -f
            $parts[0], $parts[1], $parts[2], $hotfix, $channelRank, $iteration
    }
}

# Selects the greatest exact NuGet tag that shipped a milestone.
function Get-ShippedTag([string] $Title, [string[]] $Tags) {
    if ($Title -match '-(?:preview|rc)\.') {
        $pattern = '^v' + [regex]::Escape($Title) + '\.(?<first>\d+)(?:\.(?<second>\d+))?$'
        $tagMatches = foreach ($tag in $Tags) {
            $match = [regex]::Match($tag, $pattern)
            if ($match.Success) {
                [pscustomobject] @{
                    Tag = $tag
                    First = [long] $match.Groups['first'].Value
                    Second = if ($match.Groups['second'].Success) {
                        [long] $match.Groups['second'].Value
                    } else {
                        [long] -1
                    }
                }
            }
        }
        return ($tagMatches | Sort-Object First, Second -Descending | Select-Object -First 1).Tag
    }
    $exact = "v$Title"
    if ($Tags -contains $exact) {
        return $exact
    }
    return $null
}

# Reads non-peeled remote tags, optionally narrowed to a numeric release line.
function Get-RemoteReleaseTags([string] $Root, [string] $NumericVersion = '') {
    $pattern = if ($NumericVersion) { "refs/tags/v$NumericVersion*" } else { 'refs/tags/v*' }
    $output = (Invoke-GitCommand -Root $Root -Arguments @('ls-remote', '--tags', 'origin', $pattern)).Output
    $tags = foreach ($line in @($output -split "`r?`n")) {
        if ($line -and $line -match '^[^\s]+\s+refs/tags/(?<tag>.+)$' -and !$Matches.tag.EndsWith('^{}')) {
            $Matches.tag
        }
    }
    return @($tags | Sort-Object -Unique)
}

# Enumerates all release branches and selects those in one numeric release line.
function Get-ReleaseBranches([string] $Root, [string] $Version) {
    $output = (Invoke-GitCommand -Root $Root -Arguments @('for-each-ref', '--format=%(refname:strip=3)',
        'refs/remotes/origin/release/')).Output
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

# Finds the latest stable release branch before the requested numeric line.
function Get-PreviousStableBranch([object[]] $Branches, [string] $Version) {
    $target = ConvertTo-ReleaseMilestone $Version
    return $Branches |
        Where-Object { !$_.Channel -and $_.NumericKey -lt $target.NumericKey } |
        Sort-Object NumericKey -Descending |
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

# Extracts merged pull request numbers from one first-parent release range.
function Get-ReleasePullRequests([string] $Root, [string] $Start, [string] $End) {
    $output = (Invoke-GitCommand -Root $Root -Arguments @('log', '--format=%s', '--first-parent', "$Start..$End")).Output
    $numbers = foreach ($subject in @($output -split "`r?`n")) {
        $match = [regex]::Match($subject, '\(#(?<number>\d+)\)')
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

# Assigns one issue or pull request and verifies the resulting milestone.
function Set-GitHubItemMilestone([string] $Repository, [int] $Number, [int] $MilestoneNumber,
    [string] $MilestoneTitle, [string] $Description) {
    $arguments = @('api', "repos/$Repository/issues/$Number", '-X', 'PATCH', '-F', "milestone=$MilestoneNumber")
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $Description -Push:$writeRemote
    if ($writeRemote) {
        $actual = Get-GitHubIssue -Repository $Repository -Number $Number
        if ([string] $actual.milestone.title -ne $MilestoneTitle) {
            throw "GitHub item #$Number milestone update could not be verified."
        }
        Write-ReleaseStatus applied "$Description verified."
    }
}

# Builds and applies reconciliation assignments for a shipped numeric release line.
function Invoke-ReleaseAssignmentReconciliation([string] $Root, [string] $Version, [string] $Repository) {
    Write-ReleaseStatus checking "Reconciling assignments for $Version."

    # Refresh release refs and calculate the exact shipped branch/tag boundaries.
    $null = Invoke-GitCommand -Root $Root -Arguments @('fetch', 'origin', '--prune')
    $tags = Get-RemoteReleaseTags -Root $Root -NumericVersion $Version
    $branchSet = Get-ReleaseBranches -Root $Root -Version $Version
    $branches = @($branchSet.Selected)
    $previous = Get-PreviousStableBranch -Branches $branchSet.All -Version $Version
    $warnings = [System.Collections.Generic.List[string]]::new()
    if (!$previous) {
        $warnings.Add("No previous stable release boundary exists for $Version.")
    }

    $mergeBases = @{}
    foreach ($branch in $branches) {
        $mergeBases[$branch.Name] = (Invoke-GitCommand -Root $Root -Arguments @('merge-base', 'origin/main',
            "origin/$($branch.Name)")).Output
        if (!$mergeBases[$branch.Name]) {
            $warnings.Add("No merge-base exists for $($branch.Name).")
        }
    }
    $previousBase = if ($previous) {
        (Invoke-GitCommand -Root $Root -Arguments @('merge-base', 'origin/main', "origin/$($previous.Name)")).Output
    } else {
        $null
    }
    if ($previous -and !$previousBase) {
        $warnings.Add("No merge-base exists for previous boundary $($previous.Name).")
    }

    # Roll unshipped branch milestones forward and collect required assignments.
    $effective = @(Get-EffectiveMilestoneTitles -Branches $branches -Tags $tags)
    for ($index = 1; $index -lt $branches.Count; $index++) {
        $previousEffective = $effective[$index - 1]
        $currentEffective = $effective[$index]
        if ($previousEffective -and $currentEffective -and $previousEffective -ne $currentEffective -and
            $mergeBases[$branches[$index - 1].Name] -eq $mergeBases[$branches[$index].Name]) {
            $warnings.Add(
                "Release boundaries for $previousEffective and $currentEffective resolve to the same commit."
            )
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
        $ancestor = Invoke-GitCommand -Root $Root -Arguments @('merge-base', '--is-ancestor', $start, $end) -AllowFailure
        if ($ancestor.ExitCode -ne 0) {
            $warnings.Add("Release boundaries for $targetTitle are ambiguous: $start is not an ancestor of $end.")
            continue
        }
        foreach ($pullRequest in Get-ReleasePullRequests -Root $Root -Start $start -End $end) {
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

    # Block unsafe remote writes but still show every planned dry-run mutation.
    if ($warnings.Count -gt 0) {
        if ($writeRemote) {
            throw "Reconciliation is blocked by $($warnings.Count) release-boundary or milestone warning(s)."
        }
        Write-ReleaseStatus blocked "Reconciliation has $($warnings.Count) warning(s); no mutation can be applied safely."
        foreach ($item in $operations) {
            $description = "Assign $($item.Kind) #$($item.Number) to $($item.ToMilestone)"
            Set-GitHubItemMilestone -Repository $Repository -Number $item.Number -MilestoneNumber $item.ToMilestoneNumber `
                -MilestoneTitle $item.ToMilestone -Description $description
        }
        return
    }

    # Apply and verify each unambiguous pull-request or issue assignment.
    foreach ($item in $operations) {
        $description = "Assign $($item.Kind) #$($item.Number) to $($item.ToMilestone)"
        Set-GitHubItemMilestone -Repository $Repository -Number $item.Number -MilestoneNumber $item.ToMilestoneNumber `
            -MilestoneTitle $item.ToMilestone -Description $description
    }
    Write-ReleaseStatus checked (
        "Reconciliation: $($operations.Count) assignment(s), $correct already correct; " +
        'commits after the final shipped branch were not inspected.'
    )
}

# Converts a Chromium timestamp to a UTC date.
function ConvertTo-ScheduleDate([string] $Value) {
    return [DateTimeOffset]::Parse($Value, [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::AssumeUniversal).UtcDateTime.Date
}

# Formats a schedule date for milestone descriptions.
function Format-ScheduleDate([datetime] $Date) {
    return $Date.ToString('ddd, MMM dd, yyyy', [Globalization.CultureInfo]::InvariantCulture)
}

# Normalizes GitHub string or DateTime values to an ISO calendar date.
function ConvertTo-IsoDate([object] $Value) {
    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string] $Value)) {
        return ''
    }
    return ([datetime] $Value).ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
}

# Fetches and validates one Chromium milestone schedule.
function Get-ChromiumSchedule([int] $Milestone) {
    $uri = $script:ScheduleUrl -f $Milestone
    try {
        $data = Invoke-RestMethod -Uri $uri -Headers @{ 'User-Agent' = 'SkiaSharp-release-milestones' } -TimeoutSec 30
    } catch {
        throw "Failed to fetch Chromium schedule m$Milestone`: $($_.Exception.Message)"
    }
    $entries = @($data.mstones)
    if ($entries.Count -eq 0) {
        throw "Chromium returned no schedule for m$Milestone."
    }
    $schedule = $entries[0]
    $missing = @($script:RequiredScheduleFields | Where-Object { ![string] $schedule.$_ })
    if ($missing.Count -gt 0) {
        throw "Chromium m$Milestone schedule is missing: $($missing -join ', ')."
    }
    return $schedule
}

# Maps a Chromium schedule to the four SkiaSharp release milestones.
function New-DesiredReleaseMilestones([object] $Schedule, [int] $Milestone, [int] $Major) {
    $branch = ConvertTo-ScheduleDate $Schedule.branch_point
    $beta = ConvertTo-ScheduleDate $Schedule.earliest_beta
    $earlyCut = ConvertTo-ScheduleDate $Schedule.early_stable_cut
    $earlyStable = ConvertTo-ScheduleDate $Schedule.early_stable
    $stableCut = ConvertTo-ScheduleDate $Schedule.stable_cut
    $stable = ConvertTo-ScheduleDate $Schedule.stable_date
    $base = "$Major.$Milestone.0"
    $separator = [char] 0x00b7
    return @(
        [pscustomobject] @{
            Title = "$base-preview.1"
            Due = $beta
            DueOn = $beta.ToString('yyyy-MM-dd') + 'T00:00:00Z'
            Description = (
                "Skia m$Milestone preview.1 $separator Start $(Format-ScheduleDate $branch) $separator " +
                'Merge Skia sync PR and ship preview.'
            )
        }
        [pscustomobject] @{
            Title = "$base-preview.2"
            Due = $earlyStable
            DueOn = $earlyStable.ToString('yyyy-MM-dd') + 'T00:00:00Z'
            Description = (
                "Skia m$Milestone preview.2 $separator Start $(Format-ScheduleDate $earlyCut) $separator " +
                'Bug fixes and API additions from preview.1 feedback.'
            )
        }
        [pscustomobject] @{
            Title = "$base-rc.1"
            Due = $stableCut
            DueOn = $stableCut.ToString('yyyy-MM-dd') + 'T00:00:00Z'
            Description = (
                "Skia m$Milestone RC $separator Start $(Format-ScheduleDate $earlyStable) $separator " +
                'Critical bug fixes only, no new features.'
            )
        }
        [pscustomobject] @{
            Title = $base
            Due = $stable
            DueOn = $stable.ToString('yyyy-MM-dd') + 'T00:00:00Z'
            Description = (
                "Skia m$Milestone stable $separator Start $(Format-ScheduleDate $stableCut) $separator " +
                'Ship to NuGet.org, tag and create GitHub Release.'
            )
        }
    )
}

# Plans milestone creates and updates while avoiding creation of stale historical milestones.
function Get-ScheduleOperations([object[]] $Desired, [hashtable] $Existing) {
    $cutoff = [DateTime]::UtcNow.Date.AddDays(-30)
    return @(
        foreach ($item in $Desired) {
            if ($Existing.ContainsKey($item.Title)) {
                $found = $Existing[$item.Title]
                $actualDue = ConvertTo-IsoDate -Value $found.due_on
                $expectedDue = $item.Due.ToString('yyyy-MM-dd')
                $needsUpdate = $actualDue -ne $expectedDue -or [string] $found.description -ne $item.Description
                [pscustomobject] @{
                    Title = $item.Title
                    Number = [int] $found.number
                    Action = if ($needsUpdate) { 'update' } else { 'none' }
                    DueOn = $item.DueOn
                    Description = $item.Description
                }
            } elseif ($item.Due -ge $cutoff) {
                [pscustomobject] @{
                    Title = $item.Title
                    Number = $null
                    Action = 'create'
                    DueOn = $item.DueOn
                    Description = $item.Description
                }
            } else {
                [pscustomobject] @{
                    Title = $item.Title
                    Number = $null
                    Action = 'none'
                    DueOn = $item.DueOn
                    Description = $item.Description
                }
            }
        }
    )
}

# Plans rollover and closure for every open milestone that has an exact shipped tag.
function Get-MilestoneClosureOperations([hashtable] $Existing, [object[]] $Milestones, [string[]] $Tags,
    [string[]] $CreatableTitles, [scriptblock] $OpenItemsFor) {
    $operations = [System.Collections.Generic.List[object]]::new()
    $warnings = [System.Collections.Generic.List[string]]::new()
    $ordered = @($Milestones | Sort-Object SortKey)
    foreach ($current in $ordered) {
        if (!$Existing.ContainsKey($current.Title)) {
            continue
        }
        $found = $Existing[$current.Title]
        $tag = Get-ShippedTag -Title $current.Title -Tags $Tags
        if ([string] $found.state -ne 'open' -or !$tag) {
            continue
        }
        $openItems = @(& $OpenItemsFor ([int] $found.number))
        $target = $ordered | Where-Object {
            $_.SortKey -gt $current.SortKey -and
            !(Get-ShippedTag -Title $_.Title -Tags $Tags) -and
            (
                ($Existing.ContainsKey($_.Title) -and [string] $Existing[$_.Title].state -eq 'open') -or
                $CreatableTitles -contains $_.Title
            )
        } | Select-Object -First 1
        $status = 'pending'
        if ($openItems.Count -gt 0 -and !$target) {
            $status = 'blocked'
            $warnings.Add(
                "$($current.Title) shipped as $tag but has $($openItems.Count) open item(s) and no future milestone."
            )
        }
        $operations.Add([pscustomobject] @{
            Title = $current.Title
            Number = [int] $found.number
            Tag = $tag
            Status = $status
            OpenItems = $openItems
            MoveTo = if ($target) { $target.Title } else { $null }
        })
    }
    return [pscustomobject] @{ Operations = $operations.ToArray(); Warnings = $warnings.ToArray() }
}

# Creates or updates one milestone and verifies all managed fields.
function Sync-GitHubMilestone([string] $Repository, [object] $Operation) {
    if ($Operation.Action -eq 'create') {
        $arguments = @('api', "repos/$Repository/milestones", '-X', 'POST', '-f', "title=$($Operation.Title)", '-f',
            "due_on=$($Operation.DueOn)", '-f', "description=$($Operation.Description)")
        $description = "Create milestone $($Operation.Title)"
    } elseif ($Operation.Action -eq 'update') {
        $arguments = @('api', "repos/$Repository/milestones/$($Operation.Number)", '-X', 'PATCH', '-f',
            "due_on=$($Operation.DueOn)", '-f', "description=$($Operation.Description)")
        $description = "Update milestone $($Operation.Title)"
    } else {
        return
    }
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $description -Push:$writeRemote
    if ($writeRemote) {
        $milestones = Get-GitHubMilestoneMap -Repository $Repository
        $actual = $milestones[$Operation.Title]
        if (!$actual -or (ConvertTo-IsoDate -Value $actual.due_on) -ne $Operation.DueOn.Substring(0, 10) -or
            [string] $actual.description -ne $Operation.Description) {
            throw "Milestone $($Operation.Title) synchronization could not be verified."
        }
        Write-ReleaseStatus applied "$description verified."
    }
}

# Rejects newly arrived items and waits until planned milestone moves settle.
function Wait-MilestoneMoves([string] $Repository, [int] $MilestoneNumber, [int[]] $MovedNumbers) {
    $remaining = @()
    for ($attempt = 1; $attempt -le $script:MoveSettleAttempts; $attempt++) {
        $remaining = @(Get-OpenMilestoneItems -Repository $Repository -MilestoneNumber $MilestoneNumber)
        if ($remaining.Count -eq 0) {
            return
        }
        $unexpected = @($remaining | Where-Object { $MovedNumbers -notcontains $_.Number })
        if ($unexpected.Count -gt 0) {
            $detail = ($unexpected | ForEach-Object { "$($_.Kind) #$($_.Number)" }) -join ', '
            throw "Milestone gained open items during advancement: $detail."
        }
        if ($attempt -lt $script:MoveSettleAttempts) {
            Start-Sleep -Seconds $script:MoveSettleDelaySeconds
        }
    }
    $detail = ($remaining | ForEach-Object { "$($_.Kind) #$($_.Number)" }) -join ', '
    throw "Milestone moves did not settle: $detail."
}

# Moves open work and closes one shipped milestone after consistency checks.
function Complete-GitHubMilestone([string] $Repository, [object] $Operation, [hashtable] $Milestones) {
    $movedNumbers = @($Operation.OpenItems | ForEach-Object { [int] $_.Number })
    if ($Operation.OpenItems.Count -gt 0) {
        if (!$Milestones.ContainsKey($Operation.MoveTo)) {
            throw "Future milestone $($Operation.MoveTo) does not exist."
        }
        $target = $Milestones[$Operation.MoveTo]
        foreach ($item in $Operation.OpenItems) {
            $description = "Move $($item.Kind) #$($item.Number) to $($Operation.MoveTo)"
            Set-GitHubItemMilestone -Repository $Repository -Number $item.Number -MilestoneNumber ([int] $target.number) `
                -MilestoneTitle $Operation.MoveTo -Description $description
        }
    }
    if ($writeRemote) {
        Wait-MilestoneMoves -Repository $Repository -MilestoneNumber $Operation.Number -MovedNumbers $movedNumbers
    } else {
        $remaining = @(Get-OpenMilestoneItems -Repository $Repository -MilestoneNumber $Operation.Number)
        $unexpected = @($remaining | Where-Object { $movedNumbers -notcontains $_.Number })
        if ($unexpected.Count -gt 0) {
            $detail = ($unexpected | ForEach-Object { "$($_.Kind) #$($_.Number)" }) -join ', '
            throw "Milestone gained open items during dry-run advancement: $detail."
        }
    }
    $arguments = @('api', "repos/$Repository/milestones/$($Operation.Number)", '-X', 'PATCH', '-f', 'state=closed')
    $description = "Close shipped milestone $($Operation.Title)"
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $description -Push:$writeRemote
    if ($writeRemote) {
        $actual = (Get-GitHubMilestoneMap -Repository $Repository)[$Operation.Title]
        if ([string] $actual.state -ne 'closed') {
            throw "Milestone $($Operation.Title) closure could not be verified."
        }
        Write-ReleaseStatus applied "$description verified."
    }
}

# Maintains upcoming dates and safely rolls over and closes shipped milestones.
function Invoke-ReleaseMilestoneAdvancement([string] $Root, [int] $Count, [string] $Repository) {
    Write-ReleaseStatus checking "Advancing release milestone schedule for $Count Chromium milestone(s)."

    # Build the desired milestone schedule from current repository and Chromium state.
    $currentVersion = Get-RepositoryReleaseVersion -Root $Root
    $existing = Get-GitHubMilestoneMap -Repository $Repository
    $desired = [System.Collections.Generic.List[object]]::new()
    for ($milestone = $currentVersion.Milestone; $milestone -lt $currentVersion.Milestone + $Count; $milestone++) {
        $schedule = Get-ChromiumSchedule -Milestone $milestone
        foreach ($item in New-DesiredReleaseMilestones -Schedule $schedule -Milestone $milestone `
            -Major $currentVersion.Major) {
            $desired.Add($item)
        }
    }
    $scheduleOperations = @(Get-ScheduleOperations -Desired $desired.ToArray() -Existing $existing)
    $tags = Get-RemoteReleaseTags -Root $Root
    $known = @{}
    foreach ($title in @($existing.Keys) + @($desired | ForEach-Object { $_.Title })) {
        $parsed = ConvertTo-ReleaseMilestone $title
        if ($parsed) {
            $known[$title] = $parsed
        }
    }

    # Plan rollover and closure only where every future destination is known.
    $creatable = @($scheduleOperations | Where-Object Action -eq 'create' | ForEach-Object Title)
    $closurePlan = Get-MilestoneClosureOperations -Existing $existing -Milestones @($known.Values) -Tags $tags `
        -CreatableTitles $creatable -OpenItemsFor {
            param($number)
            Get-OpenMilestoneItems -Repository $Repository -MilestoneNumber $number
        }
    foreach ($warning in $closurePlan.Warnings) {
        Write-Warning $warning
    }
    if ($closurePlan.Warnings.Count -gt 0) {
        if ($writeRemote) {
            throw "Milestone advancement is blocked by $($closurePlan.Warnings.Count) rollover warning(s)."
        }
        Write-ReleaseStatus blocked "Advancement has $($closurePlan.Warnings.Count) warning(s); mutations are unsafe."
    }

    # Synchronize milestone metadata before moving work and closing shipped milestones.
    foreach ($operation in $scheduleOperations) {
        Sync-GitHubMilestone -Repository $Repository -Operation $operation
    }
    $milestonesAfterSync = if ($writeRemote) { Get-GitHubMilestoneMap -Repository $Repository } else { $existing.Clone() }
    if (!$writeRemote) {
        foreach ($operation in $scheduleOperations | Where-Object Action -eq 'create') {
            $milestonesAfterSync[$operation.Title] = [pscustomobject] @{ number = -1; state = 'open' }
        }
    }
    foreach ($operation in $closurePlan.Operations | Where-Object Status -eq 'pending') {
        Complete-GitHubMilestone -Repository $Repository -Operation $operation -Milestones $milestonesAfterSync
    }
    $creates = @($scheduleOperations | Where-Object Action -eq 'create').Count
    $updates = @($scheduleOperations | Where-Object Action -eq 'update').Count
    $closes = @($closurePlan.Operations | Where-Object Status -eq 'pending').Count
    Write-ReleaseStatus checked "Advancement: $creates create(s), $updates update(s), $closes closure(s)."
}

if ($MyInvocation.InvocationName -ne '.') {
    $root = Get-GitRepositoryRoot

    # 1. Reconcile merged pull requests and linked issues to the release where they shipped.
    Write-ReleaseStatus start "Release milestone reconciliation ($mode)."
    Invoke-ReleaseAssignmentReconciliation -Root $root -Version $Version -Repository $Repository

    # 2. Maintain upcoming dates, roll over open work, and close shipped milestones.
    Write-ReleaseStatus start "Release milestone advancement ($mode)."
    Invoke-ReleaseMilestoneAdvancement -Root $root -Count $Count -Repository $Repository

    Write-ReleaseStatus complete "Release milestone maintenance completed ($mode)."
}
