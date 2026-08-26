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

function Run-Cake {
    param(
        [Parameter(Mandatory)]
        [string] $Products,

        [Parameter(Mandatory)]
        [string] $Transport,

        [Parameter(Mandatory)]
        [string] $PackageRoot,

        [Parameter(Mandatory)]
        [string] $PdbRoot,

        [switch] $ExpectFailure
    )

    $cake = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../nuget.cake'))
    $args = @(
        'cake'
        $cake
        '--target=nuget-assemble-arcade-assets'
        "--productPackageDirectory=$Products"
        "--transportPackageDirectory=$Transport"
        "--packageRoot=$PackageRoot"
        "--pdbArtifactsDirectory=$PdbRoot"
        '--verbosity=quiet'
    )

    & dotnet @args
    $failed = $LASTEXITCODE -ne 0
    if ($ExpectFailure) {
        if (-not $failed) {
            throw 'Cake unexpectedly accepted invalid Arcade assets.'
        }
        $global:LASTEXITCODE = 0
    } elseif ($failed) {
        throw "Cake failed with exit code $LASTEXITCODE."
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
$emptyProducts = Join-Path $root 'empty-products'
$emptyPackages = Join-Path $root 'empty-packages'
$emptyPdbs = Join-Path $root 'empty-pdbs'
$escapeProducts = Join-Path $root 'escape-products'
$escapePackages = Join-Path $root 'escape-packages'
$escapePdbs = Join-Path $root 'escape-pdbs'

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

    Run-Cake `
        -Products $signed `
        -Transport $transport `
        -PackageRoot $packages `
        -PdbRoot $pdbs

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
    if (-not (Test-Path (Join-Path $packages 'Shipping/Foo.1.0.0.nupkg')) -or
        -not (Test-Path (Join-Path $packages 'Shipping/Bar.1.0.0.nupkg'))) {
        throw 'All product packages must enter Shipping.'
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

    Run-Cake `
        -Products $signed `
        -Transport $prTransport `
        -PackageRoot $prPackages `
        -PdbRoot $prPdbs

    $prNonShipping = @(Get-ChildItem (Join-Path $prPackages 'NonShipping') -Filter '*.nupkg' -File)
    if ($prNonShipping.Count -ne 2 -or
        @($prNonShipping | Where-Object Name -notlike '*-pr.*').Count -ne 0) {
        throw 'Only the PR-versioned transport family may enter PR NonShipping.'
    }

    New-Item $emptyProducts -ItemType Directory -Force | Out-Null
    New-Package (Join-Path $emptyProducts 'NoPdb.1.0.0.nupkg') @{
        'lib/net8.0/NoPdb.dll' = 'dll'
    }
    Run-Cake `
        -Products $emptyProducts `
        -Transport $transport `
        -PackageRoot $emptyPackages `
        -PdbRoot $emptyPdbs
    if (-not (Test-Path (Join-Path $emptyPdbs '.empty') -PathType Leaf)) {
        throw 'PdbArtifacts must contain .empty when no loose PDB is eligible.'
    }

    New-Item $escapeProducts -ItemType Directory -Force | Out-Null
    New-Package (Join-Path $escapeProducts 'Escape.1.0.0.nupkg') @{
        '../escape.pdb' = 'escape'
    }
    Run-Cake `
        -Products $escapeProducts `
        -Transport $transport `
        -PackageRoot $escapePackages `
        -PdbRoot $escapePdbs `
        -ExpectFailure
    if (Test-Path (Join-Path $escapePdbs 'escape.pdb')) {
        throw 'A package entry escaped the PDB extraction root.'
    }

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
