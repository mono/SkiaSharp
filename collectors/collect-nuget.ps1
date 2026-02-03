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

# SkiaSharp packages to track
$packages = @(
    "SkiaSharp",
    "SkiaSharp.NativeAssets.Linux",
    "SkiaSharp.NativeAssets.macOS",
    "SkiaSharp.NativeAssets.Win32",
    "SkiaSharp.NativeAssets.WebAssembly",
    "SkiaSharp.Views",
    "SkiaSharp.Views.Maui.Controls",
    "SkiaSharp.Views.Maui.Core",
    "SkiaSharp.Views.WPF",
    "SkiaSharp.Views.WindowsForms",
    "SkiaSharp.Views.Blazor",
    "HarfBuzzSharp",
    "HarfBuzzSharp.NativeAssets.Linux",
    "HarfBuzzSharp.NativeAssets.macOS",
    "HarfBuzzSharp.NativeAssets.Win32"
)

function Get-NuGetStats {
    param([string]$PackageId)

    $url = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLower())/index.json"

    try {
        $versions = Invoke-RestMethod -Uri $url -ErrorAction Stop

        # Get stats for latest versions
        $latestVersions = $versions.versions | Select-Object -Last 5

        # Get registration data for download counts
        $regUrl = "https://api.nuget.org/v3/registration5-gz-semver2/$($PackageId.ToLower())/index.json"
        $regData = Invoke-RestMethod -Uri $regUrl -ErrorAction Stop

        $totalDownloads = 0
        $versionStats = @()

        foreach ($page in $regData.items) {
            if ($page.items) {
                foreach ($item in $page.items) {
                    $totalDownloads += $item.catalogEntry.downloads
                    if ($latestVersions -contains $item.catalogEntry.version) {
                        $versionStats += @{
                            version = $item.catalogEntry.version
                            downloads = $item.catalogEntry.downloads
                        }
                    }
                }
            }
            elseif ($page.'@id') {
                # Need to fetch the page
                $pageData = Invoke-RestMethod -Uri $page.'@id' -ErrorAction Stop
                foreach ($item in $pageData.items) {
                    $totalDownloads += $item.catalogEntry.downloads
                    if ($latestVersions -contains $item.catalogEntry.version) {
                        $versionStats += @{
                            version = $item.catalogEntry.version
                            downloads = $item.catalogEntry.downloads
                        }
                    }
                }
            }
        }

        return @{
            id = $PackageId
            totalDownloads = $totalDownloads
            versions = ($versionStats | Sort-Object { [version]($_.version -replace '-.*', '') } -Descending | Select-Object -First 5)
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
    Start-Sleep -Milliseconds 500
}

$output = @{
    generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    totalDownloads = $totalDownloads
    packages = $packageStats
}

$output | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "NuGet stats written to $OutputPath"
Write-Host "Total downloads across all packages: $($totalDownloads.ToString('N0'))"
