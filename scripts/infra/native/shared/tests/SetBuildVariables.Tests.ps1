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
    'DOTNET_FINAL_VERSION_KIND',
    'GIT_BRANCH_NAME',
    'GIT_SHA',
    'GIT_URL',
    'PREVIEW_LABEL',
    'PR_NUMBER',
    'RESOURCES_PIPELINE_SKIASHARP_RUNNAME',
    'SKIASHARP_VERSION',
    'SYSTEM_TEAMPROJECT',
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
Assert-Equal (Get-LastVariableValue $githubPr.Output 'BUILD_NUMBER') '26418.3' 'GitHub PR build number'
Assert-Equal (Get-LastVariableValue $githubPr.Output 'DOTNET_FINAL_VERSION_KIND') '' 'GitHub PR final version kind'
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

$testResource = Invoke-BuildIdentityCase 'Tests inherit Package identity' @{
    BUILD_REASON = 'Manual'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.22+main'
}
Assert-Equal (Get-LastVariableValue $testResource.Output 'BUILD_NUMBER') '22' 'Tests build number'
Assert-BuildLabel $testResource.Output '4.152.0-preview.0.22+main'

$main = Invoke-BuildIdentityCase 'GitHub main' @{
    BUILD_REASON = 'IndividualCI'
}
Assert-Equal (Get-LastVariableValue $main.Output 'PREVIEW_LABEL') 'preview.0' 'Main preview label'
Assert-Equal (Get-LastVariableValue $main.Output 'BUILD_NUMBER') '26418.3' 'Main build number'
Assert-BuildLabel $main.Output '4.152.0-preview.0.26418.3+main'

$resourceMain = Invoke-BuildIdentityCase 'Main resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0-preview.0.26418.3+main'
}
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'PREVIEW_LABEL') 'preview.0' 'Resource main label'
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'BUILD_NUMBER') '26418.3' 'Resource main build number'
Assert-BuildLabel $resourceMain.Output '4.152.0-preview.0.26418.3+main'

$release = Invoke-BuildIdentityCase 'Exact release' @{
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/release/4.152.0'
    BUILD_SOURCEBRANCHNAME = '4.152.0'
    PREVIEW_LABEL = 'Stable'
}
Assert-Equal (Get-LastVariableValue $release.Output 'PREVIEW_LABEL') 'stable' 'Release normalized label'
Assert-Equal (Get-LastVariableValue $release.Output 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release final version kind'
Assert-BuildLabel $release.Output '4.152.0+20260818.3'

$mixedCasePreview = Invoke-BuildIdentityCase 'Mixed-case preview label' @{
    BUILD_REASON = 'Manual'
    PREVIEW_LABEL = 'Preview.7'
}
Assert-Equal (Get-LastVariableValue $mixedCasePreview.Output 'PREVIEW_LABEL') 'preview.7' 'Normalized preview label'
Assert-BuildLabel $mixedCasePreview.Output '4.152.0-preview.7.26418.3+main'

$automaticRelease = Invoke-BuildIdentityCase 'Automatic exact release' @{
    BUILD_REASON = 'IndividualCI'
    PREVIEW_LABEL = 'stable'
} -ExpectFailure
if ($automaticRelease.Output -notmatch 'Exact release packages require') {
    throw "Automatic release failed for the wrong reason.`n$($automaticRelease.Output)"
}

$releaseResource = Invoke-BuildIdentityCase 'Release resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.152.0+20260818.3'
}
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'PREVIEW_LABEL') 'stable' 'Release resource label'
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release resource final version kind'
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'BUILD_NUMBER') '26418.3' 'Release resource build number'
Assert-BuildLabel $releaseResource.Output '4.152.0+20260818.3'

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
    $globalJson.sdk.rollForward -cne 'latestPatch' -or
    $toolSdk -ne $buildSdk -or
    $buildSdk.Major -ne 10 -or
    $buildSdk.Build -lt 400 -or
    $variablesYaml -notmatch "DOTNET_WORKLOAD_VERSION:\s*'$([regex]::Escape($globalJson.sdk.version))'" -or
    $variablesYaml -match 'DOTNET_(GLOBAL_)?INSTALL_DIR') {
    throw 'SkiaSharp, workloads, and Arcade must use the same stable .NET 10.0.4xx SDK.'
}

$packageStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-package.yml') -Raw
if ($packageStages -match 'packStableNuGets') {
    throw 'The Package stage must not select a second stable package variant.'
}
if ($packageStages -notmatch '--dotNetFinalVersionKind=\"\$\(DOTNET_FINAL_VERSION_KIND\)\"') {
    throw 'The Package stage must forward the derived Arcade final version kind.'
}

$packagePipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-package.yml') -Raw
if ($packagePipeline -match 'source:\s*skiasharp-native') {
    throw 'The combined Package pipeline must not inherit identity from a Native pipeline resource.'
}
if ($packagePipeline -notmatch "buildPipelineType:\s*'build'") {
    throw "The internal Package pipeline must use the 'build' role."
}
if ($packagePipeline -notmatch 'forceRealSigning:\s*\$\{\{\s*parameters\.forceRealSigning\s*\}\}' -or
    $packagePipeline -notmatch 'registerInBar:\s*\$\{\{\s*parameters\.registerInBar\s*\}\}' -or
    $packagePipeline -notmatch 'runApiScan:\s*\$\{\{\s*parameters\.runApiScan\s*\}\}' -or
    $packagePipeline -match '- name:\s*previewLabel' -or
    $packagePipeline -match '(?m)^\s*-\s+stage:') {
    throw 'The Package root must delegate all stages and policy parameters to the shared composer.'
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
$stagesComposer = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages.yml') -Raw
$publishStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-publish.yml') -Raw
if ($stagesComposer -notmatch '/scripts/azure-templates-stages-signing\.yml@self' -or
    $stagesComposer -notmatch '/scripts/azure-templates-stages-apiscan\.yml@self' -or
    $stagesComposer -match '/scripts/azure-templates-jobs-(signing|apiscan)\.yml@self') {
    throw 'The shared composer must consume signing and API Scan as stage templates.'
}
if ($stagesComposer -notmatch '/scripts/azure-templates-stages-publish\.yml@self') {
    throw 'The shared stage composer must delegate BAR registration and validation to the publish stage template.'
}
if ($publishStages -notmatch 'publishingVersion:\s*3\s+officialBuildId:\s*\$\(ARCADE_OFFICIAL_BUILD_ID\)') {
    throw 'BAR registration must use the same Arcade OfficialBuildId as manifest generation.'
}
if ($publishStages -notmatch 'publishAssetsImmediately:\s*true' -or
    $publishStages -match 'requireDefaultChannels:\s*true') {
    throw 'Package CI must validate BAR assets without automatic channel promotion.'
}
if ($packagePipeline -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN' -or
    $stagesComposer -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN' -or
    $publishStages -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN') {
    throw 'The combined Package pipeline must use its parameters directly instead of mirrored variables.'
}
if ($variablesYaml -notmatch "PREVIEW_LABEL:\s*'preview\.0'" -or
    $packagePipeline -match '- name:\s*previewLabel') {
    throw 'PREVIEW_LABEL must remain source-controlled in the shared variables template.'
}
if ($stagesComposer -match 'PREVIEW_LABEL') {
    throw 'Signing, API Scan, and BAR eligibility must not depend on the package version label.'
}
if (-not $stagesComposer.Contains("and(eq(parameters.forceRealSigning, 'true'), eq(parameters.registerInBar, 'true'))")) {
    throw 'Forced real signing must require a separate opt-in before BAR registration.'
}
if (-not $stagesComposer.Contains("eq(parameters.runApiScan, 'true')") -or
    -not $stagesComposer.Contains("eq(variables['Build.SourceBranch'], 'refs/heads/main')") -or
    -not $stagesComposer.Contains("eq(variables['Build.Reason'], 'Schedule')")) {
    throw 'API Scan must run only when requested or on scheduled main builds.'
}
if ($publishStages -match 'includeApiScan|api_scan') {
    throw 'API Scan must remain independent from BAR registration and validation.'
}

$packageScript = Get-Content (Join-Path $repoRoot 'scripts/infra/package/nuget.cake') -Raw
$normalTask = $packageScript.Substring(0, $packageScript.IndexOf('Task ("nuget-special")'))
if ($normalTask -match 'PACK_STABLE_NUGETS|packStableNuGets' -or
    $normalTask -notmatch '\{\s*"VersionSuffix",\s*PREVIEW_NUGET_SUFFIX\s*\}' -or
    ([regex]::Matches($normalTask, 'RunDotNetPack\s*\(').Count -ne 1)) {
    throw 'nuget-normal must pack exactly one version family selected by VersionSuffix.'
}

$publishingProps = Get-Content (Join-Path $repoRoot 'eng/Publishing.props') -Raw
if (-not $publishingProps.Contains('$(ArtifactsPackagesDir)Preview\*.nupkg') -or
    -not $publishingProps.Contains('$(ArtifactsShippingPackagesDir)**\*.nupkg') -or
    ([regex]::Matches($publishingProps, 'DotNetFinalVersionKind').Count -lt 2)) {
    throw 'Arcade publishing must separate preview and exact release package views.'
}

Write-Host 'Build identity tests passed.'
