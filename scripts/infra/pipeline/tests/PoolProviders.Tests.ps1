$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))

function Read-RepositoryFile {
    param(
        [Parameter(Mandatory)]
        [string] $RelativePath
    )

    return [IO.File]::ReadAllText((Join-Path $repositoryRoot $RelativePath))
}

function Assert-Contains {
    param(
        [Parameter(Mandatory)]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Expected,

        [Parameter(Mandatory)]
        [string] $Description
    )

    if (-not $Content.Contains($Expected, [StringComparison]::Ordinal)) {
        throw "Expected $Description to contain '$Expected'."
    }
}

function Assert-NotContains {
    param(
        [Parameter(Mandatory)]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Unexpected,

        [Parameter(Mandatory)]
        [string] $Description
    )

    if ($Content.Contains($Unexpected, [StringComparison]::Ordinal)) {
        throw "Expected $Description not to contain '$Unexpected'."
    }
}

function Assert-ParameterPool {
    param(
        [Parameter(Mandatory)]
        [string] $Content,

        [Parameter(Mandatory)]
        [string] $Parameter,

        [Parameter(Mandatory)]
        [string] $Pool
    )

    $escapedParameter = [Regex]::Escape($Parameter)
    $pattern = "(?ms)^  - name: $escapedParameter\r?\n.*?(?=^  - name: |^variables:)"
    $match = [Regex]::Match($Content, $pattern)
    if (-not $match.Success) {
        throw "Could not find the '$Parameter' parameter."
    }

    Assert-Contains $match.Value "name: $Pool" "the '$Parameter' parameter"
}

$internalPipeline = Read-RepositoryFile 'scripts/azure-pipelines-package.yml'
$publicPipeline = Read-RepositoryFile 'scripts/azure-pipelines-complete.yml'
$internalProvider = Read-RepositoryFile 'eng/common/templates-official/variables/pool-providers.yml'
$publicProvider = Read-RepositoryFile 'eng/common/templates/variables/pool-providers.yml'

Assert-Contains $internalPipeline `
    '/eng/common/templates-official/variables/pool-providers.yml@self' `
    'the internal pipeline'
Assert-Contains $publicPipeline `
    '/eng/common/templates/variables/pool-providers.yml@self' `
    'the public pipeline'

$dynamicAgents = @(
    'buildAgentHost',
    'buildAgentWindows',
    'buildAgentWindowsNative',
    'buildAgentLinux',
    'buildAgentLinuxNative'
)

foreach ($agent in $dynamicAgents) {
    Assert-ParameterPool $internalPipeline $agent '$(DncEngInternalBuildPool)'
    Assert-ParameterPool $publicPipeline $agent '$(DncEngPublicBuildPool)'
}

Assert-ParameterPool $internalPipeline 'buildAgentMac' 'Azure Pipelines'
Assert-ParameterPool $internalPipeline 'buildAgentMacNative' 'Azure Pipelines'
Assert-ParameterPool $publicPipeline 'buildAgentMac' 'Azure Pipelines'
Assert-ParameterPool $publicPipeline 'buildAgentMacNative' 'Azure Pipelines'
Assert-ParameterPool $publicPipeline 'buildAgentAndroidTests' 'Azure Pipelines'

Assert-NotContains $internalPipeline 'name: NetCore1ESPool-Internal' 'the internal pipeline'
Assert-NotContains $publicPipeline 'name: NetCore-Public' 'the public pipeline'

foreach ($branchVariable in @('System.PullRequest.TargetBranch', 'Build.SourceBranch')) {
    Assert-Contains $internalProvider $branchVariable 'the internal Arcade pool provider'
    Assert-Contains $publicProvider $branchVariable 'the public Arcade pool provider'
}

foreach ($pool in @('NetCore1ESPool-Internal', 'NetCore1ESPool-Svc-Internal')) {
    Assert-Contains $internalProvider $pool 'the internal Arcade pool provider'
}

foreach ($pool in @('NetCore-Public', 'NetCore-Svc-Public')) {
    Assert-Contains $publicProvider $pool 'the public Arcade pool provider'
}

Write-Host 'Branch-aware pool provider contracts passed.'
