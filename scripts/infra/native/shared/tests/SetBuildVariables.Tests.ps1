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
        SKIASHARP_VERSION = '4.150.4'
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
Assert-BuildLabel $githubPr.Output '4.150.4-pr.4803.26418.3'

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
Assert-BuildLabel $azurePr.Output '4.150.4-pr.63954.26418.3'

$resourcePr = Invoke-BuildIdentityCase 'PR resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    BUILD_REPOSITORY_PROVIDER = 'TfsGit'
    BUILD_REPOSITORY_URI = 'https://dev.azure.com/dnceng/internal/_git/dotnet-SkiaSharp'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    BUILD_SOURCEBRANCHNAME = 'merge'
    BUILD_SOURCEVERSION = 'ffffffffffffffffffffffffffffffffffffffff'
    BUILD_SOURCEVERSIONMESSAGE = 'Merge 1111111111111111111111111111111111111111 into 2222222222222222222222222222222222222222'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.150.4-pr.63954.26418.3'
}
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'PREVIEW_LABEL') 'pr.63954' 'Resource PR label'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'BUILD_NUMBER') '26418.3' 'Resource build number'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'BUILD_COUNTER') '26418.3' 'Resource build counter'
Assert-Equal (Get-LastVariableValue $resourcePr.Output 'GIT_SHA') '1111111111111111111111111111111111111111' 'Resource source commit'
Assert-BuildLabel $resourcePr.Output '4.150.4-pr.63954.26418.3'

$testResource = Invoke-BuildIdentityCase 'Tests inherit Package identity' @{
    BUILD_REASON = 'Manual'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.150.4-preview.0.22+main'
}
Assert-Equal (Get-LastVariableValue $testResource.Output 'BUILD_NUMBER') '22' 'Tests build number'
Assert-BuildLabel $testResource.Output '4.150.4-preview.0.22+main'

$main = Invoke-BuildIdentityCase 'GitHub main' @{
    BUILD_REASON = 'IndividualCI'
}
Assert-Equal (Get-LastVariableValue $main.Output 'PREVIEW_LABEL') 'preview.0' 'Main preview label'
Assert-Equal (Get-LastVariableValue $main.Output 'BUILD_NUMBER') '26418.3' 'Main build number'
Assert-BuildLabel $main.Output '4.150.4-preview.0.26418.3+main'

$resourceMain = Invoke-BuildIdentityCase 'Main resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.150.4-preview.0.26418.3+main'
}
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'PREVIEW_LABEL') 'preview.0' 'Resource main label'
Assert-Equal (Get-LastVariableValue $resourceMain.Output 'BUILD_NUMBER') '26418.3' 'Resource main build number'
Assert-BuildLabel $resourceMain.Output '4.150.4-preview.0.26418.3+main'

$release = Invoke-BuildIdentityCase 'Exact release' @{
    BUILD_REASON = 'IndividualCI'
    BUILD_SOURCEBRANCH = 'refs/heads/release/4.150.x'
    BUILD_SOURCEBRANCHNAME = '4.150.x'
    PREVIEW_LABEL = 'Stable'
}
Assert-Equal (Get-LastVariableValue $release.Output 'PREVIEW_LABEL') 'stable' 'Release normalized label'
Assert-Equal (Get-LastVariableValue $release.Output 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release final version kind'
Assert-BuildLabel $release.Output '4.150.4+20260818.3'

$mixedCasePreview = Invoke-BuildIdentityCase 'Mixed-case preview label' @{
    BUILD_REASON = 'Manual'
    PREVIEW_LABEL = 'Preview.7'
}
Assert-Equal (Get-LastVariableValue $mixedCasePreview.Output 'PREVIEW_LABEL') 'preview.7' 'Normalized preview label'
Assert-BuildLabel $mixedCasePreview.Output '4.150.4-preview.7.26418.3+main'

$automaticRelease = Invoke-BuildIdentityCase 'Automatic exact release' @{
    BUILD_REASON = 'IndividualCI'
    PREVIEW_LABEL = 'stable'
} -ExpectFailure
if ($automaticRelease.Output -notmatch 'Exact release packages require') {
    throw "Automatic release failed for the wrong reason.`n$($automaticRelease.Output)"
}

$releaseResource = Invoke-BuildIdentityCase 'Release resource trigger' @{
    BUILD_REASON = 'ResourceTrigger'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.150.4+20260818.3'
}
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'PREVIEW_LABEL') 'stable' 'Release resource label'
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'DOTNET_FINAL_VERSION_KIND') 'release' 'Release resource final version kind'
Assert-Equal (Get-LastVariableValue $releaseResource.Output 'BUILD_NUMBER') '26418.3' 'Release resource build number'
Assert-BuildLabel $releaseResource.Output '4.150.4+20260818.3'

$malformed = Invoke-BuildIdentityCase 'Malformed resource identity' @{
    BUILD_REASON = 'ResourceTrigger'
    BUILD_SOURCEBRANCH = 'refs/pull/63954/merge'
    RESOURCES_PIPELINE_SKIASHARP_RUNNAME = '4.150.4-pr..8'
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
$workloadSet = [regex]::Match(
    $variablesYaml,
    "DOTNET_WORKLOAD_VERSION:\s*'(?<version>[^']+)'").Groups['version'].Value
$buildSdk = [Version]$globalJson.sdk.version
$toolSdk = [Version]$globalJson.tools.dotnet
if ($pipelineSdk -cne $globalJson.sdk.version -or
    $globalJson.sdk.allowPrerelease -ne $false -or
    $globalJson.sdk.rollForward -cne 'latestPatch' -or
    $toolSdk -ne $buildSdk -or
    $buildSdk -ne [Version]'10.0.203' -or
    $workloadSet -cne '10.0.202' -or
    $variablesYaml -match 'DOTNET_(GLOBAL_)?INSTALL_DIR') {
    throw 'SkiaSharp and Arcade must use SDK 10.0.203 while workloads stay on the Xcode 26.3-compatible 10.0.202 set.'
}
if ($variablesYaml -notmatch "XCODE_VERSION:\s*'26\.3'") {
    throw 'Managed Apple builds must use Xcode 26.3 for the .NET 10.0.202 workload set.'
}

$winuiGlobalJson = Get-Content (Join-Path $repoRoot 'native/winui/global.json') -Raw | ConvertFrom-Json
$winuiSdk = [regex]::Match(
    $variablesYaml,
    "DOTNET_VERSION_WINUI:\s*'(?<version>[^']+)'").Groups['version'].Value
if ($winuiGlobalJson.sdk.version -cne $winuiSdk -or
    $winuiGlobalJson.sdk.allowPrerelease -ne $false -or
    $winuiGlobalJson.sdk.rollForward -cne 'latestPatch' -or
    ([Version]$winuiSdk).Build -ge 200) {
    throw 'The WinUI native build must stay on its VS MSBuild 17-compatible .NET 10.0.1xx feature band.'
}

$nativeDockerfiles = @(
    'scripts/infra/native/linux/docker/alpine/Dockerfile'
    'scripts/infra/native/linux/docker/bionic/Dockerfile'
    'scripts/infra/native/linux/docker/glibc/Dockerfile'
    'scripts/infra/native/linux/docker/glibc-x86/Dockerfile'
    'scripts/infra/native/tizen/docker/Dockerfile'
    'scripts/infra/native/wasm/docker/Dockerfile'
)
foreach ($dockerfile in $nativeDockerfiles) {
    $contents = Get-Content (Join-Path $repoRoot $dockerfile) -Raw
    $dockerSdk = [regex]::Match(
        $contents,
        '(?m)^ARG DOTNET_SDK_VERSION=(?<version>[^\r\n]+)\r?$').Groups['version'].Value.Trim()
    if ($dockerSdk -cne $globalJson.sdk.version) {
        throw "$dockerfile must use the repository .NET SDK version."
    }

    $sdkContainerImages = @{
        'scripts/infra/docs/docker/Dockerfile' = '10.0.203'
    }
    foreach ($container in $sdkContainerImages.GetEnumerator()) {
        $contents = Get-Content (Join-Path $repoRoot $container.Key) -Raw
        if ($contents -notmatch "FROM\s+mcr\.microsoft\.com/dotnet/sdk:$([regex]::Escape($container.Value))(\s|$)") {
            throw "$($container.Key) must pin the expected repository-compatible SDK image."
        }
    }
}

$crlfDockerSdk = [regex]::Match(
    "FROM test`r`nARG DOTNET_SDK_VERSION=$($globalJson.sdk.version)`r`n",
    '(?m)^ARG DOTNET_SDK_VERSION=(?<version>[^\r\n]+)\r?$').Groups['version'].Value.Trim()
if ($crlfDockerSdk -cne $globalJson.sdk.version) {
    throw 'Native Docker SDK validation must support Windows CRLF checkouts.'
}

$nativeWindowsStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-native-windows.yml') -Raw
$winuiSdkInstalls = [regex]::Matches(
    $nativeWindowsStages,
    '-\s*\$\(DOTNET_VERSION_WINUI\)').Count
if ($winuiSdkInstalls -ne 6 -or
    $nativeWindowsStages -match 'version:\s*\$\(DOTNET_VERSION_WINUI\)') {
    throw 'Every WinUI native job must install the VS MSBuild-compatible SDK side-by-side.'
}

$packageStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-package.yml') -Raw
if ($packageStages -match 'packStableNuGets') {
    throw 'The Package stage must not select a second stable package variant.'
}
if ($packageStages -match '--dotNetFinalVersionKind') {
    throw 'The Package stage must not pass an empty final version kind to Cake.'
}
if ($packageStages -match 'nuget_preview|nugets-preview') {
    throw 'The Package stage must publish only the single-family nuget artifact.'
}
if ($packageStages -match 'nuget_symbols|nugets-symbols') {
    throw 'Normal and symbol packages must share the single nuget pipeline artifact.'
}
if ($packageStages -match 'package_special_windows|target:\s*nuget-special|name:\s*package_windows\s+path:' -or
    $packageStages -match 'enableCaching|cacheJob|CACHE_SKIP|Re-organize the output folder' -or
    $packageStages -notmatch 'target:\s*nuget(\s|$)' -or
    $packageStages -notmatch 'name:\s*nuget_special' -or
    $packageStages -notmatch 'name:\s*arcade_shipping' -or
    $packageStages -notmatch 'name:\s*arcade_nonshipping' -or
    $packageStages -notmatch 'name:\s*PdbArtifacts' -or
    $packageStages -notmatch "isProduction:\s*'false'" -or
    $packageStages -notmatch 'postBuildSteps:(?s).*AssembleArcadeAssets\.Tests\.ps1') {
    throw 'One uncached Package job must publish raw and prepared Arcade package artifacts.'
}
$prepareStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-prepare.yml') -Raw
if ($prepareStages -match 'UseDotNet@2|dotnet tool restore|AssembleArcadeAssets\.Tests\.ps1') {
    throw 'Prepare validation must remain tool-free; Cake assembly tests run in Package.'
}
$sharedCake = Get-Content (Join-Path $repoRoot 'scripts/infra/shared/shared.cake') -Raw
if ($sharedCake -notmatch 'EnvironmentVariable\s*\(\s*"DOTNET_FINAL_VERSION_KIND"\s*\)') {
    throw 'Cake must read the derived Arcade final version kind from the environment.'
}
if ($sharedCake -notmatch '_packaging/skiasharp-transport/nuget/v3/index\.json' -or
    $sharedCake -match '_packaging/skiasharp-ci/') {
    throw 'Build-transfer packages must resolve from the future SkiaSharp transport feed.'
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
if ($packagePipeline -match 'networkIsolationPolicy' -or
    $packagePipeline -notmatch 'networkIsolationMode:\s*Audit' -or
    $packagePipeline -match 'auditNetworkIsolation') {
    throw 'Network isolation must temporarily audit the default 1ES policy without a queue-time bypass or custom policy.'
}

$apiScanStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-apiscan.yml') -Raw
if ($apiScanStages -match 'SRV\*http://symweb' -or
    $apiScanStages -notmatch 'SRV\*https://symweb') {
    throw 'API Scan and its surrogate configuration must use the HTTPS symbol endpoint.'
}

$testsPipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-tests.yml') -Raw
if ($testsPipeline -notmatch "source:\s*'\\dotnet\\skiasharp\\skiasharp-package'" -or
    $testsPipeline -notmatch "buildPipelineType:\s*'test'") {
    throw 'The separate Tests pipeline must inherit the final Package identity using its folder-qualified Azure pipeline name.'
}
$testStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-test.yml') -Raw
if ($testStages -match 'isSplitPipeline' -or
    $testStages -notmatch "eq\(parameters\.buildPipelineType,\s*'test'\)") {
    throw 'Every split Tests stage must depend on prepare rather than the absent package stage.'
}

$completePipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-complete.yml') -Raw
if ($completePipeline -notmatch "buildPipelineType:\s*'complete'") {
    throw "The public pipeline must use the 'complete' role."
}
$nativePipeline = Get-Content (Join-Path $repoRoot 'scripts/azure-pipelines-native.yml') -Raw
$stagesComposer = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages.yml') -Raw
if ($nativePipeline -notmatch "buildPipelineType:\s*'native'" -or
    -not $stagesComposer.Contains("in(parameters.buildPipelineType, 'native', 'build', 'complete')")) {
    throw "The historical Native pipeline must retain its native-only stage role."
}

foreach ($pipelineContents in @($completePipeline, $packagePipeline, $testsPipeline)) {
    $managedMacBlock = [regex]::Match(
        $pipelineContents,
        '(?ms)^\s*- name:\s*buildAgentMac\s*$.*?(?=^\s*- name:)').Value
    if ($managedMacBlock -notmatch 'name:\s*Azure Pipelines' -or
        $managedMacBlock -notmatch 'vmImage:\s*macos-15' -or
        $managedMacBlock -match 'GitHub-hosted Agents|macos-26') {
        throw 'Managed Apple builds must use the Azure Pipelines macos-15 image.'
    }
}

$packageStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-package.yml') -Raw
if ($packageStages -match 'Re-upload Native Artifacts') {
    throw 'The Package stage must consume Native artifacts from the same pipeline run.'
}
$signingStages = Get-Content (Join-Path $repoRoot 'scripts/azure-templates-stages-signing.yml') -Raw
if ($stagesComposer -notmatch '/scripts/azure-templates-stages-signing\.yml@self' -or
    $stagesComposer -notmatch '/scripts/azure-templates-stages-apiscan\.yml@self' -or
    $stagesComposer -match '/scripts/azure-templates-jobs-(signing|apiscan)\.yml@self') {
    throw 'The shared composer must consume signing and API Scan as stage templates.'
}
if ($stagesComposer -match '/scripts/azure-templates-stages-publish\.yml@self') {
    throw 'Signing and BAR registration must remain in the shared historical-line stage template.'
}
if ($signingStages -notmatch 'publishingVersion:\s*3\s+officialBuildId:\s*\$\(ARCADE_OFFICIAL_BUILD_ID\)') {
    throw 'BAR registration must use the same Arcade OfficialBuildId as manifest generation.'
}
if ($signingStages -notmatch 'publishAssetsImmediately:\s*false' -or
    $signingStages -notmatch 'requireDefaultChannels:\s*true' -or
    $signingStages -match 'General Testing|promote-build\.ps1|stage:\s*promote_build') {
    throw 'Package CI must use Arcade default-channel promotion and fail closed when no mapping exists.'
}
if ($packagePipeline -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN' -or
    $stagesComposer -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN' -or
    $signingStages -match 'PACKAGE_PIPELINE|PACKAGE_FORCE_REAL_SIGNING|PACKAGE_RUN_API_SCAN') {
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
if ($signingStages -match 'includeApiScan|api_scan') {
    throw 'API Scan must remain independent from BAR registration and validation.'
}

$packageScript = Get-Content (Join-Path $repoRoot 'scripts/infra/package/nuget.cake') -Raw
$transportProject = Get-Content (Join-Path $repoRoot 'scripts/infra/package/nuget/NuGet.csproj') -Raw
if ($transportProject -notmatch '<IsShippingPackage>false</IsShippingPackage>') {
    throw 'The special-package project must declare its NuGets as non-shipping.'
}
$normalTask = $packageScript.Substring(0, $packageScript.IndexOf('Task ("nuget-special")'))
if ($normalTask -match 'PACK_STABLE_NUGETS|packStableNuGets' -or
    $normalTask -notmatch '\{\s*"VersionSuffix",\s*PREVIEW_NUGET_SUFFIX\s*\}' -or
    $normalTask -notmatch 'DeleteFiles\s*\(\s*\$"\{OUTPUT_NUGETS_PATH\}/\*\.nupkg"\s*\)' -or
    ([regex]::Matches($normalTask, 'RunDotNetPack\s*\(').Count -ne 1)) {
    throw 'nuget-normal must clean and pack exactly one version family selected by VersionSuffix.'
}
if ($packageScript -match 'Id\s*=\s*"_(NuGetsPreview|Symbols)' -or
    $packageScript -match 'IsPreview' -or
    $packageScript -notmatch 'Id = "_NuGets"') {
    throw 'Special package transfer must use only _NuGets for the single package family.'
}
if ($packageScript -match 'versions\.Add\s*\(\s*"commit"' -or
    $packageScript -match 'transportVersionKind|GIT_SHA') {
    throw 'Production packaging must emit and assemble only the branch or PR transport family.'
}
if ($packageScript -match 'MoveFiles\s*\(.+\\.symbols\\.nupkg' -or
    $packageScript -match 'OUTPUT_SYMBOLS_NUGETS_PATH') {
    throw 'NuGet symbol packages must remain beside normal packages in the unified artifact.'
}

$samplesScript = Get-Content (Join-Path $repoRoot 'scripts/infra/samples/samples.cake') -Raw
$docsScript = Get-Content (Join-Path $repoRoot 'scripts/infra/docs/docs.cake') -Raw
if ($samplesScript -notmatch 'actualSamples\s*=\s*string\.IsNullOrEmpty\s*\(PREVIEW_NUGET_SUFFIX\)' -or
    $docsScript -notmatch 'localNugetVersion\s*=\s*string\.IsNullOrEmpty\s*\(PREVIEW_NUGET_SUFFIX\)' -or
    $samplesScript -match 'PREVIEW_ONLY_NUGETS' -or
    $docsScript -match 'PREVIEW_ONLY_NUGETS' -or
    $sharedCake -match 'PREVIEW_ONLY_NUGETS') {
    throw 'Samples and docs must consume the single package family selected by PREVIEW_NUGET_SUFFIX.'
}
if ($docsScript -match '_nugetspreview' -or
    $docsScript -notmatch 'DownloadPackageAsync\s*\(\s*"_nugets"' -or
    $docsScript -match 'GIT_SHA') {
    throw 'Docs must download the branch or PR single-family _NuGets transfer package.'
}

$publishMarker = $signingStages.IndexOf('- ${{ if eq(parameters.publishAssets, true) }}:')
if ($publishMarker -lt 0) {
    throw 'Signing must conditionally compile Arcade assembly and BAR registration jobs.'
}
$signOnly = $signingStages.Substring(0, $publishMarker)
if ($signOnly -match 'nuget_signed|nuget_special|transport-nugets|NonShipping|nuget-assemble-arcade-assets|-publish' -or
    $signOnly -notmatch 'artifactName:\s*arcade_shipping' -or
    $signOnly -notmatch 'artifactName:\s*arcade_shipping_signed') {
    throw 'Signing must consume and publish only the prepared Arcade Shipping artifact.'
}
if ($signOnly -match 'artifactName:\s*nuget_symbols|stage-android-symbol-packages\.ps1') {
    throw 'Signing must consume normal and symbol packages from the unified nuget artifact.'
}

$publishingProps = Get-Content (Join-Path $repoRoot 'eng/Publishing.props') -Raw
if (-not $publishingProps.Contains('$(ArtifactsShippingPackagesDir)**\*.nupkg') -or
    -not $publishingProps.Contains('$(ArtifactsNonShippingPackagesDir)**\*.nupkg') -or
    $publishingProps -match 'Preview|DotNetFinalVersionKind' -or
    $publishingProps -notmatch 'IsShipping="false"' -or
    $publishingProps -notmatch '<AutoGenerateSymbolPackages>false</AutoGenerateSymbolPackages>') {
    throw 'Arcade publishing must use one shipping view and one non-shipping transport view.'
}
$buildCake = Get-Content (Join-Path $repoRoot 'build.cake') -Raw
if ($packageScript -notmatch 'Task\s*\(\s*"nuget-assemble-arcade-assets"\s*\)' -or
    $buildCake -notmatch '\.IsDependentOn\s*\(\s*"nuget-assemble-arcade-assets"\s*\)' -or
    $packageStages -notmatch "path:\s*'.\\output\\arcade-assets\\Shipping'" -or
    $packageStages -notmatch "path:\s*'.\\output\\arcade-assets\\NonShipping'" -or
    $packageStages -notmatch "path:\s*'.\\output\\pdbs'" -or
    $packageStages -notmatch "isProduction:\s*'false'") {
    throw 'The Package graph must prepare and publish deterministic public Arcade asset views.'
}
$publishOnly = $signingStages.Substring($publishMarker)
if ($publishOnly -notmatch 'stage:\s*publish_assets' -or
    $publishOnly -notmatch 'dependsOn:\s*signing' -or
    $publishOnly -notmatch 'artifactName:\s*arcade_shipping_signed' -or
    $publishOnly -notmatch 'artifactName:\s*arcade_nonshipping' -or
    $publishOnly -notmatch 'publish-build-assets\.yml@self' -or
    $publishOnly -notmatch 'validateDependsOn:\s+- publish_assets') {
    throw 'BAR publication must consume signed Shipping and prepared NonShipping in a separate stage.'
}
if ($signingStages -match 'assemble-arcade-assets\.ps1|artifactName:\s*PdbArtifacts') {
    throw 'Authenticated signing must not reassemble assets or publish PDB artifacts.'
}
if ($sharedCake -notmatch 'ROOT_OUTPUT_PATH' -or
    $sharedCake -notmatch 'OUTPUT_ARCADE_ASSETS_PATH' -or
    $sharedCake -notmatch 'OUTPUT_PDB_ARTIFACTS_PATH') {
    throw 'Canonical output constants must define the public Arcade asset roots.'
}

Write-Host 'Build identity and pipeline contract tests passed.'
