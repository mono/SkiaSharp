<#
.SYNOPSIS Compute a content-based cache key for a build job.
.DESCRIPTION
    Reads the job's path patterns from repo-deps.config.json (walking
    depends_on for inheritance), hashes all files in those paths, and
    combines with submodule SHAs to produce a deterministic cache key.

.PARAMETER JobName
    The unique ADO job name (e.g. native_win32_x64_windows).

.PARAMETER CacheJob
    The job name in repo-deps.config.json (e.g. native_windows).

.PARAMETER Docker
    Optional Docker context directory to include in the hash.
#>
param(
    [Parameter(Mandatory)]
    [string]$JobName,

    [Parameter(Mandatory)]
    [string]$CacheJob,

    [string]$Docker = ''
)

$ErrorActionPreference = 'Stop'

function Get-FileHashString([string]$Path) {
    if (Test-Path $Path) { return (Get-FileHash $Path -Algorithm SHA256).Hash }
    return ''
}

# ---------------------------------------------------------------------------
# 1. Submodule SHAs (set by pipeline or read from tree)
# ---------------------------------------------------------------------------
$skiaSha = $env:SKIA_SHA
if (-not $skiaSha) {
    $line = (git ls-tree HEAD externals/skia 2>$null)
    if ($line -match '([0-9a-f]{40})') { $skiaSha = $Matches[1] }
}
$depotSha = $env:DEPOT_SHA
if (-not $depotSha) {
    $line = (git ls-tree HEAD externals/depot_tools 2>$null)
    if ($line -match '([0-9a-f]{40})') { $depotSha = $Matches[1] }
}
$skiaSha = ($skiaSha ?? 'unknown').Trim()
$depotSha = ($depotSha ?? 'unknown').Trim()

# ---------------------------------------------------------------------------
# 2. Collect paths from config (with depends_on inheritance)
# ---------------------------------------------------------------------------
$configPath = 'scripts/infra/caching/repo-deps.config.json'
$dirs = @()

if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $jobs = $config.jobs.PSObject.Properties

    function Get-JobPaths([string]$Name) {
        $job = $jobs | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
        if (-not $job) { return @() }
        $paths = @($job.Value.paths | Where-Object { $_ })
        foreach ($dep in @($job.Value.depends_on | Where-Object { $_ })) {
            $paths += Get-JobPaths $dep
        }
        return $paths
    }

    $dirs = @(Get-JobPaths $CacheJob | Select-Object -Unique)
    Write-Host "Job '$CacheJob': $($dirs.Count) path patterns (including inherited)"
} else {
    Write-Host "Config not found — using fallback"
    $dirs = @("native", "scripts/infra/native/shared", "scripts/VERSIONS.txt")
}

# Add Docker context if specified
if ($Docker -and (Test-Path $Docker)) {
    $dirs = @($dirs) + @($Docker)
}

# ---------------------------------------------------------------------------
# 3. Hash all files matching the path patterns
# ---------------------------------------------------------------------------
$fileHashes = @()
$hashedDirs = @()

foreach ($pattern in $dirs | Sort-Object -Unique) {
    $dirPath = $pattern -replace '/\*\*$', '' -replace '\*\*$', ''

    if (Test-Path $dirPath -PathType Container) {
        $files = Get-ChildItem -Path $dirPath -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[/\\](bin|obj|libs|tools|\.git)[/\\]' } |
            Sort-Object FullName
        foreach ($file in $files) {
            $hash = Get-FileHashString $file.FullName
            if ($hash) { $fileHashes += $hash }
        }
        $hashedDirs += "$dirPath/ ($($files.Count) files)"
    } elseif (Test-Path $dirPath -PathType Leaf) {
        $hash = Get-FileHashString $dirPath
        if ($hash) { $fileHashes += $hash }
        $hashedDirs += $dirPath
    }
}

# Composite hash
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes(($fileHashes -join '|'))
$compositeHash = ($sha256.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
$compositeHash = $compositeHash.Substring(0, 24)

# ---------------------------------------------------------------------------
# 4. Cache key
# ---------------------------------------------------------------------------
$cacheKey = "$CacheJob|$JobName|$skiaSha|$depotSha|$compositeHash"

# ---------------------------------------------------------------------------
# 5. Output
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗"
Write-Host "║  Cache Key                                                   ║"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Job:      $JobName"
Write-Host "║  CacheJob: $CacheJob"
Write-Host "║  Skia:     $skiaSha"
Write-Host "║  Depot:    $depotSha"
Write-Host "║  Files:    $compositeHash ($($fileHashes.Count) files)"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
foreach ($d in $hashedDirs) { Write-Host "║  $d" }
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  KEY: $cacheKey"
Write-Host "╚══════════════════════════════════════════════════════════════╝"

Write-Host "##vso[task.setvariable variable=CACHE_KEY]$cacheKey"
