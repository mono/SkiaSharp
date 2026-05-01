<#
.SYNOPSIS Compute a content-based cache key for a native build job.
.DESCRIPTION
    Produces a deterministic cache key from the Skia submodule commit SHA,
    platform-specific build script hash, shared infrastructure hashes, and
    (optionally) the Docker image Dockerfile hash.

    The key is emitted as the Azure DevOps pipeline variable NATIVE_CACHE_KEY.

.PARAMETER JobName
    The unique job name (e.g. native_win32_x64_windows). This encodes
    architecture, variant, and feature flags for matrix jobs.

.PARAMETER Target
    The Cake target (e.g. externals-windows, externals-macos,
    externals-linux-clang-cross). Used to locate the platform build script.

.PARAMETER Docker
    Optional path to the Docker context directory (e.g. scripts/Docker/alpine).
    When provided, the Dockerfile hash is included in the cache key.

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

function Get-TruncatedFileHash {
    param([string]$Path, [int]$Length = 16)
    if (Test-Path $Path) {
        return (Get-FileHash $Path -Algorithm SHA256).Hash.Substring(0, $Length)
    }
    return 'missing'
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
# 2. Platform-specific build script
# ---------------------------------------------------------------------------
# Map Cake target to the native/<platform> directory.
# Targets like "externals-linux-clang-cross" → "linux-clang-cross"
# Targets like "externals-windows" → "windows"
# Targets like "externals-macos" → "macos"
# Targets like "externals-winui-angle" → "winui-angle"
$platformDir = $Target -replace '^externals-', ''

$buildCake = "native/$platformDir/build.cake"
if (-not (Test-Path $buildCake)) {
    # Fallback: some targets share a build directory.
    # e.g. externals-linux uses native/linux/build.cake
    # but externals-linux-clang-cross uses native/linux-clang-cross/build.cake
    $fallback = $platformDir -replace '-clang-cross$', ''
    $buildCake = "native/$fallback/build.cake"
}

$buildCakeHash = Get-TruncatedFileHash $buildCake

# ---------------------------------------------------------------------------
# 3. Shared build infrastructure
# ---------------------------------------------------------------------------
$sharedCakeHash = Get-TruncatedFileHash 'scripts/cake/native-shared.cake'
$sharedCakeCommonHash = Get-TruncatedFileHash 'scripts/cake/shared.cake'
$versionsHash = Get-TruncatedFileHash 'scripts/VERSIONS.txt'

# Also hash xcode.cake if it exists (used by Apple platforms)
$xcodeCakeHash = 'none'
if ($platformDir -match '^(macos|ios|maccatalyst|tvos)$' -or $Target -match 'android') {
    $xcodeCakeHash = Get-TruncatedFileHash 'scripts/cake/xcode.cake'
}

# ---------------------------------------------------------------------------
# 4. Docker image definition (Linux/WASM builds only)
# ---------------------------------------------------------------------------
$dockerHash = 'none'
if ($Docker -and (Test-Path "$Docker/Dockerfile")) {
    $dockerHash = Get-TruncatedFileHash "$Docker/Dockerfile"
}

# ---------------------------------------------------------------------------
# 5. Compose the cache key
# ---------------------------------------------------------------------------
# Key segments:
#   JobName     — unique per matrix combination (encodes arch, variant, features)
#   skiaSha     — all C++ source code state
#   versions    — version numbers, sonames, milestone
#   buildCake   — platform-specific build logic
#   shared      — shared Cake helper scripts
#   docker      — container image definition (if applicable)
$cacheKey = "native|$JobName|$skiaSha|$versionsHash|$buildCakeHash|$sharedCakeHash|$sharedCakeCommonHash|$xcodeCakeHash|$dockerHash"

# ---------------------------------------------------------------------------
# 6. Output
# ---------------------------------------------------------------------------
Write-Host "╔══════════════════════════════════════════════════════════════╗"
Write-Host "║  Native Build Cache Key                                     ║"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  Job:          $JobName"
Write-Host "║  Skia SHA:     $skiaSha"
Write-Host "║  VERSIONS:     $versionsHash"
Write-Host "║  Build cake:   $buildCakeHash  ($buildCake)"
Write-Host "║  Shared:       $sharedCakeHash  (native-shared.cake)"
Write-Host "║  Common:       $sharedCakeCommonHash  (shared.cake)"
Write-Host "║  Xcode:        $xcodeCakeHash"
Write-Host "║  Docker:       $dockerHash  ($Docker)"
Write-Host "╠══════════════════════════════════════════════════════════════╣"
Write-Host "║  KEY: $cacheKey"
Write-Host "╚══════════════════════════════════════════════════════════════╝"

Write-Host "##vso[task.setvariable variable=NATIVE_CACHE_KEY]$cacheKey"
