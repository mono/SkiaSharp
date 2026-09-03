#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates release milestone dates, rolls open work forward, and closes shipped milestones.

.PARAMETER Count
    The number of Chromium milestones whose release milestones are maintained.

.PARAMETER Repository
    The GitHub repository whose milestones are maintained.

.PARAMETER Push
    Performs GitHub milestone mutations. Without this switch, the script is
    read-only and reports exact skipped mutations.
#>

[CmdletBinding()]
param(
    [ValidateRange(1, 20)]
    [int] $Count = 3,

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
$scheduleUrl = 'https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={0}'
$requiredScheduleFields = @(
    'branch_point',
    'early_stable_cut',
    'stable_cut',
    'stable_date'
)
$moveSettleAttempts = 5
$moveSettleDelaySeconds = 2

# Reads all open issues and pull requests assigned to one milestone.
function Get-OpenMilestoneItems([string] $Repository, [int] $MilestoneNumber) {
    $pages = Invoke-GitHubJsonWithRetry -Arguments @(
        'api',
        '--paginate',
        '--slurp',
        "repos/$Repository/issues?milestone=$MilestoneNumber&state=open&per_page=100"
    )
    return @(Expand-GitHubPages $pages | ForEach-Object {
        [pscustomobject] @{
            Number = [int] $_.number
            Title = [string] $_.title
            Url = [string] $_.html_url
            Kind = if ($_.PSObject.Properties['pull_request']) { 'pull-request' } else { 'issue' }
        }
    })
}

# Converts a Chromium timestamp to a UTC date.
function ConvertTo-ScheduleDate([string] $Value) {
    return [DateTimeOffset]::Parse(
        $Value,
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::AssumeUniversal).UtcDateTime.Date
}

# Formats a schedule date for milestone descriptions.
function Format-ScheduleDate([datetime] $Date) {
    return $Date.ToString('ddd, MMM dd, yyyy', [Globalization.CultureInfo]::InvariantCulture)
}

# Uses end-of-day UTC so GitHub preserves the intended calendar date when normalizing milestone deadlines.
function Format-GitHubDueOn([datetime] $Date) {
    return $Date.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture) + 'T23:59:59Z'
}

# Normalizes GitHub string or DateTime values to an ISO calendar date.
function ConvertTo-IsoDate([object] $Value) {
    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string] $Value)) {
        return ''
    }
    $utc = if ($Value -is [datetime]) {
        if ($Value.Kind -eq [DateTimeKind]::Unspecified) {
            [DateTime]::SpecifyKind($Value, [DateTimeKind]::Utc)
        } else {
            $Value.ToUniversalTime()
        }
    } elseif ($Value -is [DateTimeOffset]) {
        $Value.UtcDateTime
    } else {
        [DateTimeOffset]::Parse(
            [string] $Value,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::AssumeUniversal).UtcDateTime
    }
    return $utc.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
}

# Fetches and validates one Chromium milestone schedule.
function Get-ChromiumSchedule([int] $Milestone) {
    $uri = $scheduleUrl -f $Milestone
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
    $missing = @($requiredScheduleFields | Where-Object { ![string] $schedule.$_ })
    if ($missing.Count -gt 0) {
        throw "Chromium m$Milestone schedule is missing: $($missing -join ', ')."
    }
    return $schedule
}

# Maps a Chromium schedule to weekly preview, release candidate, and stable milestones.
function New-DesiredReleaseMilestones([object] $Schedule, [int] $Milestone, [int] $Major) {
    $branch = ConvertTo-ScheduleDate $Schedule.branch_point
    $preview = $branch.AddDays(1)
    $earlyStableCut = ConvertTo-ScheduleDate $Schedule.early_stable_cut
    $stableCut = ConvertTo-ScheduleDate $Schedule.stable_cut
    $stable = ConvertTo-ScheduleDate $Schedule.stable_date
    $base = "$Major.$Milestone.0"
    $separator = [char] 0x00b7
    return @(
        [pscustomobject] @{
            Title = "$base-preview.1"
            Due = $preview
            DueOn = Format-GitHubDueOn $preview
            Description = (
                "Skia m$Milestone preview.1 $separator Branch point $(Format-ScheduleDate $branch) $separator " +
                'Merge the Skia sync PR and ship the initial preview the next day.')
        }
        [pscustomobject] @{
            Title = "$base-preview.2"
            Due = $earlyStableCut
            DueOn = Format-GitHubDueOn $earlyStableCut
            Description = (
                "Skia m$Milestone preview.2 $separator Start $(Format-ScheduleDate $preview) $separator " +
                'Incorporate initial preview feedback and ship the second preview.')
        }
        [pscustomobject] @{
            Title = "$base-rc.1"
            Due = $stableCut
            DueOn = Format-GitHubDueOn $stableCut
            Description = (
                "Skia m$Milestone RC $separator Start $(Format-ScheduleDate $earlyStableCut) $separator " +
                'Stabilize the release candidate; critical fixes only.')
        }
        [pscustomobject] @{
            Title = $base
            Due = $stable
            DueOn = Format-GitHubDueOn $stable
            Description = (
                "Skia m$Milestone stable $separator Start $(Format-ScheduleDate $stableCut) $separator " +
                'Ship to NuGet.org, tag, and create the GitHub Release.')
        }
    )
}

# Plans milestone creates and updates while avoiding stale historical milestones.
function Get-ScheduleOperations([object[]] $Desired, [hashtable] $Existing) {
    $cutoff = [DateTime]::UtcNow.Date.AddDays(-30)
    return @(
        foreach ($item in $Desired) {
            if ($Existing.ContainsKey($item.Title)) {
                $found = $Existing[$item.Title]
                $actualDue = ConvertTo-IsoDate -Value $found.due_on
                $expectedDue = $item.Due.ToString('yyyy-MM-dd')
                $needsUpdate = $actualDue -ne $expectedDue -or
                    [string] $found.description -ne $item.Description
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
function Get-MilestoneClosureOperations(
    [hashtable] $Existing,
    [object[]] $Milestones,
    [string[]] $Tags,
    [string[]] $CreatableTitles,
    [scriptblock] $OpenItemsFor
) {
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
                "$($current.Title) shipped as $tag but has $($openItems.Count) open item(s) and no future milestone.")
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
        $arguments = @(
            'api', "repos/$Repository/milestones",
            '-X', 'POST',
            '-f', "title=$($Operation.Title)",
            '-f', "due_on=$($Operation.DueOn)",
            '-f', "description=$($Operation.Description)"
        )
        $description = "Create milestone $($Operation.Title)"
    } elseif ($Operation.Action -eq 'update') {
        $arguments = @(
            'api', "repos/$Repository/milestones/$($Operation.Number)",
            '-X', 'PATCH',
            '-f', "due_on=$($Operation.DueOn)",
            '-f', "description=$($Operation.Description)"
        )
        $description = "Update milestone $($Operation.Title)"
    } else {
        return
    }
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $description -Push:$Push
    if ($writeRemote) {
        $milestones = Get-GitHubMilestoneMap -Repository $Repository
        $actual = $milestones[$Operation.Title]
        if (!$actual -or
            (ConvertTo-IsoDate -Value $actual.due_on) -ne $Operation.DueOn.Substring(0, 10) -or
            [string] $actual.description -ne $Operation.Description) {
            throw "Milestone $($Operation.Title) synchronization could not be verified."
        }
        Write-ReleaseStatus applied "$description verified."
    }
}

# Rejects newly arrived items and waits until planned milestone moves settle.
function Wait-MilestoneMoves([string] $Repository, [int] $MilestoneNumber, [int[]] $MovedNumbers) {
    $remaining = @()
    for ($attempt = 1; $attempt -le $moveSettleAttempts; $attempt++) {
        $remaining = @(Get-OpenMilestoneItems -Repository $Repository -MilestoneNumber $MilestoneNumber)
        if ($remaining.Count -eq 0) {
            return
        }
        $unexpected = @($remaining | Where-Object { $MovedNumbers -notcontains $_.Number })
        if ($unexpected.Count -gt 0) {
            $detail = ($unexpected | ForEach-Object { "$($_.Kind) #$($_.Number)" }) -join ', '
            throw "Milestone gained open items during advancement: $detail."
        }
        if ($attempt -lt $moveSettleAttempts) {
            Start-Sleep -Seconds $moveSettleDelaySeconds
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
            Set-GitHubItemMilestone `
                -Repository $Repository `
                -Number $item.Number `
                -MilestoneNumber ([int] $target.number) `
                -MilestoneTitle $Operation.MoveTo `
                -Description $description `
                -Push:$Push
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
    $arguments = @(
        'api', "repos/$Repository/milestones/$($Operation.Number)",
        '-X', 'PATCH',
        '-f', 'state=closed'
    )
    $description = "Close shipped milestone $($Operation.Title)"
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $description -Push:$Push
    if ($writeRemote) {
        $actual = (Get-GitHubMilestoneMap -Repository $Repository)[$Operation.Title]
        if ([string] $actual.state -ne 'closed') {
            throw "Milestone $($Operation.Title) closure could not be verified."
        }
        Write-ReleaseStatus applied "$description verified."
    }
}

# 1. Maintain Chromium-derived dates, roll open work, and close shipped milestones.
Write-ReleaseStatus start "Release milestone update ($mode)."

# 1.1 Build the desired milestone schedule from repository and Chromium state.
$currentVersion = Get-RepositoryReleaseVersion -Root $root
$existing = Get-GitHubMilestoneMap -Repository $Repository
$desired = [System.Collections.Generic.List[object]]::new()
for ($milestone = $currentVersion.Milestone; $milestone -lt $currentVersion.Milestone + $Count; $milestone++) {
    $schedule = Get-ChromiumSchedule -Milestone $milestone
    foreach ($item in New-DesiredReleaseMilestones `
        -Schedule $schedule `
        -Milestone $milestone `
        -Major $currentVersion.Major) {
        $desired.Add($item)
    }
}
$scheduleOperations = @(Get-ScheduleOperations -Desired $desired.ToArray() -Existing $existing)
$tags = Get-RemoteReleaseTags -Root $root
$known = @{}
foreach ($title in @($existing.Keys) + @($desired | ForEach-Object { $_.Title })) {
    $parsed = ConvertTo-ReleaseMilestone $title
    if ($parsed) {
        $known[$title] = $parsed
    }
}

# 1.2 Plan rollover and closure only where every future destination is known.
$creatable = @($scheduleOperations | Where-Object Action -eq 'create' | ForEach-Object Title)
$closurePlan = Get-MilestoneClosureOperations `
    -Existing $existing `
    -Milestones @($known.Values) `
    -Tags $tags `
    -CreatableTitles $creatable `
    -OpenItemsFor {
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

# 1.3 Synchronize metadata before moving work and closing shipped milestones.
foreach ($operation in $scheduleOperations) {
    Sync-GitHubMilestone -Repository $Repository -Operation $operation
}
$milestonesAfterSync = if ($writeRemote) {
    Get-GitHubMilestoneMap -Repository $Repository
} else {
    $existing.Clone()
}
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

Write-ReleaseStatus complete "Release milestone update completed ($mode)."
