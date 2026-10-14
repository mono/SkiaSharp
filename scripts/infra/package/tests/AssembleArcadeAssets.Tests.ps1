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

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$root = Join-Path $repositoryRoot "output/tests/arcade-assets-$([Guid]::NewGuid().ToString('N'))"
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
$missingDocsOutput = Join-Path $root 'missing-docs'
$missingDocsProduct = Join-Path $missingDocsOutput 'nugets'
$mismatchedDocsOutput = Join-Path $root 'mismatched-docs'
$mismatchedDocsProduct = Join-Path $mismatchedDocsOutput 'nugets'
$cake = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../nuget.cake'))

function Assert-Equal([object] $Expected, [object] $Actual, [string] $Message) {
    if ($Expected -cne $Actual) {
        throw "$Message Expected '$Expected', received '$Actual'."
    }
}

function Invoke-MSBuildQuery {
    param(
        [string] $Project,
        [string] $Target,
        [string[]] $Properties = @(),
        [string] $Query
    )

    $arguments = @('msbuild', (Join-Path $repositoryRoot $Project), '-nologo', "-t:$Target") +
        $Properties + @($Query)
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& dotnet @arguments 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -ne 0) {
        throw "MSBuild query failed: $($output -join "`n")"
    }
    return ($output -join "`n") | ConvertFrom-Json
}

function Get-PackageVersionInfo([string] $Project, [string] $Suffix = '') {
    $properties = @('-p:TargetFramework=net10.0', '-p:TargetFrameworks=net10.0', "-p:VersionSuffix=$Suffix")
    return Invoke-MSBuildQuery -Project $Project -Target '_GetProjectVersion' -Properties $properties `
        -Query '-getProperty:Version,PackageVersion,AssemblyVersion,FileVersion,IsPackable,IsShipping,SignAssembly,GenerateDocumentationFile,ProduceReferenceAssembly,GenerateNuspecDependsOn,TargetsForTfmSpecificContentInPackage,PackageReadmeNotice'
}

function Assert-IntegrationVersion([string] $CoreVersion) {
    $properties = @("-p:SkiaSharpVersion=$CoreVersion", '-p:HarfBuzzSharpVersion=14.4.0.100',
        '-p:PackageSource=https://example.test/bar/index.json')
    $result = Invoke-MSBuildQuery -Project 'tests/SkiaSharp.Tests.Integration/SkiaSharp.Tests.Integration.csproj' `
        -Target 'ValidateVersions' -Properties $properties `
        -Query '-getItem:PackageReference,ProjectReference'
    Assert-Equal 0 @($result.Items.ProjectReference).Count 'Integration must not reference any local project.'
    foreach ($identity in @('SkiaSharp', 'SkiaSharp.Testing', 'HarfBuzzSharp')) {
        $reference = @($result.Items.PackageReference | Where-Object Identity -eq $identity)
        Assert-Equal 1 $reference.Count "Missing or duplicate integration package reference: $identity."
        $expectedVersion = switch ($identity) {
            'SkiaSharp' { $CoreVersion }
            'SkiaSharp.Testing' { $CoreVersion }
            'HarfBuzzSharp' { '14.4.0.100' }
        }
        Assert-Equal $expectedVersion $reference[0].Version "Incorrect integration version for $identity."
    }
}

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
    $helperProject = 'source/SkiaSharp.Testing/SkiaSharp.Testing.csproj'
    $coreProject = 'binding/SkiaSharp/SkiaSharp.csproj'
    $helper = (Get-PackageVersionInfo $helperProject).Properties
    $core = (Get-PackageVersionInfo $coreProject).Properties
    Assert-Equal $core.PackageVersion $helper.PackageVersion 'The helper must use the exact core package version.'
    Assert-Equal $core.Version $helper.Version 'The helper must retain its family Version.'
    Assert-Equal $core.Version $core.PackageVersion 'The core dependency must retain its stable package version.'
    Assert-Equal $core.AssemblyVersion $helper.AssemblyVersion 'The helper must retain the core assembly version.'
    Assert-Equal $core.FileVersion $helper.FileVersion 'The helper must retain the core file version.'
    foreach ($property in @('IsPackable', 'IsShipping', 'SignAssembly', 'GenerateDocumentationFile', 'ProduceReferenceAssembly')) {
        Assert-Equal 'true' $helper.$property "Helper package metadata must enable $property."
    }
    if ($helper.PackageReadmeNotice -notmatch 'Experimental API') {
        throw 'The generated package README must explain that the helper API is experimental.'
    }
    if ($helper.GenerateNuspecDependsOn -notmatch 'CollectAssemblyVersionInfo' -or
        $helper.TargetsForTfmSpecificContentInPackage -notmatch '_IncludeAdditionalTfmSpecificPackageFiles') {
        throw 'The helper must use normal shared version and XML/reference-assembly packaging targets.'
    }
    foreach ($suffix in @('preview.2.12345', 'rc.1.12345', 'pr.123.456')) {
        $preview = (Get-PackageVersionInfo $helperProject $suffix).Properties
        Assert-Equal "$($core.Version)-$suffix" $preview.PackageVersion 'Existing prerelease identities must be preserved.'
        Assert-Equal $preview.Version $preview.PackageVersion 'The helper must follow the family package version.'
        Assert-Equal $core.AssemblyVersion $preview.AssemblyVersion 'Prereleases must not change assembly versions.'
        Assert-Equal $core.FileVersion $preview.FileVersion 'Prereleases must not change file versions.'
        $corePreview = (Get-PackageVersionInfo $coreProject $suffix).Properties
        Assert-Equal $corePreview.PackageVersion $preview.PackageVersion 'Prerelease helper and core dependency versions must agree.'
        Assert-IntegrationVersion $preview.PackageVersion
    }
    Assert-IntegrationVersion $core.PackageVersion
    Assert-IntegrationVersion '4.156.0.1'
    $native = (Get-PackageVersionInfo 'binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj').Properties
    Assert-Equal $core.PackageVersion $native.PackageVersion 'Native packages must retain the family package version.'
    $harfBuzz = (Get-PackageVersionInfo 'binding/HarfBuzzSharp/HarfBuzzSharp.csproj').Properties
    Assert-Equal $harfBuzz.Version $harfBuzz.PackageVersion 'HarfBuzzSharp package versions must be unchanged.'
    [xml] $signing = Get-Content (Join-Path $repositoryRoot 'eng/Signing.props') -Raw
    Assert-Equal 1 @($signing.SelectNodes("//FirstPartyFile[@Include='SkiaSharp.Testing.dll']")).Count `
        'Register the helper assembly in the first-party signing inventory.'
    $inventory = Get-Content (Join-Path $repositoryRoot 'scripts/infra/shared/shared.cake') -Raw
    if ($inventory -notmatch '"SkiaSharp\.Testing",\s+new Version \(4, 156, 0\)') {
        throw 'Register SkiaSharp.Testing at its actual introduction line.'
    }

    New-Item $product -ItemType Directory -Force | Out-Null
    New-Item $transport -ItemType Directory -Force | Out-Null
    New-Item $emptyProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $emptyOutput 'nugets-special') -ItemType Directory -Force | Out-Null
    New-Item $escapingProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $escapingOutput 'nugets-special') -ItemType Directory -Force | Out-Null
    New-Item $missingDocsProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $missingDocsOutput 'nugets-special') -ItemType Directory -Force | Out-Null
    New-Item $mismatchedDocsProduct -ItemType Directory -Force | Out-Null
    New-Item (Join-Path $mismatchedDocsOutput 'nugets-special') -ItemType Directory -Force | Out-Null

    New-Package (Join-Path $product 'Foo.1.0.0.nupkg') @{
        'lib/net8.0/Foo.dll' = 'dll8'
        'lib/net8.0/Foo.xml' = '<doc />'
        'lib/net8.0/Foo.pdb' = 'pdb8'
        'lib/net9.0/Foo.dll' = 'dll9'
        'lib/net9.0/Foo.xml' = '<doc />'
        'lib/net9.0/Foo.pdb' = 'pdb9'
        'ref/net8.0/Foo.dll' = 'ref8'
        'ref/net8.0/Foo.xml' = '<doc />'
        'ref/net8.0/Foo.pdb' = 'reference'
        'ref/net9.0/Foo.dll' = 'ref9'
        'ref/net9.0/Foo.xml' = '<doc />'
        'runtimes/win-x64/native/Foo.pdb' = 'native'
    }
    New-Package (Join-Path $product 'Bar.1.0.0.nupkg') @{
        'lib/net8.0/Bar.dll' = 'dll'
        'lib/net8.0/Bar.pdb' = 'normal-pdb'
    }
    New-Package (Join-Path $product 'Bar.1.0.0.symbols.nupkg') @{
        'lib/net8.0/Bar.pdb' = 'explicit-pdb'
    }
    New-Package (Join-Path $product 'SkiaSharp.4.156.0.nupkg') @{
        'lib/net10.0/SkiaSharp.dll' = 'core'
    }
    New-Package (Join-Path $product 'SkiaSharp.Testing.4.156.0.nupkg') @{
        'lib/net10.0/SkiaSharp.Testing.dll' = 'helper'
        'lib/net10.0/SkiaSharp.Testing.xml' = '<doc />'
        'ref/net10.0/SkiaSharp.Testing.dll' = 'reference'
        'ref/net10.0/SkiaSharp.Testing.xml' = '<doc />'
    }

    foreach ($name in @(
        '_NuGets.0.0.0-branch.main.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-branch.main.1.nupkg')) {
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
    foreach ($name in @('SkiaSharp.4.156.0.nupkg', 'SkiaSharp.Testing.4.156.0.nupkg')) {
        if (-not (Test-Path (Join-Path $packages "Shipping/$name"))) {
            throw "Core and helper packages must both ship with the same family version: $name."
        }
    }

    $nonShipping = @(Get-ChildItem (Join-Path $packages 'NonShipping') -Filter '*.nupkg' -File)
    if ($nonShipping.Count -ne 2 -or
        @($nonShipping | Where-Object Name -notlike '*-branch.*').Count -ne 0) {
        throw 'Only the branch-versioned transport family may enter NonShipping.'
    }

    Copy-Item $product (Join-Path $prOutput 'nugets') -Recurse
    $prProduct = Join-Path $prOutput 'nugets'
    Remove-Item (Join-Path $prProduct 'SkiaSharp.4.156.0.nupkg'),
        (Join-Path $prProduct 'SkiaSharp.Testing.4.156.0.nupkg')
    foreach ($id in @('SkiaSharp', 'SkiaSharp.Testing')) {
        New-Package (Join-Path $prProduct "$id.4.156.0-pr.4863.1.nupkg") @{
            "lib/net10.0/$id.dll" = 'dll'
        }
    }
    $prTransport = New-Item (Join-Path $prOutput 'nugets-special') -ItemType Directory -Force
    foreach ($name in @(
        '_NuGets.0.0.0-pr.4863.1.nupkg'
        '_NuGets.Dependencies.1.0.0.0-pr.4863.1.nupkg')) {
        New-Package (Join-Path $prTransport $name) @{ 'README.md' = $name }
    }
    Invoke-Assembly -OutputDirectory $prOutput -PreviewLabel 'pr.4863'
    foreach ($id in @('SkiaSharp', 'SkiaSharp.Testing')) {
        if (-not (Test-Path (Join-Path $prPackages "Shipping/$id.4.156.0-pr.4863.1.nupkg"))) {
            throw "PR product packages must preserve their build identity and remain Shipping: $id."
        }
    }

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

    New-Package (Join-Path $missingDocsProduct 'MissingDocs.1.0.0.nupkg') @{
        'lib/net8.0/MissingDocs.dll' = 'dll'
        'ref/net8.0/MissingDocs.dll' = 'reference'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $missingDocsOutput 'nugets-special') -Recurse
    Invoke-Assembly -OutputDirectory $missingDocsOutput -ExpectFailure

    New-Package (Join-Path $mismatchedDocsProduct 'MismatchedDocs.1.0.0.nupkg') @{
        'lib/net8.0/MismatchedDocs.dll' = 'dll'
        'lib/net8.0/MismatchedDocs.xml' = '<doc>implementation</doc>'
        'ref/net8.0/MismatchedDocs.dll' = 'reference'
        'ref/net8.0/MismatchedDocs.xml' = '<doc>reference</doc>'
    }
    Copy-Item (Join-Path $transport '*') (Join-Path $mismatchedDocsOutput 'nugets-special') -Recurse
    Invoke-Assembly -OutputDirectory $mismatchedDocsOutput -ExpectFailure

    Write-Host 'Arcade asset assembly tests passed.'
} finally {
    Remove-Item $root -Recurse -Force -ErrorAction Ignore
}
