#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Checks SkiaSharp release state by coordinating its owning release tools.

.DESCRIPTION
    This command is read-only. It inventories real SkiaSharp release branches,
    exact tags, and public package versions, then delegates each state check to
    the tool that owns that release phase.

.PARAMETER Version
    One PowerShell wildcard pattern matched against real release identities and
    exact public package versions.

.PARAMETER Discover
    Add every in-scope real SkiaSharp release branch and public shipment.

.PARAMETER Quiet
    Suppress the human-readable report when every owner check is complete.

.PARAMETER Json
    Emit the compact machine-readable result instead of the status table.

.PARAMETER IncludeMilestoneAssignments
    Include the potentially long-running PR and issue milestone reconciliation.
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string] $Version,

    [switch] $Discover,

    [ValidateRange(1, 3650)]
    [int] $MaxAge = 3650,

    [ValidateRange(1, 2147483647)]
    [int] $BarId,

    [switch] $Quiet,

    [switch] $Json,

    [switch] $IncludeMilestoneAssignments
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Maestro.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$script:ShowAuditProgress = !$Quiet -and !$Json

function Write-AuditProgress(
    [string] $State,
    [string] $Target,
    [string] $Phase,
    [double] $ElapsedSeconds = -1,
    [string] $Detail = ''
) {
    if ($script:ShowAuditProgress) {
        $elapsed = if ($ElapsedSeconds -ge 0) {
            [string]::Format(
                [Globalization.CultureInfo]::InvariantCulture,
                ' ({0:0.0}s)',
                $ElapsedSeconds)
        } else {
            ''
        }
        $detailText = if ($Detail) { " - $Detail" } else { '' }
        Write-Host "[$State] ${Target}: $Phase$elapsed$detailText"
    }
}

function New-AuditTarget([pscustomobject] $Release, [string] $PublicVersion) {
    $isPublic = ![string]::IsNullOrWhiteSpace($PublicVersion)
    return [pscustomobject] @{
        Key = if ($isPublic) { "public:$($PublicVersion.ToLowerInvariant())" } else { "branch:$($Release.Title)" }
        Value = if ($isPublic) { $PublicVersion } else { $Release.Title }
        Release = $Release
        Title = $Release.Title
        Core = $Release.Numeric
        Kind = if ($isPublic) { 'public package' } else { 'release branch' }
        PublicVersion = if ($isPublic) { $PublicVersion } else { $null }
    }
}

function New-AuditResult(
    [string] $Target,
    [string] $Phase,
    [string] $State,
    [int] $Code,
    [string] $Output = '',
    [double] $ElapsedSeconds = 0,
    [object] $Details = $null
) {
    return [pscustomobject] @{
        Target = $Target
        Phase = $Phase
        State = $State
        Code = $Code
        Output = $Output
        ElapsedSeconds = [Math]::Round($ElapsedSeconds, 1)
        Elapsed = [string]::Format(
            [Globalization.CultureInfo]::InvariantCulture,
            '{0:0.0}s',
            $ElapsedSeconds)
        Details = $Details
    }
}

function Test-AuditHistoryScope([pscustomobject] $Release, [pscustomobject] $HistoryFloor) {
    return !$HistoryFloor -or $Release.NumericKey -ge $HistoryFloor.NumericKey
}

function Add-AuditTarget(
    [hashtable] $Targets,
    [pscustomobject] $Release,
    [string] $PublicVersion,
    [pscustomobject] $HistoryFloor
) {
    if (!$Release -or !(Test-AuditHistoryScope -Release $Release -HistoryFloor $HistoryFloor)) {
        return
    }
    $target = New-AuditTarget -Release $Release -PublicVersion $PublicVersion
    $Targets[$target.Key] = $target
}

function Get-AuditTargetMatches([object[]] $Known, [string] $Pattern) {
    return @($Known | Where-Object { $_.Value -like $Pattern })
}

function Invoke-OwnerCheck(
    [string] $Target,
    [string] $Phase,
    [string] $Executable,
    [string[]] $Arguments,
    [string] $WorkingDirectory
) {
    Write-AuditProgress -State 'checking' -Target $Target -Phase $Phase
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    $pushedLocation = $false
    try {
        Push-Location $WorkingDirectory
        $pushedLocation = $true
        $output = @(& $Executable @Arguments 2>&1)
        $code = $LASTEXITCODE
        if ($null -eq $code) {
            $code = 0
        }
    } catch {
        $output = @($_.Exception.Message)
        $code = 2
    } finally {
        $stopwatch.Stop()
        if ($pushedLocation) {
            Pop-Location
        }
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }

    $state = switch ($code) {
        0 { 'complete' }
        2 { 'unavailable' }
        default { 'pending' }
    }
    Write-AuditProgress `
        -State $state `
        -Target $Target `
        -Phase $Phase `
        -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds
    return New-AuditResult `
        -Target $Target `
        -Phase $Phase `
        -State $state `
        -Code $code `
        -Output (($output -join "`n").Trim()) `
        -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds
}

function Invoke-MaestroCheck(
    [string] $Target,
    [string] $Version,
    [int] $BarId,
    [int] $MaxAge,
    [string] $Branch = ''
) {
    Write-AuditProgress -State 'checking' -Target $Target -Phase 'Maestro BAR and channel'
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    try {
        $receipt = if ($Branch) {
            Get-MaestroReleaseReceiptForBranch `
                -Root $root `
                -Branch $Branch `
                -Version $Version `
                -BarId $BarId `
                -MaxAge $MaxAge
        } else {
            Get-MaestroReleaseReceipt `
                -Version $Version `
                -BarId $BarId `
                -MaxAge $MaxAge
        }
        $state = $receipt.State
        $code = switch ($state) {
            'complete' { 0 }
            'unavailable' { 2 }
            default { 1 }
        }
        $output = $receipt.Message
        $details = $receipt.Value
        $detail = if ($state -eq 'complete') {
            "BAR $($details.BarBuildId); $($details.SourceBranch)@$($details.SourceCommit.Substring(0, 12)); $($details.SkiaSharpVersion)"
        } else {
            ''
        }
    } catch {
        $state = 'unavailable'
        $code = 2
        $output = $_.Exception.Message
        $details = $null
        $detail = ''
    }
    $stopwatch.Stop()
    Write-AuditProgress `
        -State $state `
        -Target $Target `
        -Phase 'Maestro BAR and channel' `
        -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds `
        -Detail $detail
    return New-AuditResult `
        -Target $Target `
        -Phase 'Maestro BAR and channel' `
        -State $state `
        -Code $code `
        -Output $output `
        -ElapsedSeconds $stopwatch.Elapsed.TotalSeconds `
        -Details $details
}

$root = Get-GitRepositoryRoot -Path $PSScriptRoot
$versionsPath = Join-Path $root 'scripts/infra/docs/versions.json'
try {
    $versions = Get-Content -LiteralPath $versionsPath -Raw | ConvertFrom-Json
    $historyFloorText = [string] $versions.history_floor.skiasharp
    $historyFloor = if ($historyFloorText) { ConvertTo-ReleaseMilestone $historyFloorText } else { $null }
    if ($historyFloorText -and !$historyFloor) {
        throw 'history_floor.skiasharp must be a numeric SkiaSharp version.'
    }
} catch {
    throw "Cannot read release audit history scope: $($_.Exception.Message)"
}

$hasVersionPattern = ![string]::IsNullOrWhiteSpace($Version)
if (!$hasVersionPattern -and !$Discover) {
    throw 'Provide -Version or -Discover.'
}
if ($hasVersionPattern -and $Discover) {
    throw 'Use either -Version or -Discover, not both.'
}

$results = [System.Collections.Generic.List[object]]::new()
$branches = @()
$tags = @()
$nugetVersions = @()
try {
    Write-AuditProgress -State 'checking' -Target 'inventory' -Phase 'SkiaSharp branches'
    $branches = @(Get-RemoteBranches -Root $root -Remote origin -Pattern 'refs/heads/release/*')
    Write-AuditProgress -State 'complete' -Target 'inventory' -Phase 'SkiaSharp branches'
} catch {
    $results.Add((New-AuditResult -Target 'inventory' -Phase 'SkiaSharp branches' -State 'unavailable' -Code 2 -Output $_.Exception.Message))
    Write-AuditProgress -State 'unavailable' -Target 'inventory' -Phase 'SkiaSharp branches'
}
try {
    Write-AuditProgress -State 'checking' -Target 'inventory' -Phase 'SkiaSharp tags'
    $tags = @(Get-RemoteReleaseTags -Root $root)
    Write-AuditProgress -State 'complete' -Target 'inventory' -Phase 'SkiaSharp tags'
} catch {
    $results.Add((New-AuditResult -Target 'inventory' -Phase 'SkiaSharp tags' -State 'unavailable' -Code 2 -Output $_.Exception.Message))
    Write-AuditProgress -State 'unavailable' -Target 'inventory' -Phase 'SkiaSharp tags'
}
try {
    Write-AuditProgress -State 'checking' -Target 'inventory' -Phase 'NuGet package catalogue'
    $nugetVersions = @(Get-NuGetPackageVersions -PackageId 'SkiaSharp')
    Write-AuditProgress -State 'complete' -Target 'inventory' -Phase 'NuGet package catalogue'
} catch {
    $results.Add((New-AuditResult -Target 'inventory' -Phase 'NuGet package catalogue' -State 'unavailable' -Code 2 -Output $_.Exception.Message))
    Write-AuditProgress -State 'unavailable' -Target 'inventory' -Phase 'NuGet package catalogue'
}

$knownByKey = @{}
foreach ($branch in $branches) {
    Add-AuditTarget `
        -Targets $knownByKey `
        -Release (ConvertTo-ReleaseMilestone $branch) `
        -PublicVersion $null `
        -HistoryFloor $historyFloor
}
foreach ($publicVersion in @($tags | ForEach-Object { $_.Substring(1) }) + $nugetVersions) {
    try {
        $identity = Get-ReleaseIdentity $publicVersion
        Add-AuditTarget `
            -Targets $knownByKey `
            -Release (ConvertTo-ReleaseMilestone ($identity.Branch -replace '^release/', '')) `
            -PublicVersion $publicVersion `
            -HistoryFloor $historyFloor
    } catch {
        # Decorative tags and non-SkiaSharp catalogue entries are not shipments.
    }
}
$known = @($knownByKey.Values)

$selectedByKey = @{}
if ($Discover) {
    foreach ($target in $known) {
        $selectedByKey[$target.Key] = $target
    }
}
if ($hasVersionPattern) {
    $matches = @(Get-AuditTargetMatches -Known $known -Pattern $Version)
    if (!$matches.Count) {
        $results.Add((New-AuditResult `
            -Target $Version `
            -Phase 'selection' `
            -State 'pending' `
            -Code 1 `
            -Output "No in-scope discovered SkiaSharp release matches '$Version'."))
    } else {
        foreach ($target in $matches) {
            $selectedByKey[$target.Key] = $target
        }
    }
}

$targets = @($selectedByKey.Values | Sort-Object { $_.Release.SortKey }, PublicVersion, Value)
if (!$targets.Count) {
    $exitCode = if (@($results | Where-Object Code -eq 2).Count) { 2 } else { 1 }
    if ($Json) {
        [pscustomobject] @{ exitCode = $exitCode; results = @($results) } | ConvertTo-Json -Depth 5
    } else {
        $results | Format-Table Target, Phase, State -AutoSize | Out-Host
        Write-Output 'No in-scope SkiaSharp release identities were found.'
    }
    exit $exitCode
}

$publicTargets = @($targets | Where-Object PublicVersion)
if ($BarId -and $publicTargets.Count -ne 1) {
    throw '-BarId requires one exact public version.'
}
if ($script:ShowAuditProgress) {
    $selection = if ($Discover) { 'discovery' } else { "'$Version'" }
    Write-Host "[matched] $selection selected $($targets.Count) target(s):"
    foreach ($target in $targets) {
        Write-Host "  - $($target.Kind): $($target.Value)"
    }
    Write-Host '[matched] shared checks:'
    foreach ($core in @($targets.Core | Sort-Object -Unique)) {
        $checks = if ($IncludeMilestoneAssignments) {
            'Website release notes and Release - Milestone assignments'
        } else {
            'Website release notes'
        }
        Write-Host "  - ${core}: $checks"
    }
    if (!$IncludeMilestoneAssignments) {
        Write-Host '  - matched cores: Release - Milestone assignments skipped (pass -IncludeMilestoneAssignments)'
    }
    Write-Host '  - all: milestones'
}

$powerShell = $null
$python = $null
try {
    $powerShell = Get-Command pwsh -CommandType Application -ErrorAction Stop |
        Select-Object -First 1 -ExpandProperty Source
} catch {
    $results.Add((New-AuditResult -Target 'runtime' -Phase 'PowerShell' -State 'unavailable' -Code 2 -Output $_.Exception.Message))
}
try {
    $python = Get-Command python3 -CommandType Application -ErrorAction Stop |
        Select-Object -First 1 -ExpandProperty Source
} catch {
    $results.Add((New-AuditResult -Target 'runtime' -Phase 'Python' -State 'unavailable' -Code 2 -Output $_.Exception.Message))
}

$releasesByTitle = @{}
foreach ($target in $targets) {
    $releasesByTitle[$target.Title] = $target.Release
}
foreach ($release in @($releasesByTitle.Values | Sort-Object SortKey)) {
    if ($powerShell) {
        $prepareRelease = if ($release.Channel) { $release.Title } else { "$($release.Title)-stable" }
        $results.Add((Invoke-OwnerCheck `
            -Target $release.Title `
            -Phase 'Release - Prepare' `
            -Executable $powerShell `
            -Arguments @('-NoLogo', '-NoProfile', '-File', (Join-Path $PSScriptRoot 'prepare-release.ps1'), '-Release', $prepareRelease, '-Mode', 'Check') `
            -WorkingDirectory $root))
    }

    $releaseTargets = @($targets | Where-Object Title -eq $release.Title)
    $releasePublicTargets = @($releaseTargets | Where-Object PublicVersion)
    if (!$releasePublicTargets.Count) {
        $results.Add((Invoke-MaestroCheck `
            -Target $release.Title `
            -Version '' `
            -BarId 0 `
            -MaxAge $MaxAge `
            -Branch "release/$($release.Title)"))
        if ($powerShell) {
            $results.Add((Invoke-OwnerCheck `
                -Target $release.Title `
                -Phase 'Release - Finish (public package)' `
                -Executable $powerShell `
                -Arguments @('-NoLogo', '-NoProfile', '-File', (Join-Path $PSScriptRoot 'finish-release.ps1'), '-Version', $release.Title, '-Mode', 'Check') `
                -WorkingDirectory $root))
        }
    }
    foreach ($target in $releasePublicTargets) {
        $results.Add((Invoke-MaestroCheck `
            -Target $target.Value `
            -Version $target.PublicVersion `
            -BarId $BarId `
            -MaxAge $MaxAge `
            -Branch "release/$($release.Title)"))
        if ($powerShell) {
            $results.Add((Invoke-OwnerCheck `
                -Target $target.Value `
                -Phase 'Release - Finish (public package)' `
                -Executable $powerShell `
                -Arguments @('-NoLogo', '-NoProfile', '-File', (Join-Path $PSScriptRoot 'finish-release.ps1'), '-Version', $target.PublicVersion, '-Mode', 'Check') `
                -WorkingDirectory $root))
        }
        if ($python) {
            $results.Add((Invoke-OwnerCheck `
                -Target $target.Value `
                -Phase 'GitHub Release summary' `
                -Executable $python `
                -Arguments @('scripts/infra/docs/release_notes/update_github_summaries.py', '--check', '--tag', "v$($target.PublicVersion)", '--root', $root) `
                -WorkingDirectory $root))
        }
    }
}

foreach ($core in @($targets.Core | Sort-Object -Unique)) {
    if ($python) {
        $results.Add((Invoke-OwnerCheck `
            -Target $core `
            -Phase 'Website release notes' `
            -Executable $python `
            -Arguments @('scripts/infra/docs/release-notes-render.py', '--check', $core) `
            -WorkingDirectory $root))
    }
}
$cores = @($targets.Core | Sort-Object -Unique)
if ($IncludeMilestoneAssignments -and $powerShell) {
    $assignmentArguments = @(
        '-NoLogo',
        '-NoProfile',
        '-File',
        (Join-Path $PSScriptRoot 'reconcile-release-assignments.ps1'),
        '-Version',
        ($cores -join ','),
        '-Mode',
        'Check'
    )
    $results.Add((Invoke-OwnerCheck `
        -Target "$($cores.Count) matched core(s)" `
        -Phase 'Release - Milestone assignments' `
        -Executable $powerShell `
        -Arguments $assignmentArguments `
        -WorkingDirectory $root))
} elseif (!$IncludeMilestoneAssignments) {
    $assignmentTarget = "$($cores.Count) matched core(s)"
    $results.Add((New-AuditResult `
        -Target $assignmentTarget `
        -Phase 'Release - Milestone assignments' `
        -State 'skipped' `
        -Code 0 `
        -Output 'Skipped by default; rerun with -IncludeMilestoneAssignments.'))
    Write-AuditProgress `
        -State 'skipped' `
        -Target $assignmentTarget `
        -Phase 'Release - Milestone assignments'
}
if ($powerShell) {
    $results.Add((Invoke-OwnerCheck `
        -Target 'all' `
        -Phase 'Release - Milestone maintenance' `
        -Executable $powerShell `
        -Arguments @('-NoLogo', '-NoProfile', '-File', (Join-Path $PSScriptRoot 'update-release-milestones.ps1'), '-Mode', 'Check') `
        -WorkingDirectory $root))
}

$exitCode = if (@($results | Where-Object Code -eq 2).Count) {
    2
} elseif (@($results | Where-Object Code -ne 0).Count) {
    1
} else {
    0
}
$hasSkippedChecks = @($results | Where-Object State -eq 'skipped').Count -gt 0
if ($Json) {
    [pscustomobject] @{ exitCode = $exitCode; results = @($results) } | ConvertTo-Json -Depth 5
} elseif (!$Quiet -or $exitCode -ne 0 -or $hasSkippedChecks) {
    $results | Format-Table Target, Phase, State, Elapsed -AutoSize | Out-Host
    foreach ($result in $results | Where-Object { $_.Code -ne 0 }) {
        if ($result.Output) {
            Write-Output "[$($result.Target):$($result.Phase)]"
            Write-Output $result.Output
        }
    }
}
exit $exitCode
