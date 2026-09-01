[CmdletBinding()]
param([string] $RepositoryRoot)

$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) {
    $RepositoryRoot = & git -C $PSScriptRoot rev-parse --show-toplevel
    if ($LASTEXITCODE -or -not $RepositoryRoot) { throw 'Could not resolve the repository root.' }
}

$root = [IO.Path]::GetFullPath($RepositoryRoot)
$output = Join-Path $root 'output/logs/testlogs/integration'
$expectedParent = [IO.Path]::GetFullPath((Join-Path $root 'output/logs/testlogs'))
if ([IO.Path]::GetDirectoryName($output) -ne $expectedParent) { throw "Unexpected output path: $output" }

Push-Location $root
try {
    dotnet tool restore
    if ($LASTEXITCODE) { throw "dotnet tool restore failed with exit code $LASTEXITCODE." }

    New-Item -ItemType Directory -Force $output > $null
    Get-ChildItem -Force $output | Remove-Item -Recurse -Force
    [pscustomobject]@{ toolsRestored = $true; outputDirectory = $output; outputReset = $true } | ConvertTo-Json
}
finally {
    Pop-Location
}
