<#
.SYNOPSIS
Verifies promoted Apple native modules and DWARFs on MSDL and SymWeb.

.DESCRIPTION
Extracts the eight Apple .symbols.nupkg artifacts from a completed package build,
uses dotnet-symbol to retrieve each shipped module and its DWARF from both symbol
servers, and requires the downloaded Mach-O UUID sets to match the packaged module.

.EXAMPLE
./scripts/verify-apple-symbols.ps1 `
    -PackageDirectory ./output/nugets-symbols `
    -SkiaSharpVersion 4.152.0 `
    -HarfBuzzSharpVersion 14.2.1.200

.EXAMPLE
./scripts/verify-apple-symbols.ps1 `
    -PackageDirectory ./output/nugets-symbols `
    -SkiaSharpVersion 4.152.0 `
    -HarfBuzzSharpVersion 14.2.1.200 `
    -SkipServerRetrieval
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PackageDirectory,

    [Parameter(Mandatory = $true)]
    [string] $SkiaSharpVersion,

    [Parameter(Mandatory = $true)]
    [string] $HarfBuzzSharpVersion,

    [string] $DotNetSymbol = 'dotnet-symbol',

    [string] $DwarfDump = 'dwarfdump',

    [switch] $SkipServerRetrieval
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Get-MachOUuids {
    param([string] $Path)

    $output = & $DwarfDump --uuid $Path 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "dwarfdump failed for '$Path': $output"
    }

    $uuids = @(
        $output |
            Select-String -Pattern 'UUID:\s+([0-9A-Fa-f-]+)' -AllMatches |
            ForEach-Object { $_.Matches } |
            ForEach-Object { $_.Groups[1].Value.ToUpperInvariant() } |
            Sort-Object -Unique
    )
    if ($uuids.Count -eq 0) {
        throw "No Mach-O UUIDs were found in '$Path'."
    }
    return $uuids
}

function Assert-UuidSetsEqual {
    param(
        [string[]] $Expected,
        [string[]] $Actual,
        [string] $Description
    )

    $difference = @(Compare-Object $Expected $Actual)
    if ($difference.Count -ne 0) {
        throw "$Description UUID mismatch. Expected [$($Expected -join ', ')], actual [$($Actual -join ', ')]."
    }
}

function Invoke-SymbolRetrieval {
    param(
        [string] $Module,
        [string] $ServerName,
        [string] $ServerSwitch,
        [string] $OutputDirectory
    )

    [IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
    & $DotNetSymbol --symbols --modules $ServerSwitch $Module -o $OutputDirectory --diagnostics
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet-symbol failed to retrieve '$Module' from $ServerName."
    }

    $moduleName = [IO.Path]::GetFileName($Module)
    $downloadedModule = @(
        Get-ChildItem $OutputDirectory -File -Recurse |
            Where-Object Name -CEQ $moduleName
    )
    $downloadedDwarf = @(
        Get-ChildItem $OutputDirectory -File -Recurse |
            Where-Object Name -CEQ "$moduleName.dwarf"
    )
    if ($downloadedModule.Count -ne 1) {
        throw "$ServerName returned $($downloadedModule.Count) modules named '$moduleName'; expected one."
    }
    if ($downloadedDwarf.Count -lt 1) {
        throw "$ServerName did not return '$moduleName.dwarf'."
    }

    $expectedUuids = Get-MachOUuids $Module
    Assert-UuidSetsEqual $expectedUuids (Get-MachOUuids $downloadedModule[0].FullName) "$ServerName module"

    $dwarfUuids = @(
        $downloadedDwarf |
            ForEach-Object { Get-MachOUuids $_.FullName } |
            Sort-Object -Unique
    )
    Assert-UuidSetsEqual $expectedUuids $dwarfUuids "$ServerName DWARF"
}

$packageDirectory = (Resolve-Path $PackageDirectory).Path
$packages = @(
    @{ Id = 'SkiaSharp.NativeAssets.macOS'; Version = $SkiaSharpVersion; Modules = @('runtimes/osx/native/libSkiaSharp.dylib') }
    @{ Id = 'SkiaSharp.NativeAssets.iOS'; Version = $SkiaSharpVersion; Modules = @('runtimes/ios/native/libSkiaSharp.framework/libSkiaSharp', 'runtimes/iossimulator/native/libSkiaSharp.framework/libSkiaSharp') }
    @{ Id = 'SkiaSharp.NativeAssets.MacCatalyst'; Version = $SkiaSharpVersion; Modules = @('runtimes/maccatalyst/native/libSkiaSharp.framework/Versions/A/libSkiaSharp') }
    @{ Id = 'SkiaSharp.NativeAssets.tvOS'; Version = $SkiaSharpVersion; Modules = @('runtimes/tvos/native/libSkiaSharp.framework/libSkiaSharp', 'runtimes/tvossimulator/native/libSkiaSharp.framework/libSkiaSharp') }
    @{ Id = 'HarfBuzzSharp.NativeAssets.macOS'; Version = $HarfBuzzSharpVersion; Modules = @('runtimes/osx/native/libHarfBuzzSharp.dylib') }
    @{ Id = 'HarfBuzzSharp.NativeAssets.iOS'; Version = $HarfBuzzSharpVersion; Modules = @('runtimes/ios/native/libHarfBuzzSharp.framework/libHarfBuzzSharp', 'runtimes/iossimulator/native/libHarfBuzzSharp.framework/libHarfBuzzSharp') }
    @{ Id = 'HarfBuzzSharp.NativeAssets.MacCatalyst'; Version = $HarfBuzzSharpVersion; Modules = @('runtimes/maccatalyst/native/libHarfBuzzSharp.framework/Versions/A/libHarfBuzzSharp') }
    @{ Id = 'HarfBuzzSharp.NativeAssets.tvOS'; Version = $HarfBuzzSharpVersion; Modules = @('runtimes/tvos/native/libHarfBuzzSharp.framework/libHarfBuzzSharp', 'runtimes/tvossimulator/native/libHarfBuzzSharp.framework/libHarfBuzzSharp') }
)

$tempRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-symbol-verification-$([Guid]::NewGuid())"
try {
    foreach ($package in $packages) {
        $packagePath = Join-Path $packageDirectory "$($package.Id).$($package.Version).symbols.nupkg"
        if (-not (Test-Path $packagePath -PathType Leaf)) {
            throw "Expected promoted symbol package was not found: $packagePath"
        }

        $extractDirectory = Join-Path $tempRoot $package.Id
        [IO.Compression.ZipFile]::ExtractToDirectory($packagePath, $extractDirectory)
        foreach ($modulePath in $package.Modules) {
            $module = Join-Path $extractDirectory $modulePath
            if (-not (Test-Path $module -PathType Leaf)) {
                throw "$($package.Id) is missing module '$modulePath'."
            }

            $runtimeIdentifier = $modulePath.Split('/')[1]
            $packagedDwarfs = @(
                Get-ChildItem (Join-Path $extractDirectory "runtimes/$runtimeIdentifier/native/symbols") -Filter '*.dwarf' -File -Recurse
            )
            if ($packagedDwarfs.Count -eq 0) {
                throw "$($package.Id) has no packaged DWARFs for '$runtimeIdentifier'."
            }

            $moduleUuids = Get-MachOUuids $module
            $packagedDwarfUuids = @(
                $packagedDwarfs |
                    ForEach-Object { Get-MachOUuids $_.FullName } |
                    Sort-Object -Unique
            )
            Assert-UuidSetsEqual $moduleUuids $packagedDwarfUuids "$($package.Id) $runtimeIdentifier package"

            if ($SkipServerRetrieval) {
                continue
            }

            $safeModuleName = $modulePath -replace '[^A-Za-z0-9_.-]', '_'
            Invoke-SymbolRetrieval $module 'MSDL' '--microsoft-symbol-server' (Join-Path $tempRoot "$($package.Id)/$safeModuleName/msdl")
            Invoke-SymbolRetrieval $module 'SymWeb' '--internal-server' (Join-Path $tempRoot "$($package.Id)/$safeModuleName/symweb")
        }
    }

    if ($SkipServerRetrieval) {
        Write-Host 'All packaged Apple runtime and DWARF UUID sets match.'
    }
    else {
        Write-Host 'All Apple modules and DWARFs were retrieved from MSDL and SymWeb with matching UUID sets.'
    }
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item $tempRoot -Recurse -Force
    }
}
