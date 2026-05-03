<#
.SYNOPSIS Determine which pipeline stages need to run based on changed files.
.DESCRIPTION
    Compares HEAD against the merge base (or a given base SHA) and checks
    which stage path patterns are matched. Outputs ADO pipeline variables
    for each stage: STAGE_NATIVE, STAGE_MANAGED, STAGE_TESTS, etc.

    On protected branches (main, release/*), all stages always run.

.PARAMETER BaseSha
    The base commit to diff against. Defaults to HEAD~1.
#>
param(
    [string]$BaseSha = ''
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# 1. Load stage config
# ---------------------------------------------------------------------------
$configPath = 'scripts/infra/caching/repo-deps.config.json'
if (-not (Test-Path $configPath)) {
    Write-Host "Config not found — all stages enabled"
    exit 0
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json
$stages = $config.stages.PSObject.Properties

# ---------------------------------------------------------------------------
# 2. Determine base SHA
# ---------------------------------------------------------------------------
if (-not $BaseSha) {
    if ($env:SYSTEM_PULLREQUEST_TARGETBRANCH) {
        $target = $env:SYSTEM_PULLREQUEST_TARGETBRANCH -replace '^refs/heads/', 'origin/'
        $BaseSha = (git merge-base $target HEAD 2>$null)
        if (-not $BaseSha) { $BaseSha = 'HEAD~1' }
    } else {
        $BaseSha = 'HEAD~1'
    }
}

Write-Host "Base SHA: $BaseSha"

# ---------------------------------------------------------------------------
# 3. Protected branches always run everything
# ---------------------------------------------------------------------------
$branch = $env:BUILD_SOURCEBRANCH ?? ''
$isProtected = ($branch -eq 'refs/heads/main') -or
               ($branch -like 'refs/heads/release/*') -or
               ($branch -like 'refs/heads/develop*')

if ($isProtected) {
    Write-Host "Protected branch '$branch' — all stages enabled"
    foreach ($stage in $stages) {
        $varName = "STAGE_$($stage.Name.ToUpper())"
        Write-Host "##vso[task.setvariable variable=$varName;isOutput=true]true"
    }
    exit 0
}

# ---------------------------------------------------------------------------
# 4. Get changed files
# ---------------------------------------------------------------------------
$changedFiles = git diff --name-only $BaseSha HEAD 2>$null
if (-not $changedFiles) {
    Write-Host "No changed files detected — all stages enabled"
    foreach ($stage in $stages) {
        $varName = "STAGE_$($stage.Name.ToUpper())"
        Write-Host "##vso[task.setvariable variable=$varName;isOutput=true]true"
    }
    exit 0
}

Write-Host ""
Write-Host "Changed files ($($changedFiles.Count)):"
foreach ($f in $changedFiles) {
    Write-Host "  $f"
}

# ---------------------------------------------------------------------------
# 5. Match changed files against stage paths
# ---------------------------------------------------------------------------
function Test-PathMatch {
    param([string]$File, [string]$Pattern)
    # Convert glob to simple matching
    # ** = any depth, * = single segment
    $regex = '^' + ($Pattern -replace '\*\*', '§§' -replace '\*', '[^/]*' -replace '§§', '.*') + '$'
    return $File -match $regex
}

$stageResults = @{}
foreach ($stage in $stages) {
    $name = $stage.Name
    $paths = @($stage.Value.paths)
    $matched = $false

    foreach ($file in $changedFiles) {
        foreach ($pattern in $paths) {
            if (Test-PathMatch -File $file -Pattern $pattern) {
                $matched = $true
                break
            }
        }
        if ($matched) { break }
    }

    $stageResults[$name] = $matched
}

# ---------------------------------------------------------------------------
# 5b. Check for unmatched files — if any changed file doesn't match ANY
#     stage or ignore pattern, trigger ALL stages (safe fallback)
# ---------------------------------------------------------------------------
$ignorePatterns = @($config.ignore | Where-Object { $_ -and -not $_.StartsWith('_comment') })
$allStagePatterns = @()
foreach ($stage in $stages) {
    $allStagePatterns += @($stage.Value.paths)
}

$unmatchedFiles = @()
foreach ($file in $changedFiles) {
    $matchedAny = $false

    # Check stage patterns
    foreach ($pattern in $allStagePatterns) {
        if (Test-PathMatch -File $file -Pattern $pattern) { $matchedAny = $true; break }
    }

    # Check ignore patterns
    if (-not $matchedAny) {
        foreach ($pattern in $ignorePatterns) {
            if (Test-PathMatch -File $file -Pattern $pattern) { $matchedAny = $true; break }
        }
    }

    if (-not $matchedAny) {
        $unmatchedFiles += $file
    }
}

if ($unmatchedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️  Unmatched files (triggering ALL stages as safe fallback):"
    foreach ($f in $unmatchedFiles) {
        Write-Host "  $f"
    }
    foreach ($stage in $stages) {
        $stageResults[$stage.Name] = $true
    }
}

# ---------------------------------------------------------------------------
# 6. Propagate dependencies — if a stage is triggered, its dependents run too
# ---------------------------------------------------------------------------
$changed = $true
while ($changed) {
    $changed = $false
    foreach ($stage in $stages) {
        $name = $stage.Name
        if ($stageResults[$name]) { continue }

        $deps = @($stage.Value.depends_on)
        foreach ($dep in $deps) {
            if ($dep -and $stageResults[$dep]) {
                $stageResults[$name] = $true
                $changed = $true
                break
            }
        }
    }
}

# ---------------------------------------------------------------------------
# 7. Output
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════╗"
Write-Host "║  Stage Analysis                                  ║"
Write-Host "╠══════════════════════════════════════════════════╣"
foreach ($stage in $stages) {
    $name = $stage.Name
    $run = $stageResults[$name]
    $icon = if ($run) { "🔨" } else { "⏭️" }
    $label = if ($run) { "RUN" } else { "SKIP" }
    Write-Host "║  $icon $($name.PadRight(15)) $label"

    $varName = "STAGE_$($name.ToUpper())"
    Write-Host "##vso[task.setvariable variable=$varName;isOutput=true]$($run.ToString().ToLower())"
}
Write-Host "╚══════════════════════════════════════════════════╝"
