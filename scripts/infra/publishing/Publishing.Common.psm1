$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1')
Import-Module (Join-Path $PSScriptRoot 'GitHub.Common.psm1')

# Shared repository paths and release contracts.
New-Variable -Scope Script -Option ReadOnly -Name ReleaseRepository -Value 'mono/SkiaSharp'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSkiaRemote -Value 'https://github.com/mono/skia.git'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSkiaPath -Value 'externals/skia'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseVariablesPath -Value 'scripts/azure-templates-variables.yml'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseVersionsPath -Value 'scripts/VERSIONS.txt'

# Writes one concise publishing state transition.
function Write-ReleaseStatus([string] $State, [string] $Message) {
    Write-Host "[$State] $Message"
}

# Reads the managed SkiaSharp major version and current Skia milestone.
function Get-RepositoryReleaseVersion([string] $Root) {
    $path = Join-Path $Root $ReleaseVersionsPath
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "$ReleaseVersionsPath does not exist."
    }
    $major = $null
    $milestone = $null
    foreach ($line in Get-Content -LiteralPath $path) {
        if ($line -match '^\s*SkiaSharp\s+nuget\s+(?<major>\d+)\.') {
            $major = [int] $Matches.major
        } elseif ($line -match '^\s*libSkiaSharp\s+milestone\s+(?<milestone>\d+)\s*$') {
            $milestone = [int] $Matches.milestone
        }
    }
    if ($null -eq $major -or $null -eq $milestone) {
        throw "Could not read SkiaSharp/libSkiaSharp versions from $path."
    }
    return [pscustomobject] @{ Major = $major; Milestone = $milestone }
}

# Pushes one explicit release branch, or reports the skipped command.
function Push-ReleaseBranch(
    [string] $Root,
    [string] $Remote,
    [string] $Branch,
    [string] $LocalSha,
    [string] $RemoteSha,
    [string] $Description,
    [switch] $Push
) {
    if ($RemoteSha) {
        Write-ReleaseStatus ready "$Description $Branch is already pushed at $RemoteSha."
        return
    }
    if (!$Push) {
        Write-ReleaseStatus skipped (
            "Skipping: git -C $Root push $Remote $Branch`:refs/heads/$Branch (requires -Push).")
        return
    }
    if (!$LocalSha) {
        throw "$Description $Branch has no local commit to push."
    }

    $null = Invoke-Git `
        -Root $Root `
        -Arguments @('push', $Remote, "$Branch`:refs/heads/$Branch") `
        -WriteOutput
    $actual = Get-RemoteBranchSha -Root $Root -Remote $Remote -Branch $Branch
    if ($actual -ne $LocalSha) {
        throw "$Description $Branch push verification failed."
    }
    Write-ReleaseStatus pushed "$Description $Branch is at $actual."
}

# Parses a release branch or milestone title into its shipping-order identity.
function ConvertTo-ReleaseMilestone([string] $Value) {
    $match = [regex]::Match(
        $Value,
        '^(?:release/)?(?<numeric>\d+\.\d+\.\d+(?:\.\d+)?)(?:-(?<channel>preview|rc)\.(?<iteration>\d+))?$')
    if (!$match.Success) {
        return $null
    }
    $numericText = $match.Groups['numeric'].Value
    $parts = @($numericText.Split('.') | ForEach-Object { [int] $_ })
    $hotfix = if ($parts.Count -eq 4) { $parts[3] } else { 0 }
    $channel = if ($match.Groups['channel'].Success) { $match.Groups['channel'].Value } else { $null }
    $iteration = if ($match.Groups['iteration'].Success) { [int] $match.Groups['iteration'].Value } else { 0 }
    $channelRank = switch ($channel) {
        'preview' { 0 }
        'rc' { 1 }
        default { 2 }
    }
    $title = $numericText
    if ($channel) {
        $title += "-$channel.$iteration"
    }
    return [pscustomobject] @{
        Title = $title
        NumericKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}' -f $parts[0], $parts[1], $parts[2], $hotfix
        Channel = $channel
        SortKey = '{0:D10}.{1:D10}.{2:D10}.{3:D10}.{4:D2}.{5:D10}' -f
            $parts[0], $parts[1], $parts[2], $hotfix, $channelRank, $iteration
    }
}

# Selects the greatest exact NuGet tag that shipped a milestone.
function Get-ShippedTag([string] $Title, [string[]] $Tags) {
    if ($Title -match '-(?:preview|rc)\.') {
        $pattern = '^v' + [regex]::Escape($Title) + '\.(?<first>\d+)(?:\.(?<second>\d+))?$'
        $tagMatches = foreach ($tag in $Tags) {
            $match = [regex]::Match($tag, $pattern)
            if ($match.Success) {
                [pscustomobject] @{
                    Tag = $tag
                    First = [long] $match.Groups['first'].Value
                    Second = if ($match.Groups['second'].Success) {
                        [long] $match.Groups['second'].Value
                    } else {
                        [long] -1
                    }
                }
            }
        }
        return ($tagMatches | Sort-Object First, Second -Descending | Select-Object -First 1).Tag
    }
    $exact = "v$Title"
    if ($Tags -contains $exact) {
        return $exact
    }
    return $null
}

# Reads all non-peeled remote release tags.
function Get-RemoteReleaseTags([string] $Root) {
    $output = (Invoke-Git -Root $Root -Arguments @('ls-remote', '--tags', 'origin', 'refs/tags/v*')).Output
    $tags = foreach ($line in @($output -split "`r?`n")) {
        if ($line -and $line -match '^[^\s]+\s+refs/tags/(?<tag>.+)$' -and !$Matches.tag.EndsWith('^{}')) {
            $Matches.tag
        }
    }
    return @($tags | Sort-Object -Unique)
}

# Assigns one issue or pull request to a milestone and verifies remote writes.
function Set-GitHubItemMilestone(
    [string] $Repository,
    [int] $Number,
    [int] $MilestoneNumber,
    [string] $MilestoneTitle,
    [string] $Description,
    [switch] $Push
) {
    $arguments = @('api', "repos/$Repository/issues/$Number", '-X', 'PATCH', '-F', "milestone=$MilestoneNumber")
    $null = Invoke-GitHubMutation -Arguments $arguments -Description $Description -Push:$Push
    if ($Push) {
        $actual = Get-GitHubIssue -Repository $Repository -Number $Number
        if ([string] $actual.milestone.title -ne $MilestoneTitle) {
            throw "GitHub item #$Number milestone update could not be verified."
        }
        Write-ReleaseStatus applied "$Description verified."
    }
}

# Parses an exact public package version into branch, tag, and title identities.
function Get-ReleaseIdentity([string] $PublicVersion) {
    $prerelease = [regex]::Match(
        $PublicVersion,
        '^(?<numeric>\d+\.\d+\.\d+(?:\.\d+)?)-(?<channel>preview|rc)\.' +
            '(?<iteration>[1-9]\d*)\.(?<build>\d+(?:\.\d+)?)$')
    if ($prerelease.Success) {
        $numeric = $prerelease.Groups['numeric'].Value
        $channel = $prerelease.Groups['channel'].Value
        $iteration = $prerelease.Groups['iteration'].Value
        $identity = "$numeric-$channel.$iteration"
        $channelTitle = if ($channel -eq 'rc') { 'RC' } else { 'Preview' }
        return [pscustomobject] @{
            Numeric = $numeric
            Branch = "release/$identity"
            Tag = "v$PublicVersion"
            Title = "Version $numeric ($channelTitle $iteration)"
            IsPrerelease = $true
        }
    }

    if ($PublicVersion -match '^\d+\.\d+\.\d+(?:\.\d+)?$') {
        return [pscustomobject] @{
            Numeric = $PublicVersion
            Branch = "release/$PublicVersion"
            Tag = "v$PublicVersion"
            Title = "Version $PublicVersion"
            IsPrerelease = $false
        }
    }

    throw "Version must be stable X.Y.Z[.F] or an exact public X.Y.Z[.F]-(preview|rc).N.BUILD version."
}

# Resolves a prerelease identity to its one exact public NuGet package version.
function Resolve-NuGetPackageVersion([string] $PackageId, [string] $Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+(?:\.\d+)?-(?:preview|rc)\.[1-9]\d*$') {
        return $Version
    }

    $lowerId = $PackageId.ToLowerInvariant()
    $uri = "https://api.nuget.org/v3-flatcontainer/$lowerId/index.json"
    $versionsFound = @(
        (Invoke-RestMethod -Uri $uri).versions |
            Where-Object { $_ -match "^$([regex]::Escape($Version))\.\d+(?:\.\d+)?$" }
    )
    if ($versionsFound.Count -ne 1) {
        $found = if ($versionsFound) { $versionsFound -join ', ' } else { 'none' }
        throw "$PackageId $Version must match exactly one public NuGet version; found $found."
    }
    return $versionsFound[0]
}

# Reads the repository branch and commit from one public NuGet nuspec.
function Get-NuGetPackageSource([string] $PackageId, [string] $PackageVersion) {
    $lowerId = $PackageId.ToLowerInvariant()
    $lowerVersion = $PackageVersion.ToLowerInvariant()
    $uri = "https://api.nuget.org/v3-flatcontainer/$lowerId/$lowerVersion/$lowerId.nuspec"
    [xml] $xml = (Invoke-WebRequest -Uri $uri).Content
    $metadata = $xml.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
    $repository = $metadata.SelectSingleNode("*[local-name()='repository']")
    if (!$repository.branch -or $repository.commit -notmatch '^[0-9a-f]{40}$') {
        throw "$PackageId $PackageVersion does not identify a repository branch and commit."
    }
    return [pscustomobject] @{
        Branch = [string] $repository.branch
        Commit = [string] $repository.commit
    }
}

# Executes one GitHub mutation, or reports the exact skipped command.
function Invoke-GitHubMutation([string[]] $Arguments, [string] $Description, [switch] $Push) {
    if (!$Push) {
        Write-ReleaseStatus skipped "Skipping: $(Format-Command 'gh' $Arguments) (requires -Push; $Description)."
        return $null
    }
    Write-ReleaseStatus applying $Description
    return Invoke-GitHubJsonWithRetry -Arguments $Arguments
}

# Creates one immutable release tag, or reports the skipped command.
function Push-ReleaseTag(
    [string] $Root,
    [string] $Remote,
    [string] $Tag,
    [string] $SourceCommit,
    [switch] $Push
) {
    $actual = Get-RemoteTagSha -Root $Root -Remote $Remote -Tag $Tag
    if ($actual -and $actual -ne $SourceCommit) {
        throw "$Tag points to $actual, expected $SourceCommit."
    }
    if ($actual) {
        Write-ReleaseStatus ready "$Tag points to $SourceCommit."
        return
    }
    if (!$Push) {
        Write-ReleaseStatus skipped (
            "Skipping: git -C $Root push $Remote $SourceCommit`:refs/tags/$Tag (requires -Push).")
        return
    }

    $null = Invoke-Git `
        -Root $Root `
        -Arguments @('push', $Remote, "$SourceCommit`:refs/tags/$Tag") `
        -WriteOutput
    if ((Get-RemoteTagSha -Root $Root -Remote $Remote -Tag $Tag) -ne $SourceCommit) {
        throw "$Tag creation could not be verified."
    }
    Write-ReleaseStatus applied "Created $Tag at $SourceCommit."
}

# Tests whether an automation branch is one desired file-only commit on its base.
function Test-AutomationFileBranch(
    [string] $Root,
    [string] $RemoteSha,
    [string] $BaseSha,
    [System.Collections.IDictionary] $Files
) {
    if (!$RemoteSha) {
        return $false
    }
    $null = Invoke-Git -Root $Root -Arguments @('fetch', '--quiet', 'origin', $RemoteSha)
    $parent = Invoke-Git -Root $Root -Arguments @('rev-parse', "$RemoteSha^") -AllowFailure
    if ($parent.ExitCode -ne 0 -or $parent.Output -ne $BaseSha) {
        return $false
    }
    $changed = @(
        (Invoke-Git -Root $Root -Arguments @('diff', '--name-only', "$BaseSha..$RemoteSha")).Output `
            -split "`r?`n" |
            Where-Object { $_ } |
            Sort-Object
    )
    $expected = @($Files.Keys | ForEach-Object { [string] $_ } | Sort-Object)
    if (($changed -join "`n") -ne ($expected -join "`n")) {
        return $false
    }

    foreach ($path in $expected) {
        $temporary = [IO.Path]::GetTempFileName()
        try {
            [IO.File]::WriteAllText($temporary, [string] $Files[$path], [Text.UTF8Encoding]::new($false))
            $desiredBlob = (Invoke-Git -Root $Root -Arguments @('hash-object', $temporary)).Output
        } finally {
            Remove-Item $temporary -Force -ErrorAction SilentlyContinue
        }
        $remoteBlob = Invoke-Git -Root $Root -Arguments @('rev-parse', "$RemoteSha`:$path") -AllowFailure
        if ($remoteBlob.ExitCode -ne 0 -or $remoteBlob.Output -ne $desiredBlob) {
            return $false
        }
    }
    return $true
}

# Creates the automation pull request, or reports the existing one.
function Confirm-AutomationPullRequest(
    [string] $Repository,
    [string] $Branch,
    [string] $BaseBranch,
    [string] $Title,
    [string] $Body,
    [string] $Description,
    [object[]] $Existing
) {
    if ($Existing) {
        Write-ReleaseStatus ready "$Description PR #$($Existing[0].number) is open: $($Existing[0].url)"
        return
    }
    $null = Invoke-GitHub `
        -Arguments @(
            'pr', 'create',
            '--repo', $Repository,
            '--base', $BaseBranch,
            '--head', $Branch,
            '--title', $Title,
            '--body', $Body
        ) `
        -WriteOutput
    Write-ReleaseStatus pushed "Created the $Description PR from $Branch to $BaseBranch."
}

# Creates or reuses a guarded automation branch and pull request for desired file contents.
function Publish-AutomationFilePullRequest(
    [string] $Root,
    [string] $Repository,
    [string] $Branch,
    [string] $BaseBranch,
    [System.Collections.IDictionary] $Files,
    [string] $CommitMessage,
    [string] $Title,
    [string] $Body,
    [string] $Description,
    [switch] $Apply,
    [switch] $Push
) {
    if ($Apply -and $Push) {
        throw 'Apply and Push are mutually exclusive.'
    }
    $resolvedFiles = [ordered] @{}
    foreach ($path in $Files.Keys) {
        $fullPath = [IO.Path]::GetFullPath((Join-Path $Root ([string] $path)))
        $relative = [IO.Path]::GetRelativePath($Root, $fullPath)
        if ($relative -eq '..' -or $relative.StartsWith("../") -or $relative.StartsWith("..\")) {
            throw "Automation file '$path' is outside the repository."
        }
        $resolvedFiles[$path] = $fullPath
    }
    $filesAreCurrent = @(
        $resolvedFiles.Keys |
            Where-Object {
                !(Test-Path -LiteralPath $resolvedFiles[$_] -PathType Leaf) -or
                [IO.File]::ReadAllText($resolvedFiles[$_]) -cne [string] $Files[$_]
            }
    ).Count -eq 0
    if ($filesAreCurrent) {
        Write-ReleaseStatus ready "$Description files are current."
        return
    }
    if (!$Apply -and !$Push) {
        Write-ReleaseStatus plan "Create or update the $Description PR from $Branch to $BaseBranch."
        return
    }
    Assert-GitWorktreeClean -Root $Root -IgnoreSubmodules
    if ($Apply) {
        foreach ($path in $resolvedFiles.Keys) {
            [IO.File]::WriteAllText(
                $resolvedFiles[$path],
                [string] $Files[$path],
                [Text.UTF8Encoding]::new($false))
        }
        Write-ReleaseStatus applied "Updated $Description files locally."
        return
    }

    $baseSha = Get-ResolvedGitCommit -Root $Root -Reference $BaseBranch
    $headSha = (Invoke-Git -Root $Root -Arguments @('rev-parse', 'HEAD')).Output
    if ($headSha -ne $baseSha) {
        throw "Automation must run at current origin/$BaseBranch $baseSha, not $headSha."
    }

    $pullRequests = @(
        Invoke-GitHubJsonWithRetry -Arguments @(
            'pr', 'list',
            '--repo', $Repository,
            '--head', $Branch,
            '--base', $BaseBranch,
            '--state', 'open',
            '--json', 'number,url'
        )
    )
    if ($pullRequests.Count -gt 1) {
        throw "Multiple open pull requests use $Branch."
    }
    $remoteSha = Get-RemoteBranchSha -Root $Root -Remote origin -Branch $Branch
    if (Test-AutomationFileBranch -Root $Root -RemoteSha $remoteSha -BaseSha $baseSha -Files $Files) {
        Confirm-AutomationPullRequest `
            -Repository $Repository `
            -Branch $Branch `
            -BaseBranch $BaseBranch `
            -Title $Title `
            -Body $Body `
            -Description $Description `
            -Existing $pullRequests
        return
    }

    $null = Invoke-Git -Root $Root -Arguments @('switch', '-C', $Branch, $baseSha) -WriteOutput
    foreach ($path in $resolvedFiles.Keys) {
        [IO.File]::WriteAllText($resolvedFiles[$path], [string] $Files[$path], [Text.UTF8Encoding]::new($false))
    }
    $null = Invoke-Git -Root $Root -Arguments (@('add', '--') + @($Files.Keys))
    $null = Invoke-Git `
        -Root $Root `
        -Arguments @(
            '-c', 'user.name=github-actions[bot]',
            '-c', 'user.email=41898282+github-actions[bot]@users.noreply.github.com',
            'commit', '-m', $CommitMessage
        ) `
        -WriteOutput
    $localSha = (Invoke-Git -Root $Root -Arguments @('rev-parse', 'HEAD')).Output
    Enable-GitHubGitAuthentication
    if ($remoteSha) {
        $null = Invoke-Git `
            -Root $Root `
            -Arguments @(
                'push', 'origin',
                "HEAD:refs/heads/$Branch",
                "--force-with-lease=refs/heads/$Branch`:$remoteSha"
            ) `
            -WriteOutput
    } else {
        $null = Invoke-Git `
            -Root $Root `
            -Arguments @('push', 'origin', "HEAD:refs/heads/$Branch") `
            -WriteOutput
    }
    if ((Get-RemoteBranchSha -Root $Root -Remote origin -Branch $Branch) -ne $localSha) {
        throw "$Description automation branch push could not be verified."
    }
    Write-ReleaseStatus pushed "$Branch is at $localSha."

    Confirm-AutomationPullRequest `
        -Repository $Repository `
        -Branch $Branch `
        -BaseBranch $BaseBranch `
        -Title $Title `
        -Body $Body `
        -Description $Description `
        -Existing $pullRequests
}

Export-ModuleMember -Function @(
    'Write-ReleaseStatus',
    'Get-RepositoryReleaseVersion',
    'Push-ReleaseBranch',
    'ConvertTo-ReleaseMilestone',
    'Get-ShippedTag',
    'Get-RemoteReleaseTags',
    'Set-GitHubItemMilestone',
    'Get-ReleaseIdentity',
    'Resolve-NuGetPackageVersion',
    'Get-NuGetPackageSource',
    'Invoke-GitHubMutation',
    'Push-ReleaseTag',
    'Test-AutomationFileBranch',
    'Publish-AutomationFilePullRequest'
) -Variable @(
    'ReleaseRepository',
    'ReleaseSkiaRemote',
    'ReleaseSkiaPath',
    'ReleaseVariablesPath',
    'ReleaseVersionsPath'
)
