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
    5. Only assign a range to a milestone that actually SHIPPED (has a git tag). An unshipped
       preview rolls its commits forward to the next shipped release; commits on main past the
       last branch belong to a not-yet-cut release and are skipped until it ships.
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

# Published tags for this version line — a release counts as "shipped" only if it was tagged.
# Pre-releases are tagged v{title}.{build} (e.g. v4.151.0-preview.2.1); stable is v{title}.
$allTags = @(git ls-remote --tags origin "v$Version*" 2>/dev/null |
    ForEach-Object { ($_ -split '/')[-1] } |
    Where-Object { $_ -and $_ -notmatch '\^\{\}$' } | Sort-Object)

function Get-ShippedTag([string]$Title) {
    if ($Title -match '-(preview|rc)\.\d+$') {
        # pre-release: newest tagged build of it (v{title}.{build}), if any
        return ($allTags | Where-Object { $_ -like "v$Title.*" } | Select-Object -Last 1)
    }
    # stable / hotfix: the exact tag (v{title}), if it exists
    if ($allTags -contains "v$Title") { return "v$Title" }
    return $null
}
function Test-Shipped([string]$Title) { return [bool](Get-ShippedTag $Title) }

# Effective milestone for a range = the earliest SHIPPED release at or after this position.
# An unshipped preview rolls forward to the next shipped release (its commits are contained in
# that release); if nothing at/after this position has shipped yet, the range is provisional and
# returns $null so the caller skips it (this also stops trailing main commits from being
# speculatively parked in an unreleased "stable" milestone).
function Get-EffectiveShippedTitle([int]$StartIdx, [string[]]$Titles) {
    for ($j = $StartIdx; $j -lt $Titles.Count; $j++) {
        if (Test-Shipped $Titles[$j]) { return $Titles[$j] }
    }
    return $null
}

if ($allTags.Count -gt 0) {
    Write-Host "🏷️  Published tags for $Version : $($allTags -join ', ')"
} else {
    Write-Host "🏷️  No published tags for $Version yet"
}
Write-Host ""

# @() forces an array even when a single branch matches; without it PowerShell
# collapses a one-element result to a scalar string, and $allBranches[0] would
# then return the first *character* ('o' from "origin/...") instead of the branch.
$rawBranches = @(git branch -r --list "origin/release/$Version*" | ForEach-Object { $_.Trim() })

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

$allBranches = @($rawBranches | Sort-Object { $r = Get-ReleaseRank $_; $r[0] * 1000 + $r[1] })

Write-Host "🔍 Found $($allBranches.Count) release branches:"
foreach ($b in $allBranches) {
    $tag = Get-ShippedTag ($b -replace 'origin/release/', '')
    if ($tag) {
        Write-Host "   $b   ✅ shipped ($tag)"
    } else {
        Write-Host "   $b   ❌ unshipped (no tag → rolls forward)"
    }
}
Write-Host ""

# Find the previous version's stable branch to use as the "from" boundary for preview.1
# Look for release branches with lower version numbers (skip *.x maintenance branches)
$prevBranch = $null
$allReleaseBranches = @(git branch -r --list "origin/release/*" | ForEach-Object { $_.Trim() })
$prevCandidates = @($allReleaseBranches | Where-Object {
    # Match versioned branches like origin/release/4.148.0 but skip *.x maintenance branches
    $_ -match 'origin/release/\d+\.\d+\.\d+' -and $_ -notmatch '\.x$' -and $_ -notlike "origin/release/$Version*"
})
# Pick the highest version strictly below the target, preferring the bare stable branch
# over its prereleases (e.g. 4.148.0 over 4.148.0-rc.1) so the boundary is the previous
# STABLE cut — otherwise commits from that version's rc→stable window leak into preview.1.
$prevBest = $null
foreach ($candidate in $prevCandidates) {
    $stripped = ($candidate -replace 'origin/release/', '')
    $candidateVersion = $stripped -replace '-.*', ''
    if ([version]$candidateVersion -lt [version]$Version) {
        $ver = [version]$candidateVersion
        $isStable = ($stripped -eq $candidateVersion)  # no -preview/-rc suffix
        if ($null -eq $prevBest -or $ver -gt $prevBest.Ver -or
            ($ver -eq $prevBest.Ver -and $isStable -and -not $prevBest.Stable)) {
            $prevBest = @{ Ver = $ver; Stable = $isStable; Branch = $candidate }
        }
    }
}
if ($prevBest) { $prevBranch = $prevBest.Branch }

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

# Build ranges: each release branch contains commits between its merge-base and the previous one.
# Title is the effective SHIPPED milestone for the range — unshipped previews roll forward to the
# next shipped release; a range with no shipped release yet gets Title = $null (skipped below).
$titles = @($allBranches | ForEach-Object { $_ -replace 'origin/release/', '' })
$ranges = @()

for ($i = 0; $i -lt $allBranches.Count; $i++) {
    $from = if ($i -eq 0) { $prevMergeBase } else { $mergeBases[$i - 1] }
    $ranges += @{
        Branch   = $allBranches[$i]
        OwnTitle = $titles[$i]
        Title    = (Get-EffectiveShippedTitle $i $titles)
        From     = $from
        To       = $mergeBases[$i]
    }
}

# Commits on main after the last release branch belong to the NEXT release, which hasn't been
# cut or shipped yet — never park them in a speculative milestone. They get picked up on a later
# run once that release is cut and tagged.
$lastMb = $mergeBases[$mergeBases.Count - 1]
$headSha = git rev-parse origin/main
if ($lastMb -ne $headSha) {
    $trailing = @(git log --oneline --first-parent "$lastMb..$headSha" 2>/dev/null |
        Where-Object { $_ -match '\(#(\d+)\)' })
    Write-Host "⏭️  $($trailing.Count) commit(s) on main after the last release branch are not in any shipped release yet — skipping until the next release is cut and tagged."
    Write-Host ""
}

$fixed = 0
$correct = 0
$errors = 0

foreach ($range in $ranges) {
    $msTitle = $range.Title

    if ($null -eq $msTitle) {
        Write-Host "⏭️  $($range.OwnTitle) — not shipped yet and nothing later has shipped; skipping (provisional, will assign once a release ships)."
        Write-Host ""
        continue
    }

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

    $rollNote = if ($range.OwnTitle -ne $msTitle) { "  (rolled forward from unshipped $($range.OwnTitle))" } else { "" }
    Write-Host "📦 $msTitle ($($prs.Count) PRs) — $($range.Branch)$rollNote"

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
