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
$branchOutput = Join-Path $root 'branch'
$product = Join-Path $branchOutput 'nugets'
$transport = Join-Path $branchOutput 'nugets-special'
$packages = Join-Path $branchOutput 'arcade-assets'
$pdbs = Join-Path $branchOutput 'pdbs'
$prOutput = Join-Path $root 'pr'
$prPackages = Join-Path $prOutput 'arcade-assets'
$prPdbs = Join-Path $prOutput 'pdbs'
$emptyOutput = Join-Path $root 'empty'
$emptyProduct = Join-Path $emptyOutput 'nugets'
$emptyPackages = Join-Path $emptyOutput 'arcade-assets'
$emptyPdbs = Join-Path $emptyOutput 'pdbs'
$escapingOutput = Join-Path $root 'escaping'
$escapingProduct = Join-Path $escapingOutput 'nugets'
$cake = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../nuget.cake'))

function Invoke-Assembly {
    param(
        [Parameter(Mandatory)]
        [string] $OutputDirectory,

        [string] $PreviewLabel = 'preview.0',

        [switch] $ExpectFailure
    )

    & dotnet cake $cake `
        --target=nuget-assemble-arcade-assets `
        "--outputPath=$OutputDirectory" `
        "--previewLabel=$PreviewLabel" `
        --verbosity=quiet
    if ($ExpectFailure) {
        if ($LASTEXITCODE -eq 0) {
            throw 'Cake asset assembly unexpectedly accepted an escaping PDB path.'
        }
        $global:LASTEXITCODE = 0
    } elseif ($LASTEXITCODE -ne 0) {
        throw "Cake asset assembly failed with exit code $LASTEXITCODE."
    }
}

try {
    New-Item $product -ItemType Directory -Force | Out-Null
    New-Item $transport -ItemType Directory -Force | Out-Null
    New-Item $emptyProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $emptyOutput 'nugets-special') -ItemType Directory -Force | Out-Null
    New-Item $escapingProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $escapingOutput 'nugets-special') -ItemType Directory -Force | Out-Null

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
        '_NuGets.0.0.0-pr.4863.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-pr.4863.1.nupkg')) {
        New-Package (Join-Path $transport $name) @{ 'README.md' = $name }
    }

    Invoke-Assembly -OutputDirectory $branchOutput

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
    if (-not (Test-Path (Join-Path $packages 'Shipping/Foo.1.0.0.nupkg'))) {
        throw 'Product packages must remain in Shipping.'
    }

    $nonShipping = @(Get-ChildItem (Join-Path $packages 'NonShipping') -Filter '*.nupkg' -File)
    if ($nonShipping.Count -ne 2 -or
        @($nonShipping | Where-Object Name -like '*-commit.*').Count -ne 0) {
        throw 'Only the branch-versioned transport family may enter NonShipping.'
    }

    Copy-Item $product (Join-Path $prOutput 'nugets') -Recurse
    Copy-Item $transport (Join-Path $prOutput 'nugets-special') -Recurse
    Invoke-Assembly -OutputDirectory $prOutput -PreviewLabel 'pr.4863'

    $prNonShipping = @(Get-ChildItem (Join-Path $prPackages 'NonShipping') -Filter '*.nupkg' -File)
    if ($prNonShipping.Count -ne 2 -or
        @($prNonShipping | Where-Object Name -notlike '*-pr.*').Count -ne 0) {
        throw 'Public PR validation must stage only the PR-versioned transport family.'
    }

    New-Package (Join-Path $emptyProduct 'Empty.1.0.0.nupkg') @{
        'lib/net8.0/Empty.dll' = 'dll'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $emptyOutput 'nugets-special') -Recurse
    Invoke-Assembly -OutputDirectory $emptyOutput

    $emptyPdbFiles = @(Get-ChildItem $emptyPdbs -File -Recurse -Force)
    if ($emptyPdbFiles.Count -ne 1 -or $emptyPdbFiles[0].Name -ne '.empty') {
        throw 'PdbArtifacts must contain only .empty when no eligible PDB exists.'
    }

    New-Package (Join-Path $escapingProduct 'Escaping.1.0.0.nupkg') @{
        '../escape.pdb' = 'escape'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $escapingOutput 'nugets-special') -Recurse
    Invoke-Assembly -OutputDirectory $escapingOutput -ExpectFailure
    if (Test-Path (Join-Path $escapingOutput 'pdbs/escape.pdb')) {
        throw 'An escaping PDB path wrote outside its package extraction root.'
    }

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
