<#
Inspects macOS, iOS, tvOS and Mac Catalyst libraries and their archived build products. Checks the expected Mach-O
architecture slices and public entry points, matches dSYM UUIDs to their binaries, and verifies packaged frameworks
contain the inspected library. This is artifact validation on Linux, not execution on an Apple runtime.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"
Assert-NativeRoot $NativeRoot
foreach ($tool in 'llvm-readobj', 'llvm-nm', 'llvm-objdump') {
    Assert-Tool $tool
}

# Check every Mach-O slice and the public entry points consumers need; dSYMs must identify the corresponding slice.
foreach ($artifact in Get-AppleArtifacts $NativeRoot | Where-Object Case -eq AppleArtifacts) {
    Assert-Artifact $artifact.Path
    if ($artifact.Kind -eq 'FrameworkZip') {
        $zip = [IO.Compression.ZipFile]::OpenRead($artifact.Path)
        try {
            $expected = "$($artifact.Name).framework/Versions/A/$($artifact.Name)"
            $binaries = @()
            foreach ($entry in $zip.Entries) {
                if ($entry.Name -eq '') {
                    continue
                }
                $stream = $entry.Open()
                try {
                    if (Test-NativeArtifact $entry.FullName (Get-StreamPrefix $stream)) {
                        $binaries += $entry
                    }
                } finally {
                    $stream.Dispose()
                }
            }

            if ($binaries.Count -ne 1 -or $binaries[0].FullName -cne $expected) {
                throw "Expected only '$expected' as a native binary in $($artifact.Path); found: $($binaries.FullName -join ', ')"
            }

            $stream = $binaries[0].Open()
            try {
                $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
                $original = Join-Path ([IO.Path]::GetDirectoryName($artifact.Path)) $expected
                if ($hash -cne (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash) {
                    throw "Zipped framework binary differs from the inspected framework: $($artifact.Path)"
                }
            } finally {
                $stream.Dispose()
            }
        } finally {
            $zip.Dispose()
        }
    } else {
        $headers = (& llvm-readobj --file-headers --sections $artifact.Path) -join "`n"
        $formats = @([regex]::Matches($headers, '(?m)^Format:\s+(.+)'))
        $arches = @([regex]::Matches($headers, '(?m)^Arch:\s+(\S+)') | ForEach-Object {
                $_.Groups[1].Value -replace '^aarch64$', 'arm64'
            })
        if ($formats.Count -ne $artifact.Arches.Count -or
            @($formats | Where-Object { $_.Groups[1].Value -notmatch '^Mach-O' }).Count -gt 0 -or
            (($arches | Sort-Object) -join ',') -cne (($artifact.Arches | Sort-Object) -join ',')) {
            throw "Expected Mach-O slices [$($artifact.Arches -join ', ')]: $($artifact.Path); found [$($arches -join ', ')]"
        }

        if ($artifact.Kind -eq 'MachODebug') {
            if ($headers -notmatch '(?m)^\s*FileType:\s+DWARFSymbol\b' -or
                $headers -notmatch '(?m)^\s*Name:\s+__debug_info\b') {
                throw "Expected a Mach-O dSYM with DWARF debug information: $($artifact.Path)"
            }
            $commands = (& llvm-objdump --macho --private-headers $artifact.Path) -join "`n"
            $original = (& llvm-objdump --macho --private-headers (Join-Path $NativeRoot $artifact.DebugOf)) -join "`n"
            $uuid = [regex]::Match($commands, '(?m)^\s*uuid ([0-9A-Fa-f-]{36})\s*$')
            $originalUuid = [regex]::Match($original, '(?m)^\s*uuid ([0-9A-Fa-f-]{36})\s*$')
            if (!$uuid.Success -or !$originalUuid.Success -or
                $uuid.Groups[1].Value -ine $originalUuid.Groups[1].Value) {
                throw "dSYM UUID does not match $($artifact.DebugOf): $($artifact.Path)"
            }
        } else {
            $dynamicSlices = [regex]::Matches($headers, '(?m)^\s*FileType:\s+DynamicLibrary\b').Count
            if ($dynamicSlices -ne $artifact.Arches.Count) {
                throw "Expected a dynamic library in every Mach-O slice: $($artifact.Path)"
            }

            $exports = if ($artifact.Name -eq 'libSkiaSharp') {
                '_sk_version_get_milestone', '_sk_bitmap_new'
            } else {
                '_hb_version', '_hb_buffer_create'
            }
            foreach ($arch in $artifact.Arches) {
                $symbols = (& llvm-nm "--arch=$arch" --defined-only --extern-only --format=just-symbols $artifact.Path) -join "`n"
                foreach ($export in $exports) {
                    if ($symbols -cnotmatch "(?m)^$export`r?$") {
                        throw "Missing $arch public export '$export': $($artifact.Path)"
                    }
                }
            }
        }
    }

    Write-Host "PASS: $($artifact.Path) has the expected Mach-O ABI, matching symbols, or inspected framework contents."
}
