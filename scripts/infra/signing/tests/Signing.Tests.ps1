$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.IO.Compression
Import-Module (Join-Path $PSScriptRoot '../NuGetPayload.psm1') -Force

function New-ArchiveBytes {
    param(
        [Parameter(Mandatory)]
        [hashtable] $Entries,

        [switch] $AddSignature
    )

    $memory = [IO.MemoryStream]::new()
    $archive = [IO.Compression.ZipArchive]::new(
        $memory,
        [IO.Compression.ZipArchiveMode]::Create,
        $true)
    try {
        foreach ($path in @($Entries.Keys | Sort-Object)) {
            $entry = $archive.CreateEntry($path)
            $stream = $entry.Open()
            try {
                $value = $Entries[$path]
                $bytes = if ($value -is [byte[]]) {
                    $value
                } else {
                    [Text.Encoding]::UTF8.GetBytes([string] $value)
                }
                $stream.Write($bytes, 0, $bytes.Length)
            } finally {
                $stream.Dispose()
            }
        }

        if ($AddSignature) {
            $signature = $archive.CreateEntry('.signature.p7s')
            $stream = $signature.Open()
            try {
                $bytes = [Text.Encoding]::UTF8.GetBytes('test-signature')
                $stream.Write($bytes, 0, $bytes.Length)
            } finally {
                $stream.Dispose()
            }
        }
    } finally {
        $archive.Dispose()
    }

    $bytes = $memory.ToArray()
    $memory.Dispose()
    return ,$bytes
}

function New-TestPackage {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [switch] $Signed,

        [switch] $KeepMacUnsigned,

        [switch] $Tampered,

        [switch] $TamperSkippedJs
    )

    $suffix = if ($Signed) { '-signed' } else { '' }
    $macSuffix = if ($Signed -and -not $KeepMacUnsigned) { '-signed' } else { '' }
    $nested = New-ArchiveBytes -Entries @{
        'lib/net10.0/HarfBuzzSharp.Subset.dll' = "nested-managed$suffix"
        'content/keep-nested.txt' = 'keep-nested'
    } -AddSignature:$Signed

    $entries = @{
        'lib/net10.0/SkiaSharp.dll' = "managed$suffix"
        'lib/net10.0/SkiaSharp.Views.dll' = "views$suffix"
        'lib/uap10.0/SkiaSharp.Views.WinUI.Native.winmd' = "winmd$suffix"
        'lib/net10.0/HarfBuzzSharp.dll' = "harfbuzz$suffix"
        'runtimes/win-x64/native/libSkiaSharp.dll' = "native-skia$suffix"
        'runtimes/win-x64/native/libHarfBuzzSharp.dll' = "native-harfbuzz$suffix"
        'runtimes/win-x64/native/libEGL.dll' = "egl$suffix"
        'runtimes/win-x64/native/libGLESv2.dll' = "gles$suffix"
        'runtimes/win-x64/native/zlib1.dll' = "zlib$suffix"
        'runtimes/osx/native/libSkiaSharp.dylib' = "mac-skia$macSuffix"
        'runtimes/osx/native/libHarfBuzzSharp.dylib' = "mac-harfbuzz$macSuffix"
        'tools/payload.nupkg' = $nested
        'content/site.js' = if ($TamperSkippedJs) { 'tampered-js' } else { 'source-js' }
        'content/keep.txt' = if ($Tampered) { 'tampered' } else { 'keep' }
    }

    [IO.File]::WriteAllBytes(
        $Path,
        (New-ArchiveBytes -Entries $entries -AddSignature:$Signed))
}

$testRoot = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-signing-tests-$([Guid]::NewGuid().ToString('N'))"
$unsigned = Join-Path $testRoot 'unsigned'
$signed = Join-Path $testRoot 'signed'
$signingProps = Join-Path $testRoot 'Signing.props'
$verifier = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../verify-signed-packages.ps1'))

try {
    New-Item $unsigned -ItemType Directory -Force | Out-Null
    New-Item $signed -ItemType Directory -Force | Out-Null
    New-TestPackage (Join-Path $unsigned 'SkiaSharp.Test.1.0.0.nupkg')
    New-TestPackage (Join-Path $unsigned 'SkiaSharp.Test.1.0.0.symbols.nupkg')
    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed
    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.symbols.nupkg') -Signed

    @'
<Project>
  <ItemGroup>
    <FirstPartyFile Include="HarfBuzzSharp.dll" />
    <FirstPartyFile Include="HarfBuzzSharp.Subset.dll" />
    <FirstPartyFile Include="libEGL.dll" />
    <FirstPartyFile Include="libGLESv2.dll" />
    <FirstPartyFile Include="libHarfBuzzSharp.dll" />
    <FirstPartyFile Include="libSkiaSharp.dll" />
    <FirstPartyFile Include="SkiaSharp.dll" />
    <FirstPartyFile Include="SkiaSharp.Views.dll" />
    <FirstPartyFile Include="SkiaSharp.Views.WinUI.Native.winmd" />
    <FirstPartyFile Include="zlib1.dll" />
    <MacDeveloperFile Include="libHarfBuzzSharp.dylib" />
    <MacDeveloperFile Include="libSkiaSharp.dylib" />
    <SkippedFile Include="site.js" />
  </ItemGroup>
</Project>
'@ | Set-Content $signingProps -Encoding utf8NoBOM

    $inventory = @(Get-NuGetPackageInventory $unsigned)
    $policy = Get-ArcadeSigningPolicy $inventory $signingProps
    $fileNames = @($policy.Files.Name)
    if (-not ($fileNames -contains 'SkiaSharp.dll') -or
        -not ($fileNames -contains 'libSkiaSharp.dylib') -or
        -not ($fileNames -contains 'SkiaSharp.Views.WinUI.Native.winmd')) {
        throw 'Arcade signing policy did not include the expected payloads.'
    }
    $nestedPayloadPath = 'SkiaSharp.Test.1.0.0.nupkg!/tools/payload.nupkg!/lib/net10.0/HarfBuzzSharp.Subset.dll'
    if (-not ($inventory.Path -contains $nestedPayloadPath)) {
        throw 'Recursive package inventory did not include the nested NuGet payload.'
    }
    $policyPaths = @($policy.Files | ForEach-Object { $_.Paths } | ForEach-Object { $_ })
    if (-not ($policyPaths -contains $nestedPayloadPath)) {
        throw 'Signing policy did not classify the nested NuGet payload.'
    }

    & $verifier `
        -OriginalDirectory $unsigned `
        -SignedDirectory $signed `
        -SigningPropsPath $signingProps `
        -RequireSignatures

    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed -KeepMacUnsigned
    & $verifier `
        -OriginalDirectory $unsigned `
        -SignedDirectory $signed `
        -SigningPropsPath $signingProps `
        -SignType test `
        -RequireSignatures

    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed -Tampered
    $tamperDetected = $false
    try {
        & $verifier `
            -OriginalDirectory $unsigned `
            -SignedDirectory $signed `
            -SigningPropsPath $signingProps `
            -RequireSignatures
    } catch {
        $tamperDetected = $true
    }
    if (-not $tamperDetected) {
        throw 'Payload verification did not detect a modified unsigned entry.'
    }

    New-TestPackage (Join-Path $signed 'SkiaSharp.Test.1.0.0.nupkg') -Signed -TamperSkippedJs
    $skippedJsTamperDetected = $false
    try {
        & $verifier `
            -OriginalDirectory $unsigned `
            -SignedDirectory $signed `
            -SigningPropsPath $signingProps `
            -RequireSignatures
    } catch {
        $skippedJsTamperDetected = $true
    }
    if (-not $skippedJsTamperDetected) {
        throw 'Payload verification did not detect a modified skipped JavaScript entry.'
    }

    $unknownInventory = @($inventory) + [pscustomobject]@{
        RootPackage = 'SkiaSharp.Test.1.0.0.nupkg'
        Path = 'SkiaSharp.Test.1.0.0.nupkg!/lib/Unknown.dll'
        Name = 'Unknown.dll'
        Extension = '.dll'
        Length = 1
        Sha256 = '00'
        IsContainer = $false
        IsSignatureMetadata = $false
    }
    $unclassifiedDetected = $false
    try {
        Get-ArcadeSigningPolicy $unknownInventory $signingProps | Out-Null
    } catch {
        $unclassifiedDetected = $true
    }
    if (-not $unclassifiedDetected) {
        throw 'Signing policy did not reject an unclassified DLL.'
    }

    $repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
    [xml]$productionPolicy = Get-Content (Join-Path $repoRoot 'eng/Signing.props') -Raw
    $skippedFiles = @(
        $productionPolicy.Project.ItemGroup.SkippedFile |
            ForEach-Object { [string]$_.Include } |
            Sort-Object
    )
    $signCheckVerifiableSkippedFiles = @(
        $skippedFiles |
            Where-Object { [IO.Path]::GetExtension($_) -eq '.js' }
    )
    $expectedExclusions = @{
        'DpiWatcher.js' = @{
            Path = 'staticwebassets/DpiWatcher.js'
            Parent = 'SkiaSharp.Views.Blazor.'
        }
        'SKHtmlCanvas.js' = @{
            Path = 'staticwebassets/SKHtmlCanvas.js'
            Parent = 'SkiaSharp.Views.Blazor.'
        }
        'SizeWatcher.js' = @{
            Path = 'staticwebassets/SizeWatcher.js'
            Parent = 'SkiaSharp.Views.Blazor.'
        }
        'SkiaSharpInterop.js' = @{
            Path = 'build/SkiaSharpInterop.js'
            Parent = 'SkiaSharp.Views.Blazor.'
        }
        'library_webgpu.js' = @{
            Path = 'buildTransitive/netstandard1.0/emdawnwebgpu_pkg/webgpu/src/library_webgpu.js'
            Parent = 'SkiaSharp.NativeAssets.WebAssembly.'
        }
        'library_webgpu_enum_tables.js' = @{
            Path = 'buildTransitive/netstandard1.0/emdawnwebgpu_pkg/webgpu/src/library_webgpu_enum_tables.js'
            Parent = 'SkiaSharp.NativeAssets.WebAssembly.'
        }
        'library_webgpu_generated_sig_info.js' = @{
            Path = 'buildTransitive/netstandard1.0/emdawnwebgpu_pkg/webgpu/src/library_webgpu_generated_sig_info.js'
            Parent = 'SkiaSharp.NativeAssets.WebAssembly.'
        }
        'library_webgpu_generated_struct_info.js' = @{
            Path = 'buildTransitive/netstandard1.0/emdawnwebgpu_pkg/webgpu/src/library_webgpu_generated_struct_info.js'
            Parent = 'SkiaSharp.NativeAssets.WebAssembly.'
        }
        'webgpu-externs.js' = @{
            Path = 'buildTransitive/netstandard1.0/emdawnwebgpu_pkg/webgpu/src/webgpu-externs.js'
            Parent = 'SkiaSharp.NativeAssets.WebAssembly.'
        }
    }
    $signCheckExclusions = @{}
    Get-Content (Join-Path $repoRoot 'eng/SignCheckExclusionsFile.txt') |
        Where-Object {
            -not [string]::IsNullOrWhiteSpace($_) -and
            -not $_.StartsWith(';;', [StringComparison]::Ordinal)
        } |
        ForEach-Object {
            $parts = $_.Split(';')
            if ($parts.Count -ne 3 -or
                -not $parts[2].Contains('DO-NOT-SIGN')) {
                throw "Invalid SignCheck exclusion: $_"
            }
            $name = $parts[0].Split('/')[-1]
            $expected = $expectedExclusions[$name]
            if (-not $expected -or
                $expected.Path -cne $parts[0] -or
                $expected.Parent -cne $parts[1]) {
                throw "SignCheck exclusion is not scoped to the expected package path: $_"
            }
            $signCheckExclusions.Add($name, $_)
        }
    if (Compare-Object $signCheckVerifiableSkippedFiles @($signCheckExclusions.Keys | Sort-Object) -CaseSensitive) {
        throw 'SignCheck-verifiable SkippedFile entries and DO-NOT-SIGN entries differ.'
    }
    $noSignJs = @($productionPolicy.Project.PropertyGroup.NoSignJS)
    if ($noSignJs.Count -ne 1 -or [string]$noSignJs[0] -cne 'true') {
        throw 'Signing.props must explicitly enable the Arcade NoSignJS policy.'
    }

    [xml]$publishingPolicy = Get-Content (Join-Path $repoRoot 'eng/Publishing.props') -Raw
    $publishedArtifacts = @($publishingPolicy.Project.ItemGroup.Artifact)
    $previewArtifact = @(
        $publishedArtifacts |
            Where-Object { [string]$_.Include -ceq '$(ArtifactsPackagesDir)Preview\*.nupkg' })
    $releaseArtifact = @(
        $publishedArtifacts |
            Where-Object { [string]$_.Include -ceq '$(ArtifactsShippingPackagesDir)**\*.nupkg' })
    $transportArtifact = @(
        $publishedArtifacts |
            Where-Object { [string]$_.Include -ceq '$(ArtifactsNonShippingPackagesDir)**\*.nupkg' })
    if ($publishedArtifacts.Count -ne 3 -or
        $previewArtifact.Count -ne 1 -or
        $releaseArtifact.Count -ne 1 -or
        $transportArtifact.Count -ne 1 -or
        [string]$previewArtifact[0].Condition -notmatch "!= 'release'" -or
        [string]$releaseArtifact[0].Condition -notmatch "== 'release'" -or
        [string]$transportArtifact[0].Kind -cne 'Package' -or
        [string]$transportArtifact[0].IsShipping -cne 'false') {
        throw 'Publishing.props must separate shipping package views from non-shipping transport packages.'
    }
    if ([string]$publishingPolicy.Project.PropertyGroup.AutoGenerateSymbolPackages -cne 'false') {
        throw 'Arcade fallback symbol generation must be disabled in favor of the explicit shipping symbol inventory.'
    }
    $signingTemplate = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-signing.yml') -Raw
    if ($signingTemplate -notmatch '(?s)eng\\common\\build\.ps1\s+-configuration Release\s+-restore\s+-publish\s+-ci') {
        throw 'Arcade V3 manifest generation must restore its Publish.proj task dependencies.'
    }
    if ([regex]::Matches($signingTemplate, 'DotNetFinalVersionKind=\$\(DOTNET_FINAL_VERSION_KIND\)').Count -lt 2) {
        throw 'Arcade signing and manifest generation must share the derived final version kind.'
    }
    if ($signingTemplate -match 'artifactName:\s*nuget_symbols' -or
        $signingTemplate -match 'stage-android-symbol-packages\.ps1') {
        throw 'Arcade publishing must consume normal and symbol packages from the unified nuget artifact.'
    }
    if ($signingTemplate -notmatch 'artifactName:\s*nuget_special' -or
        $signingTemplate -notmatch 'stage-transport-packages\.ps1' -or
        $signingTemplate -notmatch 'stage-shipping-symbol-packages\.ps1' -or
        $signingTemplate -notmatch 'artifacts\\packages\\Release\\NonShipping' -or
        $signingTemplate -notmatch 'transport-payload-verification\.json' -or
        $signingTemplate -notmatch 'PackageBasePath=\$\(Build\.SourcesDirectory\)\\artifacts\\packages\\Release"') {
        throw 'Real-sign builds must sign and validate special packages in the Arcade non-shipping package view.'
    }
    $itemsToSign = @(
        $productionPolicy.SelectNodes('//ItemsToSign') |
            ForEach-Object { $_.GetAttribute('Include') })
    if (-not ($itemsToSign -contains '$(ArtifactsShippingPackagesDir)**\*.nupkg') -or
        -not ($itemsToSign -contains '$(ArtifactsNonShippingPackagesDir)**\*.nupkg')) {
        throw 'Arcade signing must include both shipping and non-shipping NuGet packages.'
    }

    Write-Host 'Signing policy and payload tests passed.'
} finally {
    if (Test-Path $testRoot) {
        Remove-Item $testRoot -Recurse -Force
    }
}
