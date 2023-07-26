Param(
    [string] $GitHubToken = ''
)

$ErrorActionPreference = 'Stop'

if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
    Write-Host "Not a PR build."
    exit 0
}

if ($GitHubToken) {
    $GitHubToken = "token $GitHubToken"
} else {
    $GitHubToken = ""
}

$json = curl `
    -H "Accept: application/vnd.github.v3+json" `
    -H "Authorization: $GitHubToken" `
    https://api.github.com/repos/mono/SkiaSharp/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER | ConvertFrom-Json

Write-Host "GitHub Response:"
Write-Host $json

$regex = '\*\*Required\ skia\ PR\*\*[\\rn\s-]+https?\://github\.com/mono/skia/pull/(\d+)'

$match = [regex]::Match($json.body, $regex, [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ((-not $match.Success) -or ($match.Groups.Count -ne 2)) {
    Write-Host "No required skia PR specified."
    exit 0
}

$skiaPR = $match.Groups[1].Value
Write-Host "Found required skia PR: $skiaPR"

try {
    Push-Location externals/skia
    git fetch --force --tags --prune --prune-tags --progress --no-recurse-submodules origin +refs/heads/*:refs/remotes/origin/* +refs/pull/$skiaPR/merge:refs/remotes/pull/$skiaPR/merge
    git checkout --progress --force refs/remotes/pull/$skiaPR/merge
} finally {
    Pop-Location
}

Write-Host "Requesting full build."
Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]required"
