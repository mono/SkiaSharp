<#
.SYNOPSIS
    Extract repro platform and action from a triage JSON file.
.DESCRIPTION
    Reads the triage output JSON and extracts suggestedReproPlatform and
    suggestedAction. Outputs GitHub Actions outputs for downstream jobs.
.PARAMETER TriageFile
    Path to the triage JSON file.
.PARAMETER GitHubOutput
    Path to $GITHUB_OUTPUT file. If not provided, writes to console only.
#>
param(
    [Parameter(Mandatory)]
    [string]$TriageFile,

    [string]$GitHubOutput = $env:GITHUB_OUTPUT
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $TriageFile)) {
    Write-Warning "Triage file not found at $TriageFile - using defaults"
    $platform = 'linux'
    $needsRepro = 'false'
    $action = ''
} else {
    Write-Host "Found triage output at $TriageFile"
    $triage = Get-Content $TriageFile -Raw | ConvertFrom-Json

    # Extract platform (default: linux)
    $platform = $triage.output.actionability.suggestedReproPlatform
    if (-not $platform) { $platform = 'linux' }
    $platform = $platform.ToLower()

    $validPlatforms = @('linux', 'macos', 'windows')
    if ($platform -notin $validPlatforms) {
        Write-Warning "Unrecognized platform '$platform', defaulting to linux"
        $platform = 'linux'
    }

    # Extract action
    $action = $triage.output.actionability.suggestedAction
    if (-not $action) { $action = '' }
    $needsRepro = if ($action -eq 'needs-investigation') { 'true' } else { 'false' }
}

Write-Host "Triage: platform=$platform, needs_repro=$needsRepro, action=$action"

if ($GitHubOutput) {
    "platform=$platform" | Out-File -FilePath $GitHubOutput -Append
    "needs_repro=$needsRepro" | Out-File -FilePath $GitHubOutput -Append
}

# Return as object for local testing
[PSCustomObject]@{
    Platform  = $platform
    NeedsRepro = $needsRepro
    Action    = $action
}
