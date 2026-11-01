#!/usr/bin/env pwsh
# Runs the OneFuzz CLI on any platform. All arguments are passed to the CLI:
#
#   ./scripts/infra/security/fuzzing/onefuzz.ps1 utils status
#   ./scripts/infra/security/fuzzing/onefuzz.ps1 jobs get --job-id <guid>
#
# The first run downloads the CLI into externals/onefuzz-cli. Use the CLI's own
# `update` command to refresh it, or delete that folder to reinstall.
#
# Requires: PowerShell 7+, the .NET 10 runtime, and the Azure CLI signed in
# (`az login`) with access to the fuzzfest/Onefuzz Azure DevOps feed.

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$installDir = Join-Path $repoRoot 'externals/onefuzz-cli'
$cliDll = Join-Path $installDir 'onefuzz.dll'

$feedOrg = 'fuzzfest'
$feedProject = 'Onefuzz'
$feedName = 'onefuzz'
$packageName = 'onefuzz'
$azureDevOpsResource = '499b84ac-1321-427f-aa17-267ca6975798'

function Install-OneFuzzCli {
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        throw 'The Azure CLI (az) is required to download the OneFuzz CLI. Install it and run `az login`.'
    }

    $token = az account get-access-token --resource $azureDevOpsResource --query accessToken -o tsv
    if ($LASTEXITCODE -ne 0 -or -not $token) {
        throw 'Unable to get an Azure DevOps token. Run `az login` first.'
    }
    $headers = @{ Authorization = "Bearer $token" }

    # The CLI is published as both a universal package (needs an x64-only tool)
    # and a NuGet package, so use the NuGet one for a platform-neutral download.
    $listUrl = "https://feeds.dev.azure.com/$feedOrg/$feedProject/_apis/packaging/feeds/$feedName/packages" +
        "?protocolType=NuGet&packageNameQuery=$packageName&includeAllVersions=true&api-version=7.1-preview.1"
    $package = (Invoke-RestMethod -Uri $listUrl -Headers $headers).value |
        Where-Object { $_.name -eq $packageName } |
        Select-Object -First 1
    $version = ($package.versions | Where-Object { $_.isLatest } | Select-Object -First 1).version
    if (-not $version) {
        throw "Unable to find the latest '$packageName' version in the '$feedName' feed."
    }

    Write-Host "Downloading the OneFuzz CLI $version..." -ForegroundColor Cyan
    $contentUrl = "https://pkgs.dev.azure.com/$feedOrg/$feedProject/_apis/packaging/feeds/$feedName" +
        "/nuget/packages/$packageName/versions/$version/content?api-version=7.1-preview.1"
    $zip = Join-Path ([IO.Path]::GetTempPath()) "onefuzz-$version.zip"
    $staging = Join-Path ([IO.Path]::GetTempPath()) "onefuzz-$version"
    try {
        Invoke-WebRequest -Uri $contentUrl -Headers $headers -OutFile $zip
        if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
        Expand-Archive -Path $zip -DestinationPath $staging

        if (Test-Path $installDir) { Remove-Item $installDir -Recurse -Force }
        New-Item -ItemType Directory -Path (Split-Path $installDir) -Force | Out-Null
        Move-Item (Join-Path $staging 'tools') $installDir
    } finally {
        Remove-Item $zip, $staging -Recurse -Force -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path $cliDll -PathType Leaf)) {
    Install-OneFuzzCli
}

& dotnet $cliDll @args
exit $LASTEXITCODE
