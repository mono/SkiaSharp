#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects community/contributor statistics for SkiaSharp.

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

Write-Host "Collecting community stats for $Owner/$Repo..."

# Get contributors
$contributors = Invoke-GitHubApi "/repos/$Owner/$Repo/contributors?per_page=100"
$totalContributors = ($contributors | Measure-Object).Count

# Get top contributors with details
$topContributors = $contributors | Select-Object -First 20 | ForEach-Object {
    @{
        login = $_.login
        avatarUrl = $_.avatar_url
        contributions = $_.contributions
    }
}

# Get recent commits with author info
$commits = Invoke-GitHubApi "/repos/$Owner/$Repo/commits?per_page=20"
$recentCommits = $commits | ForEach-Object {
    @{
        sha = $_.sha
        message = ($_.commit.message -split "`n")[0]
        author = if ($_.author) { $_.author.login } else { $_.commit.author.name }
        date = $_.commit.author.date
    }
}

# Estimate contributor growth (simplified - count unique authors per month for last 6 months)
$contributorGrowth = @()
for ($i = 5; $i -ge 0; $i--) {
    $monthStart = (Get-Date).AddMonths(-$i).ToString("yyyy-MM-01")
    $monthEnd = (Get-Date).AddMonths(-$i + 1).ToString("yyyy-MM-01")

    try {
        $monthCommits = Invoke-GitHubApi "/repos/$Owner/$Repo/commits?since=$monthStart&until=$monthEnd&per_page=100"
        $uniqueAuthors = ($monthCommits | ForEach-Object {
            if ($_.author) { $_.author.login } else { $_.commit.author.email }
        } | Sort-Object -Unique | Measure-Object).Count

        $contributorGrowth += @{
            date = $monthStart
            count = $uniqueAuthors
        }
    }
    catch {
        Write-Warning "Could not get commits for $monthStart"
    }

    Start-Sleep -Milliseconds 200
}

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    totalContributors = $totalContributors
    topContributors = $topContributors
    recentCommits = $recentCommits
    contributorGrowth = $contributorGrowth
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Community stats written to $OutputPath"
Write-Host "Total contributors: $totalContributors"
