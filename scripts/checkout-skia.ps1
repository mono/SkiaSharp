$ErrorActionPreference = 'Stop'

if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
    Write-Host "Not a PR build."
    exit 0
}

Write-Host "Fetching PR #$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER information..."

# Use Azure DevOps REST API to get PR information
# This uses System.AccessToken which is automatically available
try {
    # Get System.AccessToken from environment
    $accessToken = $env:SYSTEM_ACCESSTOKEN
    if (-not $accessToken) {
        Write-Host "##vso[task.logissue type=error]System.AccessToken is not available."
        Write-Host "##vso[task.logissue type=error]Make sure the pipeline has access to OAuth token (usually automatic)."
        Write-Host "##vso[task.complete result=Failed;]System.AccessToken not available."
        exit 1
    }
    
    # Create authorization header for Azure DevOps API
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$accessToken"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json"
    }
    
    # Get the repository ID from the source repository URI
    # Format: https://github.com/mono/SkiaSharp
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
    
    # Get the Azure Repos PR information which contains the GitHub PR data
    # When a GitHub PR triggers an Azure Pipeline, Azure DevOps stores the PR information
    $prUrl = "$($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$($env:SYSTEM_TEAMPROJECT)/_apis/git/repositories/$($env:BUILD_REPOSITORY_ID)/pullRequests?searchCriteria.status=all&api-version=7.0"
    Write-Host "Fetching PR information from Azure DevOps..."
    
    $prs = Invoke-RestMethod -Uri $prUrl -Method Get -Headers $headers
    
    # Find the PR that matches our PR number
    # For GitHub PRs in Azure Pipelines, the sourceRefName contains the PR number
    $prInfo = $null
    foreach ($pr in $prs.value) {
        # Check if this PR's description or title contains our GitHub PR number
        if ($pr.title -match "#$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" -or 
            $pr.description -match "#$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER") {
            $prInfo = $pr
            break
        }
    }
    
    # If we couldn't find it in Azure Repos, we need to get it from GitHub directly
    # Try unauthenticated request (may work if not rate-limited)
    if (-not $prInfo -or -not $prInfo.description) {
        Write-Host "Attempting to fetch PR description directly from GitHub..."
        try {
            $githubHeaders = @{
                "Accept" = "application/vnd.github.v3+json"
                "User-Agent" = "SkiaSharp-AzurePipelines"
            }
            
            $json = Invoke-RestMethod `
                -Uri "https://api.github.com/repos/$repoOwner/$repoName/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" `
                -Headers $githubHeaders `
                -Method Get `
                -ErrorAction Stop
                
            $prBody = $json.body
            $prTitle = $json.title
        } catch {
            Write-Host "##vso[task.logissue type=error]Failed to fetch PR information: $($_.Exception.Message)"
            Write-Host "##vso[task.logissue type=error]GitHub may be blocking unauthenticated requests."
            Write-Host "##vso[task.logissue type=error]This feature requires GitHub API access which is not currently available."
            Write-Host "##vso[task.complete result=Failed;]Failed to fetch PR information."
            exit 1
        }
    } else {
        $prBody = $prInfo.description
        $prTitle = $prInfo.title
    }
    
} catch {
    Write-Host "##vso[task.logissue type=error]Failed to fetch PR information: $($_.Exception.Message)"
    Write-Host "##vso[task.complete result=Failed;]Failed to fetch PR information."
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
