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
    $feeds = @(
        $asset.locations |
            Where-Object {
                [string] $_ -match '/_packaging/[^/]+/nuget/v3/index\.json$'
            }
    )
    if ($feeds.Count -ne 1) {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "BAR build $($build.id) has $($feeds.Count) NuGet feed locations."
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
            Feed = [string] $feeds[0]
            Channels = $channels
            IsReleased = [bool] $build.released
        })
}

function Get-MaestroFlatContainer([string] $Feed) {
    try {
        $index = Invoke-RestMethod -Uri $Feed
    } catch {
        return New-MaestroCheckResult `
            -State 'unavailable' `
            -Message "Cannot read BAR package feed $Feed`: $($_.Exception.Message)"
    }
    $resources = @(
        $index.resources |
            Where-Object {
                [string] $_.'@type' -like 'PackageBaseAddress*' -and $_.'@id'
            } |
            ForEach-Object { [string] $_.'@id' }
    )
    if ($resources.Count -ne 1) {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "BAR package feed has $($resources.Count) flat-container resources."
    }
    return New-MaestroCheckResult `
        -State 'complete' `
        -Value ($resources[0].TrimEnd('/') + '/')
}

function Test-MaestroNotFound([System.Management.Automation.ErrorRecord] $ErrorRecord) {
    $response = $ErrorRecord.Exception.Response
    return (
        ($response -and [int] $response.StatusCode -eq 404) -or
        $ErrorRecord.Exception.Message -match '404|not found')
}

function Get-MaestroPackageReceipt([string] $PackageId, [string] $Version, [string] $FlatContainer) {
    $lowerId = $PackageId.ToLowerInvariant()
    $lowerVersion = $Version.ToLowerInvariant()
    $escapedId = [Uri]::EscapeDataString($lowerId)
    $escapedVersion = [Uri]::EscapeDataString($lowerVersion)
    $packageName = [Uri]::EscapeDataString("$lowerId.$lowerVersion.nupkg")
    $uri = "$FlatContainer$escapedId/$escapedVersion/$packageName"
    $temporary = [IO.Path]::GetTempFileName()
    try {
        try {
            Invoke-WebRequest -Uri $uri -OutFile $temporary -ErrorAction Stop
        } catch {
            if (Test-MaestroNotFound $_) {
                return New-MaestroCheckResult `
                    -State 'pending' `
                    -Message "BAR feed package $PackageId $Version is unavailable."
            }
            return New-MaestroCheckResult `
                -State 'unavailable' `
                -Message "Cannot read BAR feed package $PackageId ${Version}: $($_.Exception.Message)"
        }

        try {
            $archive = [IO.Compression.ZipFile]::OpenRead($temporary)
            try {
                $nuspecs = @(
                    $archive.Entries |
                        Where-Object {
                            $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase)
                        }
                )
                if ($nuspecs.Count -ne 1) {
                    return New-MaestroCheckResult `
                        -State 'pending' `
                        -Message "$PackageId $Version contains $($nuspecs.Count) nuspecs."
                }
                $reader = [IO.StreamReader]::new($nuspecs[0].Open())
                try {
                    [xml] $document = $reader.ReadToEnd()
                } finally {
                    $reader.Dispose()
                }
            } finally {
                $archive.Dispose()
            }
        } catch {
            return New-MaestroCheckResult `
                -State 'pending' `
                -Message "BAR feed package $PackageId $Version is malformed: $($_.Exception.Message)"
        }

        $metadata = $document.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
        if (!$metadata) {
            return New-MaestroCheckResult `
                -State 'pending' `
                -Message "$PackageId $Version has no nuspec metadata."
        }
        $idNode = $metadata.SelectSingleNode("*[local-name()='id']")
        $versionNode = $metadata.SelectSingleNode("*[local-name()='version']")
        $id = if ($idNode) { [string] $idNode.InnerText } else { '' }
        $packageVersion = if ($versionNode) { [string] $versionNode.InnerText } else { '' }
        $repository = $metadata.SelectSingleNode("*[local-name()='repository']")
        $branch = if ($repository) { [string] $repository.GetAttribute('branch') } else { '' }
        $commit = if ($repository) { [string] $repository.GetAttribute('commit') } else { '' }
        if ($id -ne $PackageId -or $packageVersion -ne $Version -or !$branch -or $commit -notmatch '^[0-9a-f]{40}$') {
            return New-MaestroCheckResult `
                -State 'pending' `
                -Message "$PackageId $Version has inconsistent source metadata."
        }
        $harfBuzzVersions = @(
            $metadata.SelectNodes(".//*[local-name()='dependency']") |
                Where-Object { $_.GetAttribute('id') -eq 'HarfBuzzSharp' -and $_.GetAttribute('version') } |
                ForEach-Object { [string] $_.GetAttribute('version') } |
                Sort-Object -Unique
        )
        return New-MaestroCheckResult `
            -State 'complete' `
            -Value ([pscustomobject] @{
                Id = $id
                Version = $packageVersion
                Branch = $branch
                Commit = $commit
                HarfBuzzVersions = $harfBuzzVersions
            })
    } finally {
        Remove-Item -LiteralPath $temporary -Force -ErrorAction SilentlyContinue
    }
}

function Test-MaestroPackageSources([pscustomobject] $Build, [object[]] $Packages) {
    $branches = @($Packages | ForEach-Object Branch | Sort-Object -Unique)
    $commits = @($Packages | ForEach-Object Commit | Sort-Object -Unique)
    if ($branches.Count -ne 1 -or $commits.Count -ne 1) {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message 'BAR package source metadata does not match.'
    }
    $skia = $Packages | Where-Object Id -eq 'SkiaSharp' | Select-Object -First 1
    if (!$skia -or
        ($skia.Branch -replace '^refs/heads/', '') -ne $Build.Branch -or
        $skia.Commit -ne $Build.Commit) {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message 'BAR build and package source metadata do not match.'
    }
    return New-MaestroCheckResult -State 'complete'
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
    $packageVersion = $build.PackageVersion
    $flatResult = Get-MaestroFlatContainer -Feed $build.Feed
    if ($flatResult.State -ne 'complete') {
        return $flatResult
    }

    $skia = Get-MaestroPackageReceipt -PackageId 'SkiaSharp' -Version $packageVersion -FlatContainer $flatResult.Value
    if ($skia.State -ne 'complete') {
        return $skia
    }
    $bridge = Get-MaestroPackageReceipt -PackageId 'SkiaSharp.HarfBuzz' -Version $packageVersion -FlatContainer $flatResult.Value
    if ($bridge.State -ne 'complete') {
        return $bridge
    }
    $harfBuzzVersions = @($bridge.Value.HarfBuzzVersions)
    if ($harfBuzzVersions.Count -ne 1 -or $harfBuzzVersions[0] -match '[\s\[\]\(\),*]') {
        return New-MaestroCheckResult `
            -State 'pending' `
            -Message "SkiaSharp.HarfBuzz $packageVersion does not pin one concrete HarfBuzzSharp dependency."
    }
    $harfBuzz = Get-MaestroPackageReceipt `
        -PackageId 'HarfBuzzSharp' `
        -Version $harfBuzzVersions[0] `
        -FlatContainer $flatResult.Value
    if ($harfBuzz.State -ne 'complete') {
        return $harfBuzz
    }

    $packages = @($skia.Value, $bridge.Value, $harfBuzz.Value)
    $sourceResult = Test-MaestroPackageSources -Build $build -Packages $packages
    if ($sourceResult.State -ne 'complete') {
        return $sourceResult
    }
    return New-MaestroCheckResult `
        -State 'complete' `
        -Value ([pscustomobject] @{
            BarBuildId = $build.Id
            BuildNumber = $build.BuildNumber
            BuildLink = $build.BuildLink
            SourceBranch = $build.Branch
            SourceCommit = $build.Commit
            PackageFeed = $build.Feed
            SkiaSharpVersion = $packageVersion
            HarfBuzzSharpVersion = $harfBuzzVersions[0]
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
    'Test-MaestroPackageSources',
    'Get-MaestroReleaseReceipt',
    'Get-MaestroReleaseReceiptForBranch'
)
