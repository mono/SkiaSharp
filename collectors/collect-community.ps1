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

function Test-MicrosoftMember {
    param([string]$Username)
    try {
        # Check user's public profile for Microsoft affiliation
        $user = Invoke-GitHubApi "/users/$Username"
        $company = $user.company
        if ($company -and ($company -match "Microsoft|@microsoft|MSFT")) {
            return $true
        }
        return $false
    }
    catch {
        return $false
    }
}

Write-Host "Collecting community stats for $Owner/$Repo..."

# Get contributors
$contributors = Invoke-GitHubApi "/repos/$Owner/$Repo/contributors?per_page=100"
$totalContributors = ($contributors | Measure-Object).Count

# Check Microsoft membership for top contributors (limit API calls)
Write-Host "Checking Microsoft org membership..."
$microsoftCount = 0
$communityCount = 0

$topContributors = $contributors | Select-Object -First 20 | ForEach-Object {
    $isMicrosoft = Test-MicrosoftMember -Username $_.login
    if ($isMicrosoft) { $script:microsoftCount++ } else { $script:communityCount++ }
    Start-Sleep -Milliseconds 100  # Rate limit protection
    
    @{
        login = $_.login
        avatarUrl = $_.avatar_url
        contributions = $_.contributions
        isMicrosoft = $isMicrosoft
    }
}

# Estimate remaining contributors as community (we only checked top 20)
$remainingContributors = $totalContributors - 20
if ($remainingContributors -gt 0) {
    $communityCount += $remainingContributors
}

Write-Host "Microsoft contributors (top 20): $microsoftCount"
Write-Host "Community contributors: $communityCount"

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
    microsoftContributors = $microsoftCount
    communityContributors = $communityCount
    topContributors = $topContributors
    recentCommits = $recentCommits
    contributorGrowth = $contributorGrowth
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Community stats written to $OutputPath"
Write-Host "Total contributors: $totalContributors (Microsoft: $microsoftCount, Community: $communityCount)"
