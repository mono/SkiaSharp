#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Reconciles merged pull requests and linked issues to shipped release milestones.

.PARAMETER Version
    The released numeric SkiaSharp version, such as 4.153.0 or 4.153.0.1.

.PARAMETER Repository
    The GitHub repository whose release assignments are maintained.

.PARAMETER Push
    Performs GitHub milestone assignments. Without this switch, the script is
    read-only and reports exact skipped mutations.
#>

param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:\.\d+)?$')]
    [string] $Version,

    [ValidatePattern('^[^/]+/[^/]+$')]
    [string] $Repository = 'mono/SkiaSharp',

    [switch] $Push
)

# 0. Initialize shared helpers, execution mode, and repository state.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Import-Module (Join-Path $PSScriptRoot 'Git.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'Publishing.Common.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'ReleaseMilestones.Common.psm1') -Force
$writeRemote = $Push
$mode = if ($writeRemote) { 'push' } else { 'dry run' }
$root = Get-GitRepositoryRoot

# 1. Reconcile shipped commits, pull requests, and linked issues.
Write-ReleaseStatus start "Release assignment reconciliation for $Version ($mode)."
Invoke-ReleaseAssignmentReconciliation `
    -Root $root `
    -Version $Version `
    -Repository $Repository `
    -Push:$Push
Write-ReleaseStatus complete "Release assignment reconciliation completed ($mode)."
