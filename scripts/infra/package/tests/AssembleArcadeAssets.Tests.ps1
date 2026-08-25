$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression

function New-Package {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [hashtable] $Entries
    )

    $stream = [IO.File]::Create($Path)
    $archive = [IO.Compression.ZipArchive]::new(
        $stream,
        [IO.Compression.ZipArchiveMode]::Create,
        $false)
    try {
        foreach ($path in $Entries.Keys) {
            $entry = $archive.CreateEntry($path)
            $writer = [IO.StreamWriter]::new($entry.Open())
            try {
                $writer.Write($Entries[$path])
            } finally {
                $writer.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

$root = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-arcade-assets-$([Guid]::NewGuid().ToString('N'))"
$signed = Join-Path $root 'signed'
$transport = Join-Path $root 'transport'
$packages = Join-Path $root 'packages'
$pdbs = Join-Path $root 'pdbs'
$prTransport = Join-Path $root 'pr-transport'
$prPackages = Join-Path $root 'pr-packages'
$prPdbs = Join-Path $root 'pr-pdbs'
$script = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../assemble-arcade-assets.ps1'))

try {
    New-Item $signed -ItemType Directory -Force | Out-Null
    New-Item $transport -ItemType Directory -Force | Out-Null

    New-Package (Join-Path $signed 'Foo.1.0.0.nupkg') @{
        'lib/net8.0/Foo.dll' = 'dll8'
        'lib/net8.0/Foo.pdb' = 'pdb8'
        'lib/net9.0/Foo.dll' = 'dll9'
        'lib/net9.0/Foo.pdb' = 'pdb9'
        'ref/net8.0/Foo.pdb' = 'reference'
        'runtimes/win-x64/native/Foo.pdb' = 'native'
    }
    New-Package (Join-Path $signed 'Bar.1.0.0.nupkg') @{
        'lib/net8.0/Bar.dll' = 'dll'
        'lib/net8.0/Bar.pdb' = 'normal-pdb'
    }
    New-Package (Join-Path $signed 'Bar.1.0.0.symbols.nupkg') @{
        'lib/net8.0/Bar.pdb' = 'explicit-pdb'
    }

    foreach ($name in @(
        '_NuGets.0.0.0-branch.main.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-branch.main.1.nupkg')) {
        New-Package (Join-Path $transport $name) @{ 'README.md' = $name }
    }

    & $script `
        -SignedPackageDirectory $signed `
        -TransportPackageDirectory $transport `
        -PackageRoot $packages `
        -PdbArtifactsDirectory $pdbs

    $expectedPdbs = @(
        'Foo.1.0.0/lib/net8.0/Foo.pdb'
        'Foo.1.0.0/lib/net9.0/Foo.pdb'
        'Foo.1.0.0/runtimes/win-x64/native/Foo.pdb'
    )
    foreach ($pdb in $expectedPdbs) {
        if (-not (Test-Path (Join-Path $pdbs $pdb) -PathType Leaf)) {
            throw "Missing extracted PDB: $pdb"
        }
    }
    if (Test-Path (Join-Path $pdbs 'Foo.1.0.0/ref/net8.0/Foo.pdb')) {
        throw 'Reference-assembly PDBs must not be published.'
    }
    if (Test-Path (Join-Path $pdbs 'Bar.1.0.0/lib/net8.0/Bar.pdb')) {
        throw 'A package with an explicit symbol package must not duplicate loose PDBs.'
    }
    if (Test-Path (Join-Path $packages 'Shipping/Foo.1.0.0.symbols.nupkg')) {
        throw 'Managed PDBs must use PdbArtifacts instead of fallback symbol package copies.'
    }
    if (-not (Test-Path (Join-Path $packages 'Shipping/Bar.1.0.0.symbols.nupkg'))) {
        throw 'Explicit symbol packages must remain in Shipping.'
    }

    $nonShipping = @(Get-ChildItem (Join-Path $packages 'NonShipping') -Filter '*.nupkg' -File)
    if ($nonShipping.Count -ne 2 -or
        @($nonShipping | Where-Object Name -notlike '*-branch.*').Count -ne 0) {
        throw 'Only the branch-versioned transport family may enter NonShipping.'
    }

    New-Item $prTransport -ItemType Directory -Force | Out-Null
    foreach ($name in @(
        '_NuGets.0.0.0-pr.4865.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-pr.4865.1.nupkg')) {
        New-Package (Join-Path $prTransport $name) @{ 'README.md' = $name }
    }

    & $script `
        -SignedPackageDirectory $signed `
        -TransportPackageDirectory $prTransport `
        -PackageRoot $prPackages `
        -PdbArtifactsDirectory $prPdbs

    $prNonShipping = @(Get-ChildItem (Join-Path $prPackages 'NonShipping') -Filter '*.nupkg' -File)
    if ($prNonShipping.Count -ne 2 -or
        @($prNonShipping | Where-Object Name -notlike '*-pr.*').Count -ne 0) {
        throw 'Only the PR-versioned transport family may enter PR NonShipping.'
    }

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
