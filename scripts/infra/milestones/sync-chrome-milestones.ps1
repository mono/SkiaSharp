<#
.SYNOPSIS
    Sync SkiaSharp GitHub milestones with the Chrome/Skia release schedule.

.DESCRIPTION
    Fetches the Chromium release schedule from chromiumdash.appspot.com and
    creates/updates GitHub milestones for the next N Skia milestones.

    SkiaSharp release cadence (aligned to Chrome):
      - Chrome Beta         -> SkiaSharp Preview.1  (starts work on Branch day)
      - Chrome Early Stable -> SkiaSharp Preview.2  (starts work on Early Stable Cut day)
      - Chrome Stable Cut   -> SkiaSharp RC         (starts work on Early Stable day)
      - Chrome Stable       -> SkiaSharp Stable     (starts work on Stable Cut day)

    Reads major version and current milestone from scripts/VERSIONS.txt.

.PARAMETER DryRun
    Print what would be done without making changes.

.PARAMETER Count
    Number of milestones ahead to sync (default: 5).

.PARAMETER Repo
    GitHub repo (default: mono/SkiaSharp).

.EXAMPLE
    pwsh sync-chrome-milestones.ps1 -DryRun -Count 3
#>

param(
    [switch]$DryRun,
    [int]$Count = 5,
    [string]$Repo = "mono/SkiaSharp"
)

$ErrorActionPreference = "Stop"

$SCHEDULE_URL = "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={0}"

# VERSIONS.txt is at scripts/VERSIONS.txt — two levels up from this script
$VERSIONS_PATH = Join-Path $PSScriptRoot ".." ".." "VERSIONS.txt"
if (-not (Test-Path $VERSIONS_PATH)) {
    throw "VERSIONS.txt not found at $(Join-Path $PSScriptRoot '../../VERSIONS.txt')"
}
$VERSIONS_PATH = Resolve-Path $VERSIONS_PATH

function Read-Versions {
    $major = $null
    $milestone = $null

    foreach ($line in Get-Content $VERSIONS_PATH) {
        $line = $line.Trim()
        if (-not $line -or $line.StartsWith("#")) { continue }
        $parts = $line -split '\s+'
        if ($parts.Count -ge 3) {
            if ($parts[0] -eq "SkiaSharp" -and $parts[1] -eq "nuget") {
                $major = [int]($parts[2].Split(".")[0])
            }
            elseif ($parts[0] -eq "libSkiaSharp" -and $parts[1] -eq "milestone") {
                $milestone = [int]$parts[2]
            }
        }
    }

    if ($null -eq $major) {
        throw "Could not find 'SkiaSharp nuget X.Y.Z' in $VERSIONS_PATH — cannot determine major version."
    }
    if ($null -eq $milestone) {
        throw "Could not find 'libSkiaSharp milestone N' in $VERSIONS_PATH — cannot determine current Skia milestone."
    }

    return @{ Major = $major; Milestone = $milestone }
}

function Get-ChromeSchedule([int]$Mstone) {
    $url = $SCHEDULE_URL -f $Mstone
    try {
        $data = Invoke-RestMethod -Uri $url -TimeoutSec 15
    }
    catch {
        throw "Failed to fetch Chrome schedule for m$Mstone from $url`: $_"
    }

    if (-not $data.mstones -or $data.mstones.Count -eq 0) {
        throw "No schedule data returned for m$Mstone from $url — milestone may not exist yet."
    }

    $schedule = $data.mstones[0]
    $required = @("branch_point", "earliest_beta", "early_stable_cut", "early_stable", "stable_cut", "stable_date")
    foreach ($field in $required) {
        if (-not $schedule.$field) {
            throw "Chrome schedule for m$Mstone is missing required field '$field' — schedule may be incomplete."
        }
    }

    return $schedule
}

function ConvertTo-Date([string]$IsoString) {
    return [datetime]::SpecifyKind([datetime]::Parse($IsoString.Split("T")[0]), [System.DateTimeKind]::Utc)
}

function Format-DateDisplay([datetime]$Date) {
    return $Date.ToString("ddd, MMM dd, yyyy", [System.Globalization.CultureInfo]::InvariantCulture)
}

function Format-DateIso([datetime]$Date) {
    return $Date.ToString("yyyy-MM-ddT00:00:00Z")
}

function Get-SkiaMilestones($Chrome, [int]$Mstone, [int]$Major) {
    $branch = ConvertTo-Date $Chrome.branch_point
    $beta = ConvertTo-Date $Chrome.earliest_beta
    $earlyStableCut = ConvertTo-Date $Chrome.early_stable_cut
    $earlyStable = ConvertTo-Date $Chrome.early_stable
    $stableCut = ConvertTo-Date $Chrome.stable_cut
    $stable = ConvertTo-Date $Chrome.stable_date

    return @(
        @{
            Title = "$Major.$Mstone.0-preview.1"
            DueOn = $beta
            Description = "Skia m$Mstone preview.1 · Start $(Format-DateDisplay $branch) · Merge Skia sync PR and ship preview."
        },
        @{
            Title = "$Major.$Mstone.0-preview.2"
            DueOn = $earlyStable
            Description = "Skia m$Mstone preview.2 · Start $(Format-DateDisplay $earlyStableCut) · Bug fixes and API additions from preview.1 feedback."
        },
        @{
            Title = "$Major.$Mstone.0-rc.1"
            DueOn = $stableCut
            Description = "Skia m$Mstone RC · Start $(Format-DateDisplay $earlyStable) · Critical bug fixes only, no new features."
        },
        @{
            Title = "$Major.$Mstone.0"
            DueOn = $stable
            Description = "Skia m$Mstone stable · Start $(Format-DateDisplay $stableCut) · Ship to NuGet.org, tag and create GitHub Release."
        }
    )
}

function Get-ExistingMilestones([string]$Repo) {
    $json = gh api --paginate --slurp "repos/$Repo/milestones?state=all&per_page=100" 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch milestones from GitHub: $json`nEnsure 'gh' CLI is authenticated and has access to $Repo."
    }

    $milestones = @{}
    $pages = $json | ConvertFrom-Json
    # --slurp wraps pages in an outer array: [[...page1...], [...page2...]]
    $items = $pages | ForEach-Object { $_ }
    foreach ($item in $items) {
        $milestones[$item.title] = @{
            Number      = $item.number
            State       = $item.state
            DueOn       = if ($item.due_on) { ([datetimeoffset]$item.due_on).UtcDateTime.ToString("yyyy-MM-dd") } else { "" }
            Description = if ($item.description) { $item.description } else { "" }
        }
    }
    return $milestones
}

function New-Milestone([string]$Repo, [string]$Title, [datetime]$DueOn, [string]$Description, [switch]$DryRun) {
    if ($DryRun) {
        Write-Host "  📝 CREATE  $Title  due $(Format-DateDisplay $DueOn)"
        return
    }

    $body = @{ title = $Title; due_on = (Format-DateIso $DueOn); description = $Description } | ConvertTo-Json
    $result = $body | gh api "repos/$Repo/milestones" -X POST --input - 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create milestone '$Title': $result"
    }
    $ms = $result | ConvertFrom-Json
    Write-Host "  ✅ Created  $Title  ms #$($ms.number)  due $(Format-DateDisplay $DueOn)"
}

function Update-Milestone([string]$Repo, [int]$Number, [string]$Title, [datetime]$DueOn, [string]$Description, [string[]]$Changes, [switch]$DryRun) {
    if ($DryRun) {
        Write-Host "  🔄 UPDATE  $Title  ($($Changes -join ', '))"
        return
    }

    $body = @{ due_on = (Format-DateIso $DueOn); description = $Description } | ConvertTo-Json
    $result = $body | gh api "repos/$Repo/milestones/$Number" -X PATCH --input - 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to update milestone '$Title' (ms #$Number): $result"
    }
    Write-Host "  🔄 Updated  $Title  ($($Changes -join ', '))"
}

# Main
$versions = Read-Versions
$VERSIONS_MAJOR = $versions.Major
$VERSIONS_MILESTONE = $versions.Milestone

Write-Host "📅 Syncing Chrome schedule → SkiaSharp milestones"
Write-Host "   Date: $(Get-Date -Format 'yyyy-MM-dd')"
Write-Host "   Repo: $Repo"
Write-Host "   Count: $Count milestones ahead"
Write-Host "   Major version: $VERSIONS_MAJOR (from VERSIONS.txt)"
Write-Host "   Current milestone: m$VERSIONS_MILESTONE (from VERSIONS.txt)"
if ($DryRun) {
    Write-Host "   Mode: DRY RUN (no changes will be made)"
}
Write-Host ""

Write-Host "🌐 Starting from milestone: m$VERSIONS_MILESTONE"
Write-Host ""

$existing = Get-ExistingMilestones $Repo
Write-Host "📋 Found $($existing.Count) existing GitHub milestones"
Write-Host ""

$created = 0
$updated = 0
$skipped = 0
$today = [datetime]::UtcNow

for ($mstone = $VERSIONS_MILESTONE; $mstone -lt ($VERSIONS_MILESTONE + $Count); $mstone++) {
    $chrome = Get-ChromeSchedule $mstone

    $beta = ConvertTo-Date $chrome.earliest_beta
    $stable = ConvertTo-Date $chrome.stable_date
    Write-Host "m$mstone`: Beta $(Format-DateDisplay $beta) → Stable $(Format-DateDisplay $stable)"

    $skMilestones = Get-SkiaMilestones $chrome $mstone $VERSIONS_MAJOR

    foreach ($ms in $skMilestones) {
        $title = $ms.Title
        $dueOn = $ms.DueOn
        $description = $ms.Description

        if ($existing.ContainsKey($title)) {
            $ex = $existing[$title]
            $changes = @()

            $newDue = $dueOn.ToString("yyyy-MM-dd")
            if ($ex.DueOn -ne $newDue) {
                $changes += "due: $(if ($ex.DueOn) { $ex.DueOn } else { 'none' }) → $newDue"
            }

            if ($ex.Description -ne $description) {
                $changes += "description updated"
            }

            if ($changes.Count -gt 0) {
                Update-Milestone $Repo $ex.Number $title $dueOn $description $changes -DryRun:$DryRun
                $updated++
            }
            else {
                Write-Host "  ✓ $title — up to date"
                $skipped++
            }
        }
        elseif ($dueOn -ge $today.AddDays(-30)) {
            New-Milestone $Repo $title $dueOn $description -DryRun:$DryRun
            $created++
        }
        else {
            Write-Host "  ⏭️  $title — past milestone, skipping"
            $skipped++
        }
    }

    Write-Host ""
}

Write-Host ("=" * 60)
Write-Host "Summary: $created created, $updated updated, $skipped unchanged"
if ($DryRun) {
    Write-Host "(DRY RUN — no actual changes were made)"
}
Write-Host ("=" * 60)
