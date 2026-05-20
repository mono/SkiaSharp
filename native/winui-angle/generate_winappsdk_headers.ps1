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
    # Process WINMD files with winmdidl.exe
    $winmdFiles = @(
        @{ Winmd = "uap$uapVersion\Microsoft.Foundation.winmd"; Stamp = 'Microsoft.Foundation.idl' }
        @{ Winmd = "uap$uapVersion\Microsoft.Graphics.winmd"; Stamp = 'Microsoft.Graphics.DirectX.idl' }
        @{ Winmd = "uap$uapVersion\Microsoft.UI.winmd"; Stamp = 'Microsoft.UI.idl' }
        @{ Winmd = 'uap10.0\Microsoft.UI.Text.winmd'; Stamp = 'Microsoft.UI.Text.idl' }
        @{ Winmd = 'uap10.0\Microsoft.UI.Xaml.winmd'; Stamp = 'Microsoft.UI.Xaml.idl' }
        @{ Winmd = 'uap10.0\Microsoft.Web.WebView2.Core.winmd'; Stamp = 'Microsoft.Web.WebView2.Core.idl' }
    )

    foreach ($entry in $winmdFiles) {
        $stampFile = Join-Path $includePath $entry.Stamp
        if (Test-Path $stampFile) { continue }

        $winmdPath = Join-Path $libPath $entry.Winmd
        Write-Host "  winmdidl: $($entry.Winmd)"
        & $winmdidl $winmdPath `
            "/metadata_dir:C:\Windows\System32\WinMetadata" `
            "/metadata_dir:$(Join-Path $libPath "uap$uapVersion")" `
            "/metadata_dir:$(Join-Path $libPath 'uap10.0')" `
            "/outdir:$includePath" `
            /nologo
        if ($LASTEXITCODE -ne 0) { throw "winmdidl failed for $($entry.Winmd)" }
    }

    # Process IDL files with midlrt.exe
    $idlFiles = @(
        'Microsoft.Foundation.idl'
        'Microsoft.Graphics.DirectX.idl'
        'Microsoft.UI.Composition.idl'
        'Microsoft.UI.Composition.SystemBackdrops.idl'
        'Microsoft.UI.Dispatching.idl'
        'Microsoft.UI.idl'
        'Microsoft.UI.Input.idl'
        'Microsoft.UI.Text.idl'
        'Microsoft.UI.Windowing.idl'
        'Microsoft.UI.Xaml.Automation.idl'
        'Microsoft.UI.Xaml.Automation.Peers.idl'
        'Microsoft.UI.Xaml.Automation.Provider.idl'
        'Microsoft.UI.Xaml.Automation.Text.idl'
        'Microsoft.UI.Xaml.Controls.idl'
        'Microsoft.UI.Xaml.Controls.Primitives.idl'
        'Microsoft.UI.Xaml.Data.idl'
        'Microsoft.UI.Xaml.Documents.idl'
        'Microsoft.UI.Xaml.idl'
        'Microsoft.UI.Xaml.Input.idl'
        'Microsoft.UI.Xaml.Interop.idl'
        'Microsoft.UI.Xaml.Media.Animation.idl'
        'Microsoft.UI.Xaml.Media.idl'
        'Microsoft.UI.Xaml.Media.Imaging.idl'
        'Microsoft.UI.Xaml.Media.Media3D.idl'
        'Microsoft.UI.Xaml.Navigation.idl'
        'Microsoft.Web.WebView2.Core.idl'
    )

    foreach ($idl in $idlFiles) {
        $noExt = [System.IO.Path]::GetFileNameWithoutExtension($idl)
        $headerFile = Join-Path $includePath "$noExt.h"
        if (Test-Path $headerFile) { continue }

        Write-Host "  midlrt: $idl"
        & $midlrt $idl /metadata_dir C:\Windows\System32\WinMetadata /ns_prefix /nomidl /nologo
        if ($LASTEXITCODE -ne 0) { throw "midlrt failed for $idl" }
    }
} finally {
    Pop-Location
}

Write-Host "Windows App SDK header generation complete."
