#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates the SkiaSharp version dropdowns in the bug-report issue form.

.DESCRIPTION
    Reads published GitHub Releases, regenerates the current and last-known-good
    version options, and preserves all unrelated issue-form text. Push mode owns
    the automation branch, commit, and pull request used by CI.

.PARAMETER Repository
    The GitHub repository whose published releases are read.

.PARAMETER File
    The issue-form path, relative to the repository root unless absolute.

.PARAMETER Apply
    Writes the updated issue form locally without committing or pushing.

.PARAMETER Push
    Updates the owned automation branch and pull request. Without Apply or Push,
    the script is read-only.
#>

param(
    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository = 'mono/SkiaSharp',

    [string] $File = '.github/ISSUE_TEMPLATE/bug-report.yml',

    [switch] $Apply,

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'IssueTemplate.Common.psm1') -Force
$writeLocal = $Apply -or $Push
$writeRemote = $Push
$mode = if ($Push) { 'push' } elseif ($Apply) { 'local apply' } else { 'dry run' }
$root = Get-GitRepositoryRoot
$automationBranch = 'automation/update-issue-template-versions'
$path = if ([IO.Path]::IsPathRooted($File)) {
    [IO.Path]::GetFullPath($File)
} else {
    [IO.Path]::GetFullPath((Join-Path $root $File))
}
$displayPath = [IO.Path]::GetRelativePath($root, $path)
Write-Host "Updating issue-template versions ($mode)"

# 1. Push mode starts from the current remote main and a clean worktree.
if ($writeRemote) {
    if ($displayPath -eq '..' -or $displayPath.StartsWith("../") -or $displayPath.StartsWith("..\")) {
        throw 'Push mode requires an issue-form path inside the repository.'
    }
    if ((Invoke-Git -Root $root -Arguments @('status', '--porcelain')).Output) {
        throw 'Push mode requires a clean worktree.'
    }
    $mainSha = Get-ResolvedGitCommit -Root $root -Reference main
    $headSha = (Invoke-Git -Root $root -Arguments @('rev-parse', 'HEAD')).Output
    if ($headSha -ne $mainSha) {
        throw "Push mode must run at current origin/main $mainSha, not $headSha."
    }
}

# 2. Build deterministic option lists from published releases.
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

# 3. Render and optionally write the local issue form.
$original = [IO.File]::ReadAllText($path)
$updated = Get-UpdatedIssueTemplate -Text $original -Options $options
if ($updated -eq $original) {
    Write-ReleaseStatus ready "$displayPath is current."
    return
}
if (!$writeLocal) {
    Write-ReleaseStatus plan "Update $displayPath."
    return
}
if (!$writeRemote) {
    [IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($false))
    Write-ReleaseStatus applied "Updated $displayPath."
    return
}

# 4. Reuse an identical automation branch or publish one exact replacement.
$remoteSha = Get-RemoteBranchSha -Root $root -Remote origin -Branch $automationBranch
if (Test-IssueTemplateAutomationBranch `
    -Root $root `
    -RemoteSha $remoteSha `
    -MainSha $mainSha `
    -Path $displayPath `
    -Content $updated) {
    Write-ReleaseStatus ready "$automationBranch already contains the desired update at $remoteSha."
    Confirm-IssueTemplatePullRequest -Repository $Repository -Branch $automationBranch
    return
}

[IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($false))
Write-ReleaseStatus applied "Updated $displayPath."
$null = Invoke-Git -Root $root -Arguments @('switch', '-C', $automationBranch, $mainSha) -WriteOutput
$null = Invoke-Git -Root $root -Arguments @('add', '--', $displayPath)
$null = Invoke-Git `
    -Root $root `
    -Arguments @(
        '-c', 'user.name=github-actions[bot]',
        '-c', 'user.email=41898282+github-actions[bot]@users.noreply.github.com',
        'commit', '-m', 'Update issue template version dropdowns'
    ) `
    -WriteOutput
$localSha = (Invoke-Git -Root $root -Arguments @('rev-parse', 'HEAD')).Output
Enable-GitHubGitAuthentication
if ($remoteSha) {
    $null = Invoke-Git `
        -Root $root `
        -Arguments @(
            'push', 'origin',
            "HEAD:refs/heads/$automationBranch",
            "--force-with-lease=refs/heads/$automationBranch`:$remoteSha"
        ) `
        -WriteOutput
} else {
    $null = Invoke-Git `
        -Root $root `
        -Arguments @('push', 'origin', "HEAD:refs/heads/$automationBranch") `
        -WriteOutput
}
if ((Get-RemoteBranchSha -Root $root -Remote origin -Branch $automationBranch) -ne $localSha) {
    throw 'Issue-template automation branch push could not be verified.'
}
Write-ReleaseStatus pushed "$automationBranch is at $localSha."
Confirm-IssueTemplatePullRequest -Repository $Repository -Branch $automationBranch
