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

    # Heuristic-based triage (placeholder for AI analysis)
    $category = "NeedsReview"
    $reasoning = ""

    # Ready to merge indicators
    if ($hasApproval -and -not $hasChangesRequested) {
        $category = "ReadyToMerge"
        $reasoning = "PR has approval and no changes requested."
    }
    elseif ($filesChanged -le 3 -and $totalChanges -le 50 -and $ageInDays -lt 30) {
        $category = "ReadyToMerge"
        $reasoning = "Small PR ($filesChanged files, $totalChanges lines) that appears straightforward."
    }
    # Consider closing indicators
    elseif ($ageInDays -gt 365) {
        $category = "ConsiderClosing"
        $reasoning = "PR is over a year old and may be stale."
    }
    elseif ($hasChangesRequested -and $ageInDays -gt 90) {
        $category = "ConsiderClosing"
        $reasoning = "Changes were requested over 90 days ago with no updates."
    }
    # Needs review (default)
    else {
        $category = "NeedsReview"
        if ($totalChanges -gt 500) {
            $reasoning = "Large PR ($totalChanges lines changed) requires careful review."
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
    needsReview = 0
    considerClosing = 0
}

foreach ($pr in $openPRs) {
    Write-Host "  Analyzing PR #$($pr.number): $($pr.title)"

    # Get PR details (includes files changed, additions, deletions)
    $prDetails = Invoke-GitHubApi "/repos/$Owner/$Repo/pulls/$($pr.number)"

    $triage = Get-TriageCategory -PR $pr -PRDetails $prDetails

    $triagedPR = @{
        number = $pr.number
        title = $pr.title
        author = $pr.user.login
        authorAvatarUrl = $pr.user.avatar_url
        createdAt = $pr.created_at
        filesChanged = $prDetails.changed_files
        additions = $prDetails.additions
        deletions = $prDetails.deletions
        category = $triage.category
        aiReasoning = $triage.reasoning
        labels = @($pr.labels | ForEach-Object { $_.name })
    }

    $triagedPRs += $triagedPR

    switch ($triage.category) {
        "ReadyToMerge" { $summary.readyToMerge++ }
        "NeedsReview" { $summary.needsReview++ }
        "ConsiderClosing" { $summary.considerClosing++ }
    }

    # Rate limiting
    Start-Sleep -Milliseconds 300
}

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    summary = $summary
    pullRequests = $triagedPRs
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "PR triage data written to $OutputPath"
Write-Host "Summary: $($summary.readyToMerge) ready, $($summary.needsReview) need review, $($summary.considerClosing) consider closing"
