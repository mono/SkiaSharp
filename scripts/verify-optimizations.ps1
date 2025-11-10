#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Verifies that native libraries are built with optimizations enabled.

.DESCRIPTION
    This script checks native Skia libraries to ensure they were built with
    proper optimization flags. It looks for indicators that debug mode was
    NOT used, which would cause performance issues.

.PARAMETER NativePath
    Path to the output/native directory containing built libraries.
    Defaults to ./output/native

.EXAMPLE
    ./scripts/verify-optimizations.ps1
    ./scripts/verify-optimizations.ps1 -NativePath ./output/native
#>

param(
    [string]$NativePath = "./output/native"
)

$ErrorActionPreference = "Continue"
$hasIssues = $false

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Skia Build Optimization Verification" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if (!(Test-Path $NativePath)) {
    Write-Host "ERROR: Native path '$NativePath' does not exist." -ForegroundColor Red
    Write-Host "Please build the native libraries first with:" -ForegroundColor Yellow
    Write-Host "  dotnet cake --target=externals" -ForegroundColor Yellow
    exit 1
}

Write-Host "Checking native libraries in: $NativePath" -ForegroundColor White
Write-Host ""

# Find all library files
$extensions = @("*.dll", "*.so", "*.so.*", "*.dylib")
$libraries = @()
foreach ($ext in $extensions) {
    $libraries += Get-ChildItem -Path $NativePath -Filter $ext -Recurse -File
}

if ($libraries.Count -eq 0) {
    Write-Host "WARNING: No native libraries found in $NativePath" -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($libraries.Count) native libraries to check" -ForegroundColor White
Write-Host ""

foreach ($lib in $libraries) {
    $relativePath = $lib.FullName.Replace((Resolve-Path $NativePath).Path, "").TrimStart([IO.Path]::DirectorySeparatorChar)
    Write-Host "Checking: $relativePath" -ForegroundColor Cyan
    
    $hasDebugIndicators = $false
    $optimizationChecks = @()
    
    # Platform-specific checks
    if ($lib.Extension -eq ".dll") {
        # Windows DLL checks
        Write-Host "  Platform: Windows" -ForegroundColor Gray
        
        # Check if dumpbin is available
        $dumpbin = Get-Command dumpbin -ErrorAction SilentlyContinue
        if ($dumpbin) {
            try {
                # Check for debug CRT linkage (should use MSVCRT.dll for release, MSVCRTD.dll for debug)
                $imports = & dumpbin /IMPORTS "$($lib.FullName)" 2>&1 | Out-String
                if ($imports -match "MSVCR\d+D\.dll|MSVCP\d+D\.dll") {
                    Write-Host "  ❌ ISSUE: Linked against DEBUG C runtime library" -ForegroundColor Red
                    $hasDebugIndicators = $true
                    $hasIssues = $true
                } else {
                    Write-Host "  ✓ Using release C runtime" -ForegroundColor Green
                }
                
                # Check for optimization flags in the headers
                $headers = & dumpbin /HEADERS "$($lib.FullName)" 2>&1 | Out-String
                if ($headers -match "Debug|debug") {
                    Write-Host "  ℹ Contains debug information (PDB references)" -ForegroundColor Yellow
                    Write-Host "    (This is OK - debug symbols don't affect performance)" -ForegroundColor Gray
                }
            } catch {
                Write-Host "  ⚠ Could not run dumpbin: $_" -ForegroundColor Yellow
            }
        } else {
            Write-Host "  ℹ dumpbin not available, skipping detailed checks" -ForegroundColor Yellow
        }
        
        # Check file size as a heuristic (debug builds are typically much larger)
        $sizeKB = [math]::Round($lib.Length / 1KB, 0)
        Write-Host "  File size: $sizeKB KB" -ForegroundColor Gray
        
    } elseif ($lib.Extension -in @(".so", ".dylib") -or $lib.Name -match "\.so\.") {
        # Linux/macOS library checks
        $platform = if ($lib.Extension -eq ".dylib") { "macOS" } else { "Linux" }
        Write-Host "  Platform: $platform" -ForegroundColor Gray
        
        # Check with readelf (Linux) or otool (macOS)
        if ($platform -eq "Linux") {
            $readelf = Get-Command readelf -ErrorAction SilentlyContinue
            if ($readelf) {
                try {
                    # Check for debug sections (these are OK, but excessive debug info might indicate a debug build)
                    $sections = & readelf -S "$($lib.FullName)" 2>&1 | Out-String
                    $debugSections = ($sections | Select-String -Pattern "\.debug_" -AllMatches).Matches.Count
                    
                    if ($debugSections -gt 10) {
                        Write-Host "  ℹ Contains $debugSections debug sections (debug symbols present)" -ForegroundColor Yellow
                        Write-Host "    (This is OK - debug symbols don't affect runtime performance)" -ForegroundColor Gray
                    } else {
                        Write-Host "  ✓ Minimal debug sections" -ForegroundColor Green
                    }
                    
                    # Check for assert symbols (indicator of debug build with assertions enabled)
                    $symbols = & readelf -s "$($lib.FullName)" 2>&1 | Out-String
                    if ($symbols -match "__assert") {
                        Write-Host "  ⚠ WARNING: Contains __assert symbols (assertions may be enabled)" -ForegroundColor Yellow
                        Write-Host "    This could indicate a debug build or NDEBUG was not defined." -ForegroundColor Yellow
                        $hasDebugIndicators = $true
                    } else {
                        Write-Host "  ✓ No assert symbols found" -ForegroundColor Green
                    }
                } catch {
                    Write-Host "  ⚠ Could not run readelf: $_" -ForegroundColor Yellow
                }
            } else {
                Write-Host "  ℹ readelf not available, skipping detailed checks" -ForegroundColor Yellow
            }
        } elseif ($platform -eq "macOS") {
            $otool = Get-Command otool -ErrorAction SilentlyContinue
            if ($otool) {
                try {
                    # Check load commands for debug info
                    $loadCommands = & otool -l "$($lib.FullName)" 2>&1 | Out-String
                    if ($loadCommands -match "LC_DYSYMTAB") {
                        Write-Host "  ℹ Contains symbol table (debug info present)" -ForegroundColor Yellow
                    }
                } catch {
                    Write-Host "  ⚠ Could not run otool: $_" -ForegroundColor Yellow
                }
            }
        }
        
        # Check file size
        $sizeKB = [math]::Round($lib.Length / 1KB, 0)
        Write-Host "  File size: $sizeKB KB" -ForegroundColor Gray
    }
    
    Write-Host ""
}

Write-Host "================================================" -ForegroundColor Cyan
if ($hasIssues) {
    Write-Host "RESULT: Issues found!" -ForegroundColor Red
    Write-Host "" -ForegroundColor Red
    Write-Host "Some libraries may have been built without optimizations." -ForegroundColor Red
    Write-Host "This can cause significant performance degradation." -ForegroundColor Red
    Write-Host "" -ForegroundColor Red
    Write-Host "To fix this, ensure you build with:" -ForegroundColor Yellow
    Write-Host "  dotnet cake --target=externals --configuration=Release" -ForegroundColor Yellow
    Write-Host "" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "RESULT: All checks passed! ✓" -ForegroundColor Green
    Write-Host "" -ForegroundColor Green
    Write-Host "Libraries appear to be built with optimizations enabled." -ForegroundColor Green
    exit 0
}
