#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Prepares matching SkiaSharp and mono/skia release branches.

.DESCRIPTION
    SkiaSharp and HarfBuzzSharp package versions move together. Label-only
    preview, RC, and stable cuts keep both numeric versions. A four-part hotfix
    or post-stable patch increments HarfBuzzSharp whenever it increments
    SkiaSharp. New Skia milestone buckets must already exist in the selected
    base before release preparation.

.PARAMETER Release
    The exact release identity, such as 4.153.0-preview.1, 4.153.0-rc.1, or
    4.153.0-stable. Stable is normalized to the release/4.153.0 branch.

.PARAMETER Base
    The SkiaSharp branch or commit SHA from which to create the release.

.PARAMETER Apply
    Creates and validates missing local branches and commits without pushing.

.PARAMETER Push
    Creates local state as needed, then pushes branches and creates the stable bump PR.
    Without Apply or Push, the script is a dry run.
#>

param(
    [Parameter(Mandatory)]
    [string] $Release,

    [Parameter(Mandatory)]
    [string] $Base,

    [switch] $Apply,

    [switch] $Push
)

Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$writeLocal = $Apply -or $Push
$skiaRemote = $ReleaseSkiaRemote
$skiaPath = $ReleaseSkiaPath
$variablesPath = $ReleaseVariablesPath
$versionsPath = $ReleaseVersionsPath

# Rewrites the SkiaSharp version and release label in the pipeline variables.
function Set-VersionVariables([string] $Text, [string] $SkiaSharpVersion, [string] $PreviewLabel) {
    $Text = $Text -replace '(?m)^([ \t]*SKIASHARP_VERSION:[ \t]*).+$', "`${1}$SkiaSharpVersion"
    return $Text -replace '(?m)^([ \t]*PREVIEW_LABEL:[ \t]*).+$', "`${1}'$PreviewLabel'"
}

# Rewrites every SkiaSharp and HarfBuzzSharp package/file version.
function Set-PackageVersions([string] $Text, [string] $SkiaSharpVersion, [string] $HarfBuzzSharpVersion) {
    $fileVersion = if (($SkiaSharpVersion -split '\.').Count -eq 3) { "$SkiaSharpVersion.0" } else { $SkiaSharpVersion }
    $Text = $Text -replace '(?m)^([ \t]*SkiaSharp\S*[ \t]+nuget[ \t]+)\S+', "`${1}$SkiaSharpVersion"
    $Text = $Text -replace '(?m)^([ \t]*SkiaSharp[ \t]+file[ \t]+)\S+', "`${1}$fileVersion"
    $Text = $Text -replace '(?m)^([ \t]*HarfBuzzSharp\S*[ \t]+nuget[ \t]+)\S+', "`${1}$HarfBuzzSharpVersion"
    $Text = $Text -replace '(?m)^([ \t]*HarfBuzzSharp[ \t]+file[ \t]+)\S+', "`${1}$HarfBuzzSharpVersion"
    return $Text
}

# Reads the coupled package versions and release label from one commit.
function Get-VersionState([string] $Commit) {
    $variables = Get-GitFileText -Commit $Commit -Path $variablesPath
    $versions = Get-GitFileText -Commit $Commit -Path $versionsPath
    $skiaVariable = [regex]::Match($variables, '(?m)^[ \t]*SKIASHARP_VERSION:[ \t]*(?<version>\S+)[ \t]*$')
    $previewLabel = [regex]::Match($variables, "(?m)^[ \t]*PREVIEW_LABEL:[ \t]*['`"]?(?<label>[^'`"`r`n]+)")
    $skiaPackage = [regex]::Match($versions, '(?m)^SkiaSharp[ \t]+nuget[ \t]+(?<version>\S+)[ \t]*$')
    $harfBuzzPackage = [regex]::Match($versions, '(?m)^HarfBuzzSharp[ \t]+nuget[ \t]+(?<version>\S+)[ \t]*$')
    if (!$skiaVariable.Success -or !$previewLabel.Success -or !$skiaPackage.Success -or !$harfBuzzPackage.Success) {
        throw "Unable to read release versions from $Commit."
    }
    if ($skiaVariable.Groups['version'].Value -ne $skiaPackage.Groups['version'].Value) {
        throw "$Commit has inconsistent SkiaSharp versions."
    }
    return [pscustomobject] @{
        SkiaSharp = $skiaPackage.Groups['version'].Value
        HarfBuzzSharp = $harfBuzzPackage.Groups['version'].Value
        PreviewLabel = $previewLabel.Groups['label'].Value.Trim()
    }
}

# Increments HarfBuzzSharp within the milestone bucket selected by the base.
function Get-NextHarfBuzzVersion([string] $Version) {
    if ($Version -notmatch '^(?<native>\d+\.\d+\.\d+)(?:\.(?<revision>\d+))?$') {
        throw "Cannot increment HarfBuzzSharp version '$Version'."
    }
    $revision = if ($Matches.revision) { [int] $Matches.revision + 1 } else { 1 }
    return "$($Matches.native).$revision"
}

# Keeps HarfBuzz for label-only cuts and increments it for a four-part hotfix.
function Get-ReleaseHarfBuzzVersion([pscustomobject] $BaseVersions, [string] $ReleaseVersion) {
    if ($BaseVersions.SkiaSharp -eq $ReleaseVersion) {
        return $BaseVersions.HarfBuzzSharp
    }

    $releaseParts = $ReleaseVersion -split '\.'
    $stableBase = $releaseParts[0..2] -join '.'
    if ($releaseParts.Count -eq 4 -and $BaseVersions.SkiaSharp -eq $stableBase) {
        return Get-NextHarfBuzzVersion -Version $BaseVersions.HarfBuzzSharp
    }

    throw (
        "Base contains SkiaSharp $($BaseVersions.SkiaSharp), but the release " +
        "requests $ReleaseVersion. Normal preview, RC, and stable cuts must use " +
        "a base already at the requested numeric version; only a four-part " +
        "hotfix may advance it here.")
}

# Rejects any branch transformation that changes only one package family.
function Assert-CoupledPackageVersions([string] $BaseSha, [string] $SkiaSharpVersion, [string] $HarfBuzzSharpVersion) {
    $baseVersions = Get-VersionState -Commit $BaseSha
    $skiaChanged = $baseVersions.SkiaSharp -ne $SkiaSharpVersion
    $harfBuzzChanged = $baseVersions.HarfBuzzSharp -ne $HarfBuzzSharpVersion
    if ($skiaChanged -ne $harfBuzzChanged) {
        throw (
            "SkiaSharp and HarfBuzzSharp versions must change together: " +
            "$($baseVersions.SkiaSharp) -> $SkiaSharpVersion; " +
            "$($baseVersions.HarfBuzzSharp) -> $HarfBuzzSharpVersion.")
    }
}

# Tests whether a commit already contains the exact coupled version state.
function Test-VersionMetadata(
    [string] $Commit,
    [string] $SkiaSharpVersion,
    [string] $PreviewLabel,
    [string] $HarfBuzzSharpVersion
) {
    $variables = Get-GitFileText -Commit $Commit -Path $variablesPath
    $versions = Get-GitFileText -Commit $Commit -Path $versionsPath
    $expectedVariables = Set-VersionVariables `
        -Text $variables `
        -SkiaSharpVersion $SkiaSharpVersion `
        -PreviewLabel $PreviewLabel
    $expectedVersions = Set-PackageVersions `
        -Text $versions `
        -SkiaSharpVersion $SkiaSharpVersion `
        -HarfBuzzSharpVersion $HarfBuzzSharpVersion
    return $variables -eq $expectedVariables -and $versions -eq $expectedVersions
}

# Requires a branch commit to contain exact versions and the expected Skia gitlink.
function Assert-VersionMetadata(
    [string] $Commit,
    [string] $Branch,
    [string] $SkiaSharpVersion,
    [string] $PreviewLabel,
    [string] $HarfBuzzSharpVersion,
    [string] $ExpectedSkiaSha
) {
    if (!(Test-VersionMetadata `
        -Commit $Commit `
        -SkiaSharpVersion $SkiaSharpVersion `
        -PreviewLabel $PreviewLabel `
        -HarfBuzzSharpVersion $HarfBuzzSharpVersion)) {
        throw "$Branch at $Commit has different version metadata."
    }
    $actualSkiaSha = Get-GitTreeEntrySha -Commit $Commit -Path $skiaPath
    if ($actualSkiaSha -ne $ExpectedSkiaSha) {
        throw "$Branch at $Commit references mono/skia $actualSkiaSha, expected $ExpectedSkiaSha."
    }
}

# Converges one local SkiaSharp branch to an exact version commit.
function Ensure-VersionBranch(
    [string] $Branch,
    [string] $BaseSha,
    [string] $SkiaSharpVersion,
    [string] $PreviewLabel,
    [string] $HarfBuzzSharpVersion,
    [string] $ExpectedSkiaSha,
    [string] $CommitMessage
) {
    Assert-CoupledPackageVersions `
        -BaseSha $BaseSha `
        -SkiaSharpVersion $SkiaSharpVersion `
        -HarfBuzzSharpVersion $HarfBuzzSharpVersion

    # Validate an existing remote branch before trusting or fetching it.
    $remoteSha = Get-RemoteBranchSha -Remote origin -Branch $Branch
    if ($remoteSha) {
        git fetch --quiet origin "refs/heads/$Branch"
        Assert-VersionMetadata `
            -Commit $remoteSha `
            -Branch "origin/$Branch" `
            -SkiaSharpVersion $SkiaSharpVersion `
            -PreviewLabel $PreviewLabel `
            -HarfBuzzSharpVersion $HarfBuzzSharpVersion `
            -ExpectedSkiaSha $ExpectedSkiaSha
    }

    # Validate any local branch against both desired metadata and remote identity.
    $localSha = Get-LocalBranchSha -Repository . -Branch $Branch
    if ($localSha) {
        Assert-VersionMetadata `
            -Commit $localSha `
            -Branch $Branch `
            -SkiaSharpVersion $SkiaSharpVersion `
            -PreviewLabel $PreviewLabel `
            -HarfBuzzSharpVersion $HarfBuzzSharpVersion `
            -ExpectedSkiaSha $ExpectedSkiaSha
        if ($remoteSha -and $localSha -ne $remoteSha) {
            throw "Local $Branch differs from origin/$Branch."
        }
    }

    # A validated remote branch is complete; local Apply must not recreate or check it out.
    if ($remoteSha) {
        Write-ReleaseStatus ready "SkiaSharp $Branch exists at $remoteSha."
        return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
    }

    # Report the desired local action without inventing a commit SHA in dry-run mode.
    if (!$writeLocal) {
        if ($localSha) {
            Write-ReleaseStatus ready "SkiaSharp $Branch is local at $localSha and would be pushed."
        } else {
            Write-ReleaseStatus plan (
                "Create SkiaSharp $Branch from $BaseSha with SkiaSharp " +
                "$SkiaSharpVersion, HarfBuzzSharp $HarfBuzzSharpVersion, " +
                "and label $PreviewLabel.")
        }
        return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
    }

    # Reuse a valid local branch or create its single deterministic version commit.
    if ($localSha) {
        git switch $Branch | Out-Host
    } else {
        git switch -c $Branch $BaseSha | Out-Host
        $updatedVariables = Set-VersionVariables `
            -Text (Get-Content $variablesPath -Raw) `
            -SkiaSharpVersion $SkiaSharpVersion `
            -PreviewLabel $PreviewLabel
        $updatedVersions = Set-PackageVersions `
            -Text (Get-Content $versionsPath -Raw) `
            -SkiaSharpVersion $SkiaSharpVersion `
            -HarfBuzzSharpVersion $HarfBuzzSharpVersion
        Set-Content $variablesPath $updatedVariables -NoNewline
        Set-Content $versionsPath $updatedVersions -NoNewline
        if (git status --porcelain -- $variablesPath $versionsPath) {
            git add -- $variablesPath $versionsPath
            git -c user.name='SkiaSharp Release Bot' -c user.email='noreply@github.com' `
                commit -m $CommitMessage | Out-Host
        }
        $localSha = (git rev-parse HEAD).Trim()
        Assert-VersionMetadata `
            -Commit $localSha `
            -Branch $Branch `
            -SkiaSharpVersion $SkiaSharpVersion `
            -PreviewLabel $PreviewLabel `
            -HarfBuzzSharpVersion $HarfBuzzSharpVersion `
            -ExpectedSkiaSha $ExpectedSkiaSha
    }

    Write-ReleaseStatus applied "SkiaSharp $Branch is local at $localSha."
    return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
}

# Converges the local mono/skia branch to the parent repository's gitlink.
function Ensure-SkiaBranch([string] $Branch, [string] $ExpectedSha) {
    # Validate remote and local branch identities before changing the submodule checkout.
    $remoteSha = Get-RemoteBranchSha $skiaRemote $Branch
    if ($remoteSha -and $remoteSha -ne $ExpectedSha) {
        throw "mono/skia $Branch exists at $remoteSha, expected $ExpectedSha."
    }

    $localSha = $null
    if (Test-Path "$skiaPath/.git") {
        $localSha = Get-LocalBranchSha -Repository $skiaPath -Branch $Branch
        if ($localSha -and $localSha -ne $ExpectedSha) {
            throw "Local mono/skia $Branch is at $localSha, expected $ExpectedSha."
        }
    }

    # A matching remote Skia branch is complete and does not require local submodule state.
    if ($remoteSha) {
        Write-ReleaseStatus ready "mono/skia $Branch exists at $remoteSha."
        return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
    }

    # Report the local branch action without initializing the submodule in dry-run mode.
    if (!$writeLocal) {
        if ($localSha) {
            Write-ReleaseStatus ready "mono/skia $Branch is local at $localSha and would be pushed."
        } else {
            Write-ReleaseStatus plan "Create mono/skia $Branch at $ExpectedSha."
        }
        return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
    }

    # Initialize the pinned submodule commit and create or select its matching branch.
    git submodule update --init --checkout -- $skiaPath | Out-Host
    git -C $skiaPath fetch --quiet origin $ExpectedSha
    $localSha = Get-LocalBranchSha -Repository $skiaPath -Branch $Branch
    if ($localSha) {
        if ($localSha -ne $ExpectedSha) {
            throw "Local mono/skia $Branch is at $localSha, expected $ExpectedSha."
        }
        git -C $skiaPath switch $Branch | Out-Host
    } else {
        git -C $skiaPath switch -c $Branch $ExpectedSha | Out-Host
        $localSha = $ExpectedSha
    }

    Write-ReleaseStatus applied "mono/skia $Branch is local at $localSha."
    return [pscustomobject] @{ Branch = $Branch; LocalSha = $localSha; RemoteSha = $remoteSha }
}

# Detects whether maintenance already moved to the next preview version.
function Test-MaintenanceAdvanced([string] $Commit, [string] $NextVersion) {
    $variables = Get-GitFileText -Commit $Commit -Path $variablesPath
    if ($variables -notmatch '(?m)^[ \t]*SKIASHARP_VERSION:[ \t]*(?<version>\d+\.\d+\.\d+)[ \t]*$') {
        return $false
    }
    $actualVersion = [version] $Matches.version
    return $variables -match "(?m)^[ \t]*PREVIEW_LABEL:[ \t]*'preview\.0'[ \t]*$" -and
        $actualVersion -ge [version] $NextVersion
}

# Calculates the post-stable SkiaSharp and HarfBuzzSharp versions.
function Get-NextVersions([string] $Version, [string] $CurrentHarfBuzzVersion) {
    $parts = $Version -split '\.'
    $nextVersion = "$($parts[0]).$($parts[1]).$([int] $parts[2] + 1)"
    return [pscustomobject] @{
        SkiaSharp = $nextVersion
        HarfBuzzSharp = Get-NextHarfBuzzVersion $CurrentHarfBuzzVersion
    }
}

# Finds the unique current bump PR and rejects a closed, unmerged attempt.
function Get-BumpPullRequest([string] $Branch, [string] $BaseBranch) {
    $pullRequests = @(gh pr list `
        --repo mono/SkiaSharp `
        --head $Branch `
        --base $BaseBranch `
        --state all `
        --json number,state,mergedAt,url | ConvertFrom-Json)
    if ($pullRequests.Count -gt 1) {
        throw "Multiple pull requests exist for $Branch."
    }
    $pullRequest = $pullRequests | Select-Object -First 1
    if ($pullRequest -and $pullRequest.state -eq 'CLOSED' -and !$pullRequest.mergedAt) {
        throw "The pull request for $Branch was closed without merging."
    }
    return $pullRequest
}

# Creates the stable bump PR, or reports the pending action.
function Ensure-BumpPullRequest(
    [string] $Branch,
    [string] $BaseBranch,
    [string] $ReleasedVersion,
    [string] $NextVersion,
    [string] $NextHarfBuzzVersion,
    [pscustomobject] $Existing
) {
    if ($Existing) {
        $state = if ($Existing.mergedAt) { 'merged' } else { 'open' }
        Write-ReleaseStatus ready "Bump PR #$($Existing.number) is ${state}: $($Existing.url)"
        return
    }
    if (!$Push) {
        Write-ReleaseStatus plan "Create a bump PR from $Branch to $BaseBranch."
        return
    }

    # Render the repository's required PR sections for the deterministic version-only change.
    $body = @"
## Description

Advance ``$BaseBranch`` after releasing ``$ReleasedVersion`` by returning it
to ``preview.0`` at SkiaSharp $NextVersion and HarfBuzzSharp
$NextHarfBuzzVersion.

**Related issues**

N/A.

**Required skia PR**

None.

**Areas affected**

- [x] Build, packaging, or CI

## Changes

None - version metadata only.

## Testing

The release preparation script verified the version transformation.

## Checklist

- [x] Tests added or updated (not needed; version metadata only)
- [x] ``Changes`` above lists all public API and behavioral changes (None)
- [x] New/changed public API? N/A
- [x] Native change? N/A
"@
    # Open the human-owned PR without enabling merge or auto-merge.
    gh pr create `
        --repo mono/SkiaSharp `
        --head $Branch `
        --base $BaseBranch `
        --title "Bump to the next version ($NextVersion) after release" `
        --body $body | Out-Host
    Write-ReleaseStatus pushed "Created the bump PR from $Branch to $BaseBranch."
}

# 1. Parse the requested release.
# 1.1 Validate the numeric version, channel, and iteration.
$releasePattern = '^(?<version>\d+\.\d+\.\d+(?:\.\d+)?)-(?:(?<channel>preview|rc)\.(?<iteration>[1-9]\d*)|(?<stable>stable))$'
if ($Release -notmatch $releasePattern) {
    throw "Invalid release identity '$Release'."
}

# 1.2 Normalize stable input to the branch identity without "-stable".
$version = $Matches.version
$isStable = [bool] $Matches.stable
$label = if ($isStable) { 'stable' } else { "$($Matches.channel).$($Matches.iteration)" }
$identity = if ($isStable) { $version } else { $Release }
$releaseBranch = "release/$identity"
$mode = if ($Push) { 'push' } elseif ($Apply) { 'local apply' } else { 'dry run' }
Write-Host "Preparing $identity ($mode)"

# 2. Prepare the exact release branches.
# 2.1 Resolve the source commit and read both package versions.
$baseSha = Get-ResolvedGitCommit -Reference $Base
if ($env:GITHUB_OUTPUT) {
    Add-Content $env:GITHUB_OUTPUT "base_sha=$baseSha"
}
Write-ReleaseStatus ready "Base $Base resolves to $baseSha."
$baseVersions = Get-VersionState -Commit $baseSha
if (!(Test-VersionMetadata `
    -Commit $baseSha `
    -SkiaSharpVersion $baseVersions.SkiaSharp `
    -PreviewLabel $baseVersions.PreviewLabel `
    -HarfBuzzSharpVersion $baseVersions.HarfBuzzSharp)) {
    throw "$Base at $baseSha has inconsistent package-family version metadata."
}
$baseSkiaSha = Get-GitTreeEntrySha -Commit $baseSha -Path $skiaPath

# 2.2 Keep the pair for label-only cuts; increment both for a new hotfix.
$releaseHarfBuzzVersion = Get-ReleaseHarfBuzzVersion -BaseVersions $baseVersions -ReleaseVersion $version
Write-ReleaseStatus ready (
    "Release versions: SkiaSharp $($baseVersions.SkiaSharp) -> $version; " +
    "HarfBuzzSharp $($baseVersions.HarfBuzzSharp) -> $releaseHarfBuzzVersion.")

# 2.3 Ensure the SkiaSharp branch contains the coupled package versions.
$releaseState = Ensure-VersionBranch `
    -Branch $releaseBranch `
    -BaseSha $baseSha `
    -SkiaSharpVersion $version `
    -PreviewLabel $label `
    -HarfBuzzSharpVersion $releaseHarfBuzzVersion `
    -ExpectedSkiaSha $baseSkiaSha `
    -CommitMessage "Create release branch for $identity"

# 2.4 Ensure mono/skia has the matching branch at the exact gitlink.
$releaseCommit = if ($releaseState.LocalSha) {
    $releaseState.LocalSha
} elseif ($releaseState.RemoteSha) {
    $releaseState.RemoteSha
} else {
    $baseSha
}
$releaseSkiaSha = Get-GitTreeEntrySha -Commit $releaseCommit -Path $skiaPath
$skiaState = Ensure-SkiaBranch -Branch $releaseBranch -ExpectedSha $releaseSkiaSha

# 3. Prepare the post-stable bump.
$bumpState = $null
$bumpPullRequest = $null
if ($isStable -and ($version -split '\.').Count -eq 3) {
    # 3.1 Resolve the maintenance branch that receives the next version.
    $parts = $version -split '\.'
    $maintenanceBranch = "release/$($parts[0]).$($parts[1]).x"
    $maintenanceSha = Get-RemoteBranchSha origin $maintenanceBranch
    if (!$maintenanceSha) {
        throw "origin/$maintenanceBranch does not exist."
    }
    git fetch --quiet origin "refs/heads/$maintenanceBranch"

    # 3.2 Bump SkiaSharp and HarfBuzzSharp together for the next build.
    $next = Get-NextVersions -Version $version -CurrentHarfBuzzVersion $releaseHarfBuzzVersion

    # 3.3 Reuse an advanced branch or converge the bump branch and PR.
    if (Test-MaintenanceAdvanced -Commit $maintenanceSha -NextVersion $next.SkiaSharp) {
        Write-ReleaseStatus ready "$maintenanceBranch already contains $($next.SkiaSharp) or later at preview.0."
    } else {
        $bumpBranch = "bump-version-$($next.SkiaSharp)"
        $bumpPullRequest = Get-BumpPullRequest -Branch $bumpBranch -BaseBranch $maintenanceBranch
        if ($bumpPullRequest -and $bumpPullRequest.mergedAt) {
            Ensure-BumpPullRequest `
                -Branch $bumpBranch `
                -BaseBranch $maintenanceBranch `
                -ReleasedVersion $identity `
                -NextVersion $next.SkiaSharp `
                -NextHarfBuzzVersion $next.HarfBuzzSharp `
                -Existing $bumpPullRequest
        } else {
            $maintenanceSkiaSha = Get-GitTreeEntrySha -Commit $maintenanceSha -Path $skiaPath
            $bumpState = Ensure-VersionBranch `
                -Branch $bumpBranch `
                -BaseSha $maintenanceSha `
                -SkiaSharpVersion $next.SkiaSharp `
                -PreviewLabel 'preview.0' `
                -HarfBuzzSharpVersion $next.HarfBuzzSharp `
                -ExpectedSkiaSha $maintenanceSkiaSha `
                -CommitMessage "Bump to the next version ($($next.SkiaSharp)) after release"
        }
    }
}

# 4. Publish or report every prepared remote action.
if ($Push) {
    Enable-GitHubGitAuthentication
}

# 4.1 Push mono/skia before the SkiaSharp branch that references it.
Push-ReleaseBranch `
    -Repository $skiaPath `
    -Remote $skiaRemote `
    -Branch $skiaState.Branch `
    -LocalSha $skiaState.LocalSha `
    -RemoteSha $skiaState.RemoteSha `
    -Description 'mono/skia' `
    -Push:$Push

# 4.2 Push the exact SkiaSharp release branch.
Push-ReleaseBranch `
    -Repository . `
    -Remote origin `
    -Branch $releaseState.Branch `
    -LocalSha $releaseState.LocalSha `
    -RemoteSha $releaseState.RemoteSha `
    -Description 'SkiaSharp' `
    -Push:$Push

# 4.3 Push the stable bump and create its maintenance PR.
if ($bumpState) {
    Push-ReleaseBranch `
        -Repository . `
        -Remote origin `
        -Branch $bumpState.Branch `
        -LocalSha $bumpState.LocalSha `
        -RemoteSha $bumpState.RemoteSha `
        -Description 'SkiaSharp' `
        -Push:$Push
    Ensure-BumpPullRequest `
        -Branch $bumpState.Branch `
        -BaseBranch $maintenanceBranch `
        -ReleasedVersion $identity `
        -NextVersion $next.SkiaSharp `
        -NextHarfBuzzVersion $next.HarfBuzzSharp `
        -Existing $bumpPullRequest
}

Write-Host "Release preparation complete ($mode)."
