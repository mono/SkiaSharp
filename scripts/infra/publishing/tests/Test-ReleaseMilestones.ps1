#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$publishingRoot = Split-Path $PSScriptRoot
Import-Module (Join-Path $publishingRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $publishingRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $publishingRoot 'Publishing.Common.psm1') -Force

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

Invoke-Expression (Get-ScriptFunctionText (Join-Path $publishingRoot 'reconcile-release-assignments.ps1'))
Invoke-Expression (Get-ScriptFunctionText (Join-Path $publishingRoot 'update-release-milestones.ps1'))
$Push = $false
$writeRemote = $false
$moveSettleAttempts = 5
$moveSettleDelaySeconds = 0

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

$branches = @(
    ConvertTo-ReleaseMilestone 'release/4.152.0'
    ConvertTo-ReleaseMilestone 'release/4.152.0-rc.1'
    ConvertTo-ReleaseMilestone 'release/4.152.0-preview.2'
    ConvertTo-ReleaseMilestone 'release/4.152.0.1-preview.1'
    ConvertTo-ReleaseMilestone 'release/4.152.0.1'
) | Sort-Object SortKey
Assert-Equal @(
    '4.152.0-preview.2',
    '4.152.0-rc.1',
    '4.152.0',
    '4.152.0.1-preview.1',
    '4.152.0.1'
) @($branches.Title) 'Release branches were not sorted in shipping order.'

$rollForwardBranches = @(
    ConvertTo-ReleaseMilestone 'release/4.152.0-preview.1'
    ConvertTo-ReleaseMilestone 'release/4.152.0-preview.2'
    ConvertTo-ReleaseMilestone 'release/4.152.0-rc.1'
)
$effective = Get-EffectiveMilestoneTitles -Branches $rollForwardBranches -Tags @('v4.152.0-preview.2.1', 'v4.152.0-rc.1.1')
Assert-Equal @(
    '4.152.0-preview.2',
    '4.152.0-preview.2',
    '4.152.0-rc.1'
) @($effective) 'An unshipped preview did not roll forward.'

$greatestTag = Get-ShippedTag '4.152.0-preview.1' @(
    'v4.152.0-preview.1.10',
    'v4.152.0-preview.1.26426.14',
    'v4.152.0-preview.1.26426.2',
    'v4.152.0-preview.1.invalid'
)
Assert-Equal 'v4.152.0-preview.1.26426.14' $greatestTag 'The greatest dnceng build tuple was not selected.'
Assert-Equal 'v4.152.0' (Get-ShippedTag '4.152.0' @('v4.152.0')) 'A stable exact tag was not detected.'

$topologyBranches = @(
    ConvertTo-ReleaseMilestone 'release/4.152.0-preview.1'
    ConvertTo-ReleaseMilestone 'release/4.153.0-preview.1'
    ConvertTo-ReleaseMilestone 'release/4.153.0-rc.1'
    ConvertTo-ReleaseMilestone 'release/4.154.0-preview.1'
)
$topologyTags = @(
    'v4.152.0-preview.1.1',
    'v4.153.0-preview.1.1',
    'v4.153.0-rc.1.1',
    'v4.154.0-preview.1.1'
)
$shippedTopology = @(Get-ShippedReleases -Branches $topologyBranches -Tags $topologyTags)
Assert-Equal @(
    '4.152.0-preview.1',
    '4.153.0-preview.1',
    '4.153.0-rc.1',
    '4.154.0-preview.1'
) @($shippedTopology.Title) 'Shipped releases were not ordered by release identity.'

$boundaryRoot = Join-Path $PSScriptRoot ".boundary-test-$([guid]::NewGuid().ToString('N'))"
try {
    $null = New-Item -ItemType Directory -Path $boundaryRoot
    & git -C $boundaryRoot init --quiet
    & git -C $boundaryRoot config user.name 'Release Boundary Tests'
    & git -C $boundaryRoot config user.email 'release-boundaries@example.invalid'
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Previous release boundary'
    & git -C $boundaryRoot tag v4.152.0-preview.1.1
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Shared m153 line work (#4826)'
    $m153LinePoint = (& git -C $boundaryRoot rev-parse HEAD).Trim()
    & git -C $boundaryRoot branch m153-servicing
    & git -C $boundaryRoot branch m153-preview
    & git -C $boundaryRoot switch --quiet m153-preview
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Create m153 preview release branch'
    & git -C $boundaryRoot tag v4.153.0-preview.1.1
    & git -C $boundaryRoot switch --quiet m153-servicing
    & git -C $boundaryRoot branch m154-preview
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Later m153 servicing work (#5000)'
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Create later m153 RC release branch'
    & git -C $boundaryRoot tag v4.153.0-rc.1.1
    & git -C $boundaryRoot switch --quiet m154-preview
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'm154-only work (#4945)'
    & git -C $boundaryRoot commit --quiet --allow-empty -m 'Create m154 preview release branch'
    & git -C $boundaryRoot tag v4.154.0-preview.1.1

    $m153Preview = $shippedTopology | Where-Object Title -eq '4.153.0-preview.1'
    $m153PreviewBoundary = Get-PreviousShippedBoundary `
        -Root $boundaryRoot `
        -Releases $shippedTopology `
        -CurrentRelease $m153Preview
    Assert-Equal 'v4.152.0-preview.1.1' $m153PreviewBoundary.Tag `
        'A later numeric release became the boundary for an earlier release line.'
    Assert-Equal @(4826) @(Get-ReleasePullRequests `
        -Root $boundaryRoot `
        -Start $m153PreviewBoundary.Start `
        -End $m153PreviewBoundary.End) `
        'The m153 preview range did not begin at the previous release-line branch commit.'

    $m154Preview = $shippedTopology | Where-Object Title -eq '4.154.0-preview.1'
    $m154Boundary = Get-PreviousShippedBoundary `
        -Root $boundaryRoot `
        -Releases $shippedTopology `
        -CurrentRelease $m154Preview
    Assert-Equal $m153LinePoint $m154Boundary.Start `
        'A parallel m153 release did not preserve the shared m154 branch commit boundary.'
    Assert-Equal @(4945) @(Get-ReleasePullRequests `
        -Root $boundaryRoot `
        -Start $m154Boundary.Start `
        -End $m154Boundary.End) `
        'The m154 range included work from its parallel m153 release branch.'

    $m153Rc = $shippedTopology | Where-Object Title -eq '4.153.0-rc.1'
    $m153RcBoundary = Get-PreviousShippedBoundary `
        -Root $boundaryRoot `
        -Releases $shippedTopology `
        -CurrentRelease $m153Rc
    Assert-Equal @(5000) @(Get-ReleasePullRequests `
        -Root $boundaryRoot `
        -Start $m153RcBoundary.Start `
        -End $m153RcBoundary.End) `
        'A later m153 RC did not retain its own servicing-branch range.'
} finally {
    if (Test-Path -LiteralPath $boundaryRoot) {
        Remove-Item -LiteralPath $boundaryRoot -Recurse -Force
    }
}

$assignmentMilestones = @{
    '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
    '4.154.0-preview.1' = [pscustomobject] @{ number = 75; state = 'closed' }
}
$assignmentTags = @(
    'v4.153.0-preview.1.26454.6',
    'v4.154.0-preview.1.26454.9',
    'v4.153.0-rc.1.26455.1'
)
$pullAssignment = Get-ReleaseAssignmentPlan `
    -Kind 'pull-request' `
    -Number 4826 `
    -ViaPullRequest $null `
    -CurrentMilestone '4.154.0-preview.1' `
    -TargetMilestone '4.153.0-preview.1' `
    -Milestones $assignmentMilestones `
    -Tags $assignmentTags
Assert-Equal 'assign' $pullAssignment.Status 'A pull request could not be repaired to its first shipped milestone.'
Assert-Equal 74 $pullAssignment.Operation.ToMilestoneNumber 'A pull request repair targeted the wrong milestone.'

$issueAssignment = Get-ReleaseAssignmentPlan `
    -Kind 'issue' `
    -Number 1234 `
    -ViaPullRequest 4826 `
    -CurrentMilestone '4.154.0-preview.1' `
    -TargetMilestone '4.153.0-preview.1' `
    -Milestones $assignmentMilestones `
    -Tags $assignmentTags
Assert-Equal 'assign' $issueAssignment.Status 'A linked issue could not be repaired to its first shipped milestone.'
Assert-Equal 4826 $issueAssignment.Operation.ViaPullRequest 'A linked issue lost its source pull request.'

$unshippedRollForward = Get-ReleaseAssignmentPlan `
    -Kind 'pull-request' `
    -Number 2000 `
    -ViaPullRequest $null `
    -CurrentMilestone '4.152.0-preview.1' `
    -TargetMilestone '4.152.0-rc.1' `
    -Milestones @{
        '4.152.0-preview.1' = [pscustomobject] @{ number = 72; state = 'open' }
        '4.152.0-rc.1' = [pscustomobject] @{ number = 73; state = 'closed' }
    } `
    -Tags @('v4.152.0-rc.1.26426.14')
Assert-Equal 'assign' $unshippedRollForward.Status `
    'An unshipped preview could not roll forward to the next shipped milestone in its numeric line.'

foreach ($unsafe in @(
    Get-ReleaseAssignmentPlan `
        -Kind 'pull-request' `
        -Number 4826 `
        -ViaPullRequest $null `
        -CurrentMilestone '4.153.0-preview.1' `
        -TargetMilestone '4.154.0-preview.1' `
        -Milestones $assignmentMilestones `
        -Tags $assignmentTags
    Get-ReleaseAssignmentPlan `
        -Kind 'issue' `
        -Number 1234 `
        -ViaPullRequest 4826 `
        -CurrentMilestone '4.153.0-preview.1' `
        -TargetMilestone '4.154.0-preview.1' `
        -Milestones $assignmentMilestones `
        -Tags $assignmentTags
)) {
    Assert-Equal 'blocked' $unsafe.Status `
        'A forward move out of an earlier closed and shipped milestone was not blocked.'
    Assert-True ($unsafe.Warning -match 'Refusing to move .* from earlier milestone') `
        'A blocked assignment did not explain the safety invariant.'
}

$closedOnlyGuard = Get-ReleaseAssignmentPlan `
    -Kind 'issue' `
    -Number 4321 `
    -ViaPullRequest 2000 `
    -CurrentMilestone '4.152.0-preview.1' `
    -TargetMilestone '4.153.0-preview.1' `
    -Milestones @{
        '4.152.0-preview.1' = [pscustomobject] @{ number = 72; state = 'closed' }
        '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
    } `
    -Tags @('v4.153.0-preview.1.26454.6')
Assert-Equal 'blocked' $closedOnlyGuard.Status `
    'A forward move out of an earlier closed milestone was not blocked without a shipped tag.'

$schedule = [pscustomobject] @{
    branch_point = '2026-07-27T00:00:00Z'
    earliest_beta = '2026-07-29T00:00:00Z'
    early_stable_cut = '2026-08-04T00:00:00Z'
    stable_cut = '2026-08-11T00:00:00Z'
    stable_date = '2026-08-18T00:00:00Z'
}
$desired = New-DesiredReleaseMilestones -Schedule $schedule -Milestone 152 -Major 4
Assert-Equal @(
    '4.152.0-preview.1',
    '4.152.0-preview.2',
    '4.152.0-rc.1',
    '4.152.0'
) @($desired.Title) 'Chromium stages were not mapped to release milestones.'
Assert-Equal @(
    '2026-07-29T23:59:59Z',
    '2026-08-04T23:59:59Z',
    '2026-08-11T23:59:59Z',
    '2026-08-18T23:59:59Z'
) @($desired.DueOn) 'Chromium schedule dates were not mapped to end-of-day GitHub deadlines.'
Assert-Equal @(
    'Skia m152 preview.1 · Start Mon, Jul 27, 2026 · Merge Skia sync PR and ship preview.',
    'Skia m152 preview.2 · Start Wed, Jul 29, 2026 · Bug fixes and API additions from preview.1 feedback.',
    'Skia m152 RC 1 · Start Tue, Aug 04, 2026 · Critical bug fixes only, no new features.',
    'Skia m152 stable · Start Tue, Aug 11, 2026 · Ship to NuGet.org, tag and create GitHub Release.'
) @($desired.Description) 'Milestone descriptions did not preserve the SkiaSharp release windows.'
Assert-True ($desired[0].Description.Contains([char] 0x00b7)) 'Milestone descriptions lost their separators.'

$matchingSchedule = @{
    '4.152.0-preview.1' = [pscustomobject] @{
        number = 1
        state = 'open'
        due_on = [datetime] '2026-07-29T00:00:00Z'
        description = $desired[0].Description
    }
}
$matchingOperation = Get-ScheduleOperations -Desired @($desired[0]) -Existing $matchingSchedule
Assert-Equal 'none' $matchingOperation[0].Action 'An already-current DateTime due date planned a redundant update.'

$existing = @{
    '4.152.0-preview.1' = [pscustomobject] @{ number = 1; state = 'open' }
    '4.152.0-preview.2' = [pscustomobject] @{ number = 2; state = 'open' }
}
$releaseMilestones = @(
    ConvertTo-ReleaseMilestone '4.152.0-preview.1'
    ConvertTo-ReleaseMilestone '4.152.0-preview.2'
    ConvertTo-ReleaseMilestone '4.152.0-rc.1'
    ConvertTo-ReleaseMilestone '4.152.0'
)
$closure = Get-MilestoneClosureOperations -Existing $existing -Milestones $releaseMilestones `
    -Tags @('v4.152.0-preview.1.2') -CreatableTitles @() -OpenItemsFor {
        @([pscustomobject] @{ Number = 99; Kind = 'issue' })
    }
Assert-Equal '4.152.0-preview.2' $closure.Operations[0].MoveTo 'Open work did not move to the next unshipped milestone.'
Assert-Equal 0 $closure.Warnings.Count 'A valid rollover unexpectedly produced a warning.'

$blocked = Get-MilestoneClosureOperations -Existing @{ '4.152.0' = [pscustomobject] @{ number = 4; state = 'open' } } `
    -Milestones @(ConvertTo-ReleaseMilestone '4.152.0') -Tags @('v4.152.0') -CreatableTitles @() `
    -OpenItemsFor { @([pscustomobject] @{ Number = 100; Kind = 'issue' }) }
Assert-Equal 'blocked' $blocked.Operations[0].Status 'A rollover without a destination was not blocked.'
Assert-Equal 1 $blocked.Warnings.Count 'A blocked rollover did not report exactly one warning.'

$emptyFinal = Get-MilestoneClosureOperations -Existing @{ '4.152.0' = [pscustomobject] @{ number = 4; state = 'open' } } `
    -Milestones @(ConvertTo-ReleaseMilestone '4.152.0') -Tags @('v4.152.0') -CreatableTitles @() -OpenItemsFor { @() }
Assert-Equal 'pending' $emptyFinal.Operations[0].Status 'An empty final milestone was not closable.'
Assert-Equal $null $emptyFinal.Operations[0].MoveTo 'An empty final milestone unexpectedly required a destination.'

$gitRoot = Join-Path $PSScriptRoot ".git-test-$([guid]::NewGuid().ToString('N'))"
try {
    $null = New-Item -ItemType Directory -Path $gitRoot
    & git -C $gitRoot init --quiet
    & git -C $gitRoot config user.name 'Release Milestone Tests'
    & git -C $gitRoot config user.email 'release-milestones@example.invalid'
    & git -C $gitRoot commit --quiet --allow-empty -m 'Boundary'
    $start = (& git -C $gitRoot rev-parse HEAD).Trim()
    & git -C $gitRoot commit --quiet --allow-empty -m 'Merge feature (#42)'
    & git -C $gitRoot commit --quiet --allow-empty -m 'Commit without pull request'
    & git -C $gitRoot commit --quiet --allow-empty -m 'Revert "Feature (#4087)" (#4091)'
    $end = (& git -C $gitRoot rev-parse HEAD).Trim()
    Assert-Equal @(42, 4091) @(Get-ReleasePullRequests -Root $gitRoot -Start $start -End $end) `
        'First-parent Git history did not yield trailing merged pull request numbers.'
} finally {
    if (Test-Path -LiteralPath $gitRoot) {
        Remove-Item -LiteralPath $gitRoot -Recurse -Force
    }
}

$script:FakeGhCalls = [System.Collections.Generic.List[string]]::new()
$script:FakeGhScenario = 'read'
$script:FakeMilestoneState = 'open'
$script:FakeItemMilestone = '4.152.0-preview.1'
$script:FakeLiveReleaseTags = @()
$script:FakeLiveReleaseTagReads = 0
function Get-CurrentRemoteReleaseTags([string] $Root) {
    $script:FakeLiveReleaseTagReads++
    if ($script:FakeGhScenario -eq 'post-write-shipment-race' -and $script:FakeLiveReleaseTagReads -gt 1) {
        return @(
            'v4.152.0-preview.1.26426.14',
            'v4.153.0-preview.1.26454.6'
        )
    }
    return $script:FakeLiveReleaseTags
}
function global:gh {
    $command = $args -join ' '
    $script:FakeGhCalls.Add($command)
    if ($script:FakeGhScenario -eq 'apply') {
        if ($command -match 'issues/99 .*PATCH') {
            $script:FakeItemMilestone = '4.152.0-preview.2'
            return '{"number":99}'
        } elseif ($command -match 'issues/99$') {
            return '{"number":99,"milestone":{"title":"4.152.0-preview.2"}}'
        } elseif ($command -match 'issues\?milestone=1') {
            return '[[]]'
        } elseif ($command -match 'milestones/1 .*PATCH') {
            $script:FakeMilestoneState = 'closed'
            return '{"number":1,"state":"closed"}'
        } elseif ($command -match 'milestones\?state=all') {
            return @"
[
  [
    {"number":1,"title":"4.152.0-preview.1","state":"$script:FakeMilestoneState"},
    {"number":2,"title":"4.152.0-preview.2","state":"open"}
  ]
]
"@
        }
    }
    if ($script:FakeGhScenario -eq 'new-item' -and $command -match 'issues\?milestone=1') {
        return '[[{"number":101,"title":"New issue","html_url":"url"}]]'
    }
    if ($script:FakeGhScenario -eq 'duplicate' -and $command -match 'milestones\?state=all') {
        return '[[{"number":1,"title":"duplicate"},{"number":2,"title":"duplicate"}]]'
    }
    if ($script:FakeGhScenario -eq 'assignment-race') {
        if ($command -match 'issues/4826 .*PATCH') {
            throw 'Unsafe raced assignment reached PATCH.'
        } elseif ($command -match 'issues/4826$') {
            return '{"number":4826,"milestone":{"number":74,"title":"4.153.0-preview.1","state":"closed"}}'
        }
    }
    if ($script:FakeGhScenario -eq 'closure-race') {
        if ($command -match 'issues/6000 .*PATCH') {
            throw 'Unsafe closure-raced assignment reached PATCH.'
        } elseif ($command -match 'issues/6000$') {
            return '{"number":6000,"milestone":{"number":72,"title":"4.152.0-preview.1","state":"closed"}}'
        }
    }
    if ($script:FakeGhScenario -eq 'shipment-race') {
        if ($command -match 'issues/6001 .*PATCH') {
            throw 'Unsafe shipment-raced assignment reached PATCH.'
        } elseif ($command -match 'issues/6001$') {
            return '{"number":6001,"milestone":{"number":72,"title":"4.152.0-preview.1","state":"open"}}'
        }
    }
    if ($script:FakeGhScenario -eq 'post-write-shipment-race') {
        if ($command -match 'issues/6002 .*PATCH.*milestone=74') {
            $script:FakeItemMilestone = '4.153.0-preview.1'
            return '{"number":6002}'
        } elseif ($command -match 'issues/6002 .*PATCH.*milestone=72') {
            $script:FakeItemMilestone = '4.152.0-preview.1'
            return '{"number":6002}'
        } elseif ($command -match 'issues/6002$') {
            $milestoneNumber = if ($script:FakeItemMilestone -eq '4.152.0-preview.1') { 72 } else { 74 }
            return [pscustomobject] @{
                number = 6002
                milestone = [pscustomobject] @{
                    number = $milestoneNumber
                    title = $script:FakeItemMilestone
                    state = 'open'
                }
            } | ConvertTo-Json -Compress
        } elseif ($command -match 'milestones\?state=all') {
            return @'
[[{"number":72,"title":"4.152.0-preview.1","state":"open"},{"number":74,"title":"4.153.0-preview.1","state":"closed"}]]
'@
        }
    }
    if ($command -match 'milestones\?state=all') {
        @'
[[{"number":70,"title":"4.152.0-preview.1","state":"open"}]]
'@
    } elseif ($command -match 'issues\?milestone=70') {
        @'
[
  [
    {"number":10,"title":"Issue","html_url":"https://example/issues/10"},
    {
      "number":20,
      "title":"PR",
      "html_url":"https://example/pull/20",
      "pull_request":{"url":"https://api.example/pulls/20"}
    }
  ]
]
'@
    } elseif ($command -match 'graphql') {
        '{"data":{"repository":{"pullRequest":{"closingIssuesReferences":{"nodes":[{"number":12}]}}}}}'
    } elseif ($command -match 'pulls/77') {
        '{"body":"Fixes #34 and resolved: #56"}'
    } else {
        throw "Unexpected fake gh command: $command"
    }
}

$map = Get-GitHubMilestoneMap -Repository 'mono/SkiaSharp'
Assert-Equal 70 $map['4.152.0-preview.1'].number 'The fake-gh milestone response was not parsed.'
$openItems = Get-OpenMilestoneItems -Repository 'mono/SkiaSharp' -MilestoneNumber 70
Assert-Equal @('issue', 'pull-request') @($openItems.Kind) 'Issues and pull requests were not distinguished.'
Assert-Equal @(12, 34, 56) @(Get-LinkedIssues -Repository 'mono/SkiaSharp' -PullRequest 77) `
    'GitHub references and closing keywords were not combined.'

$callsBeforeDryRun = $script:FakeGhCalls.Count
$dryRunOutput = @(
    Invoke-GitHubMutation -Arguments @('api', 'repos/mono/SkiaSharp/milestones/70', '-X', 'PATCH', '-f', 'state=closed') `
        -Description 'Close milestone' 6>&1
) -join "`n"
Assert-Equal $callsBeforeDryRun $script:FakeGhCalls.Count 'A dry-run mutation invoked gh.'
Assert-True ($dryRunOutput -match 'Skipping: gh api .*state=closed.*requires -Push') `
    'A dry-run did not log the exact skipped mutation.'

$script:FakeGhScenario = 'apply'
$pushOperation = [pscustomobject] @{
    Title = '4.152.0-preview.1'
    Number = 1
    OpenItems = @([pscustomobject] @{ Number = 99; Kind = 'issue' })
    MoveTo = '4.152.0-preview.2'
}
$pushMilestones = @{
    '4.152.0-preview.1' = [pscustomobject] @{ number = 1; state = 'open' }
    '4.152.0-preview.2' = [pscustomobject] @{ number = 2; state = 'open' }
}
$Push = $true
$writeRemote = $true
try {
    Complete-GitHubMilestone -Repository 'mono/SkiaSharp' -Operation $pushOperation -Milestones $pushMilestones
} finally {
    $Push = $false
    $writeRemote = $false
}
Assert-Equal '4.152.0-preview.2' $script:FakeItemMilestone 'The fake-gh apply path did not move open work.'
Assert-Equal 'closed' $script:FakeMilestoneState 'The fake-gh apply path did not close the emptied milestone.'

$script:FakeGhScenario = 'assignment-race'
$script:FakeLiveReleaseTags = @(
    'v4.153.0-preview.1.26454.6',
    'v4.154.0-preview.1.26454.9'
)
$racedOperation = [pscustomobject] @{
    Kind = 'pull-request'
    Number = 4826
    ViaPullRequest = $null
    FromMilestone = ''
    ToMilestone = '4.154.0-preview.1'
    ToMilestoneNumber = 75
}
Assert-Throws {
    Set-PlannedReleaseAssignment `
        -Root 'fake-root' `
        -Repository 'mono/SkiaSharp' `
        -Item $racedOperation `
        -Milestones @{
            '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
            '4.154.0-preview.1' = [pscustomobject] @{ number = 75; state = 'closed' }
        } `
        -Push
} 'Refusing to move pull-request #4826' `
    'A concurrent earlier shipped assignment was not revalidated before PATCH.'

$script:FakeGhScenario = 'closure-race'
$script:FakeLiveReleaseTags = @('v4.153.0-preview.1.26454.6')
$closureRacedOperation = [pscustomobject] @{
    Kind = 'pull-request'
    Number = 6000
    ViaPullRequest = $null
    FromMilestone = '4.152.0-preview.1'
    ToMilestone = '4.153.0-preview.1'
    ToMilestoneNumber = 74
}
Assert-Throws {
    Set-PlannedReleaseAssignment `
        -Root 'fake-root' `
        -Repository 'mono/SkiaSharp' `
        -Item $closureRacedOperation `
        -Milestones @{
            '4.152.0-preview.1' = [pscustomobject] @{ number = 72; state = 'open' }
            '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
        } `
        -Push
} 'Refusing to move pull-request #6000' `
    'A source milestone closed after planning was not revalidated before PATCH.'

$script:FakeGhScenario = 'shipment-race'
$script:FakeLiveReleaseTags = @(
    'v4.152.0-preview.1.26426.14',
    'v4.153.0-preview.1.26454.6'
)
$shipmentRacedOperation = [pscustomobject] @{
    Kind = 'pull-request'
    Number = 6001
    ViaPullRequest = $null
    FromMilestone = '4.152.0-preview.1'
    ToMilestone = '4.153.0-preview.1'
    ToMilestoneNumber = 74
}
Assert-Throws {
    Set-PlannedReleaseAssignment `
        -Root 'fake-root' `
        -Repository 'mono/SkiaSharp' `
        -Item $shipmentRacedOperation `
        -Milestones @{
            '4.152.0-preview.1' = [pscustomobject] @{ number = 72; state = 'open' }
            '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
        } `
        -Push
} 'Refusing to move pull-request #6001.*shipped as v4.152.0-preview.1.26426.14' `
    'A source milestone shipped after planning was not revalidated before PATCH.'

$script:FakeGhScenario = 'post-write-shipment-race'
$script:FakeItemMilestone = '4.152.0-preview.1'
$script:FakeLiveReleaseTagReads = 0
$script:FakeLiveReleaseTags = @('v4.153.0-preview.1.26454.6')
$postWriteRacedOperation = [pscustomobject] @{
    Kind = 'pull-request'
    Number = 6002
    ViaPullRequest = $null
    FromMilestone = '4.152.0-preview.1'
    FromMilestoneNumber = 72
    ToMilestone = '4.153.0-preview.1'
    ToMilestoneNumber = 74
}
Assert-Throws {
    Set-PlannedReleaseAssignment `
        -Root 'fake-root' `
        -Repository 'mono/SkiaSharp' `
        -Item $postWriteRacedOperation `
        -Milestones @{
            '4.152.0-preview.1' = [pscustomobject] @{ number = 72; state = 'open' }
            '4.153.0-preview.1' = [pscustomobject] @{ number = 74; state = 'closed' }
        } `
        -Push
} 'Refusing to move pull-request #6002.*concurrent change was detected after mutation.*restored' `
    'A source milestone shipped between revalidation and PATCH without restoring the assignment.'
Assert-Equal '4.152.0-preview.1' $script:FakeItemMilestone `
    'A raced assignment was not restored to its newly shipped source milestone.'

$script:FakeGhScenario = 'new-item'
Assert-Throws {
    Wait-MilestoneMoves -Repository 'mono/SkiaSharp' -MilestoneNumber 1 -MovedNumbers @(99)
} 'gained open items.*issue #101' 'A newly appeared item did not block milestone closure.'

$script:FakeGhScenario = 'duplicate'
Assert-Throws {
    $null = Get-GitHubMilestoneMap -Repository 'mono/SkiaSharp'
} 'Multiple milestones' 'Ambiguous duplicate milestone titles were not rejected.'

Remove-Item Function:\gh
Write-Output "All $script:TestsRun publishing milestone tests passed."
