$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/shared/set-build-variables.ps1'
$pwsh = (Get-Command pwsh).Source

$identityVariables = @(
    'ARCADE_OFFICIAL_BUILD_ID'
    'BUILD_COUNTER'
    'BUILD_NUMBER'
    'BUILD_BUILDNUMBER'
    'BUILD_REASON'
    'BUILD_REPOSITORY_PROVIDER'
    'BUILD_REPOSITORY_URI'
    'BUILD_SOURCEBRANCH'
    'BUILD_SOURCEBRANCHNAME'
    'BUILD_SOURCEVERSION'
    'BUILD_SOURCEVERSIONMESSAGE'
    'DOTNET_FINAL_VERSION_KIND'
    'GIT_BRANCH_NAME'
    'GIT_SHA'
    'GIT_URL'
    'PREVIEW_LABEL'
    'PR_NUMBER'
    'RESOURCES_PIPELINE_SKIASHARP_RUNNAME'
    'SKIASHARP_VERSION'
    'SYSTEM_TEAMPROJECT'
    'SYSTEM_PULLREQUEST_PULLREQUESTID'
    'SYSTEM_PULLREQUEST_PULLREQUESTNUMBER'
    'SYSTEM_PULLREQUEST_SOURCEBRANCH'
    'SYSTEM_PULLREQUEST_SOURCECOMMITID'
    'SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI'
)

function Invoke-BuildIdentityCase {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [hashtable] $Environment,

        [switch] $ExpectFailure,

        [switch] $NoUpdateBuildNumber
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $pwsh
    $startInfo.ArgumentList.Add('-NoLogo')
    $startInfo.ArgumentList.Add('-NoProfile')
    $startInfo.ArgumentList.Add('-File')
    $startInfo.ArgumentList.Add($scriptPath)
    if (-not $NoUpdateBuildNumber) {
        $startInfo.ArgumentList.Add('-UpdateBuildNumber')
    }
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    foreach ($variable in $identityVariables) {
        $startInfo.Environment.Remove($variable) | Out-Null
    }

    $defaults = @{
        ARCADE_OFFICIAL_BUILD_ID = '20260818.3'
        BUILD_COUNTER = '41'
        BUILD_NUMBER = ''
        BUILD_BUILDNUMBER = ''
        BUILD_REPOSITORY_PROVIDER = 'GitHub'
        BUILD_REPOSITORY_URI = 'https://github.com/mono/SkiaSharp.git'
        BUILD_SOURCEBRANCH = 'refs/heads/main'
        BUILD_SOURCEBRANCHNAME = 'main'
        BUILD_SOURCEVERSION = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        BUILD_SOURCEVERSIONMESSAGE = ''
        PREVIEW_LABEL = 'preview.0'
        SKIASHARP_VERSION = '4.152.0'
        SYSTEM_TEAMPROJECT = 'internal'
    }
    foreach ($pair in $defaults.GetEnumerator()) {
        $startInfo.Environment[$pair.Key] = $pair.Value
    }
    foreach ($pair in $Environment.GetEnumerator()) {
        $startInfo.Environment[$pair.Key] = [string]$pair.Value
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null
    $output = $process.StandardOutput.ReadToEnd()
    $errorOutput = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($ExpectFailure) {
        if ($process.ExitCode -eq 0) {
            throw "Case '$Name' unexpectedly succeeded.`n$output"
        }
    } elseif ($process.ExitCode -ne 0) {
        throw "Case '$Name' failed with exit code $($process.ExitCode).`n$output`n$errorOutput"
    }

    return "$output`n$errorOutput"
}

function Get-VariableValue {
    param(
        [Parameter(Mandatory)]
        [string] $Output,

        [Parameter(Mandatory)]
        [string] $Name
    )

    $matches = [regex]::Matches(
        $Output,
        "##vso\[task\.setvariable variable=$([regex]::Escape($Name))\]([^\r\n]*)")
    if ($matches.Count -eq 0) {
        throw "Output did not set variable '$Name'.`n$Output"
    }

    return $matches[$matches.Count - 1].Groups[1].Value
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Actual,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Expected,

        [Parameter(Mandatory)]
        [string] $Description
    )

    if ($Actual -cne $Expected) {
        throw "$Description expected '$Expected' but got '$Actual'."
    }
}

function Assert-BuildLabel {
    param(
        [Parameter(Mandatory)]
        [string] $Output,

        [Parameter(Mandatory)]
        [string] $Expected
    )

    $match = [regex]::Match($Output, '(?m)^Build label: (.+)$')
    if (-not $match.Success) {
        throw "Output did not contain a build label.`n$Output"
    }
    Assert-Equal $match.Groups[1].Value.Trim() $Expected 'Build label'
}

$githubPr = Invoke-BuildIdentityCase 'GitHub PR' @{
    BUILD_REASON = 'PullRequest'
    BUILD_SOURCEBRANCH = 'refs/pull/4803/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    BUILD_SOURCEVERSION = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
    SYSTEM_PULLREQUEST_PULLREQUESTNUMBER = '4803'
    SYSTEM_PULLREQUEST_SOURCEBRANCH = 'refs/heads/feature/foo_bar'
    SYSTEM_PULLREQUEST_SOURCECOMMITID = 'cccccccccccccccccccccccccccccccccccccccc'
    SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI = 'https://github.com/mono/SkiaSharp.git'
}
Assert-Equal (Get-VariableValue $githubPr 'PREVIEW_LABEL') 'pr.4803' 'GitHub PR label'
Assert-Equal (Get-VariableValue $githubPr 'GIT_SHA') 'cccccccccccccccccccccccccccccccccccccccc' 'GitHub PR commit'
Assert-Equal (Get-VariableValue $githubPr 'GIT_BRANCH_NAME') 'refs/heads/feature/foo_bar' 'GitHub PR branch'
Assert-BuildLabel $githubPr '4.152.0-pr.4803.26418.3'

$azurePr = Invoke-BuildIdentityCase 'Azure Repos PR' @{
    BUILD_REASON = 'PullRequest'
    BUILD_REPOSITORY_PROVIDER = 'TfsGit'
    BUILD_REPOSITORY_URI = 'https://dev.azure.com/dnceng/internal/_git/dotnet-SkiaSharp'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    BUILD_SOURCEVERSION = 'dddddddddddddddddddddddddddddddddddddddd'
    SYSTEM_PULLREQUEST_PULLREQUESTID = '63954'
    SYSTEM_PULLREQUEST_SOURCEBRANCH = 'refs/heads/dev/dnceng-pipelines'
    SYSTEM_PULLREQUEST_SOURCECOMMITID = 'eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee'
}
Assert-Equal (Get-VariableValue $azurePr 'PREVIEW_LABEL') 'pr.63954' 'Azure Repos PR label'
Assert-Equal (Get-VariableValue $azurePr 'PR_NUMBER') '63954' 'Azure Repos PR number'
Assert-BuildLabel $azurePr '4.152.0-pr.63954.26418.3'

$main = Invoke-BuildIdentityCase 'Main CI' @{
    BUILD_REASON = 'IndividualCI'
}
Assert-Equal (Get-VariableValue $main 'PREVIEW_LABEL') 'preview.0' 'Main preview label'
Assert-BuildLabel $main '4.152.0-preview.0.26418.3+main'

$release = Invoke-BuildIdentityCase 'Exact release' @{
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/release/4.152.0'
    BUILD_SOURCEBRANCHNAME = '4.152.0'
    PREVIEW_LABEL = 'Stable'
}
Assert-Equal (Get-VariableValue $release 'PREVIEW_LABEL') 'stable' 'Release normalized label'
Assert-Equal (Get-VariableValue $release 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release final version kind'
Assert-BuildLabel $release '4.152.0+20260818.3'

$resource = Invoke-BuildIdentityCase 'Tests inherit Package identity' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.22+main'
}
Assert-Equal (Get-VariableValue $resource 'BUILD_NUMBER') '22' 'Resource build number'
Assert-Equal (Get-VariableValue $resource 'BUILD_COUNTER') '22' 'Legacy resource counter'
if ($resource -match '##vso\[task\.setvariable variable=ARCADE_OFFICIAL_BUILD_ID\]') {
    throw "Undated resource identity must not invent an official build ID.`n$resource"
}
Assert-BuildLabel $resource '4.152.0-preview.0.22+main'

$midnight = Invoke-BuildIdentityCase 'PR consumer after midnight' @{
    ARCADE_OFFICIAL_BUILD_ID = '20261005.1'
    BUILD_BUILDNUMBER = '4.152.0-pr.5266.26504.37'
    BUILD_REASON = 'PullRequest'
    BUILD_SOURCEBRANCH = 'refs/pull/5266/merge'
    SYSTEM_PULLREQUEST_PULLREQUESTNUMBER = '5266'
}
Assert-Equal (Get-VariableValue $midnight 'BUILD_NUMBER') '26504.37' 'Original PR build number'
Assert-Equal (Get-VariableValue $midnight 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Original official build ID'
Assert-Equal ([regex]::Match($midnight, '(?m)^Special-package counter: (.+)$').Groups[1].Value.Trim()) `
    '41' 'Same-run special-package counter'
Assert-BuildLabel $midnight '4.152.0-pr.5266.26504.37'

$producer = Invoke-BuildIdentityCase 'Prepare producer identity' @{
    SKIASHARP_VERSION = '4.156.0'
    ARCADE_OFFICIAL_BUILD_ID = '20261008.29'
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/mattleibow-samples-sdk-matrix'
    BUILD_SOURCEBRANCHNAME = 'mattleibow-samples-sdk-matrix'
}
$producerRunName = [regex]::Match($producer, '(?m)^Build label: (.+)$').Groups[1].Value.Trim()
Assert-Equal $producerRunName '4.156.0-preview.0.26508.29+mattleibow-samples-sdk-matrix' 'Prepare run name'
$consumer = Invoke-BuildIdentityCase 'MacSamples retains producer identity after midnight' @{
    SKIASHARP_VERSION = '4.156.0'
    ARCADE_OFFICIAL_BUILD_ID = '20261009.2'
    BUILD_BUILDNUMBER = $producerRunName
    BUILD_COUNTER = '42'
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/mattleibow-samples-sdk-matrix'
    BUILD_SOURCEBRANCHNAME = 'mattleibow-samples-sdk-matrix'
}
Assert-Equal (Get-VariableValue $consumer 'PREVIEW_LABEL') 'preview.0' 'Producer preview label'
Assert-Equal (Get-VariableValue $consumer 'BUILD_NUMBER') '26508.29' 'Producer build number'
Assert-Equal (Get-VariableValue $consumer 'ARCADE_OFFICIAL_BUILD_ID') '20261008.29' 'Producer official build ID'
Assert-Equal ([regex]::Match($consumer, '(?m)^Special-package counter: (.+)$').Groups[1].Value.Trim()) `
    '42' 'Later-job special-package counter'
if ($consumer -match '##vso\[task\.setvariable variable=BUILD_COUNTER\]') {
    throw "Same-run identity must not overwrite the special-package counter.`n$consumer"
}
Assert-BuildLabel $consumer $producerRunName

$laterJob = Invoke-BuildIdentityCase 'Later job without run-name update' @{
    SKIASHARP_VERSION = '4.156.0'
    ARCADE_OFFICIAL_BUILD_ID = '20261009.2'
    BUILD_BUILDNUMBER = $producerRunName
    BUILD_REASON = 'IndividualCI'
} -NoUpdateBuildNumber
Assert-Equal (Get-VariableValue $laterJob 'BUILD_NUMBER') '26508.29' 'Later-job producer build number'
Assert-Equal (Get-VariableValue $laterJob 'ARCADE_OFFICIAL_BUILD_ID') '20261008.29' 'Later-job producer official build ID'
if ($laterJob -match '##vso\[build\.updatebuildnumber\]') {
    throw "Later job must not update the run name.`n$laterJob"
}

$upstream = Invoke-BuildIdentityCase 'Resource identity takes precedence after midnight' @{
    ARCADE_OFFICIAL_BUILD_ID = '20261005.1'
    BUILD_BUILDNUMBER = '4.152.0-preview.0.26505.1+main'
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.26504.37+main'
}
Assert-Equal (Get-VariableValue $upstream 'BUILD_NUMBER') '26504.37' 'Upstream build number'
Assert-Equal (Get-VariableValue $upstream 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Upstream official build ID'
Assert-Equal (Get-VariableValue $upstream 'BUILD_COUNTER') '26504.37' 'Upstream special-package counter'
Assert-BuildLabel $upstream '4.152.0-preview.0.26504.37+main'

$releaseMidnight = Invoke-BuildIdentityCase 'Release consumer after midnight' @{
    ARCADE_OFFICIAL_BUILD_ID = '20261005.1'
    BUILD_BUILDNUMBER = '4.152.0+20261004.37'
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/release/4.152.0'
    BUILD_SOURCEBRANCHNAME = '4.152.0'
    PREVIEW_LABEL = 'stable'
}
Assert-Equal (Get-VariableValue $releaseMidnight 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release consumer kind'
Assert-Equal (Get-VariableValue $releaseMidnight 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Release producer official build ID'
Assert-BuildLabel $releaseMidnight '4.152.0+20261004.37'

$yearBoundary = Invoke-BuildIdentityCase 'Consumer after year boundary' @{
    ARCADE_OFFICIAL_BUILD_ID = '20270101.1'
    BUILD_BUILDNUMBER = '4.152.0-preview.0.26631.9+main'
    BUILD_REASON = 'IndividualCI'
}
Assert-Equal (Get-VariableValue $yearBoundary 'BUILD_NUMBER') '26631.9' 'Year-boundary build number'
Assert-Equal (Get-VariableValue $yearBoundary 'ARCADE_OFFICIAL_BUILD_ID') '20261231.9' 'Year-boundary official build ID'
Assert-BuildLabel $yearBoundary '4.152.0-preview.0.26631.9+main'

$manualResource = Invoke-BuildIdentityCase 'Manual tests inherit dated Package identity' @{
    ARCADE_OFFICIAL_BUILD_ID = '20261005.1'
    BUILD_BUILDNUMBER = '20261005.1'
    BUILD_REASON = 'Manual'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.26504.37+main'
}
Assert-Equal (Get-VariableValue $manualResource 'BUILD_NUMBER') '26504.37' 'Manual upstream build number'
Assert-Equal (Get-VariableValue $manualResource 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Manual upstream official build ID'
Assert-BuildLabel $manualResource '4.152.0-preview.0.26504.37+main'

$datedResource = Invoke-BuildIdentityCase 'Resource with full-date preview identity' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.20261004.37+main'
}
Assert-Equal (Get-VariableValue $datedResource 'BUILD_NUMBER') '20261004.37' 'Full-date upstream build number'
Assert-Equal (Get-VariableValue $datedResource 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Full-date upstream official build ID'
Assert-BuildLabel $datedResource '4.152.0-preview.0.20261004.37+main'

$stableResource = Invoke-BuildIdentityCase 'Resource with stable identity' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0+20261004.37'
    SYSTEM_TEAMPROJECT = 'public'
}
Assert-Equal (Get-VariableValue $stableResource 'PREVIEW_LABEL') 'stable' 'Stable upstream label'
Assert-Equal (Get-VariableValue $stableResource 'BUILD_NUMBER') '26504.37' 'Stable upstream build number'
Assert-Equal (Get-VariableValue $stableResource 'ARCADE_OFFICIAL_BUILD_ID') '20261004.37' 'Stable upstream official build ID'
Assert-Equal (Get-VariableValue $stableResource 'BUILD_COUNTER') '26504.37' 'Stable upstream counter'
Assert-Equal (Get-VariableValue $stableResource 'DOTNET_FINAL_VERSION_KIND') 'release' 'Stable upstream kind'
Assert-BuildLabel $stableResource '4.152.0+20261004.37'

$initialRun = Invoke-BuildIdentityCase 'Initial non-product run name' @{
    BUILD_REASON = 'IndividualCI'
    BUILD_BUILDNUMBER = '20260818.3'
}
Assert-Equal (Get-VariableValue $initialRun 'BUILD_NUMBER') '26418.3' 'Initial computed build number'
Assert-BuildLabel $initialRun '4.152.0-preview.0.26418.3+main'

$malformedCanonical = Invoke-BuildIdentityCase 'Malformed canonical identity' @{
    BUILD_REASON = 'IndividualCI'
    BUILD_BUILDNUMBER = '4.152.0-preview..8'
} -ExpectFailure
if ($malformedCanonical -notmatch 'Unable to parse upstream build identity') {
    throw "Malformed canonical identity failed for the wrong reason.`n$malformedCanonical"
}

$automaticRelease = Invoke-BuildIdentityCase 'Automatic exact release' @{
    BUILD_REASON = 'IndividualCI'
    PREVIEW_LABEL = 'stable'
} -ExpectFailure
if ($automaticRelease -notmatch 'Exact release packages require') {
    throw "Automatic release failed for the wrong reason.`n$automaticRelease"
}

$malformed = Invoke-BuildIdentityCase 'Malformed resource identity' @{
    BUILD_REASON = 'ResourceTrigger'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-pr..8'
} -ExpectFailure
if ($malformed -notmatch 'Unable to parse upstream build identity') {
    throw "Malformed resource identity failed for the wrong reason.`n$malformed"
}

Write-Host 'Build identity tests passed.'
