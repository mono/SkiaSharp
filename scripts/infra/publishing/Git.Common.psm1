$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

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

# Runs Git from an explicit repository root with consistent output and errors.
function Invoke-Git(
    [string] $Root,
    [string[]] $Arguments,
    [switch] $AllowFailure,
    [switch] $WriteOutput
) {
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& git -C $Root @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($WriteOutput -and $output) {
        $output | Out-Host
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

# Resolves the root of the Git repository containing one path.
function Get-GitRepositoryRoot([string] $Path = $PWD.Path) {
    return (Invoke-Git -Root $Path -Arguments @('rev-parse', '--show-toplevel')).Output
}

# Resolves a branch or tag from a remote, peeling annotated tags when present.
function Get-RemoteRefSha([string] $Root, [string] $Remote, [string] $Ref) {
    $output = (Invoke-Git -Root $Root -Arguments @('ls-remote', $Remote, $Ref, "$Ref^{}")).Output
    $lines = @($output -split "`r?`n" | Where-Object { $_ })
    $peeled = $lines | Where-Object { $_ -match '\^\{\}$' } | Select-Object -First 1
    $line = if ($peeled) { $peeled } else { $lines | Select-Object -First 1 }
    if ($line) {
        return ($line -split '\s+')[0]
    }
    return $null
}

# Resolves one remote branch SHA.
function Get-RemoteBranchSha([string] $Root, [string] $Remote, [string] $Branch) {
    return Get-RemoteRefSha -Root $Root -Remote $Remote -Ref "refs/heads/$Branch"
}

# Resolves one remote tag SHA.
function Get-RemoteTagSha([string] $Root, [string] $Remote, [string] $Tag) {
    return Get-RemoteRefSha -Root $Root -Remote $Remote -Ref "refs/tags/$Tag"
}

# Resolves one local branch SHA without changing the current branch.
function Get-LocalBranchSha([string] $Root, [string] $Branch) {
    $output = (Invoke-Git -Root $Root -Arguments @('branch', '--list', $Branch, '--format=%(objectname)')).Output
    if ($output) {
        return $output.Trim()
    }
    return $null
}

# Resolves a remote branch or SHA to one immutable commit.
function Get-ResolvedGitCommit([string] $Root, [string] $Reference, [string] $Remote = 'origin') {
    if ($Reference -match '^[0-9a-fA-F]{40}$') {
        $null = Invoke-Git -Root $Root -Arguments @('fetch', '--quiet', $Remote, $Reference)
        $resolvedRef = $Reference
    } else {
        $branch = $Reference -replace '^(refs/heads/|origin/)'
        $null = Invoke-Git -Root $Root -Arguments @('fetch', '--quiet', $Remote, "refs/heads/$branch")
        $resolvedRef = 'FETCH_HEAD'
    }
    return (Invoke-Git -Root $Root -Arguments @('rev-parse', '--verify', "$resolvedRef`^{commit}")).Output
}

# Reads one text file from a commit using consistent newline normalization.
function Get-GitFileText([string] $Root, [string] $Commit, [string] $Path) {
    return (Invoke-Git -Root $Root -Arguments @('show', "${Commit}:$Path")).Output
}

# Resolves one tree entry, including a submodule gitlink, from a commit.
function Get-GitTreeEntrySha([string] $Root, [string] $Commit, [string] $Path) {
    return (Invoke-Git -Root $Root -Arguments @('rev-parse', "${Commit}:$Path")).Output
}

Export-ModuleMember -Function @(
    'Format-Command',
    'Invoke-Git',
    'Get-GitRepositoryRoot',
    'Get-RemoteRefSha',
    'Get-RemoteBranchSha',
    'Get-RemoteTagSha',
    'Get-LocalBranchSha',
    'Get-ResolvedGitCommit',
    'Get-GitFileText',
    'Get-GitTreeEntrySha'
)
