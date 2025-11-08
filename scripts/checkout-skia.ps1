$ErrorActionPreference = 'Stop'

if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
    Write-Host "Not a PR build."
    exit 0
}

Write-Host "Fetching PR #$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER information from GitHub..."

# Make an unauthenticated request to the GitHub API
# GitHub allows 60 requests per hour for unauthenticated requests to public repositories
# This is sufficient for PR builds
try {
    $json = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/mono/SkiaSharp/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" `
        -Headers @{
            "Accept" = "application/vnd.github.v3+json"
            "User-Agent" = "SkiaSharp-AzurePipelines"
        } `
        -Method Get `
        -ErrorAction Stop
} catch {
    Write-Host "Failed to fetch PR information from GitHub API: $($_.Exception.Message)"
    Write-Host "This might be due to rate limiting or network issues."
    Write-Host "Continuing without checking out a specific skia PR."
    exit 0
}

Write-Host "PR Title: $($json.title)"
Write-Host "Searching for required skia PR in the description..."

# Look for the pattern: **Required skia PR** followed by a GitHub PR URL
$regex = '\*\*Required\ skia\ PR\*\*[\\rn\s-]+https?\://github\.com/mono/skia/pull/(\d+)'

$match = [regex]::Match($json.body, $regex, [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ((-not $match.Success) -or ($match.Groups.Count -ne 2)) {
    Write-Host "No required skia PR specified in the PR description."
    Write-Host "If you need to build with a specific skia PR, please add it to the PR description using:"
    Write-Host "**Required skia PR**"
    Write-Host "https://github.com/mono/skia/pull/<PR_NUMBER>"
    exit 0
}

$skiaPR = $match.Groups[1].Value
Write-Host "Found required skia PR: #$skiaPR"
Write-Host "Checking out skia PR #$skiaPR..."

try {
    Push-Location externals/skia
    Write-Host "Fetching skia PR #$skiaPR from origin..."
    git fetch --force --tags --prune --prune-tags --progress --no-recurse-submodules origin "+refs/pull/$skiaPR/merge:refs/remotes/pull/$skiaPR/merge"
    
    Write-Host "Checking out skia PR #$skiaPR..."
    git checkout --progress --force "refs/remotes/pull/$skiaPR/merge"
    
    Write-Host "Successfully checked out skia PR #$skiaPR"
    $currentCommit = git rev-parse HEAD
    Write-Host "Current commit: $currentCommit"
} catch {
    Write-Host "Failed to checkout skia PR #${skiaPR}: $($_.Exception.Message)"
    Write-Host "Continuing with the default skia submodule version."
    Pop-Location
    exit 0
} finally {
    Pop-Location
}

Write-Host "Requesting full build with custom skia PR."
Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]required"
