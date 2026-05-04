<#
.SYNOPSIS Compute a content-based cache key for a build job.
.DESCRIPTION
    Reads the job's path patterns and submodules from repo-deps.config.json
    (walking depends_on for inheritance), hashes all files in those paths,
    and combines with submodule SHAs to produce a deterministic cache key.

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
# 1. Load config and collect paths + submodules via depends_on
# ---------------------------------------------------------------------------
$configPath = 'scripts/infra/caching/repo-deps.config.json'
$dirs = @()
$submodulePaths = @()

if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $allJobs = $config.jobs.PSObject.Properties

    function Get-JobConfig([string]$Name) {
        $job = $allJobs | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
        if (-not $job) { return @{ paths = @(); submodules = @() } }
        $paths = @($job.Value.paths | Where-Object { $_ })
        $subs = @($job.Value.submodules | Where-Object { $_ })
        foreach ($dep in @($job.Value.depends_on | Where-Object { $_ })) {
            $depConfig = Get-JobConfig $dep
            $paths += $depConfig.paths
            $subs += $depConfig.submodules
        }
        return @{ paths = $paths; submodules = $subs }
    }

    $jobConfig = Get-JobConfig $CacheJob
    $dirs = @($jobConfig.paths | Select-Object -Unique)
    $submodulePaths = @($jobConfig.submodules | Select-Object -Unique)
    Write-Host "Job '$CacheJob': $($dirs.Count) paths, $($submodulePaths.Count) submodules"
} else {
    Write-Host "Config not found — using fallback"
    $dirs = @("native", "scripts/infra/native/shared", "scripts/VERSIONS.txt")
    $submodulePaths = @("externals/skia")
}

# Add Docker context if specified
if ($Docker -and (Test-Path $Docker)) {
    $dirs = @($dirs) + @($Docker)
}

# ---------------------------------------------------------------------------
# 2. Get submodule SHAs (from env vars set by pipeline, or from git tree)
# ---------------------------------------------------------------------------
$submoduleShas = @()
foreach ($sub in $submodulePaths) {
    $envVar = $sub.Replace('/', '_').Replace('-', '_').ToUpper() + '_SHA'
    $sha = [System.Environment]::GetEnvironmentVariable($envVar)
    if (-not $sha) {
        $line = (git ls-tree HEAD $sub 2>$null)
        if ($line -match '([0-9a-f]{40})') { $sha = $Matches[1] }
    }
    $sha = ($sha ?? 'unknown').Trim()
    $submoduleShas += "${sub}:${sha}"
    Write-Host "  Submodule $sub = $sha"
}
$submoduleKey = ($submoduleShas | Sort-Object) -join '|'

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

# Composite hash of all file hashes
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes(($fileHashes -join '|'))
$compositeHash = ($sha256.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
$compositeHash = $compositeHash.Substring(0, 24)

# ---------------------------------------------------------------------------
# 4. Cache key = cacheJob | jobName | submodules | files
# ---------------------------------------------------------------------------
$cacheKey = "$CacheJob|$JobName|$submoduleKey|$compositeHash"

# ---------------------------------------------------------------------------
# 5. Output
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗"
Write-Host "║  Cache Key                                                   ║"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Job:      $JobName"
Write-Host "║  CacheJob: $CacheJob"
foreach ($s in $submoduleShas) { Write-Host "║  Sub:     $s" }
Write-Host "║  Files:    $compositeHash ($($fileHashes.Count) files)"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
foreach ($d in $hashedDirs) { Write-Host "║  $d" }
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  KEY: $cacheKey"
Write-Host "╚══════════════════════════════════════════════════════════════╝"

if ($env:BUILD_BUILDID) {
    Write-Host "##vso[task.setvariable variable=CACHE_KEY]$cacheKey"
}
