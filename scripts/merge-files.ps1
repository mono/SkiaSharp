Param(
    [string] $Path
)

$ErrorActionPreference = 'Stop'

if ((!$Path) -or ($Path -eq "")) {
    Write-Error "Target filename missing."
    exit 1
}

$fileList = @($INPUT)
if ($fileList.Count -eq 0) {
    Write-Error "Source files missing."
    exit 1
}

$destFile = [System.IO.File]::Create($Path)

$fileList | ForEach-Object {
    Write-Host "Appending $_ to $Path..."
    $sourceFile = [System.IO.File]::OpenRead($_)
    $sourceFile.CopyTo($destFile)
    $sourceFile.Close()
}

$destFile.Close()

Write-Host "Merge complete."

exit $LASTEXITCODE
