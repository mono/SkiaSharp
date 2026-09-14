$ErrorActionPreference = 'Stop'
$prepare = Join-Path $PSScriptRoot '../prepare-test-run.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) "release-testing-$([guid]::NewGuid().ToString('N'))"
$integration = Join-Path $root 'output/logs/testlogs/integration'
$sibling = Join-Path $root 'output/logs/testlogs/keep.txt'

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

try {
    New-Item -ItemType Directory -Force (Join-Path $integration 'nested') > $null
    Set-Content (Join-Path $integration 'old.txt') 'old'
    Set-Content (Join-Path $integration 'nested/old.txt') 'old'
    Set-Content $sibling 'keep'
    $global:ReleaseTestDotnetCalled = $false
    function global:dotnet {
        $global:ReleaseTestDotnetCalled = $true
        $global:LASTEXITCODE = 0
    }

    $result = (& $prepare -RepositoryRoot $root | Out-String) | ConvertFrom-Json
    Assert-True $global:ReleaseTestDotnetCalled 'dotnet tool restore was not invoked.'
    Assert-True ($result.outputDirectory -eq $integration) 'The output directory was not reported.'
    Assert-True (-not (Test-Path (Join-Path $integration 'old.txt'))) 'Old integration output was not removed.'
    Assert-True (Test-Path $sibling) 'The sibling output file was removed.'
}
finally {
    Remove-Item Function:\dotnet -ErrorAction SilentlyContinue
    Remove-Item $root -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Variable ReleaseTestDotnetCalled -Scope Global -ErrorAction SilentlyContinue
}
