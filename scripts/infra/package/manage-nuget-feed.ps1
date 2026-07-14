<#
.SYNOPSIS
    Mirror (copy) SkiaSharp NuGet packages from the old xamarin Azure DevOps
    feeds to the new dnceng feeds. Pick a feed with -Feed; the script knows the
    source, destination, and the per-feed rules intrinsically.

.DESCRIPTION
    Copies every GOOD package version that exists in the source feed but is
    missing from the destination feed. It is:

      * Rules-driven — you only choose the feed (-Feed skiasharp | skiasharp-ci).
                      The script encodes each feed's source, destination and
                      cleanliness rules (see FEED PROFILES below).
      * Clean by default — known-bad versions (per-PR '-pr.*' builds and
                      malformed '-preview-*') are NEVER copied. We do not mirror
                      junk.
      * Diff-first  — it inventories both feeds (metadata only, no downloads)
                      and computes exactly what is missing BEFORE transferring
                      anything. This is the fast "what is already there" check.
      * Idempotent  — the destination feed IS the state. Re-running only pushes
                      what is still missing; anything already present is skipped.
                      Safe to run at any time, as many times as you like.
      * Opt-in push — DRY RUN by default. Nothing is written unless you pass
                      -Push. Dry run still does the full inventory + diff so you
                      can see exactly what a real run would transfer.
      * Timeout-safe — honours -MaxDurationMinutes and stops GRACEFULLY between
                      packages (never mid-push). Whatever was copied stays valid;
                      the next run continues where this one left off.
      * Corruption-safe — every downloaded package is verified (valid zip + a
                      readable .nuspec whose id/version match what we expected)
                      BEFORE it is pushed. Corrupt downloads are skipped, never
                      pushed. Existing destination packages are never overwritten
                      (409/"already exists" is treated as success).

    FEED PROFILES (intrinsic — this is "the difference" between the feeds):

      skiasharp     PUBLIC release feed.
                    xamarin/public/SkiaSharp  ->  dnceng/public/skiasharp
                    Excludes the internal underscore-prefixed CI wrapper packages
                    (_NativeAssets, _NuGets, _Symbols, ...) so the public feed only
                    ever contains real, user-facing packages.

      skiasharp-ci  INTERNAL CI artifact feed.
                    xamarin/public/SkiaSharp-CI  ->  dnceng/public/skiasharp-ci
                    Keeps the underscore wrapper/artifact packages (used by the
                    build pipeline and externals-download). Still drops the bad
                    '-pr.*' / malformed versions like every feed.

    Read access to the (public) source feed is anonymous; only the PUSH to the
    destination needs a PAT (packaging read/write on the destination org).

.PARAMETER Feed
    Which feed to mirror: 'skiasharp' (public) or 'skiasharp-ci' (internal).
    This selects the source, destination and per-feed rules. Required.

.PARAMETER Push
    OPT IN. Actually download+push missing packages. Without this flag the
    script runs a DRY RUN (inventory + diff + plan only).

    AUTH (environment variables only — never command-line arguments):
      AZURE_DEVOPS_PAT        Packaging read/write on the DESTINATION org.
                              Required only for -Push.
      AZURE_DEVOPS_SOURCE_PAT Optional; only if the SOURCE feed is not public.

    SECURITY: the PAT is never passed on a command line, never written to a
    nuget.config, and never printed. For the push it is handed to the Azure
    Artifacts Credential Provider via the VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
    process env var (with the on-disk session-token cache disabled); for REST
    reads (only if the source feed is private) it goes in an Authorization
    header. It is also registered with the GitHub Actions log masker. Dry runs
    need no PAT at all (public feeds are read anonymously).

.PARAMETER MaxDurationMinutes
    Wall-clock budget. The script stops starting new work once this is reached
    and exits cleanly. Default: 330 (leaves headroom under a ~350-min CI timeout).

.PARAMETER PackageFilter
    Advanced. Optional regex to limit which package ids are considered
    (e.g. '^SkiaSharp$' to smoke-test the push path on one package).

.PARAMETER IncludeUnlisted
    Also mirror versions that are unlisted in the source. Default: off.

.PARAMETER MaxPushRetries
    Per-package download+push attempts before giving up on that package and
    moving on. Default: 3.

.PARAMETER CacheDir
    Directory for downloaded .nupkg files. Default: a temp folder. Downloads are
    reused across runs.

.PARAMETER BatchSize
    Page size for feed inventory listing. Default: 100.

.PARAMETER SummaryFile
    Optional path to append a GitHub-flavoured Markdown summary of the diff and
    plan (what would be copied). In CI, point this at $env:GITHUB_STEP_SUMMARY so
    the result shows up in the PR check summary.

.EXAMPLE
    # Dry run — see exactly what WOULD be copied to the public feed (safe)
    ./manage-nuget-feed.ps1 -Feed skiasharp

.EXAMPLE
    # Real mirror of the public feed
    $env:AZURE_DEVOPS_PAT = '<pat>'
    ./manage-nuget-feed.ps1 -Feed skiasharp -Push

.EXAMPLE
    # Real mirror of the internal CI artifact feed
    ./manage-nuget-feed.ps1 -Feed skiasharp-ci -Push

.EXAMPLE
    # Smoke-test the push path end to end on a single package first
    ./manage-nuget-feed.ps1 -Feed skiasharp -PackageFilter '^SkiaSharp$' -Push

.NOTES
    Exit codes:
      0  success — everything synced, dry run completed, or gracefully timed out
         with progress and no errors (safe to re-run to continue)
      1  one or more packages failed after retries (re-run to retry just those)
      2  fatal setup error (bad args, feed unreachable, missing PAT for -Push)
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("skiasharp", "skiasharp-ci")]
    [string]$Feed,

    [switch]$Push,

    [ValidateRange(13, 1440)]
    [int]$MaxDurationMinutes = 330,
    [string]$PackageFilter = "",
    [switch]$IncludeUnlisted,

    [ValidateRange(1, 10)]
    [int]$MaxPushRetries = 3,
    [string]$CacheDir = "",
    [ValidateRange(1, 1000)]
    [int]$BatchSize = 100,
    [string]$SummaryFile = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Secrets come ONLY from the environment — never as a command-line argument (so a
# PAT can never end up in shell history or the process command line).
#   AZURE_DEVOPS_PAT        - packaging read/write on the destination org (push).
#   AZURE_DEVOPS_SOURCE_PAT - optional, only if the SOURCE feed is not public.
$Pat = $env:AZURE_DEVOPS_PAT
$SourcePat = $env:AZURE_DEVOPS_SOURCE_PAT

#region Helpers

# --- Version classification: which package versions are "real releases" vs
# --- ephemeral CI build artifacts. Bad ones ('-pr.*', malformed '-preview-*')
# --- are never copied.
$script:MainVersionPattern = '\d+\.\d+(?:\.\d+)?(?:\.\d+)?'
$script:AllowedPrereleaseSuffixes = @(
    '-alpha\.\d+(?:\.\d+)?'
    '-preview\.\d+(?:\.\d+)?'
    '-rc\.\d+(?:\.\d+)?'
    '-nightly\.\d+(?:\.\d+)?'
    '-stable\.\d+(?:\.\d+)?'
)
$script:DeletePrereleaseSuffixes = @(
    '-pr\..*'       # any per-PR build (e.g. -pr.1234.5, -pr.1.2.3, -pr.anything)
    '-preview-.*'   # malformed: -preview- instead of -preview.
)
$script:StableRegex = "^$($script:MainVersionPattern)$"
$script:AllowedRegex = $script:AllowedPrereleaseSuffixes | ForEach-Object { "^$($script:MainVersionPattern)$_`$" }
$script:DeleteRegex  = $script:DeletePrereleaseSuffixes  | ForEach-Object { "^$($script:MainVersionPattern)$_`$" }

function Get-VersionAction {
    param([string]$Version)
    # Strip build metadata (+...) before matching.
    $v = $Version
    $plus = $v.IndexOf('+')
    if ($plus -gt 0) { $v = $v.Substring(0, $plus) }

    if ($v -match $script:StableRegex) { return "KEEP" }
    foreach ($p in $script:AllowedRegex) { if ($v -match $p) { return "KEEP" } }
    foreach ($p in $script:DeleteRegex)  { if ($v -match $p) { return "DELETE" } }
    return "UNKNOWN"
}

function Write-Info { param([string]$M) Write-Host $M -ForegroundColor Gray }
function Write-Head { param([string]$M) Write-Host $M -ForegroundColor Cyan }
function Write-Good { param([string]$M) Write-Host $M -ForegroundColor Green }
function Write-Warn2 { param([string]$M) Write-Host $M -ForegroundColor Yellow }
function Write-Err2 { param([string]$M) Write-Host $M -ForegroundColor Red }

# Markdown summary accumulator (flushed to -SummaryFile / $GITHUB_STEP_SUMMARY).
$script:SummaryLines = [System.Collections.Generic.List[string]]::new()
function Add-Summary { param([string]$Line = "") $script:SummaryLines.Add($Line) }
function Save-Summary {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return }
    try { Add-Content -Path $Path -Value ($script:SummaryLines -join "`n") -Encoding UTF8 }
    catch { Write-Warn2 "  (could not write summary file '$Path': $_)" }
}

function New-AuthHeaders {
    param([string]$TokenPat)
    if ([string]::IsNullOrWhiteSpace($TokenPat)) {
        return @{}
    }
    $bytes = [System.Text.Encoding]::ASCII.GetBytes(":$TokenPat")
    $b64 = [Convert]::ToBase64String($bytes)
    return @{ Authorization = "Basic $b64" }
}

function Get-FeedBaseUrl {
    param(
        [string]$Org,
        [string]$Proj,
        [ValidateSet("feeds", "pkgs")]
        [string]$UrlType
    )
    $sub = if ($UrlType -eq "feeds") { "feeds" } else { "pkgs" }
    if ([string]::IsNullOrEmpty($Proj)) {
        return "https://${sub}.dev.azure.com/${Org}"
    }
    return "https://${sub}.dev.azure.com/${Org}/${Proj}"
}

function Invoke-AzDoApi {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [int]$MaxRetries = 5
    )
    $attempt = 0
    $baseDelay = 2
    while ($true) {
        try {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -ContentType "application/json"
        }
        catch {
            $status = $null
            try { $status = $_.Exception.Response.StatusCode.value__ } catch { }
            if ($status -in @(429, 500, 502, 503, 504) -and $attempt -lt $MaxRetries) {
                $attempt++
                $delay = $baseDelay * [Math]::Pow(2, $attempt)
                if ($status -eq 429) {
                    try {
                        $ra = $_.Exception.Response.Headers["Retry-After"]
                        if ($ra) { $delay = [int]$ra }
                    } catch { }
                }
                Write-Warn2 "  API $status on $Method $Uri — retrying in $delay s ($attempt/$MaxRetries)"
                Start-Sleep -Seconds $delay
                continue
            }
            throw
        }
    }
}

# Determine working read auth for a feed: try ANONYMOUS first (public feeds work
# without a token, and an expired/misscoped PAT must not break a read-only dry
# run), then fall back to the PAT only if anonymous is denied.
function Resolve-ReadHeaders {
    param(
        [string]$FeedsBaseUrl,
        [string]$FeedId,
        [string]$Pat
    )
    $probe = "${FeedsBaseUrl}/_apis/packaging/Feeds/${FeedId}/packages?api-version=7.1&protocolType=NuGet&`$top=1"
    try {
        $null = Invoke-RestMethod -Uri $probe -Method GET
        return @{}   # anonymous works
    }
    catch {
        if (-not [string]::IsNullOrWhiteSpace($Pat)) {
            $h = New-AuthHeaders -TokenPat $Pat
            $null = Invoke-RestMethod -Uri $probe -Method GET -Headers $h  # throws if this also fails
            return $h
        }
        throw
    }
}

# Inventory a feed: returns a list of @{ Id; Version; NVer } for all (non-deleted)
# package versions. Metadata only — no package downloads.
function Get-FeedInventory {
    param(
        [string]$FeedsBaseUrl,
        [string]$FeedId,
        [hashtable]$Headers,
        [int]$BatchSize,
        [switch]$IncludeUnlisted
    )
    $result = [System.Collections.Generic.List[object]]::new()
    $skip = 0
    while ($true) {
        $uri = "${FeedsBaseUrl}/_apis/packaging/Feeds/${FeedId}/packages?api-version=7.1&protocolType=NuGet&includeAllVersions=true&`$top=${BatchSize}&`$skip=${skip}"
        $resp = Invoke-AzDoApi -Uri $uri -Headers $Headers
        $page = @($resp.value)
        if ($page.Count -eq 0) { break }
        foreach ($pkg in $page) {
            $pkgName = $pkg.name
            $versions = @()
            if ($pkg.PSObject.Properties.Name -contains 'versions' -and $pkg.versions) {
                $versions = @($pkg.versions)
            }
            foreach ($v in $versions) {
                # Skip deleted versions — they cannot be downloaded.
                if (($v.PSObject.Properties.Name -contains 'isDeleted') -and $v.isDeleted) { continue }
                if (-not $IncludeUnlisted -and ($v.PSObject.Properties.Name -contains 'isListed') -and (-not $v.isListed)) { continue }
                $ver = $v.version
                $nver = if ($v.PSObject.Properties.Name -contains 'normalizedVersion' -and $v.normalizedVersion) { $v.normalizedVersion } else { $ver }
                $result.Add([pscustomobject]@{
                    Id = $pkgName
                    Version = $ver
                    NVer = $nver
                })
            }
        }
        if ($page.Count -lt $BatchSize) { break }
        $skip += $BatchSize
        Start-Sleep -Milliseconds 100
    }
    # Unary comma prevents PowerShell from unrolling the list (which would turn an
    # empty result into $null and a populated one into a bare array).
    return ,$result
}

function Get-EntryKey {
    param([string]$Id, [string]$NVer)
    return ("{0}@{1}" -f $Id, $NVer).ToLowerInvariant()
}

# Download a package version (follows AzDO 303 -> blob redirect) and verify it is
# a real zip. Returns the local path, or throws.
function Save-FeedPackage {
    param(
        [string]$PkgsBaseUrl,
        [string]$FeedId,
        [string]$Id,
        [string]$Version,
        [hashtable]$Headers,
        [string]$CacheDir
    )
    if (-not (Test-Path $CacheDir)) { New-Item -ItemType Directory -Path $CacheDir -Force | Out-Null }
    $safe = ("{0}.{1}.nupkg" -f $Id, $Version)
    $nupkg = Join-Path $CacheDir $safe
    $tmp = "$nupkg.downloading"

    if (Test-Path $tmp) { Remove-Item $tmp -Force }

    # Reuse a previously cached, still-valid download.
    if (Test-Path $nupkg) {
        if (Test-IsValidZip -Path $nupkg) { return $nupkg }
        Remove-Item $nupkg -Force
    }

    $url = "${PkgsBaseUrl}/_apis/packaging/feeds/${FeedId}/nuget/packages/${Id}/versions/${Version}/content?api-version=7.1-preview.1"

    $req = [System.Net.HttpWebRequest]::Create($url)
    $req.Method = "GET"
    if ($Headers.ContainsKey('Authorization')) { $req.Headers.Add("Authorization", $Headers.Authorization) }
    $req.AllowAutoRedirect = $true
    $req.MaximumAutomaticRedirections = 5
    $req.Timeout = 600000            # 10 min headers timeout
    $req.ReadWriteTimeout = 600000   # 10 min socket read timeout

    $resp = $req.GetResponse()
    $rs = $resp.GetResponseStream()
    $fs = [System.IO.File]::Create($tmp)
    try {
        $buffer = New-Object byte[] 65536
        while (($read = $rs.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $fs.Write($buffer, 0, $read)
        }
    }
    finally {
        $fs.Close(); $rs.Close(); $resp.Close()
    }

    if (-not (Test-IsValidZip -Path $tmp)) {
        Remove-Item $tmp -Force -ErrorAction SilentlyContinue
        throw "downloaded file is not a valid zip"
    }
    Move-Item $tmp $nupkg -Force
    return $nupkg
}

function Test-IsValidZip {
    param([string]$Path)
    # Validate the whole archive, not just the central directory: decompress every
    # entry so a truncated/corrupt payload (bad CRC, short read) is caught before
    # the package is ever pushed.
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
        $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
        try {
            $buffer = New-Object byte[] 65536
            foreach ($entry in $zip.Entries) {
                # Directory entries have no content.
                if ($entry.FullName.EndsWith('/')) { continue }
                $stream = $entry.Open()
                try {
                    while ($stream.Read($buffer, 0, $buffer.Length) -gt 0) { }
                }
                finally { $stream.Dispose() }
            }
        }
        finally { $zip.Dispose() }
        return $true
    }
    catch { return $false }
}

# Verify the .nupkg is a valid package whose nuspec id/version match what we
# intended to copy. Guards against corrupt or mismatched downloads.
function Test-NupkgMatches {
    param(
        [string]$Path,
        [string]$ExpectId,
        [string]$ExpectVersion
    )
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
        $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
        try {
            # The package manifest is the root-level .nuspec (no directory in its
            # FullName). Avoid matching a nested *.nuspec bundled as content.
            $nuspec = $zip.Entries |
                Where-Object { $_.Name -like "*.nuspec" -and $_.FullName -eq $_.Name } |
                Select-Object -First 1
            if (-not $nuspec) {
                $nuspec = $zip.Entries | Where-Object { $_.Name -like "*.nuspec" } | Select-Object -First 1
            }
            if (-not $nuspec) { return @{ Ok = $false; Reason = "no .nuspec in package" } }
            $reader = New-Object System.IO.StreamReader($nuspec.Open())
            $content = $reader.ReadToEnd()
            $reader.Close()
        }
        finally { $zip.Dispose() }

        [xml]$xml = $content
        $id = "$($xml.package.metadata.id)"
        $ver = "$($xml.package.metadata.version)"
        if ($id.ToLowerInvariant() -ne $ExpectId.ToLowerInvariant()) {
            return @{ Ok = $false; Reason = "nuspec id '$id' != expected '$ExpectId'" }
        }
        # Compare on normalized-ish basis: exact, else case-insensitive.
        if ($ver.ToLowerInvariant() -ne $ExpectVersion.ToLowerInvariant()) {
            return @{ Ok = $false; Reason = "nuspec version '$ver' != expected '$ExpectVersion'" }
        }
        return @{ Ok = $true; Reason = ""; Id = $id; Version = $ver }
    }
    catch {
        return @{ Ok = $false; Reason = "cannot read package: $_" }
    }
}

# Configure destination push credentials WITHOUT writing the PAT to disk or to
# any command line. Uses the Azure Artifacts Credential Provider, which reads the
# VSS_NUGET_EXTERNAL_FEED_ENDPOINTS process environment variable. The PAT lives
# only in this process's memory/env for the duration of the run — never in a
# nuget.config, never as a `dotnet` argument, never echoed. We also register it
# with the GitHub log masker as belt-and-suspenders.
function Set-PushCredentials {
    param(
        [string]$IndexUrl,
        [string]$Pat
    )
    if ([string]::IsNullOrWhiteSpace($Pat)) { return }

    # Defensive masking. GitHub already masks configured secrets; this also covers
    # a -SourcePat / -Pat supplied by other means so it can never surface in logs.
    if ($env:GITHUB_ACTIONS -eq 'true') {
        Write-Host ("::add-mask::{0}" -f $Pat)
    }

    $endpoints = @{
        endpointCredentials = @(
            @{ endpoint = $IndexUrl; username = "az"; password = $Pat }
        )
    }
    # -Compress keeps it single-line; this value is consumed only by the credential
    # provider plugin and is never printed by this script.
    $env:VSS_NUGET_EXTERNAL_FEED_ENDPOINTS = ($endpoints | ConvertTo-Json -Depth 5 -Compress)

    # Do NOT let the Azure Artifacts Credential Provider persist session tokens to
    # disk — the credential should live only in this process's memory.
    $env:NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED = "false"
}

function Clear-PushCredentials {
    Remove-Item Env:\VSS_NUGET_EXTERNAL_FEED_ENDPOINTS -ErrorAction SilentlyContinue
}

# Push a single package. Auth comes from the credential provider (see
# Set-PushCredentials) — no secret is passed here. Output is scrubbed of the PAT
# before it is ever returned/logged. Exit 0 = success; because --skip-duplicate
# makes a genuine duplicate exit 0, a NON-zero exit is always treated as a real
# failure (never masked as "already present").
function Publish-FeedPackage {
    param(
        [string]$NupkgPath,
        [string]$IndexUrl,
        [string]$ScrubValue = ""
    )
    $out = & dotnet nuget push $NupkgPath --source $IndexUrl --api-key az --skip-duplicate --timeout 300 2>&1 | Out-String
    if (-not [string]::IsNullOrEmpty($ScrubValue)) {
        $out = $out.Replace($ScrubValue, "***")
    }
    if ($LASTEXITCODE -eq 0) {
        $skipped = ($out -match "already exists|Conflict|Skipping|duplicate")
        return @{ Ok = $true; Skipped = $skipped; Out = $out }
    }
    return @{ Ok = $false; Skipped = $false; Out = $out }
}

#endregion

#region Main

# --- Feed profiles: intrinsic source/destination + per-feed cleanliness rules ---
# VersionPolicy:
#   'ReleasesOnly' — ALLOWLIST: push only recognized releases (KEEP). Anything
#                    not recognized as a release is refused. Used for the PUBLIC
#                    feed so nothing unexpected can ever land there.
#   'ExcludeBad'   — DENYLIST: push everything EXCEPT known-bad versions
#                    (-pr.* / malformed -preview-*). Keeps the 0.0.0-commit.* /
#                    0.0.0-branch.* build artifacts. Used for the INTERNAL feed.
$FeedProfiles = @{
    'skiasharp' = @{
        Kind                = 'public release feed'
        SourceOrg           = 'xamarin'; SourceProject = 'public'; SourceFeed = 'SkiaSharp'
        DestOrg             = 'dnceng';  DestProject   = 'public'; DestFeed   = 'skiasharp'
        # Public feed: drop the internal underscore-prefixed CI wrapper packages,
        # and only ever publish recognized releases (allowlist).
        ExcludePackageRegex = '^_'
        VersionPolicy       = 'ReleasesOnly'
    }
    'skiasharp-ci' = @{
        Kind                = 'internal CI artifact feed'
        SourceOrg           = 'xamarin'; SourceProject = 'public'; SourceFeed = 'SkiaSharp-CI'
        DestOrg             = 'dnceng';  DestProject   = 'public'; DestFeed   = 'skiasharp-ci'
        # Internal feed: keep the underscore wrapper/artifact packages and the
        # commit/branch build versions; only drop the known-bad ones.
        ExcludePackageRegex = ''
        VersionPolicy       = 'ExcludeBad'
    }
}
$feedProfile = $FeedProfiles[$Feed]

$SourceOrg     = $feedProfile.SourceOrg
$SourceProject = $feedProfile.SourceProject
$SourceFeed    = $feedProfile.SourceFeed
$DestOrg       = $feedProfile.DestOrg
$DestProject   = $feedProfile.DestProject
$DestFeed      = $feedProfile.DestFeed
$ExcludePackageRegex = $feedProfile.ExcludePackageRegex
$VersionPolicy = $feedProfile.VersionPolicy

Write-Host ""
Write-Head "=================================================="
Write-Head " NuGet Feed Mirror — $Feed ($($feedProfile.Kind))"
Write-Head "=================================================="
Write-Host ""

$srcFeedsUrl = Get-FeedBaseUrl -Org $SourceOrg -Proj $SourceProject -UrlType "feeds"
$srcPkgsUrl  = Get-FeedBaseUrl -Org $SourceOrg -Proj $SourceProject -UrlType "pkgs"
$dstFeedsUrl = Get-FeedBaseUrl -Org $DestOrg -Proj $DestProject -UrlType "feeds"
$dstPkgsUrl  = Get-FeedBaseUrl -Org $DestOrg -Proj $DestProject -UrlType "pkgs"
$dstIndexUrl = "${dstPkgsUrl}/_packaging/${DestFeed}/nuget/v3/index.json"

# Read auth is resolved lazily (anonymous first) so an expired/misscoped PAT
# never breaks a read-only dry run of public feeds. Source reads use ONLY the
# dedicated source token (AZURE_DEVOPS_SOURCE_PAT) — never the destination PAT —
# so a destination write token is never sent to the source org.
$srcReadPat = $SourcePat

Write-Head "Configuration"
Write-Info "  Source      : $SourceOrg/$SourceProject  feed '$SourceFeed'"
Write-Info "  Destination : $DestOrg/$DestProject  feed '$DestFeed'"
Write-Info "  Mode        : $(if ($Push) { 'PUSH (writes enabled)' } else { 'DRY RUN (no writes)' })"
Write-Info "  Version rule: $(if ($VersionPolicy -eq 'ReleasesOnly') { 'releases only (allowlist)' } else { 'exclude bad -pr.* / malformed -preview-* (denylist)' })"
if ($ExcludePackageRegex) { Write-Info "  Exclude pkgs: $ExcludePackageRegex" }
Write-Info "  Time budget : $MaxDurationMinutes min"
if ($PackageFilter) { Write-Warn2 "  Filter      : $PackageFilter" }
Write-Host ""

if ($Push -and [string]::IsNullOrWhiteSpace($Pat)) {
    Write-Err2 "ERROR: -Push requires the AZURE_DEVOPS_PAT environment variable (packaging read/write on '$DestOrg')."
    exit 2
}

$startTime = Get-Date
function Get-ElapsedMinutes { return ((Get-Date) - $startTime).TotalMinutes }

# --- Fast phase: inventory both feeds (metadata only) --------------------------

Write-Head "Inventorying source feed (metadata only)..."
try {
    $srcHeaders = Resolve-ReadHeaders -FeedsBaseUrl $srcFeedsUrl -FeedId $SourceFeed -Pat $srcReadPat
    $srcInv = Get-FeedInventory -FeedsBaseUrl $srcFeedsUrl -FeedId $SourceFeed -Headers $srcHeaders -BatchSize $BatchSize -IncludeUnlisted:$IncludeUnlisted
}
catch {
    Write-Err2 "ERROR: could not read source feed '$SourceFeed' in $SourceOrg/$SourceProject : $_"
    Add-Summary "## ❌ Feed mirror failed"
    Add-Summary ""
    Add-Summary "Could not read **source** feed ``$SourceOrg/$SourceProject/$SourceFeed``:"
    Add-Summary ""
    Add-Summary "``````"
    Add-Summary "$_"
    Add-Summary "``````"
    Save-Summary -Path $SummaryFile
    exit 2
}
$srcTotal = $srcInv.Count

# Safety: a migration tool must never treat an unexpectedly empty source read as
# "nothing to do". A populated feed returning zero versions means something is
# wrong (auth, wrong feed, API change) — fail loudly instead of silently.
if ($srcTotal -eq 0) {
    Write-Err2 "ERROR: source feed '$SourceFeed' returned 0 versions — refusing to proceed (unexpected empty read)."
    Add-Summary "## ❌ Feed mirror aborted"
    Add-Summary ""
    Add-Summary "Source feed ``$SourceOrg/$SourceProject/$SourceFeed`` returned **0 versions**. Refusing to proceed."
    Save-Summary -Path $SummaryFile
    exit 2
}

# --- Apply intrinsic rules to the source set -----------------------------------
# 1. Optional smoke-test include filter.
if ($PackageFilter) {
    $srcInv = @($srcInv | Where-Object { $_.Id -match $PackageFilter })
}
# 2. Per-feed package exclusion (e.g. drop internal '_' packages from public).
$excludedByPackage = 0
if ($ExcludePackageRegex) {
    $before = $srcInv.Count
    $srcInv = @($srcInv | Where-Object { $_.Id -notmatch $ExcludePackageRegex })
    $excludedByPackage = $before - $srcInv.Count
}
# 3. Per-feed version policy.
#    - ReleasesOnly (public): ALLOWLIST — keep only recognized releases (KEEP).
#      Anything unrecognized is refused so junk can never reach the public feed.
#    - ExcludeBad  (internal): DENYLIST — keep everything except known-bad
#      (-pr.* / malformed -preview-*); commit/branch build versions are kept.
$excludedBadVersions = 0
$excludedUnknown = 0
$before = $srcInv.Count
if ($VersionPolicy -eq 'ReleasesOnly') {
    $kept = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $srcInv) {
        $action = Get-VersionAction -Version $e.Version
        if ($action -eq 'KEEP') { $kept.Add($e) }
        elseif ($action -eq 'DELETE') { $excludedBadVersions++ }
        else { $excludedUnknown++ }
    }
    $srcInv = @($kept)
}
else {
    $srcInv = @($srcInv | Where-Object { (Get-VersionAction -Version $_.Version) -ne 'DELETE' })
    $excludedBadVersions = $before - $srcInv.Count
}

Write-Good "  Source versions: $srcTotal total; $($srcInv.Count) eligible after rules"
if ($excludedByPackage -gt 0)   { Write-Info "    - excluded $excludedByPackage version(s) on '$ExcludePackageRegex' packages" }
if ($excludedBadVersions -gt 0) { Write-Info "    - excluded $excludedBadVersions bad version(s) (-pr.* / malformed -preview-*)" }
if ($excludedUnknown -gt 0)     { Write-Warn2 "    - excluded $excludedUnknown unrecognized version(s) (allowlist: releases only)" }

Write-Head "Inventorying destination feed (metadata only)..."
$dstIndex = [System.Collections.Generic.HashSet[string]]::new()
try {
    $dstHeaders = Resolve-ReadHeaders -FeedsBaseUrl $dstFeedsUrl -FeedId $DestFeed -Pat $Pat
    $dstInv = Get-FeedInventory -FeedsBaseUrl $dstFeedsUrl -FeedId $DestFeed -Headers $dstHeaders -BatchSize $BatchSize -IncludeUnlisted
    foreach ($e in $dstInv) { [void]$dstIndex.Add((Get-EntryKey -Id $e.Id -NVer $e.NVer)) }
    Write-Good "  Destination versions: $($dstInv.Count)"
}
catch {
    Write-Err2 "ERROR: could not read destination feed '$DestFeed' in $DestOrg/$DestProject : $_"
    Write-Warn2 "  (The destination feed must exist. Create it before mirroring.)"
    Add-Summary "## ❌ Feed mirror failed"
    Add-Summary ""
    Add-Summary "Could not read **destination** feed ``$DestOrg/$DestProject/$DestFeed`` — it must exist before mirroring."
    Add-Summary ""
    Add-Summary "``````"
    Add-Summary "$_"
    Add-Summary "``````"
    Save-Summary -Path $SummaryFile
    exit 2
}

# --- Diff: what is missing in the destination ----------------------------------

$missing = [System.Collections.Generic.List[object]]::new()
foreach ($e in $srcInv) {
    $key = Get-EntryKey -Id $e.Id -NVer $e.NVer
    if (-not $dstIndex.Contains($key)) { $missing.Add($e) }
}

$alreadyThere = $srcInv.Count - $missing.Count

Write-Host ""
Write-Head "Diff"
Write-Info  "  Eligible to mirror   : $($srcInv.Count)"
Write-Good  "  Already in dest      : $alreadyThere"
Write-Warn2 "  Missing (to copy)    : $($missing.Count)"
Write-Host ""

# --- Build the Markdown summary (shown in CI as the PR check step summary) ------
Add-Summary "## Feed mirror — ``$Feed`` ($($feedProfile.Kind))"
Add-Summary ""
Add-Summary "``$SourceOrg/$SourceProject/$SourceFeed`` → ``$DestOrg/$DestProject/$DestFeed``"
Add-Summary ""
Add-Summary "**Mode:** $(if ($Push) { 'PUSH (writes enabled)' } else { '🔍 DRY RUN — no packages are downloaded or pushed' })"
Add-Summary ""
Add-Summary "**Rules applied:**"
if ($ExcludePackageRegex) {
    Add-Summary "- exclude internal packages matching ``$ExcludePackageRegex`` (dropped $excludedByPackage version(s))"
} else {
    Add-Summary "- keep all packages (internal ``_`` wrapper packages included)"
}
if ($VersionPolicy -eq 'ReleasesOnly') {
    Add-Summary "- allowlist: publish recognized **releases only** — dropped $excludedBadVersions bad + $excludedUnknown unrecognized version(s)"
} else {
    Add-Summary "- skip known-bad versions ``-pr.*`` / malformed ``-preview-*`` (dropped $excludedBadVersions version(s)); commit/branch builds kept"
}
Add-Summary ""
Add-Summary "| Metric | Count |"
Add-Summary "|---|---:|"
Add-Summary "| In source feed (all) | $srcTotal |"
Add-Summary "| Eligible after rules | $($srcInv.Count) |"
Add-Summary "| Already in destination | $alreadyThere |"
Add-Summary "| **Missing (to copy)** | **$($missing.Count)** |"
Add-Summary ""

if ($missing.Count -eq 0) {
    Write-Good "Destination is already in sync. Nothing to do."
    Add-Summary "### ✅ Result: destination already in sync — nothing to copy."
    Save-Summary -Path $SummaryFile
    exit 0
}

# Per-package grouping (all package ids that will be copied — no truncation).
$byPkg = @($missing | Group-Object Id | Sort-Object Count -Descending)
Add-Summary "### To copy: $($missing.Count) version(s) across $($byPkg.Count) package(s)"
Add-Summary ""
Add-Summary "| Package | Versions to copy |"
Add-Summary "|---|---:|"
foreach ($g in $byPkg) {
    Add-Summary "| $($g.Name) | $($g.Count) |"
}
Add-Summary ""

# Preview a sample of what would be copied.
$previewN = [Math]::Min(15, $missing.Count)
Write-Head "First $previewN package(s) to copy:"
for ($i = 0; $i -lt $previewN; $i++) {
    Write-Info ("   - {0} {1}" -f $missing[$i].Id, $missing[$i].Version)
}
if ($missing.Count -gt $previewN) { Write-Info "   ... and $($missing.Count - $previewN) more" }
Write-Host ""

if (-not $Push) {
    Write-Warn2 "DRY RUN complete — no packages were transferred."
    Write-Warn2 "Re-run with -Push (and a PAT) to copy the $($missing.Count) missing package(s)."
    Add-Summary "> 🔍 **DRY RUN** — nothing was downloaded or pushed. Re-run the workflow manually with **push: true** to copy the $($missing.Count) missing version(s)."
    Save-Summary -Path $SummaryFile
    exit 0
}

# --- Push phase (opt-in) -------------------------------------------------------

if ([string]::IsNullOrWhiteSpace($CacheDir)) {
    $CacheDir = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-feed-mirror"
}
# Per-feed cache subdirectory so a cached download for one feed can never be
# reused for a different feed. (CI runners start clean; this matters for local
# re-runs across feeds.)
$CacheDir = Join-Path $CacheDir $Feed

# Configure push auth via the credential provider (no secret on disk or CLI).
Set-PushCredentials -IndexUrl $dstIndexUrl -Pat $Pat

Write-Head "Pushing $($missing.Count) missing package(s) to '$DestFeed'..."
Write-Info  "  Cache: $CacheDir"
Write-Host ""

$copied = 0
$failed = 0
$skippedExisting = 0
$processed = 0
$failedList = [System.Collections.Generic.List[string]]::new()
$timedOut = $false

foreach ($item in $missing) {
    # Graceful, between-package timeout. Reserve enough headroom for one worst-case
    # package (large native asset download + push --timeout 300s + retry backoff)
    # so we always stop cleanly BETWEEN packages and never mid-push.
    if ((Get-ElapsedMinutes) -ge ($MaxDurationMinutes - 12)) {
        $timedOut = $true
        Write-Warn2 "Time budget reached after $processed item(s) — stopping gracefully."
        break
    }

    $processed++
    $label = "[{0}/{1}] {2} {3}" -f $processed, $missing.Count, $item.Id, $item.Version

    $attempt = 0
    $done = $false
    while (-not $done -and $attempt -lt $MaxPushRetries) {
        $attempt++
        try {
            $path = Save-FeedPackage -PkgsBaseUrl $srcPkgsUrl -FeedId $SourceFeed -Id $item.Id -Version $item.Version -Headers $srcHeaders -CacheDir $CacheDir

            $check = Test-NupkgMatches -Path $path -ExpectId $item.Id -ExpectVersion $item.Version
            if (-not $check.Ok) {
                # Corrupt / mismatched download — never push it. Drop the cached
                # copy and retry a fresh download.
                Write-Warn2 "  $label — integrity check failed ($($check.Reason)); re-downloading"
                Remove-Item $path -Force -ErrorAction SilentlyContinue
                if ($attempt -ge $MaxPushRetries) {
                    throw "integrity check failed after $MaxPushRetries attempts: $($check.Reason)"
                }
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt))
                continue
            }

            $pushResult = Publish-FeedPackage -NupkgPath $path -IndexUrl $dstIndexUrl -ScrubValue $Pat
            if ($pushResult.Ok) {
                if ($pushResult.Skipped) {
                    $skippedExisting++
                    Write-Good "  $label — already present (skipped)"
                } else {
                    $copied++
                    Write-Good "  $label — copied"
                }
                $done = $true
            }
            else {
                if ($attempt -ge $MaxPushRetries) {
                    throw "push failed: $($pushResult.Out.Trim())"
                }
                Write-Warn2 "  $label — push failed (attempt $attempt/$MaxPushRetries); retrying"
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt))
            }
        }
        catch {
            if ($attempt -ge $MaxPushRetries) {
                $failed++
                # Flatten the (often multi-line) error to a single line so it
                # renders cleanly in the console list and the Markdown summary.
                $errText = ([string]$_ -replace '\s+', ' ').Trim()
                $failedList.Add("$($item.Id) $($item.Version) :: $errText")
                Write-Err2 "  $label — FAILED after $MaxPushRetries attempt(s): $errText"
                $done = $true
            }
            else {
                Write-Warn2 "  $label — error (attempt $attempt/$MaxPushRetries): $_ — retrying"
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt))
            }
        }
    }
}

# --- Summary -------------------------------------------------------------------

$remaining = $missing.Count - $processed
Write-Host ""
Write-Head "=================================================="
Write-Head " Summary"
Write-Head "=================================================="
Write-Good  "  Copied           : $copied"
Write-Info  "  Already present  : $skippedExisting"
Write-Err2  "  Failed           : $failed"
if ($timedOut) { Write-Warn2 "  Not yet attempted: $remaining (timed out — re-run to continue)" }
Write-Info  "  Elapsed          : $([Math]::Round((Get-ElapsedMinutes), 1)) min"
Write-Host ""

# Push results into the Markdown summary.
Add-Summary "### Push results"
Add-Summary ""
Add-Summary "| Result | Count |"
Add-Summary "|---|---:|"
Add-Summary "| ✅ Copied | $copied |"
Add-Summary "| ⏭️ Already present | $skippedExisting |"
Add-Summary "| ❌ Failed | $failed |"
if ($timedOut) { Add-Summary "| ⏳ Not yet attempted | $remaining |" }
Add-Summary ""
if ($timedOut) { Add-Summary "> ⏳ Stopped on the time budget with progress and no errors — re-run to continue (idempotent)." }

if ($failed -gt 0) {
    Write-Err2 "The following package(s) failed — re-run to retry just these:"
    foreach ($f in $failedList) { Write-Err2 "   - $f" }
    Add-Summary ""
    Add-Summary "#### ❌ Failed package(s) (re-run to retry just these)"
    Add-Summary ""
    foreach ($f in $failedList) { Add-Summary "- ``$f``" }
    Save-Summary -Path $SummaryFile
    exit 1
}

if ($timedOut) {
    Write-Warn2 "Stopped on time budget with progress and no errors. Re-run to continue (idempotent)."
    Save-Summary -Path $SummaryFile
    exit 0
}

# --- End-of-run verification ---------------------------------------------------
# Independently confirm the push actually landed: re-inventory the destination and
# assert that every version we set out to copy is now present. This catches any
# push that reported success but did not commit (e.g. a masked/soft failure).
Write-Head "Verifying destination now contains every copied version..."
try {
    $verifyHeaders = Resolve-ReadHeaders -FeedsBaseUrl $dstFeedsUrl -FeedId $DestFeed -Pat $Pat
    $verifyInv = Get-FeedInventory -FeedsBaseUrl $dstFeedsUrl -FeedId $DestFeed -Headers $verifyHeaders -BatchSize $BatchSize -IncludeUnlisted
    $verifyIndex = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($e in $verifyInv) { [void]$verifyIndex.Add((Get-EntryKey -Id $e.Id -NVer $e.NVer)) }

    $stillMissing = [System.Collections.Generic.List[string]]::new()
    foreach ($item in $missing) {
        if (-not $verifyIndex.Contains((Get-EntryKey -Id $item.Id -NVer $item.NVer))) {
            $stillMissing.Add("$($item.Id) $($item.Version)")
        }
    }

    if ($stillMissing.Count -gt 0) {
        Write-Err2 "VERIFICATION FAILED: $($stillMissing.Count) version(s) reported pushed but are NOT in the destination."
        Add-Summary ""
        Add-Summary "### ❌ Verification failed"
        Add-Summary ""
        Add-Summary "$($stillMissing.Count) version(s) were reported as pushed but are not present in the destination on re-inventory. Re-run to retry."
        Add-Summary ""
        foreach ($m in ($stillMissing | Select-Object -First 50)) { Add-Summary "- ``$m``" }
        Save-Summary -Path $SummaryFile
        exit 1
    }
    Write-Good "  Verified: all $($missing.Count) version(s) are present in the destination."
}
catch {
    Write-Err2 "VERIFICATION could not complete (could not re-read destination): $_"
    Add-Summary ""
    Add-Summary "### ⚠️ Could not verify"
    Add-Summary ""
    Add-Summary "The push loop completed but the destination re-inventory failed, so the result is unconfirmed. Re-run to verify (idempotent)."
    Save-Summary -Path $SummaryFile
    exit 1
}

Write-Good "All missing packages processed and verified successfully."
Add-Summary ""
Add-Summary "✅ All missing packages processed and verified present in the destination."
Save-Summary -Path $SummaryFile
exit 0

#endregion
