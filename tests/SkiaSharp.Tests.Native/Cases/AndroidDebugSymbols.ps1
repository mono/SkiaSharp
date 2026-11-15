<#
Checks both Android libraries for every supported ABI. The shipped module and its split debug file must have the same
nonempty GNU build ID, and .gnu_debuglink must name the matching debug file exactly. These checks keep crash-symbol
resolution working after stripping; they inspect the files without executing Android code.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"

Assert-NativeRoot $NativeRoot
Assert-Tool llvm-readobj

foreach ($library in Get-AndroidArtifacts $NativeRoot) {
    $symbols = "$library.dbg"
    Assert-Artifact $library
    Assert-Artifact $symbols

    # Stripping must preserve the identity of the shipped module and its split symbols.
    $moduleId = Get-ElfBuildId $library
    $symbolId = Get-ElfBuildId $symbols
    if ($moduleId -ine $symbolId) {
        throw "GNU build IDs differ: $library ($moduleId) and $symbols ($symbolId)"
    }

    # The filename is case-sensitive even though build ID hex digits are not.
    $expected = [IO.Path]::GetFileName($symbols)
    $debugLink = (& llvm-readobj --elf-output-style=GNU --string-dump=.gnu_debuglink $library) -join "`n"
    $match = [regex]::Match($debugLink, '(?m)^\s*\[\s*0\]\s+([^\r\n]+)')
    if (!$match.Success -or $match.Groups[1].Value -cne $expected) {
        throw "Expected .gnu_debuglink '$expected': $library"
    }

    Write-Host "PASS: $library links $expected (GNU build ID $moduleId)"
}
