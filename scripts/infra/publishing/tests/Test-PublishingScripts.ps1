#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$publishingRoot = Split-Path $PSScriptRoot
$gitCommonPath = Join-Path $publishingRoot 'Git.Common.psm1'
$gitHubCommonPath = Join-Path $publishingRoot 'GitHub.Common.psm1'
$commonPath = Join-Path $publishingRoot 'Publishing.Common.psm1'
$preparePath = Join-Path $publishingRoot 'prepare-release.ps1'
$finishPath = Join-Path $publishingRoot 'finish-release.ps1'
$bugTemplatePath = Join-Path $publishingRoot 'update-bug-template.ps1'
$reconcilePath = Join-Path $publishingRoot 'reconcile-release-assignments.ps1'
$milestonesPath = Join-Path $publishingRoot 'update-release-milestones.ps1'
$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../../../..')
$prepareWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-prepare.yml'
$finishWorkflowPath = Join-Path $repositoryRoot '.github/workflows/release-finish.yml'

Import-Module $gitCommonPath -Force
Import-Module $gitHubCommonPath -Force
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
$identityConfig = Get-Content (Join-Path $repositoryRoot 'scripts/infra/repository-identity.json') -Raw |
    ConvertFrom-Json
$configuredSkiaUrl = (& git -C $repositoryRoot config -f .gitmodules --get submodule.externals/skia.url).Trim()
$configuredSkiaRepository = [regex]::Match(
    $configuredSkiaUrl,
    '(?i)github\.com[/:](?<repository>[^/:\s]+/[^/\s]+?)(?:\.git)?$'
).Groups['repository'].Value
$expectedSkiaRemote = "https://github.com/$configuredSkiaRepository.git"
$expectedCurrentRepository = if ($env:GITHUB_REPOSITORY) {
    $env:GITHUB_REPOSITORY
} else {
    $identityConfig.offlineRepository
}
Assert-Equal $expectedCurrentRepository $ReleaseRepository `
    'The current repository did not follow runtime context or the configured fallback.'
Assert-Equal $expectedSkiaRemote $ReleaseSkiaRemote `
    'The paired Skia remote was not loaded from .gitmodules.'
$previousRepository = $env:GITHUB_REPOSITORY
try {
    Remove-Item Env:\GITHUB_REPOSITORY -ErrorAction SilentlyContinue
    $offlineIdentity = @(
        pwsh -NoLogo -NoProfile -Command (
            "Import-Module '$commonPath' -Force; " +
            'Write-Output "$ReleaseRepository|$ReleaseSkiaRemote"')
    ) -join "`n"
    $env:GITHUB_REPOSITORY = 'dotnet/SkiaSharp'
    $runtimeIdentity = @(
        pwsh -NoLogo -NoProfile -Command (
            "Import-Module '$commonPath' -Force; " +
            'Write-Output "$ReleaseRepository|$ReleaseSkiaRemote"')
    ) -join "`n"
} finally {
    $env:GITHUB_REPOSITORY = $previousRepository
}
Assert-Equal "$($identityConfig.offlineRepository)|$expectedSkiaRemote" $offlineIdentity.Trim() `
    'The offline repository fallback was not loaded.'
Assert-Equal "dotnet/SkiaSharp|$expectedSkiaRemote" $runtimeIdentity.Trim() `
    'The runtime repository did not override only the current repository identity.'
$transferRoot = Join-Path $PSScriptRoot ".identity-transfer-$([guid]::NewGuid().ToString('N'))"
try {
    $null = New-Item -ItemType Directory -Path $transferRoot
    @'
[submodule "externals/skia"]
	path = externals/skia
	url = https://github.com/dotnet/skia.git
[submodule "docs"]
	path = docs
	url = https://github.com/dotnet/SkiaSharp-API-docs
'@ | Set-Content (Join-Path $transferRoot '.gitmodules')
    $transferConfig = $identityConfig.PSObject.Copy()
    $transferConfig.offlineRepository = 'dotnet/SkiaSharp'
    $transferConfig.publicSiteBaseUrl = 'https://docs.example/SkiaSharp'
    $transferConfig | ConvertTo-Json -Depth 10 |
        Set-Content (Join-Path $transferRoot 'repository-identity.json')
    $oldConfig = $env:SKIASHARP_IDENTITY_CONFIG
    $oldRoot = $env:SKIASHARP_REPOSITORY_ROOT
    $oldRepository = $env:GITHUB_REPOSITORY
    try {
        Remove-Item Env:\GITHUB_REPOSITORY -ErrorAction SilentlyContinue
        $env:SKIASHARP_IDENTITY_CONFIG = Join-Path $transferRoot 'repository-identity.json'
        $env:SKIASHARP_REPOSITORY_ROOT = $transferRoot
        $transferIdentity = @(
            pwsh -NoLogo -NoProfile -Command (
                "Import-Module '$commonPath' -Force; " +
                'Write-Output "$ReleaseRepository|$ReleaseSkiaRemote"')
        ) -join "`n"
    } finally {
        $env:SKIASHARP_IDENTITY_CONFIG = $oldConfig
        $env:SKIASHARP_REPOSITORY_ROOT = $oldRoot
        $env:GITHUB_REPOSITORY = $oldRepository
    }
    Assert-Equal 'dotnet/SkiaSharp|https://github.com/dotnet/skia.git' `
        $transferIdentity.Trim() 'The complete transfer identity fixture was not resolved.'
} finally {
    Remove-Item $transferRoot -Recurse -Force -ErrorAction SilentlyContinue
}
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
$pages = @(
    @([pscustomobject] @{ number = 1 }, [pscustomobject] @{ number = 2 }),
    @([pscustomobject] @{ number = 3 })
)
Assert-Equal @(1, 2, 3) @((Expand-GitHubPages $pages).number) 'GitHub pages were not flattened.'

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
    isDraft = $false
    isPrerelease = $true
    body = 'Historical body'
}
Assert-Equal $null (Assert-GitHubRelease $preview $publishedHistorical) `
    'A published historical release was rejected.'
$draft = [pscustomobject] @{
    tagName = $preview.Tag
    name = $preview.Title
    isDraft = $true
    isPrerelease = $true
    body = 'Draft notes'
}
Assert-Equal $null (Assert-GitHubRelease $preview $draft) 'A valid draft was rejected.'
$draft.name = 'Wrong title'
Assert-Throws { Assert-GitHubRelease $preview $draft } 'conflicting metadata' `
    'A draft with the wrong title was accepted.'
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
$followUpPlan = @(Invoke-ReleaseFollowUpWorkflows $preview 6>&1) -join "`n"
Assert-Equal 0 $script:FakeGhCalls 'Finish dry-run invoked gh.'
Assert-True ($publishPlan -match 'Create and publish') 'Finish did not plan release publication.'
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
