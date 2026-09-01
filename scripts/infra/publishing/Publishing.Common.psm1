$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# Shared repository paths and release contracts.
New-Variable -Scope Script -Option ReadOnly -Name ReleaseRepository -Value 'mono/SkiaSharp'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSkiaRemote -Value 'https://github.com/mono/skia.git'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSkiaPath -Value 'externals/skia'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseVariablesPath -Value 'scripts/azure-templates-variables.yml'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseVersionsPath -Value 'scripts/VERSIONS.txt'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSummaryStartMarker -Value '<!-- SKIASHARP:RELEASE-SUMMARY:START -->'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseSummaryEndMarker -Value '<!-- SKIASHARP:RELEASE-SUMMARY:END -->'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseGeneratedStartMarker `
    -Value '<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->'
New-Variable -Scope Script -Option ReadOnly -Name ReleaseGeneratedEndMarker `
    -Value '<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->'

# Writes one concise state transition to the console.
function Write-ReleaseStatus([string] $State, [string] $Message) {
    Write-Host "[$State] $Message"
}

# Quotes one command argument for readable dry-run output.
function Format-CommandArgument([string] $Value) {
    if ($Value -match '^[A-Za-z0-9_./:@=,+-]+$') {
        return $Value
    }
    return "'$($Value.Replace("'", "''"))'"
}

# Formats an executable and its arguments as a copyable PowerShell command.
function Format-Command([string] $Executable, [string[]] $Arguments) {
    return (@($Executable) + @($Arguments) | ForEach-Object { Format-CommandArgument $_ }) -join ' '
}

# Runs Git and returns output plus exit status while preserving failures.
function Invoke-GitCommand([string] $Root, [string[]] $Arguments, [switch] $AllowFailure) {
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& git -C $Root @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -ne 0 -and !$AllowFailure) {
        $detail = ($output -join "`n").Trim()
        throw "Command failed ($exitCode): $(Format-Command 'git' (@('-C', $Root) + $Arguments))`n$detail"
    }
    return [pscustomobject] @{
        ExitCode = $exitCode
        Output = ($output -join "`n").Trim()
    }
}

# Resolves the root of the current Git repository.
function Get-GitRepositoryRoot {
    return (Invoke-GitCommand -Root $PWD.Path -Arguments @('rev-parse', '--show-toplevel')).Output
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

# Resolves a branch or tag from a remote, peeling annotated tags when present.
function Get-RemoteRefSha([string] $Remote, [string] $Ref) {
    $lines = @(git ls-remote $Remote $Ref "$Ref^{}")
    $peeled = $lines | Where-Object { $_ -match '\^\{\}$' } | Select-Object -First 1
    $line = if ($peeled) {
        $peeled
    } else {
        $lines | Select-Object -First 1
    }
    if ($line) {
        return ($line -split '\s+')[0]
    }
    return $null
}

# Resolves one remote branch SHA.
function Get-RemoteBranchSha([string] $Remote, [string] $Branch) {
    return Get-RemoteRefSha -Remote $Remote -Ref "refs/heads/$Branch"
}

# Resolves one remote tag SHA.
function Get-RemoteTagSha([string] $Remote, [string] $Tag) {
    return Get-RemoteRefSha -Remote $Remote -Ref "refs/tags/$Tag"
}

# Resolves a local branch SHA without changing the current branch.
function Get-LocalBranchSha([string] $Repository, [string] $Branch) {
    $sha = git -C $Repository branch --list $Branch --format='%(objectname)'
    if ($sha) {
        return $sha.Trim()
    }
    return $null
}

# Resolves a branch or SHA to one immutable commit.
function Get-ResolvedGitCommit([string] $Reference, [string] $Remote = 'origin') {
    if ($Reference -match '^[0-9a-fA-F]{40}$') {
        git fetch --quiet $Remote $Reference
        $resolvedRef = $Reference
    } else {
        $branch = $Reference -replace '^(refs/heads/|origin/)'
        git fetch --quiet $Remote "refs/heads/$branch"
        $resolvedRef = 'FETCH_HEAD'
    }
    return (git rev-parse --verify "$resolvedRef`^{commit}").Trim()
}

# Reads one text file from a commit using consistent newline normalization.
function Get-GitFileText([string] $Commit, [string] $Path) {
    return (git show "${Commit}:$Path") -join "`n"
}

# Resolves one tree entry, including a submodule gitlink, from a commit.
function Get-GitTreeEntrySha([string] $Commit, [string] $Path) {
    return (git rev-parse "${Commit}:$Path").Trim()
}

# Pushes one explicit branch state, or reports the skipped command.
function Push-ReleaseBranch(
    [string] $Repository,
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
        Write-ReleaseStatus skipped "Skipping: git -C $Repository push $Remote $Branch`:refs/heads/$Branch (requires -Push)."
        return
    }
    if (!$LocalSha) {
        throw "$Description $Branch has no local commit to push."
    }

    git -C $Repository push $Remote "$Branch`:refs/heads/$Branch" | Out-Host
    $actual = Get-RemoteBranchSha -Remote $Remote -Branch $Branch
    if ($actual -ne $LocalSha) {
        throw "$Description $Branch push verification failed."
    }
    Write-ReleaseStatus pushed "$Description $Branch is at $actual."
}

# Parses an exact public package version into branch and tag identities.
function Get-ReleaseIdentity([string] $PublicVersion) {
    # Prerelease packages include a build revision that belongs in the tag but not the branch.
    $prerelease = [regex]::Match(
        $PublicVersion,
        '^(?<numeric>\d+\.\d+\.\d+(?:\.\d+)?)-(?<channel>preview|rc)\.(?<iteration>[1-9]\d*)\.(?<build>\d+(?:\.\d+)?)$')
    if ($prerelease.Success) {
        $numeric = $prerelease.Groups['numeric'].Value
        $channel = $prerelease.Groups['channel'].Value
        $iteration = $prerelease.Groups['iteration'].Value
        $identity = (
            "$numeric-$channel." +
            $iteration)
        $channelTitle = if ($channel -eq 'rc') { 'RC' } else { 'Preview' }
        return [pscustomobject] @{
            Identity = $identity
            Numeric = $numeric
            Branch = "release/$identity"
            Tag = "v$PublicVersion"
            Title = "Version $numeric ($channelTitle $iteration)"
            IsPrerelease = $true
        }
    }

    # Stable packages use the same numeric identity for the package, branch, and tag.
    if ($PublicVersion -match '^\d+\.\d+\.\d+(?:\.\d+)?$') {
        return [pscustomobject] @{
            Identity = $PublicVersion
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

# Runs gh and parses its JSON response.
function Invoke-GitHubJson([string[]] $Arguments) {
    $errorPath = [System.IO.Path]::GetTempFileName()
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& gh @Arguments 2> $errorPath)
        $exitCode = $LASTEXITCODE
        $errorText = Get-Content $errorPath -Raw
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
        Remove-Item $errorPath -Force -ErrorAction SilentlyContinue
    }
    if ($exitCode -ne 0) {
        throw "gh failed ($exitCode): $($errorText.Trim())"
    }
    return (($output -join "`n") | ConvertFrom-Json)
}

# Runs a GitHub JSON request with retries for transient gateway errors.
function Invoke-GitHubJsonWithRetry([string[]] $Arguments) {
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        try {
            return Invoke-GitHubJson -Arguments $Arguments
        } catch {
            $transient = $_.Exception.Message -match 'HTTP (502|503|504)'
            if (!$transient -or $attempt -eq 3) {
                throw
            }
            Start-Sleep -Seconds ($attempt * 2)
        }
    }
    throw 'GitHub request retry loop exited unexpectedly.'
}

# Flattens pages returned by gh api --paginate --slurp.
function Expand-GitHubPages([object] $Pages) {
    $items = [System.Collections.Generic.List[object]]::new()
    foreach ($page in @($Pages)) {
        if ($null -eq $page) {
            continue
        }
        if ($page.PSObject.Properties['number']) {
            $items.Add($page)
            continue
        }
        foreach ($item in @($page)) {
            if ($null -ne $item) {
                $items.Add($item)
            }
        }
    }
    return $items.ToArray()
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

# Reads a GitHub Release while treating a missing tag as normal state.
function Get-GitHubRelease([string] $Repository, [string] $Tag) {
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = & gh release view $Tag `
            --repo $Repository `
            --json tagName,name,isDraft,isPrerelease,targetCommitish,body,url `
            2>&1
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -eq 0) {
        return ($output -join "`n") | ConvertFrom-Json
    }
    if (($output -join "`n") -match 'release not found|HTTP 404') {
        return $null
    }
    throw "Unable to read GitHub Release $Tag`: $($output -join "`n")"
}

# Creates one immutable remote tag, or reports the skipped command.
function Push-ReleaseTag([string] $Remote, [string] $Tag, [string] $SourceCommit, [switch] $Push) {
    $actual = Get-RemoteTagSha -Remote $Remote -Tag $Tag
    if ($actual -and $actual -ne $SourceCommit) {
        throw "$Tag points to $actual, expected $SourceCommit."
    }
    if ($actual) {
        Write-ReleaseStatus ready "$Tag points to $SourceCommit."
        return
    }
    if (!$Push) {
        Write-ReleaseStatus skipped "Skipping: git push $Remote $SourceCommit`:refs/tags/$Tag (requires -Push)."
        return
    }

    git push $Remote "$SourceCommit`:refs/tags/$Tag" | Out-Host
    if ((Get-RemoteTagSha -Remote $Remote -Tag $Tag) -ne $SourceCommit) {
        throw "$Tag creation could not be verified."
    }
    Write-ReleaseStatus applied "Created $Tag at $SourceCommit."
}

# Configures Git to use the active gh authentication for github.com.
function Enable-GitHubGitAuthentication {
    gh auth setup-git --hostname github.com
}

Export-ModuleMember -Function @(
    'Write-ReleaseStatus',
    'Invoke-GitCommand',
    'Get-GitRepositoryRoot',
    'Get-RepositoryReleaseVersion',
    'Get-RemoteBranchSha',
    'Get-LocalBranchSha',
    'Get-ResolvedGitCommit',
    'Get-GitFileText',
    'Get-GitTreeEntrySha',
    'Push-ReleaseBranch',
    'Get-ReleaseIdentity',
    'Resolve-NuGetPackageVersion',
    'Get-NuGetPackageSource',
    'Invoke-GitHubJson',
    'Invoke-GitHubJsonWithRetry',
    'Expand-GitHubPages',
    'Invoke-GitHubMutation',
    'Get-GitHubRelease',
    'Push-ReleaseTag',
    'Enable-GitHubGitAuthentication'
) -Variable @(
    'ReleaseRepository',
    'ReleaseSkiaRemote',
    'ReleaseSkiaPath',
    'ReleaseVariablesPath',
    'ReleaseVersionsPath',
    'ReleaseSummaryStartMarker',
    'ReleaseSummaryEndMarker',
    'ReleaseGeneratedStartMarker',
    'ReleaseGeneratedEndMarker'
)
