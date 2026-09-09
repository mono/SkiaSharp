$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1')

$script:MaestroUri = 'https://maestro.dot.net'
$script:AssetsByMaxAge = @{}

function New-MaestroCheckResult([string] $State, [string] $Message = '', [object] $Value = $null) {
    return [pscustomobject] @{
        State = $State
        Message = $Message
        Value = $Value
    }
}

function Get-MaestroAssets([int] $MaxAge) {
    if ($script:AssetsByMaxAge.ContainsKey($MaxAge)) {
        return @($script:AssetsByMaxAge[$MaxAge])
    }
    $arguments = @(
        'get-asset',
        '--name', 'SkiaSharp',
        '--max-age', $MaxAge,
        '--bar-uri', $script:MaestroUri,
        '--output-format', 'json'
    )

    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $output = @(& darc @arguments 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -ne 0) {
        $detail = ($output -join "`n").Trim()
        if ($exitCode -eq 42 -or $detail -match 'No assets found') {
            return @()
        }
        throw "Darc query failed ($exitCode): $detail"
    }
    try {
        $assets = @((($output -join "`n") | ConvertFrom-Json))
        $script:AssetsByMaxAge[$MaxAge] = $assets
        return $assets
    } catch {
        throw "Darc returned invalid asset JSON: $($_.Exception.Message)"
    }
}

function Resolve-MaestroReleaseBuild(
    [object[]] $Assets,
    [string] $Version = '',
    [int] $BarId,
    [string] $ExpectedBranch = '',
    [string] $ExpectedCommit = ''
) {
    $candidates = @{}
    $branchAssets = [System.Collections.Generic.List[object]]::new()
    $versionAssets = [System.Collections.Generic.List[object]]::new()
    foreach ($asset in $Assets) {
        $build = $asset.build
        if ($asset.name -ne 'SkiaSharp' -or $null -eq $build.id) {
            continue
        }
        $branch = [string] $build.branch -replace '^refs/heads/', ''
        if ($Version -and $asset.version -eq $Version) {
            $versionAssets.Add($asset)
        }
        if ($ExpectedBranch -and $branch -eq $ExpectedBranch) {
            $branchAssets.Add($asset)
        }
        if (($Version -and $asset.version -ne $Version) -or
            ($ExpectedBranch -and $branch -ne $ExpectedBranch) -or
            ($ExpectedCommit -and $build.commit -ne $ExpectedCommit)) {
            continue
        }
        $candidates[[int] $build.id] = $asset
    }
    $subject = if ($Version) {
        "SkiaSharp $Version"
    } elseif ($ExpectedBranch -and $ExpectedCommit) {
        "SkiaSharp artifacts for $ExpectedBranch at $ExpectedCommit"
    } else {
        'SkiaSharp artifacts'
    }
    if ($BarId) {
        if (!$candidates.ContainsKey($BarId)) {
            return New-MaestroCheckResult `
                -State 'pending' `
                -Message "$subject was not found in BAR build $BarId."
        }
        $asset = $candidates[$BarId]
    } elseif ($candidates.Count -eq 0) {
        $comparison = if ($branchAssets.Count) { @($branchAssets) } else { @($versionAssets) }
        if (($ExpectedBranch -or $ExpectedCommit) -and $comparison.Count) {
            $sources = @(
                $comparison |
                    ForEach-Object {
                        "$([string] $_.build.branch -replace '^refs/heads/', '')@$([string] $_.build.commit)"
                    } |
                    Sort-Object -Unique
            )
            $expectedSource = "$ExpectedBranch@$ExpectedCommit".Trim('@')
            return New-MaestroCheckResult `
                -State 'pending' `
                -Message "Darc has $subject from $($sources -join ', '), not $expectedSource."
        }
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "$subject was not found in Maestro."
    } elseif ($candidates.Count -ne 1) {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "Multiple BAR builds contain ${subject}: $((@($candidates.Keys | Sort-Object)) -join ', '). Select one with -BarId."
    } else {
        $asset = @($candidates.Values)[0]
    }

    $build = $asset.build
    $branch = [string] $build.branch
    $commit = [string] $build.commit
    if (!$branch -or $commit -notmatch '^[0-9a-f]{40}$') {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "BAR build $($build.id) has incomplete source metadata."
    }
    # Maestro's released flag is not the channel receipt; the build's product
    # channel assignment is the durable promotion evidence for this audit.
    $channels = @(
        $build.channels |
            ForEach-Object {
                if ($_ -is [string]) { $_ } else { [string] $_.name }
            } |
            Where-Object { $_ } |
            Sort-Object -Unique
    )
    if ($channels -notcontains '.NET Libraries') {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "BAR build $($build.id) is not assigned to the .NET Libraries channel."
    }
    return New-MaestroCheckResult `
        -State 'complete' `
        -Value ([pscustomobject] @{
            Id = [int] $build.id
            BuildNumber = [string] $build.buildNumber
            BuildLink = [string] $build.buildLink
            PackageVersion = [string] $asset.version
            Branch = $branch -replace '^refs/heads/', ''
            Commit = $commit
            Channels = $channels
            IsReleased = [bool] $build.released
        })
}

function Get-MaestroReleaseReceipt(
    [string] $Version = '',
    [int] $BarId,
    [int] $MaxAge,
    [string] $ExpectedBranch = '',
    [string] $ExpectedCommit = ''
) {
    try {
        $assets = Get-MaestroAssets -MaxAge $MaxAge
    } catch {
        return New-MaestroCheckResult -State 'unavailable' -Message $_.Exception.Message
    }
    $buildResult = Resolve-MaestroReleaseBuild `
        -Assets $assets `
        -Version $Version `
        -BarId $BarId `
        -ExpectedBranch $ExpectedBranch `
        -ExpectedCommit $ExpectedCommit
    if ($buildResult.State -ne 'complete') {
        return $buildResult
    }
    $build = $buildResult.Value
    return New-MaestroCheckResult `
        -State 'complete' `
        -Value ([pscustomobject] @{
            BarBuildId = $build.Id
            BuildNumber = $build.BuildNumber
            BuildLink = $build.BuildLink
            SourceBranch = $build.Branch
            SourceCommit = $build.Commit
            SkiaSharpVersion = $build.PackageVersion
            Channels = $build.Channels
        })
}

function Get-MaestroReleaseReceiptForBranch(
    [string] $Root,
    [string] $Branch,
    [string] $Version = '',
    [int] $BarId = 0,
    [int] $MaxAge
) {
    $commit = Get-RemoteBranchSha -Root $Root -Remote origin -Branch $Branch
    if (!$commit) {
        return New-MaestroCheckResult -State 'pending' -Message "Missing SkiaSharp branch $Branch."
    }
    return Get-MaestroReleaseReceipt `
        -Version $Version `
        -BarId $BarId `
        -MaxAge $MaxAge `
        -ExpectedBranch $Branch `
        -ExpectedCommit $commit
}

Export-ModuleMember -Function @(
    'Get-MaestroAssets',
    'Resolve-MaestroReleaseBuild',
    'Get-MaestroReleaseReceipt',
    'Get-MaestroReleaseReceiptForBranch'
)
