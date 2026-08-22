[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../../..')).Path
$dotnet = Join-Path $repoRoot '.dotnet/dotnet'
if (-not (Test-Path $dotnet)) {
    $dotnet = 'dotnet'
}

$versionsText = Get-Content (Join-Path $repoRoot 'scripts/VERSIONS.txt') -Raw
$products = @(
    @{
        Prefix = 'SkiaSharp'
        Library = 'libSkiaSharp'
        Version = [regex]::Match($versionsText, '(?m)^SkiaSharp\s+nuget\s+(\S+)').Groups[1].Value
    }
    @{
        Prefix = 'HarfBuzzSharp'
        Library = 'libHarfBuzzSharp'
        Version = [regex]::Match($versionsText, '(?m)^HarfBuzzSharp\s+nuget\s+(\S+)').Groups[1].Value
    }
)
$platforms = @('macOS', 'iOS', 'MacCatalyst', 'tvOS')

function New-NativeFile {
    param(
        [string] $Path,
        [switch] $MachO
    )

    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    $content = if ($MachO) {
        [byte[]](0xcf, 0xfa, 0xed, 0xfe, 1, 2, 3, 4)
    }
    else {
        [Text.Encoding]::UTF8.GetBytes("fixture:$Path")
    }
    [IO.File]::WriteAllBytes($Path, $content)
}

function New-Dsym {
    param(
        [string] $BundlePath,
        [string] $DwarfName
    )

    New-NativeFile (Join-Path $BundlePath "Contents/Resources/DWARF/$DwarfName") -MachO
    New-NativeFile (Join-Path $BundlePath 'Contents/Info.plist')
    New-NativeFile (Join-Path $BundlePath "Contents/Resources/Relocations/test/$DwarfName.yml")
}

function New-Framework {
    param(
        [string] $Path,
        [string] $Library
    )

    New-NativeFile (Join-Path $Path $Library) -MachO
    New-NativeFile (Join-Path $Path 'Info.plist')
}

function New-Fixture {
    param([string] $Root)

    $native = Join-Path $Root 'native'
    foreach ($product in $products) {
        $library = $product.Library

        New-NativeFile (Join-Path $native "osx/$library.dylib") -MachO
        foreach ($arch in @('arm64', 'x86_64')) {
            New-Dsym `
                (Join-Path $native "osx/$library/$arch.xcarchive/dSYMs/$library.dylib.dSYM") `
                "$library.dylib"
        }

        foreach ($rid in @('ios', 'iossimulator', 'tvos', 'tvossimulator')) {
            New-Framework (Join-Path $native "$rid/$library.framework") $library
            foreach ($arch in @('arm64', 'x86_64')) {
                New-Dsym `
                    (Join-Path $native "$rid/$library/$arch.xcarchive/dSYMs/$library.framework.dSYM") `
                    $library
            }
        }

        $catalystFramework = Join-Path $native "maccatalyst/$library.framework"
        New-NativeFile (Join-Path $catalystFramework "Versions/A/$library") -MachO
        [IO.Directory]::CreateDirectory($catalystFramework) | Out-Null
        [IO.Compression.ZipFile]::CreateFromDirectory(
            $catalystFramework,
            "$catalystFramework.zip",
            [IO.Compression.CompressionLevel]::Optimal,
            $true)
        foreach ($arch in @('arm64', 'x86_64')) {
            New-Dsym `
                (Join-Path $native "maccatalyst/$library/$arch.xcarchive/dSYMs/$library.framework.dSYM") `
                $library
        }
    }

    return $native
}

function Invoke-Pack {
    param(
        [string] $Project,
        [string] $NativeOutputPath,
        [string] $PackageOutputPath,
        [bool] $BuildSymbols,
        [bool] $RequireSymbols
    )

    [IO.Directory]::CreateDirectory($PackageOutputPath) | Out-Null
    $nativeOutputPathWithSeparator = $NativeOutputPath.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $arguments = @(
        'pack'
        (Join-Path $repoRoot $Project)
        '--nologo'
        "-p:NativeOutputPath=$nativeOutputPathWithSeparator"
        "-p:PackageOutputPath=$PackageOutputPath"
        '-p:VersionSuffix=symboltest'
        "-p:BuildAppleSymbols=$($BuildSymbols.ToString().ToLowerInvariant())"
        "-p:RequireAppleSymbols=$($RequireSymbols.ToString().ToLowerInvariant())"
    )
    & $dotnet @arguments | Out-Host
    return $LASTEXITCODE
}

function Get-ZipEntries {
    param([string] $Path)

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @{}
        foreach ($entry in $archive.Entries) {
            $stream = $entry.Open()
            try {
                $memory = [IO.MemoryStream]::new()
                $stream.CopyTo($memory)
                $entries[$entry.FullName] = $memory.ToArray()
                $memory.Dispose()
            }
            finally {
                $stream.Dispose()
            }
        }
        return $entries
    }
    finally {
        $archive.Dispose()
    }
}

function Test-BytesEqual {
    param(
        [byte[]] $Left,
        [byte[]] $Right
    )

    return [Convert]::ToBase64String($Left) -ceq [Convert]::ToBase64String($Right)
}

function Get-Project {
    param(
        [string] $Prefix,
        [string] $Platform
    )

    return "binding/$Prefix.NativeAssets.$Platform/$Prefix.NativeAssets.$Platform.csproj"
}

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-apple-symbol-tests-$([Guid]::NewGuid())"
try {
    $native = New-Fixture $testRoot
    $withSymbols = Join-Path $testRoot 'with-symbols'
    $withoutSymbols = Join-Path $testRoot 'without-symbols'

    foreach ($product in $products) {
        foreach ($platform in $platforms) {
            $project = Get-Project $product.Prefix $platform
            if ((Invoke-Pack $project $native $withSymbols $true $true) -ne 0) {
                throw "Packing $project with symbols failed."
            }
            if ((Invoke-Pack $project $native $withoutSymbols $false $false) -ne 0) {
                throw "Packing $project without symbols failed."
            }
        }
    }

    $symbolPackages = @(Get-ChildItem $withSymbols -Filter '*.symbols.nupkg')
    if ($symbolPackages.Count -ne 8) {
        throw "Expected exactly eight Apple symbol packages, found $($symbolPackages.Count)."
    }
    if (@(Get-ChildItem $withoutSymbols -Filter '*.symbols.nupkg').Count -ne 0) {
        throw 'BuildAppleSymbols=false unexpectedly produced symbol packages.'
    }

    foreach ($product in $products) {
        $library = $product.Library
        foreach ($platform in $platforms) {
            $packageId = "$($product.Prefix).NativeAssets.$platform"
            $fileBase = "$packageId.$($product.Version)-symboltest"
            $normalPackage = Join-Path $withSymbols "$fileBase.nupkg"
            $baselinePackage = Join-Path $withoutSymbols "$fileBase.nupkg"
            $symbolPackage = Join-Path $withSymbols "$fileBase.symbols.nupkg"

            $normalEntries = Get-ZipEntries $normalPackage
            $baselineEntries = Get-ZipEntries $baselinePackage
            $symbolEntries = Get-ZipEntries $symbolPackage
            $baselinePayload = @($baselineEntries.GetEnumerator() | Where-Object {
                $_.Key -ne '_rels/.rels' -and
                $_.Key -ne '[Content_Types].xml' -and
                $_.Key -notlike 'package/services/metadata/*'
            })
            $normalPayload = @($normalEntries.GetEnumerator() | Where-Object {
                $_.Key -ne '_rels/.rels' -and
                $_.Key -ne '[Content_Types].xml' -and
                $_.Key -notlike 'package/services/metadata/*'
            })
            if ($normalPayload.Count -ne $baselinePayload.Count) {
                throw "Enabling symbol production changed the normal package payload entry count for $packageId."
            }
            foreach ($entry in $baselinePayload) {
                if (-not $normalEntries.ContainsKey($entry.Key) -or
                    -not (Test-BytesEqual $entry.Value $normalEntries[$entry.Key])) {
                    throw "Enabling symbol production changed '$($entry.Key)' in the normal package $packageId."
                }
            }
            foreach ($entry in $normalPayload) {
                if (-not $symbolEntries.ContainsKey($entry.Key) -or
                    -not (Test-BytesEqual $entry.Value $symbolEntries[$entry.Key])) {
                    throw "$packageId symbols package did not preserve '$($entry.Key)'."
                }
            }

            $dwarfPaths = @($symbolEntries.Keys | Where-Object {
                $_ -match '\.dSYM/Contents/Resources/DWARF/'
            })
            $expectedDwarfs = if ($platform -in @('iOS', 'tvOS')) { 4 } else { 2 }
            if ($dwarfPaths.Count -ne $expectedDwarfs) {
                throw "$packageId contains $($dwarfPaths.Count) dSYM DWARFs instead of $expectedDwarfs."
            }
            foreach ($dwarfPath in $dwarfPaths) {
                if ([IO.Path]::GetFileName($dwarfPath) -ne $library -and
                    [IO.Path]::GetFileName($dwarfPath) -ne "$library.dylib") {
                    throw "$packageId changed the official dSYM DWARF name: $dwarfPath"
                }
            }

            $normalSymbolEntries = @($normalEntries.Keys | Where-Object { $_ -match '\.dSYM/' })
            if ($normalSymbolEntries.Count -ne 0) {
                throw "$packageId customer package contains dSYM files."
            }

            if ($platform -eq 'MacCatalyst') {
                $runtimePath = "runtimes/maccatalyst/native/$library.framework/Versions/A/$library"
                if (-not $symbolEntries.ContainsKey($runtimePath)) {
                    throw "$packageId symbols package is missing '$runtimePath'."
                }
                if (-not (Test-BytesEqual $symbolEntries[$runtimePath][0..3] ([byte[]](0xcf, 0xfa, 0xed, 0xfe)))) {
                    throw "$packageId symbols package contains a non-Mach-O Catalyst runtime."
                }
            }
        }
    }

    $missingDwarf = Join-Path $native 'ios/libSkiaSharp/arm64.xcarchive/dSYMs/libSkiaSharp.framework.dSYM/Contents/Resources/DWARF/libSkiaSharp'
    Remove-Item $missingDwarf

    $strictOutput = Join-Path $testRoot 'strict-missing'
    if ((Invoke-Pack (Get-Project 'SkiaSharp' 'iOS') $native $strictOutput $true $true) -eq 0) {
        throw 'A full package build with a missing arm64 dSYM unexpectedly succeeded.'
    }

    $partialOutput = Join-Path $testRoot 'partial'
    [IO.Directory]::CreateDirectory($partialOutput) | Out-Null
    $staleSymbolPackage = Join-Path $partialOutput "SkiaSharp.NativeAssets.iOS.$($products[0].Version)-symboltest.symbols.nupkg"
    [IO.File]::WriteAllText($staleSymbolPackage, 'stale')
    if ((Invoke-Pack (Get-Project 'SkiaSharp' 'iOS') $native $partialOutput $true $false) -ne 0) {
        throw 'A partial package build failed instead of skipping unavailable symbols.'
    }
    if (@(Get-ChildItem $partialOutput -Filter '*.symbols.nupkg').Count -ne 0) {
        throw 'A partial package build emitted an incomplete symbol package.'
    }
    if (@(Get-ChildItem $partialOutput -Filter '*.nupkg').Count -ne 1) {
        throw 'A partial package build did not emit its normal package.'
    }

    $missingRuntimeRoot = Join-Path $testRoot 'missing-runtime'
    $missingRuntime = New-Fixture $missingRuntimeRoot
    Remove-Item (Join-Path $missingRuntime 'ios/libSkiaSharp.framework/libSkiaSharp')
    if ((Invoke-Pack (Get-Project 'SkiaSharp' 'iOS') $missingRuntime (Join-Path $missingRuntimeRoot 'packages') $true $true) -eq 0) {
        throw 'A full package build with a missing runtime unexpectedly succeeded.'
    }

    $missingPlistRoot = Join-Path $testRoot 'missing-plist'
    $missingPlist = New-Fixture $missingPlistRoot
    Remove-Item (Join-Path $missingPlist 'ios/libSkiaSharp/arm64.xcarchive/dSYMs/libSkiaSharp.framework.dSYM/Contents/Info.plist')
    if ((Invoke-Pack (Get-Project 'SkiaSharp' 'iOS') $missingPlist (Join-Path $missingPlistRoot 'packages') $true $true) -eq 0) {
        throw 'A full package build with an incomplete dSYM unexpectedly succeeded.'
    }

    Write-Host 'Apple symbol package tests passed.'
}
finally {
    if (Test-Path $testRoot) {
        Remove-Item $testRoot -Recurse -Force
    }
}
