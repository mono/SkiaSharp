Param(
    [string] $GitHubServiceConnection = '',
    [string] $SystemAccessToken = ''
)

$ErrorActionPreference = 'Stop'

if (-not $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER) {
    Write-Host "Not a PR build."
    exit 0
}

Write-Host "Fetching PR #$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER information from GitHub..."

# Get GitHub token from Azure DevOps service endpoint
if (-not $GitHubServiceConnection -or $GitHubServiceConnection -eq '$(GITHUB_SERVICE_CONNECTION)') {
    Write-Host "##vso[task.logissue type=error]GITHUB_SERVICE_CONNECTION variable is not configured in Azure Pipelines."
    Write-Host "##vso[task.logissue type=error]Please add a pipeline variable named GITHUB_SERVICE_CONNECTION with the name of your GitHub service connection."
    Write-Host "##vso[task.complete result=Failed;]GitHub service connection not configured."
    exit 1
}

if (-not $SystemAccessToken) {
    Write-Host "##vso[task.logissue type=error]System.AccessToken is not available."
    Write-Host "##vso[task.complete result=Failed;]System.AccessToken not available."
    exit 1
}

$gitHubToken = ""
if ($GitHubServiceConnection -and $SystemAccessToken) {
    try {
        Write-Host "Retrieving GitHub token from service connection '$GitHubServiceConnection'..."
        
        # Create authorization header for Azure DevOps API
        $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$SystemAccessToken"))
        $headers = @{
            Authorization = "Basic $base64AuthInfo"
        }
        
        # Get the service endpoint ID
        $serviceEndpointUrl = "$($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$($env:SYSTEM_TEAMPROJECT)/_apis/serviceendpoint/endpoints?endpointNames=$GitHubServiceConnection&api-version=7.1-preview.4"
        Write-Host "Fetching service endpoint from: $serviceEndpointUrl"
        
        $endpoints = Invoke-RestMethod -Uri $serviceEndpointUrl -Method Get -Headers $headers -ContentType "application/json"
        
        if ($endpoints.value -and $endpoints.value.Count -gt 0) {
            $endpointId = $endpoints.value[0].id
            Write-Host "Found service endpoint ID: $endpointId"
            
            # Get the endpoint details with credentials
            $endpointUrl = "$($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$($env:SYSTEM_TEAMPROJECT)/_apis/serviceendpoint/endpoints/$endpointId`?api-version=7.1-preview.4"
            $endpoint = Invoke-RestMethod -Uri $endpointUrl -Method Get -Headers $headers -ContentType "application/json"
            
            # Extract token from the authorization parameters
            if ($endpoint.authorization.scheme -eq "Token" -and $endpoint.authorization.parameters.accessToken) {
                $gitHubToken = $endpoint.authorization.parameters.accessToken
                Write-Host "Successfully retrieved GitHub token from service connection."
            } else {
                Write-Host "##vso[task.logissue type=error]Service connection '$GitHubServiceConnection' does not contain a token in the expected format."
                Write-Host "##vso[task.complete result=Failed;]Invalid service connection configuration."
                exit 1
            }
        } else {
            Write-Host "##vso[task.logissue type=error]Service connection '$GitHubServiceConnection' not found."
            Write-Host "##vso[task.complete result=Failed;]Service connection not found."
            exit 1
        }
    } catch {
        Write-Host "##vso[task.logissue type=error]Failed to retrieve GitHub token from service connection: $($_.Exception.Message)"
        Write-Host "##vso[task.complete result=Failed;]Failed to retrieve GitHub token from service connection."
        exit 1
    }
}

# Make request to the GitHub API
try {
    $headers = @{
        "Accept" = "application/vnd.github.v3+json"
        "User-Agent" = "SkiaSharp-AzurePipelines"
    }
    
    if ($gitHubToken) {
        $headers["Authorization"] = "token $gitHubToken"
        Write-Host "Using authenticated GitHub API request."
    } else {
        Write-Host "##vso[task.logissue type=error]No GitHub token available. Cannot authenticate with GitHub API."
        Write-Host "##vso[task.complete result=Failed;]GitHub authentication failed."
        exit 1
    }
    
    $json = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/mono/SkiaSharp/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" `
        -Headers $headers `
        -Method Get `
        -ErrorAction Stop
} catch {
    Write-Host "##vso[task.logissue type=error]Failed to fetch PR information from GitHub API: $($_.Exception.Message)"
    Write-Host "##vso[task.complete result=Failed;]Failed to fetch PR information from GitHub."
    exit 1
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
    Pop-Location
    Write-Host "##vso[task.logissue type=error]Failed to checkout skia PR #${skiaPR}: $($_.Exception.Message)"
    Write-Host "##vso[task.complete result=Failed;]Failed to checkout required skia PR."
    exit 1
} finally {
    Pop-Location
}

Write-Host "Requesting full build with custom skia PR."
Write-Host "##vso[task.setvariable variable=DOWNLOAD_EXTERNALS]required"
