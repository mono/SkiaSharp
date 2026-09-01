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
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'ReleaseMilestones.Common.psm1') -ArgumentList ([bool] $Push) -Force
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }
$root = Get-GitRepositoryRoot

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
