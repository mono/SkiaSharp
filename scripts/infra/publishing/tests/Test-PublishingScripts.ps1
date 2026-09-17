#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$publishingRoot = Split-Path $PSScriptRoot
$gitCommonPath = Join-Path $publishingRoot 'Git.Common.psm1'
$gitHubCommonPath = Join-Path $publishingRoot 'GitHub.Common.psm1'
$azureDevOpsCommonPath = Join-Path $publishingRoot 'AzureDevOps.Common.psm1'
$commonPath = Join-Path $publishingRoot 'Publishing.Common.psm1'
$preparePath = Join-Path $publishingRoot 'prepare-release.ps1'
$finishPath = Join-Path $publishingRoot 'finish-release.ps1'
$bugTemplatePath = Join-Path $publishingRoot 'update-bug-template.ps1'
$reconcilePath = Join-Path $publishingRoot 'reconcile-release-assignments.ps1'
$milestonesPath = Join-Path $publishingRoot 'update-release-milestones.ps1'
$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../../../..')
$prepareWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-prepare.yml'
$finishWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-finish.yml'
$milestonesWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-milestones.yml'

Import-Module $gitCommonPath -Force
Import-Module $gitHubCommonPath -Force
Import-Module $azureDevOpsCommonPath -Force
Import-Module $commonPath -Force
$script:TestsRun = 0

# Requires two values to be equal.
function Assert-Equal([object] $Expected, [object] $Actual, [string] $Message) {
    $script:TestsRun++
    $expectedJson = ConvertTo-Json @($Expected) -Compress -Depth 20
    $actualJson = ConvertTo-Json @($Actual) -Compress -Depth 20
    if ($expectedJson -ne $actualJson) {
        throw "$Message`nExpected: $expectedJson`nActual:   $actualJson"
    }
}

# Requires a condition to be true.
function Assert-True([bool] $Condition, [string] $Message) {
    $script:TestsRun++
    if (!$Condition) {
        throw $Message
    }
}

# Requires a script block to throw a matching error.
function Assert-Throws([scriptblock] $Action, [string] $Pattern, [string] $Message) {
    $script:TestsRun++
    try {
        & $Action
    } catch {
        if ($_.Exception.Message -notmatch $Pattern) {
            throw "$Message`nUnexpected error: $($_.Exception.Message)"
        }
        return
    }
    throw "$Message`nNo error was thrown."
}

# Requires a script to reject an unsupported Apply switch before execution.
function Assert-RejectsApply([string] $Path, [string[]] $ScriptArguments) {
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& pwsh -NoLogo -NoProfile -File $Path @ScriptArguments -Apply 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    Assert-True ($exitCode -ne 0 -and ($output -join "`n") -match 'parameter name .Apply') `
        "$([IO.Path]::GetFileName($Path)) did not reject Apply before execution."
}

# Loads top-level function definitions without executing a script's main flow.
function Get-ScriptFunctionText([string] $Path) {
    $tokens = $null
    $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile(
        (Resolve-Path $Path),
        [ref] $tokens,
        [ref] $errors)
    if ($errors.Count) {
        throw "Could not parse $Path."
    }
    return (
        $ast.FindAll(
            { param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] },
            $false) |
            ForEach-Object { $_.Extent.Text }
    ) -join "`n`n"
}

# Verifies each script exposes only its intended mutation switches.
$prepareParameters = (Get-Command $preparePath).Parameters.Keys
$finishParameters = (Get-Command $finishPath).Parameters.Keys
$bugTemplateParameters = (Get-Command $bugTemplatePath).Parameters.Keys
$reconcileParameters = (Get-Command $reconcilePath).Parameters.Keys
$milestoneParameters = (Get-Command $milestonesPath).Parameters.Keys
Assert-True ($prepareParameters -contains 'Mode' -and
    $prepareParameters -notcontains 'Apply' -and $prepareParameters -notcontains 'Push') `
    'Prepare must expose only the three-state Mode parameter.'
Assert-True ($finishParameters -contains 'Mode' -and
    $finishParameters -notcontains 'Apply' -and $finishParameters -notcontains 'Push') `
    'Finish must expose only the three-state Mode parameter.'
Assert-True ($bugTemplateParameters -contains 'Mode' -and
    $bugTemplateParameters -notcontains 'Apply' -and $bugTemplateParameters -notcontains 'Push') `
    'The bug-template updater must expose only the three-state Mode parameter.'
Assert-True ($reconcileParameters -contains 'Version' -and $reconcileParameters -contains 'Push' -and
    $reconcileParameters -notcontains 'Apply') 'Assignment reconciliation must expose Version and Push but not Apply.'
Assert-True ($milestoneParameters -contains 'Count' -and $milestoneParameters -contains 'Push' -and
    $milestoneParameters -notcontains 'Apply' -and $milestoneParameters -notcontains 'Version') `
    'The milestone updater must expose Count and Push but not Apply or Version.'
Assert-RejectsApply $reconcilePath @('-Version', '4.152.0')
Assert-RejectsApply $milestonesPath @()
foreach ($workflowPath in @($prepareWorkflowPath, $finishWorkflowPath)) {
    $workflow = Get-Content $workflowPath -Raw
    $workflowName = [IO.Path]::GetFileName($workflowPath)
    Assert-True ($workflow -notmatch '(?m)^      mode:\s*$') `
        "$workflowName still exposes the disposable three-state mode input."
    Assert-True ($workflow -match '(?ms)^      push:\r?\n(?:        .*\r?\n)+?        default: false\r?\n        type: boolean\s*$') `
        "$workflowName does not expose an unchecked boolean push input."
    Assert-True ($workflow.Contains("MODE: `${{ inputs.push && 'Push' || 'DryRun' }}")) `
        "$workflowName does not map its push checkbox to DryRun or Push."
}
$finishWorkflow = Get-Content $finishWorkflowPath -Raw
$milestonesWorkflow = Get-Content $milestonesWorkflowPath -Raw
Assert-True ($finishWorkflow -match '(?ms)outputs:\s+release_version:.*release_numeric:.*release_tag:.*source_commit:') `
    'Finish does not expose its resolved-release output contract.'
Assert-True ($finishWorkflow -match '(?ms)milestones:\s+needs: finish\s+uses: \./\.github/workflows/release-milestones\.yml') `
    'Finish does not require the shared milestone workflow after publication planning.'
Assert-True ($finishWorkflow -match '(?ms)milestones:.*version: \$\{\{ needs\.finish\.outputs\.release_numeric \}\}.*tag: \$\{\{ needs\.finish\.outputs\.release_tag \}\}.*source_commit: \$\{\{ needs\.finish\.outputs\.source_commit \}\}.*reconcile: true.*update: true.*push: \$\{\{ inputs\.push \}\}') `
    'Finish does not pass the complete resolved shipment to milestone maintenance.'
Assert-True ($finishWorkflow -match '(?ms)milestones:.*permissions:.*contents: read.*issues: write.*pull-requests: write') `
    'Finish does not grant the called milestone workflow its required permissions.'
Assert-True ($finishWorkflow -match '(?ms)^concurrency:\s+group: release-state\s+cancel-in-progress: false') `
    'Finish does not serialize publication through milestone completion.'
Assert-True ($finishWorkflow -notmatch 'group: release-\$\{\{ inputs\.version \}\}') `
    'Finish still permits different release inputs to publish outside the global release-state lock.'
Assert-True ($milestonesWorkflow -match '(?ms)workflow_call:.*inputs:.*version:.*tag:.*source_commit:.*reconcile:.*update:.*push:') `
    'Milestone maintenance cannot be called with the resolved shipment contract.'
Assert-True ($milestonesWorkflow -match '(?ms)workflow_dispatch:.*inputs:.*version:.*reconcile:.*update:.*push:') `
    'Standalone milestone dispatch no longer preserves its independent operation toggles.'
Assert-True ($milestonesWorkflow -match
    '(?ms)^concurrency:\s+group: \$\{\{ inputs\.source_commit.*release-milestones-called-\{0\}.*release-state.*\}\}\s+cancel-in-progress: false') `
    'Milestone maintenance does not share standalone serialization without deadlocking a Finish caller.'
$milestonesDispatchInputs = [regex]::Match(
    $milestonesWorkflow,
    '(?ms)  workflow_dispatch:\s*(?<block>.*?)\npermissions:').Groups['block'].Value
Assert-True ($milestonesDispatchInputs -notmatch
    '(?m)^\s{6}(?:tag|source_commit):') `
    'Standalone milestone dispatch unexpectedly exposes Finish-only virtual shipment inputs.'
Assert-True ($milestonesWorkflow -match '(?ms)Reconcile release assignments.*Update release milestones') `
    'Milestone reconciliation no longer runs before date/rollover maintenance.'
$milestoneScripts = (Get-Content $reconcilePath -Raw) + (Get-Content $milestonesPath -Raw)
Assert-True ($milestoneScripts -match '(?ms)PlannedTag.*PlannedCommit.*Get-ReleaseShipmentContract.*-RequireTag:\$Push') `
    'Milestone maintenance does not validate planned shipment parity before mutation.'
$bugTemplateScript = Get-Content $bugTemplatePath -Raw
$gitCommonScript = Get-Content $gitCommonPath -Raw
$commonScript = Get-Content $commonPath -Raw
Assert-True ($gitCommonScript -notmatch 'FETCH_HEAD') `
    'Shared branch resolution must not use process-global FETCH_HEAD.'
Assert-True ($commonScript -match '--force-with-lease') `
    'The shared automation branch helper must use force-with-lease.'
Assert-True ($commonScript -notmatch '(?m)git push[^\r\n]*--force(?:\s|$)') `
    'The shared automation branch helper contains an unguarded force push.'
Assert-True ($bugTemplateScript -notmatch '(?m)^\s*git (?:switch|add|push|rev-parse)') `
    'A bug-template Git command is not rooted with git -C.'
$productionFiles = Get-ChildItem $publishingRoot -File |
    Where-Object { $_.Extension -in @('.ps1', '.psm1') -and $_.Name -notin @(
        'Git.Common.psm1',
        'GitHub.Common.psm1'
    ) }
foreach ($productionFile in $productionFiles) {
    $content = Get-Content $productionFile.FullName -Raw
    Assert-True ($content -notmatch '(?m)^\s*(?:&\s*)?(?:git|gh)\s') `
        "$($productionFile.Name) bypasses the shared Git or GitHub invoker."
    Assert-True ($content -notmatch 'Invoke-GitCommand|MyInvocation\.InvocationName') `
        "$($productionFile.Name) contains a retired command or dot-source guard."
}

# Exercises shared release identities, pagination, mutation safety, and repository versions.
$preview = Get-ReleaseIdentity '4.152.0-preview.1.26426.14'
Assert-Equal 'release/4.152.0-preview.1' $preview.Branch 'Preview branch identity was incorrect.'
Assert-Equal 'v4.152.0-preview.1.26426.14' $preview.Tag 'Preview tag identity was incorrect.'
Assert-Equal 'Version 4.152.0 (Preview 1)' $preview.Title 'Preview release title was incorrect.'
$stable = Get-ReleaseIdentity '4.151.1'
Assert-Equal 'Version 4.151.1' $stable.Title 'Stable release title was incorrect.'
Assert-Throws { Get-ReleaseIdentity '4.152.0-preview.1' } 'exact public' `
    'An abbreviated version unexpectedly passed exact identity parsing.'

# Resolve-NuGetPackageVersion decides whether Finish must disambiguate an abbreviated
# preview/rc against nuget.org. Only versions matching the abbreviated shape may reach the
# network; everything else must be returned untouched. These assertions cover exactly the
# no-network branches, so they stay deterministic and offline. Deliberately absent:
# '4.152.0-preview.1', the one shape that *would* call Invoke-RestMethod.
foreach ($exact in @('4.151.1', '4.151.1.1', '4.152.0-preview.1.26426.14', '4.152.0-rc.2.1.2')) {
    Assert-Equal $exact (Resolve-NuGetPackageVersion 'SkiaSharp' $exact) `
        "An exact version ($exact) was not passed through unchanged."
}
foreach ($invalid in @('4.152.0-preview.0', '4.152.0-rc.0', '4.152.0-beta.1', '4.152.0-preview')) {
    Assert-Equal $invalid (Resolve-NuGetPackageVersion 'SkiaSharp' $invalid) `
        "A non-resolvable version ($invalid) was not passed through unchanged."
}
$releaseTopology = @(
    'v2.88.4-preview.95',
    'v4.150.2',
    'v4.150.3',
    'v4.151.0',
    'v4.151.1',
    'v4.151.2',
    'v4.152.0-preview.1.1',
    'v4.152.0-rc.1.26426.14',
    'v4.153.0-preview.1.26454.6',
    'v4.154.0-preview.1.26454.9',
    'v4.154.0-gpu1'
)
Assert-Equal 'v4.152.0-rc.1.26426.14' `
    (Get-PreviousShippedTag 'v4.153.0-preview.1.26454.6' $releaseTopology) `
    'The 4.153 preview fell back past the 4.152 RC shipment.'
Assert-Equal 'v4.153.0-preview.1.26454.6' `
    (Get-PreviousShippedTag 'v4.154.0-preview.1.26454.9' $releaseTopology) `
    'The 4.154 preview fell back past the 4.153 preview shipment.'
Assert-Equal 'v4.150.2' (Get-PreviousShippedTag 'v4.150.3' $releaseTopology) `
    'A parallel 4.150 patch did not stay on its own semantic release line.'
Assert-Throws { Get-PreviousShippedTag 'v4.154.0-beta.1' $releaseTopology } 'not an exact' `
    'A decorative tag was accepted as a shipment.'
$pages = @(
    @([pscustomobject] @{ number = 1 }, [pscustomobject] @{ number = 2 }),
    @([pscustomobject] @{ number = 3 })
)
Assert-Equal @(1, 2, 3) @((Expand-GitHubPages $pages).number) 'GitHub pages were not flattened.'

$buildBranch = 'release/4.152.1'
$buildCommit = 'a' * 40
$greenBuildFixture = [pscustomobject] @{
    id = 100
    buildNumber = '4.152.1+test'
    sourceVersion = $buildCommit
    status = 'completed'
    result = 'succeeded'
    queueTime = '2026-09-17T00:00:00Z'
    finishTime = '2026-09-17T00:10:00Z'
    tags = @('BAR ID - 331115')
}
$greenBuild = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @($greenBuildFixture)
Assert-Equal 'green' $greenBuild.State 'A successful exact-tip package build was not green.'
Assert-Equal 331115 $greenBuild.BarId 'The package build BAR ID was not extracted.'
Assert-Equal $true $greenBuild.Ready 'A successful exact-tip BAR was not release-ready.'
$failedBuildFixture = [pscustomobject] @{
    id = 101
    buildNumber = '4.152.1+retry'
    sourceVersion = $buildCommit
    status = 'completed'
    result = 'failed'
    queueTime = '2026-09-17T00:20:00Z'
    finishTime = '2026-09-17T00:30:00Z'
    tags = @()
}
$failedBuild = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @($greenBuildFixture, $failedBuildFixture)
Assert-Equal 101 $failedBuild.BuildId 'The newest exact-tip build was not selected.'
Assert-Equal 'failed' $failedBuild.State 'A newer failed build was hidden by an older green build.'
$canceledBuild = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @([pscustomobject] @{
        id = 105
        buildNumber = '4.152.1+canceled'
        sourceVersion = $buildCommit
        status = 'completed'
        result = 'canceled'
        queueTime = '2026-09-17T00:35:00Z'
        finishTime = '2026-09-17T00:36:00Z'
        tags = @()
    })
Assert-Equal 'canceled' $canceledBuild.State `
    'A canceled exact-tip build was folded into the generic failed state.'
$runningBuild = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @([pscustomobject] @{
        id = 102
        buildNumber = '4.152.1+running'
        sourceVersion = $buildCommit
        status = 'inProgress'
        result = ''
        queueTime = '2026-09-17T00:40:00Z'
        finishTime = $null
        tags = @()
    })
Assert-Equal 'running' $runningBuild.State 'An active exact-tip build was not reported as running.'
$missingBarBuild = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @([pscustomobject] @{
        id = 103
        buildNumber = '4.152.1+missing-bar'
        sourceVersion = $buildCommit
        status = 'completed'
        result = 'succeeded'
        queueTime = '2026-09-17T00:50:00Z'
        finishTime = '2026-09-17T01:00:00Z'
        tags = @()
    })
Assert-Equal 'incomplete' $missingBarBuild.State 'A successful build without a BAR ID was marked green.'
$notBuilt = ConvertTo-ReleasePackageBuildState `
    -Branch $buildBranch `
    -Commit $buildCommit `
    -Builds @([pscustomobject] @{
        id = 104
        sourceVersion = ('b' * 40)
        status = 'completed'
        result = 'succeeded'
        tags = @('BAR ID - 999999')
    })
Assert-Equal 'not built' $notBuilt.State 'A build from another commit was accepted for the branch tip.'
$script:FakeAzCalls = 0
function global:az {
    $script:FakeAzCalls++
    $global:LASTEXITCODE = 1
    'The internal pipeline could not be reached.'
}
try {
    $unavailableBuild = Get-ReleasePackageBuild -Branch $buildBranch -Commit $buildCommit
} finally {
    Remove-Item Function:\az
}
Assert-Equal 1 $script:FakeAzCalls 'The package build lookup did not invoke Azure DevOps.'
Assert-Equal $false $unavailableBuild.Available `
    'An unavailable internal pipeline was treated as a completed build check.'
Assert-Equal 'unavailable' $unavailableBuild.State `
    'An unavailable internal pipeline did not preserve its distinct state.'

$script:PullListArguments = @()
function global:gh {
    $script:PullListArguments = @($args)
    $global:LASTEXITCODE = 0
    @'
[
  {
    "number": 5062,
    "title": "[skia-sync] Merge upstream chrome/m153 bug fixes",
    "headRefName": "skia-sync/release-4.153.x",
    "headRefOid": "2222222222222222222222222222222222222222",
    "baseRefName": "release/4.153.x",
    "baseRefOid": "1111111111111111111111111111111111111111",
    "isDraft": false,
    "mergeStateStatus": "BLOCKED",
    "url": "https://github.com/mono/SkiaSharp/pull/5062"
  }
]
'@
}
try {
    $openPullRequests = @(Get-GitHubOpenPullRequests `
        -Repository 'mono/SkiaSharp' `
        -Head 'skia-sync/release-4.153.x' `
        -Base 'release/4.153.x')
} finally {
    Remove-Item Function:\gh
}
Assert-Equal @(5062) @($openPullRequests.number) `
    'The exact open maintenance pull request was not returned.'
Assert-True (($script:PullListArguments -join ' ') -match '--head skia-sync/release-4\.153\.x' -and
    ($script:PullListArguments -join ' ') -match '--base release/4\.153\.x') `
    'The maintenance pull request query was not scoped to the expected head/base pair.'
$script:ComparisonArguments = @()
function global:gh {
    $script:ComparisonArguments = @($args)
    $global:LASTEXITCODE = 0
    '{"behind_by":3,"ahead_by":12,"status":"diverged"}'
}
try {
    $comparison = Get-GitHubComparison `
        -Repository 'mono/skia' `
        -Base 'upstream-sha' `
        -Head 'release/4.153.x'
} finally {
    Remove-Item Function:\gh
}
Assert-Equal 3 $comparison.behind_by 'The GitHub comparison did not retain behind_by.'
Assert-True (($script:ComparisonArguments -join ' ') -match
    'repos/mono/skia/compare/upstream-sha\.\.\.release/4\.153\.x') `
    'The upstream comparison did not use upstream as the base and mono/skia as the head.'
$script:PullListArguments = @()
function global:gh {
    $script:PullListArguments = @($args)
    $global:LASTEXITCODE = 0
    '[]'
}
try {
    Assert-Equal 0 @(Get-GitHubOpenPullRequests `
        -Repository 'mono/SkiaSharp' `
        -Head 'skia-sync/release-4.152.x' `
        -Base 'release/4.152.x').Count `
        'An empty pull request query produced a phantom result.'
} finally {
    Remove-Item Function:\gh
}

$script:FakeGhCalls = 0
function global:gh {
    $script:FakeGhCalls++
    throw 'Dry-run unexpectedly called gh.'
}
$dryMutation = @(Invoke-GitHubMutation `
    -Arguments @('api', 'repos/mono/SkiaSharp/issues/1', '-X', 'PATCH') `
    -Description 'Update issue' 6>&1) -join "`n"
Assert-Equal 0 $script:FakeGhCalls 'A dry-run GitHub mutation invoked gh.'
Assert-True ($dryMutation -match 'requires -Push') 'A dry-run GitHub mutation did not explain its guard.'
Remove-Item Function:\gh

$versionRoot = Join-Path $PSScriptRoot ".version-test-$([guid]::NewGuid().ToString('N'))"
try {
    $null = New-Item -ItemType Directory -Path (Join-Path $versionRoot 'scripts') -Force
    @'
SkiaSharp        nuget       4.152.0
libSkiaSharp     milestone   152
'@ | Set-Content (Join-Path $versionRoot 'scripts/VERSIONS.txt')
    $repositoryVersion = Get-RepositoryReleaseVersion -Root $versionRoot
    Assert-Equal 4 $repositoryVersion.Major 'The repository major version was not read.'
    Assert-Equal 152 $repositoryVersion.Milestone 'The Skia milestone was not read.'
} finally {
    Remove-Item $versionRoot -Recurse -Force -ErrorAction SilentlyContinue
}

$gitRoot = Join-Path $PSScriptRoot ".common-git-$([guid]::NewGuid().ToString('N'))"
$bareRoot = "$gitRoot.git"
$readerRoot = "$gitRoot-reader"
try {
    $null = New-Item -ItemType Directory -Path $gitRoot
    & git -C $gitRoot init --quiet
    & git -C $gitRoot config user.name 'Publishing Tests'
    & git -C $gitRoot config user.email 'publishing@example.invalid'
    & git -C $gitRoot commit --quiet --allow-empty -m 'Initial'
    Assert-Equal $null (Assert-GitWorktreeClean $gitRoot) 'A clean worktree was rejected.'
    'dirty' | Set-Content (Join-Path $gitRoot 'dirty.txt')
    Assert-Throws { Assert-GitWorktreeClean $gitRoot } 'must be clean' 'A dirty worktree was accepted.'
    Remove-Item (Join-Path $gitRoot 'dirty.txt')
    'tree entry' | Set-Content (Join-Path $gitRoot 'tree-entry.txt')
    & git -C $gitRoot add tree-entry.txt
    & git -C $gitRoot commit --quiet -m 'Add tree entry'
    & git -C $gitRoot branch release/test
    & git init --quiet --bare $bareRoot
    & git -C $gitRoot remote add origin $bareRoot
    $localSha = (git -C $gitRoot rev-parse release/test).Trim()
    Assert-Equal $localSha (Get-LocalBranchSha -Root $gitRoot -Branch release/test) `
        'A local branch SHA was not resolved.'
    $treeEntrySha = (git -C $gitRoot rev-parse 'release/test:tree-entry.txt').Trim()
    Assert-Equal $treeEntrySha (Get-GitTreeEntrySha `
        -Root $gitRoot `
        -Commit $localSha `
        -Path 'tree-entry.txt') `
        'A commit tree entry SHA was not resolved.'
    Push-ReleaseBranch `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Branch release/test `
        -LocalSha $localSha `
        -RemoteSha $null `
        -Description 'test' `
        -Push
    Assert-Equal $localSha (Get-RemoteBranchSha -Root $gitRoot -Remote $bareRoot -Branch release/test) `
        'A local test branch was not pushed.'
    $branchShas = Get-RemoteBranchShas `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Branches @('release/test', 'release/missing')
    Assert-Equal $localSha $branchShas['release/test'] `
        'The multi-branch remote lookup did not return the advertised branch.'
    Assert-Equal $false $branchShas.ContainsKey('release/missing') `
        'The multi-branch remote lookup invented a missing branch.'
    $null = New-Item -ItemType Directory -Path $readerRoot
    & git -C $readerRoot init --quiet
    $branchMap = Get-RemoteBranchMap `
        -Root $readerRoot `
        -Remote $bareRoot `
        -Pattern 'refs/heads/release/*'
    Assert-Equal $localSha $branchMap['release/test'] `
        'The remote branch map did not return the advertised branch tip.'
    & git -C $readerRoot cat-file -e "$localSha`^{commit}"
    Assert-Equal 0 $LASTEXITCODE `
        'The remote branch map did not fetch the advertised parent commit.'
    Assert-Equal $localSha (Get-ResolvedGitCommit `
        -Root $readerRoot `
        -Reference 'release/test' `
        -Remote $bareRoot) `
        'A remote branch did not resolve through its immutable advertised commit.'
    $dryBranch = @(Push-ReleaseBranch `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Branch release/dry `
        -LocalSha $localSha `
        -RemoteSha $null `
        -Description 'test' 6>&1) -join "`n"
    Assert-True ($dryBranch -match 'requires -Push') 'A dry branch push did not explain its guard.'
    Assert-Equal $null (Get-RemoteBranchSha -Root $gitRoot -Remote $bareRoot -Branch release/dry) `
        'A dry branch push changed its remote.'

    # --- Push-ReleaseTag: the irreversible path Finish uses to publish a release tag. ---
    # A tag is immutable once consumed by a release, so the guarded behaviours below
    # (dry-run refusal, create-and-verify, idempotent re-run, conflict rejection) are the
    # ones worth pinning. Everything here runs against a local bare remote: no network.
    $tagSha = (git -C $gitRoot rev-parse release/test).Trim()

    $virtualShipment = Get-ReleaseShipmentContract `
        -Root $gitRoot `
        -Version '4.153.0' `
        -Tag 'v4.153.0-preview.1.26426.14' `
        -SourceCommit $tagSha
    Assert-True $virtualShipment.IsVirtual 'A missing dry-run tag was not represented as a virtual shipment.'
    Assert-Equal @('v4.153.0-preview.1.26426.14') `
        (Add-PlannedReleaseShipmentTag -Tags @() -Shipment $virtualShipment) `
        'A virtual shipment was not included in dry-run milestone planning.'
    Assert-Throws {
        Get-ReleaseShipmentContract `
            -Root $gitRoot `
            -Version '4.153.0' `
            -Tag 'v4.153.0-preview.1.26426.14' `
            -SourceCommit $tagSha `
            -RequireTag
    } 'must exist' 'Push-mode milestone maintenance accepted a missing exact tag.'
    Assert-Throws {
        Get-ReleaseShipmentContract `
            -Root $gitRoot `
            -Version '4.153.0' `
            -Tag 'v4.154.0-preview.1.26426.14' `
            -SourceCommit $tagSha
    } 'does not match numeric' 'A planned tag for another numeric release was accepted.'
    Assert-Throws {
        Get-ReleaseShipmentContract `
            -Root $gitRoot `
            -Version '4.153.0' `
            -Tag 'v4.153.0-preview.1.26426.14' `
            -SourceCommit ''
    } 'supplied together' 'An unpaired planned tag was accepted.'

    $dryTag = @(Push-ReleaseTag `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Tag v9.9.9 `
        -SourceCommit $tagSha 6>&1) -join "`n"
    Assert-True ($dryTag -match 'requires -Push') 'A dry tag push did not explain its guard.'
    Assert-Equal $null (Get-RemoteTagSha -Root $gitRoot -Remote $bareRoot -Tag v9.9.9) `
        'A dry tag push created a remote tag.'

    Push-ReleaseTag `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Tag v9.9.9 `
        -SourceCommit $tagSha `
        -Push
    Assert-Equal $tagSha (Get-RemoteTagSha -Root $gitRoot -Remote $bareRoot -Tag v9.9.9) `
        'A release tag was not created at its source commit.'

    Push-ReleaseTag `
        -Root $gitRoot `
        -Remote origin `
        -Tag $virtualShipment.Tag `
        -SourceCommit $tagSha `
        -Push
    $verifiedShipment = Get-ReleaseShipmentContract `
        -Root $gitRoot `
        -Version $virtualShipment.Version `
        -Tag $virtualShipment.Tag `
        -SourceCommit $tagSha `
        -RequireTag
    Assert-True (!$verifiedShipment.IsVirtual) 'A verified push tag was still treated as virtual.'
    Assert-Equal @($virtualShipment.Tag) (Add-PlannedReleaseShipmentTag -Tags @($virtualShipment.Tag) -Shipment $verifiedShipment) `
        'A verified exact tag was duplicated during milestone planning.'

    # Re-running Finish must be safe: the tag already points at the same commit.
    $repeatTag = @(Push-ReleaseTag `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Tag v9.9.9 `
        -SourceCommit $tagSha `
        -Push 6>&1) -join "`n"
    Assert-True ($repeatTag -match "points to $tagSha") `
        'Re-pushing an identical tag was not reported as already ready.'

    # A tag that already points somewhere else must NEVER be moved.
    & git -C $gitRoot commit --quiet --allow-empty -m 'Second'
    $otherSha = (git -C $gitRoot rev-parse HEAD).Trim()
    & git -C $gitRoot push --quiet origin HEAD:refs/heads/other
    Assert-Throws {
        Get-ReleaseShipmentContract `
            -Root $gitRoot `
            -Version $virtualShipment.Version `
            -Tag $virtualShipment.Tag `
            -SourceCommit $otherSha `
            -RequireTag
    } 'expected' 'Push-mode milestone maintenance accepted a tag at the wrong source commit.'
    Assert-Throws { Push-ReleaseTag `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Tag v9.9.9 `
        -SourceCommit $otherSha `
        -Push } 'expected' 'A conflicting release tag was silently accepted.'
    Assert-Equal $tagSha (Get-RemoteTagSha -Root $gitRoot -Remote $bareRoot -Tag v9.9.9) `
        'A conflicting tag push moved the remote tag.'
    # The conflict must be detected before -Push is even considered.
    Assert-Throws { Push-ReleaseTag `
        -Root $gitRoot `
        -Remote $bareRoot `
        -Tag v9.9.9 `
        -SourceCommit $otherSha } 'expected' 'A dry run ignored a conflicting release tag.'
} finally {
    Remove-Item $gitRoot, $bareRoot, $readerRoot -Recurse -Force -ErrorAction SilentlyContinue
}

# Loads and exercises Prepare's pure version transformation functions.
Invoke-Expression (Get-ScriptFunctionText $preparePath)
Assert-Equal '14.2.1.201' (Get-NextHarfBuzzVersion '14.2.1.200') `
    'A high HarfBuzzSharp milestone bucket did not preserve its revision range.'
Assert-Equal '14.2.1.103' (Get-NextHarfBuzzVersion '14.2.1.102') `
    'The M151 HarfBuzzSharp bucket did not advance within its reserved range.'
$baseVersions = [pscustomobject] @{ SkiaSharp = '4.151.1'; HarfBuzzSharp = '14.2.1.1' }
Assert-Equal '14.2.1.1' (Get-ReleaseHarfBuzzVersion $baseVersions '4.151.1') `
    'A label-only cut changed HarfBuzzSharp.'
Assert-Equal '14.2.1.2' (Get-ReleaseHarfBuzzVersion $baseVersions '4.151.1.1') `
    'The first hotfix did not increment HarfBuzzSharp.'
$hotfixBase = [pscustomobject] @{ SkiaSharp = '4.151.1.1'; HarfBuzzSharp = '14.2.1.2' }
Assert-Equal '14.2.1.3' (Get-ReleaseHarfBuzzVersion $hotfixBase '4.151.1.2') `
    'A sequential hotfix did not increment HarfBuzzSharp.'
Assert-Throws { Get-ReleaseHarfBuzzVersion $baseVersions '4.151.1.0' } 'must be 4\.151\.1\.1' `
    'A zero hotfix revision was accepted.'
Assert-Throws { Get-ReleaseHarfBuzzVersion $baseVersions '4.151.1.2' } 'must be 4\.151\.1\.1' `
    'A skipped hotfix revision was accepted.'
$variables = "variables:`n  SKIASHARP_VERSION: 4.151.1`n  PREVIEW_LABEL: 'stable'`n"
$updatedVariables = Set-VersionVariables $variables '4.151.1.1' 'rc.1'
Assert-True ($updatedVariables -match 'SKIASHARP_VERSION: 4\.151\.1\.1') `
    'Prepare did not update the SkiaSharp variable.'
Assert-True ($updatedVariables -match "PREVIEW_LABEL: 'rc\.1'") 'Prepare did not update the release label.'
$versionsText = "SkiaSharp nuget 4.151.1`nSkiaSharp file 4.151.1.0`nHarfBuzzSharp nuget 14.2.1.1`n"
$updatedVersions = Set-PackageVersions $versionsText '4.151.1.1' '14.2.1.2'
Assert-True ($updatedVersions -match 'SkiaSharp nuget 4\.151\.1\.1') `
    'Prepare did not update SkiaSharp packages.'
Assert-True ($updatedVersions -match 'HarfBuzzSharp nuget 14\.2\.1\.2') `
    'Prepare did not update HarfBuzzSharp packages.'

# Loads and exercises Finish's release metadata and dry-run behavior.
Invoke-Expression (Get-ScriptFunctionText $finishPath)
$publishedHistorical = [pscustomobject] @{
    tagName = $preview.Tag
    name = 'Historical title'
    isPrerelease = $true
    body = 'Historical body'
}
Assert-Equal $null (Assert-GitHubRelease $preview $publishedHistorical) `
    'A published historical release was rejected.'
$powerShellReleaseText = (Get-Content $finishPath -Raw) + (Get-Content $commonPath -Raw)
Assert-True ($powerShellReleaseText -notmatch 'SKIASHARP:(?:RELEASE-SUMMARY|GITHUB-GENERATED-NOTES)') `
    'PowerShell unexpectedly owns release-summary body markers.'
Assert-True ((Get-Content $finishPath -Raw) -match 'Update-ReleaseSupport') `
    'Finish does not invoke release-support maintenance.'
$writeRemote = $false
$script:FakeGhCalls = 0
function global:gh {
    $script:FakeGhCalls++
    throw 'Finish dry-run unexpectedly called gh.'
}
$publishPlan = @(Publish-GitHubRelease $preview '0' $null 6>&1) -join "`n"
$existingPlan = @(Publish-GitHubRelease $preview '0' $null $publishedHistorical 6>&1) -join "`n"
$followUpPlan = @(Invoke-ReleaseFollowUpWorkflows $preview 6>&1) -join "`n"
Assert-Equal 0 $script:FakeGhCalls 'Finish dry-run invoked gh.'
Assert-True ($publishPlan -match 'Create and publish') 'Finish did not plan release publication.'
Assert-True ($existingPlan -match 'is published') 'Finish did not preserve published-release idempotency.'
Assert-True ($followUpPlan -match 'release-note generation') 'Finish did not plan release-note follow-up.'
Remove-Item Function:\gh

$finishOutput = Join-Path $PSScriptRoot ".finish-output-$([guid]::NewGuid().ToString('N'))"
try {
    $env:GITHUB_OUTPUT = $finishOutput
    foreach ($publicVersion in @(
        '4.152.0-preview.1.26426.14',
        '4.152.0-rc.1.26427.1',
        '4.152.0',
        '4.152.1',
        '4.152.0.1'
    )) {
        $resolvedRelease = Get-ReleaseIdentity $publicVersion
        Set-ReleaseFinishOutput -Release $resolvedRelease -PublicVersion $publicVersion -SourceCommit ('a' * 40)
        $finishOutputs = Get-Content -LiteralPath $finishOutput | Select-Object -Last 4
        Assert-Equal @(
            "release_version=$publicVersion",
            "release_numeric=$($resolvedRelease.Numeric)",
            "release_tag=$($resolvedRelease.Tag)",
            ('source_commit=' + ('a' * 40))
        ) @($finishOutputs) "Finish did not write the resolved-release output contract for $publicVersion."
    }
} finally {
    Remove-Item Env:\GITHUB_OUTPUT -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $finishOutput -Force -ErrorAction SilentlyContinue
}

$packageCommit = 'a' * 40
$packageBranchCommit = 'b' * 40
$script:PackageAncestryExitCode = 0
function Get-ResolvedGitCommit([string] $Root, [string] $Reference, [string] $Remote = 'origin') {
    if ($Reference -match '^[0-9a-f]{40}$') {
        return $Reference
    }
    return $packageBranchCommit
}
function Invoke-Git(
    [string] $Root,
    [string[]] $Arguments,
    [switch] $AllowFailure,
    [switch] $WriteOutput
) {
    return [pscustomobject] @{
        ExitCode = $script:PackageAncestryExitCode
        Output = ''
    }
}
try {
    $stableRelease = Get-ReleaseIdentity '4.152.1'
    $verifiedPackageSource = Assert-ReleasePackageSource `
        -Root . `
        -Release $stableRelease `
        -PackageSource ([pscustomobject] @{
            Branch = 'refs/heads/release/4.152.1'
            Commit = $packageCommit
        })
    Assert-Equal 'release/4.152.1' $verifiedPackageSource.Branch `
        'Finish did not normalize the package source branch.'
    Assert-Throws {
        Assert-ReleasePackageSource `
            -Root . `
            -Release $stableRelease `
            -PackageSource ([pscustomobject] @{
                Branch = 'release/4.153.0'
                Commit = $packageCommit
            })
    } 'expected release/4\.152\.1' `
        'Finish accepted package metadata from another release branch.'
    $script:PackageAncestryExitCode = 1
    Assert-Throws {
        Assert-ReleasePackageSource `
            -Root . `
            -Release $stableRelease `
            -PackageSource ([pscustomobject] @{
                Branch = 'release/4.152.1'
                Commit = $packageCommit
            })
    } 'not reachable' `
        'Finish accepted a package commit outside the expected release branch.'
} finally {
    Remove-Item Function:\Get-ResolvedGitCommit
    Remove-Item Function:\Invoke-Git
}

$script:FakeGhCommands = [System.Collections.Generic.List[string]]::new()
function global:gh {
    $script:FakeGhCommands.Add(($args -join ' '))
    $global:LASTEXITCODE = 0
}
$writeRemote = $true
try {
    $null = Invoke-ReleaseFollowUpWorkflows $stable
} finally {
    $writeRemote = $false
    Remove-Item Function:\gh
}
Assert-True ([bool] (
    $script:FakeGhCommands |
        Where-Object { $_ -match 'auto-update-issue-template-versions\.yml.*-f mode=Push' }
)) 'Stable Finish did not dispatch the issue-template workflow in Push mode.'

$script:FakeGhCommands = [System.Collections.Generic.List[string]]::new()
$script:FakePublishedRelease = [pscustomobject] @{
    tagName = $preview.Tag
    name = $preview.Title
    isPrerelease = $true
    targetCommitish = '0'
    body = 'Generated notes'
    url = "https://github.com/mono/SkiaSharp/releases/tag/$($preview.Tag)"
}
function global:gh {
    $command = $args -join ' '
    $script:FakeGhCommands.Add($command)
    $global:LASTEXITCODE = 0
    if ($command -match '^release view ') {
        return $script:FakePublishedRelease | ConvertTo-Json -Compress
    }
}
$writeRemote = $true
try {
    $null = Publish-GitHubRelease `
        -Release $preview `
        -SourceCommit '0' `
        -PreviousTag 'v4.151.2' `
        -Existing $null
} finally {
    $writeRemote = $false
    Remove-Item Function:\gh
}
Assert-True ([bool] (
    $script:FakeGhCommands |
        Where-Object {
            $_ -match '^release create ' -and
            $_ -match '--generate-notes' -and
            $_ -match '--notes-start-tag v4\.151\.2'
        }
)) 'Finish did not give GitHub-generated notes the selected predecessor tag.'
Assert-True (-not [bool] (
    $script:FakeGhCommands |
        Where-Object { $_ -match '^release create .*--draft(?:\s|=|$)' }
)) 'Finish unexpectedly created a draft release.'

# Exercises exact-release support-tier additions and promotions.
$supportConfig = @'
{
  "$comment": "keep this text",
  "unrelated_before": {
    "support": {
      "stable": ["do-not-change"],
      "preview": ["also-do-not-change"]
    }
  },
  "support": {
    "$comment": "keep this support text",
    "metadata": {
      "stable": ["nested-stable"],
      "preview": ["nested-preview"]
    },
    "stable": [
      "4.148",
      "4.150"
    ],
    "preview": [
      "4.151"
    ]
  },
  "history_floor": {
    "skiasharp": "3.0.0"
  },
  "unrelated": {
    "enabled": true
  }
}
'@
$previewRelease = Get-ReleaseIdentity '4.152.0-preview.1.26426.14'
$previewSupport = Get-UpdatedReleaseSupport -Text $supportConfig -Release $previewRelease
$previewDocument = $previewSupport | ConvertFrom-Json
Assert-Equal @('4.151', '4.152') @($previewDocument.support.preview) `
    'A preview release did not append its line.'
Assert-Equal @('4.148', '4.150') @($previewDocument.support.stable) `
    'A preview release changed existing stable lines.'

$rcRelease = Get-ReleaseIdentity '4.153.0-rc.1.26430.2'
$rcSupport = Get-UpdatedReleaseSupport -Text $previewSupport -Release $rcRelease
$rcDocument = $rcSupport | ConvertFrom-Json
Assert-Equal @('4.151', '4.152', '4.153') @($rcDocument.support.preview) `
    'An RC release did not append its line.'

$promotedRelease = Get-ReleaseIdentity '4.152.0'
$promotedSupport = Get-UpdatedReleaseSupport -Text $rcSupport -Release $promotedRelease
$promotedDocument = $promotedSupport | ConvertFrom-Json
Assert-Equal @('4.148', '4.150', '4.152') @($promotedDocument.support.stable) `
    'A stable release did not retain existing stable lines and append its line.'
Assert-Equal @('4.151', '4.153') @($promotedDocument.support.preview) `
    'A stable release removed a preview line other than its own.'
Assert-Equal $promotedSupport (Get-UpdatedReleaseSupport -Text $promotedSupport -Release $promotedRelease) `
    'A repeated stable promotion was not idempotent.'
Assert-Equal 'keep this text' $promotedDocument.'$comment' `
    'Release support changed an unrelated top-level comment.'
Assert-Equal @('do-not-change') @($promotedDocument.unrelated_before.support.stable) `
    'Release support changed a nested support object.'
Assert-Equal @('also-do-not-change') @($promotedDocument.unrelated_before.support.preview) `
    'Release support changed a nested preview tier.'
Assert-Equal 'keep this support text' $promotedDocument.support.'$comment' `
    'Release support changed the support comment.'
Assert-Equal @('nested-stable') @($promotedDocument.support.metadata.stable) `
    'Release support changed a nested property within the top-level support object.'
Assert-Equal @('nested-preview') @($promotedDocument.support.metadata.preview) `
    'Release support changed nested preview metadata.'
Assert-Equal '3.0.0' $promotedDocument.history_floor.skiasharp `
    'Release support changed history-floor configuration.'
Assert-Equal $true $promotedDocument.unrelated.enabled `
    'Release support changed unrelated configuration.'

$inlineConfig = @'
{
  "support": {
    "stable": [ "4.150" ],
    "preview": []
  }
}
'@
$inlinePreview = Get-UpdatedReleaseSupport -Text $inlineConfig -Release $previewRelease
$inlinePreviewDocument = $inlinePreview | ConvertFrom-Json
Assert-Equal @('4.150') @($inlinePreviewDocument.support.stable) `
    'A preview release changed the unchanged stable tier.'
$emptyPromotionConfig = @'
{
  "support": {
    "stable": [],
    "preview": ["4.152"]
  }
}
'@
$emptyPromotion = Get-UpdatedReleaseSupport -Text $emptyPromotionConfig -Release $promotedRelease
$emptyPromotionDocument = $emptyPromotion | ConvertFrom-Json
Assert-Equal @('4.152') @($emptyPromotionDocument.support.stable) `
    'Promotion from an empty stable tier failed.'
Assert-Equal @() @($emptyPromotionDocument.support.preview) `
    'Promotion did not empty the sole preview tier.'
Assert-Equal $emptyPromotion (Get-UpdatedReleaseSupport -Text $emptyPromotion -Release $promotedRelease) `
    'Rerunning after promotion from the sole preview tier failed.'

# Exercises issue-template release parsing, selection, and text surgery.
Invoke-Expression (Get-ScriptFunctionText $bugTemplatePath)
$nightlyOption = 'Nightly / CI build'
$otherOption = 'Other (Please indicate in the description)'
$hotfix = ConvertTo-IssueTemplateVersion 'v4.151.1.1'
Assert-Equal '4.151.1.1' $hotfix.Display 'A stable hotfix lost its fourth version part.'
$exactRc = ConvertTo-IssueTemplateVersion 'v4.152.0-rc.1.26426.14'
Assert-Equal '4.152.0-rc.1' $exactRc.Display 'A two-part build revision was not removed from display.'

$script:FakeGhCalls = 0
function global:gh {
    $script:FakeGhCalls++
    $global:LASTEXITCODE = 0
    @'
[
  {"tagName":"v4.152.0-preview.1.1","isDraft":false},
  {"tagName":"v4.152.0-preview.1.26426.14","isDraft":false},
  {"tagName":"v4.151.1","isDraft":false},
  {"tagName":"v4.151.0","isDraft":false},
  {"tagName":"v3.119.4","isDraft":false},
  {"tagName":"v4.153.0-preview.1.1","isDraft":true},
  {"tagName":"not-a-version","isDraft":false}
]
'@
}
$publishedVersions = @(Get-PublishedReleaseVersions 'mono/SkiaSharp')
Remove-Item Function:\gh
Assert-Equal 4 $publishedVersions.Count 'Published release filtering or de-duplication was incorrect.'
Assert-Equal 'v4.152.0-preview.1.26426.14' `
    ($publishedVersions | Where-Object Display -eq '4.152.0-preview.1').Tag `
    'The greatest exact prerelease build was not retained.'
$issueOptions = New-IssueTemplateOptions -Versions $publishedVersions -Major 4
Assert-Equal @(
    'Nightly / CI build',
    '4.152.0-preview.1 (Pre-release)',
    '4.151.1 (Current)',
    '4.151.0 (Previous)',
    '3.x (Obsolete)',
    'Other (Please indicate in the description)'
) $issueOptions.Version 'The primary version options were incorrect.'
Assert-Equal 2 $issueOptions.VersionDefault 'The primary current-version default was incorrect.'

$template = @'
name: Bug
body:
  - type: dropdown
    id: version
    attributes:
      options:
        - old
      default: 0
  - type: dropdown
    id: goodversion
    attributes:
      options:
        - old
      default: 0
  - type: input
    id: untouched
'@
$rendered = Get-UpdatedIssueTemplate -Text $template -Options $issueOptions
Assert-True ($rendered -match 'id: untouched') 'Issue-template rendering changed an unrelated block.'
Assert-True ($rendered -match 'default: 2') 'Issue-template rendering did not update the primary default.'
Assert-Equal $rendered (Get-UpdatedIssueTemplate -Text $rendered -Options $issueOptions) `
    'Issue-template rendering was not idempotent.'
$crlfTemplate = $template.Replace("`n", "`r`n")
$crlfRendered = Get-UpdatedIssueTemplate -Text $crlfTemplate -Options $issueOptions
Assert-True (!$crlfRendered.Replace("`r`n", '').Contains("`n")) `
    'Issue-template rendering changed CRLF line endings.'
Assert-Throws {
    Set-IssueTemplateDropdown -Text $template -DropdownId missing -Options @('one') -Default 0
} 'Could not find dropdown id' 'A missing dropdown did not fail.'

$automationRoot = Join-Path $PSScriptRoot ".automation-git-$([guid]::NewGuid().ToString('N'))"
$automationBare = "$automationRoot.git"
try {
    $null = New-Item -ItemType Directory -Path $automationRoot
    & git -C $automationRoot init --quiet
    & git -C $automationRoot config user.name 'Publishing Tests'
    & git -C $automationRoot config user.email 'publishing@example.invalid'
    'old' | Set-Content (Join-Path $automationRoot 'template.yml')
    & git -C $automationRoot add template.yml
    & git -C $automationRoot commit --quiet -m 'Main'
    & git -C $automationRoot branch -M main
    & git init --quiet --bare $automationBare
    & git -C $automationRoot remote add origin $automationBare
    & git -C $automationRoot push --quiet origin main
    $mainSha = (git -C $automationRoot rev-parse HEAD).Trim()

    function global:gh {
        $global:LASTEXITCODE = 0
        '[]'
    }
    try {
        Publish-AutomationFilePullRequest `
            -Root $automationRoot `
            -Repository 'mono/SkiaSharp' `
            -Branch automation/apply `
            -BaseBranch main `
            -Files ([ordered] @{ 'template.yml' = "applied`n" }) `
            -CommitMessage 'Apply test' `
            -Title 'Apply test' `
            -Body 'Apply test' `
            -Description test `
            -Mode Apply
    } finally {
        Remove-Item Function:\gh
    }
    Assert-Equal "applied`n" ([IO.File]::ReadAllText((Join-Path $automationRoot 'template.yml'))) `
        'Automation Apply did not write the desired local content.'
    Assert-Equal 'automation/apply' ((git -C $automationRoot branch --show-current).Trim()) `
        'Automation Apply did not create the local automation branch.'
    Assert-Equal '' ((git -C $automationRoot status --porcelain) -join '') `
        'Automation Apply did not commit the local update.'
    & git -C $automationRoot switch --quiet main

    & git -C $automationRoot switch --quiet -c automation/update
    [IO.File]::WriteAllText(
        (Join-Path $automationRoot 'template.yml'),
        "new`n",
        [Text.UTF8Encoding]::new($false))
    & git -C $automationRoot add template.yml
    & git -C $automationRoot commit --quiet -m 'Update'
    & git -C $automationRoot push --quiet origin HEAD:refs/heads/automation/update
    $remoteSha = (git -C $automationRoot rev-parse HEAD).Trim()
    & git -C $automationRoot switch --quiet main

    Assert-True (Test-AutomationFileBranch `
        -Root $automationRoot `
        -RemoteSha $remoteSha `
        -BaseSha $mainSha `
        -Files ([ordered] @{ 'template.yml' = "new`n" })) `
        'An identical automation branch was not reusable.'
    Assert-True (!(Test-AutomationFileBranch `
        -Root $automationRoot `
        -RemoteSha $remoteSha `
        -BaseSha $mainSha `
        -Files ([ordered] @{ 'template.yml' = "different`n" }))) `
        'A different automation branch was incorrectly reusable.'
} finally {
    Remove-Item $automationRoot, $automationBare -Recurse -Force -ErrorAction SilentlyContinue
}

# Exercises the focused release-line audit without contacting GitHub or NuGet.
$auditPath = Join-Path $publishingRoot 'audit-release-state.ps1'
$auditCommand = Get-Command $auditPath
$auditParameters = @($auditCommand.Parameters.Keys | Where-Object {
    $_ -notin @('Verbose', 'Debug', 'ErrorAction', 'WarningAction', 'InformationAction',
        'ProgressAction', 'ErrorVariable', 'WarningVariable', 'InformationVariable',
        'OutVariable', 'OutBuffer', 'PipelineVariable')
})
Assert-Equal @('Json', 'Version') @($auditParameters | Sort-Object) `
    'The release-line audit must expose only Version and Json.'
Assert-Equal 'System.String' $auditCommand.Parameters['Version'].ParameterType.FullName `
    'The release-line audit Version parameter must be a string.'
$auditScript = Get-Content $auditPath -Raw
Assert-True ($auditScript.Contains('Format-Table -AutoSize -Wrap') -and
    $auditScript.Contains('$PSStyle.Bold') -and
    $auditScript.Contains('Get-GitHubOpenPullRequests')) `
    'The release-line audit must use PowerShell table formatting and host-aware emphasis.'
Assert-True ($auditScript -match
    'Write-Error "Release-state audit unavailable:.*-ErrorAction Continue') `
    'The release-line audit failure path must preserve unavailable exit code 2.'
$auditFunctions = Get-ScriptFunctionText $auditPath
Invoke-Expression $auditFunctions

function New-AuditBranch([string] $Identity, [string] $Sha) {
    return [pscustomobject] @{
        Name = "release/$Identity"
        Sha = $Sha
        Identity = ConvertTo-ReleaseMilestone $Identity
    }
}

function New-AuditPackage([string] $Version, [string] $Branch, [string] $Commit) {
    return [pscustomobject] @{ Version = $Version; Branch = $Branch; Commit = $Commit }
}

function New-AuditRelease([string] $Tag, [string] $Commit, [bool] $Prerelease = $false) {
    return [pscustomobject] @{
        tagName = $Tag
        targetCommitish = $Commit
        isPrerelease = $Prerelease
    }
}

function New-AuditPackageBuild(
    [string] $Branch,
    [string] $Commit,
    [string] $State,
    [bool] $Ready,
    [int] $BuildId = 200,
    [int] $BarId = 331115,
    [bool] $Available = $true
) {
    return [pscustomobject] @{
        Available = $Available
        Branch = $Branch
        Commit = $Commit
        State = $State
        Ready = $Ready
        BuildId = if ($BuildId) { $BuildId } else { $null }
        BuildNumber = 'test'
        Status = if ($State -eq 'running') { 'inProgress' } else { 'completed' }
        Result = if ($State -eq 'green') { 'succeeded' } elseif ($State -eq 'failed') { 'failed' } else { '' }
        QueueTime = $null
        FinishTime = $null
        BarId = if ($BarId) { $BarId } else { $null }
        Url = if ($BuildId) { "https://dev.azure.com/dnceng/internal/_build/results?buildId=$BuildId" } else { '' }
        Message = if ($Available) { "Build state is $State." } else { 'Azure DevOps unavailable.' }
    }
}

function New-AuditPullRequest(
    [int] $Number,
    [string] $Head,
    [string] $HeadSha,
    [string] $Base,
    [string] $BaseSha,
    [bool] $Draft = $false
) {
    return [pscustomobject] @{
        number = $Number
        title = 'Sync more Skia changes'
        headRefName = $Head
        headRefOid = $HeadSha
        baseRefName = $Base
        baseRefOid = $BaseSha
        isDraft = $Draft
        mergeStateStatus = if ($Draft) { 'BEHIND' } else { 'BLOCKED' }
        url = "https://github.com/mono/SkiaSharp/pull/$Number"
    }
}

function New-AuditDelta([string] $Text, [int] $Queued = 0) {
    return [pscustomobject] @{
        Text = $Text
        Queued = if ($Queued) {
            @(1..$Queued | ForEach-Object { [pscustomobject] @{ Sha = "$_"; Subject = 'Change' } })
        } else {
            @()
        }
    }
}

$sha0 = '0' * 40
$sha1 = '1' * 40
$sha2 = '2' * 40
$script:AuditDeltaSubjects = @(
    [pscustomobject] @{ Sha = $sha0; Subject = 'Bump to the next version (4.152.1)' }
)
function global:Invoke-Git {
    param([string] $Root, [string[]] $Arguments)
    if ($Arguments[0] -eq 'log') {
        $lines = $script:AuditDeltaSubjects | ForEach-Object {
            "$($_.Sha)$([char] 0x1f)$($_.Subject)"
        }
        return [pscustomobject] @{ Output = $lines -join "`n" }
    }
    if ($Arguments[0] -eq 'diff-tree') {
        return [pscustomobject] @{
            Output = "scripts/VERSIONS.txt`nscripts/azure-templates-variables.yml"
        }
    }
    throw "Unexpected audit test git invocation: $($Arguments -join ' ')"
}
try {
    $delta = Get-MaintenanceDelta -Root . -MaintenanceSha $sha2 -ReleaseSha $sha0
    Assert-Equal 'version bump only' $delta.Text 'The automatic version bump was not ignored.'
    $script:AuditDeltaSubjects += [pscustomobject] @{ Sha = $sha1; Subject = 'Fix release branch' }
    $delta = Get-MaintenanceDelta -Root . -MaintenanceSha $sha2 -ReleaseSha $sha0
    Assert-Equal '1 queued release commit' $delta.Text 'A real maintenance commit was not retained.'
} finally {
    Remove-Item Function:\Invoke-Git
}
function Get-GitFileText([string] $Root, [string] $Commit, [string] $Path) {
    return "SkiaSharp nuget 4.154.0`nlibSkiaSharp milestone 154`n"
}
try {
    $releaseInfo = Get-SkiaSharpReleaseInfoAtCommit -Root . -Commit $sha2
    Assert-Equal '4.154.0' $releaseInfo.Version 'The SkiaSharp version was not read from VERSIONS.txt.'
    Assert-Equal 154 $releaseInfo.SkiaMilestone 'The Skia milestone was not read from VERSIONS.txt.'
} finally {
    Remove-Item Function:\Get-GitFileText
}
$maintenance = [pscustomobject] @{
    Branch = 'release/4.152.x'
    Sha = $sha2
    Version = '4.152.1'
    SkiaMilestone = 152
}
$mainMaintenance = [pscustomobject] @{
    Branch = 'main'
    Sha = $sha2
    Version = '4.154.0'
    SkiaMilestone = 154
}
$stable = New-AuditBranch '4.152.0' $sha0
$stablePackage = New-AuditPackage '4.152.0' $stable.Name $sha0
$stableTag = 'v4.152.0'
$stableRelease = New-AuditRelease $stableTag $sha0
Assert-Equal 'skia-sync/release-4.152.x' (Get-ExpectedSyncBranch $maintenance) `
    'The servicing-line sync branch name was not derived.'
Assert-Equal 'skia-sync/m154' (Get-ExpectedSyncBranch $mainMaintenance) `
    'The current-line milestone sync branch name was not derived.'
$servicingSyncTopology = Get-SkiaSyncTopology -Maintenance $maintenance
Assert-Equal 'chrome/m152' $servicingSyncTopology.UpstreamRef `
    'The servicing line did not derive its Chrome milestone branch.'
Assert-Equal 'release/4.152.x' $servicingSyncTopology.SkiaBaseBranch `
    'The servicing line did not use the matching mono/skia release branch.'
$mainSyncTopology = Get-SkiaSyncTopology -Maintenance $mainMaintenance
Assert-Equal 'chrome/m154' $mainSyncTopology.UpstreamRef `
    'The current line did not derive its Chrome milestone branch.'
Assert-Equal 'skiasharp' $mainSyncTopology.SkiaBaseBranch `
    'The current line did not use the mono/skia integration branch.'
$pendingMilestone = Get-PendingMainMilestone `
    -Line '4.155' `
    -MainSha $mainMaintenance.Sha `
    -MainReleaseInfo ([pscustomobject] @{
        Version = $mainMaintenance.Version
        SkiaMilestone = $mainMaintenance.SkiaMilestone
    })
Assert-Equal 155 $pendingMilestone.Milestone `
    'The next Skia milestone was not recognized as a pending main line.'
Assert-Equal $null (Get-PendingMainMilestone `
    -Line '4.156' `
    -MainSha $mainMaintenance.Sha `
    -MainReleaseInfo ([pscustomobject] @{
        Version = $mainMaintenance.Version
        SkiaMilestone = $mainMaintenance.SkiaMilestone
    })) `
    'A line beyond the next Skia milestone was treated as pending.'
Assert-Equal $null (Get-PendingMainMilestone `
    -Line '4.155' `
    -MainSha $mainMaintenance.Sha `
    -MainReleaseInfo ([pscustomobject] @{
        Version = '4.153.0'
        SkiaMilestone = 154
    })) `
    'A mismatched main release line and Skia milestone produced a pending line.'
$pendingSyncTopology = Get-SkiaSyncTopology `
    -Maintenance $mainMaintenance `
    -TargetMilestone $pendingMilestone.Milestone
Assert-Equal 'chrome/m155' $pendingSyncTopology.UpstreamRef `
    'The pending line did not derive its future Chrome milestone branch.'
Assert-Equal 'skia-sync/m155' $pendingSyncTopology.SyncBranch `
    'The pending line did not derive its future milestone sync branch.'

$upstreamSha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
$skiaBaseSha = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
$skiaSyncSha = 'cccccccccccccccccccccccccccccccccccccccc'
$parentSkiaSha = 'dddddddddddddddddddddddddddddddddddddddd'
$script:AuditSkiaBranches = @{}
$script:AuditSkiaBranches[$maintenance.Branch] = $skiaBaseSha
$script:AuditBehindBy = 1
$script:AuditCompareHead = ''
function Get-RemoteBranchSha(
    [string] $Root,
    [string] $Remote,
    [string] $Branch
) {
    return $upstreamSha
}
function Get-RemoteBranchShas(
    [string] $Root,
    [string] $Remote,
    [string[]] $Branches
) {
    return $script:AuditSkiaBranches
}
function Get-GitHubComparison([string] $Repository, [string] $Base, [string] $Head) {
    $script:AuditCompareHead = $Head
    return [pscustomobject] @{ behind_by = $script:AuditBehindBy }
}
function Get-GitTreeEntrySha([string] $Root, [string] $Commit, [string] $Path) {
    return $parentSkiaSha
}
try {
    $upstreamChanges = Get-SkiaUpstreamStatus -Root . -Maintenance $maintenance
    Assert-Equal 1 $upstreamChanges.BehindBy `
        'New Chrome milestone commits were not reported.'
    Assert-Equal $maintenance.Branch $script:AuditCompareHead `
        'The upstream check did not fall back to the mono/skia base branch.'
    Assert-Equal $true $upstreamChanges.BlocksRelease `
        'New upstream commits did not block a release cut.'

    $script:AuditSkiaBranches = @{}
    $script:AuditSkiaBranches[$maintenance.Branch] = $skiaBaseSha
    $script:AuditSkiaBranches[$servicingSyncTopology.SyncBranch] = $skiaSyncSha
    $script:AuditBehindBy = 0
    $upstreamCurrent = Get-SkiaUpstreamStatus -Root . -Maintenance $maintenance
    Assert-Equal $servicingSyncTopology.SyncBranch $script:AuditCompareHead `
        'The existing mono/skia sync branch was not preferred for comparison.'
    Assert-Equal 'current' $upstreamCurrent.State `
        'An up-to-date sync branch was reported as having upstream work.'
} finally {
    Remove-Item Function:\Get-RemoteBranchSha
    Remove-Item Function:\Get-RemoteBranchShas
    Remove-Item Function:\Get-GitHubComparison
    Remove-Item Function:\Get-GitTreeEntrySha
}
$openSyncPullRequest = Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @(
        New-AuditPullRequest 5062 'skia-sync/release-4.152.x' $sha1 $maintenance.Branch $maintenance.Sha
    ) `
    -Topology $servicingSyncTopology `
    -SkiaSyncBranchSha $skiaSyncSha `
    -ParentSyncSkiaSha $skiaSyncSha
Assert-Equal 'open' $openSyncPullRequest.State 'A ready incoming maintenance PR was not detected.'
Assert-Equal $true $openSyncPullRequest.BlocksRelease `
    'A ready incoming maintenance PR did not block a new release cut.'
$staleParentSyncPullRequest = Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @(
        New-AuditPullRequest 5062 'skia-sync/release-4.152.x' $sha1 $maintenance.Branch $maintenance.Sha
    ) `
    -Topology $servicingSyncTopology `
    -SkiaSyncBranchSha $skiaSyncSha `
    -ParentSyncSkiaSha $parentSkiaSha
Assert-Equal 'inconsistent' $staleParentSyncPullRequest.State `
    'A parent sync PR with a stale mono/skia gitlink was reported as ready.'
Assert-True ($staleParentSyncPullRequest.Message -match 'expected') `
    'A stale parent sync PR did not explain the expected native commit.'
$mainSyncPullRequest = Get-IncomingReleasePullRequest `
    -Maintenance $mainMaintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @(
        New-AuditPullRequest 5054 'skia-sync/m154' $sha1 'main' $sha0
    )
Assert-Equal 'skia-sync/m154' $mainSyncPullRequest.HeadBranch `
    'The current line inspected the upstream-tip sync branch instead of the milestone branch.'
Assert-Equal $true $mainSyncPullRequest.BlocksRelease `
    'A ready current-line milestone PR did not block a new release cut.'
$draftSyncPullRequest = Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @(
        New-AuditPullRequest 5063 'skia-sync/release-4.152.x' $sha1 $maintenance.Branch $sha0 $true
    )
Assert-Equal 'draft' $draftSyncPullRequest.State 'A draft incoming sync PR was not identified.'
Assert-Equal $false $draftSyncPullRequest.BlocksRelease `
    'A draft incoming sync PR incorrectly blocked a release cut.'
$pendingSyncPullRequest = Get-IncomingReleasePullRequest `
    -Maintenance $mainMaintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @(
        New-AuditPullRequest 5081 'skia-sync/m155' $sha1 'main' $sha0 $true
    ) `
    -Topology $pendingSyncTopology
Assert-Equal 'draft' $pendingSyncPullRequest.State `
    'The pending milestone pull request was not identified.'
$pendingUpstreamCurrent = [pscustomobject] @{
    Milestone = 155
    UpstreamRef = 'chrome/m155'
    UpstreamSha = $upstreamSha
    ParentBaseBranch = 'main'
    ParentSkiaSha = $parentSkiaSha
    SkiaBaseBranch = 'skiasharp'
    SkiaBaseSha = $skiaBaseSha
    SyncBranch = 'skia-sync/m155'
    SyncBranchSha = $skiaSyncSha
    CompareRef = 'skia-sync/m155'
    CompareSha = $skiaSyncSha
    BehindBy = 0
    HasChanges = $false
    State = 'current'
    BlocksRelease = $false
    Message = ''
}
$pendingState = Get-ReleaseAuditState `
    -Line '4.155' `
    -Maintenance $null `
    -Branches @() `
    -Packages @() `
    -TagShas @{} `
    -GitHubReleases @{} `
    -Delta (New-AuditDelta 'none') `
    -IncomingPullRequest $pendingSyncPullRequest `
    -UpstreamSync $pendingUpstreamCurrent `
    -PendingMilestone $pendingMilestone
Assert-True (@($pendingState.Actions.Kind) -contains 'review-sync') `
    'A draft next-milestone PR did not produce a completion action.'
Assert-True (@($pendingState.Actions.Kind) -notcontains 'start') `
    'The pending milestone incorrectly started a release before main advanced.'
$orphanedSyncBranch = Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha $sha1 `
    -PullRequests @()
Assert-Equal 'inconsistent' $orphanedSyncBranch.State `
    'A sync branch without an open pull request was not reported.'
Assert-Equal $null (Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha '' `
    -PullRequests $null) `
    'An absent sync branch and empty pull request query produced a phantom incoming PR.'
$incompleteNativeSync = Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha '' `
    -PullRequests $null `
    -Topology $servicingSyncTopology `
    -SkiaSyncBranchSha $skiaSyncSha `
    -ParentSkiaSha $parentSkiaSha
Assert-Equal 'inconsistent' $incompleteNativeSync.State `
    'A mono/skia-only sync was not reported as incomplete.'
Assert-Equal $true $incompleteNativeSync.BlocksRelease `
    'A mono/skia-only sync did not block a release cut.'
Assert-Equal $null (Get-IncomingReleasePullRequest `
    -Maintenance $maintenance `
    -SyncBranchSha '' `
    -PullRequests $null `
    -Topology $servicingSyncTopology `
    -SkiaSyncBranchSha $skiaSyncSha `
    -ParentSkiaSha $skiaSyncSha) `
    'A sync already integrated by the parent base was reported as incomplete.'

$bumpOnly = New-AuditDelta 'version bump only'
$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } -Delta $bumpOnly
Assert-Equal 0 $state.Actions.Count 'A maintenance-only version bump unexpectedly started a release.'
Assert-Equal 'version bump only' $state.MaintenanceDelta.Text 'The bump-only maintenance state was lost.'
Assert-Equal $sha0 $state.Releases[0].SourceCommit 'NuGet source provenance was omitted from the audit state.'

$realChanges = New-AuditDelta '2 queued release commits' 2
$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } -Delta $realChanges
Assert-True (@($state.Actions.Kind) -contains 'start') 'Real maintenance changes did not recommend the next release.'
Assert-True ((@($state.Actions.Command) -join "`n") -match '4\.152\.1-stable') 'The stable/patch next identity was not derived.'
$blockedByUpstream = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -UpstreamSync $upstreamChanges
Assert-True (@($blockedByUpstream.Actions.Kind) -contains 'sync-skia') `
    'New upstream Skia commits did not produce a sync action.'
Assert-True (@($blockedByUpstream.Actions.Kind) -notcontains 'start') `
    'A release was recommended before newer upstream Skia commits were synchronized.'
Assert-True ((@($blockedByUpstream.Actions.Command) -join "`n") -match
    'auto-skia-sync\.lock\.yml.*target=152') `
    'The upstream action did not target the resolved milestone.'
$currentUpstreamAllowsRelease = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -UpstreamSync $upstreamCurrent
Assert-True (@($currentUpstreamAllowsRelease.Actions.Kind) -contains 'start') `
    'An up-to-date upstream branch incorrectly blocked a release cut.'
$greenMaintenanceBuild = New-AuditPackageBuild `
    -Branch $maintenance.Branch `
    -Commit $maintenance.Sha `
    -State green `
    -Ready $true
$greenMaintenanceBuilds = @{}
$greenMaintenanceBuilds[$maintenance.Branch] = $greenMaintenanceBuild
$greenMaintenanceAllowsRelease = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -UpstreamSync $upstreamCurrent `
    -PackageBuilds $greenMaintenanceBuilds
Assert-True (@($greenMaintenanceAllowsRelease.Actions.Kind) -contains 'start') `
    'A green exact-tip maintenance build did not allow the release cut.'
$failedMaintenanceBuild = New-AuditPackageBuild `
    -Branch $maintenance.Branch `
    -Commit $maintenance.Sha `
    -State failed `
    -Ready $false
$failedMaintenanceBuilds = @{}
$failedMaintenanceBuilds[$maintenance.Branch] = $failedMaintenanceBuild
$failedMaintenanceBlocksRelease = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -UpstreamSync $upstreamCurrent `
    -PackageBuilds $failedMaintenanceBuilds
Assert-True (@($failedMaintenanceBlocksRelease.Actions.Kind) -contains 'fix-build') `
    'A failed maintenance build did not produce a build repair action.'
Assert-True (@($failedMaintenanceBlocksRelease.Actions.Kind) -notcontains 'start') `
    'A failed maintenance build incorrectly allowed a release cut.'
$incompleteNativeSyncBlocksRelease = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -UpstreamSync $upstreamCurrent -IncomingPullRequest $incompleteNativeSync
Assert-True (@($incompleteNativeSyncBlocksRelease.Actions.Kind) -contains 'investigate-sync') `
    'A mono/skia-only sync did not produce an investigation action.'
Assert-True (@($incompleteNativeSyncBlocksRelease.Actions.Kind) -notcontains 'start') `
    'A release was recommended while a mono/skia-only sync was incomplete.'
$blockedBySync = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -IncomingPullRequest $openSyncPullRequest
Assert-True (@($blockedBySync.Actions.Kind) -contains 'merge-sync') `
    'An incoming maintenance PR did not produce a merge action.'
Assert-True (@($blockedBySync.Actions.Kind) -notcontains 'start') `
    'A new release was recommended before the incoming maintenance PR was merged.'
$draftDoesNotBlock = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{ $stableTag = $stableRelease } `
    -Delta $realChanges -IncomingPullRequest $draftSyncPullRequest
Assert-True (@($draftDoesNotBlock.Actions.Kind) -contains 'start') `
    'A draft incoming sync PR incorrectly suppressed a release cut.'

$oldPreview = New-AuditBranch '4.153.0-preview.1' $sha0
$latestRc = New-AuditBranch '4.153.0-rc.1' $sha1
$latestUnpublished = Get-ReleaseAuditState `
    -Line '4.153' -Maintenance ([pscustomobject] @{ Branch = 'main'; Sha = $sha1; Version = '4.153.0' }) `
    -Branches @($oldPreview, $latestRc) -Packages @() -TagShas @{} -GitHubReleases @{} `
    -Delta (New-AuditDelta '1 queued release commit' 1)
Assert-Equal 'superseded' ($latestUnpublished.Releases | Where-Object Branch -eq $oldPreview.Name).State `
    'An older unpublished branch was not superseded.'
Assert-True (@($latestUnpublished.Actions.Kind) -contains 'publish') 'The latest unpublished branch was not publishable.'
Assert-True (@($latestUnpublished.Actions.Kind) -contains 'queued') 'Maintenance changes were not queued behind an unpublished branch.'
Assert-True (@($latestUnpublished.Actions.Kind) -notcontains 'start') 'An unpublished latest branch incorrectly started another release.'
$failedReleaseBuild = New-AuditPackageBuild `
    -Branch $latestRc.Name `
    -Commit $latestRc.Sha `
    -State failed `
    -Ready $false `
    -BuildId 201 `
    -BarId 0
$failedReleaseBuilds = @{}
$failedReleaseBuilds[$latestRc.Name] = $failedReleaseBuild
$failedReleaseBuildState = Get-ReleaseAuditState `
    -Line '4.153' -Maintenance ([pscustomobject] @{ Branch = 'main'; Sha = $sha1; Version = '4.153.0' }) `
    -Branches @($oldPreview, $latestRc) -Packages @() -TagShas @{} -GitHubReleases @{} `
    -Delta (New-AuditDelta '1 queued release commit' 1) `
    -PackageBuilds $failedReleaseBuilds
Assert-True (@($failedReleaseBuildState.Actions.Kind) -contains 'fix-build') `
    'A failed release-branch build did not block publication.'
Assert-True (@($failedReleaseBuildState.Actions.Kind) -notcontains 'publish') `
    'A failed release-branch build incorrectly allowed publication.'
$greenReleaseBuild = New-AuditPackageBuild `
    -Branch $latestRc.Name `
    -Commit $latestRc.Sha `
    -State green `
    -Ready $true `
    -BuildId 202 `
    -BarId 330714
$greenReleaseBuilds = @{}
$greenReleaseBuilds[$latestRc.Name] = $greenReleaseBuild
$greenReleaseBuildState = Get-ReleaseAuditState `
    -Line '4.153' -Maintenance ([pscustomobject] @{ Branch = 'main'; Sha = $sha1; Version = '4.153.0' }) `
    -Branches @($oldPreview, $latestRc) -Packages @() -TagShas @{} -GitHubReleases @{} `
    -Delta (New-AuditDelta '1 queued release commit' 1) `
    -PackageBuilds $greenReleaseBuilds
Assert-True (@($greenReleaseBuildState.Actions.Kind) -contains 'publish') `
    'A green release-branch build did not allow publication.'
Assert-True ((@($greenReleaseBuildState.Actions.Message) -join "`n") -match
    'build #202 succeeded with BAR 330714') `
    'The publication action omitted exact build and BAR evidence.'

$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{} -GitHubReleases @{ $stableTag = $stableRelease } -Delta (New-AuditDelta 'none')
Assert-True (@($state.Actions.Kind) -contains 'finish') 'A public package without a tag did not require Finish.'
$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha1 } -GitHubReleases @{ $stableTag = $stableRelease } -Delta (New-AuditDelta 'none')
Assert-True (@($state.Actions.Kind) -contains 'finish') 'A mismatched tag did not require Finish.'
$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stablePackage) `
    -TagShas @{ $stableTag = $sha0 } -GitHubReleases @{} -Delta (New-AuditDelta 'none')
Assert-True (@($state.Actions.Kind) -contains 'finish') 'A missing GitHub Release did not require Finish.'
$stalePackage = New-AuditPackage '4.152.0' $stable.Name $sha1
$state = Get-ReleaseAuditState `
    -Line '4.152' -Maintenance $maintenance -Branches @($stable) -Packages @($stalePackage) `
    -TagShas @{ $stableTag = $sha1 } -GitHubReleases @{ $stableTag = (New-AuditRelease $stableTag $sha1) } `
    -Delta (New-AuditDelta 'none')
Assert-True (@($state.Actions.Kind) -contains 'investigate') `
    'A public package from another commit did not require provenance investigation.'
Assert-True (@($state.Actions.Kind) -notcontains 'publish') `
    'A public package from another commit incorrectly suggested republishing an immutable version.'

$preview = New-AuditBranch '4.153.0-preview.1' $sha0
$previewPackages = @(
    New-AuditPackage '4.153.0-preview.1.1' $preview.Name $sha0
    New-AuditPackage '4.153.0-preview.1.2' $preview.Name $sha0
)
$state = Get-ReleaseAuditState `
    -Line '4.153' -Maintenance $null -Branches @($preview) -Packages $previewPackages `
    -TagShas @{ 'v4.153.0-preview.1.1' = $sha0; 'v4.153.0-preview.1.2' = $sha0 } `
    -GitHubReleases @{
        'v4.153.0-preview.1.1' = New-AuditRelease 'v4.153.0-preview.1.1' $sha0 $true
        'v4.153.0-preview.1.2' = New-AuditRelease 'v4.153.0-preview.1.2' $sha0 $true
    } -Delta (New-AuditDelta 'none')
Assert-Equal 2 @($state.Releases | Where-Object NuGet -match 'preview').Count `
    'Exact public prerelease shipments were collapsed.'

$mainPreview = New-AuditBranch '4.154.0-preview.1' $sha0
$mainPreviewPackage = New-AuditPackage '4.154.0-preview.1.1' $mainPreview.Name $sha0
$mainPreviewTag = 'v4.154.0-preview.1.1'
$state = Get-ReleaseAuditState `
    -Line '4.154' -Maintenance $mainMaintenance -Branches @($mainPreview) -Packages @($mainPreviewPackage) `
    -TagShas @{ $mainPreviewTag = $sha0 } `
    -GitHubReleases @{ $mainPreviewTag = (New-AuditRelease $mainPreviewTag $sha0 $true) } `
    -Delta (New-AuditDelta '1 queued release commit' 1)
Assert-True ((@($state.Actions.Command) -join "`n") -match '-Base main') `
    'Matching main was not used when no servicing branch exists.'
$inactive = Get-ReleaseAuditState `
    -Line '4.150' -Maintenance $null -Branches @() -Packages @() -TagShas @{} -GitHubReleases @{} `
    -Delta (New-AuditDelta 'none')
Assert-Equal 0 $inactive.Actions.Count 'An inactive old release line should not invent work.'
$firstRelease = Get-ReleaseAuditState `
    -Line '4.155' `
    -Maintenance ([pscustomobject] @{
        Branch = 'main'
        Sha = $sha1
        Version = '4.155.0'
        SkiaMilestone = 155
    }) `
    -Branches @() `
    -Packages @() `
    -TagShas @{} `
    -GitHubReleases @{} `
    -Delta (New-AuditDelta 'none')
Assert-True ((@($firstRelease.Actions.Command) -join "`n") -match '4\.155\.0-preview\.1') `
    'A maintained line with no release branch did not recommend its first preview.'
$latestUnpublishedReport = @(Write-ReleaseAuditReport $latestUnpublished) -join "`n"
$plainLatestUnpublishedReport = [regex]::Replace(
    $latestUnpublishedReport,
    "$([char] 0x1b)\[[0-9;]*m",
    '')
Assert-True ($latestUnpublishedReport -notmatch 'publish release branch; Publish release branch') `
    'The human-readable report duplicated equivalent state and action text.'
Assert-True ($plainLatestUnpublishedReport -match '(?m)^Release line:\s+4\.153$') `
    'The report summary fields were not aligned.'
Assert-True ($plainLatestUnpublishedReport -match
    '(?m)^Identity\s+Branch SHA\s+NuGet\s+Build / BAR\s+Tag\s+GitHub Release\s+State / action$') `
    'The report did not render an aligned PowerShell table.'
Assert-True ($plainLatestUnpublishedReport -notmatch '\| Identity \|') `
    'The report still rendered the old Markdown table.'
$incomingPullRequestReport = [regex]::Replace(
    (@(Write-ReleaseAuditReport $blockedBySync) -join "`n"),
    "$([char] 0x1b)\[[0-9;]*m",
    '')
Assert-True ($incomingPullRequestReport -match '(?m)^Incoming sync PR:\s+#5062 ') `
    'The incoming maintenance PR was not shown in the report summary.'
$upstreamReport = [regex]::Replace(
    (@(Write-ReleaseAuditReport $blockedByUpstream) -join "`n"),
    "$([char] 0x1b)\[[0-9;]*m",
    '')
Assert-True ($upstreamReport -match
    '(?m)^Upstream Skia:\s+chrome/m152@a{12} -> mono/skia:release/4\.152\.x \(1 newer commit\)') `
    'The report did not show the newer upstream Skia commit.'
$pendingReport = [regex]::Replace(
    (@(Write-ReleaseAuditReport $pendingState) -join "`n"),
    "$([char] 0x1b)\[[0-9;]*m",
    '')
Assert-True ($pendingReport -match
    '(?m)^Pending milestone:\s+4\.155 via m155 -> main \(current 4\.154\.0/m154\)') `
    'The report did not explain the pending next milestone.'
Assert-True ($pendingReport -match '(?m)^Incoming sync PR:\s+#5081 ') `
    'The report did not show the pending next-milestone PR.'
Assert-Equal 'main' $mainMaintenance.Branch 'A matching current main was not usable as maintenance.'
Assert-Equal '4.154.0-preview.1' (Get-NextReleaseIdentity '4.154' $null '4.154.0') `
    'A first release identity was not preview.1.'
Assert-Equal '4.154.0-preview.2' (Get-NextReleaseIdentity '4.154' (New-AuditBranch '4.154.0-preview.1' $sha0) '4.154.0') `
    'Preview.1 did not advance to preview.2.'
Assert-Equal '4.154.0-rc.1' (Get-NextReleaseIdentity '4.154' (New-AuditBranch '4.154.0-preview.2' $sha0) '4.154.0') `
    'Later previews did not advance to RC.1.'
Assert-Equal '4.154.0-stable' (Get-NextReleaseIdentity '4.154' (New-AuditBranch '4.154.0-rc.1' $sha0) '4.154.0') `
    'RC did not advance to stable.'

Write-Output "All $script:TestsRun publishing script tests passed."
