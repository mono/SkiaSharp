$ErrorActionPreference = 'Stop'

# Free disk space on CI agents by removing pre-installed tools and SDK
# components that are not needed by SkiaSharp builds.
#
# On Linux hosted agents the image ships with many toolchains (Swift,
# Haskell, Julia, Miniconda, AWS CLI, etc.) that SkiaSharp never uses.
# On macOS the pre-installed .NET and Homebrew caches can also be large.
# On Windows this script is currently a no-op but is wired in so that
# platform-specific cleanup can be added later without touching the
# pipeline YAML.

# -----------------------------------------------------------------------
# Helper: silently remove a path (works cross-platform, no error on miss)
# -----------------------------------------------------------------------
function Remove-IfExists {
    param([string[]]$Paths)
    foreach ($p in $Paths) {
        if (Test-Path $p) {
            Write-Host "  Removing $p"
            if ($IsLinux -or $IsMacOS) {
                & sudo rm -rf $p
            } else {
                Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

# -----------------------------------------------------------------------
# Resolve well-known directories from environment variables
# -----------------------------------------------------------------------
$agentTools = $env:AGENT_TOOLSDIRECTORY  # e.g. /opt/hostedtoolcache
$androidSdk = if ($env:ANDROID_HOME) { $env:ANDROID_HOME }
              elseif ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT }
              elseif ($IsLinux) { '/usr/local/lib/android/sdk' }
              else { $null }

# Platforms to keep (from ANDROID_PLATFORM_VERSIONS pipeline variable)
$keepPlatforms = ($env:ANDROID_PLATFORM_VERSIONS ?? '21,35,36') -split ','

Write-Host "=== Free disk space ==="
Write-Host "  OS            : $($IsLinux ? 'Linux' : ($IsMacOS ? 'macOS' : 'Windows'))"
Write-Host "  AGENT_TOOLS   : $agentTools"
Write-Host "  ANDROID_SDK   : $androidSdk"
Write-Host ""

# -----------------------------------------------------------------------
# Pre-installed .NET — we install our own via UseDotNet@2
# -----------------------------------------------------------------------
if ($IsLinux -or $IsMacOS) {
    Write-Host "Removing pre-installed .NET..."
    Remove-IfExists '/usr/share/dotnet', '/usr/local/share/dotnet'

    if ($agentTools) {
        Remove-IfExists (Join-Path $agentTools 'dotnet')
    }
}

# -----------------------------------------------------------------------
# Linux-only: large toolchains that SkiaSharp never uses
# -----------------------------------------------------------------------
if ($IsLinux) {
    Write-Host "Removing unused Linux toolchains..."
    Remove-IfExists '/usr/share/swift'             # ~3.3 GB
    Remove-IfExists '/usr/local/.ghcup'            # ~3.7 GB
    Remove-IfExists '/usr/share/miniconda'         # ~858 MB
    Remove-IfExists '/usr/local/share/powershell'  # ~178 MB (binary stays at /opt/microsoft/powershell)
    Remove-IfExists '/usr/local/share/chromium'
    Remove-IfExists '/usr/local/aws-cli'           # ~255 MB
    Remove-IfExists '/usr/local/aws-sam-cli'       # ~260 MB

    # Julia installs with a version suffix
    Get-ChildItem '/usr/local/julia*' -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-IfExists $_.FullName }

    if ($agentTools) {
        Remove-IfExists (Join-Path $agentTools 'go')      # ~1.1 GB
        Remove-IfExists (Join-Path $agentTools 'CodeQL')   # ~1.7 GB
        Remove-IfExists (Join-Path $agentTools 'Python')   # ~1.7 GB
        Remove-IfExists (Join-Path $agentTools 'PyPy')     # ~524 MB
        Remove-IfExists (Join-Path $agentTools 'Ruby')     # ~312 MB
    }

    # Container and package caches
    Write-Host "Cleaning up Docker and apt caches..."
    & sudo docker system prune -af 2>$null
    & sudo apt-get clean -qq 2>$null
}

# -----------------------------------------------------------------------
# macOS-only: Homebrew and Xcode caches
# -----------------------------------------------------------------------
if ($IsMacOS) {
    Write-Host "Removing unused macOS caches..."
    Remove-IfExists '/usr/local/.ghcup'

    if ($agentTools) {
        Remove-IfExists (Join-Path $agentTools 'go')
        Remove-IfExists (Join-Path $agentTools 'CodeQL')
        Remove-IfExists (Join-Path $agentTools 'Python')
        Remove-IfExists (Join-Path $agentTools 'PyPy')
        Remove-IfExists (Join-Path $agentTools 'Ruby')
    }
}

# -----------------------------------------------------------------------
# Android SDK cleanup (Linux + macOS)
# -----------------------------------------------------------------------
if ($androidSdk -and (Test-Path $androidSdk)) {
    Write-Host "Removing unused Android SDK components from $androidSdk..."
    Remove-IfExists (Join-Path $androidSdk 'ndk')
    Remove-IfExists (Join-Path $androidSdk 'cmake')
    Remove-IfExists (Join-Path $androidSdk 'system-images')
    Remove-IfExists (Join-Path $androidSdk 'sources')
    Remove-IfExists (Join-Path $androidSdk 'extras')
    Remove-IfExists (Join-Path $androidSdk 'emulator')

    # Keep only the latest build-tools version
    $btDir = Join-Path $androidSdk 'build-tools'
    if (Test-Path $btDir) {
        $versions = Get-ChildItem $btDir -Directory | Sort-Object Name
        if ($versions.Count -gt 1) {
            $keep = $versions[-1].Name
            Write-Host "  Keeping build-tools/$keep, removing older versions..."
            $versions | Where-Object { $_.Name -ne $keep } | ForEach-Object {
                Write-Host "    Removing build-tools/$($_.Name)"
                Remove-IfExists $_.FullName
            }
        }
    }

    # Keep only the platforms we need
    $platDir = Join-Path $androidSdk 'platforms'
    if (Test-Path $platDir) {
        Write-Host "  Keeping platforms: $($keepPlatforms -join ', ')"
        Get-ChildItem $platDir -Directory | ForEach-Object {
            $apiLevel = $_.Name -replace '^android-', ''
            if ($apiLevel -notin $keepPlatforms) {
                Write-Host "    Removing platforms/$($_.Name)"
                Remove-IfExists $_.FullName
            }
        }
    }
} elseif ($IsLinux -or $IsMacOS) {
    Write-Host "Android SDK not found at $androidSdk, skipping."
}

# -----------------------------------------------------------------------
# Report results
# -----------------------------------------------------------------------
Write-Host ""
if ($IsLinux -or $IsMacOS) {
    & df -h /
} else {
    Get-Volume | Where-Object { $_.DriveLetter } | Format-Table DriveLetter, FileSystemLabel, @{N='SizeGB';E={[math]::Round($_.Size/1GB,1)}}, @{N='FreeGB';E={[math]::Round($_.SizeRemaining/1GB,1)}}
}
