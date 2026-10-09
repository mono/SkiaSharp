$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-sample-generation-$([Guid]::NewGuid().ToString('N'))"
$cake = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../samples.cake'))

function Get-PackageVersion {
    param([string] $Project, [string] $Package)
    [xml] $xml = Get-Content $Project -Raw
    $references = @($xml.SelectNodes("//PackageReference[@Include='$Package']"))
    if ($references.Count -ne 1) {
        throw "Expected one $Package reference in $Project."
    }
    return $references[0].Version
}

function Assert-Version {
    param([string] $Actual, [string] $Expected)
    if ($Actual -cne $Expected) {
        throw "Expected '$Expected', got '$Actual'."
    }
}

try {
    foreach ($case in @('preview', 'stable', 'publication')) {
        $output = Join-Path $root $case
        New-Item $output -ItemType Directory -Force | Out-Null
        $suffix = if ($case -eq 'preview') { '-preview.0.26508.29' } else { '' }
        if ($case -ne 'publication') {
            $packages = Join-Path $output 'nugets'
            New-Item $packages -ItemType Directory | Out-Null
            foreach ($name in @("SkiaSharp.1.2.3$suffix.nupkg", "HarfBuzzSharp.4.5.6$suffix.nupkg",
                'SkiaSharp.9.9.9.symbols.nupkg')) {
                New-Item (Join-Path $packages $name) -ItemType File | Out-Null
            }
        }

        & dotnet cake $cake --target=samples-generate "--outputPath=$output" `
            --previewLabel=preview.0 --buildNumber=26509.2 --verbosity=quiet
        if ($LASTEXITCODE -ne 0) {
            throw "Sample generation failed for $case with exit code $LASTEXITCODE."
        }

        $tree = if ($case -eq 'stable') { 'samples' } else { 'samples-preview' }
        $console = Join-Path $output "$tree/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj"
        $web = Join-Path $output "$tree/Basic/Web/SkiaSharpSample/SkiaSharpSample.csproj"
        $uno = Join-Path $output "$tree/Gallery/Uno/SkiaSharpSample.Uno.csproj"
        $skia = "1.2.3$suffix"
        $harfBuzz = "4.5.6$suffix"
        if ($case -eq 'publication') {
            $skia = (Get-PackageVersion (Join-Path $output 'samples/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj') 'SkiaSharp') + '-preview.0.26509.2'
            $harfBuzz = (Get-PackageVersion (Join-Path $output 'samples/Gallery/Uno/SkiaSharpSample.Uno.csproj') 'HarfBuzzSharp.NativeAssets.Linux') + '-preview.0.26509.2'
        }
        Assert-Version (Get-PackageVersion $console 'SkiaSharp') $skia
        Assert-Version (Get-PackageVersion $console 'SkiaSharp.NativeAssets.Linux.NoDependencies') $skia
        Assert-Version (Get-PackageVersion $web 'SkiaSharp.NativeAssets.Linux.NoDependencies') $skia
        Assert-Version (Get-PackageVersion $uno 'HarfBuzzSharp.NativeAssets.Linux') $harfBuzz
        [xml] $unoXml = Get-Content $uno -Raw
        Assert-Version $unoXml.SelectSingleNode('//SkiaSharpVersion').InnerText $skia
    }
    foreach ($case in @('mismatched', 'duplicate', 'incomplete')) {
        $output = Join-Path $root $case
        $packages = Join-Path $output 'nugets'
        New-Item $packages -ItemType Directory -Force | Out-Null
        $names = @('SkiaSharp.1.2.3-pr.1.nupkg', 'HarfBuzzSharp.4.5.6-pr.1.nupkg')
        if ($case -eq 'mismatched') { $names[1] = 'HarfBuzzSharp.4.5.6-pr.2.nupkg' }
        if ($case -eq 'duplicate') { $names += 'SkiaSharp.1.2.3-pr.2.nupkg' }
        if ($case -eq 'incomplete') { $names = @($names[0]) }
        foreach ($name in $names) {
            New-Item (Join-Path $packages $name) -ItemType File | Out-Null
        }
        $generation = & dotnet cake $cake --target=samples-generate "--outputPath=$output" --verbosity=quiet 2>&1
        if ($LASTEXITCODE -eq 0) {
            throw "Sample generation unexpectedly accepted $case packages."
        }
        $expected = switch ($case) {
            mismatched { 'packages from the same producing build' }
            duplicate { 'exactly one non-symbol SkiaSharp core package' }
            incomplete { 'exactly one non-symbol HarfBuzzSharp core package' }
        }
        if (-not ($generation -join "`n").Contains($expected)) {
            throw "Unexpected generation failure for ${case}: $($generation -join "`n")"
        }
        $global:LASTEXITCODE = 0
    }
    Write-Host 'Sample generation passed: preview, stable, publication, mismatched, duplicate, incomplete.'
} finally {
    if (Test-Path $root) {
        Remove-Item $root -Recurse -Force
    }
}
