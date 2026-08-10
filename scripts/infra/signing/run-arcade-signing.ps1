[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $InputDirectory,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [Parameter(Mandatory)]
    [string] $SignListPath,

    [Parameter(Mandatory)]
    [string] $WorkDirectory,

    [ValidateSet('dry-run', 'test', 'real')]
    [string] $SignType = 'dry-run',

    [string] $OfficialBuildId = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot 'SkiaSharp.Signing.psm1') -Force

function Invoke-DotNet {
    param([Parameter(ValueFromRemainingArguments)] [string[]] $Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Add-TrailingDirectorySeparator {
    param([Parameter(Mandatory)] [string] $Path)

    if ($Path.EndsWith([IO.Path]::DirectorySeparatorChar) -or
        $Path.EndsWith([IO.Path]::AltDirectorySeparatorChar)) {
        return $Path
    }

    return "$Path$([IO.Path]::DirectorySeparatorChar)"
}

$inputPath = (Resolve-Path $InputDirectory).Path
$signList = (Resolve-Path $SignListPath).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
$workPath = [IO.Path]::GetFullPath($WorkDirectory)
$packageRoot = Join-Path $workPath 'packages'
$artifactsDirectory = Join-Path $workPath 'artifacts'

if ($inputPath.Equals($outputPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'InputDirectory and OutputDirectory must be different so unsigned artifacts remain immutable.'
}
if ($SignType -ne 'dry-run' -and [string]::IsNullOrWhiteSpace($OfficialBuildId)) {
    throw "OfficialBuildId is required for $SignType signing."
}
if (Test-Path $outputPath) {
    $existingOutput = @(Get-ChildItem $outputPath -Force)
    if ($existingOutput.Count -ne 0) {
        throw "OutputDirectory '$outputPath' must be empty."
    }
} else {
    New-Item $outputPath -ItemType Directory -Force | Out-Null
}
if (Test-Path $workPath) {
    $existingWork = @(Get-ChildItem $workPath -Force)
    if ($existingWork.Count -ne 0) {
        throw "WorkDirectory '$workPath' must be empty."
    }
} else {
    New-Item $workPath -ItemType Directory -Force | Out-Null
}
New-Item (Join-Path $artifactsDirectory 'log/Release') -ItemType Directory -Force | Out-Null

$packages = @(Get-ChildItem $inputPath -Filter '*.nupkg' -File | Sort-Object Name)
if ($packages.Count -eq 0) {
    throw "No NuGet packages were found in '$inputPath'."
}

Copy-Item $packages.FullName $outputPath
Copy-Item $signList (Join-Path $outputPath 'SignList.xml')

$inventory = @(Get-SkiaSharpPackageInventory $outputPath)
$policy = Get-SkiaSharpSigningPolicy $inventory $signList
$engineeringDirectory = Join-Path $workPath 'eng'
$signingProps = Join-Path $engineeringDirectory 'Signing.props'
$manifest = Join-Path $outputPath 'signing-manifest.json'

Write-SkiaSharpArcadeSigningProps $policy $signingProps
Copy-Item (Join-Path $PSScriptRoot 'Tools.props') (Join-Path $engineeringDirectory 'Tools.props')
Copy-Item (Join-Path $PSScriptRoot 'common') (Join-Path $engineeringDirectory 'common') -Recurse
Write-SkiaSharpSigningManifest $outputPath $signList $inventory $policy $manifest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$toolsetLocationFile = Join-Path $workPath 'toolset-location.txt'
$bootstrapProject = Join-Path $PSScriptRoot 'Bootstrap.proj'
$noBuildProject = Join-Path $PSScriptRoot 'NoBuild.proj'
$nugetConfig = Join-Path $repoRoot 'NuGet.config'
New-Item $packageRoot -ItemType Directory -Force | Out-Null
New-Item $packageRoot -ItemType Directory -Force | Out-Null

Push-Location $PSScriptRoot
try {
    Invoke-DotNet msbuild $bootstrapProject `
        /t:__WriteToolsetLocation `
        "/p:__ToolsetLocationOutputFile=$toolsetLocationFile" `
        /p:RestoreIgnoreFailedSources=false `
        /nr:false

    $toolsetBuildProject = (Get-Content $toolsetLocationFile -TotalCount 1).Trim()
    if (-not (Test-Path $toolsetBuildProject -PathType Leaf)) {
        throw "Arcade returned an invalid Build.proj path '$toolsetBuildProject'."
    }

    $effectiveSignType = if ($SignType -eq 'real') { 'real' } else { 'test' }
    $arguments = @(
        'msbuild',
        $toolsetBuildProject,
        "/p:Projects=$noBuildProject",
        "/p:RepoRoot=$(Add-TrailingDirectorySeparator $repoRoot)",
        "/p:RepositoryEngineeringDir=$(Add-TrailingDirectorySeparator $engineeringDirectory)",
        "/p:ArtifactsDir=$(Add-TrailingDirectorySeparator $artifactsDirectory)",
        "/p:NuGetPackageRoot=$(Add-TrailingDirectorySeparator $packageRoot)",
        "/p:RestorePackagesPath=$packageRoot",
        "/p:RestoreConfigFile=$nugetConfig",
        "/p:SigningPackageDirectory=$outputPath",
        '/p:Configuration=Release',
        '/p:ContinuousIntegrationBuild=true',
        '/p:Restore=true',
        '/p:Build=false',
        '/p:Pack=false',
        '/p:Publish=false',
        '/p:Sign=true',
        "/p:DotNetSignType=$effectiveSignType",
        '/nr:false'
    )
    if ($SignType -ne 'dry-run') {
        $arguments += "/p:OfficialBuildId=$OfficialBuildId"
    }

    Invoke-DotNet @arguments
} finally {
    Pop-Location
}

Write-Host "Arcade $SignType signing completed for $($packages.Count) copied NuGet package(s)."
