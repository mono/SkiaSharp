<#
Verifies that every LOAD segment in both Android libraries supports 16 KB memory pages. Segment alignment must be a
power of two of at least 16 KB, with compatible file offsets and virtual addresses. All supported ABIs are inspected
without loading the libraries, catching artifacts that would fail on devices using larger pages.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Readelf

foreach ($library in Get-AndroidArtifacts $NativeRoot) {
    Assert-Artifact $library
    $headers = & llvm-readelf --program-headers --wide $library
    $loads = @($headers | Where-Object { $_ -match '^\s*LOAD\s' })
    if ($loads.Count -eq 0) {
        throw "No LOAD program headers found: $library"
    }

    # Every loadable segment must support 16 KB pages; larger power-of-two alignments also work.
    foreach ($line in $loads) {
        $fields = $line.Trim() -split '\s+'
        if ($fields.Count -lt 8) {
            throw "Malformed LOAD header: $library"
        }

        $values = @($fields[1], $fields[2], $fields[-1])
        if (@($values | Where-Object { $_ -notmatch '^0x[0-9a-fA-F]+$' }).Count -ne 0) {
            throw "Malformed LOAD values in ${library}: $line"
        }

        $offset, $address, $alignment = $values | ForEach-Object { [Convert]::ToUInt64($_.Substring(2), 16) }
        if ($alignment -lt 0x4000 -or ($alignment -band ($alignment - 1)) -ne 0) {
            throw "LOAD alignment $($fields[-1]) is not 16 KB compatible: $library"
        }
        if ($offset % $alignment -ne $address % $alignment) {
            throw "LOAD offset and address are not congruent: $library"
        }
    }

    Write-Host "PASS: $library ($($loads.Count) LOAD headers, 16 KB compatible)"
}
