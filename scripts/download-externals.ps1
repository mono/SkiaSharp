param (
    [string] $BuildNumber,
    [string] $Artifact
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$url = "https://dev.azure.com/xamarin/public/_apis/build/builds/$BuildNumber/artifacts?artifactName=$Artifact&api-version=5.1&%24format=zip"
$outputTemp = "./output-temp"
$dest = "$outputTemp/$Artifact.zip"
$destTemp = "$outputTemp/$Artifact"
$output = "./output"

# dowload the artifact
New-Item -Type Directory -Path $outputTemp -Force | Out-Null
Invoke-WebRequest -Uri $url -OutFile $dest

# extract it
if (Test-Path -Path $destTemp) {
    Remove-Item -Path $destTemp/** -Force -Recurse
}
[System.IO.Compression.ZipFile]::ExtractToDirectory($dest, $outputTemp)

# create the output folder
New-Item -Type Directory -Path $output -Force | Out-Null

# move the items into the output folder
Get-ChildItem $destTemp | Copy-Item -Destination $output -Force -Recurse
