param (
    [string] $ExternalsBuildId = ''
)

$ErrorActionPreference = 'Stop'

# some stage earlier requested a full build
if ($env:DOWNLOAD_EXTERNALS -eq 'required') {
    Write-Host "Full build explicitly requested."
    Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]"
    exit 0
}

# this was explicit, so just us that
$intBuildId = "$ExternalsBuildId" -as [int]
if ($intBuildId -gt 0) {
    Write-Host "Explicit managed-only build using $intBuildId."
    Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]$intBuildId"
    exit 0
}

# if this is a PR and we are requesting last-build artifacts
if (("$ExternalsBuildId" -eq 'latest') -and ("$env:BUILD_REASON" -eq 'PullRequest')) {
    Write-Host "All changes:"
    $all = (git diff-tree --no-commit-id --name-only -r HEAD~ HEAD)
    foreach ($d in $all) {
        Write-Host " - $d"
    }

    Write-Host "Matching changes:"
    $matching = @(
        'cake',
        'externals',
        'native',
        'scripts',
        '.gitmodules',
        'VERSIONS.txt'
    )
    $requiresFull = (git diff-tree --no-commit-id --name-only -r HEAD~ HEAD @matching)
    foreach ($d in $requiresFull) {
        Write-Host " - $d"
    }

    if (-not $requiresFull) {
        Write-Host "Download-only build."
        Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]latest"
        exit 0
    }
}

# either not a PR, native files changed or explicit build-all
Write-Host "Full build."
Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]"
exit 0
