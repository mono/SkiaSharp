<#
.SYNOPSIS
    Audit and fix milestone assignments based on what shipped in each release branch.

.DESCRIPTION
    Determines what shipped in each release by comparing merge-bases of consecutive
    release branches on main. For each PR found in a release range, ensures the PR
    and any linked issues are assigned to the correct milestone.

    Algorithm:
    1. Find all release branches for the current version (e.g., 4.150.0-*)
    2. Sort them in release order (preview.1, preview.2, rc.1, stable)
    3. For consecutive branches, compute: merge-base(main, branchN) .. merge-base(main, branchN+1)
    4. Commits in that range shipped in branchN+1
    5. For main HEAD after the last branch: those go into the next milestone (rc.1 or stable)
    6. Extract PR numbers from commit messages, resolve linked issues, fix milestones

.PARAMETER DryRun
    Print what would be done without making changes.

.PARAMETER Repo
    GitHub repo (default: mono/SkiaSharp).

.PARAMETER Version
    Version prefix to audit (e.g., "4.150.0"). Defaults to reading from VERSIONS.txt.

.EXAMPLE
    pwsh audit-milestones.ps1 -DryRun
    pwsh audit-milestones.ps1 -Version 4.150.0
#>

param(
    [switch]$DryRun,
    [string]$Repo = "mono/SkiaSharp",
    [string]$Version
)

$ErrorActionPreference = "Stop"

# Resolve version from VERSIONS.txt if not provided
if (-not $Version) {
    $versionsPath = Join-Path $PSScriptRoot ".." ".." "VERSIONS.txt"
    if (-not (Test-Path $versionsPath)) {
        throw "VERSIONS.txt not found at $versionsPath"
    }
    $versionsPath = Resolve-Path $versionsPath
    $major = $null
    $milestone = $null
    foreach ($line in Get-Content $versionsPath) {
        $parts = $line.Trim() -split '\s+'
        if ($parts.Count -ge 3) {
            if ($parts[0] -eq "SkiaSharp" -and $parts[1] -eq "nuget") { $major = $parts[2].Split(".")[0] }
            if ($parts[0] -eq "libSkiaSharp" -and $parts[1] -eq "milestone") { $milestone = $parts[2] }
        }
    }
    if (-not $major -or -not $milestone) { throw "Could not read version from VERSIONS.txt" }
    $Version = "$major.$milestone.0"
}

Write-Host "📋 Auditing milestone assignments for $Version"
Write-Host "   Repo: $Repo"
if ($DryRun) { Write-Host "   Mode: DRY RUN" }
Write-Host ""

# Fetch release branches, sorted in release order
git fetch origin --quiet 2>/dev/null
$rawBranches = git branch -r --list "origin/release/$Version*" | ForEach-Object { $_.Trim() }

if ($rawBranches.Count -eq 0) {
    throw "No release branches found matching origin/release/$Version*"
}

# Sort in proper release order: preview.1, preview.2, ..., rc.1, rc.2, ..., stable
function Get-ReleaseRank([string]$BranchName) {
    $suffix = $BranchName -replace "origin/release/$Version", ''
    if (-not $suffix) { return @(2, 0) }  # stable (no suffix) comes last
    $suffix = $suffix.TrimStart('-')
    if ($suffix -match '^preview\.(\d+)$') { return @(0, [int]$Matches[1]) }
    if ($suffix -match '^rc\.(\d+)$') { return @(1, [int]$Matches[1]) }
    return @(3, 0)  # unknown suffix, sort last
}

$allBranches = $rawBranches | Sort-Object { $r = Get-ReleaseRank $_; $r[0] * 1000 + $r[1] }

Write-Host "🔍 Found $($allBranches.Count) release branches:"
$allBranches | ForEach-Object { Write-Host "   $_" }
Write-Host ""

# Find the previous version's stable branch to use as the "from" boundary for preview.1
# Look for release branches with lower version numbers (skip *.x maintenance branches)
$prevBranch = $null
$allReleaseBranches = git branch -r --list "origin/release/*" | ForEach-Object { $_.Trim() }
$prevCandidates = $allReleaseBranches | Where-Object {
    # Match versioned branches like origin/release/4.148.0 but skip *.x maintenance branches
    $_ -match 'origin/release/\d+\.\d+\.\d+' -and $_ -notmatch '\.x$' -and $_ -notlike "origin/release/$Version*"
} | Sort-Object -Descending
foreach ($candidate in $prevCandidates) {
    $candidateVersion = ($candidate -replace 'origin/release/', '') -replace '-.*', ''
    if ([version]$candidateVersion -lt [version]$Version) {
        $prevBranch = $candidate
        break
    }
}

# Get merge-bases for each branch (the point on main where the branch was cut)
$mergeBases = @()
foreach ($branch in $allBranches) {
    $mb = git merge-base origin/main $branch 2>/dev/null
    if (-not $mb) { throw "Could not find merge-base for $branch" }
    $mergeBases += $mb
}

# Get merge-base for previous version's stable branch (boundary for preview.1)
$prevMergeBase = $null
if ($prevBranch) {
    $prevMergeBase = git merge-base origin/main $prevBranch 2>/dev/null
    Write-Host "📌 Previous release: $prevBranch (boundary for first preview)"
    Write-Host ""
}

# Get milestone map from GitHub
$json = gh api --paginate --slurp "repos/$Repo/milestones?state=all&per_page=100" 2>&1
if ($LASTEXITCODE -ne 0) { throw "Failed to fetch milestones: $json" }
$msMap = @{}
($json | ConvertFrom-Json) | ForEach-Object { $_ } | ForEach-Object { $msMap[$_.title] = $_.number }

# Build ranges: each release branch contains commits between its merge-base and the previous one
$ranges = @()

for ($i = 0; $i -lt $allBranches.Count; $i++) {
    $branch = $allBranches[$i]
    $msTitle = ($branch -replace 'origin/release/', '')

    if ($i -eq 0) {
        # First branch — use previous version's stable branch merge-base as the "from" boundary
        $ranges += @{ Branch = $branch; Title = $msTitle; From = $prevMergeBase; To = $mergeBases[$i] }
    } else {
        $ranges += @{ Branch = $branch; Title = $msTitle; From = $mergeBases[$i - 1]; To = $mergeBases[$i] }
    }
}

# Also: commits on main after the last branch → next milestone
$lastMb = $mergeBases[$mergeBases.Count - 1]
$headSha = git rev-parse origin/main
if ($lastMb -ne $headSha) {
    # Find the next open milestone for this version by checking what exists
    $lastTitle = ($allBranches[$allBranches.Count - 1] -replace 'origin/release/', '')
    $lastRank = Get-ReleaseRank "origin/release/$lastTitle"

    # Look through all milestones for this version, find the next one in release order
    $nextTitle = $null
    $candidates = $msMap.Keys | Where-Object { $_ -like "$Version*" } | Sort-Object {
        $r = Get-ReleaseRank "origin/release/$_"; $r[0] * 1000 + $r[1]
    }
    foreach ($candidate in $candidates) {
        $candidateRank = Get-ReleaseRank "origin/release/$candidate"
        if (($candidateRank[0] * 1000 + $candidateRank[1]) -gt ($lastRank[0] * 1000 + $lastRank[1])) {
            $nextTitle = $candidate
            break
        }
    }

    if ($nextTitle) {
        $ranges += @{ Branch = "origin/main (HEAD)"; Title = $nextTitle; From = $lastMb; To = $headSha }
    }
}

$fixed = 0
$correct = 0

foreach ($range in $ranges) {
    $msTitle = $range.Title
    if (-not $msMap.ContainsKey($msTitle)) {
        Write-Host "⚠️  Milestone '$msTitle' not found on GitHub, skipping"
        Write-Host ""
        continue
    }
    $msId = $msMap[$msTitle]

    if ($null -eq $range.From) {
        Write-Host "⏭️  $msTitle — no previous release branch found, skipping (verify manually)"
        Write-Host ""
        continue
    }

    # Get PRs in this range
    $log = git log --oneline --first-parent "$($range.From)..$($range.To)" 2>/dev/null
    $prs = @()
    foreach ($line in $log) {
        if ($line -match '\(#(\d+)\)') {
            $prs += [int]$Matches[1]
        }
    }

    Write-Host "📦 $msTitle ($($prs.Count) PRs) — $($range.Branch)"

    foreach ($pr in $prs) {
        # Check PR milestone
        $currentMs = gh api "repos/$Repo/issues/$pr" --jq '.milestone.title // ""' 2>/dev/null
        if ($currentMs -ne $msTitle) {
            Write-Host "  🔄 #$pr  $(if ($currentMs) { $currentMs } else { 'none' }) → $msTitle"
            if (-not $DryRun) {
                gh api "repos/$Repo/issues/$pr" -X PATCH -F milestone=$msId --silent 2>/dev/null
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "  ❌ Failed to update #$pr"
                    $errors++
                    continue
                }
            }
            $fixed++
        } else {
            $correct++
        }

        # Get linked issues via GraphQL (catches sidebar-linked + keyword-linked)
        $linkedIssues = @()
        $gqlResult = gh api graphql -f query="
            query {
                repository(owner: `"$($Repo.Split('/')[0])`", name: `"$($Repo.Split('/')[1])`") {
                    pullRequest(number: $pr) {
                        closingIssuesReferences(first: 50) {
                            nodes { number }
                        }
                    }
                }
            }" --jq '.data.repository.pullRequest.closingIssuesReferences.nodes[].number' 2>/dev/null
        if ($LASTEXITCODE -eq 0 -and $gqlResult) {
            $linkedIssues += $gqlResult -split "`n" | Where-Object { $_ -match '^\d+$' } | ForEach-Object { [int]$_ }
        }

        # Also check PR body for closing keywords (fallback)
        $body = gh api "repos/$Repo/pulls/$pr" --jq '.body // ""' 2>/dev/null
        if ($LASTEXITCODE -eq 0 -and $body) {
            $closingPattern = '(?i)(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\s*:?\s+#(\d+)'
            $regexMatches = [regex]::Matches($body, $closingPattern)
            foreach ($m in $regexMatches) {
                $linkedIssues += [int]$m.Groups[1].Value
            }
        }
        $linkedIssues = $linkedIssues | Sort-Object -Unique

        foreach ($issue in $linkedIssues) {
            $issueMs = gh api "repos/$Repo/issues/$issue" --jq '.milestone.title // ""' 2>/dev/null
            if ($issueMs -ne $msTitle) {
                Write-Host "  🔄 #$issue (via #$pr)  $(if ($issueMs) { $issueMs } else { 'none' }) → $msTitle"
                if (-not $DryRun) {
                    gh api "repos/$Repo/issues/$issue" -X PATCH -F milestone=$msId --silent 2>/dev/null
                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "  ❌ Failed to update #$issue"
                        $errors++
                        continue
                    }
                }
                $fixed++
            } else {
                $correct++
            }
        }
    }
    Write-Host ""
}

Write-Host ("=" * 60)
Write-Host "Summary: $fixed to fix, $correct correct, $errors errors"
if ($DryRun) { Write-Host "(DRY RUN — no actual changes were made)" }
Write-Host ("=" * 60)
