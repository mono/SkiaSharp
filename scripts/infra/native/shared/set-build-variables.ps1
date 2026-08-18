Param(
    [switch] $UpdateBuildNumber
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-FirstNonEmpty {
    param([string[]] $Values)

    foreach ($value in $Values) {
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    return ''
}

function Set-BuildVariable {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [AllowEmptyString()]
        [string] $Value
    )

    Set-Item -Path "Env:$Name" -Value $Value
    Write-Host "##vso[task.setvariable variable=$Name]$Value"
}

function ConvertTo-ArcadeBuildNumber {
    param([Parameter(Mandatory)][string] $OfficialBuildId)

    $match = [regex]::Match(
        $OfficialBuildId,
        '^(?<date>\d{8})\.(?<revision>\d+)$')
    if (-not $match.Success) {
        throw "ARCADE_OFFICIAL_BUILD_ID '$OfficialBuildId' must use yyyyMMdd.revision."
    }

    $date = [DateTime]::ParseExact(
        $match.Groups['date'].Value,
        'yyyyMMdd',
        [Globalization.CultureInfo]::InvariantCulture)
    $shortDate = (($date.Year % 100) * 1000) + (50 * $date.Month) + $date.Day
    return "$shortDate.$($match.Groups['revision'].Value)"
}

$officialBuildId = "$env:ARCADE_OFFICIAL_BUILD_ID"
if ([string]::IsNullOrWhiteSpace($officialBuildId)) {
    throw 'ARCADE_OFFICIAL_BUILD_ID is empty.'
}

$productBuildNumber = ConvertTo-ArcadeBuildNumber $officialBuildId
Write-Host '# Arcade build identity'
Write-Host "Official build ID: $officialBuildId"
Write-Host "Product build number: $productBuildNumber"
Set-BuildVariable BUILD_NUMBER $productBuildNumber

$rawBranch = "$env:BUILD_SOURCEBRANCH"
$pullRequestRef = [regex]::Match($rawBranch, '^refs/pull/(\d+)/merge$')
$isPullRequest = $env:BUILD_REASON -eq 'PullRequest' -or $pullRequestRef.Success
$prNumber = Get-FirstNonEmpty @(
    "$env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER",
    "$env:SYSTEM_PULLREQUEST_PULLREQUESTID",
    $(if ($pullRequestRef.Success) { $pullRequestRef.Groups[1].Value } else { '' })
)

if ($isPullRequest -and [string]::IsNullOrWhiteSpace($prNumber)) {
    throw "Unable to determine the pull request number for '$rawBranch' from provider '$env:BUILD_REPOSITORY_PROVIDER'."
}

if ($isPullRequest) {
    Write-Host "# Pull request identity"
    Write-Host "PR number: $prNumber"
    Set-BuildVariable PR_NUMBER $prNumber
    if ([string]::IsNullOrWhiteSpace($env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER)) {
        Set-BuildVariable SYSTEM_PULLREQUEST_PULLREQUESTNUMBER $prNumber
    }
}

$sourceCommit = if ($isPullRequest) {
    Get-FirstNonEmpty @("$env:SYSTEM_PULLREQUEST_SOURCECOMMITID")
} else {
    ''
}
if ([string]::IsNullOrWhiteSpace($sourceCommit) -and $isPullRequest) {
    $mergeMessage = [regex]::Match(
        "$env:BUILD_SOURCEVERSIONMESSAGE",
        '^Merge\s+([0-9a-fA-F]{7,40})\s+into\s+')
    if ($mergeMessage.Success) {
        $sourceCommit = $mergeMessage.Groups[1].Value
    }
}
if ([string]::IsNullOrWhiteSpace($sourceCommit)) {
    $sourceCommit = "$env:BUILD_SOURCEVERSION"
}

$sourceBranch = if ($isPullRequest) {
    Get-FirstNonEmpty @("$env:SYSTEM_PULLREQUEST_SOURCEBRANCH", $rawBranch)
} else {
    $rawBranch
}
$sourceRepository = Get-FirstNonEmpty @(
    "$env:SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI",
    "$env:BUILD_REPOSITORY_URI"
)

if ([string]::IsNullOrWhiteSpace($sourceCommit) -or
    [string]::IsNullOrWhiteSpace($sourceBranch) -or
    [string]::IsNullOrWhiteSpace($sourceRepository)) {
    throw "Incomplete source identity: commit='$sourceCommit', branch='$sourceBranch', repository='$sourceRepository'."
}

Set-BuildVariable GIT_SHA $sourceCommit
Set-BuildVariable GIT_BRANCH_NAME $sourceBranch
Set-BuildVariable GIT_URL $sourceRepository

Write-Host "`n# Normalized source identity"
Write-Host "Provider: $env:BUILD_REPOSITORY_PROVIDER"
Write-Host "Reason: $env:BUILD_REASON"
Write-Host "Raw branch: $rawBranch"
Write-Host "Source branch: $sourceBranch"
Write-Host "Source commit: $sourceCommit"
Write-Host "Source repository: $sourceRepository"

Write-Host "`n# Setting preview label"
$previewLabel = "$env:PREVIEW_LABEL".Trim().ToLowerInvariant()
if ($isPullRequest) {
    $previewLabel = "pr.$prNumber"
} elseif ($env:BUILD_REASON -eq 'Schedule') {
    $previewLabel = 'nightly'
}

if ([string]::IsNullOrWhiteSpace($previewLabel)) {
    throw "Preview label is empty for build reason '$env:BUILD_REASON'."
}

Write-Host "Preview label: $previewLabel"
Set-BuildVariable PREVIEW_LABEL $previewLabel

Write-Host "`n# Checking for secondary build information"
$resourceRunName = "$env:RESOURCES_PIPELINE_SKIASHARP_RUNNAME"
if (($env:BUILD_REASON -eq 'ResourceTrigger' -or $env:BUILD_REASON -eq 'Manual') -and
    -not [string]::IsNullOrWhiteSpace($resourceRunName)) {
    Write-Host "Working with $resourceRunName"
    $versionPrefix = [regex]::Escape("$env:SKIASHARP_VERSION-")
    $releasePrefix = [regex]::Escape("$env:SKIASHARP_VERSION+")
    $releaseMatch = [regex]::Match(
        $resourceRunName,
        "^$releasePrefix(?<official>\d{8}\.\d+)$")
    $runNameWithoutMetadata = $resourceRunName.Split('+')[0]
    $previewMatch = [regex]::Match(
        $runNameWithoutMetadata,
        "^$versionPrefix(?<label>.+?)\.(?<build>(?:(?:\d{5}|\d{8})\.)?\d+)$")
    if ($releaseMatch.Success) {
        $previewLabel = 'stable'
        $buildNumber = ConvertTo-ArcadeBuildNumber $releaseMatch.Groups['official'].Value
    } elseif ($previewMatch.Success -and -not $previewMatch.Groups['label'].Value.EndsWith('.')) {
        $previewLabel = $previewMatch.Groups['label'].Value.ToLowerInvariant()
        $buildNumber = $previewMatch.Groups['build'].Value
    } else {
        throw "Unable to parse upstream build identity '$resourceRunName'."
    }

    Write-Host "Inherited preview label: $previewLabel"
    Write-Host "Inherited build number: $buildNumber"
    Set-BuildVariable PREVIEW_LABEL $previewLabel
    Set-BuildVariable BUILD_NUMBER $buildNumber
    Set-BuildVariable BUILD_COUNTER $buildNumber
} else {
    Write-Host "Using this pipeline's Arcade build identity."
}

$isReleaseBuild = $previewLabel -ceq 'stable'
$finalVersionKind = if ($isReleaseBuild) { 'release' } else { '' }
Set-BuildVariable DOTNET_FINAL_VERSION_KIND $finalVersionKind

if ($isReleaseBuild -and
    [string]::IsNullOrWhiteSpace($resourceRunName) -and
    ($env:SYSTEM_TEAMPROJECT -ne 'internal' -or $env:BUILD_REASON -ne 'Manual')) {
    throw 'Exact release packages require an internal manual build.'
}

if ([string]::IsNullOrWhiteSpace($env:BUILD_NUMBER)) {
    throw 'BUILD_NUMBER is empty.'
}

Write-Host "Product build number: $env:BUILD_NUMBER"
Write-Host "Special-package counter: $env:BUILD_COUNTER"

Write-Host "`n# Setting build label"
if ($UpdateBuildNumber) {
    if (-not [string]::IsNullOrWhiteSpace($resourceRunName)) {
        $label = $resourceRunName
    } elseif ($isReleaseBuild) {
        $label = "$env:SKIASHARP_VERSION+$officialBuildId"
    } else {
        $branchMetadata = if ($isPullRequest) { '' } else { "+$env:BUILD_SOURCEBRANCHNAME" }
        $label = "$env:SKIASHARP_VERSION-$env:PREVIEW_LABEL.$env:BUILD_NUMBER$branchMetadata"
    }

    Write-Host "Build label: $label"
    Write-Host "##vso[build.updatebuildnumber]$label"
} else {
    Write-Host 'Skipping build number update.'
}

exit 0
