<#
Guards the complete merged native collection against gaps in test coverage. Every required artifact must exist and
have an inspection case; aliases must match their inspected originals. Files are identified by suffix and binary
signature, so a new platform, architecture, library or bundled binary fails until its coverage is declared explicitly.
#>

param([Parameter(Mandatory)][string] $NativeRoot)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module "$PSScriptRoot/Helpers.psm1"
Assert-NativeRoot $NativeRoot

# This is the complete final collection, not just the platforms understood by the original regression cases.
$inventory = @(Get-NativeArtifactInventory $NativeRoot)
$claimed = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$errors = [Collections.Generic.List[string]]::new()
foreach ($artifact in $inventory) {
    if (!$claimed.Add($artifact.Path)) {
        $errors.Add("Duplicate inventory path: $($artifact.Path)")
    }
    $scripts = @("$PSScriptRoot/$($artifact.Case).ps1", "$PSScriptRoot/$($artifact.Case)/test.ps1" |
            Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
    if ($scripts.Count -ne 1) {
        $errors.Add("Expected exactly one inspection case '$($artifact.Case)': $($artifact.Path)")
    }

    try {
        Assert-Artifact $artifact.Path
        if ($artifact.AliasOf) {
            Assert-IdenticalArtifact $artifact.Path (Join-Path $NativeRoot $artifact.AliasOf)
        }
    } catch {
        $errors.Add($_.Exception.Message)
    }
}

# Signature detection also catches extensionless framework/DWARF files and binaries hidden under a new suffix.
# The pinned emdawnwebgpu package currently contains source only; any future bundled binary must be explicitly claimed.
$collections = @($NativeRoot, [IO.Path]::GetFullPath((Join-Path $NativeRoot '../msvc/native')))
foreach ($collection in $collections) {
    Assert-NativeRoot $collection
    foreach ($path in Get-NativeArtifactFiles $collection) {
        if (!$claimed.Contains($path)) {
            $errors.Add("Unclaimed native artifact (add an inventory entry and an inspection): $path")
        }
    }
}

if ($errors.Count -gt 0) {
    throw "Native artifact coverage failed:`n$($errors -join "`n")"
}
Write-Host "PASS: all $($inventory.Count) required artifacts are present and every detected native artifact has an inspection."
