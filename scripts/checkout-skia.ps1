$ErrorActionPreference = 'Stop'

if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
    Write-Host "Not a PR build."
    exit 0
}

Write-Host "Fetching PR #$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER information..."

# Try to get GitHub token from System.AccessToken which Azure Pipelines provides
# when the repository is connected through GitHub App integration
$githubToken = $env:SYSTEM_ACCESSTOKEN

if (-not $githubToken) {
    Write-Host "##vso[task.logissue type=error]System.AccessToken is not available."
    Write-Host "##vso[task.logissue type=error]This script requires authentication to access GitHub API."
    Write-Host "##vso[task.logissue type=error]"
    Write-Host "##vso[task.logissue type=error]Without GitHub API access, this feature cannot work because:"
    Write-Host "##vso[task.logissue type=error]1. GitHub blocks unauthenticated API requests"
    Write-Host "##vso[task.logissue type=error]2. We need the PR description to find the required skia PR"
    Write-Host "##vso[task.logissue type=error]"
    Write-Host "##vso[task.logissue type=error]To enable this feature, you would need to configure a GitHub token,"
    Write-Host "##vso[task.logissue type=error]but since that's not possible in this environment, this feature"
    Write-Host "##vso[task.logissue type=error]cannot be automatically enabled."
    Write-Host "##vso[task.logissue type=error]"
    Write-Host "##vso[task.logissue type=error]Manual workaround: Contributors must manually check out the required"
    Write-Host "##vso[task.logissue type=error]skia PR in their local environment before pushing changes."
    Write-Host "##vso[task.complete result=Failed;]Cannot access GitHub API without authentication."
    exit 1
}

# Get repository information
$repoUrl = $env:BUILD_REPOSITORY_URI
if ($repoUrl -match 'github\.com/([^/]+)/([^/]+?)(?:\.git)?$') {
    $repoOwner = $matches[1]
    $repoName = $matches[2]
    Write-Host "Repository: $repoOwner/$repoName"
} else {
    Write-Host "##vso[task.logissue type=error]Unable to parse repository information from BUILD_REPOSITORY_URI: $repoUrl"
    Write-Host "##vso[task.complete result=Failed;]Invalid repository URI."
    exit 1
}

# Attempt to fetch PR information from GitHub
# Using System.AccessToken - note that this may not work if the token doesn't have GitHub API access
Write-Host "Attempting to fetch PR description from GitHub API..."
try {
    $githubHeaders = @{
        "Accept" = "application/vnd.github.v3+json"
        "User-Agent" = "SkiaSharp-AzurePipelines"
        "Authorization" = "Bearer $githubToken"
    }
    
    $json = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/$repoOwner/$repoName/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" `
        -Headers $githubHeaders `
        -Method Get `
        -ErrorAction Stop
        
    $prBody = $json.body
    $prTitle = $json.title
    
    Write-Host "Successfully fetched PR information from GitHub."
} catch {
    $errorMessage = $_.Exception.Message
    $statusCode = $_.Exception.Response.StatusCode.value__
    
    Write-Host "##vso[task.logissue type=error]Failed to fetch PR information from GitHub API: $errorMessage"
    Write-Host "##vso[task.logissue type=error]HTTP Status Code: $statusCode"
    Write-Host "##vso[task.logissue type=error]"
    Write-Host "##vso[task.logissue type=error]This likely means:"
    Write-Host "##vso[task.logissue type=error]- System.AccessToken does not grant access to GitHub API (it's for Azure DevOps)"
    Write-Host "##vso[task.logissue type=error]- GitHub requires proper authentication which is not available"
    Write-Host "##vso[task.logissue type=error]"
    Write-Host "##vso[task.logissue type=error]This feature cannot be automatically enabled without a way to access GitHub API."
    Write-Host "##vso[task.logissue type=error]Manual workaround: Contributors must manually check out required skia PRs locally."
    Write-Host "##vso[task.complete result=Failed;]Cannot access GitHub API."
    exit 1
}

Write-Host "PR Title: $prTitle"
Write-Host "Searching for required skia PR in the description..."

# Look for the pattern: **Required skia PR** followed by a GitHub PR URL
$regex = '\*\*Required\ skia\ PR\*\*[\\rn\s-]+https?\://github\.com/mono/skia/pull/(\d+)'

$match = [regex]::Match($prBody, $regex, [System.Text.RegularExpressions.RegexOptions]::Singleline)

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
    Pop-Location
    Write-Host "##vso[task.logissue type=error]Failed to checkout skia PR #${skiaPR}: $($_.Exception.Message)"
    Write-Host "##vso[task.complete result=Failed;]Failed to checkout required skia PR."
    exit 1
} finally {
    Pop-Location
}

Write-Host "Requesting full build with custom skia PR."
Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]required"
