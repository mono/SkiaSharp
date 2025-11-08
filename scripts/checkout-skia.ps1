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
                Write-Host "Warning: Service connection does not contain a token in the expected format."
            }
        } else {
            Write-Host "Warning: Service connection '$GitHubServiceConnection' not found."
        }
    } catch {
        Write-Host "Warning: Failed to retrieve GitHub token from service connection: $($_.Exception.Message)"
        Write-Host "Continuing without authentication."
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
        Write-Host "Warning: No GitHub token available. API request may fail due to GitHub authentication requirements."
    }
    
    $json = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/mono/SkiaSharp/pulls/$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER" `
        -Headers $headers `
        -Method Get `
        -ErrorAction Stop
} catch {
    Write-Host "Failed to fetch PR information from GitHub API: $($_.Exception.Message)"
    Write-Host "This might be due to authentication requirements or network issues."
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
