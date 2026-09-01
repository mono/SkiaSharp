$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1')
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1')
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1')
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

# Tests whether the remote automation branch already contains the desired one-file update.
function Test-IssueTemplateAutomationBranch(
    [string] $Root,
    [string] $RemoteSha,
    [string] $MainSha,
    [string] $Path,
    [string] $Content
) {
    if (!$RemoteSha) {
        return $false
    }
    $null = Invoke-Git -Root $Root -Arguments @('fetch', '--quiet', 'origin', $RemoteSha)
    $parent = Invoke-Git -Root $Root -Arguments @('rev-parse', "$RemoteSha^") -AllowFailure
    if ($parent.ExitCode -ne 0 -or $parent.Output -ne $MainSha) {
        return $false
    }
    $changed = @(
        (Invoke-Git -Root $Root -Arguments @('diff', '--name-only', "$MainSha..$RemoteSha")).Output `
            -split "`r?`n" |
            Where-Object { $_ }
    )
    if ($changed.Count -ne 1 -or $changed[0] -ne $Path) {
        return $false
    }

    $temporary = [IO.Path]::GetTempFileName()
    try {
        [IO.File]::WriteAllText($temporary, $Content, [Text.UTF8Encoding]::new($false))
        $desiredBlob = (Invoke-Git -Root $Root -Arguments @('hash-object', $temporary)).Output
    } finally {
        Remove-Item $temporary -Force -ErrorAction SilentlyContinue
    }
    $remoteBlob = Invoke-Git -Root $Root -Arguments @('rev-parse', "$RemoteSha`:$Path") -AllowFailure
    return $remoteBlob.ExitCode -eq 0 -and $remoteBlob.Output -eq $desiredBlob
}

# Creates the automation pull request or reports the existing one.
function Confirm-IssueTemplatePullRequest([string] $Repository, [string] $Branch) {
    $pullRequests = @(
        Invoke-GitHubJsonWithRetry -Arguments @(
            'pr', 'list',
            '--repo', $Repository,
            '--head', $Branch,
            '--base', 'main',
            '--state', 'open',
            '--json', 'number,url'
        )
    )
    if ($pullRequests.Count -gt 1) {
        throw "Multiple open pull requests use $Branch."
    }
    if ($pullRequests) {
        Write-ReleaseStatus ready "Issue-template PR #$($pullRequests[0].number) is open: $($pullRequests[0].url)"
        return
    }
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
    $null = Invoke-GitHub `
        -Arguments @(
            'pr', 'create',
            '--repo', $Repository,
            '--base', 'main',
            '--head', $Branch,
            '--title', 'Update issue template version dropdowns',
            '--body', $body
        ) `
        -WriteOutput
    Write-ReleaseStatus pushed "Created the issue-template PR from $Branch to main."
}

Export-ModuleMember -Function *
