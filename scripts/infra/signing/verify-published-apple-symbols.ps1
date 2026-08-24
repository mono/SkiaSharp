[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [string] $DotNetSymbolPath,

    [string] $OutputDirectory = '',

    [int] $MaxRetry = 10,

    [int] $RetryDelaySeconds = 30
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-MachOUuids {
    param(
        [Parameter(Mandatory)]
        [System.IO.FileInfo[]] $Files
    )

    $uuids = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($file in $Files) {
        $output = & dwarfdump --uuid $file.FullName
        if ($LASTEXITCODE -ne 0) {
            throw "dwarfdump failed for '$($file.FullName)'."
        }
        foreach ($line in $output) {
            $match = [regex]::Match($line, 'UUID:\s*(?<uuid>[0-9a-f-]+)', 'IgnoreCase')
            if ($match.Success) {
                [void]$uuids.Add($match.Groups['uuid'].Value)
            }
        }
    }
    if ($uuids.Count -eq 0) {
        throw "No Mach-O UUIDs were found in: $($Files.FullName -join ', ')"
    }
    return ,$uuids
}

function Get-PackageModule {
    param(
        [Parameter(Mandatory)]
        [string] $ExtractDirectory,

        [Parameter(Mandatory)]
        [string] $RelativePath,

        [Parameter(Mandatory)]
        [string] $Library
    )

    $path = Join-Path $ExtractDirectory $RelativePath
    if ($path.EndsWith('.zip', [StringComparison]::OrdinalIgnoreCase)) {
        $frameworkDirectory = "$path.extracted"
        [IO.Compression.ZipFile]::ExtractToDirectory($path, $frameworkDirectory)
        $modules = @(
            Get-ChildItem $frameworkDirectory -Recurse -File |
                Where-Object {
                    $_.Name -ceq $Library -and
                    $_.FullName.Replace('\', '/').EndsWith(
                        "/Versions/A/$Library",
                        [StringComparison]::Ordinal)
                })
        if ($modules.Count -ne 1) {
            throw "Expected one versioned '$Library' runtime in '$RelativePath', found $($modules.Count)."
        }
        return $modules[0]
    }

    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Apple runtime does not exist in its package: $RelativePath"
    }
    return Get-Item $path
}

$packagePath = (Resolve-Path $PackageDirectory).Path
$symbolTool = (Resolve-Path $DotNetSymbolPath).Path
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-apple-symbol-verification-$([Guid]::NewGuid().ToString('N'))"
} else {
    $OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
}
New-Item $OutputDirectory -ItemType Directory -Force | Out-Null

$specs = @(
    [pscustomobject]@{
        PackageId = 'SkiaSharp.NativeAssets.macOS'
        Library = 'libSkiaSharp.dylib'
        Modules = @('runtimes/osx/native/libSkiaSharp.dylib')
    }
    [pscustomobject]@{
        PackageId = 'HarfBuzzSharp.NativeAssets.macOS'
        Library = 'libHarfBuzzSharp.dylib'
        Modules = @('runtimes/osx/native/libHarfBuzzSharp.dylib')
    }
    [pscustomobject]@{
        PackageId = 'SkiaSharp.NativeAssets.iOS'
        Library = 'libSkiaSharp'
        Modules = @(
            'runtimes/ios/native/libSkiaSharp.framework/libSkiaSharp'
            'runtimes/iossimulator/native/libSkiaSharp.framework/libSkiaSharp')
    }
    [pscustomobject]@{
        PackageId = 'HarfBuzzSharp.NativeAssets.iOS'
        Library = 'libHarfBuzzSharp'
        Modules = @(
            'runtimes/ios/native/libHarfBuzzSharp.framework/libHarfBuzzSharp'
            'runtimes/iossimulator/native/libHarfBuzzSharp.framework/libHarfBuzzSharp')
    }
    [pscustomobject]@{
        PackageId = 'SkiaSharp.NativeAssets.MacCatalyst'
        Library = 'libSkiaSharp'
        Modules = @('runtimes/maccatalyst/native/libSkiaSharp.framework.zip')
    }
    [pscustomobject]@{
        PackageId = 'HarfBuzzSharp.NativeAssets.MacCatalyst'
        Library = 'libHarfBuzzSharp'
        Modules = @('runtimes/maccatalyst/native/libHarfBuzzSharp.framework.zip')
    }
    [pscustomobject]@{
        PackageId = 'SkiaSharp.NativeAssets.tvOS'
        Library = 'libSkiaSharp'
        Modules = @(
            'runtimes/tvos/native/libSkiaSharp.framework/libSkiaSharp'
            'runtimes/tvossimulator/native/libSkiaSharp.framework/libSkiaSharp')
    }
    [pscustomobject]@{
        PackageId = 'HarfBuzzSharp.NativeAssets.tvOS'
        Library = 'libHarfBuzzSharp'
        Modules = @(
            'runtimes/tvos/native/libHarfBuzzSharp.framework/libHarfBuzzSharp'
            'runtimes/tvossimulator/native/libHarfBuzzSharp.framework/libHarfBuzzSharp')
    }
)
$servers = @(
    [pscustomobject]@{ Name = 'MSDL'; Argument = '--microsoft-symbol-server' }
    [pscustomobject]@{ Name = 'SymWeb'; Argument = '--internal-server' }
)
$verified = 0

foreach ($spec in $specs) {
    $packages = @(
        Get-ChildItem $packagePath -Filter "$($spec.PackageId).*.nupkg" -File |
            Where-Object {
                -not $_.Name.EndsWith('.symbols.nupkg', [StringComparison]::OrdinalIgnoreCase)
            })
    if ($packages.Count -ne 1) {
        throw "Expected exactly one '$($spec.PackageId)' package, found $($packages.Count)."
    }

    $extractDirectory = Join-Path $OutputDirectory "packages/$($spec.PackageId)"
    [IO.Compression.ZipFile]::ExtractToDirectory($packages[0].FullName, $extractDirectory)
    foreach ($relativeModule in $spec.Modules) {
        $module = Get-PackageModule $extractDirectory $relativeModule $spec.Library
        $expectedUuids = Get-MachOUuids @($module)

        foreach ($server in $servers) {
            $resultDirectory = Join-Path $OutputDirectory "results/$($server.Name)/$($spec.PackageId)/$([IO.Path]::GetFileName($relativeModule))"
            $matched = $false
            for ($attempt = 1; $attempt -le $MaxRetry -and -not $matched; $attempt++) {
                Remove-Item $resultDirectory -Recurse -Force -ErrorAction SilentlyContinue
                New-Item $resultDirectory -ItemType Directory -Force | Out-Null

                & $symbolTool `
                    --symbols `
                    --modules `
                    $server.Argument `
                    $module.FullName `
                    -o $resultDirectory `
                    --diagnostics | Write-Host

                $downloadedModules = @(
                    Get-ChildItem $resultDirectory -Recurse -File |
                        Where-Object Name -ceq $module.Name)
                $downloadedDwarfs = @(
                    Get-ChildItem $resultDirectory -Recurse -File |
                        Where-Object Extension -ceq '.dwarf')
                if ($downloadedModules.Count -ne 0 -and $downloadedDwarfs.Count -ne 0) {
                    $actualUuids = Get-MachOUuids $downloadedDwarfs
                    $matched = $expectedUuids.SetEquals($actualUuids)
                }

                if (-not $matched -and $attempt -lt $MaxRetry) {
                    Start-Sleep -Seconds $RetryDelaySeconds
                }
            }

            if (-not $matched) {
                throw "$($server.Name) did not return matching module and DWARF symbols for '$relativeModule'."
            }
            Write-Host "$($server.Name): verified module and DWARF symbols for $relativeModule"
            $verified++
        }
    }
}

Write-Host "Verified $verified Apple module/server symbol combinations."
