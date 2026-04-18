<#
.SYNOPSIS
Determines whether a pipeline job should build externals or download previous artifacts.

.DESCRIPTION
Sets the Azure DevOps DOWNLOAD_EXTERNALS variable used by downstream bootstrapper logic:
 - empty    => full external build
 - latest   => latest successful build from the relevant branch
 - <number> => explicit build ID to download

Supported values for -ExternalsBuildId:
 - ''       => default behavior (full build unless latest optimization applies)
 - <int>    => always use that specific build ID
 - latest   => on pull requests, attempt artifact reuse only when native-impacting files did not change
 - always   => always attempt artifact reuse from latest build on branch
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string] $ExternalsBuildId = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Set-DownloadExternalsVariable {
    param (
        [AllowEmptyString()]
        [string] $Value
    )

    Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]$Value"
}

function Get-ChangedFiles {
    param (
        [string[]] $Pathspec = @()
    )

    $arguments = @('diff-tree', '--no-commit-id', '--name-only', '-r', 'HEAD~', 'HEAD')
    if ($Pathspec.Count -gt 0) {
        $arguments += '--'
        $arguments += $Pathspec
    }

    $changes = & git @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($arguments -join ' ') failed with exit code $LASTEXITCODE."
    }

    return @($changes | Where-Object { $_ -and $_.Trim() })
}

$requestedBuild = "$ExternalsBuildId".Trim()

# Some earlier stage explicitly requested a full build.
if ($env:DOWNLOAD_EXTERNALS -eq 'required') {
    Write-Host "Full build explicitly requested."
    Set-DownloadExternalsVariable -Value ''
    exit 0
}

# Explicit build ID always wins.
$explicitBuildId = $requestedBuild -as [int]
if ($explicitBuildId -gt 0) {
    Write-Host "Explicit managed-only build using $explicitBuildId."
    Set-DownloadExternalsVariable -Value "$explicitBuildId"
    exit 0
}

$reuseMode = $requestedBuild.ToLowerInvariant()

# Always attempt to download the previous branch artifacts.
if ($reuseMode -eq 'always') {
    Write-Host "Forced artifact reuse enabled: always downloading latest artifacts from branch."
    Set-DownloadExternalsVariable -Value 'latest'
    exit 0
}

# For PRs, attempt reuse only when native-impacting files did not change.
if (($reuseMode -eq 'latest') -and ($env:BUILD_REASON -eq 'PullRequest')) {
    try {
        Write-Host "All changes:"
        $allChanges = @(Get-ChangedFiles)
        foreach ($change in $allChanges) {
            Write-Host " - $change"
        }

        Write-Host "Matching changes:"
        $nativeImpactingPaths = @(
            'externals',
            'native',
            'scripts',
            '.gitmodules'
        )
        $matchingChanges = @(Get-ChangedFiles -Pathspec $nativeImpactingPaths)
        foreach ($change in $matchingChanges) {
            Write-Host " - $change"
        }

        if ($matchingChanges.Count -eq 0) {
            Write-Host "Download-only build."
            Set-DownloadExternalsVariable -Value 'latest'
            exit 0
        }
    } catch {
        Write-Warning "Unable to evaluate changed files. Falling back to full build. $($_.Exception.Message)"
    }
}

# Not a PR, native files changed, unsupported value, or explicit full build request.
Write-Host "Full build."
Set-DownloadExternalsVariable -Value ''
exit 0
