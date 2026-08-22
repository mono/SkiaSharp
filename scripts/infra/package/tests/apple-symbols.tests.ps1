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
$skiaVersion = [regex]::Match($versionsText, '(?m)^SkiaSharp\s+nuget\s+(\S+)').Groups[1].Value
$harfBuzzVersion = [regex]::Match($versionsText, '(?m)^HarfBuzzSharp\s+nuget\s+(\S+)').Groups[1].Value
if (-not $skiaVersion -or -not $harfBuzzVersion) {
    throw 'Unable to read package versions from scripts/VERSIONS.txt.'
}

$products = @(
    @{ Prefix = 'SkiaSharp'; Library = 'libSkiaSharp'; Version = $skiaVersion }
    @{ Prefix = 'HarfBuzzSharp'; Library = 'libHarfBuzzSharp'; Version = $harfBuzzVersion }
)

function Add-ZipEntry {
    param(
        [System.IO.Compression.ZipArchive] $Archive,
        [string] $Path,
        [byte[]] $Content
    )

    $entry = $Archive.CreateEntry($Path)
    $stream = $entry.Open()
    try {
        $stream.Write($Content, 0, $Content.Length)
    }
    finally {
        $stream.Dispose()
    }
}

function New-TestPackage {
    param(
        [string] $Path,
        [string] $PackageId,
        [string] $Version,
        [string[]] $RuntimeEntries
    )

    $archive = [System.IO.Compression.ZipFile]::Open($Path, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $nuspec = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>$PackageId</id>
    <version>$Version</version>
    <authors>Microsoft</authors>
    <description>Test package metadata.</description>
  </metadata>
</package>
"@
        $contentTypes = @"
<?xml version="1.0" encoding="utf-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="nuspec" ContentType="application/octet" />
  <Default Extension="md" ContentType="application/octet" />
</Types>
"@
        Add-ZipEntry $archive "$PackageId.nuspec" ([Text.Encoding]::UTF8.GetBytes($nuspec))
        Add-ZipEntry $archive '[Content_Types].xml' ([Text.Encoding]::UTF8.GetBytes($contentTypes))
        Add-ZipEntry $archive 'README.md' ([Text.Encoding]::UTF8.GetBytes('unchanged runtime package metadata'))
        foreach ($runtimeEntry in $RuntimeEntries) {
            Add-ZipEntry $archive $runtimeEntry ([byte[]](0xcf, 0xfa, 0xed, 0xfe, 1, 2, 3, 4))
        }
    }
    finally {
        $archive.Dispose()
    }
}

function New-NativeFile {
    param(
        [string] $Path,
        [switch] $MachO
    )

    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    $content = if ($MachO) {
        [byte[]](0xcf, 0xfa, 0xed, 0xfe, 5, 6, 7, 8)
    }
    else {
        [byte[]](9, 10, 11, 12)
    }
    [IO.File]::WriteAllBytes($Path, $content)
}

function New-Fixture {
    param(
        [string] $Root,
        [string[]] $Platforms = @('macOS', 'iOS', 'MacCatalyst', 'tvOS')
    )

    $packages = Join-Path $Root 'packages'
    $native = Join-Path $Root 'native'
    $symbols = Join-Path $Root 'symbols'
    [IO.Directory]::CreateDirectory($packages) | Out-Null
    [IO.Directory]::CreateDirectory($native) | Out-Null
    [IO.Directory]::CreateDirectory($symbols) | Out-Null

    foreach ($product in $products) {
        $library = $product.Library

        if ($Platforms -contains 'macOS') {
            $id = "$($product.Prefix).NativeAssets.macOS"
            New-TestPackage (Join-Path $packages "$id.$($product.Version).nupkg") $id $product.Version @(
                "runtimes/osx/native/$library.dylib"
            )
            foreach ($arch in @('arm64', 'x86_64')) {
                New-NativeFile (Join-Path $native "osx/$library/$arch.xcarchive/dSYMs/$library.dylib.dSYM/Contents/Resources/DWARF/$library.dylib")
            }
        }

        if ($Platforms -contains 'iOS') {
            $id = "$($product.Prefix).NativeAssets.iOS"
            New-TestPackage (Join-Path $packages "$id.$($product.Version).nupkg") $id $product.Version @(
                "runtimes/ios/native/$library.framework/$library"
                "runtimes/iossimulator/native/$library.framework/$library"
            )
            foreach ($rid in @('ios', 'iossimulator')) {
                foreach ($arch in @('arm64', 'x86_64')) {
                    New-NativeFile (Join-Path $native "$rid/$library/$arch.xcarchive/dSYMs/$library.framework.dSYM/Contents/Resources/DWARF/$library")
                }
            }
        }

        if ($Platforms -contains 'MacCatalyst') {
            $id = "$($product.Prefix).NativeAssets.MacCatalyst"
            New-TestPackage (Join-Path $packages "$id.$($product.Version).nupkg") $id $product.Version @(
                "runtimes/maccatalyst/native/$library.framework.zip"
            )
            New-NativeFile (Join-Path $native "maccatalyst/$library.framework/Versions/A/$library") -MachO
            foreach ($arch in @('arm64', 'x86_64')) {
                New-NativeFile (Join-Path $native "maccatalyst/$library/$arch.xcarchive/dSYMs/$library.framework.dSYM/Contents/Resources/DWARF/$library")
            }
        }

        if ($Platforms -contains 'tvOS') {
            $id = "$($product.Prefix).NativeAssets.tvOS"
            New-TestPackage (Join-Path $packages "$id.$($product.Version).nupkg") $id $product.Version @(
                "runtimes/tvos/native/$library.framework/$library"
                "runtimes/tvossimulator/native/$library.framework/$library"
            )
            foreach ($rid in @('tvos', 'tvossimulator')) {
                foreach ($arch in @('arm64', 'x86_64')) {
                    New-NativeFile (Join-Path $native "$rid/$library/$arch.xcarchive/dSYMs/$library.framework.dSYM/Contents/Resources/DWARF/$library")
                }
            }
        }
    }

    return @{
        Packages = $packages
        Native = $native
        Symbols = $symbols
    }
}

function Invoke-Pack {
    param(
        [hashtable] $Fixture,
        [bool] $RequireAll
    )

    $arguments = @(
        'cake'
        (Join-Path $repoRoot 'scripts/infra/package/nuget.cake')
        '--target=nuget-apple-symbols'
        "--appleSymbolsPackagePath=$($Fixture.Packages)"
        "--appleSymbolsNativePath=$($Fixture.Native)"
        "--appleSymbolsOutputPath=$($Fixture.Symbols)"
        "--requireAppleSymbols=$($RequireAll.ToString().ToLowerInvariant())"
    )
    & $dotnet @arguments | Out-Host
    return $LASTEXITCODE
}

function Get-ZipEntries {
    param([string] $Path)

    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $result = @{}
        foreach ($entry in $archive.Entries) {
            $stream = $entry.Open()
            try {
                $memory = [IO.MemoryStream]::new()
                $stream.CopyTo($memory)
                $result[$entry.FullName] = $memory.ToArray()
                $memory.Dispose()
            }
            finally {
                $stream.Dispose()
            }
        }
        return $result
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

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-apple-symbol-tests-$([Guid]::NewGuid())"
try {
    $full = New-Fixture (Join-Path $testRoot 'full')
    $hashes = @{}
    Get-ChildItem $full.Packages -Filter '*.nupkg' | ForEach-Object {
        $hashes[$_.Name] = (Get-FileHash $_.FullName -Algorithm SHA256).Hash
    }

    if ((Invoke-Pack $full $true) -ne 0) {
        throw 'The full Apple symbol package fixture failed.'
    }

    $symbolPackages = @(Get-ChildItem $full.Symbols -Filter '*.symbols.nupkg')
    if ($symbolPackages.Count -ne 8) {
        throw "Expected exactly eight Apple symbol packages, found $($symbolPackages.Count)."
    }

    foreach ($normalPackage in Get-ChildItem $full.Packages -Filter '*.nupkg') {
        $afterHash = (Get-FileHash $normalPackage.FullName -Algorithm SHA256).Hash
        if ($hashes[$normalPackage.Name] -ne $afterHash) {
            throw "Normal package changed while creating symbols: $($normalPackage.Name)"
        }

        $symbolName = $normalPackage.Name -replace '\.nupkg$', '.symbols.nupkg'
        $symbolPackage = Join-Path $full.Symbols $symbolName
        $normalEntries = Get-ZipEntries $normalPackage.FullName
        $symbolEntries = Get-ZipEntries $symbolPackage
        foreach ($entry in $normalEntries.GetEnumerator()) {
            if ($entry.Key -eq '[Content_Types].xml') {
                continue
            }
            if (-not $symbolEntries.ContainsKey($entry.Key)) {
                throw "$symbolName dropped normal package entry $($entry.Key)."
            }
            if (-not (Test-BytesEqual $entry.Value $symbolEntries[$entry.Key])) {
                throw "$symbolName changed normal package entry $($entry.Key)."
            }
        }
    }

    foreach ($product in $products) {
        $library = $product.Library
        $expectedDwarfs = @{
            "$($product.Prefix).NativeAssets.macOS" = @(
                "runtimes/osx/native/symbols/arm64/$library.dylib.dwarf"
                "runtimes/osx/native/symbols/x86_64/$library.dylib.dwarf"
            )
            "$($product.Prefix).NativeAssets.iOS" = @(
                "runtimes/ios/native/symbols/arm64/$library.dwarf"
                "runtimes/ios/native/symbols/x86_64/$library.dwarf"
                "runtimes/iossimulator/native/symbols/arm64/$library.dwarf"
                "runtimes/iossimulator/native/symbols/x86_64/$library.dwarf"
            )
            "$($product.Prefix).NativeAssets.MacCatalyst" = @(
                "runtimes/maccatalyst/native/symbols/arm64/$library.dwarf"
                "runtimes/maccatalyst/native/symbols/x86_64/$library.dwarf"
            )
            "$($product.Prefix).NativeAssets.tvOS" = @(
                "runtimes/tvos/native/symbols/arm64/$library.dwarf"
                "runtimes/tvos/native/symbols/x86_64/$library.dwarf"
                "runtimes/tvossimulator/native/symbols/arm64/$library.dwarf"
                "runtimes/tvossimulator/native/symbols/x86_64/$library.dwarf"
            )
        }

        $catalyst = Join-Path $full.Symbols "$($product.Prefix).NativeAssets.MacCatalyst.$($product.Version).symbols.nupkg"
        $entries = Get-ZipEntries $catalyst
        $runtimePath = "runtimes/maccatalyst/native/$library.framework/Versions/A/$library"
        if (-not $entries.ContainsKey($runtimePath)) {
            throw "Mac Catalyst symbol package is missing $runtimePath."
        }
        if (-not (Test-BytesEqual $entries[$runtimePath][0..3] ([byte[]](0xcf, 0xfa, 0xed, 0xfe)))) {
            throw "Mac Catalyst symbol package contains a non-Mach-O runtime for $library."
        }

        $contentTypes = [Text.Encoding]::UTF8.GetString($entries['[Content_Types].xml'])
        if ($contentTypes -notmatch 'Extension="dwarf"') {
            throw "$($product.Prefix) Catalyst symbol package does not declare the DWARF content type."
        }
        if ($contentTypes -notmatch [regex]::Escape("PartName=`"/$runtimePath`"")) {
            throw "$($product.Prefix) Catalyst symbol package does not declare its extensionless runtime."
        }

        foreach ($packageId in $expectedDwarfs.Keys) {
            $packagePath = Join-Path $full.Symbols "$packageId.$($product.Version).symbols.nupkg"
            $packageEntries = Get-ZipEntries $packagePath
            foreach ($dwarf in $expectedDwarfs[$packageId]) {
                if (-not $packageEntries.ContainsKey($dwarf)) {
                    throw "$packageId is missing expected DWARF '$dwarf'."
                }
            }
            $actualDwarfs = @($packageEntries.Keys | Where-Object { $_ -like '*.dwarf' })
            if ($actualDwarfs.Count -ne $expectedDwarfs[$packageId].Count) {
                throw "$packageId contains $($actualDwarfs.Count) DWARFs instead of $($expectedDwarfs[$packageId].Count)."
            }
        }
    }

    $missing = New-Fixture (Join-Path $testRoot 'missing')
    Remove-Item (Join-Path $missing.Native 'osx/libSkiaSharp/arm64.xcarchive/dSYMs/libSkiaSharp.dylib.dSYM/Contents/Resources/DWARF/libSkiaSharp.dylib')
    if ((Invoke-Pack $missing $true) -eq 0) {
        throw 'A full build with a missing arm64 DWARF unexpectedly succeeded.'
    }

    $partial = New-Fixture (Join-Path $testRoot 'partial') @('macOS')
    if ((Invoke-Pack $partial $false) -ne 0) {
        throw 'A partial macOS fixture failed instead of skipping unavailable packages.'
    }
    if (@(Get-ChildItem $partial.Symbols -Filter '*.symbols.nupkg').Count -ne 2) {
        throw 'The partial macOS fixture did not create exactly its two available symbol packages.'
    }

    Write-Host 'Apple symbol package tests passed.'
}
finally {
    if (Test-Path $testRoot) {
        Remove-Item $testRoot -Recurse -Force
    }
}
