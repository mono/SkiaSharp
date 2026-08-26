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
        [string] $OutputDirectory,

        [string] $PreviewLabel = 'preview.0',

        [switch] $ExpectFailure
    )

    $cake = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../nuget.cake'))
    $args = @(
        'cake'
        $cake
        '--target=nuget-assemble-arcade-assets'
        "--outputPath=$OutputDirectory"
        "--previewLabel=$PreviewLabel"
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
$branchOutput = Join-Path $root 'branch'
$product = Join-Path $branchOutput 'nugets'
$transport = Join-Path $branchOutput 'nugets-special'
$packages = Join-Path $branchOutput 'arcade-assets'
$pdbs = Join-Path $branchOutput 'pdbs'
$prOutput = Join-Path $root 'pr'
$prPackages = Join-Path $prOutput 'arcade-assets'
$emptyOutput = Join-Path $root 'empty'
$emptyProduct = Join-Path $emptyOutput 'nugets'
$emptyPdbs = Join-Path $emptyOutput 'pdbs'
$escapeOutput = Join-Path $root 'escape'
$escapeProduct = Join-Path $escapeOutput 'nugets'

try {
    New-Item $product -ItemType Directory -Force | Out-Null
    New-Item $transport -ItemType Directory -Force | Out-Null
    New-Item $emptyProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $emptyOutput 'nugets-special') -ItemType Directory -Force | Out-Null
    New-Item $escapeProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $escapeOutput 'nugets-special') -ItemType Directory -Force | Out-Null

    New-Package (Join-Path $product 'Foo.1.0.0.nupkg') @{
        'lib/net8.0/Foo.dll' = 'dll8'
        'lib/net8.0/Foo.pdb' = 'pdb8'
        'lib/net9.0/Foo.dll' = 'dll9'
        'lib/net9.0/Foo.pdb' = 'pdb9'
        'ref/net8.0/Foo.pdb' = 'reference'
        'runtimes/win-x64/native/Foo.pdb' = 'native'
    }
    New-Package (Join-Path $product 'Bar.1.0.0.nupkg') @{
        'lib/net8.0/Bar.dll' = 'dll'
        'lib/net8.0/Bar.pdb' = 'normal-pdb'
    }
    New-Package (Join-Path $product 'Bar.1.0.0.symbols.nupkg') @{
        'lib/net8.0/Bar.pdb' = 'explicit-pdb'
    }

    foreach ($name in @(
        '_NuGets.0.0.0-branch.main.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-branch.main.1.nupkg'
        '_NuGets.0.0.0-commit.abc.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-commit.abc.1.nupkg'
        '_NuGets.0.0.0-pr.4865.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-pr.4865.1.nupkg')) {
        New-Package (Join-Path $transport $name) @{ 'README.md' = $name }
    }

    Run-Cake -OutputDirectory $branchOutput

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

    Copy-Item $product (Join-Path $prOutput 'nugets') -Recurse
    Copy-Item $transport (Join-Path $prOutput 'nugets-special') -Recurse
    Run-Cake -OutputDirectory $prOutput -PreviewLabel 'pr.4865'

    $prNonShipping = @(Get-ChildItem (Join-Path $prPackages 'NonShipping') -Filter '*.nupkg' -File)
    if ($prNonShipping.Count -ne 2 -or
        @($prNonShipping | Where-Object Name -notlike '*-pr.*').Count -ne 0) {
        throw 'Only the PR-versioned transport family may enter PR NonShipping.'
    }

    New-Package (Join-Path $emptyProduct 'NoPdb.1.0.0.nupkg') @{
        'lib/net8.0/NoPdb.dll' = 'dll'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $emptyOutput 'nugets-special') -Recurse
    Run-Cake -OutputDirectory $emptyOutput
    if (-not (Test-Path (Join-Path $emptyPdbs '.empty') -PathType Leaf)) {
        throw 'PdbArtifacts must contain .empty when no loose PDB is eligible.'
    }

    New-Package (Join-Path $escapeProduct 'Escape.1.0.0.nupkg') @{
        '../escape.pdb' = 'escape'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $escapeOutput 'nugets-special') -Recurse
    Run-Cake -OutputDirectory $escapeOutput -ExpectFailure
    if (Test-Path (Join-Path $escapeOutput 'pdbs/escape.pdb')) {
        throw 'A package entry escaped the PDB extraction root.'
    }

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
