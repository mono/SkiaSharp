$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# Runs gh with consistent output and errors.
function Invoke-GitHub(
    [string[]] $Arguments,
    [switch] $AllowFailure,
    [switch] $WriteOutput
) {
    $errorPath = [System.IO.Path]::GetTempFileName()
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $LASTEXITCODE = 0
        $output = @(& gh @Arguments 2> $errorPath)
        $exitCode = $LASTEXITCODE
        $errorText = [string] (Get-Content $errorPath -Raw)
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
        Remove-Item $errorPath -Force -ErrorAction SilentlyContinue
    }
    if ($WriteOutput -and $output) {
        $output | Out-Host
    }
    if ($exitCode -ne 0 -and !$AllowFailure) {
        throw "gh failed ($exitCode): $($errorText.Trim())"
    }
    return [pscustomobject] @{
        ExitCode = $exitCode
        Output = ($output -join "`n").Trim()
        Error = ([string] $errorText).Trim()
    }
}

# Runs gh and parses its JSON response.
function Invoke-GitHubJson([string[]] $Arguments) {
    $result = Invoke-GitHub -Arguments $Arguments
    return ($result.Output | ConvertFrom-Json)
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

# Reads a GitHub Release while treating a missing tag as normal state.
function Get-GitHubRelease([string] $Repository, [string] $Tag) {
    $result = Invoke-GitHub -Arguments @(
        'release', 'view', $Tag,
        '--repo', $Repository,
        '--json', 'tagName,name,isDraft,isPrerelease,targetCommitish,body,url'
    ) -AllowFailure
    if ($result.ExitCode -eq 0) {
        return $result.Output | ConvertFrom-Json
    }
    $detail = "$($result.Output)`n$($result.Error)".Trim()
    if ($detail -match 'release not found|HTTP 404') {
        return $null
    }
    throw "Unable to read GitHub Release $Tag`: $detail"
}

# Configures Git to use the active gh authentication for github.com.
function Enable-GitHubGitAuthentication {
    $null = Invoke-GitHub -Arguments @('auth', 'setup-git', '--hostname', 'github.com') -WriteOutput
}

Export-ModuleMember -Function @(
    'Invoke-GitHub',
    'Invoke-GitHubJson',
    'Invoke-GitHubJsonWithRetry',
    'Expand-GitHubPages',
    'Get-GitHubRelease',
    'Enable-GitHubGitAuthentication'
)
