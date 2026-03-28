#
# Copyright (c) Samsung Electronics. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

<#
.SYNOPSIS
Installs Tizen workload manifest.
.DESCRIPTION
Installs the WorkloadManifest.json and WorkloadManifest.targets files for Tizen to the dotnet sdk.
.PARAMETER Version
Use specific VERSION
.PARAMETER DotnetInstallDir
Dotnet SDK Location installed
#>

[cmdletbinding()]
param(
    [Alias('v')][string]$Version="<latest>",
    [Alias('d')][string]$DotnetInstallDir="<auto>",
    [Alias('t')][string]$DotnetTargetVersionBand="<auto>",
    [Alias('u')][switch]$UpdateAllWorkloads
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$ManifestBaseName = "Samsung.NET.Sdk.Tizen.Manifest"
$global:FallbackId = ""

$LatestVersionMap = [ordered]@{
    "$ManifestBaseName-6.0.100" = "7.0.101";
    "$ManifestBaseName-6.0.200" = "7.0.100-preview.13.6";
    "$ManifestBaseName-6.0.300" = "8.0.133";
    "$ManifestBaseName-6.0.400" = "9.0.104";
    "$ManifestBaseName-7.0.100-preview.6" = "7.0.100-preview.6.14";
    "$ManifestBaseName-7.0.100-preview.7" = "7.0.100-preview.7.20";
    "$ManifestBaseName-7.0.100-rc.1" = "7.0.100-rc.1.22";
    "$ManifestBaseName-7.0.100-rc.2" = "7.0.100-rc.2.24";
    "$ManifestBaseName-7.0.100" = "7.0.103";
    "$ManifestBaseName-7.0.200" = "7.0.105";
    "$ManifestBaseName-7.0.300" = "7.0.120";
    "$ManifestBaseName-7.0.400" = "10.0.106";
    "$ManifestBaseName-8.0.100-alpha.1" = "7.0.104";
    "$ManifestBaseName-8.0.100-preview.2" = "7.0.106";
    "$ManifestBaseName-8.0.100-preview.3" = "7.0.107";
    "$ManifestBaseName-8.0.100-preview.4" = "7.0.108";
    "$ManifestBaseName-8.0.100-preview.5" = "7.0.110";
    "$ManifestBaseName-8.0.100-preview.6" = "7.0.121";
    "$ManifestBaseName-8.0.100-preview.7" = "7.0.122";
    "$ManifestBaseName-8.0.100-rc.1" = "7.0.124";
    "$ManifestBaseName-8.0.100-rc.2" = "7.0.125";
    "$ManifestBaseName-8.0.100-rtm" = "7.0.127";
    "$ManifestBaseName-8.0.100" = "8.0.144";
    "$ManifestBaseName-8.0.200" = "8.0.157";
    "$ManifestBaseName-8.0.300" = "8.0.156";
    "$ManifestBaseName-8.0.400" = "10.0.109";
    "$ManifestBaseName-9.0.100-alpha.1" = "8.0.134";
    "$ManifestBaseName-9.0.100-preview.1" = "8.0.135";
    "$ManifestBaseName-9.0.100-preview.2" = "8.0.137";
    "$ManifestBaseName-9.0.100" = "10.0.104";
    "$ManifestBaseName-9.0.200" = "10.0.110";
    "$ManifestBaseName-9.0.300" = "10.0.111";
    "$ManifestBaseName-10.0.100" = "10.0.123";
}

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    $name = [System.IO.Path]::GetRandomFileName()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

function Ensure-Directory([string]$TestDir) {
    Try {
        New-Item -ItemType Directory -Path $TestDir -Force -ErrorAction stop | Out-Null
        [io.file]::OpenWrite($(Join-Path -Path $TestDir -ChildPath ".test-write-access")).Close()
        Remove-Item -Path $(Join-Path -Path $TestDir -ChildPath ".test-write-access") -Force
    }
    Catch [System.UnauthorizedAccessException] {
        Write-Error "No permission to install. Try run with administrator mode."
    }
}

function Get-LatestVersion([string]$Id) {
    $attempts=3
    $sleepInSeconds=3
    do
    {
        try
        {
            $Response = Invoke-WebRequest -Uri https://api.nuget.org/v3-flatcontainer/$($Id.ToLowerInvariant())/index.json -UseBasicParsing | ConvertFrom-Json
            return $Response.versions | Select-Object -Last 1
        }
        catch {
            Write-Host "Id: $Id"
            Write-Host "An exception was caught: $($_.Exception.Message)"
        }

        $attempts--
        if ($attempts -gt 0) { Start-Sleep $sleepInSeconds }
    } while ($attempts -gt 0)

    if ($LatestVersionMap.Contains($Id))
    {
        Write-Host "Return cached latest version."
        return $LatestVersionMap.$Id
    }
    else
    {
        $SubStringId = $Id.Substring(0, $ManifestBaseName.Length + 2);
        $MatchingFallbackId = @()
        $MatchingFallbackVersion = @()
        foreach ($key in $LatestVersionMap.Keys) {
            if ($key -like "$SubStringId*") {
                $MatchingFallbackId += $key
                $MatchingFallbackVersion += $LatestVersionMap[$key]
            }
        }
        if ($MatchingFallbackVersion)
        {
            $global:FallbackId = $MatchingFallbackId[-1]
            $FallbackVersion = $MatchingFallbackVersion[-1]
            Write-Host "Return fallback version: $FallbackVersion"
            return $FallbackVersion
        }
    }

    Write-Error "Wrong Id: $Id"
}

function Get-Package([string]$Id, [string]$Version, [string]$Destination, [string]$FileExt = "nupkg") {
    $OutFileName = "$Id.$Version.$FileExt"
    $OutFilePath = Join-Path -Path $Destination -ChildPath $OutFileName

    if ($Id -match ".net[0-9]+$") {
        $Id = $Id -replace (".net[0-9]+", "")
    }

    Invoke-WebRequest -Uri "https://www.nuget.org/api/v2/package/$Id/$Version" -OutFile $OutFilePath

    return $OutFilePath
}

function Install-Pack([string]$Id, [string]$Version, [string]$Kind) {
    $TempZipFile = $(Get-Package -Id $Id -Version $Version -Destination $TempDir -FileExt "zip")
    $TempUnzipDir = Join-Path -Path $TempDir -ChildPath "unzipped\$Id"

    switch ($Kind) {
        "manifest" {
            Expand-Archive -Path $TempZipFile -DestinationPath $TempUnzipDir
            New-Item -Path $TizenManifestDir -ItemType "directory" -Force | Out-Null
            Copy-Item -Path "$TempUnzipDir\data\*" -Destination $TizenManifestDir -Force
        }
        {($_ -eq "sdk") -or ($_ -eq "framework")} {
            Expand-Archive -Path $TempZipFile -DestinationPath $TempUnzipDir
            if ( ($kind -eq "sdk") -and ($Id -match ".net[0-9]+$")) {
                $Id = $Id -replace (".net[0-9]+", "")
            }
            $TargetDirectory = $(Join-Path -Path $DotnetInstallDir -ChildPath "packs\$Id\$Version")
            New-Item -Path $TargetDirectory -ItemType "directory" -Force | Out-Null
            Copy-Item -Path "$TempUnzipDir/*" -Destination $TargetDirectory -Recurse -Force
        }
        "template" {
            $TargetFileName = "$Id.$Version.nupkg".ToLower()
            $TargetDirectory = $(Join-Path -Path $DotnetInstallDir -ChildPath "template-packs")
            New-Item -Path $TargetDirectory -ItemType "directory" -Force | Out-Null
            Copy-Item $TempZipFile -Destination $(Join-Path -Path $TargetDirectory -ChildPath "$TargetFileName") -Force
        }
    }
}

function Remove-Pack([string]$Id, [string]$Version, [string]$Kind) {
    switch ($Kind) {
        "manifest" {
            Remove-Item -Path $TizenManifestDir -Recurse -Force
        }
        {($_ -eq "sdk") -or ($_ -eq "framework")} {
            $TargetDirectory = $(Join-Path -Path $DotnetInstallDir -ChildPath "packs\$Id\$Version")
            Remove-Item -Path $TargetDirectory -Recurse -Force
        }
        "template" {
            $TargetFileName = "$Id.$Version.nupkg".ToLower();
            Remove-Item -Path $(Join-Path -Path $DotnetInstallDir -ChildPath "template-packs\$TargetFileName") -Force
        }
    }
}

function Install-TizenWorkload([string]$DotnetVersion)
{
    $VersionSplitSymbol = '.'
    $SplitVersion = $DotnetVersion.Split($VersionSplitSymbol)

    $CurrentDotnetVersion = [Version]"$($SplitVersion[0]).$($SplitVersion[1])"
    $DotnetVersionBand = $SplitVersion[0] + $VersionSplitSymbol + $SplitVersion[1] + $VersionSplitSymbol + $SplitVersion[2][0] + "00"
    $ManifestName = "$ManifestBaseName-$DotnetVersionBand"

    if ($DotnetTargetVersionBand -eq "<auto>" -or $UpdateAllWorkloads.IsPresent) {
        if ($CurrentDotnetVersion -ge "7.0")
        {
            $IsPreviewVersion = $DotnetVersion.Contains("-preview") -or $DotnetVersion.Contains("-rc") -or $DotnetVersion.Contains("-alpha")
            if ($IsPreviewVersion -and ($SplitVersion.Count -ge 4)) {
                $DotnetTargetVersionBand = $DotnetVersionBand + $SplitVersion[2].SubString(3) + $VersionSplitSymbol + $($SplitVersion[3])
                $ManifestName = "$ManifestBaseName-$DotnetTargetVersionBand"
            }
            elseif ($DotnetVersion.Contains("-rtm") -and ($SplitVersion.Count -ge 3)) {
                $DotnetTargetVersionBand = $DotnetVersionBand + $SplitVersion[2].SubString(3)
                $ManifestName = "$ManifestBaseName-$DotnetTargetVersionBand"
            }
            else {
                $DotnetTargetVersionBand = $DotnetVersionBand
            }
        }
        else {
            $DotnetTargetVersionBand = $DotnetVersionBand
        }
    }

    # Check latest version of manifest.
    if ($Version -eq "<latest>" -or $UpdateAllWorkloads.IsPresent) {
        $Version = Get-LatestVersion -Id $ManifestName
    }

    # Check workload manifest directory.
    $ManifestDir = Join-Path -Path $DotnetInstallDir -ChildPath "sdk-manifests" | Join-Path -ChildPath $DotnetTargetVersionBand
    $TizenManifestDir = Join-Path -Path $ManifestDir -ChildPath "samsung.net.sdk.tizen"
    $TizenManifestFile = Join-Path -Path $TizenManifestDir -ChildPath "WorkloadManifest.json"

    # Check and remove already installed old version.
    if (Test-Path $TizenManifestFile) {
        $ManifestJson = $(Get-Content $TizenManifestFile | ConvertFrom-Json)
        $OldVersion = $ManifestJson.version
        if ($OldVersion -eq $Version) {
            $DotnetWorkloadList = Invoke-Expression "& '$DotnetCommand' workload list | Select-String -Pattern '^tizen'"
            if ($DotnetWorkloadList)
            {
                Write-Host "Tizen Workload $Version version is already installed."
                Continue
            }
        }

        Ensure-Directory $ManifestDir
        Write-Host "Removing $ManifestName/$OldVersion from $ManifestDir..."
        Remove-Pack -Id $ManifestName -Version $OldVersion -Kind "manifest"
        $ManifestJson.packs.PSObject.Properties | ForEach-Object {
            Write-Host "Removing $($_.Name)/$($_.Value.version)..."
            Remove-Pack -Id $_.Name -Version $_.Value.version -Kind $_.Value.kind
        }
    }

    Ensure-Directory $ManifestDir
    $TempDir = $(New-TemporaryDirectory)

    # Install workload manifest.
    Write-Host "Installing $ManifestName/$Version to $ManifestDir..."
    if ($global:FallbackId) {
        Install-Pack -Id $global:FallbackId -Version $Version -Kind "manifest"
    } else {
        Install-Pack -Id $ManifestName -Version $Version -Kind "manifest"
    }

    # Download and install workload packs.
    $NewManifestJson = $(Get-Content $TizenManifestFile | ConvertFrom-Json)
    $NewManifestJson.packs.PSObject.Properties | ForEach-Object {
        Write-Host "Installing $($_.Name)/$($_.Value.version)..."
        Install-Pack -Id $_.Name -Version $_.Value.version -Kind $_.Value.kind
    }

    # Add tizen to the installed workload metadata.
    # Featured version band for metadata does NOT include any preview specifier.
    # https://github.com/dotnet/sdk/blob/main/documentation/general/workloads/user-local-workloads.md
    New-Item -Path $(Join-Path -Path $DotnetInstallDir -ChildPath "metadata\workloads\$DotnetVersionBand\InstalledWorkloads\tizen") -Force | Out-Null
    if (Test-Path $(Join-Path -Path $DotnetInstallDir -ChildPath "metadata\workloads\$DotnetVersionBand\InstallerType\msi")) {
        New-Item -Path "HKLM:\SOFTWARE\Microsoft\dotnet\InstalledWorkloads\Standalone\x64\$DotnetTargetVersionBand\tizen" -Force | Out-Null
    }

    # Clean up
    Remove-Item -Path $TempDir -Force -Recurse

    Write-Host "Done installing Tizen workload $Version"
}

# Check dotnet install directory.
if ($DotnetInstallDir -eq "<auto>") {
    if ($Env:DOTNET_ROOT -And $(Test-Path "$Env:DOTNET_ROOT")) {
        $DotnetInstallDir = $Env:DOTNET_ROOT
    } else {
        $DotnetInstallDir = Join-Path -Path $Env:Programfiles -ChildPath "dotnet"
    }
}
if (-Not $(Test-Path "$DotnetInstallDir")) {
    Write-Error "No installed dotnet '$DotnetInstallDir'."
}

# Check installed dotnet version
$DotnetCommand = "$DotnetInstallDir\dotnet"
if (Get-Command $DotnetCommand -ErrorAction SilentlyContinue)
{
    if ($UpdateAllWorkloads.IsPresent)
    {
        $InstalledDotnetSdks = Invoke-Expression "& '$DotnetCommand' --list-sdks | Select-String -Pattern '^6|^7|^8|^9|^10'" | ForEach-Object {$_ -replace (" \[.*","")}
    }
    else
    {
        $InstalledDotnetSdks = Invoke-Expression "& '$DotnetCommand' --version"
    }
}
else
{
    Write-Error "'$DotnetCommand' occurs an error."
}

if (-Not $InstalledDotnetSdks)
{
    Write-Host "`n.NET SDK version 6 or later is required to install Tizen Workload."
}
else
{
    foreach ($DotnetSdk in $InstalledDotnetSdks)
    {
        try {
            Write-Host "`nCheck Tizen Workload for sdk $DotnetSdk"
            Install-TizenWorkload -DotnetVersion $DotnetSdk
        }
        catch {
            Write-Host "Failed to install Tizen Workload for sdk $DotnetSdk"
            Write-Host "$_"
            Continue
        }
    }
}

Write-Host "`nDone"
