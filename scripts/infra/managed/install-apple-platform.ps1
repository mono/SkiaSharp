Param(
    [Parameter(Mandatory)]
    [ValidateSet('iOS', 'tvOS')]
    [string] $Platform,

    [ValidatePattern('^[0-9]+\.[0-9]+(?:\.[0-9]+)?$')]
    [string] $Version
)

$ErrorActionPreference = 'Stop'

if (-not $IsMacOS) {
    throw 'Apple platforms can only be installed on macOS.'
}

$logDirectory = Join-Path $PSScriptRoot '../../../output/logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
Start-Transcript -Path (Join-Path $logDirectory "install-$($Platform.ToLowerInvariant())-platform.log") | Out-Null

try {
    function Test-SelectedRuntime {
        $json = & xcrun simctl list runtimes --json
        if ($LASTEXITCODE -ne 0) {
            throw "Could not inspect installed simulator runtimes (exit code $LASTEXITCODE)."
        }

        $inventory = ($json -join "`n") | ConvertFrom-Json
        if ($null -eq $inventory -or $inventory.runtimes -isnot [Array]) {
            throw 'simctl returned malformed runtime inventory: expected a runtimes array.'
        }

        foreach ($runtime in $inventory.runtimes) {
            if ($runtime.name -isnot [string] -or $runtime.version -isnot [string] -or
                $runtime.isAvailable -isnot [bool]) {
                throw 'simctl returned malformed runtime inventory: expected name, version and boolean isAvailable.'
            }

            if ($runtime.name.StartsWith("$Platform ", [StringComparison]::Ordinal) -and
                $runtime.version -ceq $Version -and
                $runtime.isAvailable -eq $true) {
                return $true
            }
        }

        return $false
    }

    if (-not $Version -or -not (Test-SelectedRuntime)) {
        $downloadArguments = @('-downloadPlatform', $Platform)
        if ($Version) {
            $downloadArguments += @('-buildVersion', $Version)
        }

        & xcodebuild @downloadArguments
        if ($LASTEXITCODE -ne 0) {
            throw "$Platform platform installation failed (exit code $LASTEXITCODE)."
        }

        if ($Version -and -not (Test-SelectedRuntime)) {
            throw "The $Platform $Version runtime is not available after installation. Check this Xcode's runtime catalog."
        }
    }
}
finally {
    Stop-Transcript | Out-Null
}
