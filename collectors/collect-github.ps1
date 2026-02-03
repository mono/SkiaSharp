#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects GitHub repository statistics for SkiaSharp dashboard.

.PARAMETER OutputPath
    Path to output the JSON file.

.PARAMETER Owner
    Repository owner (default: mono)

.PARAMETER Repo
    Repository name (default: SkiaSharp)
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [string]$Owner = "mono",
    [string]$Repo = "SkiaSharp"
)

$ErrorActionPreference = "Stop"

# Use GitHub token if available
$headers = @{
    "Accept" = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

if ($env:GITHUB_TOKEN) {
    $headers["Authorization"] = "Bearer $env:GITHUB_TOKEN"
}

function Invoke-GitHubApi {
    param([string]$Endpoint)
    $url = "https://api.github.com$Endpoint"
    Invoke-RestMethod -Uri $url -Headers $headers
}

Write-Host "Collecting GitHub stats for $Owner/$Repo..."

# Get repository info
$repoInfo = Invoke-GitHubApi "/repos/$Owner/$Repo"

# Get open issues count (issues endpoint includes PRs, so we filter)
$openIssues = Invoke-GitHubApi "/repos/$Owner/$Repo/issues?state=open&per_page=1"
$openIssueCount = if ($openIssues) { $repoInfo.open_issues_count } else { 0 }

# Get closed issues (sample to estimate)
$closedIssues = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:issue+is:closed&per_page=1"
$closedIssueCount = $closedIssues.total_count

# Get issues opened in last 30 days
$thirtyDaysAgo = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
$recentOpenedIssues = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:issue+created:>=$thirtyDaysAgo&per_page=1"
$openedLast30Days = $recentOpenedIssues.total_count

# Get issues closed in last 30 days
$recentClosedIssues = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:issue+is:closed+closed:>=$thirtyDaysAgo&per_page=1"
$closedLast30Days = $recentClosedIssues.total_count

# Get open PRs
$openPRs = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:pr+is:open&per_page=1"
$openPRCount = $openPRs.total_count

# Get merged PRs
$mergedPRs = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:pr+is:merged&per_page=1"
$mergedPRCount = $mergedPRs.total_count

# Get closed (not merged) PRs
$closedPRs = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:pr+is:closed+is:unmerged&per_page=1"
$closedPRCount = $closedPRs.total_count

# Get PRs merged in last 30 days
$recentMergedPRs = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:pr+is:merged+merged:>=$thirtyDaysAgo&per_page=1"
$mergedLast30Days = $recentMergedPRs.total_count

# Get PRs opened in last 30 days
$recentOpenedPRs = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:pr+created:>=$thirtyDaysAgo&per_page=1"
$openedPRsLast30Days = $recentOpenedPRs.total_count

# Get recent commits
$commits = Invoke-GitHubApi "/repos/$Owner/$Repo/commits?per_page=10"
$recentCommits = $commits | ForEach-Object {
    @{
        sha = $_.sha
        message = ($_.commit.message -split "`n")[0]  # First line only
        author = $_.commit.author.name
        date = $_.commit.author.date
    }
}

# Get commits in last 30 days
$commitsSince = Invoke-GitHubApi "/repos/$Owner/$Repo/commits?since=$thirtyDaysAgo&per_page=100"
$commitsLast30Days = ($commitsSince | Measure-Object).Count

# Get issue labels with counts
$labels = Invoke-GitHubApi "/repos/$Owner/$Repo/labels?per_page=100"
$labelCounts = @()
foreach ($label in ($labels | Select-Object -First 10)) {
    $labelIssues = Invoke-GitHubApi "/search/issues?q=repo:$Owner/$Repo+is:issue+is:open+label:`"$($label.name)`"&per_page=1"
    if ($labelIssues.total_count -gt 0) {
        $labelCounts += @{
            label = $label.name
            count = $labelIssues.total_count
        }
    }
}

# Build output object
$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    repository = @{
        stars = $repoInfo.stargazers_count
        forks = $repoInfo.forks_count
        watchers = $repoInfo.subscribers_count
        openIssues = $openIssueCount
        closedIssues = $closedIssueCount
    }
    issues = @{
        open = $openIssueCount
        closed = $closedIssueCount
        openedLast30Days = $openedLast30Days
        closedLast30Days = $closedLast30Days
        byLabel = $labelCounts
    }
    pullRequests = @{
        open = $openPRCount
        merged = $mergedPRCount
        closed = $closedPRCount
        openedLast30Days = $openedPRsLast30Days
        mergedLast30Days = $mergedLast30Days
    }
    activity = @{
        commitsLast30Days = $commitsLast30Days
        recentCommits = $recentCommits
    }
}

# Write to file
$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "GitHub stats written to $OutputPath"
