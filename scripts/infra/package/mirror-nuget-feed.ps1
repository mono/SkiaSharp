<#
.SYNOPSIS
    Mirror (copy) NuGet packages from a SOURCE Azure DevOps Artifacts feed to a
    DESTINATION feed — designed for the xamarin -> dnceng feed migration.

.DESCRIPTION
    TEMPORARY MIGRATION TOOL. Copies every package version that exists in the
    source feed but is missing from the destination feed. It is:

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

    Read access to the (public) source feed is anonymous; only the PUSH to the
    destination needs a PAT (packaging read/write on the destination org).

.PARAMETER SourceOrg
    Source Azure DevOps organization. Default: xamarin

.PARAMETER SourceProject
    Source project (empty for org-scoped feeds). Default: public

.PARAMETER SourceFeed
    Source feed name or id. Default: SkiaSharp

.PARAMETER DestOrg
    Destination Azure DevOps organization. Default: dnceng

.PARAMETER DestProject
    Destination project (empty for org-scoped feeds). Default: public

.PARAMETER DestFeed
    Destination feed name or id. Default: skiasharp

.PARAMETER Push
    OPT IN. Actually download+push missing packages. Without this flag the
    script runs a DRY RUN (inventory + diff + plan only).

.PARAMETER Pat
    Personal Access Token with Packaging read/write on the DESTINATION org.
    Required only when -Push is set. Read from $env:AZURE_DEVOPS_PAT by default.

    SECURITY: the PAT is never passed on a command line, never written to a
    nuget.config, and never printed. For the push it is handed to the Azure
    Artifacts Credential Provider via the VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
    process env var; for REST reads (only if the source feed is private) it goes
    in an Authorization header. It is also registered with the GitHub Actions log
    masker. Dry runs need no PAT at all (public feeds are read anonymously).

.PARAMETER SourcePat
    Optional PAT for the source feed if it is not anonymously readable.
    Falls back to $env:AZURE_DEVOPS_SOURCE_PAT, then to -Pat, then anonymous.

.PARAMETER MaxDurationMinutes
    Wall-clock budget. The script stops starting new work once this is reached
    and exits cleanly. Default: 50 (leaves headroom under a 60-min CI timeout).

.PARAMETER PackageFilter
    Optional regex to limit which package ids are mirrored (e.g. '^SkiaSharp$').

.PARAMETER ExcludePackageRegex
    Optional regex of package ids to EXCLUDE. Useful for keeping a public feed
    clean of internal CI plumbing, e.g. '^_' drops the underscore-prefixed
    wrapper packages (_NativeAssets, _NuGets, _Symbols, ...).

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
    plan (what would be copied, broken down by scope). In CI, point this at
    $env:GITHUB_STEP_SUMMARY so the result shows up in the PR check summary.

.EXAMPLE
    # Dry run — see exactly what WOULD be copied (safe, no writes)
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp -DestFeed skiasharp

.EXAMPLE
    # Real mirror of the CI feed, with a 45-minute budget
    $env:AZURE_DEVOPS_PAT = '<pat>'
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp-CI -DestFeed skiasharp-ci -Push -MaxDurationMinutes 45

.EXAMPLE
    # Mirror only real releases (skip -pr.* and malformed -preview-* build artifacts)
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp -DestFeed skiasharp -GoodVersionsOnly -Push

.EXAMPLE
    # Keep the PUBLIC feed clean: mirror real packages only, excluding the
    # internal underscore-prefixed CI wrapper packages (_NativeAssets, _NuGets…).
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp -DestFeed skiasharp -ExcludePackageRegex '^_' -Push

.EXAMPLE
    # Strictest scope: mirror ONLY recognized releases, dropping 0.0.0-commit.* /
    # 0.0.0-branch.* per-build CI artifacts as well.
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp-CI -DestFeed skiasharp-ci -ReleasesOnly -Push

.EXAMPLE
    # Smoke-test the push path end to end on a single package first
    ./mirror-nuget-feed.ps1 -SourceFeed SkiaSharp -DestFeed skiasharp -PackageFilter '^SkiaSharp$' -Push

.NOTES
    Exit codes:
      0  success — everything synced, dry run completed, or gracefully timed out
         with progress and no errors (safe to re-run to continue)
      1  one or more packages failed after retries (re-run to retry just those)
      2  fatal setup error (bad args, feed unreachable, missing PAT for -Push)
#>

[CmdletBinding()]
param(
    [string]$SourceOrg = "xamarin",
    [string]$SourceProject = "public",
    [string]$SourceFeed = "SkiaSharp",

    [string]$DestOrg = "dnceng",
    [string]$DestProject = "public",
    [string]$DestFeed = "skiasharp",

    [switch]$Push,

    [string]$Pat = $env:AZURE_DEVOPS_PAT,
    [string]$SourcePat = $env:AZURE_DEVOPS_SOURCE_PAT,

    [int]$MaxDurationMinutes = 50,
    [string]$PackageFilter = "",
    [string]$ExcludePackageRegex = "",
    [switch]$IncludeUnlisted,

    # Migration-scope control. By default EVERYTHING is mirrored. -GoodVersionsOnly
    # skips ephemeral build artifacts (-pr.* and malformed -preview-* versions),
    # matching the retention policy of manage-nuget-feed.ps1. -ReleasesOnly is
    # stricter still: it keeps ONLY recognized releases (stable / -preview. / -rc.
    # / -nightly. / -stable. / -alpha.) and drops everything else, including the
    # 0.0.0-commit.* / 0.0.0-branch.* per-build CI artifacts.
    [switch]$GoodVersionsOnly,
    [switch]$ReleasesOnly,
    [string]$ExcludeVersionRegex = "",

    [int]$MaxPushRetries = 3,
    [string]$CacheDir = "",
    [int]$BatchSize = 100,
    [string]$SummaryFile = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

#region Helpers

# --- Version classification (mirrors manage-nuget-feed.ps1 retention policy) ----
$script:MainVersionPattern = '\d+\.\d+(?:\.\d+)?(?:\.\d+)?'
$script:AllowedPrereleaseSuffixes = @(
    '-alpha\.\d+(?:\.\d+)?'
    '-preview\.\d+(?:\.\d+)?'
    '-rc\.\d+(?:\.\d+)?'
    '-nightly\.\d+(?:\.\d+)?'
    '-stable\.\d+(?:\.\d+)?'
)
$script:DeletePrereleaseSuffixes = @(
    '-pr\.\d+(?:\.\d+)?'
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
function Save-Package {
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
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
        $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
        $zip.Dispose()
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
            $nuspec = $zip.Entries | Where-Object { $_.Name -like "*.nuspec" } | Select-Object -First 1
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
}

function Clear-PushCredentials {
    Remove-Item Env:\VSS_NUGET_EXTERNAL_FEED_ENDPOINTS -ErrorAction SilentlyContinue
}

# Push a single package. 409 / already-exists is treated as success (idempotent).
# Auth comes from the credential provider (see Set-PushCredentials) — no secret is
# passed here. Output is scrubbed of the PAT before it is ever returned/logged.
function Push-Package {
    param(
        [string]$NupkgPath,
        [string]$IndexUrl,
        [string]$ScrubValue = ""
    )
    $out = & dotnet nuget push $NupkgPath --source $IndexUrl --api-key az --skip-duplicate 2>&1 | Out-String
    if (-not [string]::IsNullOrEmpty($ScrubValue)) {
        $out = $out.Replace($ScrubValue, "***")
    }
    if ($LASTEXITCODE -eq 0) { return @{ Ok = $true; Skipped = $false; Out = $out } }
    if ($out -match "already exists|409|Conflict|Skipping|duplicate") {
        return @{ Ok = $true; Skipped = $true; Out = $out }
    }
    return @{ Ok = $false; Skipped = $false; Out = $out }
}

#endregion

#region Main

Write-Host ""
Write-Head "=================================================="
Write-Head " NuGet Feed Mirror (source -> destination)"
Write-Head "=================================================="
Write-Host ""

$srcFeedsUrl = Get-FeedBaseUrl -Org $SourceOrg -Proj $SourceProject -UrlType "feeds"
$srcPkgsUrl  = Get-FeedBaseUrl -Org $SourceOrg -Proj $SourceProject -UrlType "pkgs"
$dstFeedsUrl = Get-FeedBaseUrl -Org $DestOrg -Proj $DestProject -UrlType "feeds"
$dstPkgsUrl  = Get-FeedBaseUrl -Org $DestOrg -Proj $DestProject -UrlType "pkgs"
$dstIndexUrl = "${dstPkgsUrl}/_packaging/${DestFeed}/nuget/v3/index.json"

# Read auth is resolved lazily (anonymous first) so an expired/misscoped PAT
# never breaks a read-only dry run of public feeds. The PAT is only truly
# required for the push. -SourcePat (or its env var) is used only if the source
# is not anonymously readable.
$srcReadPat = if (-not [string]::IsNullOrWhiteSpace($SourcePat)) { $SourcePat } else { $Pat }
$dstHeaders = New-AuthHeaders -TokenPat $Pat

Write-Head "Configuration"
Write-Info "  Source      : $SourceOrg/$SourceProject  feed '$SourceFeed'"
Write-Info "  Destination : $DestOrg/$DestProject  feed '$DestFeed'"
Write-Info "  Mode        : $(if ($Push) { 'PUSH (writes enabled)' } else { 'DRY RUN (no writes)' })"
Write-Info "  Time budget : $MaxDurationMinutes min"
if ($PackageFilter) { Write-Warn2 "  Filter      : $PackageFilter" }
if ($ExcludePackageRegex) { Write-Warn2 "  Exclude pkgs: $ExcludePackageRegex" }
Write-Host ""

if ($Push -and [string]::IsNullOrWhiteSpace($Pat)) {
    Write-Err2 "ERROR: -Push requires a PAT (set -Pat or `$env:AZURE_DEVOPS_PAT) with packaging read/write on '$DestOrg'."
    exit 2
}

$startTime = Get-Date
function Get-ElapsedMinutes { return ((Get-Date) - $startTime).TotalMinutes }
function Test-BudgetExceeded { return (Get-ElapsedMinutes) -ge $MaxDurationMinutes }

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
if ($PackageFilter) {
    $srcInv = @($srcInv | Where-Object { $_.Id -match $PackageFilter })
}
if ($ExcludePackageRegex) {
    $srcInv = @($srcInv | Where-Object { $_.Id -notmatch $ExcludePackageRegex })
}
Write-Good "  Source versions: $($srcInv.Count)"

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

# Classify the missing set so the operator can see (and skip) ephemeral builds.
$breakdownKeep = 0; $breakdownDelete = 0; $breakdownUnknown = 0
foreach ($e in $missing) {
    switch (Get-VersionAction -Version $e.Version) {
        "KEEP"    { $breakdownKeep++ }
        "DELETE"  { $breakdownDelete++ }
        default   { $breakdownUnknown++ }
    }
}

Write-Host ""
Write-Head "Diff"
Write-Info  "  In source            : $($srcInv.Count)"
Write-Good  "  Already in dest      : $alreadyThere"
Write-Warn2 "  Missing (to copy)    : $($missing.Count)"
Write-Host ""
Write-Head "Missing breakdown by version policy"
Write-Good  "  Releases to keep     : $breakdownKeep  (stable / -preview. / -rc. / -nightly. / -stable. / -alpha.)"
Write-Warn2 "  Ephemeral builds     : $breakdownDelete  (-pr.* and malformed -preview-*)"
Write-Info  "  Unrecognized         : $breakdownUnknown"
Write-Host ""

# --- Build the Markdown summary (shown in CI as the PR check step summary) ------
$scopeAll      = $breakdownKeep + $breakdownDelete + $breakdownUnknown
$scopeGood     = $breakdownKeep + $breakdownUnknown
$scopeReleases = $breakdownKeep
Add-Summary "## Feed mirror — ``$SourceOrg/$SourceProject/$SourceFeed`` → ``$DestOrg/$DestProject/$DestFeed``"
Add-Summary ""
Add-Summary "**Mode:** $(if ($Push) { 'PUSH (writes enabled)' } else { '🔍 DRY RUN — no packages are downloaded or pushed' })"
Add-Summary ""
Add-Summary "| Metric | Count |"
Add-Summary "|---|---:|"
Add-Summary "| In source feed | $($srcInv.Count) |"
Add-Summary "| Already in destination | $alreadyThere |"
Add-Summary "| **Missing (all versions)** | **$($missing.Count)** |"
Add-Summary ""
Add-Summary "### What each scope would copy"
Add-Summary ""
Add-Summary "| Scope | Flag | Would copy |"
Add-Summary "|---|---|---:|"
Add-Summary "| Everything | _(default)_ | $scopeAll |"
Add-Summary "| Good versions | ``-GoodVersionsOnly`` | $scopeGood |"
Add-Summary "| Releases only | ``-ReleasesOnly`` | $scopeReleases |"
Add-Summary ""
Add-Summary "<sub>releases = $breakdownKeep (stable / -preview. / -rc. / -nightly. / -stable. / -alpha.) · ephemeral = $breakdownDelete (``-pr.*`` / malformed ``-preview-*``) · unrecognized = $breakdownUnknown (e.g. ``0.0.0-commit.*`` / ``0.0.0-branch.*``)</sub>"
Add-Summary ""

# Apply migration-scope filters.
if ($ReleasesOnly) {
    $before = $missing.Count
    $filtered = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $missing) {
        if ((Get-VersionAction -Version $e.Version) -eq "KEEP") { $filtered.Add($e) }
    }
    $missing = $filtered
    Write-Warn2 "  -ReleasesOnly: keeping recognized releases only -> $($missing.Count) of $before will be mirrored"
}
elseif ($GoodVersionsOnly) {
    $before = $missing.Count
    $filtered = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $missing) {
        if ((Get-VersionAction -Version $e.Version) -ne "DELETE") { $filtered.Add($e) }
    }
    $missing = $filtered
    Write-Warn2 "  -GoodVersionsOnly: excluding ephemeral builds -> $($missing.Count) of $before will be mirrored"
}
if ($ExcludeVersionRegex) {
    $before = $missing.Count
    $filtered = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $missing) {
        if ($e.Version -notmatch $ExcludeVersionRegex) { $filtered.Add($e) }
    }
    $missing = $filtered
    Write-Warn2 "  -ExcludeVersionRegex '$ExcludeVersionRegex': -> $($missing.Count) of $before will be mirrored"
}
if ($ReleasesOnly -or $GoodVersionsOnly -or $ExcludeVersionRegex) { Write-Host "" }

if ($missing.Count -eq 0) {
    Write-Good "Destination is already in sync (for the selected scope). Nothing to do."
    Add-Summary "### ✅ Result: destination already in sync — nothing to copy."
    Save-Summary -Path $SummaryFile
    exit 0
}

# Per-package grouping (what would actually be copied for the selected scope).
$selectedLabel = if ($ReleasesOnly) { 'releases only' } elseif ($GoodVersionsOnly) { 'good versions' } else { 'everything' }
$byPkg = @($missing | Group-Object Id | Sort-Object Count -Descending)
Add-Summary "### Selected scope: **$selectedLabel** → $($missing.Count) version(s) across $($byPkg.Count) package(s)"
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
    # Graceful, between-package timeout. Reserve ~3 min so an in-flight download
    # is unlikely to be cut off mid-transfer.
    if ((Get-ElapsedMinutes) -ge ($MaxDurationMinutes - 3)) {
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
            $path = Save-Package -PkgsBaseUrl $srcPkgsUrl -FeedId $SourceFeed -Id $item.Id -Version $item.Version -Headers $srcHeaders -CacheDir $CacheDir

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

            $push = Push-Package -NupkgPath $path -IndexUrl $dstIndexUrl -ScrubValue $Pat
            if ($push.Ok) {
                if ($push.Skipped) {
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
                    throw "push failed: $($push.Out.Trim())"
                }
                Write-Warn2 "  $label — push failed (attempt $attempt/$MaxPushRetries); retrying"
                Start-Sleep -Seconds ([Math]::Pow(2, $attempt))
            }
        }
        catch {
            if ($attempt -ge $MaxPushRetries) {
                $failed++
                $failedList.Add("$($item.Id) $($item.Version) :: $_")
                Write-Err2 "  $label — FAILED after $MaxPushRetries attempt(s): $_"
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

Write-Good "All missing packages processed successfully."
Add-Summary ""
Add-Summary "✅ All missing packages processed successfully."
Save-Summary -Path $SummaryFile
exit 0

#endregion
