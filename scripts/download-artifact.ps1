Param (
    [Parameter(Mandatory, ValueFromPipeline)] [string] $ArtifactsJson,
    [Parameter(Mandatory)] [string] $BuildId,
    [string] $OutputDirectory = "./output/"
)

Write-Host "Using JSON:"
Write-Host "============================================================"
Write-Host $ArtifactsJson
Write-Host "============================================================"
Write-Host ""

Write-Host "Looking for artifacts..."
$json = ConvertFrom-Json $ArtifactsJson
$objects = $json | Get-Member -MemberType NoteProperty
$names = $objects | ForEach-Object { $json."$($_.Name)".name }

Write-Host "Found $($names.Count) item[s]:"
$names | ForEach-Object { Write-Host " - $_" }

$actualNames = $names | Where-Object { $json."$_".result -ne "Skipped" }
$skippedNames = $names | Where-Object { $actualNames -notcontains $_ }

Write-Host "Skipping $($skippedNames.Count) item[s]:"
$skippedNames | ForEach-Object { Write-Host " - $_" }

Write-Host ""
Write-Host "Downloading artifacts..."
New-Item "$OutputDirectory" -Type Directory -Force | Out-Null
foreach ($name in $actualNames) {
    Write-Host "Downloading '$name'..."
    az pipelines runs artifact download --artifact-name "$name" --path "$OutputDirectory" --run-id "$BuildId" --verbose
}
Write-Host "Downloads complete."

Write-Host ""
Write-Host "Downloaded files:"
Get-ChildItem "$OutputDirectory"
