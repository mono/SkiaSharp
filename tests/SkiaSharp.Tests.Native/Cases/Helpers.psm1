#requires -Version 7.4

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$env:LC_ALL = 'C'

function Assert-NativeRoot([string] $NativeRoot) {
    if (![IO.Path]::IsPathFullyQualified($NativeRoot) -or !(Test-Path -LiteralPath $NativeRoot -PathType Container)) {
        throw "Expected an existing absolute native artifact directory: $NativeRoot"
    }
}

function Assert-Artifact([string] $Path) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf) -or (Get-Item -LiteralPath $Path).Length -eq 0) {
        throw "Missing or empty artifact: $Path"
    }
}

function Assert-Tool([string] $Name) {
    $null = Get-Command $Name -CommandType Application
}

# Centralize the artifact layouts, not the test assertions.
function Get-AndroidArtifacts([string] $NativeRoot) {
    foreach ($abi in 'x86', 'x86_64', 'armeabi-v7a', 'arm64-v8a') {
        foreach ($name in 'libSkiaSharp.so', 'libHarfBuzzSharp.so') {
            Join-Path $NativeRoot "android/$abi/$name"
        }
    }
}

function Get-LinuxArtifacts([string] $NativeRoot) {
    foreach ($variant in 'linux', 'linuxnodeps', 'alpine', 'alpinenodeps', 'bionic') {
        $arches = switch -Wildcard ($variant) {
            'linux*' {
                'x86', 'x64', 'arm', 'arm64', 'riscv64', 'loongarch64'
            }
            'alpine*' {
                'x64', 'arm', 'arm64', 'riscv64', 'loongarch64'
            }
            'bionic' {
                'x64', 'arm64'
            }
        }

        foreach ($arch in $arches) {
            foreach ($name in 'libSkiaSharp', 'libHarfBuzzSharp') {
                [pscustomobject]@{
                    Variant = $variant
                    Arch = $arch
                    Name = $name
                    Path = Join-Path $NativeRoot "$variant/$arch/$name.so"
                }
            }
        }
    }
}

function Get-WindowsArtifacts([string] $NativeRoot) {
    $desktop = @(
        'windows/x86/libSkiaSharp.dll', 'windows/x86/libHarfBuzzSharp.dll'
        'windows/x64/libSkiaSharp.dll', 'windows/x64/libHarfBuzzSharp.dll'
        'windows/ARM64/libSkiaSharp.dll', 'windows/arm64/libHarfBuzzSharp.dll'
    )

    # MSVC desktop builds are published separately from the Clang, NanoServer and WinUI collection.
    $msvcRoot = [IO.Path]::GetFullPath((Join-Path $NativeRoot '../msvc/native'))
    foreach ($root in @($NativeRoot, $msvcRoot)) {
        $paths = @($desktop)
        if ($root -ceq $NativeRoot) {
            $paths += @(
                'nanoserver/x64/libSkiaSharp.dll', 'nanoserver/x64/libHarfBuzzSharp.dll'
                'winui/any/SkiaSharp.Views.WinUI.Native.Projection.dll'
            )
            foreach ($arch in 'x86', 'x64', 'arm64') {
                foreach ($name in 'SkiaSharp.Views.WinUI.Native', 'libEGL', 'libGLESv2') {
                    $paths += "winui/$arch/$name.dll"
                }
            }
        }

        foreach ($path in $paths) {
            # Windows merges ARM64 and arm64 directories; preserve the spelling retained by artifact extraction.
            $windows = Join-Path $root 'windows'
            if ($path -cmatch '^windows/(?:ARM64|arm64)/(.+)$' -and (Test-Path -LiteralPath $windows)) {
                $name = $Matches[1]
                $directories = @(Get-ChildItem -LiteralPath $windows -Directory |
                        Where-Object { $_.Name -cin @('ARM64', 'arm64') -and
                            (Test-Path -LiteralPath (Join-Path $_.FullName $name) -PathType Leaf) })
                if ($directories.Count -eq 1) {
                    $path = "windows/$($directories[0].Name)/$name"
                }
            }

            [pscustomobject]@{
                Path = [IO.Path]::GetFullPath((Join-Path $root $path))
                RuntimeMarker = $path -notlike 'winui/any/*'
                Direct3D = $path -like 'windows/*/libSkiaSharp.dll'
            }
        }
    }
}

# These tools inspect target files without loading them into the test host.
function Assert-GnuReadelf {
    Assert-Tool readelf
    if (((& readelf --version) -join "`n") -notmatch 'GNU readelf') {
        throw 'GNU readelf is required.'
    }
}

function Get-ElfBuildId([string] $Path) {
    $notes = (& readelf --notes $Path) -join "`n"
    $match = [regex]::Match($notes, '(?i)Build ID:\s*([0-9a-f]+)')
    if (!$match.Success) {
        throw "Missing GNU build ID: $Path"
    }

    return $match.Groups[1].Value
}

function Get-PeImports([string] $Path) {
    $output = (& llvm-readobj --coff-imports $Path) -join "`n"
    if ($output -notmatch '(?m)^Format: COFF-') {
        throw "Expected a Windows PE artifact: $Path"
    }

    $imports = @{ Hard = @(); Delayed = @() }
    foreach ($block in [regex]::Matches($output, '(?ms)^(Import|DelayImport) \{\r?\n(.*?)^\}')) {
        $name = [regex]::Match($block.Groups[2].Value, '(?m)^\s*Name:\s*([^\r\n]+)')
        if (!$name.Success) {
            throw "Import section has no DLL name: $Path"
        }

        $kind = if ($block.Groups[1].Value -eq 'Import') {
            'Hard'
        } else {
            'Delayed'
        }
        $imports[$kind] += $name.Groups[1].Value
    }

    return [pscustomobject]$imports
}

# Complete collection layout. Aliases are covered by comparing them with the inspected original.
function New-NativeArtifact(
    [string] $NativeRoot, [string] $RelativePath, [string] $Case, [string] $Kind,
    [string] $Name = '', [string[]] $Arches = @(), [string] $AliasOf = '', [string] $DebugOf = ''
) {
    [pscustomobject]@{
        Path = [IO.Path]::GetFullPath((Join-Path $NativeRoot $RelativePath))
        RelativePath = $RelativePath
        Case = $Case
        Kind = $Kind
        Name = $Name
        Arches = $Arches
        AliasOf = $AliasOf
        DebugOf = $DebugOf
    }
}

function Get-AppleArtifacts([string] $NativeRoot) {
    foreach ($platform in 'osx', 'ios', 'iossimulator', 'tvos', 'tvossimulator', 'maccatalyst') {
        foreach ($name in 'libSkiaSharp', 'libHarfBuzzSharp') {
            if ($platform -eq 'osx') {
                New-NativeArtifact $NativeRoot "$platform/$name.dylib" AppleArtifacts MachO $name @('x86_64', 'arm64')
            } else {
                $framework = "$platform/$name.framework"
                $binary = if ($platform -eq 'maccatalyst') {
                    "$framework/Versions/A/$name"
                } else {
                    "$framework/$name"
                }
                New-NativeArtifact $NativeRoot $binary AppleArtifacts MachO $name @('x86_64', 'arm64')
                if ($platform -eq 'maccatalyst') {
                    $current = "$framework/Versions/Current/$name"
                    New-NativeArtifact $NativeRoot "$framework/$name" ArtifactCoverage Alias -AliasOf $binary
                    New-NativeArtifact $NativeRoot $current ArtifactCoverage Alias -AliasOf $binary
                    New-NativeArtifact $NativeRoot "$framework.zip" AppleArtifacts FrameworkZip $name
                }
            }

            foreach ($arch in 'x86_64', 'arm64') {
                $archive = "$platform/$name/$arch.xcarchive"
                if ($platform -eq 'osx') {
                    $binary = "$archive/Products/@rpath/$name.dylib"
                    $symbols = "$archive/dSYMs/$name.dylib.dSYM/Contents/Resources/DWARF/$name.dylib"
                } else {
                    $framework = "$archive/Products/Library/Frameworks/$name.framework"
                    $binary = if ($platform -eq 'maccatalyst') {
                        "$framework/Versions/A/$name"
                    } else {
                        "$framework/$name"
                    }
                    $symbols = "$archive/dSYMs/$name.framework.dSYM/Contents/Resources/DWARF/$name"
                    if ($platform -eq 'maccatalyst') {
                        $current = "$framework/Versions/Current/$name"
                        New-NativeArtifact $NativeRoot "$framework/$name" ArtifactCoverage Alias -AliasOf $binary
                        New-NativeArtifact $NativeRoot $current ArtifactCoverage Alias -AliasOf $binary
                    }
                }

                New-NativeArtifact $NativeRoot $binary AppleArtifacts MachO $name @($arch)
                New-NativeArtifact $NativeRoot $symbols AppleArtifacts MachODebug $name @($arch) -DebugOf $binary
            }
        }
    }
}

function Get-TizenArtifacts([string] $NativeRoot) {
    foreach ($arch in 'x86_64', 'aarch64') {
        foreach ($name in 'libSkiaSharp', 'libHarfBuzzSharp') {
            New-NativeArtifact $NativeRoot "tizen/$arch/$name.so" TizenArtifacts Elf $name @($arch)
        }
    }
}

function Get-WasmArtifacts([string] $NativeRoot) {
    foreach ($version in '3.1.34', '3.1.56', '6.0.2') {
        foreach ($features in 'st', 'mt', 'st,simd', 'mt,simd') {
            foreach ($name in 'libSkiaSharp', 'libHarfBuzzSharp') {
                $directory = "wasm/$name.a/$version/$features"
                $binary = "$directory/$name.a"
                New-NativeArtifact $NativeRoot $binary WasmArtifacts WasmArchive $name @('wasm32')
                if ($name -eq 'libHarfBuzzSharp') {
                    # The recipe retains whichever spelling its emsdk produced, beside the normalized archive.
                    $aliases = @("$directory/$name.a.wasm", "$directory/$name.wasm.a")
                    $present = @($aliases | Where-Object { Test-Path -LiteralPath (Join-Path $NativeRoot $_) -PathType Leaf })
                    if ($present.Count -eq 0) {
                        $present = @($aliases[0])
                    }
                    foreach ($alias in $present) {
                        New-NativeArtifact $NativeRoot $alias ArtifactCoverage Alias -AliasOf $binary
                    }
                }
            }
        }
    }
}

function Get-NativeArtifactInventory([string] $NativeRoot) {
    foreach ($path in Get-AndroidArtifacts $NativeRoot) {
        $relative = [IO.Path]::GetRelativePath($NativeRoot, $path).Replace('\', '/')
        New-NativeArtifact $NativeRoot $relative AndroidPageAlignment Elf
        New-NativeArtifact $NativeRoot "$relative.dbg" AndroidDebugSymbols ElfDebug
    }

    # Keep these exact aliases aligned with scripts/VERSIONS.txt; a changed SONAME needs review.
    $sonames = @{ libSkiaSharp = '156.0.0'; libHarfBuzzSharp = '0.61440.0' }
    foreach ($artifact in Get-LinuxArtifacts $NativeRoot) {
        $relative = [IO.Path]::GetRelativePath($NativeRoot, $artifact.Path).Replace('\', '/')
        New-NativeArtifact $NativeRoot $relative LinuxDependencies Elf
        New-NativeArtifact $NativeRoot "$relative.$($sonames[$artifact.Name])" ArtifactCoverage Alias -AliasOf $relative
    }

    foreach ($artifact in Get-WindowsArtifacts $NativeRoot) {
        $relative = [IO.Path]::GetRelativePath($NativeRoot, $artifact.Path).Replace('\', '/')
        New-NativeArtifact $NativeRoot $relative WindowsDependencies PE
        $pdb = [IO.Path]::ChangeExtension($relative, '.pdb')
        New-NativeArtifact $NativeRoot $pdb WindowsSymbols Pdb -DebugOf $relative
    }

    foreach ($arch in 'x86', 'x64', 'arm64') {
        New-NativeArtifact $NativeRoot "winui/$arch/SkiaSharp.Views.WinUI.Native.winmd" WindowsSymbols WinMD
    }

    Get-AppleArtifacts $NativeRoot
    Get-TizenArtifacts $NativeRoot
    Get-WasmArtifacts $NativeRoot
}

# Detect binaries by signature as well as suffix, so an unrecognized library or target cannot disappear from coverage.
function Get-StreamPrefix([IO.Stream] $Stream) {
    $bytes = [byte[]]::new(32)
    $count = $Stream.Read($bytes, 0, $bytes.Length)
    if ($count -eq 0) {
        return , [byte[]]::new(0)
    }
    return , $bytes[0..($count - 1)]
}

function Test-NativeArtifact([string] $Name, [byte[]] $Prefix) {
    if ($Name -match '\.(so(?:\..*)?|dll|pdb|dbg|dylib|a|a\.wasm|wasm(?:\.a)?|o|obj|lib|exe|winmd|bc|zip)$') {
        return $true
    }

    $hex = [Convert]::ToHexString($Prefix)
    return $hex -match '^(7F454C46|4D5A|CEFAEDFE|CFFAEDFE|FEEDFACE|FEEDFACF|CAFEBABE|BEBAFECA|CAFEBABF|BFBAFECA|' +
    '213C617263683E0A|213C7468696E3E0A|0061736D|4243C0DE|DEC0170B|42534A42|504B0304|4D6963726F736F667420432F432B2B)'
}

function Get-NativeArtifactFiles([string] $NativeRoot) {
    foreach ($file in Get-ChildItem -LiteralPath $NativeRoot -Recurse -Force) {
        if ($file.PSIsContainer) {
            if (!$file.LinkTarget) {
                continue
            }

            # Only these known framework directory aliases may avoid a second traversal.
            $relative = [IO.Path]::GetRelativePath($NativeRoot, $file.FullName).Replace('\', '/')
            $framework = '^maccatalyst/(?<library>libSkiaSharp|libHarfBuzzSharp)' +
            '(?:/(?:x86_64|arm64)\.xcarchive/Products/Library/Frameworks/\k<library>)?\.framework'
            $match = [regex]::Match($relative, "^($framework)/(?<link>Versions/Current|Headers|Resources|Modules)$")
            if (!$match.Success) {
                throw "Unclaimed directory symlink in native collection: $($file.FullName)"
            }
            $suffix = if ($match.Groups['link'].Value -eq 'Versions/Current') {
                ''
            } else {
                "/$($match.Groups['link'].Value)"
            }
            $expected = Join-Path $NativeRoot "$($match.Groups[1].Value)/Versions/A$suffix"
            $target = $file.ResolveLinkTarget($true)
            if (!$target -or $target.FullName -cne $expected) {
                throw "Framework directory alias does not resolve to $expected`: $($file.FullName)"
            }

            continue
        }

        $stream = [IO.File]::OpenRead($file.FullName)
        try {
            if (Test-NativeArtifact $file.Name (Get-StreamPrefix $stream)) {
                $file.FullName
            }
        } finally {
            $stream.Dispose()
        }
    }
}

function Assert-IdenticalArtifact([string] $Path, [string] $Original) {
    Assert-Artifact $Path
    Assert-Artifact $Original
    if ((Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -cne (Get-FileHash -LiteralPath $Original -Algorithm SHA256).Hash) {
        throw "Artifact alias differs from its inspected original: $Path (original: $Original)"
    }
}

# Read PE/CLI metadata in the host without loading or executing the target assembly.
function Get-PeCodeViewIdentity([string] $Path) {
    $stream = [IO.File]::OpenRead($Path)
    $reader = [Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $entries = @($reader.ReadDebugDirectory() | Where-Object Type -eq CodeView)
        if ($entries.Count -ne 1) {
            throw "Expected one CodeView identity: $Path"
        }
        $identity = $reader.ReadCodeViewDebugDirectoryData($entries[0])
        return [pscustomobject]@{ Guid = $identity.Guid; Age = $identity.Age }
    } finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

function Assert-CliMetadata([string] $Path) {
    $stream = [IO.File]::OpenRead($Path)
    $reader = [Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        if (!$reader.HasMetadata -or !$reader.PEHeaders.CorHeader) {
            throw "Expected PE/CLI metadata: $Path"
        }
        $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($reader)
        if ($metadata.TypeDefinitions.Count -eq 0) {
            throw "Empty CLI metadata: $Path"
        }
    } finally {
        $reader.Dispose()
        $stream.Dispose()
    }
}

Export-ModuleMember -Function @(
    'Assert-NativeRoot', 'Assert-Artifact', 'Assert-Tool', 'Assert-GnuReadelf', 'Assert-IdenticalArtifact', 'Assert-CliMetadata'
    'Get-AndroidArtifacts', 'Get-LinuxArtifacts', 'Get-WindowsArtifacts', 'Get-ElfBuildId', 'Get-PeImports'
    'Get-AppleArtifacts', 'Get-TizenArtifacts', 'Get-WasmArtifacts', 'Get-NativeArtifactInventory', 'Get-NativeArtifactFiles'
    'Get-StreamPrefix', 'Test-NativeArtifact', 'Get-PeCodeViewIdentity'
)
