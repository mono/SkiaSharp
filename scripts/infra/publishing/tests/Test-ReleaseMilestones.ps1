#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$publishingRoot = Split-Path $PSScriptRoot
Import-Module (Join-Path $publishingRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $publishingRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $publishingRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $publishingRoot 'ReleaseMilestones.Common.psm1') -Force
$milestoneModule = Get-Module ReleaseMilestones.Common

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

$schedule = [pscustomobject] @{
    branch_point = '2026-07-27T00:00:00Z'
    earliest_beta = '2026-08-04T00:00:00Z'
    early_stable_cut = '2026-08-11T00:00:00Z'
    early_stable = '2026-08-12T00:00:00Z'
    stable_cut = '2026-08-18T00:00:00Z'
    stable_date = '2026-08-25T00:00:00Z'
}
$desired = New-DesiredReleaseMilestones -Schedule $schedule -Milestone 152 -Major 4
Assert-Equal @(
    '4.152.0-preview.1',
    '4.152.0-preview.2',
    '4.152.0-rc.1',
    '4.152.0'
) @($desired.Title) 'Chromium stages were not mapped to release milestones.'
Assert-Equal @(
    '2026-08-04T00:00:00Z',
    '2026-08-12T00:00:00Z',
    '2026-08-18T00:00:00Z',
    '2026-08-25T00:00:00Z'
) @($desired.DueOn) 'Chromium schedule dates were not mapped to the expected stages.'
Assert-True ($desired[0].Description.Contains([char] 0x00b7)) 'Milestone descriptions lost their separators.'

$matchingSchedule = @{
    '4.152.0-preview.1' = [pscustomobject] @{
        number = 1
        state = 'open'
        due_on = [datetime] '2026-08-04T00:00:00Z'
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
    $end = (& git -C $gitRoot rev-parse HEAD).Trim()
    Assert-Equal @(42) @(Get-ReleasePullRequests -Root $gitRoot -Start $start -End $end) `
        'First-parent Git history did not yield its merged pull request.'
} finally {
    if (Test-Path -LiteralPath $gitRoot) {
        Remove-Item -LiteralPath $gitRoot -Recurse -Force
    }
}

$script:FakeGhCalls = [System.Collections.Generic.List[string]]::new()
$script:FakeGhScenario = 'read'
$script:FakeMilestoneState = 'open'
$script:FakeItemMilestone = '4.152.0-preview.1'
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
& $milestoneModule { $script:WriteRemote = $true }
try {
    Complete-GitHubMilestone -Repository 'mono/SkiaSharp' -Operation $pushOperation -Milestones $pushMilestones
} finally {
    & $milestoneModule { $script:WriteRemote = $false }
}
Assert-Equal '4.152.0-preview.2' $script:FakeItemMilestone 'The fake-gh apply path did not move open work.'
Assert-Equal 'closed' $script:FakeMilestoneState 'The fake-gh apply path did not close the emptied milestone.'

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
