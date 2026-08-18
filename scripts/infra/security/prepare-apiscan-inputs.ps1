[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ScanRoot,

    [string] $SurrogateSource = (Join-Path $PSScriptRoot 'APIScanSurrogates.in.xml')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function ConvertTo-LocalPath {
    param([Parameter(Mandatory)][string] $ConfiguredPath)

    return $ConfiguredPath.
        Replace('\', [IO.Path]::DirectorySeparatorChar).
        Replace('/', [IO.Path]::DirectorySeparatorChar)
}

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$extractScript = Join-Path $repoRoot 'scripts/infra/package/extract-nupkg-files.ps1'
$scanRootPath = [IO.Path]::GetFullPath($ScanRoot)
$surrogateSourcePath = (Resolve-Path $SurrogateSource).Path
$extractedPackages = Join-Path $scanRootPath 'nuget_symbols-extracted'
$surrogateFolder = Join-Path $scanRootPath 'surrogates'
$surrogateBinaryFolder = Join-Path $scanRootPath 'surrogate-binaries'
$surrogateFile = Join-Path $surrogateFolder 'APIScanSurrogates.xml'
$packagePattern = Join-Path $scanRootPath 'packages/*/*.*nupkg'

if (-not (Test-Path $extractScript -PathType Leaf)) {
    throw "Unable to find the NuGet extraction script: $extractScript"
}

$packages = @(Get-ChildItem $packagePattern -File)
if ($packages.Count -eq 0) {
    throw "No API Scan NuGet packages were found under '$scanRootPath'."
}

& $extractScript `
    -SourcePath $packagePattern `
    -DestinationPath $extractedPackages `
    -RemoveOriginal

New-Item -ItemType Directory -Force -Path $surrogateFolder, $surrogateBinaryFolder | Out-Null

$winUiSurrogateSource = Get-ChildItem $extractedPackages -Directory |
    Where-Object Name -Like 'skiasharp.nativeassets.winui-*.symbols' |
    ForEach-Object { Join-Path $_.FullName 'runtimes/win-x64/native' } |
    Where-Object { Test-Path $_ -PathType Container } |
    Select-Object -First 1
if (-not $winUiSurrogateSource) {
    throw 'Unable to find x64 WinUI binaries for API Scan ARM64 surrogates.'
}

$winUiSurrogateFiles = @(
    'libEGL.dll'
    'libEGL.pdb'
    'libGLESv2.dll'
    'libGLESv2.pdb'
    'SkiaSharp.Views.WinUI.Native.dll'
    'SkiaSharp.Views.WinUI.Native.pdb'
)
foreach ($file in $winUiSurrogateFiles) {
    $source = Join-Path $winUiSurrogateSource $file
    if (-not (Test-Path $source -PathType Leaf)) {
        throw "Unable to find API Scan surrogate file: $source"
    }
    Copy-Item $source $surrogateBinaryFolder
}

$surrogateXml = (Get-Content $surrogateSourcePath -Raw).
    Replace('{SOFTWARE_FOLDER}', $scanRootPath)
Set-Content -LiteralPath $surrogateFile -Value $surrogateXml -Encoding utf8

[xml] $surrogateConfiguration = $surrogateXml
$mappings = @($surrogateConfiguration.APIScanSurrogates.Mappings.Mapping)
if ($mappings.Count -eq 0) {
    throw "No API Scan surrogate mappings were found in '$surrogateSourcePath'."
}

$binaries = @(Get-ChildItem $scanRootPath -Recurse -File -Include *.dll, *.exe, *.winmd)
$symbols = @(Get-ChildItem $scanRootPath -Recurse -File -Filter *.pdb)
if ($binaries.Count -eq 0) {
    throw "No shipped DLL, EXE, or WINMD files were collected in '$scanRootPath'."
}
if ($symbols.Count -eq 0) {
    throw "No matching PDB files were collected in '$scanRootPath'."
}

$windowsBinaryPaths = @(
    $binaries | ForEach-Object { $_.FullName.Replace('/', '\') }
)
foreach ($mapping in $mappings) {
    foreach ($surrogate in @($mapping.SurrogateSet.BinarySet.Binary)) {
        $surrogatePath = ConvertTo-LocalPath $surrogate.path
        if (-not (Test-Path $surrogatePath -PathType Leaf)) {
            throw "Configured API Scan surrogate does not exist: $($surrogate.path)"
        }
    }

    foreach ($target in @($mapping.Targets.Binary)) {
        if ($target.pathType -eq 'Regex') {
            $matches = @(
                $windowsBinaryPaths |
                    Where-Object { [regex]::IsMatch($_, $target.path, 'IgnoreCase') }
            )
            if ($matches.Count -eq 0) {
                throw "API Scan surrogate target matched no shipped binary: $($target.path)"
            }
        } else {
            $targetPath = ConvertTo-LocalPath $target.path
            if (-not (Test-Path $targetPath -PathType Leaf)) {
                throw "Configured API Scan surrogate target does not exist: $($target.path)"
            }
        }
    }
}

Write-Host "Prepared $($mappings.Count) API Scan surrogate mapping(s), $($binaries.Count) binaries, and $($symbols.Count) PDB files."
