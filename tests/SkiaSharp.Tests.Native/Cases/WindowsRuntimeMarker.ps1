<#
Checks native Windows PDBs for the existing objr_spectre or objd_spectre runtime build marker. The managed WinUI
projection is excluded. This verifies marker presence only: it does not prove compiler mitigation, CFG configuration
or complete Spectre protection. Files are scanned in bounded chunks, including markers crossing chunk boundaries.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot

# The managed projection is excluded from the native runtime-marker policy.
# Marker presence identifies a runtime build; it does not prove compiler mitigation.
foreach ($artifact in Get-WindowsArtifacts $NativeRoot | Where-Object RuntimeMarker) {
    $pdb = [IO.Path]::ChangeExtension($artifact.Path, '.pdb')
    Assert-Artifact $pdb
    $found = $false

    # Scan bounded chunks and retain overlap for markers crossing a chunk boundary.
    $reader = [IO.StreamReader]::new($pdb, [Text.Encoding]::ASCII)
    try {
        $buffer = [char[]]::new(65536)
        $tail = ''
        while (($count = $reader.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $text = $tail + [string]::new($buffer, 0, $count)
            if ($text -match 'objr_spectre|objd_spectre') {
                $found = $true
                break
            }
            $tail = $text.Substring([Math]::Max(0, $text.Length - 11))
        }
    } finally {
        $reader.Dispose()
    }

    if (!$found) {
        throw "Missing Spectre runtime build marker (objr_spectre/objd_spectre): $pdb"
    }
    Write-Host "PASS: $pdb contains the Spectre runtime build marker."
}
