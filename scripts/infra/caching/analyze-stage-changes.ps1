<#
.SYNOPSIS Determine which pipeline stages need to run based on changed files.
.DESCRIPTION
    Compares HEAD against the merge base (or a given base SHA) and checks
    which job path patterns are matched. Outputs ADO pipeline variables
    for each stage: JOB_NATIVE, JOB_MANAGED, JOB_TESTS, etc.

    On protected branches (main, release/*), all jobs always run.

    Use -Validate to check that ALL files in the repo are covered by
    at least one job or ignore pattern.

.PARAMETER BaseSha
    The base commit to diff against. Defaults to HEAD~1.

.PARAMETER Validate
    Scan the entire repo for uncovered files instead of checking a diff.
#>
param(
    [string]$BaseSha = '',
    [switch]$Validate
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# 1. Load stage config
# ---------------------------------------------------------------------------
$configPath = 'scripts/infra/caching/repo-deps.config.json'
if (-not (Test-Path $configPath)) {
    Write-Host "Config not found — all jobs enabled"
    exit 0
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json
$jobs = $config.jobs.PSObject.Properties

# ---------------------------------------------------------------------------
# VALIDATE MODE — check all tracked files are covered
# ---------------------------------------------------------------------------
if ($Validate) {
    Write-Host "Validating all tracked files have coverage..."
    Write-Host ""

    $ignorePatterns = @($config.ignore | Where-Object { $_ -and -not $_.StartsWith('_comment') })
    $allJobPatterns = @()
    $allSubmodules = @()
    foreach ($job in $jobs) {
        $allJobPatterns += @($job.Value.paths)
        $allSubmodules += @($job.Value.submodules | Where-Object { $_ })
    }

    function Test-PathMatch {
        param([string]$File, [string]$Pattern)
        $regex = '^' + ($Pattern -replace '\*\*', '§§' -replace '\*', '[^/]*' -replace '§§', '.*') + '$'
        return $File -match $regex
    }

    $trackedFiles = git ls-files -z 2>$null | ForEach-Object { $_ -split "`0" } | Where-Object { $_ }
    # Unquote git's octal-escaped paths
    $trackedFiles = $trackedFiles | ForEach-Object { $_ -replace '^"(.*)"$', '$1' }
    $uncovered = @()
    $coveredCount = 0
    $ignoredCount = 0

    foreach ($file in $trackedFiles) {
        $matchedAny = $false

        foreach ($pattern in $allJobPatterns) {
            if (Test-PathMatch -File $file -Pattern $pattern) { $matchedAny = $true; break }
        }
        if (-not $matchedAny) {
            foreach ($pattern in $ignorePatterns) {
                if (Test-PathMatch -File $file -Pattern $pattern) { $matchedAny = $true; $ignoredCount++; break }
            }
        }
        if (-not $matchedAny) {
            foreach ($sub in $allSubmodules) {
                if ($file -eq $sub -or $file.StartsWith("$sub/")) { $matchedAny = $true; break }
            }
        }

        if ($matchedAny) {
            $coveredCount++
        } else {
            $uncovered += $file
        }
    }

    Write-Host "Total tracked files: $($trackedFiles.Count)"
    Write-Host "Covered by jobs:     $coveredCount"
    Write-Host "Covered by ignore:   $ignoredCount"
    Write-Host "Uncovered:           $($uncovered.Count)"

    if ($uncovered.Count -gt 0) {
        Write-Host ""
        Write-Host "##[error]❌ UNCOVERED FILES:"
        foreach ($f in $uncovered) {
            Write-Host "##[error]  $f"
        }
        Write-Host ""
        Write-Host "##[error]Add these to a job or ignore list in $configPath"
        exit 1
    } else {
        Write-Host ""
        Write-Host "✅ All files covered!"
        exit 0
    }
}

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
    Write-Host "Protected branch '$branch' — all jobs enabled"
    foreach ($job in $jobs) {
        $varName = "JOB_$($job.Name.ToUpper())"
        Write-Host "##vso[task.setvariable variable=$varName;isOutput=true]true"
    }
    exit 0
}

# ---------------------------------------------------------------------------
# 4. Get changed files
# ---------------------------------------------------------------------------
$changedFiles = git diff --name-only $BaseSha HEAD 2>$null
if (-not $changedFiles) {
    Write-Host "No changed files detected — all jobs enabled"
    foreach ($job in $jobs) {
        $varName = "JOB_$($job.Name.ToUpper())"
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
# 5. Match changed files against job paths
# ---------------------------------------------------------------------------
function Test-PathMatch {
    param([string]$File, [string]$Pattern)
    # Convert glob to simple matching
    # ** = any depth, * = single segment
    $regex = '^' + ($Pattern -replace '\*\*', '§§' -replace '\*', '[^/]*' -replace '§§', '.*') + '$'
    return $File -match $regex
}

$jobResults = @{}
foreach ($job in $jobs) {
    $name = $job.Name
    $paths = @($job.Value.paths)
    $submodules = @($job.Value.submodules | Where-Object { $_ })
    $matched = $false

    foreach ($file in $changedFiles) {
        # Check path patterns
        foreach ($pattern in $paths) {
            if (Test-PathMatch -File $file -Pattern $pattern) {
                $matched = $true
                break
            }
        }
        if ($matched) { break }

        # Check submodule paths (match if the file starts with the submodule path)
        foreach ($sub in $submodules) {
            if ($file -eq $sub -or $file.StartsWith("$sub/")) {
                $matched = $true
                break
            }
        }
        if ($matched) { break }
    }

    $jobResults[$name] = $matched
}

# ---------------------------------------------------------------------------
# 5b. Check for unmatched files — if any changed file doesn't match ANY
#     job or ignore pattern, trigger ALL jobs (safe fallback)
# ---------------------------------------------------------------------------
$ignorePatterns = @($config.ignore | Where-Object { $_ -and -not $_.StartsWith('_comment') })
$allJobPatterns = @()
foreach ($job in $jobs) {
    $allJobPatterns += @($job.Value.paths)
}

$unmatchedFiles = @()
foreach ($file in $changedFiles) {
    $matchedAny = $false

    # Check stage patterns
    foreach ($pattern in $allJobPatterns) {
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
    Write-Host "##[error]❌ UNMATCHED FILES — not covered by any job or ignore pattern:"
    foreach ($f in $unmatchedFiles) {
        Write-Host "##[error]  $f"
    }
    Write-Host ""
    Write-Host "##[error]Add these paths to a job in scripts/infra/caching/repo-deps.config.json"
    Write-Host "##[error]or add them to the 'ignore' list if they don't affect builds."
    Write-Host ""
    Write-Host "##vso[task.logissue type=error]Unmatched files detected: $($unmatchedFiles -join ', ')"
    exit 1
}

# ---------------------------------------------------------------------------
# 6. Propagate dependencies — if a stage is triggered, its dependents run too
# ---------------------------------------------------------------------------
$changed = $true
while ($changed) {
    $changed = $false
    foreach ($job in $jobs) {
        $name = $job.Name
        if ($jobResults[$name]) { continue }

        $deps = @($job.Value.depends_on)
        foreach ($dep in $deps) {
            if ($dep -and $jobResults[$dep]) {
                $jobResults[$name] = $true
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
Write-Host "║  Job Analysis                                  ║"
Write-Host "╠══════════════════════════════════════════════════╣"
foreach ($job in $jobs) {
    $name = $job.Name
    $run = $jobResults[$name]
    $icon = if ($run) { "🔨" } else { "⏭️" }
    $label = if ($run) { "RUN" } else { "SKIP" }
    Write-Host "║  $icon $($name.PadRight(15)) $label"

    $varName = "JOB_$($name.ToUpper())"
    Write-Host "##vso[task.setvariable variable=$varName;isOutput=true]$($run.ToString().ToLower())"
}
Write-Host "╚══════════════════════════════════════════════════╝"
