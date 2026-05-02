<#
.SYNOPSIS Compute a content-based cache key for a native build job.
.DESCRIPTION
    Reads the dependency graph from scripts/infra/caching/repo-deps.json (generated
    by scripts/generate-native-dep-graph.py) and hashes ALL files that can
    affect the native build output for the given target.

    Falls back to a basic key if the dep graph file is missing.

    The key is emitted as the Azure DevOps pipeline variable NATIVE_CACHE_KEY.

.PARAMETER JobName
    The unique job name (e.g. native_win32_x64_windows). This encodes
    architecture, variant, and feature flags for matrix jobs.

.PARAMETER Target
    The Cake target (e.g. externals-windows, externals-macos,
    externals-linux-clang-cross). Used to locate the platform in the dep graph.

.PARAMETER Docker
    Optional path to the Docker context directory (e.g. scripts/Docker/alpine).
    When provided, all files in that Docker context are included in the hash.

.EXAMPLE
    .\scripts\compute-native-cache-key.ps1 -JobName 'native_win32_x64_windows' -Target 'externals-windows'
.EXAMPLE
    .\scripts\compute-native-cache-key.ps1 -JobName 'native_linux_arm64_alpine_linux' -Target 'externals-linux-clang-cross' -Docker 'scripts/Docker/alpine'
#>
param(
    [Parameter(Mandatory)]
    [string]$JobName,

    [Parameter(Mandatory)]
    [string]$Target,

    [string]$Docker = ''
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Get-FileHashString {
    param([string]$Path)
    if (Test-Path $Path) {
        return (Get-FileHash $Path -Algorithm SHA256).Hash
    }
    return ''
}

# ---------------------------------------------------------------------------
# 1. Skia submodule SHA — uniquely identifies ALL C++ source
# ---------------------------------------------------------------------------
$skiaSha = (git -C externals/skia rev-parse HEAD 2>$null)
if (-not $skiaSha) {
    Write-Warning "Could not read externals/skia HEAD — using 'unknown'"
    $skiaSha = 'unknown'
} else {
    $skiaSha = $skiaSha.Trim()
}

# ---------------------------------------------------------------------------
# 2. Load dependency graph
# ---------------------------------------------------------------------------
$depGraphPath = 'scripts/infra/caching/repo-deps.json'
$platformDir = $Target -replace '^externals-', ''
$dirsToHash = @()

if (Test-Path $depGraphPath) {
    $depGraph = Get-Content $depGraphPath -Raw | ConvertFrom-Json

    $targetInfo = $depGraph.targets.$platformDir
    if ($targetInfo -and $targetInfo.dirs) {
        $dirsToHash = @($targetInfo.dirs)
        Write-Host "Dep graph: found $($dirsToHash.Count) directories for target '$platformDir'"
    } else {
        Write-Warning "Target '$platformDir' not found in dep graph — using fallback"
    }
}

# Add Docker context directory if applicable
if ($Docker -and (Test-Path $Docker)) {
    $dirsToHash = @($dirsToHash) + @($Docker) | Select-Object -Unique
    Write-Host "Dep graph: added Docker context '$Docker'"
}

# Fallback: if dep graph is missing or target not found
if ($dirsToHash.Count -eq 0) {
    Write-Host "Using fallback directory list (dep graph not available)"
    $dirsToHash = @(
        "native/$platformDir",
        'scripts/cake',
        'scripts'
    )
    if ($Docker) {
        $dirsToHash += $Docker
    }
}

# ---------------------------------------------------------------------------
# 3. Hash all files in dependency directories
# ---------------------------------------------------------------------------
$fileHashes = @()
$hashedFiles = @()
$hashedDirs = @()

foreach ($dir in $dirsToHash | Sort-Object) {
    if (-not (Test-Path $dir)) { continue }

    if (Test-Path $dir -PathType Container) {
        # Hash every file in the directory
        $files = Get-ChildItem -Path $dir -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[/\\](bin|obj|libs|tools|\.git)[/\\]' } |
            Sort-Object FullName
        foreach ($file in $files) {
            $hash = Get-FileHashString $file.FullName
            if ($hash) {
                $fileHashes += $hash
                $hashedFiles += $file.FullName
            }
        }
        $hashedDirs += "$dir/ ($($files.Count) files)"
    } else {
        # It's a single file
        $hash = Get-FileHashString $dir
        if ($hash) {
            $fileHashes += $hash
            $hashedFiles += $dir
        }
    }
}

# Combine all file hashes into a single composite hash
$combinedInput = ($fileHashes -join '|')
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($combinedInput)
$compositeHash = ($sha256.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
$compositeHash = $compositeHash.Substring(0, 24)

# ---------------------------------------------------------------------------
# 4. Compose the cache key
# ---------------------------------------------------------------------------
# Key segments:
#   JobName        — unique per matrix combination (encodes arch, variant, features)
#   skiaSha        — all C++ source code state
#   compositeHash  — hash of all dependency files (scripts, Dockerfiles, etc.)
$cacheKey = "native|$JobName|$skiaSha|$compositeHash"

# ---------------------------------------------------------------------------
# 5. Output
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗"
Write-Host "║  Native Build Cache Key                                     ║"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Job:        $JobName"
Write-Host "║  Target:     $Target → $platformDir"
Write-Host "║  Skia SHA:   $skiaSha"
Write-Host "║  Files hash: $compositeHash ($($hashedFiles.Count) files)"
Write-Host "║  Docker:     $(if ($Docker) { $Docker } else { 'none' })"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Hashed directories:"
foreach ($d in $hashedDirs) {
    Write-Host "║    $d"
}
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  KEY: $cacheKey"
Write-Host "╚══════════════════════════════════════════════════════════════╝"

Write-Host "##vso[task.setvariable variable=NATIVE_CACHE_KEY]$cacheKey"
