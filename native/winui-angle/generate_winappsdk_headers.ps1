<#
.SYNOPSIS
    Generates Windows App SDK C++ headers from WINMD/IDL files.

.DESCRIPTION
    Downloads processing is handled by the caller (build.cake). This script
    takes the extracted NuGet root and runs winmdidl + midlrt to produce
    the C++ headers ANGLE needs.

    Must be run in a vcvarsall environment so midlrt can find cl.exe.

.PARAMETER Path
    Root of the extracted Microsoft.WindowsAppSDK NuGet package.
#>
param(
    [Parameter(Mandatory)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'

$includePath = Join-Path $Path 'include'
$libPath = Join-Path $Path 'lib'
$uapVersion = '10.0.18362'

# Ensure include directory exists
if (-not (Test-Path $includePath)) {
    New-Item -ItemType Directory -Path $includePath | Out-Null
}

# Find Windows SDK bin directory
$winSdkDir = $env:WindowsSdkDir
if (-not $winSdkDir) {
    $winSdkDir = 'C:\Program Files (x86)\Windows Kits\10'
}
$sdkVersion = Get-ChildItem (Join-Path $winSdkDir 'bin') -Directory -Filter '10.0.*' |
    Sort-Object Name -Descending |
    Select-Object -First 1
if (-not $sdkVersion) {
    throw "Could not find Windows SDK bin directory under $winSdkDir"
}
$winSdkBin = Join-Path $sdkVersion.FullName 'x64'
$winmdidl = Join-Path $winSdkBin 'winmdidl.exe'
$midlrt = Join-Path $winSdkBin 'midlrt.exe'

Write-Host "Using Windows SDK: $($sdkVersion.Name)"
Write-Host "WinAppSDK path: $Path"

# Both winmdidl and midlrt run with CWD set to the include directory
# (midlrt resolves imported IDL files relative to CWD)
Push-Location $includePath
try {
    # Process all WINMD files with winmdidl.exe
    # Discover all .winmd files under the lib directory so new namespaces in
    # newer WinAppSDK versions are handled automatically.
    $metadataDirs = @(
        'C:\Windows\System32\WinMetadata'
        (Join-Path $libPath "uap$uapVersion")
        (Join-Path $libPath 'uap10.0')
    )
    $winmdFiles = Get-ChildItem $libPath -Filter '*.winmd' -Recurse

    foreach ($winmd in $winmdFiles) {
        # Use the first IDL that would be produced as a stamp file
        $stampName = [System.IO.Path]::GetFileNameWithoutExtension($winmd.Name) + '.idl'
        $stampFile = Join-Path $includePath $stampName
        if (Test-Path $stampFile) { continue }

        Write-Host "  winmdidl: $($winmd.FullName)"
        $metadataArgs = $metadataDirs | ForEach-Object { "/metadata_dir:$_" }
        & $winmdidl $winmd.FullName @metadataArgs "/outdir:$includePath" /nologo
        if ($LASTEXITCODE -ne 0) { throw "winmdidl failed for $($winmd.Name)" }
    }

    # Include local transitive imports: MIDL reads imported IDLs but does not
    # generate their headers when processing the importing file.
    $idlFiles = Get-ChildItem $includePath -Filter '*.idl' |
        Where-Object { $_.Name -like 'Microsoft.UI*.idl' -or $_.Name -like 'Microsoft.Graphics*.idl' } |
        Select-Object -ExpandProperty Name
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
                }
            }
        }
    }

    foreach ($idl in ($requiredIdls | Sort-Object)) {
        $noExt = [System.IO.Path]::GetFileNameWithoutExtension($idl)
        $headerFile = Join-Path $includePath "$noExt.h"
        if (Test-Path $headerFile) { continue }

        Write-Host "  midlrt: $idl"
        & $midlrt $idl /metadata_dir C:\Windows\System32\WinMetadata /ns_prefix /nomidl /nologo
        if ($LASTEXITCODE -ne 0) {
            throw "midlrt failed for required ANGLE header $idl (exit code $LASTEXITCODE)."
        }
    }
    foreach ($header in @('Microsoft.Foundation.h', 'Microsoft.UI.Dispatching.h', 'Microsoft.UI.Xaml.h')) {
        if (!(Test-Path (Join-Path $includePath $header))) { throw "Missing required ANGLE header: $header" }
    }
} finally {
    Pop-Location
}

Write-Host "Windows App SDK header generation complete."
