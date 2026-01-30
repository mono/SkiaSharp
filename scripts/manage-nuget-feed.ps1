<#
.SYNOPSIS
    Manages Azure DevOps Artifacts NuGet feed - deletes unwanted versions and moves packages.

.DESCRIPTION
    This script:
    - Deletes package versions with -pr.* suffix
    - Deletes package versions that don't match allowed patterns (-preview.*, -rc.*, -nightly.*, -stable.*, or stable releases)
    - Moves packages starting with _ (e.g., _NuGets, _Symbols) to a destination feed
    - Caches downloaded packages locally to avoid re-downloading on errors
    - STOPS if it encounters an unexpected version pattern (safety first)

.PARAMETER Organization
    Azure DevOps organization name

.PARAMETER Project
    Azure DevOps project name (leave empty for org-scoped feeds)

.PARAMETER SourceFeed
    Source feed name or ID

.PARAMETER DestinationFeed
    Destination feed name or ID (for moving _* packages)

.PARAMETER PAT
    Personal Access Token with Packaging read/write/manage permissions

.PARAMETER CacheDir
    Local directory for caching downloaded packages (default: ./nuget-cache)

.PARAMETER StateFile
    File to track progress for resumability (default: ./feed-management-state.json)

.PARAMETER BatchSize
    Number of packages to fetch per API call (default: 100, max: 1000)

.PARAMETER Execute
    Actually perform deletions and moves. Without this flag, runs in dry-run mode.

.PARAMETER SkipMoveOperations
    Skip the move operations for _* packages (only do deletes)

.PARAMETER SkipDeleteOperations
    Skip the delete operations (only do moves)

.PARAMETER MovePhase
    Control which phase of the move operation to execute:
    - "All" (default): Download, upload, and delete in one pass
    - "CopyOnly": Download and upload to destination, but DON'T delete from source
    - "DeleteOnly": Only delete from source (packages must already be in destination)

.PARAMETER VerifyBeforeDelete
    When using DeleteOnly phase, verify each package exists in destination before deleting from source

.PARAMETER PackageFilter
    Regex pattern to filter which packages to process. Useful for parallel runs.
    Examples: "^_NativeAssets$", "^[A-M]", "^SkiaSharp"

.PARAMETER ParallelJobs
    Number of parallel operations (default: 1 = sequential)

.EXAMPLE
    # Dry run - see what would happen
    ./manage-nuget-feed.ps1 -Organization "myorg" -SourceFeed "myfeed" -DestinationFeed "archive" -PAT $pat

.EXAMPLE
    # Process only _NativeAssets package
    ./manage-nuget-feed.ps1 ... -Execute -PackageFilter "^_NativeAssets$"

.EXAMPLE
    # Run parallel instances (in separate terminals):
    # Terminal 1: ./manage-nuget-feed.ps1 ... -PackageFilter "^_" -StateFile ./state-underscore.json
    # Terminal 2: ./manage-nuget-feed.ps1 ... -PackageFilter "^[A-M]" -StateFile ./state-am.json
    # Terminal 3: ./manage-nuget-feed.ps1 ... -PackageFilter "^[N-Z]" -StateFile ./state-nz.json

.EXAMPLE
    # Phase 1: Copy packages to destination (no delete)
    ./manage-nuget-feed.ps1 -Organization "myorg" -SourceFeed "myfeed" -DestinationFeed "archive" -PAT $pat -Execute -MovePhase CopyOnly

.EXAMPLE
    # Phase 2: Later, after verification, delete from source
    ./manage-nuget-feed.ps1 -Organization "myorg" -SourceFeed "myfeed" -DestinationFeed "archive" -PAT $pat -Execute -MovePhase DeleteOnly -VerifyBeforeDelete

.EXAMPLE
    # Actually execute (all phases at once)
    ./manage-nuget-feed.ps1 -Organization "myorg" -SourceFeed "myfeed" -DestinationFeed "archive" -PAT $pat -Execute
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory = $false)]
    [string]$Organization = "xamarin",

    [Parameter(Mandatory = $false)]
    [string]$Project = "public",  # Leave empty for org-scoped feeds

    [Parameter(Mandatory = $false)]
    [string]$SourceFeed = "SkiaSharp",

    [Parameter(Mandatory = $false)]
    [string]$DestinationFeed = "SkiaSharp-CI",

    [Parameter(Mandatory = $false)]
    [string]$PAT = $env:AZURE_DEVOPS_PAT,

    [Parameter(Mandatory = $false)]
    [string]$CacheDir = "./nuget-cache",

    [Parameter(Mandatory = $false)]
    [string]$StateFile = "./feed-management-state.json",

    [Parameter(Mandatory = $false)]
    [int]$BatchSize = 100,

    [Parameter(Mandatory = $false)]
    [switch]$Execute,

    [Parameter(Mandatory = $false)]
    [switch]$SkipMoveOperations,

    [Parameter(Mandatory = $false)]
    [switch]$SkipDeleteOperations,

    [Parameter(Mandatory = $false)]
    [ValidateSet("All", "CopyOnly", "DeleteOnly")]
    [string]$MovePhase = "All",

    [Parameter(Mandatory = $false)]
    [switch]$VerifyBeforeDelete,

    [Parameter(Mandatory = $false)]
    [string]$PackageFilter = "",  # Regex to filter package IDs (e.g., "^_NativeAssets$" or "^[A-M]")

    [Parameter(Mandatory = $false)]
    [int]$ParallelJobs = 1  # Number of parallel operations (1 = sequential)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

#region Configuration and Constants

# Version pattern definitions
# Main version: X.Y or X.Y.Z or X.Y.Z.W
$MainVersionPattern = '\d+\.\d+(?:\.\d+)?(?:\.\d+)?'

# Allowed prerelease suffixes (with period and numbers after)
$AllowedPrereleasePatterns = @(
    '-alpha\.\d+(?:\.\d+)?'
    '-preview\.\d+(?:\.\d+)?'
    '-rc\.\d+(?:\.\d+)?'
    '-nightly\.\d+(?:\.\d+)?'
    '-stable\.\d+(?:\.\d+)?'
)

# Patterns to DELETE
$DeletePrereleasePatterns = @(
    '-pr\.\d+(?:\.\d+)?'
    '-preview-.*'  # Poorly formatted: -preview- instead of -preview.
)

# Build the complete regex patterns
$StableVersionRegex = "^${MainVersionPattern}$"
$AllowedPrereleaseRegex = $AllowedPrereleasePatterns | ForEach-Object { "^${MainVersionPattern}${_}$" }
$DeletePrereleaseRegex = $DeletePrereleasePatterns | ForEach-Object { "^${MainVersionPattern}${_}$" }

# Combine all known patterns for validation
$AllKnownPatterns = @($StableVersionRegex) + $AllowedPrereleaseRegex + $DeletePrereleaseRegex

#endregion

#region Helper Functions

# Global cancel flag
$script:CancelRequested = $false
$script:IsInteractive = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected

function Test-CancelRequested {
    # Check if user pressed 'Q' or 'q' to request cancel (only in interactive mode)
    if ($script:IsInteractive) {
        try {
            if ([Console]::KeyAvailable) {
                $key = [Console]::ReadKey($true)
                if ($key.Key -eq 'Q') {
                    $script:CancelRequested = $true
                    Write-Host ""
                    Write-Host "Cancel requested - finishing current operation..." -ForegroundColor Yellow
                    return $true
                }
            }
        } catch {
            # Ignore console errors in non-interactive environments
        }
    }
    return $script:CancelRequested
}

function Write-Status {
    param(
        [string]$Message,
        [string]$ForegroundColor = "White"
    )
    # Overwrite current line (handle CI where WindowWidth may be 0)
    $consoleWidth = try { [Console]::WindowWidth } catch { 0 }
    $width = [Math]::Max(80, [Math]::Min($consoleWidth - 1, 120))
    $paddedMsg = $Message.PadRight($width).Substring(0, $width)
    Write-Host "`r$paddedMsg" -NoNewline -ForegroundColor $ForegroundColor
}

function Write-ProgressSummary {
    param(
        [int]$Current,
        [int]$Total,
        [string]$CurrentPackage,
        [string]$Operation,
        [int]$PackageVersionCurrent = 0,
        [int]$PackageVersionTotal = 0
    )
    
    # Overall progress
    Write-Progress -Id 0 -Activity "Overall Progress" -Status "$Current of $Total packages" -PercentComplete (($Current / [Math]::Max($Total, 1)) * 100)
    
    # Current package progress (if applicable)
    if ($PackageVersionTotal -gt 0) {
        Write-Progress -Id 1 -ParentId 0 -Activity $CurrentPackage -Status "$Operation ($PackageVersionCurrent / $PackageVersionTotal)" -PercentComplete (($PackageVersionCurrent / [Math]::Max($PackageVersionTotal, 1)) * 100)
    }
}

function Get-Base64Auth {
    param([string]$Pat)
    $bytes = [System.Text.Encoding]::ASCII.GetBytes(":$Pat")
    return [Convert]::ToBase64String($bytes)
}

function Invoke-AzDoApi {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers,
        [int]$MaxRetries = 5
    )

    $retryCount = 0
    $baseDelay = 2

    while ($retryCount -lt $MaxRetries) {
        try {
            $params = @{
                Uri         = $Uri
                Method      = $Method
                Headers     = $Headers
                ContentType = "application/json"
            }

            if ($Body) {
                $params.Body = ($Body | ConvertTo-Json -Depth 10)
            }

            $response = Invoke-RestMethod @params
            return $response
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            
            # Rate limiting or server errors - retry
            if ($statusCode -in @(429, 500, 502, 503, 504)) {
                $retryCount++
                $delay = $baseDelay * [Math]::Pow(2, $retryCount)
                
                if ($statusCode -eq 429) {
                    # Check for Retry-After header
                    $retryAfter = $_.Exception.Response.Headers["Retry-After"]
                    if ($retryAfter) {
                        $delay = [int]$retryAfter
                    }
                }
                
                Write-Warning "API call failed with status $statusCode. Retrying in $delay seconds... (Attempt $retryCount/$MaxRetries)"
                Start-Sleep -Seconds $delay
            }
            else {
                throw
            }
        }
    }

    throw "API call failed after $MaxRetries retries"
}

function Test-VersionPattern {
    param([string]$Version)

    # Strip build metadata before pattern matching
    $versionToMatch = Get-VersionWithoutMetadata -Version $Version

    # Check if version matches any known pattern
    foreach ($pattern in $AllKnownPatterns) {
        if ($versionToMatch -match $pattern) {
            return @{
                IsKnown = $true
                Pattern = $pattern
            }
        }
    }

    return @{
        IsKnown = $false
        Pattern = $null
    }
}

function Get-VersionWithoutMetadata {
    param([string]$Version)
    
    # Strip build metadata (+anything) from version
    # SemVer: 1.2.3-preview.1+build.123 -> 1.2.3-preview.1
    $plusIndex = $Version.IndexOf('+')
    if ($plusIndex -gt 0) {
        return $Version.Substring(0, $plusIndex)
    }
    return $Version
}

function Get-VersionAction {
    param([string]$Version)

    # Strip build metadata before pattern matching
    $versionToMatch = Get-VersionWithoutMetadata -Version $Version

    # First, check if it's a stable version (no prerelease)
    if ($versionToMatch -match $StableVersionRegex) {
        return "KEEP"
    }

    # Check if it matches allowed prerelease patterns
    foreach ($pattern in $AllowedPrereleaseRegex) {
        if ($versionToMatch -match $pattern) {
            return "KEEP"
        }
    }

    # Check if it matches delete patterns
    foreach ($pattern in $DeletePrereleaseRegex) {
        if ($versionToMatch -match $pattern) {
            return "DELETE"
        }
    }

    # Unknown pattern - this should stop the script
    return "UNKNOWN"
}

function Get-PackageAction {
    param([string]$PackageName)

    if ($PackageName.StartsWith("_")) {
        return "MOVE"
    }

    return "PROCESS_VERSIONS"
}

function Get-FeedBaseUrl {
    param(
        [string]$Org,
        [string]$Proj,
        [string]$UrlType  # "feeds" or "pkgs"
    )

    $subdomain = if ($UrlType -eq "feeds") { "feeds" } else { "pkgs" }
    
    if ([string]::IsNullOrEmpty($Proj)) {
        return "https://${subdomain}.dev.azure.com/${Org}"
    }
    else {
        return "https://${subdomain}.dev.azure.com/${Org}/${Proj}"
    }
}

function Get-AllPackages {
    param(
        [string]$FeedsBaseUrl,
        [string]$FeedId,
        [hashtable]$Headers,
        [int]$BatchSize
    )

    $allPackages = @()
    $skip = 0
    $hasMore = $true

    Write-Host "Fetching packages from feed..." -ForegroundColor Cyan

    while ($hasMore) {
        $uri = "${FeedsBaseUrl}/_apis/packaging/Feeds/${FeedId}/packages?api-version=7.1&protocolType=NuGet&includeAllVersions=true&`$top=${BatchSize}&`$skip=${skip}"
        
        Write-Host "  Fetching packages $skip to $($skip + $BatchSize)..." -ForegroundColor Gray
        
        $response = Invoke-AzDoApi -Uri $uri -Headers $Headers
        
        if ($response.value -and $response.value.Count -gt 0) {
            $allPackages += $response.value
            $skip += $BatchSize
            
            Write-Host "    Found $($response.value.Count) packages (total: $($allPackages.Count))" -ForegroundColor Gray
            
            # If we got fewer than BatchSize, we've reached the end
            if ($response.value.Count -lt $BatchSize) {
                $hasMore = $false
            }
        }
        else {
            $hasMore = $false
        }

        # Small delay to be nice to the API
        Start-Sleep -Milliseconds 100
    }

    Write-Host "Total packages found: $($allPackages.Count)" -ForegroundColor Green
    return $allPackages
}

function Get-CachePath {
    param(
        [string]$CacheDir,
        [string]$PackageId,
        [string]$Version
    )

    $safeName = $PackageId -replace '[^\w\-\.]', '_'
    $safeVersion = $Version -replace '[^\w\-\.]', '_'
    return Join-Path $CacheDir $safeName $safeVersion
}

function Get-CachedPackagePath {
    param(
        [string]$CacheDir,
        [string]$PackageId,
        [string]$Version
    )

    $cachePath = Get-CachePath -CacheDir $CacheDir -PackageId $PackageId -Version $Version
    $nupkgPath = Join-Path $cachePath "${PackageId}.${Version}.nupkg"
    
    if (Test-Path $nupkgPath) {
        return $nupkgPath
    }
    
    return $null
}

function Save-PackageToCache {
    param(
        [string]$CacheDir,
        [string]$PackageId,
        [string]$Version,
        [string]$DownloadUrl,
        [hashtable]$Headers
    )

    $cachePath = Get-CachePath -CacheDir $CacheDir -PackageId $PackageId -Version $Version
    $nupkgPath = Join-Path $cachePath "${PackageId}.${Version}.nupkg"
    $tempPath = Join-Path $cachePath "${PackageId}.${Version}.nupkg.downloading"

    # Check if already cached
    if (Test-Path $nupkgPath) {
        Write-Host "    Package already cached: $nupkgPath" -ForegroundColor Gray
        return $nupkgPath
    }

    # Clean up any partial downloads
    if (Test-Path $tempPath) {
        Remove-Item $tempPath -Force
    }

    # Create cache directory
    if (-not (Test-Path $cachePath)) {
        New-Item -ItemType Directory -Path $cachePath -Force | Out-Null
    }

    Write-Host "    Downloading to cache..." -ForegroundColor Gray
    
    # Use HttpWebRequest to handle 303 redirects properly
    # Azure DevOps returns 303 redirect to blob storage
    try {
        $request = [System.Net.HttpWebRequest]::Create($DownloadUrl)
        $request.Method = "GET"
        $request.Headers.Add("Authorization", $Headers.Authorization)
        $request.AllowAutoRedirect = $true  # Follow 303 redirects
        $request.MaximumAutomaticRedirections = 5
        $request.Timeout = 600000  # 10 minute timeout for large files
        
        $response = $request.GetResponse()
        $responseStream = $response.GetResponseStream()
        
        $fileStream = [System.IO.File]::Create($tempPath)
        try {
            $buffer = New-Object byte[] 65536  # 64KB buffer
            $bytesRead = 0
            while (($bytesRead = $responseStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                $fileStream.Write($buffer, 0, $bytesRead)
            }
        }
        finally {
            $fileStream.Close()
            $responseStream.Close()
            $response.Close()
        }
        
        # Verify it's a valid ZIP before renaming
        try {
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            $zip = [System.IO.Compression.ZipFile]::OpenRead($tempPath)
            $zip.Dispose()
        }
        catch {
            throw "Downloaded file is not a valid package (ZIP verification failed)"
        }
        
        # Rename temp to final
        Move-Item $tempPath $nupkgPath -Force
        return $nupkgPath
    }
    catch {
        Write-Warning "    Failed to download package: $_"
        if (Test-Path $tempPath) {
            Remove-Item $tempPath -Force
        }
        throw
    }
}

function Push-PackageToFeed {
    param(
        [string]$NupkgPath,
        [string]$FeedUrl,
        [string]$PAT
    )

    # Use dotnet nuget push
    $pushUrl = "${FeedUrl}/nuget/v3/index.json"
    
    $result = & dotnet nuget push $NupkgPath --source $pushUrl --api-key "az" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        # Check if it's a "package already exists" error - that's OK
        if ($result -match "already exists|409|Conflict") {
            Write-Host "    Package already exists in destination feed (OK)" -ForegroundColor Yellow
            return $true
        }
        throw "Failed to push package: $result"
    }
    
    return $true
}

function Remove-PackageVersion {
    param(
        [string]$PkgsBaseUrl,
        [string]$FeedId,
        [string]$PackageId,
        [string]$Version,
        [hashtable]$Headers
    )

    $uri = "${PkgsBaseUrl}/_apis/packaging/feeds/${FeedId}/nuget/packages/${PackageId}/versions/${Version}?api-version=7.1"
    
    $null = Invoke-AzDoApi -Uri $uri -Method "DELETE" -Headers $Headers
}

function Test-PackageExistsInFeed {
    param(
        [string]$PkgsBaseUrl,
        [string]$FeedId,
        [string]$PackageId,
        [string]$Version,
        [hashtable]$Headers
    )

    $uri = "${PkgsBaseUrl}/_apis/packaging/feeds/${FeedId}/nuget/packages/${PackageId}/versions/${Version}?api-version=7.1"
    
    try {
        $response = Invoke-AzDoApi -Uri $uri -Headers $Headers
        return ($null -ne $response)
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 404) {
            return $false
        }
        throw
    }
}

function Test-NupkgIntegrity {
    param([string]$NupkgPath)

    # NuGet packages are ZIP files - verify we can open and read the .nuspec
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($NupkgPath)
        
        # Find .nuspec file
        $nuspecEntry = $zip.Entries | Where-Object { $_.Name -like "*.nuspec" } | Select-Object -First 1
        
        if (-not $nuspecEntry) {
            $zip.Dispose()
            return @{ Valid = $false; Error = "No .nuspec file found in package" }
        }
        
        # Try to read nuspec content
        $stream = $nuspecEntry.Open()
        $reader = New-Object System.IO.StreamReader($stream)
        $content = $reader.ReadToEnd()
        $reader.Close()
        $stream.Close()
        $zip.Dispose()
        
        # Basic XML validation
        try {
            [xml]$xml = $content
            $packageId = $xml.package.metadata.id
            $packageVersion = $xml.package.metadata.version
            
            # Compute SHA256 hash of the file
            $sha256 = Get-FileHash -Path $NupkgPath -Algorithm SHA256
            
            return @{ 
                Valid = $true
                PackageId = $packageId
                Version = $packageVersion
                SHA256 = $sha256.Hash
                FileSize = (Get-Item $NupkgPath).Length
            }
        }
        catch {
            return @{ Valid = $false; Error = "Invalid nuspec XML: $_" }
        }
    }
    catch {
        return @{ Valid = $false; Error = "Failed to open package: $_" }
    }
}

function Save-State {
    param(
        [string]$StateFile,
        [hashtable]$State
    )

    $State | ConvertTo-Json -Depth 10 | Set-Content $StateFile -Encoding UTF8
}

function Get-State {
    param([string]$StateFile)

    if (Test-Path $StateFile) {
        return Get-Content $StateFile -Raw | ConvertFrom-Json -AsHashtable
    }

    return @{
        ProcessedPackages = @{}
        DeletedVersions   = @()
        MovedPackages     = @()
        CopiedPackages    = @()  # Tracks packages copied to dest but not yet deleted from source
        PackageHashes     = @{}  # Stores SHA256 hashes: "PackageId@Version" -> hash
        Errors            = @()
        StartTime         = (Get-Date).ToString("o")
    }
}

#endregion

#region Main Script

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Azure DevOps NuGet Feed Manager" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate parameters
if ($Organization -eq "YOUR_ORGANIZATION" -or $SourceFeed -eq "YOUR_SOURCE_FEED") {
    Write-Error "Please provide valid Organization and SourceFeed parameters"
    exit 1
}

if (-not $PAT) {
    Write-Error "PAT is required. Set via -PAT parameter or AZURE_DEVOPS_PAT environment variable"
    exit 1
}

if (-not $SkipMoveOperations -and $DestinationFeed -eq "YOUR_DESTINATION_FEED") {
    Write-Error "Please provide valid DestinationFeed parameter or use -SkipMoveOperations"
    exit 1
}

# Display mode
if ($Execute) {
    Write-Host "MODE: EXECUTE - Changes WILL be made!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Press Ctrl+C within 5 seconds to cancel..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}
else {
    Write-Host "MODE: DRY RUN - No changes will be made" -ForegroundColor Green
    Write-Host "Use -Execute flag to actually perform operations" -ForegroundColor Gray
}
Write-Host ""

# Setup
$auth = Get-Base64Auth -Pat $PAT
$headers = @{
    Authorization = "Basic $auth"
}

$feedsBaseUrl = Get-FeedBaseUrl -Org $Organization -Proj $Project -UrlType "feeds"
$pkgsBaseUrl = Get-FeedBaseUrl -Org $Organization -Proj $Project -UrlType "pkgs"

# Resolve to absolute paths
$CacheDir = [System.IO.Path]::GetFullPath($CacheDir)
$StateFile = [System.IO.Path]::GetFullPath($StateFile)

Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  Organization: $Organization" -ForegroundColor Gray
Write-Host "  Project: $(if ($Project) { $Project } else { '(org-scoped)' })" -ForegroundColor Gray
Write-Host "  Source Feed: $SourceFeed" -ForegroundColor Gray
Write-Host "  Destination Feed: $DestinationFeed" -ForegroundColor Gray
Write-Host "  Cache Directory: $CacheDir" -ForegroundColor Gray
Write-Host "  State File: $StateFile" -ForegroundColor Gray
if ($PackageFilter) {
    Write-Host "  Package Filter: $PackageFilter" -ForegroundColor Yellow
}
Write-Host ""

# Create cache directory
if (-not (Test-Path $CacheDir)) {
    New-Item -ItemType Directory -Path $CacheDir -Force | Out-Null
}

# Load state
$state = Get-State -StateFile $StateFile

# Validate cache integrity
Write-Host "Validating cache..." -ForegroundColor Cyan
$cacheFiles = @(Get-ChildItem -Path $CacheDir -Filter "*.nupkg" -Recurse -ErrorAction SilentlyContinue)
$cacheValid = 0
$cacheInvalid = 0
$cacheRemoved = 0

if ($cacheFiles.Count -gt 0) {
    $cacheIndex = 0
    foreach ($file in $cacheFiles) {
        $cacheIndex++
        Write-Progress -Activity "Validating cache" -Status $file.Name -PercentComplete (($cacheIndex / $cacheFiles.Count) * 100)
        
        try {
            # Quick ZIP validation - try to open it
            Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
            $zip = [System.IO.Compression.ZipFile]::OpenRead($file.FullName)
            $hasNuspec = ($zip.Entries | Where-Object { $_.Name -like "*.nuspec" } | Measure-Object).Count -gt 0
            $zip.Dispose()
            
            if ($hasNuspec) {
                $cacheValid++
            }
            else {
                throw "No nuspec found"
            }
        }
        catch {
            $cacheInvalid++
            Write-Host "  Removing invalid cache file: $($file.Name)" -ForegroundColor Yellow
            Remove-Item $file.FullName -Force -ErrorAction SilentlyContinue
            $cacheRemoved++
        }
    }
    Write-Progress -Activity "Validating cache" -Completed
    
    Write-Host "  Valid: $cacheValid, Invalid/Removed: $cacheRemoved" -ForegroundColor $(if ($cacheRemoved -gt 0) { "Yellow" } else { "Green" })
}
else {
    Write-Host "  Cache is empty" -ForegroundColor Gray
}

# Also clean up any partial downloads (.downloading files)
$partialFiles = @(Get-ChildItem -Path $CacheDir -Filter "*.downloading" -Recurse -ErrorAction SilentlyContinue)
if ($partialFiles.Count -gt 0) {
    Write-Host "  Removing $($partialFiles.Count) partial downloads..." -ForegroundColor Yellow
    $partialFiles | Remove-Item -Force -ErrorAction SilentlyContinue
}

Write-Host ""

# Get all packages
$packages = Get-AllPackages -FeedsBaseUrl $feedsBaseUrl -FeedId $SourceFeed -Headers $headers -BatchSize $BatchSize

# Apply package filter if specified
if ($PackageFilter) {
    $originalCount = $packages.Count
    $packages = @($packages | Where-Object { $_.name -match $PackageFilter })
    Write-Host "Package filter '$PackageFilter': $($packages.Count) of $originalCount packages match" -ForegroundColor Cyan
}

# Analyze packages
Write-Host ""
Write-Host "Analyzing packages..." -ForegroundColor Cyan

$packagesToMove = @()
$versionsToDelete = @()
$versionsToKeep = @()
$unknownVersions = @()

$totalVersions = 0
$processedCount = 0

foreach ($package in $packages) {
    $processedCount++
    $packageName = $package.name
    $packageAction = Get-PackageAction -PackageName $packageName

    Write-Progress -Activity "Analyzing packages" -Status "$packageName" -PercentComplete (($processedCount / $packages.Count) * 100)

    if ($packageAction -eq "MOVE") {
        # Package starts with _ - mark all versions for move
        foreach ($version in $package.versions) {
            $totalVersions++
            $packagesToMove += @{
                PackageId = $packageName
                Version   = $version.version
            }
        }
    }
    else {
        # Process each version
        foreach ($version in $package.versions) {
            $totalVersions++
            $versionString = $version.version
            $versionAction = Get-VersionAction -Version $versionString

            switch ($versionAction) {
                "KEEP" {
                    $versionsToKeep += @{
                        PackageId = $packageName
                        Version   = $versionString
                    }
                }
                "DELETE" {
                    $versionsToDelete += @{
                        PackageId = $packageName
                        Version   = $versionString
                    }
                }
                "UNKNOWN" {
                    $unknownVersions += @{
                        PackageId = $packageName
                        Version   = $versionString
                    }
                }
            }
        }
    }
}

Write-Progress -Activity "Analyzing packages" -Completed

# Report findings
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Analysis Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total packages: $($packages.Count)" -ForegroundColor White
Write-Host "Total versions: $totalVersions" -ForegroundColor White
Write-Host ""
Write-Host "  Versions to KEEP:   $($versionsToKeep.Count)" -ForegroundColor Green
Write-Host "  Versions to DELETE: $($versionsToDelete.Count)" -ForegroundColor Red
Write-Host "  Packages to MOVE:   $($packagesToMove.Count) versions in $($packagesToMove | Select-Object -ExpandProperty PackageId -Unique | Measure-Object | Select-Object -ExpandProperty Count) packages" -ForegroundColor Yellow
Write-Host "  UNKNOWN versions:   $($unknownVersions.Count)" -ForegroundColor Magenta
Write-Host ""

# Check for unknown versions - STOP if any found
if ($unknownVersions.Count -gt 0) {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "STOPPING: Unknown version patterns found!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "The following versions do not match any known pattern:" -ForegroundColor Red
    Write-Host ""
    
    $unknownVersions | Group-Object { $_.PackageId } | ForEach-Object {
        Write-Host "  $($_.Name):" -ForegroundColor Yellow
        $_.Group | ForEach-Object {
            Write-Host "    - $($_.Version)" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Expected patterns:" -ForegroundColor Cyan
    Write-Host "  KEEP: X.Y[.Z[.W]] (stable)" -ForegroundColor Green
    Write-Host "  KEEP: X.Y[.Z[.W]]-alpha.N[.N]" -ForegroundColor Green
    Write-Host "  KEEP: X.Y[.Z[.W]]-preview.N[.N]" -ForegroundColor Green
    Write-Host "  KEEP: X.Y[.Z[.W]]-rc.N[.N]" -ForegroundColor Green
    Write-Host "  KEEP: X.Y[.Z[.W]]-nightly.N[.N]" -ForegroundColor Green
    Write-Host "  KEEP: X.Y[.Z[.W]]-stable.N[.N]" -ForegroundColor Green
    Write-Host "  DELETE: X.Y[.Z[.W]]-pr.N[.N]" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please review these versions and update the script patterns if needed." -ForegroundColor Yellow
    
    # Save unknown versions to a file for review
    $unknownFile = "./unknown-versions.json"
    $unknownVersions | ConvertTo-Json -Depth 10 | Set-Content $unknownFile -Encoding UTF8
    Write-Host "Unknown versions saved to: $unknownFile" -ForegroundColor Gray
    
    exit 1
}

# Show samples of what will be deleted
if ($versionsToDelete.Count -gt 0) {
    Write-Host "Sample versions to DELETE (first 20):" -ForegroundColor Red
    $versionsToDelete | Select-Object -First 20 | ForEach-Object {
        Write-Host "  $($_.PackageId)@$($_.Version)" -ForegroundColor Gray
    }
    if ($versionsToDelete.Count -gt 20) {
        Write-Host "  ... and $($versionsToDelete.Count - 20) more" -ForegroundColor Gray
    }
    Write-Host ""
}

# Show packages to move
if ($packagesToMove.Count -gt 0) {
    Write-Host "Packages to MOVE (starting with _):" -ForegroundColor Yellow
    $packagesToMove | Group-Object { $_.PackageId } | ForEach-Object {
        Write-Host "  $($_.Name): $($_.Count) versions" -ForegroundColor Gray
    }
    Write-Host ""
}

# If dry run, exit here
if (-not $Execute) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "DRY RUN COMPLETE" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "To execute these changes, run again with -Execute flag" -ForegroundColor Yellow
    Write-Host ""
    exit 0
}

# Execute operations
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Executing Operations" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$deleteCount = 0
$moveCount = 0
$errorCount = 0

# Process moves first (download, push, delete)
if (-not $SkipMoveOperations -and $packagesToMove.Count -gt 0) {
    
    # Determine what phases to execute
    $doDownloadAndUpload = $MovePhase -in @("All", "CopyOnly")
    $doDeleteFromSource = $MovePhase -in @("All", "DeleteOnly")
    
    if ($MovePhase -eq "CopyOnly") {
        Write-Host "PHASE: Copy Only - packages will be copied to destination but NOT deleted from source" -ForegroundColor Yellow
    }
    elseif ($MovePhase -eq "DeleteOnly") {
        Write-Host "PHASE: Delete Only - packages will be deleted from source (must already exist in destination)" -ForegroundColor Yellow
    }
    else {
        Write-Host "Moving packages to destination feed (full move)..." -ForegroundColor Yellow
    }
    Write-Host ""
    
    # NuGet push requires pkgs.dev.azure.com, not feeds.dev.azure.com
    $destFeedUrl = "${pkgsBaseUrl}/_packaging/${DestinationFeed}"
    
    # Configure NuGet source for destination feed (only if uploading)
    $sourceConfigured = $false
    if ($doDownloadAndUpload) {
        try {
            # Remove existing source if present
            & dotnet nuget remove source "AzDoDestFeed" 2>$null
            
            # Add the source
            & dotnet nuget add source $destFeedUrl/nuget/v3/index.json --name "AzDoDestFeed" --username "az" --password $PAT --store-password-in-clear-text
            $sourceConfigured = $true
            Write-Host "  Configured NuGet source: $destFeedUrl/nuget/v3/index.json" -ForegroundColor Gray
        }
        catch {
            Write-Warning "Could not configure NuGet source: $_"
        }
    }

    $copyCount = 0
    $copySkipped = 0
    $deleteFromSourceCount = 0
    $moveIndex = 0
    $totalMoves = $packagesToMove.Count
    
    Write-Host ""
    Write-Host "Press 'Q' at any time to cancel after current operation completes" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($item in $packagesToMove) {
        $moveIndex++
        $packageId = $item.PackageId
        $version = $item.Version
        $stateKey = "${packageId}@${version}"

        # Check for cancel request
        if (Test-CancelRequested) {
            Write-Host ""
            Write-Host "Cancelled by user after $moveIndex items" -ForegroundColor Yellow
            break
        }

        # Update progress
        Write-Progress -Id 0 -Activity "Moving packages" -Status "$packageId@$version" -PercentComplete (($moveIndex / $totalMoves) * 100) -CurrentOperation "$moveIndex of $totalMoves"

        try {
            # PHASE: Download and Upload
            if ($doDownloadAndUpload) {
                # Check if already copied (in state as "CopiedPackages")
                if ($state.CopiedPackages -contains $stateKey) {
                    Write-Status "  [$moveIndex/$totalMoves] Skipped (cached): $stateKey" -ForegroundColor Gray
                    $copySkipped++
                }
                else {
                    # Check if package already exists in destination feed (skip download if so)
                    Write-Status "  [$moveIndex/$totalMoves] Checking destination: $packageId@$version" -ForegroundColor Gray
                    $existsInDest = Test-PackageExistsInFeed -PkgsBaseUrl $pkgsBaseUrl -FeedId $DestinationFeed -PackageId $packageId -Version $version -Headers $headers
                    
                    if ($existsInDest) {
                        Write-Status "  [$moveIndex/$totalMoves] Already in destination: $packageId@$version" -ForegroundColor Green
                        $copySkipped++
                        
                        # Track as copied so we don't check again
                        if (-not $state.CopiedPackages) { $state.CopiedPackages = @() }
                        $state.CopiedPackages += $stateKey
                        Save-State -StateFile $StateFile -State $state
                    }
                    else {
                        Write-Status "  [$moveIndex/$totalMoves] Downloading: $packageId@$version" -ForegroundColor Yellow

                        # Download URL
                        $downloadUrl = "${pkgsBaseUrl}/_apis/packaging/feeds/${SourceFeed}/nuget/packages/${packageId}/versions/${version}/content?api-version=7.1-preview.1"

                        # Download to cache (or use existing cached file)
                        $cachedPath = Save-PackageToCache -CacheDir $CacheDir -PackageId $packageId -Version $version -DownloadUrl $downloadUrl -Headers $headers

                        # Verify package integrity
                        Write-Status "  [$moveIndex/$totalMoves] Verifying: $packageId@$version" -ForegroundColor Yellow
                        $integrity = Test-NupkgIntegrity -NupkgPath $cachedPath
                        if (-not $integrity.Valid) {
                            throw "Package integrity check failed: $($integrity.Error)"
                        }

                        # Store hash for later verification
                        if (-not $state.PackageHashes) { $state.PackageHashes = @{} }
                        $state.PackageHashes[$stateKey] = @{
                            SHA256 = $integrity.SHA256
                            FileSize = $integrity.FileSize
                            CachedPath = $cachedPath
                            DownloadTime = (Get-Date).ToString("o")
                        }

                        # Push to destination feed
                        Write-Status "  [$moveIndex/$totalMoves] Pushing: $packageId@$version" -ForegroundColor Yellow
                        if ($sourceConfigured) {
                            Push-PackageToFeed -NupkgPath $cachedPath -FeedUrl $destFeedUrl -PAT $PAT
                        }
                        else {
                            throw "Cannot push without NuGet source configured"
                        }

                        $copyCount++
                        
                        # Track as copied (separate from moved, for phased migration)
                        if (-not $state.CopiedPackages) { $state.CopiedPackages = @() }
                        $state.CopiedPackages += $stateKey
                        Save-State -StateFile $StateFile -State $state
                        
                        Write-Status "  [$moveIndex/$totalMoves] Copied: $packageId@$version [SHA256: $($integrity.SHA256.Substring(0,12))...]" -ForegroundColor Green
                    }
                }
            }

            # PHASE: Delete from source
            if ($doDeleteFromSource) {
                # Skip if already fully moved
                if ($state.MovedPackages -contains $stateKey) {
                    Write-Status "  [$moveIndex/$totalMoves] Skipped (moved): $stateKey" -ForegroundColor Gray
                    continue
                }

                # Verify exists in destination before deleting (if requested)
                if ($VerifyBeforeDelete) {
                    Write-Status "  [$moveIndex/$totalMoves] Verifying in dest: $packageId@$version" -ForegroundColor Yellow
                    $existsInDest = Test-PackageExistsInFeed -PkgsBaseUrl $pkgsBaseUrl -FeedId $DestinationFeed -PackageId $packageId -Version $version -Headers $headers
                    
                    if (-not $existsInDest) {
                        Write-Host ""
                        Write-Warning "    Package NOT found in destination feed - SKIPPING DELETE: $stateKey"
                        $state.Errors += @{
                            Time    = (Get-Date).ToString("o")
                            Package = $packageId
                            Version = $version
                            Action  = "VERIFY_BEFORE_DELETE"
                            Error   = "Package not found in destination feed"
                        }
                        Save-State -StateFile $StateFile -State $state
                        continue
                    }
                }

                # Delete from source
                Write-Status "  [$moveIndex/$totalMoves] Deleting from source: $packageId@$version" -ForegroundColor Red
                Remove-PackageVersion -PkgsBaseUrl $pkgsBaseUrl -FeedId $SourceFeed -PackageId $packageId -Version $version -Headers $headers
                
                $deleteFromSourceCount++
                $moveCount++
                $state.MovedPackages += $stateKey
                Save-State -StateFile $StateFile -State $state
            }
            
            Write-Host ""  # New line after each package
        }
        catch {
            $errorCount++
            Write-Host ""
            Write-Warning "Failed to process ${packageId}@${version}: $_"
            $state.Errors += @{
                Time    = (Get-Date).ToString("o")
                Package = $packageId
                Version = $version
                Action  = "MOVE_$MovePhase"
                Error   = $_.ToString()
            }
            Save-State -StateFile $StateFile -State $state
        }

        # Rate limiting
        Start-Sleep -Milliseconds 200
    }

    # Clean up NuGet source
    if ($sourceConfigured) {
        & dotnet nuget remove source "AzDoDestFeed" 2>$null
    }
    
    Write-Host ""
    Write-Host "Move phase summary:" -ForegroundColor Cyan
    if ($doDownloadAndUpload) {
        Write-Host "  Copied to destination: $copyCount (skipped: $copySkipped)" -ForegroundColor Green
    }
    if ($doDeleteFromSource) {
        Write-Host "  Deleted from source: $deleteFromSourceCount" -ForegroundColor Red
    }
}

# Process deletes
if (-not $SkipDeleteOperations -and $versionsToDelete.Count -gt 0 -and -not $script:CancelRequested) {
    Write-Host ""
    Write-Host "Deleting package versions..." -ForegroundColor Red
    Write-Host "Press 'Q' at any time to cancel after current operation completes" -ForegroundColor Cyan
    Write-Host ""
    
    $deleteIndex = 0
    $totalDeletes = $versionsToDelete.Count
    
    foreach ($item in $versionsToDelete) {
        $deleteIndex++
        $packageId = $item.PackageId
        $version = $item.Version

        # Check for cancel request
        if (Test-CancelRequested) {
            Write-Host ""
            Write-Host "Cancelled by user after $deleteIndex items" -ForegroundColor Yellow
            break
        }

        # Skip if already processed
        $stateKey = "${packageId}@${version}"
        if ($state.DeletedVersions -contains $stateKey) {
            Write-Status "  [$deleteIndex/$totalDeletes] Skipped (deleted): $stateKey" -ForegroundColor Gray
            continue
        }

        Write-Progress -Id 0 -Activity "Deleting versions" -Status "$packageId@$version" -PercentComplete (($deleteIndex / $totalDeletes) * 100) -CurrentOperation "$deleteIndex of $totalDeletes"
        Write-Status "  [$deleteIndex/$totalDeletes] Deleting: $packageId@$version" -ForegroundColor Red

        try {
            Remove-PackageVersion -PkgsBaseUrl $pkgsBaseUrl -FeedId $SourceFeed -PackageId $packageId -Version $version -Headers $headers
            
            $deleteCount++
            $state.DeletedVersions += $stateKey
            
            # Save state every 10 deletes
            if ($deleteCount % 10 -eq 0) {
                Save-State -StateFile $StateFile -State $state
            }
        }
        catch {
            $errorCount++
            Write-Host ""
            Write-Warning "Failed to delete ${packageId}@${version}: $_"
            $state.Errors += @{
                Time    = (Get-Date).ToString("o")
                Package = $packageId
                Version = $version
                Action  = "DELETE"
                Error   = $_.ToString()
            }
        }

        # Rate limiting
        Start-Sleep -Milliseconds 100
    }

    Write-Progress -Activity "Deleting versions" -Completed
}

# Final state save
$state.EndTime = (Get-Date).ToString("o")
Save-State -StateFile $StateFile -State $state

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Execution Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Versions deleted: $deleteCount" -ForegroundColor Red
Write-Host "  Packages moved:   $moveCount" -ForegroundColor Yellow
Write-Host "  Errors:           $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
Write-Host ""
Write-Host "State saved to: $StateFile" -ForegroundColor Gray
Write-Host "Cache directory: $CacheDir" -ForegroundColor Gray
Write-Host ""

if ($errorCount -gt 0) {
    Write-Host "Some operations failed. Review the state file for details." -ForegroundColor Yellow
    Write-Host "Re-run the script to retry failed operations." -ForegroundColor Yellow
}

#endregion
