$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../../..'))
$scriptPath = Join-Path $repoRoot 'scripts/infra/native/shared/set-build-variables.ps1'
$pwsh = (Get-Command pwsh).Source

$identityVariables = @(
    'ARCADE_OFFICIAL_BUILD_ID',
    'BUILD_COUNTER',
    'BUILD_NUMBER',
    'BUILD_REASON',
    'BUILD_REPOSITORY_PROVIDER',
    'BUILD_REPOSITORY_URI',
    'BUILD_SOURCEBRANCH',
    'BUILD_SOURCEBRANCHNAME',
    'BUILD_SOURCEVERSION',
    'BUILD_SOURCEVERSIONMESSAGE',
    'FEATURE_NAME',
    'FEATURE_NAME_PREFIX',
    'GIT_BRANCH_NAME',
    'GIT_SHA',
    'GIT_URL',
    'PREVIEW_LABEL',
    'PR_NUMBER',
    'RESOURCES_PIPELINE_SKIASHARP_RUNNAME',
    'SKIASHARP_VERSION',
    'SYSTEM_PULLREQUEST_PULLREQUESTID',
    'SYSTEM_PULLREQUEST_PULLREQUESTNUMBER',
    'SYSTEM_PULLREQUEST_SOURCEBRANCH',
    'SYSTEM_PULLREQUEST_SOURCECOMMITID',
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
        FEATURE_NAME_PREFIX = 'feature/'
        PREVIEW_LABEL = 'preview.0'
        SKIASHARP_VERSION = '4.152.0'
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

    return [pscustomobject]@{
        Name = $Name
        ExitCode = $process.ExitCode
        Output = "$output`n$errorOutput"
    }
}

function Get-LastVariableValue {
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
Assert-Equal (Get-LastVariableValue $githubPr.Output 'PREVIEW_LABEL') 'pr.4803' 'GitHub PR label'
Assert-Equal (Get-LastVariableValue $githubPr.Output 'GIT_SHA') 'cccccccccccccccccccccccccccccccccccccccc' 'GitHub PR commit'
Assert-Equal (Get-LastVariableValue $githubPr.Output 'GIT_BRANCH_NAME') 'refs/heads/feature/foo_bar' 'GitHub PR branch'
Assert-Equal (Get-LastVariableValue $githubPr.Output 'FEATURE_NAME') '' 'GitHub PR feature name'
Assert-Equal (Get-LastVariableValue $githubPr.Output 'BUILD_NUMBER') '26418.3' 'GitHub PR build number'
Assert-BuildLabel $githubPr.Output '4.152.0-pr.4803.26418.3'

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
Assert-Equal (Get-LastVariableValue $azurePr.Output 'PREVIEW_LABEL') 'pr.63954' 'Azure Repos PR label'
Assert-Equal (Get-LastVariableValue $azurePr.Output 'PR_NUMBER') '63954' 'Azure Repos PR number'
Assert-Equal (Get-LastVariableValue $azurePr.Output 'BUILD_NUMBER') '26418.3' 'Azure Repos PR build number'
Assert-BuildLabel $azurePr.Output '4.152.0-pr.63954.26418.3'

$resourcePr = Invoke-BuildIdentityCase 'PR resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    BUILD_REPOSITORY_PROVIDER = 'TfsGit'
    BUILD_REPOSITORY_URI = 'https://dev.azure.com/dnceng/internal/_git/dotnet-SkiaSharp'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    BUILD_SOURCEVERSION = 'ffffffffffffffffffffffffffffffffffffffff'
    BUILD_SOURCEVERSIONMESSAGE = 'Merge 1111111111111111111111111111111111111111 into 2222222222222222222222222222222222222222'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-pr.63954.26418.3'
}
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'PREVIEW_LABEL') 'pr.63954' 'Resource PR label'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'BUILD_NUMBER') '26418.3' 'Resource build number'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'BUILD_COUNTER') '26418.3' 'Resource build counter'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'GIT_SHA') '1111111111111111111111111111111111111111' 'Resource source commit'
Assert-BuildLabel $resourcePr.Output '4.152.0-pr.63954.26418.3'

$main = Invoke-BuildIdentityCase 'GitHub main' @{
    BUILD_REASON = 'IndividualCI'
}
Assert-Equal (Get-LastVariableValue $main.Output 'PREVIEW_LABEL') 'preview.0' 'Main preview label'
Assert-Equal (Get-LastVariableValue $main.Output 'BUILD_NUMBER') '26418.3' 'Main build number'
Assert-BuildLabel $main.Output '4.152.0-preview.0.26418.3+main'

$feature = Invoke-BuildIdentityCase 'Feature branch' @{
    BUILD_REASON = 'Manual'
    BUILD_SOURCEBRANCH = 'refs/heads/feature/foo_bar'
    BUILD_SOURCEBRANCHNAME = 'foo_bar'
}
Assert-Equal (Get-LastVariableValue $feature.Output 'FEATURE_NAME') 'foo-bar' 'Feature package label'

$resourceMain = Invoke-BuildIdentityCase 'Main resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.26418.3+main'
}
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'PREVIEW_LABEL') 'preview.0' 'Resource main label'
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'BUILD_NUMBER') '26418.3' 'Resource main build number'
Assert-BuildLabel $resourceMain.Output '4.152.0-preview.0.26418.3+main'

$malformed = Invoke-BuildIdentityCase 'Malformed resource identity' @{
    BUILD_REASON = 'ResourceTrigger'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-pr..8'
} -ExpectFailure
if ($malformed.Output -notmatch 'Unable to parse upstream build identity') {
    throw "Malformed resource identity failed for the wrong reason.`n$($malformed.Output)"
}

$variablesYaml = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-variables.yml') -Raw
if ($variablesYaml -notmatch 'ARCADE_OFFICIAL_BUILD_ID:\s+\$\[format\(' -or
    $variablesYaml -notmatch "BUILD_NUMBER:\s*''") {
    throw 'Azure variables must provide Arcade OfficialBuildId and derive the package build number at runtime.'
}

$globalJson = Get-Content (Join-Path $repoRoot 'global.json') -Raw | ConvertFrom-Json
$pipelineSdk = [regex]::Match(
    $variablesYaml,
    "DOTNET_VERSION:\s*'(?<version>[^']+)'").Groups['version'].Value
$buildSdk = [Version]$globalJson.sdk.version
$toolSdk = [Version]$globalJson.tools.dotnet
if ($pipelineSdk -cne $globalJson.sdk.version -or
    $globalJson.sdk.allowPrerelease -ne $false -or
    $globalJson.sdk.rollForward -cne 'latestFeature' -or
    $toolSdk.Major -ne $buildSdk.Major -or
    $toolSdk.Minor -ne $buildSdk.Minor -or
    $toolSdk.Build -lt 200 -or
    $variablesYaml -notmatch 'DOTNET_INSTALL_DIR:\s*\$\(Agent\.TempDirectory\)/arcade-dotnet-probe' -or
    $variablesYaml -notmatch 'DOTNET_GLOBAL_INSTALL_DIR:\s*\$\(Agent\.TempDirectory\)/arcade-dotnet') {
    throw 'SkiaSharp must build with the stable pipeline SDK while Arcade uses a stable .NET 10.0.2xx-or-later tool CLI.'
}

$packageStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-package.yml') -Raw
if ($packageStages -match 'packStableNuGets') {
    throw 'The Package stage must not select a second stable package variant.'
}

$packagePipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-package.yml') -Raw
if ($packagePipeline -notmatch 'publishingVersion:\s*3\s+officialBuildId:\s*\$\(ARCADE_OFFICIAL_BUILD_ID\)') {
    throw 'BAR registration must use the same Arcade OfficialBuildId as manifest generation.'
}
if ($packagePipeline -match 'source:\s*skiasharp-native') {
    throw 'The combined Package pipeline must not inherit identity from a Native pipeline resource.'
}
if ($packagePipeline -notmatch "buildPipelineType:\s*'build'") {
    throw "The internal Package pipeline must use the 'build' role."
}

$testsPipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-tests.yml') -Raw
if ($testsPipeline -notmatch 'source:\s*skiasharp-package' -or
    $testsPipeline -notmatch "buildPipelineType:\s*'test'") {
    throw 'The separate Tests pipeline must inherit the final Package identity.'
}

$completePipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-complete.yml') -Raw
if ($completePipeline -notmatch "buildPipelineType:\s*'complete'") {
    throw "The public pipeline must use the 'complete' role."
}

$packageStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-package.yml') -Raw
if ($packageStages -match 'Re-upload Native Artifacts') {
    throw 'The Package stage must consume Native artifacts from the same pipeline run.'
}

$packageScript = Get-Content (Join-Path $repoRoot 'scripts/infra/package/nuget.cake') -Raw
$normalTask = $packageScript.Substring(0, $packageScript.IndexOf('Task ("nuget-special")'))
if ($normalTask -match 'PACK_STABLE_NUGETS|packStableNuGets' -or
    $normalTask -notmatch '\{\s*"VersionSuffix",\s*PREVIEW_NUGET_SUFFIX\s*\}' -or
    ([regex]::Matches($normalTask, 'RunDotNetPack\s*\(').Count -ne 1)) {
    throw 'nuget-normal must pack exactly one version family selected by VersionSuffix.'
}

Write-Host 'Build identity tests passed.'
