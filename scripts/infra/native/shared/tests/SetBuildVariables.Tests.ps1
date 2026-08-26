$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/native/shared/set-build-variables.ps1'
$pwsh = (Get-Command pwsh).Source

$identityVariables = @(
    'ARCADE_OFFICIAL_BUILD_ID'
    'BUILD_COUNTER'
    'BUILD_NUMBER'
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

        [switch] $ExpectFailure
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $pwsh
    $startInfo.ArgumentList.Add('-NoLogo')
    $startInfo.ArgumentList.Add('-NoProfile')
    $startInfo.ArgumentList.Add('-File')
    $startInfo.ArgumentList.Add($scriptPath)
    $startInfo.ArgumentList.Add('-UpdateBuildNumber')
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
Assert-BuildLabel $resource '4.152.0-preview.0.22+main'

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
