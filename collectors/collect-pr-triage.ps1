#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects and analyzes open PRs for triage recommendations.

.DESCRIPTION
    This script fetches open PRs and analyzes them to provide triage recommendations.
    In a future version, this will use AI to analyze PR content.
    Currently uses heuristics based on PR metadata.

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

function Get-TriageCategory {
    param(
        [object]$PR,
        [object]$PRDetails
    )

    $filesChanged = $PRDetails.changed_files
    $additions = $PRDetails.additions
    $deletions = $PRDetails.deletions
    $totalChanges = $additions + $deletions
    $ageInDays = ((Get-Date) - [DateTime]$PR.created_at).TotalDays
    $hasApproval = $false
    $hasChangesRequested = $false

    # Check reviews
    try {
        $reviews = Invoke-GitHubApi "/repos/$Owner/$Repo/pulls/$($PR.number)/reviews"
        $hasApproval = ($reviews | Where-Object { $_.state -eq "APPROVED" } | Measure-Object).Count -gt 0
        $hasChangesRequested = ($reviews | Where-Object { $_.state -eq "CHANGES_REQUESTED" } | Measure-Object).Count -gt 0
    }
    catch {
        Write-Warning "Could not fetch reviews for PR #$($PR.number)"
    }

    $category = "NeedsReview"
    $reasoning = ""

    # Ready to merge: approved and no changes requested
    if ($hasApproval -and -not $hasChangesRequested -and -not $PR.draft) {
        $category = "ReadyToMerge"
        $reasoning = "PR has approval and no changes requested."
    }
    # Needs author response: changes requested
    elseif ($hasChangesRequested) {
        if ($ageInDays -gt 90) {
            $category = "ConsiderClosing"
            $reasoning = "Changes requested over 90 days ago with no response."
        }
        else {
            $category = "NeedsAuthor"
            $reasoning = "Changes have been requested, waiting for author."
        }
    }
    # Consider closing: very old
    elseif ($ageInDays -gt 365) {
        $category = "ConsiderClosing"
        $reasoning = "PR is over a year old and may be stale."
    }
    # Quick review: small and not too old
    elseif ($totalChanges -le 50 -and $ageInDays -lt 30 -and -not $PR.draft) {
        $category = "QuickReview"
        $reasoning = "Small PR ($totalChanges lines) - quick review opportunity."
    }
    # Needs review (default)
    else {
        $category = "NeedsReview"
        if ($PR.draft) {
            $reasoning = "Draft PR - not ready for review yet."
        }
        elseif ($totalChanges -gt 500) {
            $reasoning = "Large PR ($totalChanges lines) requires careful review."
        }
        elseif ($filesChanged -gt 20) {
            $reasoning = "PR touches many files ($filesChanged) and needs thorough review."
        }
        else {
            $reasoning = "PR needs human review to assess quality and fit."
        }
    }

    return @{
        category = $category
        reasoning = $reasoning
    }
}

Write-Host "Collecting PR triage data for $Owner/$Repo..."

# Get open PRs
$openPRs = Invoke-GitHubApi "/repos/$Owner/$Repo/pulls?state=open&per_page=100"

$triagedPRs = @()
$summary = @{
    readyToMerge = 0
    quickReview = 0
    needsReview = 0
    needsAuthor = 0
    considerClosing = 0
}

$bySize = @{
    xs = 0
    s = 0
    m = 0
    l = 0
    xl = 0
}

$byAge = @{
    fresh = 0
    recent = 0
    aging = 0
    stale = 0
    ancient = 0
}

function Get-SizeCategory {
    param([int]$TotalChanges)
    
    if ($TotalChanges -lt 10) { return "xs" }
    if ($TotalChanges -lt 50) { return "s" }
    if ($TotalChanges -lt 200) { return "m" }
    if ($TotalChanges -lt 500) { return "l" }
    return "xl"
}

function Get-AgeCategory {
    param([int]$DaysOpen)
    
    if ($DaysOpen -lt 7) { return "fresh" }
    if ($DaysOpen -lt 30) { return "recent" }
    if ($DaysOpen -lt 90) { return "aging" }
    if ($DaysOpen -lt 365) { return "stale" }
    return "ancient"
}

function Test-MicrosoftAuthor {
    param([string]$Username)
    try {
        $user = Invoke-GitHubApi "/users/$Username"
        $company = $user.company
        if ($company -and ($company -match "Microsoft|@microsoft|MSFT")) {
            return $true
        }
        $orgs = Invoke-GitHubApi "/users/$Username/orgs"
        $msOrgs = @("microsoft", "dotnet", "xamarin", "mono", "azure")
        foreach ($org in $orgs) {
            if ($msOrgs -contains $org.login.ToLower()) {
                return $true
            }
        }
        return $false
    }
    catch {
        return $false
    }
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
        if ($name.StartsWith("type/")) { $result.type = $name.Substring(5) }
        elseif ($name.StartsWith("area/")) { $result.areas += $name.Substring(5) }
        elseif ($name.StartsWith("backend/")) { $result.backends += $name.Substring(8) }
        elseif ($name.StartsWith("os/")) { $result.oses += $name.Substring(3) }
        else { $result.other += $name }
    }
    return $result
}

foreach ($pr in $openPRs) {
    Write-Host "  Analyzing PR #$($pr.number): $($pr.title)"

    # Get PR details (includes files changed, additions, deletions)
    $prDetails = Invoke-GitHubApi "/repos/$Owner/$Repo/pulls/$($pr.number)"
    
    $totalChanges = $prDetails.additions + $prDetails.deletions
    $daysOpen = [math]::Floor(((Get-Date) - [DateTime]$pr.created_at).TotalDays)
    $sizeCategory = Get-SizeCategory -TotalChanges $totalChanges
    $ageCategory = Get-AgeCategory -DaysOpen $daysOpen
    $parsedLabels = Parse-Labels -Labels $pr.labels
    
    # Check if author is Microsoft (cache to avoid repeated API calls)
    $isMicrosoft = Test-MicrosoftAuthor -Username $pr.user.login
    $authorType = if ($pr.user.login -match '\[bot\]$') { "bot" } elseif ($isMicrosoft) { "microsoft" } else { "community" }

    $triage = Get-TriageCategory -PR $pr -PRDetails $prDetails

    $triagedPR = @{
        number = $pr.number
        title = $pr.title
        author = $pr.user.login
        authorAvatarUrl = $pr.user.avatar_url
        authorType = $authorType
        createdAt = $pr.created_at
        updatedAt = $pr.updated_at
        daysOpen = $daysOpen
        ageCategory = $ageCategory
        filesChanged = $prDetails.changed_files
        additions = $prDetails.additions
        deletions = $prDetails.deletions
        totalChanges = $totalChanges
        sizeCategory = $sizeCategory
        isDraft = $pr.draft
        category = $triage.category
        reasoning = $triage.reasoning
        type = $parsedLabels.type
        areas = $parsedLabels.areas
        backends = $parsedLabels.backends
        oses = $parsedLabels.oses
        otherLabels = $parsedLabels.other
        url = $pr.html_url
    }

    $triagedPRs += $triagedPR

    switch ($triage.category) {
        "ReadyToMerge" { $summary.readyToMerge++ }
        "QuickReview" { $summary.quickReview++ }
        "NeedsReview" { $summary.needsReview++ }
        "NeedsAuthor" { $summary.needsAuthor++ }
        "ConsiderClosing" { $summary.considerClosing++ }
    }
    
    $bySize[$sizeCategory]++
    $byAge[$ageCategory]++

    # Rate limiting
    Start-Sleep -Milliseconds 300
}

$bySizeArray = @(
    @{ label = "xs"; display = "< 10 lines"; count = $bySize.xs }
    @{ label = "s"; display = "10-50 lines"; count = $bySize.s }
    @{ label = "m"; display = "50-200 lines"; count = $bySize.m }
    @{ label = "l"; display = "200-500 lines"; count = $bySize.l }
    @{ label = "xl"; display = "> 500 lines"; count = $bySize.xl }
)

$byAgeArray = @(
    @{ label = "fresh"; display = "< 7 days"; count = $byAge.fresh }
    @{ label = "recent"; display = "7-30 days"; count = $byAge.recent }
    @{ label = "aging"; display = "30-90 days"; count = $byAge.aging }
    @{ label = "stale"; display = "90-365 days"; count = $byAge.stale }
    @{ label = "ancient"; display = "> 1 year"; count = $byAge.ancient }
)

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    totalCount = $openPRs.Count
    summary = $summary
    bySize = $bySizeArray
    byAge = $byAgeArray
    pullRequests = $triagedPRs
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "PR triage data written to $OutputPath"
Write-Host "Summary: $($summary.readyToMerge) ready, $($summary.quickReview) quick, $($summary.needsReview) need review, $($summary.needsAuthor) need author, $($summary.considerClosing) consider closing"
