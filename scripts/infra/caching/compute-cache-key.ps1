<#
.SYNOPSIS Compute a content-based cache key for a native build job.
.DESCRIPTION
    Reads stage paths from repo-deps.config.json and hashes all files
    in the directories that affect the given target. Combined with
    submodule SHAs to produce a deterministic cache key.

.PARAMETER JobName
    The unique job name (e.g. native_win32_x64_windows).

.PARAMETER Target
    The Cake target (e.g. externals-windows). Used to find the
    matching stage in the config.

.PARAMETER Docker
    Optional Docker context directory path.
#>
param(
    [Parameter(Mandatory)]
    [string]$JobName,

    [Parameter(Mandatory)]
    [string]$Target,

    [string]$Docker = ''
)

$ErrorActionPreference = 'Stop'

function Get-FileHashString([string]$Path) {
    if (Test-Path $Path) { return (Get-FileHash $Path -Algorithm SHA256).Hash }
    return ''
}

# ---------------------------------------------------------------------------
# 1. Submodule SHAs (set by pipeline step or read from tree)
# ---------------------------------------------------------------------------
$skiaSha = $env:SKIA_SHA
if (-not $skiaSha) {
    $treeLine = (git ls-tree HEAD externals/skia 2>$null)
    if ($treeLine -match '([0-9a-f]{40})') { $skiaSha = $Matches[1] }
}
$depotSha = $env:DEPOT_SHA
if (-not $depotSha) {
    $treeLine = (git ls-tree HEAD externals/depot_tools 2>$null)
    if ($treeLine -match '([0-9a-f]{40})') { $depotSha = $Matches[1] }
}
$skiaSha = ($skiaSha ?? 'unknown').Trim()
$depotSha = ($depotSha ?? 'unknown').Trim()

# ---------------------------------------------------------------------------
# 2. Find matching stage from config
# ---------------------------------------------------------------------------
$configPath = 'scripts/infra/caching/repo-deps.config.json'
$platformDir = $Target -replace '^externals-', ''

# Map target to stage name
$stageName = "native_$($platformDir -replace '-','_')"

$dirs = @()
if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $stages = $config.stages.PSObject.Properties

    # Collect paths by walking depends_on chain
    function Get-StagePaths([string]$Name) {
        $stage = $stages | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
        if (-not $stage) { return @() }
        $paths = @($stage.Value.paths | Where-Object { $_ })
        foreach ($dep in @($stage.Value.depends_on | Where-Object { $_ })) {
            $paths += Get-StagePaths $dep
        }
        return $paths
    }

    $dirs = @(Get-StagePaths $stageName | Select-Object -Unique)
    Write-Host "Stage '$stageName': $($dirs.Count) path patterns (including inherited)"
} else {
    Write-Host "Config not found — using fallback"
    $dirs = @("native/$platformDir", "scripts/infra/native/shared", "scripts/VERSIONS.txt")
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
    # Strip trailing /** for directory matching
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
$combinedInput = ($fileHashes -join '|')
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($combinedInput)
$compositeHash = ($sha256.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
$compositeHash = $compositeHash.Substring(0, 24)

# ---------------------------------------------------------------------------
# 4. Compose cache key
# ---------------------------------------------------------------------------
$cacheKey = "native|$JobName|$skiaSha|$depotSha|$compositeHash"

# ---------------------------------------------------------------------------
# 5. Output
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗"
Write-Host "║  Cache Key                                                   ║"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Job:      $JobName"
Write-Host "║  Stage:    $stageName"
Write-Host "║  Skia:     $skiaSha"
Write-Host "║  Depot:    $depotSha"
Write-Host "║  Files:    $compositeHash ($($fileHashes.Count) files)"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
foreach ($d in $hashedDirs) {
    Write-Host "║  $d"
}
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  KEY: $cacheKey"
Write-Host "╚══════════════════════════════════════════════════════════════╝"

Write-Host "##vso[task.setvariable variable=NATIVE_CACHE_KEY]$cacheKey"
