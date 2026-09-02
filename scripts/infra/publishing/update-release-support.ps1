#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates SkiaSharp support tiers from one exact released package version.

.DESCRIPTION
    Adds preview and RC release lines to support.preview. Promotes stable release
    lines to support.stable and removes only that same line from support.preview.
    Push mode owns a line-specific automation branch and pull request.

.PARAMETER Version
    An exact stable or public prerelease SkiaSharp package version.

.PARAMETER File
    The versions.json path, relative to the repository root unless absolute.

.PARAMETER Push
    Updates the owned automation branch and pull request. Without this switch,
    the script is read-only.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $Version,

    [string] $File = 'scripts/infra/docs/versions.json',

    [switch] $Push
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
$repository = $ReleaseRepository
$mode = if ($Push) { 'push' } else { 'dry run' }

# Converts a JSON string or array property into an ordered string array.
function Get-SupportLines([pscustomobject] $Support, [string] $Tier) {
    $property = $Support.PSObject.Properties[$Tier]
    if (!$property -or $null -eq $property.Value) {
        return @()
    }
    return @($property.Value | ForEach-Object { [string] $_ })
}

# Locates the body of one top-level JSON object property without reformatting it.
function Get-TopLevelJsonObjectBody([string] $Text, [string] $Property) {
    $depth = 0
    $inString = $false
    $escaped = $false
    for ($index = 0; $index -lt $Text.Length; $index++) {
        $character = $Text[$index]
        if ($inString) {
            if ($escaped) {
                $escaped = $false
            } elseif ($character -eq '\') {
                $escaped = $true
            } elseif ($character -eq '"') {
                $inString = $false
            }
            continue
        }
        if ($character -eq '"') {
            if ($depth -ne 1) {
                $inString = $true
                continue
            }
            $nameStart = $index + 1
            $nameEnd = $nameStart
            while ($nameEnd -lt $Text.Length -and $Text[$nameEnd] -ne '"') {
                if ($Text[$nameEnd] -eq '\') {
                    $nameEnd++
                }
                $nameEnd++
            }
            if ($nameEnd -ge $Text.Length) {
                throw 'versions.json contains an unterminated property name.'
            }
            $name = $Text.Substring($nameStart, $nameEnd - $nameStart)
            $cursor = $nameEnd + 1
            while ($cursor -lt $Text.Length -and [char]::IsWhiteSpace($Text[$cursor])) {
                $cursor++
            }
            if ($name -ne $Property -or $cursor -ge $Text.Length -or $Text[$cursor] -ne ':') {
                $index = $nameEnd
                continue
            }
            $cursor++
            while ($cursor -lt $Text.Length -and [char]::IsWhiteSpace($Text[$cursor])) {
                $cursor++
            }
            if ($cursor -ge $Text.Length -or $Text[$cursor] -ne '{') {
                throw "Top-level $Property must be a JSON object."
            }
            $objectDepth = 0
            $objectInString = $false
            $objectEscaped = $false
            for ($end = $cursor; $end -lt $Text.Length; $end++) {
                $objectCharacter = $Text[$end]
                if ($objectInString) {
                    if ($objectEscaped) {
                        $objectEscaped = $false
                    } elseif ($objectCharacter -eq '\') {
                        $objectEscaped = $true
                    } elseif ($objectCharacter -eq '"') {
                        $objectInString = $false
                    }
                    continue
                }
                if ($objectCharacter -eq '"') {
                    $objectInString = $true
                } elseif ($objectCharacter -eq '{') {
                    $objectDepth++
                } elseif ($objectCharacter -eq '}') {
                    $objectDepth--
                    if ($objectDepth -eq 0) {
                        return [pscustomobject] @{
                            Start = $cursor + 1
                            Length = $end - $cursor - 1
                        }
                    }
                }
            }
            throw "Top-level $Property object is unterminated."
        }
        if ($character -eq '{') {
            $depth++
        } elseif ($character -eq '}') {
            $depth--
        }
    }
    throw "Could not find the top-level $Property object in versions.json."
}

# Locates one direct string or array property value within a JSON object body.
function Get-TopLevelJsonPropertyValueRange([string] $Body, [string] $Property) {
    $depth = 0
    $inString = $false
    $escaped = $false
    for ($index = 0; $index -lt $Body.Length; $index++) {
        $character = $Body[$index]
        if ($inString) {
            if ($escaped) {
                $escaped = $false
            } elseif ($character -eq '\') {
                $escaped = $true
            } elseif ($character -eq '"') {
                $inString = $false
            }
            continue
        }
        if ($character -eq '"') {
            if ($depth -ne 0) {
                $inString = $true
                continue
            }
            $nameStart = $index + 1
            $nameEnd = $nameStart
            while ($nameEnd -lt $Body.Length -and $Body[$nameEnd] -ne '"') {
                if ($Body[$nameEnd] -eq '\') {
                    $nameEnd++
                }
                $nameEnd++
            }
            if ($nameEnd -ge $Body.Length) {
                throw 'The support object contains an unterminated property name.'
            }
            $name = $Body.Substring($nameStart, $nameEnd - $nameStart)
            $cursor = $nameEnd + 1
            while ($cursor -lt $Body.Length -and [char]::IsWhiteSpace($Body[$cursor])) {
                $cursor++
            }
            if ($name -ne $Property -or $cursor -ge $Body.Length -or $Body[$cursor] -ne ':') {
                $index = $nameEnd
                continue
            }
            $cursor++
            while ($cursor -lt $Body.Length -and [char]::IsWhiteSpace($Body[$cursor])) {
                $cursor++
            }
            if ($cursor -ge $Body.Length -or $Body[$cursor] -notin @('"', '[')) {
                throw "support.$Property must be a JSON string or array."
            }
            $valueStart = $cursor
            if ($Body[$cursor] -eq '"') {
                $cursor++
                $valueEscaped = $false
                while ($cursor -lt $Body.Length) {
                    if ($valueEscaped) {
                        $valueEscaped = $false
                    } elseif ($Body[$cursor] -eq '\') {
                        $valueEscaped = $true
                    } elseif ($Body[$cursor] -eq '"') {
                        return [pscustomobject] @{
                            Start = $valueStart
                            Length = $cursor - $valueStart + 1
                        }
                    }
                    $cursor++
                }
            } else {
                $arrayDepth = 0
                $arrayInString = $false
                $arrayEscaped = $false
                for (; $cursor -lt $Body.Length; $cursor++) {
                    $valueCharacter = $Body[$cursor]
                    if ($arrayInString) {
                        if ($arrayEscaped) {
                            $arrayEscaped = $false
                        } elseif ($valueCharacter -eq '\') {
                            $arrayEscaped = $true
                        } elseif ($valueCharacter -eq '"') {
                            $arrayInString = $false
                        }
                        continue
                    }
                    if ($valueCharacter -eq '"') {
                        $arrayInString = $true
                    } elseif ($valueCharacter -eq '[') {
                        $arrayDepth++
                    } elseif ($valueCharacter -eq ']') {
                        $arrayDepth--
                        if ($arrayDepth -eq 0) {
                            return [pscustomobject] @{
                                Start = $valueStart
                                Length = $cursor - $valueStart + 1
                            }
                        }
                    }
                }
            }
            throw "support.$Property contains an unterminated value."
        }
        if ($character -in @('{', '[')) {
            $depth++
        } elseif ($character -in @('}', ']')) {
            $depth--
        }
    }
    throw "Could not find support.$Property in versions.json."
}

# Renders one support property while retaining the surrounding document text.
function Set-SupportProperty(
    [string] $Text,
    [string] $Tier,
    [string[]] $Lines
) {
    $newline = if ($Text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $supportBody = Get-TopLevelJsonObjectBody -Text $Text -Property support
    $body = $Text.Substring($supportBody.Start, $supportBody.Length)
    $valueRange = Get-TopLevelJsonPropertyValueRange -Body $body -Property $Tier
    $lineStart = $body.LastIndexOf("`n", $valueRange.Start)
    $lineStart = if ($lineStart -lt 0) { 0 } else { $lineStart + 1 }
    $linePrefix = $body.Substring($lineStart, $valueRange.Start - $lineStart)
    $indent = [regex]::Match($linePrefix, '^\s*').Value
    $itemIndent = "$indent  "
    $rendered = if ($Lines.Count -eq 0) {
        '[]'
    } else {
        '[' + $newline +
            (($Lines | ForEach-Object {
                "$itemIndent$($_ | ConvertTo-Json -Compress)"
            }) -join ",$newline") +
            "$newline$indent]"
    }
    $valueIndex = $supportBody.Start + $valueRange.Start
    return $Text.Substring(0, $valueIndex) +
        $rendered +
        $Text.Substring($valueIndex + $valueRange.Length)
}

# Applies the monotonic release-driven support policy.
function Get-UpdatedReleaseSupport([string] $Text, [pscustomobject] $Release) {
    try {
        $document = $Text | ConvertFrom-Json
    } catch {
        throw "versions.json is invalid: $($_.Exception.Message)"
    }
    if (!$document.PSObject.Properties['support']) {
        throw 'versions.json does not contain a support block.'
    }

    $parts = @($Release.Numeric.Split('.'))
    $line = "$($parts[0]).$($parts[1])"
    $stable = [System.Collections.Generic.List[string]]::new()
    foreach ($supportedLine in @(Get-SupportLines $document.support 'stable')) {
        $stable.Add($supportedLine)
    }
    $preview = [System.Collections.Generic.List[string]]::new()
    foreach ($supportedLine in @(Get-SupportLines $document.support 'preview')) {
        $preview.Add($supportedLine)
    }
    $stableChanged = $false
    $previewChanged = $false

    if ($Release.IsPrerelease) {
        if (!$preview.Contains($line)) {
            $preview.Add($line)
            $previewChanged = $true
        }
    } else {
        if (!$stable.Contains($line)) {
            $stable.Add($line)
            $stableChanged = $true
        }
        while ($preview.Remove($line)) {
            $previewChanged = $true
        }
    }

    $updated = $Text
    if ($stableChanged) {
        $updated = Set-SupportProperty -Text $updated -Tier stable -Lines $stable.ToArray()
    }
    if ($previewChanged) {
        $updated = Set-SupportProperty -Text $updated -Tier preview -Lines $preview.ToArray()
    }
    return $updated
}

# Tests whether an owned branch is exactly one desired one-file commit on main.
function Test-ReleaseSupportAutomationBranch(
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

# Creates the line-specific automation pull request or reports the existing one.
function Confirm-ReleaseSupportPullRequest(
    [string] $Repository,
    [string] $Branch,
    [string] $Line,
    [bool] $IsPrerelease
) {
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
        Write-ReleaseStatus ready "Release-support PR #$($pullRequests[0].number) is open: $($pullRequests[0].url)"
        return
    }

    $action = if ($IsPrerelease) {
        "Add $Line to the preview support tier after publishing its preview/RC release."
    } else {
        "Promote $Line to the stable support tier after publishing its stable release."
    }
    $body = @"
## Description

$action Existing supported lines are retained because ending support remains an explicit maintainer decision.

**Related issues**

N/A.

**Required skia PR**

None.

**Areas affected**

- [x] Build, packaging, or CI
- [x] Documentation or samples

## Changes

None - release support metadata only.

## Testing

The publishing tests cover preview, RC, stable promotion, idempotency, multiple supported lines, and preservation of unrelated configuration.

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
            '--title', "Update $Line release support tier",
            '--body', $body
        ) `
        -WriteOutput
    Write-ReleaseStatus pushed "Created the release-support PR from $Branch to main."
}

$release = Get-ReleaseIdentity -PublicVersion $Version
$parts = @($release.Numeric.Split('.'))
$line = "$($parts[0]).$($parts[1])"
$automationBranch = "automation/update-release-support-$line"
$root = Get-GitRepositoryRoot
$path = if ([IO.Path]::IsPathRooted($File)) {
    [IO.Path]::GetFullPath($File)
} else {
    [IO.Path]::GetFullPath((Join-Path $root $File))
}
$displayPath = [IO.Path]::GetRelativePath($root, $path)
Write-Host "Updating release support for $Version ($mode)"

if ($Push) {
    if ($displayPath -eq '..' -or $displayPath.StartsWith("../") -or $displayPath.StartsWith("..\")) {
        throw 'Push mode requires a versions.json path inside the repository.'
    }
    Assert-GitWorktreeClean -Root $root
    $mainSha = Get-ResolvedGitCommit -Root $root -Reference main
    $headSha = (Invoke-Git -Root $root -Arguments @('rev-parse', 'HEAD')).Output
    if ($headSha -ne $mainSha) {
        throw "Push mode must run at current origin/main $mainSha, not $headSha."
    }
}

$original = [IO.File]::ReadAllText($path)
$updated = Get-UpdatedReleaseSupport -Text $original -Release $release
if ($updated -eq $original) {
    Write-ReleaseStatus ready "$displayPath already reflects $Version."
    return
}
if (!$Push) {
    Write-ReleaseStatus plan "Update support tiers in $displayPath for released line $line."
    return
}

$remoteSha = Get-RemoteBranchSha -Root $root -Remote origin -Branch $automationBranch
if (Test-ReleaseSupportAutomationBranch `
    -Root $root `
    -RemoteSha $remoteSha `
    -MainSha $mainSha `
    -Path $displayPath `
    -Content $updated) {
    Write-ReleaseStatus ready "$automationBranch already contains the desired update at $remoteSha."
    Confirm-ReleaseSupportPullRequest `
        -Repository $repository `
        -Branch $automationBranch `
        -Line $line `
        -IsPrerelease $release.IsPrerelease
    return
}

$null = Invoke-Git -Root $root -Arguments @('switch', '-C', $automationBranch, $mainSha) -WriteOutput
[IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($false))
Write-ReleaseStatus applied "Updated $displayPath."
$null = Invoke-Git -Root $root -Arguments @('add', '--', $displayPath)
$null = Invoke-Git `
    -Root $root `
    -Arguments @(
        '-c', 'user.name=github-actions[bot]',
        '-c', 'user.email=41898282+github-actions[bot]@users.noreply.github.com',
        'commit', '-m', "Update $line release support tier"
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
    throw 'Release-support automation branch push could not be verified.'
}
Write-ReleaseStatus pushed "$automationBranch is at $localSha."
Confirm-ReleaseSupportPullRequest `
    -Repository $repository `
    -Branch $automationBranch `
    -Line $line `
    -IsPrerelease $release.IsPrerelease
