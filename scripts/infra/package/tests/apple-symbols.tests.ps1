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

function Invoke-Pack {
    param(
        [string] $Project,
        [string] $PackageOutputPath,
        [string] $VersionSuffix = '',
        [bool] $RequireSymbols = $true
    )

    [IO.Directory]::CreateDirectory($PackageOutputPath) | Out-Null
    $arguments = @(
        'pack'
        $Project
        '--nologo'
        "-p:PackageOutputPath=$PackageOutputPath"
        "-p:RequireNativeSymbols=$($RequireSymbols.ToString().ToLowerInvariant())"
    )
    if ($VersionSuffix) {
        $arguments += "-p:VersionSuffix=$VersionSuffix"
    }

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

function Test-MachO {
    param([byte[]] $Bytes)

    $magic = ($Bytes[0..3] | ForEach-Object { $_.ToString('x2') }) -join ''
    return $magic -in @(
        'feedface', 'cefaedfe',
        'feedfacf', 'cffaedfe',
        'cafebabe', 'bebafeca',
        'cafebabf', 'bfbafeca'
    )
}

function Get-Project {
    param(
        [string] $Prefix,
        [string] $Platform
    )

    return Join-Path $repoRoot "binding/$Prefix.NativeAssets.$Platform/$Prefix.NativeAssets.$Platform.csproj"
}

function Assert-Packages {
    param(
        [string] $PackageDirectory,
        [string] $VersionSuffix
    )

    foreach ($product in $products) {
        $library = $product.Library
        foreach ($platform in $platforms) {
            $packageId = "$($product.Prefix).NativeAssets.$platform"
            $version = $product.Version
            if ($VersionSuffix) {
                $version = "$version-$VersionSuffix"
            }

            $normalPackage = Join-Path $PackageDirectory "$packageId.$version.nupkg"
            $symbolPackage = Join-Path $PackageDirectory "$packageId.$version.symbols.nupkg"
            $normalEntries = Get-ZipEntries $normalPackage
            $symbolEntries = Get-ZipEntries $symbolPackage

            $normalPayload = @($normalEntries.GetEnumerator() | Where-Object {
                $_.Key -ne '_rels/.rels' -and
                $_.Key -ne '[Content_Types].xml' -and
                $_.Key -notlike 'package/services/metadata/*'
            })
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

            if (@($normalEntries.Keys | Where-Object { $_ -match '\.dSYM/' }).Count -ne 0) {
                throw "$packageId customer package contains dSYM files."
            }

            $expectedNormalRuntimeEntries = switch ($platform) {
                'iOS' {
                    @(
                        "runtimes/ios/native/$library.framework/Info.plist"
                        "runtimes/ios/native/$library.framework/$library"
                        "runtimes/iossimulator/native/$library.framework/Info.plist"
                        "runtimes/iossimulator/native/$library.framework/$library"
                    )
                }
                'MacCatalyst' {
                    @("runtimes/maccatalyst/native/$library.framework.zip")
                }
                default {
                    @()
                }
            }
            foreach ($expectedEntry in $expectedNormalRuntimeEntries) {
                if (-not $normalEntries.ContainsKey($expectedEntry)) {
                    throw "$packageId customer package is missing expected runtime entry '$expectedEntry'."
                }
            }

            if ($platform -eq 'MacCatalyst') {
                $runtimePath = "runtimes/maccatalyst/native/$library.framework/Versions/A/$library"
                if (-not $symbolEntries.ContainsKey($runtimePath)) {
                    throw "$packageId symbols package is missing '$runtimePath'."
                }
                if (-not (Test-MachO $symbolEntries[$runtimePath])) {
                    throw "$packageId symbols package contains a non-Mach-O Catalyst runtime."
                }
            }
        }
    }
}

function New-FixtureProject {
    param(
        [string] $Root,
        [switch] $MissingRuntime,
        [switch] $MissingDwarf,
        [switch] $MissingPlist
    )

    $native = Join-Path $Root 'native'
    $framework = Join-Path $native 'ios/libSkiaSharp.framework'
    $module = Join-Path $framework 'libSkiaSharp'
    [IO.Directory]::CreateDirectory($framework) | Out-Null
    [IO.File]::WriteAllBytes((Join-Path $framework 'Info.plist'), [byte[]](1))
    if (-not $MissingRuntime) {
        [IO.File]::WriteAllBytes($module, [byte[]](0xcf, 0xfa, 0xed, 0xfe))
    }

    $dsym = Join-Path $native 'ios/libSkiaSharp.framework.dSYM'
    $dwarf = Join-Path $dsym 'Contents/Resources/DWARF/libSkiaSharp'
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($dwarf)) | Out-Null
    if (-not $MissingDwarf) {
        [IO.File]::WriteAllBytes($dwarf, [byte[]](0xcf, 0xfa, 0xed, 0xfe))
    }
    if (-not $MissingPlist) {
        [IO.File]::WriteAllBytes((Join-Path $dsym 'Contents/Info.plist'), [byte[]](1))
    }

    $targets = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'binding/NativeAssets.Build.targets'))
    $nativeXml = [Security.SecurityElement]::Escape($native)
    $project = Join-Path $Root 'NativeSymbols.Fixture.csproj'
    @"
<Project Sdk="Microsoft.Build.NoTargets/3.7.0">
  <PropertyGroup>
    <TargetFrameworks>net10.0-ios26.0</TargetFrameworks>
    <PackageId>NativeSymbols.Fixture</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>SkiaSharp</Authors>
    <Description>Native symbol packaging fixture.</Description>
    <PackagingGroup>SkiaSharp</PackagingGroup>
  </PropertyGroup>
  <ItemGroup>
    <NativeRuntimeBundle Include="$nativeXml/ios/libSkiaSharp.framework" ModuleName="libSkiaSharp" PackagePath="runtimes/ios/native/libSkiaSharp.framework" />
    <NativeSymbolBundle Include="$nativeXml/ios/libSkiaSharp.framework.dSYM" DwarfName="libSkiaSharp" PackagePath="runtimes/ios/native/symbols/arm64/libSkiaSharp.framework.dSYM" />
  </ItemGroup>
  <Import Project="$targets" />
  <Target Name="CollectAssemblyVersionInfo" />
</Project>
"@ | Set-Content $project
    return $project
}

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-native-symbol-tests-$([Guid]::NewGuid())"
try {
    $packages = Join-Path $testRoot 'packages'
    foreach ($versionSuffix in @('', 'preview.symboltest')) {
        foreach ($product in $products) {
            foreach ($platform in $platforms) {
                $project = Get-Project $product.Prefix $platform
                if ((Invoke-Pack $project $packages $versionSuffix $true) -ne 0) {
                    throw "Packing $project failed for suffix '$versionSuffix'."
                }
            }
        }
        Assert-Packages $packages $versionSuffix
    }

    if (@(Get-ChildItem $packages -Filter '*.symbols.nupkg').Count -ne 16) {
        throw 'Expected eight stable and eight preview native symbol packages.'
    }

    $missingRuntimeRoot = Join-Path $testRoot 'missing-runtime'
    $missingRuntimeProject = New-FixtureProject $missingRuntimeRoot -MissingRuntime
    $missingRuntimePackages = Join-Path $missingRuntimeRoot 'packages'
    [IO.Directory]::CreateDirectory($missingRuntimePackages) | Out-Null
    [IO.File]::WriteAllText(
        (Join-Path $missingRuntimePackages 'NativeSymbols.Fixture.1.0.0.symbols.nupkg'),
        'stale')
    if ((Invoke-Pack $missingRuntimeProject $missingRuntimePackages '' $true) -eq 0) {
        throw 'A full package build with a missing runtime unexpectedly succeeded.'
    }
    if (@(Get-ChildItem $missingRuntimePackages -Filter '*.nupkg').Count -ne 0) {
        throw 'A failed full package build left a normal or stale symbol package behind.'
    }

    foreach ($missing in @('Dwarf', 'Plist')) {
        $root = Join-Path $testRoot "missing-$($missing.ToLowerInvariant())"
        $project = if ($missing -eq 'Dwarf') {
            New-FixtureProject $root -MissingDwarf
        }
        else {
            New-FixtureProject $root -MissingPlist
        }

        $strictPackages = Join-Path $root 'strict'
        if ((Invoke-Pack $project $strictPackages '' $true) -eq 0) {
            throw "A full package build with a missing $missing unexpectedly succeeded."
        }

        $partialPackages = Join-Path $root 'partial'
        [IO.Directory]::CreateDirectory($partialPackages) | Out-Null
        [IO.File]::WriteAllText(
            (Join-Path $partialPackages 'NativeSymbols.Fixture.1.0.0.symbols.nupkg'),
            'stale')
        if ((Invoke-Pack $project $partialPackages '' $false) -ne 0) {
            throw "A partial package build failed instead of skipping a missing $missing."
        }
        if (@(Get-ChildItem $partialPackages -Filter '*.symbols.nupkg').Count -ne 0) {
            throw "A partial package build emitted a stale or incomplete symbol package for a missing $missing."
        }
        if (@(Get-ChildItem $partialPackages -Filter '*.nupkg').Count -ne 1) {
            throw "A partial package build did not emit its normal package for a missing $missing."
        }
    }

    Write-Host 'Native symbol package tests passed.'
}
finally {
    if (Test-Path $testRoot) {
        Remove-Item $testRoot -Recurse -Force
    }
}
