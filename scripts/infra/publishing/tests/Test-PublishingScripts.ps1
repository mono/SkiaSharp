#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$publishingRoot = Split-Path $PSScriptRoot
$gitCommonPath = Join-Path $publishingRoot 'Git.Common.psm1'
$gitHubCommonPath = Join-Path $publishingRoot 'GitHub.Common.psm1'
$maestroCommonPath = Join-Path $publishingRoot 'Maestro.Common.psm1'
$commonPath = Join-Path $publishingRoot 'Publishing.Common.psm1'
$preparePath = Join-Path $publishingRoot 'prepare-release.ps1'
$finishPath = Join-Path $publishingRoot 'finish-release.ps1'
$bugTemplatePath = Join-Path $publishingRoot 'update-bug-template.ps1'
$reconcilePath = Join-Path $publishingRoot 'reconcile-release-assignments.ps1'
$milestonesPath = Join-Path $publishingRoot 'update-release-milestones.ps1'
$auditPath = Join-Path $publishingRoot 'audit-release-state.ps1'
$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../../../..')
$prepareWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-prepare.yml'
$finishWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-finish.yml'
$milestonesWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-milestones.yml'

Import-Module $gitCommonPath -Force
Import-Module $gitHubCommonPath -Force
Import-Module $maestroCommonPath -Force
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
$auditCommand = Get-Command $auditPath
$auditParameters = $auditCommand.Parameters.Keys
Assert-True ($prepareParameters -contains 'Mode' -and $prepareParameters -notcontains 'Check' -and
    $prepareParameters -notcontains 'Apply' -and $prepareParameters -notcontains 'Push') `
    'Prepare must expose Check through its Mode parameter only.'
Assert-True ($finishParameters -contains 'Mode' -and $finishParameters -notcontains 'Check' -and
    $finishParameters -notcontains 'Apply' -and $finishParameters -notcontains 'Push') `
    'Finish must expose Check through its Mode parameter only.'
Assert-True ($bugTemplateParameters -contains 'Mode' -and
    $bugTemplateParameters -notcontains 'Apply' -and $bugTemplateParameters -notcontains 'Push') `
    'The bug-template updater must expose only the three-state Mode parameter.'
Assert-True ($reconcileParameters -contains 'Version' -and $reconcileParameters -contains 'Mode' -and
    $reconcileParameters -notcontains 'Push' -and $reconcileParameters -notcontains 'Check' -and
    $reconcileParameters -notcontains 'Apply') 'Assignment reconciliation must expose its modes through Mode only.'
Assert-True ($milestoneParameters -contains 'Count' -and $milestoneParameters -contains 'Mode' -and
    $milestoneParameters -notcontains 'Push' -and $milestoneParameters -notcontains 'Check' -and
    $milestoneParameters -notcontains 'Apply' -and $milestoneParameters -notcontains 'Version') `
    'The milestone updater must expose its modes through Mode only.'
Assert-True ($auditParameters -contains 'Version' -and $auditParameters -contains 'Discover' -and
    $auditParameters -contains 'MaxAge' -and $auditParameters -contains 'Quiet' -and
    $auditParameters -contains 'Json' -and $auditParameters -contains 'IncludeMilestoneAssignments' -and
    $auditParameters -notcontains 'Push' -and $auditParameters -notcontains 'Mode' -and
    $auditParameters -notcontains 'Apply') `
    'The release-state audit must expose read-only target selection and no mutation mode.'
Assert-Equal 'System.String' $auditCommand.Parameters['Version'].ParameterType.FullName `
    'The release-state audit Version parameter must be one literal wildcard string.'
$auditScript = Get-Content $auditPath -Raw
Assert-True ($auditScript.Contains('Get-RemoteBranches') -and
    $auditScript.Contains('Get-NuGetPackageVersions') -and
    $auditScript.Contains('Get-MaestroReleaseReceiptForBranch') -and
    $auditScript.Contains('Get-MaestroReleaseReceipt') -and
    $auditScript.Contains('Get-NuGetPublicationReceipt') -and
    $auditScript.Contains('IncludeMilestoneAssignments') -and
    $auditScript -match 'if \(\$IncludeMilestoneAssignments -and \$powerShell\)' -and
    $auditScript -match "State 'skipped'" -and
    $auditScript.Contains('Invoke-OwnerCheck') -and
    $auditScript -notmatch 'ReleaseAudit\.Common|release_state_audit\.py') `
    'The release-state audit must coordinate owner checks without snapshot parsing.'
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
$milestonesWorkflow = Get-Content $milestonesWorkflowPath -Raw
Assert-True ($milestonesWorkflow.Contains("MODE: `${{ inputs.push && 'Push' || 'DryRun' }}") -and
    $milestonesWorkflow -notmatch '\$arguments\.Push') `
    'The milestone workflow does not map its push checkbox through Mode.'
$bugTemplateScript = Get-Content $bugTemplatePath -Raw
$commonScript = Get-Content $commonPath -Raw
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
$script:NuGetCatalogueUri = $null
function global:Invoke-RestMethod {
    param([string] $Uri)
    $script:NuGetCatalogueUri = $Uri
    return [pscustomobject] @{
        versions = @(
            '4.153.0-preview.1.26454.6',
            '4.153.0-preview.1.26454.7'
        )
    }
}
try {
    Assert-Equal @(
        '4.153.0-preview.1.26454.6',
        '4.153.0-preview.1.26454.7'
    ) @(Get-NuGetPackageVersions 'SkiaSharp') `
        'The shared NuGet catalogue reader did not return all public versions.'
    Assert-True ($script:NuGetCatalogueUri -match '/skiasharp/index\.json$') `
        'The shared NuGet catalogue reader did not use the SkiaSharp flat container.'
} finally {
    Remove-Item Function:\Invoke-RestMethod
}
$releaseMilestone = ConvertTo-ReleaseMilestone '4.153.0-preview.1'
Assert-Equal '4.153.0' $releaseMilestone.Numeric `
    'Release milestone parsing did not retain the numeric release core.'

# The coordinator must retain every exact shipment and select only the
# identities explicitly named by a PowerShell wildcard.
Invoke-Expression (Get-ScriptFunctionText $auditPath)
$auditKnown = @(
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.152.0-preview.1') '4.152.0-preview.1.26426.14'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0-preview.1') $null
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0-preview.1') '4.153.0-preview.1.26454.6'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0-preview.1') '4.153.0-preview.1.26454.7'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0-rc.1') '4.153.0-rc.1.26455.1'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0') '4.153.0'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.153.0.1') '4.153.0.1'
    New-AuditTarget (ConvertTo-ReleaseMilestone '4.154.0-preview.1') '4.154.0-preview.1.26456.1'
)
Assert-Equal @(
    '4.153.0-preview.1',
    '4.153.0-preview.1.26454.6',
    '4.153.0-preview.1.26454.7',
    '4.153.0-rc.1.26455.1',
    '4.153.0',
    '4.153.0.1'
) @((Get-AuditTargetMatches $auditKnown '4.153.*').Value) `
    'A literal wildcard did not expand all matching release identities.'
Assert-Equal 8 @(Get-AuditTargetMatches $auditKnown '4.15*').Count `
    'A broad literal wildcard did not select all matching release identities.'
Assert-Equal @('4.153.0-preview.1.26454.6') @(
    (Get-AuditTargetMatches $auditKnown '4.153.0-preview.1.26454.6').Value
) 'An exact public shipment selected another build.'
$auditTargets = @{}
$floor = ConvertTo-ReleaseMilestone '4.153.0'
foreach ($target in $auditKnown) {
    Add-AuditTarget $auditTargets $target.Release $target.PublicVersion $floor
}
Assert-Equal 7 $auditTargets.Count `
    'The audit did not preserve all in-scope exact shipments or exclude the pre-floor release.'
Assert-True ($auditTargets.ContainsKey('public:4.153.0-preview.1.26454.6') -and
    $auditTargets.ContainsKey('public:4.153.0-preview.1.26454.7')) `
    'The audit collapsed multiple exact public builds into one target.'
$barAssets = @(
    [pscustomobject] @{
        name = 'SkiaSharp'
        version = '4.153.0-preview.1.26454.6'
        build = [pscustomobject] @{
            id = 123
            branch = 'refs/heads/release/4.153.0-preview.1'
            commit = ('a' * 40) -join ''
            released = $false
            channels = @('.NET Libraries')
            buildNumber = '4.153.0-preview.1.26454.6'
            buildLink = 'https://example.invalid/build/123'
        }
        locations = @('https://pkgs.dev.azure.com/dnceng/public/_packaging/test/nuget/v3/index.json')
    }
)
$barBuild = Resolve-MaestroReleaseBuild `
    -Assets $barAssets `
    -Version '4.153.0-preview.1.26454.6' `
    -BarId 0
Assert-Equal 'complete' $barBuild.State 'A unique promoted BAR was not selected.'
Assert-Equal 'release/4.153.0-preview.1' $barBuild.Value.Branch `
    'A BAR source branch was not normalized.'
Assert-Equal $false $barBuild.Value.IsReleased `
    'The BAR receipt incorrectly treats the unrelated released flag as channel promotion.'
Assert-Equal 'complete' (Resolve-MaestroReleaseBuild `
    -Assets $barAssets `
    -Version '4.153.0-preview.1.26454.6' `
    -BarId 0 `
    -ExpectedBranch 'release/4.153.0-preview.1' `
    -ExpectedCommit $barAssets[0].build.commit).State `
    'A BAR matching its release branch tip was rejected.'
$mismatchedBar = Resolve-MaestroReleaseBuild `
    -Assets $barAssets `
    -Version '4.153.0-preview.1.26454.6' `
    -BarId 0 `
    -ExpectedBranch 'release/4.153.0-preview.1' `
    -ExpectedCommit (('b' * 40) -join '')
Assert-Equal 'pending' $mismatchedBar.State `
    'A BAR at a different commit was accepted for the release branch tip.'
Assert-True ($mismatchedBar.Message -match 'release/4\.153\.0-preview\.1@a{40}' -and
    $mismatchedBar.Message -match 'release/4\.153\.0-preview\.1@b{40}') `
    'A branch-tip mismatch did not name both Darc and expected commits.'
Assert-Equal 'pending' (Resolve-MaestroReleaseBuild `
    -Assets @() `
    -Version '4.153.0-preview.1.26454.6' `
    -BarId 0).State 'A missing BAR was not reported as pending release state.'
$unchanneledAssets = @(
    [pscustomobject] @{
        name = $barAssets[0].name
        version = $barAssets[0].version
        build = [pscustomobject] @{
            id = $barAssets[0].build.id
            branch = $barAssets[0].build.branch
            commit = $barAssets[0].build.commit
            released = $true
            channels = @()
        }
        locations = $barAssets[0].locations
    }
)
Assert-Equal 'pending' (Resolve-MaestroReleaseBuild `
    -Assets $unchanneledAssets `
    -Version '4.153.0-preview.1.26454.6' `
    -BarId 0).State 'A BAR outside the .NET Libraries channel was accepted as a release receipt.'
$barPackages = @(
    [pscustomobject] @{
        Id = 'SkiaSharp'
        Branch = 'release/4.153.0-preview.1'
        Commit = ('a' * 40) -join ''
    }
    [pscustomobject] @{
        Id = 'SkiaSharp.HarfBuzz'
        Branch = 'release/4.153.0-preview.1'
        Commit = ('a' * 40) -join ''
    }
    [pscustomobject] @{
        Id = 'HarfBuzzSharp'
        Branch = 'release/4.153.0-preview.1'
        Commit = ('a' * 40) -join ''
    }
)
Assert-Equal 'complete' (Test-MaestroPackageSources `
    -Build $barBuild.Value `
    -Packages $barPackages).State 'Matching BAR package sources were rejected.'
$barPackages[2].Commit = ('b' * 40) -join ''
Assert-Equal 'pending' (Test-MaestroPackageSources `
    -Build $barBuild.Value `
    -Packages $barPackages).State 'A mismatched BAR package source was accepted.'
$testPowerShell = Get-Command pwsh -CommandType Application |
    Select-Object -First 1 -ExpandProperty Source
$completeOwnerCheck = Invoke-OwnerCheck `
    -Target 'test' `
    -Phase 'owner' `
    -Executable $testPowerShell `
    -Arguments @('-NoLogo', '-NoProfile', '-Command', 'exit 0') `
    -WorkingDirectory $repositoryRoot
Assert-Equal 0 $completeOwnerCheck.Code `
    'The audit coordinator did not preserve a completed child process status.'
$pendingOwnerCheck = Invoke-OwnerCheck `
    -Target 'test' `
    -Phase 'owner' `
    -Executable $testPowerShell `
    -Arguments @('-NoLogo', '-NoProfile', '-Command', 'exit 1') `
    -WorkingDirectory $repositoryRoot
Assert-Equal 'pending' $pendingOwnerCheck.State `
    'The audit coordinator did not preserve a pending child process status.'
$script:ShowAuditProgress = $true
$progressRecords = @(Invoke-OwnerCheck `
    -Target 'test' `
    -Phase 'owner' `
    -Executable $testPowerShell `
    -Arguments @('-NoLogo', '-NoProfile', '-Command', 'exit 0') `
    -WorkingDirectory $repositoryRoot 6>&1)
$script:ShowAuditProgress = $false
$progressText = $progressRecords -join "`n"
Assert-True ($progressText -match '\[checking\] test: owner' -and
    $progressText -match '\[complete\] test: owner') `
    'The audit coordinator did not report child-check progress.'
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

$script:ReleaseViewArguments = @()
function global:gh {
    $script:ReleaseViewArguments = @($args)
    $global:LASTEXITCODE = 0
    [pscustomobject]@{
        tagName = 'v4.153.0-preview.1.26454.6'
        name = 'Version 4.153.0 (Preview 1)'
        isDraft = $true
        isPrerelease = $true
        targetCommitish = 'f82c5845d7ee6e5ba415cec351d8ec640fb0fd8c'
        body = 'Draft body'
        url = 'https://example.invalid/release'
    } | ConvertTo-Json -Compress
}
try {
    $draftRelease = Get-GitHubRelease `
        -Repository 'mono/SkiaSharp' `
        -Tag 'v4.153.0-preview.1.26454.6'
} finally {
    Remove-Item Function:\gh
}
Assert-Equal $true $draftRelease.isDraft 'GitHub release reads must preserve draft state for the audit.'
Assert-True (($script:ReleaseViewArguments -join ' ') -match '--json .*isDraft') `
    'GitHub release reads did not request draft state.'

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
    & git -C $gitRoot branch release/test
    & git init --quiet --bare $bareRoot
    $localSha = (git -C $gitRoot rev-parse release/test).Trim()
    Assert-Equal $localSha (Get-LocalBranchSha -Root $gitRoot -Branch release/test) `
        'A local branch SHA was not resolved.'
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
    Remove-Item $gitRoot, $bareRoot -Recurse -Force -ErrorAction SilentlyContinue
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

Write-Output "All $script:TestsRun publishing script tests passed."
$global:LASTEXITCODE = 0
