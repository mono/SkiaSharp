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
Assert-True ($prepareParameters -contains 'Apply' -and $prepareParameters -contains 'Push') `
    'Prepare must expose Apply and Push.'
Assert-True ($finishParameters -contains 'Push' -and $finishParameters -notcontains 'Apply') `
    'Finish must expose Push but not Apply.'
Assert-True ($bugTemplateParameters -contains 'Apply' -and $bugTemplateParameters -contains 'Push') `
    'The bug-template updater must expose Apply and Push.'
Assert-True ($reconcileParameters -contains 'Version' -and $reconcileParameters -contains 'Push' -and
    $reconcileParameters -notcontains 'Apply') 'Assignment reconciliation must expose Version and Push but not Apply.'
Assert-True ($milestoneParameters -contains 'Count' -and $milestoneParameters -contains 'Push' -and
    $milestoneParameters -notcontains 'Apply' -and $milestoneParameters -notcontains 'Version') `
    'The milestone updater must expose Count and Push but not Apply or Version.'
Assert-RejectsApply $finishPath @('-Version', '4.152.0-preview.1')
Assert-RejectsApply $reconcilePath @('-Version', '4.152.0')
Assert-RejectsApply $milestonesPath @()
$bugTemplateScript = Get-Content $bugTemplatePath -Raw
Assert-True ($bugTemplateScript -match '--force-with-lease') `
    'The issue-template automation branch must use force-with-lease.'
Assert-True ($bugTemplateScript -notmatch '(?m)git push[^\r\n]*--force(?:\s|$)') `
    'The issue-template automation branch contains an unguarded force push.'
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
} finally {
    Remove-Item $gitRoot, $bareRoot -Recurse -Force -ErrorAction SilentlyContinue
}

# Loads and exercises Prepare's pure version transformation functions.
Invoke-Expression (Get-ScriptFunctionText $preparePath)
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

# Loads and exercises Finish's managed-body and dry-run behavior.
Invoke-Expression (Get-ScriptFunctionText $finishPath)
$summaryStart = $ReleaseSummaryStartMarker
$summaryEnd = $ReleaseSummaryEndMarker
$generatedStart = $ReleaseGeneratedStartMarker
$generatedEnd = $ReleaseGeneratedEndMarker
$managedBody = "$summaryStart`nsummary`n$summaryEnd`n$generatedStart`nnotes`n$generatedEnd"
Assert-Equal $null (Assert-ManagedBody $managedBody) 'A valid managed release body was rejected.'
Assert-Throws { Assert-ManagedBody "$summaryStart`n$summaryEnd" } 'must contain exactly one' `
    'An incomplete managed release body was accepted.'
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
    body = $managedBody
}
Assert-Equal $null (Assert-GitHubRelease $preview $draft) 'A valid managed draft was rejected.'
$draft.body = 'unmanaged'
Assert-Throws { Assert-GitHubRelease $preview $draft } 'must contain exactly one' `
    'An unmanaged draft was accepted.'
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

    Assert-True (Test-IssueTemplateAutomationBranch `
        -Root $automationRoot `
        -RemoteSha $remoteSha `
        -MainSha $mainSha `
        -Path 'template.yml' `
        -Content "new`n") 'An identical automation branch was not reusable.'
    Assert-True (!(Test-IssueTemplateAutomationBranch `
        -Root $automationRoot `
        -RemoteSha $remoteSha `
        -MainSha $mainSha `
        -Path 'template.yml' `
        -Content "different`n")) 'A different automation branch was incorrectly reusable.'
} finally {
    Remove-Item $automationRoot, $automationBare -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Output "All $script:TestsRun publishing script tests passed."
