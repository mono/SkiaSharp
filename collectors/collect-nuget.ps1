#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects NuGet download statistics for SkiaSharp packages.

.PARAMETER OutputPath
    Path to output the JSON file.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

# Dynamically build package list from VERSIONS.txt files in the repo
function Get-PackageListFromVersionsFile {
    param([string]$Url)
    
    try {
        $content = Invoke-RestMethod -Uri $Url -ErrorAction Stop
        # Match lines like "SkiaSharp.Views.WPF                             nuget       2.88.9"
        $matches = [regex]::Matches($content, '^\s*((?:SkiaSharp|HarfBuzzSharp)[^\s]*)\s+nuget\s+', [System.Text.RegularExpressions.RegexOptions]::Multiline)
        return $matches | ForEach-Object { $_.Groups[1].Value }
    }
    catch {
        Write-Warning "Failed to fetch $Url : $_"
        return @()
    }
}

Write-Host "Building package list from VERSIONS.txt files..."

# Fetch from both main and release/2.x branches
$mainUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/VERSIONS.txt"
$releaseUrl = "https://raw.githubusercontent.com/mono/SkiaSharp/release/2.x/VERSIONS.txt"

$mainPackages = Get-PackageListFromVersionsFile -Url $mainUrl
$releasePackages = Get-PackageListFromVersionsFile -Url $releaseUrl

# Union of both lists, sorted
$packages = ($mainPackages + $releasePackages) | Sort-Object -Unique

Write-Host "  Found $($packages.Count) packages to track"

function Get-NuGetStats {
    param([string]$PackageId)

    try {
        # Use the Search API which has reliable download counts
        $searchUrl = "https://azuresearch-usnc.nuget.org/query?q=packageid:$PackageId&prerelease=true&take=1"
        $searchData = Invoke-RestMethod -Uri $searchUrl -ErrorAction Stop

        if ($searchData.data.Count -eq 0) {
            Write-Warning "Package $PackageId not found in search"
            return @{
                id = $PackageId
                totalDownloads = 0
                versions = @()
            }
        }

        $pkg = $searchData.data[0]
        $totalDownloads = $pkg.totalDownloads

        # Get the latest 5 versions with their downloads
        $versionStats = $pkg.versions | Select-Object -Last 5 | ForEach-Object {
            @{
                version = $_.version
                downloads = $_.downloads
            }
        }

        # Sort by version descending (newest first)
        $versionStats = $versionStats | Sort-Object { 
            try { [version]($_.version -replace '-.*', '') } catch { [version]"0.0.0" }
        } -Descending

        return @{
            id = $PackageId
            totalDownloads = $totalDownloads
            versions = $versionStats
        }
    }
    catch {
        Write-Warning "Failed to get stats for $PackageId : $_"
        return @{
            id = $PackageId
            totalDownloads = 0
            versions = @()
        }
    }
}

Write-Host "Collecting NuGet stats..."

$packageStats = @()
$totalDownloads = 0

foreach ($package in $packages) {
    Write-Host "  Fetching $package..."
    $stats = Get-NuGetStats -PackageId $package
    $packageStats += $stats
    $totalDownloads += $stats.totalDownloads

    # Rate limiting - be nice to NuGet API
    Start-Sleep -Milliseconds 300
}

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    totalDownloads = $totalDownloads
    packages = $packageStats
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "NuGet stats written to $OutputPath"
Write-Host "Total downloads across all packages: $($totalDownloads.ToString('N0'))"
