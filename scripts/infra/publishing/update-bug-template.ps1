#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates the SkiaSharp version dropdowns in the bug-report issue form.

.DESCRIPTION
    Reads published GitHub Releases, regenerates the current and last-known-good
    version options, and preserves all unrelated issue-form text. Push mode owns
    the automation branch, commit, and pull request used by CI.

.PARAMETER Repository
    The GitHub repository whose published releases are read. Defaults to the
    runtime or configured repository identity.

.PARAMETER File
    The issue-form path, relative to the repository root unless absolute.

.PARAMETER Mode
    DryRun is read-only, Apply writes the updated issue form locally, and Push
    updates the owned automation branch and pull request.
#>

[CmdletBinding()]
param(
    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository,

    [string] $File = '.github/ISSUE_TEMPLATE/bug-report.yml',

    [ValidateSet('DryRun', 'Apply', 'Push')]
    [string] $Mode = 'DryRun'
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$Repository = Resolve-PublishingRepository $Repository
$modeDescription = $Mode.ToLowerInvariant()
$automationBranch = 'automation/update-issue-template-versions'
$otherOption = 'Other (Please indicate in the description)'
$nightlyOption = 'Nightly / CI build'

# Parses one release tag into display and deterministic sorting metadata.
function ConvertTo-IssueTemplateVersion([string] $Tag) {
    $match = [regex]::Match(
        $Tag.Trim(),
        '^v?(?<numeric>\d+\.\d+\.\d+(?:\.\d+)?)(?:-(?<label>alpha|beta|preview|rc)\.' +
            '(?<iteration>\d+)(?:\.(?<build1>\d+)(?:\.(?<build2>\d+))?)?)?$')
    if (!$match.Success) {
        return $null
    }

    $parts = @($match.Groups['numeric'].Value.Split('.') | ForEach-Object { [int] $_ })
    $revision = if ($parts.Count -eq 4) { $parts[3] } else { 0 }
    $label = if ($match.Groups['label'].Success) { $match.Groups['label'].Value } else { $null }
    $iteration = if ($match.Groups['iteration'].Success) { [int] $match.Groups['iteration'].Value } else { 0 }
    $build1 = if ($match.Groups['build1'].Success) { [long] $match.Groups['build1'].Value } else { 0 }
    $build2 = if ($match.Groups['build2'].Success) { [long] $match.Groups['build2'].Value } else { 0 }
    $labelRank = switch ($label) {
        'alpha' { 0 }
        'beta' { 1 }
        'preview' { 2 }
        'rc' { 3 }
        default { 99 }
    }
    $display = $match.Groups['numeric'].Value
    if ($label) {
        $display += "-$label.$iteration"
    }
    return [pscustomobject] @{
        Tag = $Tag
        Display = $display
        Major = $parts[0]
        IsPrerelease = [bool] $label
        NumericKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}' -f $parts[0], $parts[1], $parts[2], $revision
        SortKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}.{4:D2}.{5:D2}.{6:D10}.{7:D20}.{8:D20}' -f
            $parts[0], $parts[1], $parts[2], $revision, [int] !$label, $labelRank, $iteration, $build1, $build2
    }
}

# Reads published releases and keeps the greatest build for each display version.
function Get-PublishedReleaseVersions([string] $Repository) {
    $releases = Invoke-GitHubJsonWithRetry -Arguments @(
        'release', 'list',
        '--repo', $Repository,
        '--limit', '300',
        '--json', 'tagName,isDraft'
    )
    $byDisplay = @{}
    foreach ($release in @($releases)) {
        if ($release.isDraft) {
            continue
        }
        $version = ConvertTo-IssueTemplateVersion -Tag ([string] $release.tagName)
        if (!$version) {
            continue
        }
        $existing = $byDisplay[$version.Display]
        if (!$existing -or $version.SortKey -gt $existing.SortKey) {
            $byDisplay[$version.Display] = $version
        }
    }
    return @($byDisplay.Values | Sort-Object SortKey -Descending)
}

# Builds both issue-form option lists and their current-version defaults.
function New-IssueTemplateOptions([object[]] $Versions, [int] $Major) {
    $supported = @($Versions | Where-Object Major -eq $Major | Sort-Object SortKey -Descending)
    $stables = @($supported | Where-Object { !$_.IsPrerelease })
    $prereleases = @($supported | Where-Object IsPrerelease)
    $current = $stables | Select-Object -First 1
    $currentKey = if ($current) { $current.NumericKey } else { '' }
    $upcoming = @($prereleases | Where-Object { !$current -or $_.NumericKey -gt $currentKey })

    $stableOptions = [System.Collections.Generic.List[string]]::new()
    if ($current) {
        $stableOptions.Add("$($current.Display) (Current)")
    }
    if ($stables.Count -gt 1) {
        $stableOptions.Add("$($stables[1].Display) (Previous)")
    }
    foreach ($deprecated in @($stables | Select-Object -Skip 2)) {
        $stableOptions.Add("$($deprecated.Display) (Deprecated)")
    }
    $obsolete = @(
        $Versions |
            Where-Object { $_.Major -lt $Major } |
            Select-Object -ExpandProperty Major -Unique |
            Sort-Object -Descending |
            ForEach-Object { "$_.x (Obsolete)" }
    )

    $versionPrerelease = if ($upcoming) { @("$($upcoming[0].Display) (Pre-release)") } else { @() }
    $goodPrerelease = @($upcoming | ForEach-Object { "$($_.Display) (Pre-release)" })
    return [pscustomobject] @{
        Version = @($nightlyOption) + $versionPrerelease + $stableOptions.ToArray() + $obsolete + @($otherOption)
        GoodVersion = $goodPrerelease + $stableOptions.ToArray() + $obsolete + @($otherOption)
        VersionDefault = 1 + $versionPrerelease.Count
        GoodVersionDefault = $goodPrerelease.Count
    }
}

# Replaces one dropdown's options and default while preserving unrelated lines.
function Set-IssueTemplateDropdown(
    [string] $Text,
    [string] $DropdownId,
    [string[]] $Options,
    [int] $Default
) {
    $newline = if ($Text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $hasFinalNewline = $Text.EndsWith("`n", [StringComparison]::Ordinal)
    $normalized = $Text.Replace("`r`n", "`n")
    $lines = @($normalized -split "`n")
    if ($hasFinalNewline -and $lines.Count -gt 0 -and $lines[-1] -eq '') {
        $lines = @($lines | Select-Object -SkipLast 1)
    }

    $start = -1
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match "^\s*id:\s*$([regex]::Escape($DropdownId))\s*$") {
            if ($start -ge 0) {
                throw "Multiple dropdowns use id $DropdownId."
            }
            $start = $index
        }
    }
    if ($start -lt 0) {
        throw "Could not find dropdown id $DropdownId."
    }

    $end = $lines.Count
    for ($index = $start + 1; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match '^  - type:') {
            $end = $index
            break
        }
    }
    $optionsIndex = -1
    for ($index = $start; $index -lt $end; $index++) {
        if ($lines[$index] -match '^(?<indent>\s*)options:\s*$') {
            $optionsIndex = $index
            $optionIndent = "$($Matches.indent)  "
            break
        }
    }
    if ($optionsIndex -lt 0) {
        throw "Could not find options for dropdown id $DropdownId."
    }

    $first = $optionsIndex + 1
    $last = $first
    while ($last -lt $end -and $lines[$last] -match '^\s*-\s') {
        $last++
    }
    $before = if ($first -gt 0) { @($lines[0..($first - 1)]) } else { @() }
    $items = @($Options | ForEach-Object { "$optionIndent- $_" })
    $after = if ($last -lt $lines.Count) { @($lines[$last..($lines.Count - 1)]) } else { @() }
    $lines = $before + $items + $after

    $defaultFound = $false
    $newEnd = $end + $items.Count - ($last - $first)
    for ($index = $start; $index -lt $newEnd; $index++) {
        if ($lines[$index] -match '^(?<indent>\s*)default:\s*\d+\s*$') {
            $lines[$index] = "$($Matches.indent)default: $Default"
            $defaultFound = $true
            break
        }
    }
    if (!$defaultFound) {
        throw "Could not find default for dropdown id $DropdownId."
    }

    $result = $lines -join $newline
    if ($hasFinalNewline) {
        $result += $newline
    }
    return $result
}

# Renders both managed dropdowns and verifies that rerendering is idempotent.
function Get-UpdatedIssueTemplate([string] $Text, [pscustomobject] $Options) {
    $updated = Set-IssueTemplateDropdown `
        -Text $Text `
        -DropdownId 'version' `
        -Options $Options.Version `
        -Default $Options.VersionDefault
    $updated = Set-IssueTemplateDropdown `
        -Text $updated `
        -DropdownId 'goodversion' `
        -Options $Options.GoodVersion `
        -Default $Options.GoodVersionDefault
    $verified = Set-IssueTemplateDropdown `
        -Text $updated `
        -DropdownId 'version' `
        -Options $Options.Version `
        -Default $Options.VersionDefault
    $verified = Set-IssueTemplateDropdown `
        -Text $verified `
        -DropdownId 'goodversion' `
        -Options $Options.GoodVersion `
        -Default $Options.GoodVersionDefault
    if ($verified -ne $updated) {
        throw 'Issue template rendering is not idempotent.'
    }
    return $updated
}

$root = Get-GitRepositoryRoot
$path = if ([IO.Path]::IsPathRooted($File)) {
    [IO.Path]::GetFullPath($File)
} else {
    [IO.Path]::GetFullPath((Join-Path $root $File))
}
$displayPath = [IO.Path]::GetRelativePath($root, $path)
Write-Host "Updating issue-template versions ($modeDescription)"

# 1. Build deterministic option lists from published releases.
$releaseVersion = Get-RepositoryReleaseVersion -Root $root
$versions = @(Get-PublishedReleaseVersions -Repository $Repository)
if ($versions.Count -eq 0) {
    throw 'No published releases were found.'
}
$options = New-IssueTemplateOptions -Versions $versions -Major $releaseVersion.Major
Write-ReleaseStatus ready "Supported major: $($releaseVersion.Major).x"
Write-Host "Version options:`n  - $($options.Version -join "`n  - ")"
Write-Host "Version default: $($options.VersionDefault)"
Write-Host "Last-known-good options:`n  - $($options.GoodVersion -join "`n  - ")"
Write-Host "Last-known-good default: $($options.GoodVersionDefault)"

# 2. Render and optionally write the local issue form.
$original = [IO.File]::ReadAllText($path)
$updated = Get-UpdatedIssueTemplate -Text $original -Options $options
# 3. Converge the update through the shared dry-run/apply/push path.
$body = @"
## Description

Refresh the bug-report version dropdowns from published GitHub Releases.

**Related issues**

N/A.

**Required skia PR**

None.

**Areas affected**

- [x] Build, packaging, or CI

## Changes

None - issue-template metadata only.

## Testing

The publishing tests verify version selection and byte-preserving issue-form updates.

## Checklist

- [x] Tests added or updated
- [x] ``Changes`` above lists all public API and behavioral changes (None)
- [x] New/changed public API? N/A
- [x] Native change? N/A
"@
Publish-AutomationFilePullRequest `
    -Root $root `
    -Repository $Repository `
    -Branch $automationBranch `
    -BaseBranch main `
    -Files ([ordered] @{ $displayPath = $updated }) `
    -CommitMessage 'Update issue template version dropdowns' `
    -Title 'Update issue template version dropdowns' `
    -Body $body `
    -Description 'issue-template' `
    -Mode $Mode
