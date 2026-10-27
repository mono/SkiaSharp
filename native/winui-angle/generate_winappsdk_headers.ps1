<#
.SYNOPSIS
    Generates Windows App SDK C++ headers from WINMD/IDL files.

.DESCRIPTION
    Downloads processing is handled by the caller (build.cake). This script
    takes the extracted NuGet root and runs winmdidl + midlrt to produce
    the C++ headers ANGLE needs.

    Must be run in a vcvarsall environment so midlrt can find cl.exe.

.PARAMETER Path
    Root of the merged Microsoft.WindowsAppSDK and Microsoft.Web.WebView2 metadata.
#>
param(
    [Parameter(Mandatory)]
    [string] $Path,

    [Parameter(Mandatory)]
    [string] $WindowsSdkVersion
)

$ErrorActionPreference = 'Stop'

$includePath = Join-Path $Path 'include'
$libPath = Join-Path $Path 'lib'
$uapVersion = '10.0.18362'

$winSdkDir = $env:WindowsSdkDir
if (!$winSdkDir) { throw 'vcvarsall did not set WindowsSdkDir.' }
if (!$env:WindowsSDKVersion) { throw 'vcvarsall did not set WindowsSDKVersion.' }

$sdkVersion = $WindowsSdkVersion.TrimEnd('\')
$selectedSdkVersion = $env:WindowsSDKVersion.TrimEnd('\')
if ($selectedSdkVersion -ne $sdkVersion) {
    throw "vcvarsall selected Windows SDK $selectedSdkVersion, expected $sdkVersion."
}

$winSdkBin = Join-Path $winSdkDir "bin\$sdkVersion\x64"
$winmdidl = Join-Path $winSdkBin 'winmdidl.exe'
$midlrt = Join-Path $winSdkBin 'midlrt.exe'
foreach ($tool in $winmdidl, $midlrt) {
    if (!(Test-Path $tool -PathType Leaf)) { throw "Missing Windows SDK tool: $tool" }
}

Write-Host "Using Windows SDK: $sdkVersion"
Write-Host "WinAppSDK path: $Path"

Push-Location $includePath
try {
    $packageWinmdFiles = @(Get-ChildItem $libPath -Filter '*.winmd' -Recurse)
    $dependencyMetadataDirs = $packageWinmdFiles | ForEach-Object { $_.DirectoryName }
    $metadataDirs = @(
        'C:\Windows\System32\WinMetadata'
        (Join-Path $libPath "uap$uapVersion")
        (Join-Path $libPath 'uap10.0')
    ) + $dependencyMetadataDirs
    $metadataDirs = $metadataDirs | Select-Object -Unique
    foreach ($directory in $metadataDirs) {
        if (!(Test-Path $directory -PathType Container)) { throw "Missing metadata directory: $directory" }
    }

    $winmdFiles = $packageWinmdFiles
    if (!$winmdFiles.Count) { throw "No WINMD files found under $libPath" }

    $abiIdlPath = Join-Path $includePath 'winmdidl'
    if (Test-Path $abiIdlPath) {
        Remove-Item $abiIdlPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $abiIdlPath -Force | Out-Null

    foreach ($winmd in $winmdFiles) {
        Write-Host "  winmdidl: $($winmd.FullName)"
        $metadataArgs = $metadataDirs | ForEach-Object { "/metadata_dir:$_" }
        & $winmdidl $winmd.FullName @metadataArgs "/outdir:$abiIdlPath" /nologo
        if ($LASTEXITCODE -ne 0) { throw "winmdidl failed for $($winmd.Name)" }
    }
    $abiIdls = @(Get-ChildItem $abiIdlPath -Filter '*.idl')
    if (!$abiIdls.Count) { throw "winmdidl produced no ABI IDLs under $abiIdlPath" }
    $abiIdls | Copy-Item -Destination $includePath -Force

    $idlFiles = Get-ChildItem $includePath -Filter '*.idl' |
        Where-Object { $_.Name -like 'Microsoft.UI*.idl' -or $_.Name -like 'Microsoft.Graphics*.idl' } |
        Select-Object -ExpandProperty Name
    if (!$idlFiles.Count) { throw "No ANGLE Windows App SDK IDLs found under $includePath" }

    $requiredIdls = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $pendingIdls = [System.Collections.Generic.Queue[string]]::new()
    foreach ($idl in $idlFiles) { $pendingIdls.Enqueue($idl) }
    while ($pendingIdls.Count -gt 0) {
        $idl = $pendingIdls.Dequeue()
        if (!$requiredIdls.Add($idl)) { continue }
        $contents = Get-Content (Join-Path $includePath $idl) -Raw
        foreach ($import in [regex]::Matches($contents, '(?is)\bimport\s+([^;]+);')) {
            foreach ($name in [regex]::Matches($import.Groups[1].Value, '"([^"]+\.idl)"')) {
                $dependency = $name.Groups[1].Value
                if (Test-Path (Join-Path $includePath $dependency)) {
                    $pendingIdls.Enqueue($dependency)
                } elseif ($dependency.StartsWith('Microsoft.', [StringComparison]::OrdinalIgnoreCase)) {
                    throw "Missing local IDL import '$dependency' required by $idl"
                }
            }
        }
    }

    foreach ($idl in ($requiredIdls | Sort-Object)) {
        $noExt = [System.IO.Path]::GetFileNameWithoutExtension($idl)
        Write-Host "  midlrt: $idl"
        & $midlrt $idl /metadata_dir C:\Windows\System32\WinMetadata /ns_prefix /nomidl /nologo
        if ($LASTEXITCODE -ne 0) { throw "midlrt failed for $idl (exit code $LASTEXITCODE)." }
    }

    foreach ($header in @('Microsoft.Foundation.h', 'Microsoft.UI.Dispatching.h', 'Microsoft.UI.Xaml.h')) {
        if (!(Test-Path (Join-Path $includePath $header))) { throw "Missing required ANGLE header: $header" }
    }
} finally {
    Pop-Location
}

Write-Host "Windows App SDK header generation complete."
