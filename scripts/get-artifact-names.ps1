Param (
    [Parameter(Mandatory, ValueFromPipeline)] [string] $ArtifactsJson
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

Write-Host "Final $($actualNames.Count) item[s]:"
$actualNames | ForEach-Object { Write-Host " - $_" }

Write-Host "Skipping $($skippedNames.Count) item[s]:"
$skippedNames | ForEach-Object { Write-Host " - $_" }
