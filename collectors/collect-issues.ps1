#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects all open issues with labels for the dashboard.

.DESCRIPTION
    Fetches all open issues (paginated) and categorizes them by label prefixes:
    - type/ (bug, feature-request, enhancement, question, etc.)
    - area/ (specific areas of the codebase)
    - backend/ (rendering backends)
    - os/ (operating systems)

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

function Get-AgeCategory {
    param([int]$DaysOpen)
    
    if ($DaysOpen -lt 7) { return "fresh" }
    if ($DaysOpen -lt 30) { return "recent" }
    if ($DaysOpen -lt 90) { return "aging" }
    if ($DaysOpen -lt 365) { return "stale" }
    return "ancient"
}

function Parse-Labels {
    param([array]$Labels)
    
    $result = @{
        type = $null
        areas = @()
        backends = @()
        oses = @()
        other = @()
    }
    
    foreach ($label in $Labels) {
        $name = $label.name
        
        if ($name.StartsWith("type/")) {
            $result.type = $name.Substring(5)
        }
        elseif ($name.StartsWith("area/")) {
            $result.areas += $name.Substring(5)
        }
        elseif ($name.StartsWith("backend/")) {
            $result.backends += $name.Substring(8)
        }
        elseif ($name.StartsWith("os/")) {
            $result.oses += $name.Substring(3)
        }
        else {
            $result.other += $name
        }
    }
    
    return $result
}

Write-Host "Collecting issues for $Owner/$Repo..."

# Paginate through all open issues
$page = 1
$allIssues = @()

do {
    Write-Host "  Fetching page $page..."
    $issues = Invoke-GitHubApi "/repos/$Owner/$Repo/issues?state=open&per_page=100&page=$page"
    
    # Filter out pull requests (GitHub API returns PRs in issues endpoint)
    $issuesOnly = $issues | Where-Object { -not $_.pull_request }
    $allIssues += $issuesOnly
    
    $page++
    Start-Sleep -Milliseconds 200
} while ($issues.Count -eq 100)

Write-Host "  Found $($allIssues.Count) open issues"

# Process issues
$processedIssues = @()
$byType = @{}
$byArea = @{}
$byBackend = @{}
$byOs = @{}
$byAge = @{
    fresh = 0
    recent = 0
    aging = 0
    stale = 0
    ancient = 0
}

foreach ($issue in $allIssues) {
    $daysOpen = [math]::Floor(((Get-Date) - [DateTime]$issue.created_at).TotalDays)
    $daysSinceActivity = [math]::Floor(((Get-Date) - [DateTime]$issue.updated_at).TotalDays)
    $ageCategory = Get-AgeCategory -DaysOpen $daysOpen
    $parsedLabels = Parse-Labels -Labels $issue.labels
    
    $processed = @{
        number = $issue.number
        title = $issue.title
        author = $issue.user.login
        authorAvatarUrl = $issue.user.avatar_url
        createdAt = $issue.created_at
        updatedAt = $issue.updated_at
        commentCount = $issue.comments
        daysOpen = $daysOpen
        daysSinceActivity = $daysSinceActivity
        ageCategory = $ageCategory
        type = $parsedLabels.type
        areas = $parsedLabels.areas
        backends = $parsedLabels.backends
        oses = $parsedLabels.oses
        otherLabels = $parsedLabels.other
        url = $issue.html_url
    }
    
    $processedIssues += $processed
    
    # Aggregate by type
    $typeKey = if ($parsedLabels.type) { $parsedLabels.type } else { "unlabeled" }
    if (-not $byType.ContainsKey($typeKey)) { $byType[$typeKey] = 0 }
    $byType[$typeKey]++
    
    # Aggregate by area
    foreach ($area in $parsedLabels.areas) {
        if (-not $byArea.ContainsKey($area)) { $byArea[$area] = 0 }
        $byArea[$area]++
    }
    
    # Aggregate by backend
    foreach ($backend in $parsedLabels.backends) {
        if (-not $byBackend.ContainsKey($backend)) { $byBackend[$backend] = 0 }
        $byBackend[$backend]++
    }
    
    # Aggregate by OS
    foreach ($os in $parsedLabels.oses) {
        if (-not $byOs.ContainsKey($os)) { $byOs[$os] = 0 }
        $byOs[$os]++
    }
    
    # Aggregate by age
    $byAge[$ageCategory]++
}

# Convert hashtables to sorted arrays for JSON
$byTypeArray = $byType.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
    @{ label = $_.Key; count = $_.Value }
}

$byAreaArray = $byArea.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
    @{ label = $_.Key; count = $_.Value }
}

$byBackendArray = $byBackend.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
    @{ label = $_.Key; count = $_.Value }
}

$byOsArray = $byOs.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
    @{ label = $_.Key; count = $_.Value }
}

$byAgeArray = @(
    @{ label = "fresh"; display = "< 7 days"; count = $byAge.fresh }
    @{ label = "recent"; display = "7-30 days"; count = $byAge.recent }
    @{ label = "aging"; display = "30-90 days"; count = $byAge.aging }
    @{ label = "stale"; display = "90-365 days"; count = $byAge.stale }
    @{ label = "ancient"; display = "> 1 year"; count = $byAge.ancient }
)

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    totalCount = $allIssues.Count
    byType = $byTypeArray
    byArea = $byAreaArray
    byBackend = $byBackendArray
    byOs = $byOsArray
    byAge = $byAgeArray
    issues = $processedIssues
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Issues data written to $OutputPath"
Write-Host "Summary:"
Write-Host "  Total: $($allIssues.Count)"
Write-Host "  By age: fresh=$($byAge.fresh), recent=$($byAge.recent), aging=$($byAge.aging), stale=$($byAge.stale), ancient=$($byAge.ancient)"
