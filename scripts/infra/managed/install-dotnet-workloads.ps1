param(
    [Parameter(Mandatory)] [string] $SdkVersion,
    [Parameter(Mandatory)] [string] $WorkloadSetVersion,
    [Parameter(Mandatory)] [string] $Workloads,
    [string] $TizenManifestBand = '',
    [string] $TizenManifestVersion = ''
)

$ErrorActionPreference = 'Stop'
$config = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../nuget.config'))
$ids = @($Workloads -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -Unique)
$tizen = $ids -contains 'tizen'
$ids = @($ids | Where-Object { $_ -ne 'tizen' })
if (-not $ids.Count) { throw 'Specify at least one Microsoft workload for the pinned set.' }
if ($tizen -and (-not $TizenManifestBand -or -not $TizenManifestVersion)) {
    throw 'The tizen workload requires an exact Samsung manifest band and version.'
}

$context = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-workloads-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $context | Out-Null
try {
    @{ sdk = @{ version = $SdkVersion; rollForward = 'disable'; allowPrerelease = $true } } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $context 'global.json')
    Push-Location $context
    try {
        $actual = & dotnet --version
        if ($LASTEXITCODE -ne 0 -or $actual.Trim() -ne $SdkVersion) {
            throw "Expected SDK $SdkVersion, got '$actual'."
        }
        if ($tizen) {
            $package = "samsung.net.sdk.tizen.manifest-$TizenManifestBand"
            $url = "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/$package/$TizenManifestVersion/$package.$TizenManifestVersion.nupkg"
            Invoke-WebRequest $url -OutFile 'tizen.nupkg'
            [IO.Compression.ZipFile]::ExtractToDirectory("$context/tizen.nupkg", "$context/tizen")
            $data = "$context/tizen/data"
            if (-not (Test-Path "$data/WorkloadManifest.json")) { throw "Missing Samsung workload manifest in $package." }
            $exe = [IO.FileInfo]::new((Get-Command dotnet -CommandType Application | Select-Object -First 1).Source)
            $resolved = $exe.ResolveLinkTarget($true)
            $root = if ($resolved) { $resolved.Directory.FullName } else { $exe.Directory.FullName }
            Write-Host "Installing Samsung manifest for SDK $SdkVersion under $root."
            $parts = ($SdkVersion -split '-')[0] -split '\.'
            $sdkBand = "$($parts[0]).$($parts[1]).$([math]::Floor([int]$parts[2] / 100) * 100)"
            if ($SdkVersion -match '-((?:preview|rc)\.\d+)') { $sdkBand += "-$($Matches[1])" }
            foreach ($band in @($TizenManifestBand, $sdkBand) | Select-Object -Unique) {
                $target = Join-Path $root "sdk-manifests/$band/samsung.net.sdk.tizen"
                New-Item -ItemType Directory -Path $target -Force | Out-Null
                Get-ChildItem -LiteralPath $data | Copy-Item -Destination $target -Recurse -Force
            }
        }

        & dotnet workload install @ids --version $WorkloadSetVersion --configfile $config --skip-sign-check
        if ($LASTEXITCODE -ne 0) { throw "Microsoft workload installation failed ($LASTEXITCODE)." }
        if ($tizen) {
            & dotnet workload install tizen --configfile $config --skip-sign-check --skip-manifest-update
            if ($LASTEXITCODE -ne 0) { throw "Tizen workload installation failed ($LASTEXITCODE)." }
        }
    } finally {
        Pop-Location
    }
} finally {
    Remove-Item -LiteralPath $context -Recurse -Force
}
