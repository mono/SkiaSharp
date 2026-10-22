<#
Validates Windows symbol and metadata artifacts. Native MSF and managed portable PDB names and identities must match
their DLL's CodeView data, and WinUI projection/WinMD files must contain readable CLI metadata. This catches
mismatched or invalid debug artifacts without loading the target assemblies or requiring a Windows runtime.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"
Assert-NativeRoot $NativeRoot
Assert-Tool llvm-pdbutil

# Projection symbols may be portable PDBs; native PDBs use MSF. Both must match the DLL's CodeView identity.
foreach ($artifact in Get-NativeArtifactInventory $NativeRoot | Where-Object Case -eq WindowsSymbols) {
    Assert-Artifact $artifact.Path
    if ($artifact.Kind -eq 'WinMD') {
        Assert-CliMetadata $artifact.Path
    } else {
        $binary = Join-Path $NativeRoot $artifact.DebugOf
        Assert-Artifact $binary
        $identity = Get-PeCodeViewIdentity $binary
        $pdbName = Split-Path $artifact.Path -Leaf
        if ([IO.Path]::GetFileName($identity.Path) -cne $pdbName) {
            throw "Embedded PDB name does not match $pdbName`: $($identity.Path)"
        }
        $stream = [IO.File]::OpenRead($artifact.Path)
        try {
            $portable = [Convert]::ToHexString((Get-StreamPrefix $stream)).StartsWith('42534A42')
            if ($portable) {
                if ($artifact.RelativePath -cne 'winui/any/SkiaSharp.Views.WinUI.Native.Projection.pdb') {
                    throw "Expected native MSF symbols, not a portable PDB: $($artifact.Path)"
                }

                $stream.Position = 0
                $provider = [Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($stream)
                try {
                    $metadata = $provider.GetMetadataReader()
                    $id = $metadata.DebugMetadataHeader.Id
                    $guid = [Guid]::new([byte[]]$id[0..15])
                    $age = 1
                } finally {
                    $provider.Dispose()
                }
            }
        } finally {
            $stream.Dispose()
        }

        if (!$portable) {
            $summary = (& llvm-pdbutil dump -summary $artifact.Path) -join "`n"
            $guidMatch = [regex]::Match($summary, '(?m)^\s*GUID:\s+\{([0-9A-Fa-f-]+)\}\s*$')
            $ageMatch = [regex]::Match($summary, '(?m)^\s*Age:\s+(\d+)\s*$')
            if (!$guidMatch.Success -or !$ageMatch.Success) {
                throw "Invalid native PDB summary: $($artifact.Path)"
            }
            $guid = [Guid]::Parse($guidMatch.Groups[1].Value)
            $age = [int]$ageMatch.Groups[1].Value
        }

        if ($guid -ne $identity.Guid -or $age -ne $identity.Age) {
            throw "PDB identity does not match $binary`: $($artifact.Path)"
        }
        if ($artifact.RelativePath -like 'winui/any/*') {
            Assert-CliMetadata $binary
        }
    }

    Write-Host "PASS: $($artifact.Path) has valid metadata or matching debug identity."
}
