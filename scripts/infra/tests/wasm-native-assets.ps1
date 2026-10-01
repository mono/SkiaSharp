param()

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$fixture = Join-Path ([System.IO.Path]::GetTempPath()) "skiasharp-wasm-assets-$([Guid]::NewGuid().ToString('N'))"
$versions = @('3.1.34', '3.1.56', '5.0.6', '6.0.2')
$variants = @('st', 'st,simd', 'mt', 'mt,simd')
$libraries = @('SkiaSharp', 'HarfBuzzSharp')

function Get-Items($project, $items, $properties) {
    $arguments = @('msbuild', $project, '-nologo', '-verbosity:quiet', "-getItem:$items") + $properties
    $output = & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild evaluation failed: $project`n$($output -join "`n")"
    }
    return ($output -join "`n" | ConvertFrom-Json).Items
}

function Assert-Equal($actual, $expected, $context) {
    if ($actual -ne $expected) {
        throw "${context}: expected '$expected', got '$actual'."
    }
}

try {
    $matrix = Get-Content (Join-Path $repo 'scripts\azure-templates-stages-native-wasm.yml') -Raw
    $merge = Get-Content (Join-Path $repo 'scripts\azure-templates-stages-native-merge.yml') -Raw
    Assert-Equal ([regex]::Matches($matrix, 'version: 6\.0\.2\b').Count) 4 'Emscripten 6 build variants'
    Assert-Equal ([regex]::Matches($matrix, 'features: _wasmeh,_newexc,').Count) 4 'Emscripten 6 exception mode'
    Assert-Equal $matrix.Contains('5.0.6') $false 'Retired compiler build'
    Assert-Equal $merge.Contains('native_wasm_5_0_6') $false 'Retired compiler artifacts'
    foreach ($suffix in @('', '_Threading', '_SIMD', '_SIMD_Threading')) {
        $artifact = "native_wasm_6_0_2${suffix}_linux"
        Assert-Equal ([regex]::Matches($merge, "\b$artifact\b").Count) 2 "$artifact merger lists"
    }
    foreach ($library in $libraries) {
        $package = Join-Path $fixture "binding\$library.NativeAssets.WebAssembly"
        New-Item -ItemType Directory -Path (Join-Path $package 'buildTransitive') -Force | Out-Null
        Copy-Item (Join-Path $repo "binding\$library.NativeAssets.WebAssembly\$library.NativeAssets.WebAssembly.csproj") $package
        Copy-Item (Join-Path $repo "binding\$library.NativeAssets.WebAssembly\buildTransitive\*") (Join-Path $package 'buildTransitive')
        Copy-Item (Join-Path $repo "binding\IncludeNativeAssets.$library.targets") (Join-Path $fixture 'binding')
        foreach ($version in $versions) {
            foreach ($variant in $variants) {
                foreach ($directory in @(
                    (Join-Path $fixture "output\native\wasm\lib$library.a\$version\$variant"),
                    (Join-Path $package "buildTransitive\lib$library.a\$version\$variant")
                )) {
                    New-Item -ItemType Directory -Path $directory -Force | Out-Null
                    [System.IO.File]::WriteAllBytes((Join-Path $directory "lib$library.a"), [byte[]]@())
                }
            }
        }
    }
    $dawn = Join-Path $fixture 'output\native\wasm\emdawnwebgpu_pkg\webgpu\src'
    New-Item -ItemType Directory -Path $dawn -Force | Out-Null
    [System.IO.File]::WriteAllBytes((Join-Path $dawn 'webgpu.cpp'), [byte[]]@())
    Copy-Item (Join-Path $PSScriptRoot 'wasm-native-assets.proj') $fixture
    $probe = Join-Path $fixture 'wasm-native-assets.proj'
    $checks = 0
    foreach ($library in $libraries) {
        foreach ($surface in @('Package', 'Source')) {
            $common = @("-p:FixtureRoot=$fixture", "-p:Library=$library", "-p:AssetSurface=$surface")
            foreach ($sdk in @('BlazorWebAssembly', 'WebAssembly')) {
                foreach ($framework in 8..12) {
                    $expectedVersion = if ($framework -eq 8) { '3.1.34' } elseif ($framework -lt 11) { '3.1.56' } else { '6.0.2' }
                    foreach ($variant in $variants) {
                        $threads = $variant.StartsWith('mt').ToString()
                        $simd = $variant.Contains('simd').ToString()
                        $properties = $common + @("-p:Version=$framework.0", "-p:UsingMicrosoftNETSdk$sdk=true", "-p:WasmEnableThreads=$threads", "-p:WasmEnableSIMD=$simd")
                        $items = Get-Items $probe 'NativeFileReference' $properties
                        Assert-Equal @($items.NativeFileReference).Count 1 "$library/$surface/net$framework/$sdk/$variant count"
                        $path = $items.NativeFileReference[0].Identity.Replace('/', '\')
                        Assert-Equal $path.EndsWith("\lib$library.a\$expectedVersion\$variant\lib$library.a") $true "$library/$surface/net$framework/$sdk/$variant path"
                        $checks++
                    }
                }
            }
            $uno = Get-Items $probe 'Content' ($common + @('-p:Version=11.0', '-p:IsUnoHead=True', '-p:UnoRuntimeIdentifier=WebAssembly', '-p:WasmEnableSIMD=True'))
            $expectedCount = if ($surface -eq 'Package') { 12 } else { 3 }
            Assert-Equal @($uno.Content).Count $expectedCount "$library/$surface Uno archive count"
            Assert-Equal @($uno.Content | Where-Object { $_.Identity -match '[\\/]5\.0\.6[\\/]' }).Count 0 "$library/$surface Uno stale archives"
        }
        $project = Join-Path $fixture "binding\$library.NativeAssets.WebAssembly\$library.NativeAssets.WebAssembly.csproj"
        $packageItems = Get-Items $project 'PackageFile' @('-p:BasicTargetFrameworks=net10.0', '-p:TargetFramework=net10.0')
        $archives = @($packageItems.PackageFile | Where-Object { $_.Identity.EndsWith('.a') })
        Assert-Equal $archives.Count 12 "$library package archive count"
        foreach ($archive in $archives) {
            $relative = $archive.Identity.Replace('/', '\').Split(@("\lib$library.a\"), [System.StringSplitOptions]::None)[1]
            Assert-Equal $archive.PackagePath.Replace('/', '\') "buildTransitive\netstandard1.0\lib$library.a\$relative" "$library archive package path"
            Assert-Equal $relative.StartsWith('5.0.6\') $false "$library package stale archive"
        }
        if ($library -eq 'SkiaSharp') {
            Assert-Equal @($packageItems.PackageFile | Where-Object { $_.Identity.EndsWith('webgpu.cpp') }).Count 1 'Dawn port package inclusion'
        }
    }
    Write-Host "Passed $checks native selection checks, CI matrix, merger lists, Uno inclusion, package paths and stale-version exclusion."
}
finally {
    if (Test-Path $fixture) {
        Remove-Item -LiteralPath $fixture -Recurse -Force
    }
}
