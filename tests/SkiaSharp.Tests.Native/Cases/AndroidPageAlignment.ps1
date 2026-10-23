<#
Verifies the ELF class and machine match each Android ABI, and every LOAD segment supports 16 KB memory pages.
Alignment must be a power of two of at least 16 KB, with compatible file offsets and virtual addresses.
All supported ABIs are inspected without loading the libraries, catching wrong-ABI and larger-page loading failures.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

$abis = @{
    x86 = @('ELF32', 'Intel 80386')
    x86_64 = @('ELF64', 'Advanced Micro Devices X86-64')
    'armeabi-v7a' = @('ELF32', 'ARM')
    'arm64-v8a' = @('ELF64', 'AArch64')
}

foreach ($library in Get-AndroidArtifacts $NativeRoot) {
    Assert-Artifact $library

    # A correctly aligned library still cannot load if it was copied into the wrong ABI directory.
    $abi = Split-Path (Split-Path $library -Parent) -Leaf
    if (!$abis.ContainsKey($abi)) {
        throw "Unsupported Android ABI: $abi"
    }
    $class, $machine = $abis[$abi]
    $header = (& llvm-readobj --elf-output-style=GNU --file-headers $library) -join "`n"
    if ($header -notmatch "(?m)^\s*Class:\s+$class\s*$" -or
        $header -notmatch "(?m)^\s*Machine:\s+$([regex]::Escape($machine))\s*$" -or
        $header -notmatch '(?m)^\s*Type:\s+DYN\b' -or
        $header -notmatch '(?m)^\s*Data:\s+2''s complement, little endian\s*$') {
        throw "Expected little-endian $class shared library for Android $abi ($machine): $library"
    }

    $headers = & llvm-readobj --elf-output-style=GNU --program-headers $library
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
