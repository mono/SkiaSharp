Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [string]$Names = "",
    [string]$Targets = "",
    [string]$ForceBuild = "",
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf
)

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
$XC_EXE = Join-Path $TOOLS_DIR "xamarin-component.exe"

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent) {
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if (!(Test-Path $TOOLS_DIR)) {
    New-Item -ItemType directory -Path $TOOLS_DIR | Out-Null
}

# Make sure NuGet exists where we expect it.
if (!(Test-Path $NUGET_EXE)) {
    Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $NUGET_EXE
}

# Make sure NuGet exists where we expect it.
if (!(Test-Path $NUGET_EXE)) {
    Throw "Could not find NuGet.exe"
}

# Make sure xamarin-component exists where we expect it.
if (!(Test-Path $XC_EXE)) {
    Invoke-WebRequest -Uri https://components.xamarin.com/submit/xpkg -OutFile (Join-Path $TOOLS_DIR "xpkg.zip")    
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $TOOLS_DIR "xpkg.zip"), ($TOOLS_DIR))   
}

# Restore tools from NuGet.
Push-Location
Set-Location $TOOLS_DIR
Invoke-Expression "$NUGET_EXE install -ExcludeVersion -Source https://www.nuget.org/api/v2"
Pop-Location
if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

# Make sure that Cake has been installed.
if (!(Test-Path $CAKE_EXE)) {
    Throw "Could not find Cake.exe"
}

$buildTargets = ""
if ($Targets) {
    $buildTargets = "-targets=`"$Targets`""
}

$buildNames = ""
if ($Names) {
    $buildNames = "-names=`"$Names`""
}

$buildForce = "-forcebuild=false"
if ($ForceBuild -eq "True") {
    $buildForce = "-forcebuild=true"
}

# Start Cake
Invoke-Expression "$CAKE_EXE `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseDryRun $UseExperimental $buildForce $buildNames $buildTargets"
exit $LASTEXITCODE
