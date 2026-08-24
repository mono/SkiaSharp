$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../stage-shipping-symbol-packages.ps1'))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-symbol-staging-tests-$([Guid]::NewGuid().ToString('N'))"

try {
    New-Item $testRoot -ItemType Directory -Force | Out-Null
    $foo = Join-Path $testRoot 'Foo.1.0.0-preview.1.nupkg'
    $fooSymbols = Join-Path $testRoot 'Foo.1.0.0-preview.1.symbols.nupkg'
    $bar = Join-Path $testRoot 'Bar.1.0.0-preview.1.nupkg'
    [IO.File]::WriteAllText($foo, 'foo-normal')
    [IO.File]::WriteAllText($fooSymbols, 'foo-real-symbols')
    [IO.File]::WriteAllText($bar, 'bar-normal')

    & $scriptPath -PackageDirectory $testRoot

    if ([IO.File]::ReadAllText($fooSymbols) -cne 'foo-real-symbols') {
        throw 'An explicit symbol package was overwritten.'
    }

    $barSymbols = Join-Path $testRoot 'Bar.1.0.0-preview.1.symbols.nupkg'
    if (-not (Test-Path $barSymbols -PathType Leaf)) {
        throw 'The fallback symbol package was not created.'
    }
    if ((Get-FileHash $bar).Hash -cne (Get-FileHash $barSymbols).Hash) {
        throw 'The fallback symbol package is not byte-identical to the shipping package.'
    }

    [IO.File]::WriteAllText((Join-Path $testRoot '_Transport.1.0.0.nupkg'), 'transport')
    $rejected = $false
    try {
        & $scriptPath -PackageDirectory $testRoot
    } catch {
        $rejected = $true
    }
    if (-not $rejected) {
        throw 'A transport package in the shipping directory was not rejected.'
    }

    Write-Host 'Shipping symbol staging tests passed.'
} finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction Ignore
}
