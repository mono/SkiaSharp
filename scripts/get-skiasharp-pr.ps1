#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Download SkiaSharp NuGet packages from a specific PR's build artifacts

.DESCRIPTION
    Downloads SkiaSharp NuGet packages from a specific pull request's latest successful build.
    Uses the Azure DevOps REST API to find the build and download the nuget artifact.

    Packages are installed to ~/.skiasharp/hives/pr-{PRNumber}/packages/ by default.

.PARAMETER PRNumber
    Pull request number (required)

.PARAMETER BuildId
    Build ID to download from (optional - if not specified, finds latest successful build for PR)

.PARAMETER InstallPath
    Directory prefix to install (default: $HOME/.skiasharp on Unix, $env:USERPROFILE\.skiasharp on Windows)
    Packages will be installed to InstallPath/hives/pr-{PRNumber}/packages/

.PARAMETER Filter
    Filter pattern for packages to extract (default: "*-pr.*" to only get PR packages)

.PARAMETER Force
    Overwrite existing packages in the hive

.PARAMETER List
    List available artifacts without downloading

.PARAMETER Help
    Show this help message

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234 -BuildId 12345678

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234 -InstallPath "C:\my-skiasharp"

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234 -Filter "*" -Verbose

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234 -List

.EXAMPLE
    .\get-skiasharp-pr.ps1 1234 -WhatIf

.EXAMPLE
    Piped execution
    iex "& { $(irm https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/get-skiasharp-pr.ps1) } 1234"

.NOTES
    No authentication required - SkiaSharp builds are public.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Position = 0, Mandatory = $true, HelpMessage = "Pull request number")]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PRNumber,

    [Parameter(HelpMessage = "Build ID to download from")]
    [ValidateRange(1, [long]::MaxValue)]
    [long]$BuildId,

    [Parameter(HelpMessage = "Directory prefix to install")]
    [string]$InstallPath = "",

    [Parameter(HelpMessage = "Filter pattern for packages to extract")]
    [string]$Filter = "*-pr.*",

    [Parameter(HelpMessage = "Overwrite existing packages")]
    [switch]$Force,

    [Parameter(HelpMessage = "List available artifacts without downloading")]
    [switch]$List
)

# Constants
$Script:Organization = "xamarin"
$Script:Project = "public"
$Script:PipelineId = 4  # SkiaSharp (Public) pipeline
$Script:PreferredArtifact = "nuget_preview"  # Smaller artifact with only prerelease packages
$Script:FallbackArtifact = "nuget"           # Full artifact (for older builds without nuget_preview)
$Script:BaseUrl = "https://dev.azure.com/$($Script:Organization)/$($Script:Project)/_apis"

# Determine install path
if ([string]::IsNullOrWhiteSpace($InstallPath)) {
    if ($env:HOME) {
        $InstallPath = Join-Path $env:HOME ".skiasharp"
    } elseif ($env:USERPROFILE) {
        $InstallPath = Join-Path $env:USERPROFILE ".skiasharp"
    } else {
        $InstallPath = Join-Path (Get-Location) ".skiasharp"
    }
}

function Write-Message {
    param(
        [string]$Message,
        [ValidateSet("Info", "Success", "Warning", "Error", "Verbose")]
        [string]$Level = "Info"
    )

    switch ($Level) {
        "Info"    { Write-Host $Message }
        "Success" { Write-Host $Message -ForegroundColor Green }
        "Warning" { Write-Warning $Message }
        "Error"   { Write-Error $Message }
        "Verbose" { Write-Verbose $Message }
    }
}

function Invoke-AzDoApi {
    param(
        [string]$Endpoint,
        [string]$ErrorMessage = "API call failed"
    )

    $uri = "$($Script:BaseUrl)/$Endpoint"
    Write-Message "Calling: $uri" -Level Verbose

    try {
        $response = Invoke-RestMethod -Uri $uri -ContentType "application/json"
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 404) {
            throw "$ErrorMessage - Resource not found"
        }
        else {
            throw "$ErrorMessage : $($_.Exception.Message)"
        }
    }
}

function Find-BuildForPR {
    param(
        [int]$PRNumber
    )

    Write-Message "Finding latest build for PR #$PRNumber..." -Level Info

    $sourceBranch = "refs/pull/$PRNumber/merge"
    
    # First try: successful builds
    $endpoint = "build/builds?api-version=7.1&definitions=$($Script:PipelineId)&reasonFilter=pullRequest&statusFilter=completed&resultFilter=succeeded&`$top=20"
    $builds = Invoke-AzDoApi -Endpoint $endpoint -ErrorMessage "Failed to query builds"
    $prBuilds = $builds.value | Where-Object { $_.sourceBranch -eq $sourceBranch }

    if ($prBuilds -and $prBuilds.Count -gt 0) {
        $latestBuild = $prBuilds | Sort-Object -Property finishTime -Descending | Select-Object -First 1
        Write-Message "Found successful build: $($latestBuild.buildNumber) (ID: $($latestBuild.id))" -Level Success
        Write-Message "  Finished: $($latestBuild.finishTime)" -Level Info
        Write-Message "  URL: $($latestBuild._links.web.href)" -Level Info
        return $latestBuild
    }

    # Second try: any completed build (may have failed tests but artifacts could be fine)
    Write-Message "No successful builds found, checking for completed builds..." -Level Warning
    $endpoint = "build/builds?api-version=7.1&definitions=$($Script:PipelineId)&reasonFilter=pullRequest&statusFilter=completed&`$top=20"
    $builds = Invoke-AzDoApi -Endpoint $endpoint -ErrorMessage "Failed to query builds"
    $prBuilds = $builds.value | Where-Object { $_.sourceBranch -eq $sourceBranch }

    if ($prBuilds -and $prBuilds.Count -gt 0) {
        $latestBuild = $prBuilds | Sort-Object -Property finishTime -Descending | Select-Object -First 1
        Write-Message "Found completed build (result: $($latestBuild.result)): $($latestBuild.buildNumber) (ID: $($latestBuild.id))" -Level Warning
        Write-Message "  Finished: $($latestBuild.finishTime)" -Level Info
        Write-Message "  URL: $($latestBuild._links.web.href)" -Level Info
        Write-Message "  Note: Build did not succeed - artifacts may be incomplete" -Level Warning
        return $latestBuild
    }

    # Third try: in-progress builds (artifacts may already be published)
    Write-Message "No completed builds found, checking for in-progress builds..." -Level Warning
    $endpoint = "build/builds?api-version=7.1&definitions=$($Script:PipelineId)&reasonFilter=pullRequest&statusFilter=inProgress&`$top=20"
    $builds = Invoke-AzDoApi -Endpoint $endpoint -ErrorMessage "Failed to query builds"
    $prBuilds = $builds.value | Where-Object { $_.sourceBranch -eq $sourceBranch }

    if ($prBuilds -and $prBuilds.Count -gt 0) {
        $latestBuild = $prBuilds | Sort-Object -Property startTime -Descending | Select-Object -First 1
        Write-Message "Found in-progress build: $($latestBuild.buildNumber) (ID: $($latestBuild.id))" -Level Warning
        Write-Message "  Started: $($latestBuild.startTime)" -Level Info
        Write-Message "  URL: $($latestBuild._links.web.href)" -Level Info
        Write-Message "  Note: Build still running - artifacts may not be available yet" -Level Warning
        return $latestBuild
    }

    throw "No builds found for PR #$PRNumber. Check if the PR exists at: https://dev.azure.com/xamarin/public/_build?definitionId=$($Script:PipelineId)"
}

function Get-BuildArtifacts {
    param(
        [long]$BuildId
    )

    Write-Message "Getting artifacts for build $BuildId..." -Level Verbose

    $endpoint = "build/builds/$BuildId/artifacts?api-version=7.1"
    $artifacts = Invoke-AzDoApi -Endpoint $endpoint -ErrorMessage "Failed to get artifacts"

    return $artifacts.value
}

function Download-Artifact {
    param(
        [object]$Artifact,
        [string]$DownloadPath
    )

    $downloadUrl = $Artifact.resource.downloadUrl
    $zipPath = Join-Path $DownloadPath "$($Artifact.name).zip"

    Write-Message "Downloading $($Artifact.name) artifact..." -Level Info
    Write-Message "  URL: $downloadUrl" -Level Verbose

    if ($PSCmdlet.ShouldProcess($Artifact.name, "Download artifact")) {
        try {
            # Create download directory
            if (-not (Test-Path $DownloadPath)) {
                New-Item -ItemType Directory -Path $DownloadPath -Force | Out-Null
            }

            # Use HttpClient for progress tracking
            $handler = [System.Net.Http.HttpClientHandler]::new()
            $client = [System.Net.Http.HttpClient]::new($handler)
            $client.Timeout = [TimeSpan]::FromMinutes(30)

            try {
                $response = $client.GetAsync($downloadUrl, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).Result
                $response.EnsureSuccessStatusCode() | Out-Null

                $totalBytes = $response.Content.Headers.ContentLength
                $stream = $response.Content.ReadAsStreamAsync().Result
                $fileStream = [System.IO.File]::Create($zipPath)
                
                try {
                    $buffer = New-Object byte[] 81920
                    $totalRead = 0
                    $lastUpdate = [DateTime]::Now

                    while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                        $fileStream.Write($buffer, 0, $read)
                        $totalRead += $read

                        # Update progress every 500ms
                        if (([DateTime]::Now - $lastUpdate).TotalMilliseconds -ge 500) {
                            $downloadedMB = [math]::Round($totalRead / 1MB, 1)
                            if ($totalBytes -and $totalBytes -gt 0) {
                                $totalMB = [math]::Round($totalBytes / 1MB, 1)
                                $percent = [math]::Round(($totalRead / $totalBytes) * 100, 0)
                                Write-Host "`r  Downloading: $downloadedMB MB / $totalMB MB ($percent%)" -NoNewline
                            } else {
                                Write-Host "`r  Downloading: $downloadedMB MB" -NoNewline
                            }
                            $lastUpdate = [DateTime]::Now
                        }
                    }
                    Write-Host ""  # New line after progress
                }
                finally {
                    $fileStream.Close()
                    $stream.Close()
                }
            }
            finally {
                $client.Dispose()
            }

            $fileSize = (Get-Item $zipPath).Length
            $fileSizeMB = [math]::Round($fileSize / 1MB, 1)
            Write-Message "  Downloaded: $zipPath ($fileSizeMB MB)" -Level Success
            return $zipPath
        }
        catch {
            throw "Failed to download artifact: $($_.Exception.Message)"
        }
    }

    return $null
}

function Extract-Packages {
    param(
        [string]$ZipPath,
        [string]$DestinationPath,
        [string]$Filter,
        [switch]$Force
    )

    Write-Message "Extracting packages matching '$Filter'..." -Level Info

    if ($PSCmdlet.ShouldProcess($DestinationPath, "Extract packages")) {
        # Create destination directory
        if (-not (Test-Path $DestinationPath)) {
            New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
        }

        # Extract to temp directory first
        $tempExtract = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-pr-$([System.Guid]::NewGuid().ToString('N'))"
        
        try {
            Expand-Archive -Path $ZipPath -DestinationPath $tempExtract -Force

            # Find and copy matching packages
            $packages = Get-ChildItem -Path $tempExtract -Filter "*.nupkg" -Recurse | 
                        Where-Object { $_.Name -like $Filter }

            if ($packages.Count -eq 0) {
                Write-Message "  No packages found matching filter '$Filter'" -Level Warning
                Write-Message "  Available packages:" -Level Info
                Get-ChildItem -Path $tempExtract -Filter "*.nupkg" -Recurse | ForEach-Object {
                    Write-Message "    $($_.Name)" -Level Info
                }
                return @()
            }

            $copied = @()
            foreach ($pkg in $packages) {
                $destFile = Join-Path $DestinationPath $pkg.Name
                
                if ((Test-Path $destFile) -and -not $Force) {
                    Write-Message "  Skipped (exists): $($pkg.Name)" -Level Verbose
                }
                else {
                    Copy-Item -Path $pkg.FullName -Destination $destFile -Force
                    Write-Message "  Extracted: $($pkg.Name)" -Level Success
                    $copied += $destFile
                }
            }

            return $copied
        }
        finally {
            # Cleanup temp directory
            if (Test-Path $tempExtract) {
                Remove-Item -Path $tempExtract -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
    }

    return @()
}

# Main execution
try {
    Write-Host ""
    Write-Host "SkiaSharp PR Package Downloader" -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host ""

    # Find or use provided build
    if ($BuildId) {
        Write-Message "Using provided build ID: $BuildId" -Level Info
        $build = @{ id = $BuildId }
    }
    else {
        $build = Find-BuildForPR -PRNumber $PRNumber
    }

    # Get artifacts
    $artifacts = Get-BuildArtifacts -BuildId $build.id

    if (-not $artifacts -or $artifacts.Count -eq 0) {
        throw "No artifacts found for build $($build.id)"
    }

    # List mode
    if ($List) {
        Write-Host ""
        Write-Message "Available artifacts:" -Level Info
        foreach ($artifact in $artifacts) {
            Write-Message "  - $($artifact.name)" -Level Info
        }
        exit 0
    }

    # Find the nuget artifact - prefer nuget_preview (smaller), fallback to nuget
    $nugetArtifact = $artifacts | Where-Object { $_.name -eq $Script:PreferredArtifact }
    $usingPreview = $true
    
    if (-not $nugetArtifact) {
        Write-Message "nuget_preview artifact not found, trying full nuget artifact..." -Level Verbose
        $nugetArtifact = $artifacts | Where-Object { $_.name -eq $Script:FallbackArtifact }
        $usingPreview = $false
    }
    
    if (-not $nugetArtifact) {
        Write-Message "Available artifacts:" -Level Warning
        foreach ($artifact in $artifacts) {
            Write-Message "  - $($artifact.name)" -Level Info
        }
        throw "Neither '$($Script:PreferredArtifact)' nor '$($Script:FallbackArtifact)' artifact found. The build may still be in progress."
    }

    $artifactName = $nugetArtifact.name
    Write-Message "Using artifact: $artifactName" -Level Info

    # Setup paths
    $hivePath = Join-Path $InstallPath "hives" "pr-$PRNumber"
    $packagesPath = Join-Path $hivePath "packages"
    $tempPath = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-download"

    Write-Host ""
    Write-Message "Installing to: $packagesPath" -Level Info
    Write-Host ""

    # Download artifact
    $zipPath = Download-Artifact -Artifact $nugetArtifact -DownloadPath $tempPath

    if ($zipPath) {
        # Extract packages (if using nuget_preview, all packages match; otherwise filter)
        $effectiveFilter = if ($usingPreview) { "*.nupkg" } else { $Filter }
        $extractedPackages = Extract-Packages -ZipPath $zipPath -DestinationPath $packagesPath -Filter $effectiveFilter -Force:$Force

        # Cleanup zip
        if (Test-Path $zipPath) {
            Remove-Item -Path $zipPath -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path $tempPath) {
            Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue
        }

        # Summary
        Write-Host ""
        if ($extractedPackages.Count -gt 0) {
            Write-Message "Successfully installed $($extractedPackages.Count) package(s)" -Level Success
            Write-Host ""
            Write-Message "To use these packages, add the folder as a NuGet source:" -Level Info
            Write-Host ""
            Write-Host "  dotnet nuget add source `"$packagesPath`" --name skiasharp-pr-$PRNumber" -ForegroundColor Yellow
            Write-Host ""
            Write-Message "Or add to nuget.config:" -Level Info
            Write-Host ""
            Write-Host "  <add key=`"skiasharp-pr-$PRNumber`" value=`"$packagesPath`" />" -ForegroundColor Yellow
            Write-Host ""
        }
        else {
            Write-Message "No packages were installed" -Level Warning
        }
    }
}
catch {
    Write-Host ""
    Write-Error $_.Exception.Message
    exit 1
}
