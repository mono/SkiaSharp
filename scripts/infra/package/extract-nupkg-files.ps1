#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,
    [Parameter(Mandatory = $true)]
    [string]$DestinationPath,
    [switch]$RemoveOriginal
)

$ErrorActionPreference = 'Stop'

function Get-NuGetPackageInfo {
    param([string]$ExtractedPath)
    
    try {
        # Get the folder name as fallback (this is the package ID)
        $packageId = Split-Path $ExtractedPath -Leaf
        $isSymbols = $packageId.EndsWith(".symbols")
    
        # Try to find the .nuspec file to get the version
        $nuspecFile = Get-ChildItem -Path $ExtractedPath -Filter "*.nuspec" -Recurse | Select-Object -First 1
        
        # Get the package ID and version from the .nuspec
        [xml]$nuspec = Get-Content $nuspecFile.FullName
        $packageId = $nuspec.package.metadata.id
        $version = $nuspec.package.metadata.version
        
        # Return folder name based on whether version contains dash
        $folderName = if ($version.Contains('-')) {
            "$packageId-0.0.0-preview.0"
        } else {
            "$packageId-0.0.0"
        }
        
        # Append .symbols suffix for symbol packages
        if ($isSymbols) {
            $folderName += ".symbols"
        }
        
        return $folderName
    }
    catch {
        # If parsing fails, fall through
    }
    
    return $null
}

# Ensure the destination directory exists
if (-not (Test-Path $DestinationPath)) {
    New-Item -ItemType Directory -Path $DestinationPath -Force | Out-Null
    Write-Host "Created destination directory: $DestinationPath"
}

# Find all .nupkg files
$nupkgs = Get-ChildItem $SourcePath
if ($nupkgs.Count -eq 0) {
    Write-Host "No .nupkg files found in: $SourcePath"
    return
}
Write-Host "Found $($nupkgs.Count) .nupkg file(s) to extract"

# Extract each .nupkg file
foreach ($nupkg in $nupkgs) {
    $filename = $nupkg.Name.TrimEnd('.nupkg')
    Write-Host "Extracting '$nupkg' to default location: '$filename'..."
    
    # Extract to default location first
    $defaultDest = Join-Path $DestinationPath $filename
    Expand-Archive -Path $nupkg.FullName -DestinationPath $defaultDest -Force
        
    # Try to get package info from the nuspec to see if we should move
    $betterFolderName = Get-NuGetPackageInfo -ExtractedPath $defaultDest
    if ($betterFolderName -and $betterFolderName -ne $filename) {
        $finalDest = Join-Path $DestinationPath $betterFolderName
        Move-Item $defaultDest $finalDest
        Write-Host "Moved '$filename' to '$betterFolderName' based on nuspec info"
    }
    
    if ($RemoveOriginal) {
        Remove-Item $nupkg.FullName -Force
        Write-Host "Removed original file: $($nupkg.FullName)"
    }
}

Write-Host "Extraction completed successfully"
