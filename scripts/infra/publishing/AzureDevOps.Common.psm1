$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# Runs the Azure DevOps CLI with consistent output and errors.
function Invoke-AzureDevOpsCli([string[]] $Arguments) {
    if (!(Get-Command az -ErrorAction SilentlyContinue)) {
        throw 'Azure CLI (az) is not installed.'
    }
    $nativePreference = $PSNativeCommandUseErrorActionPreference
    $PSNativeCommandUseErrorActionPreference = $false
    try {
        $LASTEXITCODE = 0
        $output = @(& az @Arguments 2>&1)
        $exitCode = $LASTEXITCODE
    } finally {
        $PSNativeCommandUseErrorActionPreference = $nativePreference
    }
    if ($exitCode -ne 0) {
        throw "az failed ($exitCode): $(($output -join "`n").Trim())"
    }
    return ($output -join "`n").Trim()
}

# Classifies the latest internal package build for one exact branch tip.
function ConvertTo-ReleasePackageBuildState(
    [string] $Branch,
    [string] $Commit,
    [object[]] $Builds
) {
    $exactBuilds = @($Builds |
        Where-Object { [string] $_.sourceVersion -eq $Commit } |
        Sort-Object { [long] $_.id } -Descending)
    if (!$exactBuilds.Count) {
        return [pscustomobject] @{
            Available = $true
            Branch = $Branch
            Commit = $Commit
            State = 'not built'
            Ready = $false
            BuildId = $null
            BuildNumber = ''
            Status = ''
            Result = ''
            QueueTime = $null
            FinishTime = $null
            BarId = $null
            Url = ''
            Message = "No exact-tip skiasharp-package build was found for $Branch@$Commit."
        }
    }

    $build = $exactBuilds[0]
    $barIds = @($build.tags | ForEach-Object {
        if ([string] $_ -match '^BAR ID - (?<id>\d+)$') {
            [int] $Matches.id
        }
    } | Sort-Object -Unique)
    $status = ([string] $build.status).ToLowerInvariant()
    $result = ([string] $build.result).ToLowerInvariant()
    $state = if ($status -ne 'completed') {
        'running'
    } elseif ($result -ne 'succeeded') {
        'failed'
    } elseif ($barIds.Count -ne 1) {
        'incomplete'
    } else {
        'green'
    }
    $message = switch ($state) {
        'running' { "skiasharp-package build $($build.id) is $status." }
        'failed' { "skiasharp-package build $($build.id) completed with result $result." }
        'incomplete' {
            if ($barIds.Count) {
                "skiasharp-package build $($build.id) recorded multiple BAR IDs: $($barIds -join ', ')."
            } else {
                "skiasharp-package build $($build.id) succeeded without a BAR ID."
            }
        }
        default { '' }
    }
    return [pscustomobject] @{
        Available = $true
        Branch = $Branch
        Commit = $Commit
        State = $state
        Ready = $state -eq 'green'
        BuildId = [int] $build.id
        BuildNumber = [string] $build.buildNumber
        Status = [string] $build.status
        Result = [string] $build.result
        QueueTime = $build.queueTime
        FinishTime = $build.finishTime
        BarId = if ($barIds.Count -eq 1) { $barIds[0] } else { $null }
        Url = "https://dev.azure.com/dnceng/internal/_build/results?buildId=$($build.id)"
        Message = $message
    }
}

# Reads the internal skiasharp-package build for one exact branch tip.
function Get-ReleasePackageBuild([string] $Branch, [string] $Commit) {
    try {
        $json = Invoke-AzureDevOpsCli -Arguments @(
            'pipelines', 'build', 'list',
            '--organization', 'https://dev.azure.com/dnceng',
            '--project', 'internal',
            '--definition-ids', '1642',
            '--branch', "refs/heads/$Branch",
            '--top', '20',
            '--only-show-errors',
            '--output', 'json'
        )
        $builds = if ($json) { @($json | ConvertFrom-Json) } else { @() }
        return ConvertTo-ReleasePackageBuildState `
            -Branch $Branch `
            -Commit $Commit `
            -Builds $builds
    } catch {
        return [pscustomobject] @{
            Available = $false
            Branch = $Branch
            Commit = $Commit
            State = 'unavailable'
            Ready = $false
            BuildId = $null
            BuildNumber = ''
            Status = ''
            Result = ''
            QueueTime = $null
            FinishTime = $null
            BarId = $null
            Url = ''
            Message = $_.Exception.Message
        }
    }
}

Export-ModuleMember -Function @(
    'Invoke-AzureDevOpsCli',
    'ConvertTo-ReleasePackageBuildState',
    'Get-ReleasePackageBuild'
)
