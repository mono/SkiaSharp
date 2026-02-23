<#
.SYNOPSIS  Run triage+repro benchmark across multiple issues and AI models.
.DESCRIPTION
    Launches Copilot CLI for each issue×model combination, running triage then repro.
    Collects JSONs, validates them, and generates a summary report.
    Optionally runs cross-model analysis and meta-analysis.

    In benchmark mode ($env:SKIASHARP_BENCHMARK=1), the persist scripts skip git push.

.EXAMPLE  pwsh scripts/benchmark-pipeline.ps1
.EXAMPLE  pwsh scripts/benchmark-pipeline.ps1 -Issues 3400,3428 -Models claude-opus-4.6
.EXAMPLE  pwsh scripts/benchmark-pipeline.ps1 -Resume output/benchmarks/20260223-111600
.EXAMPLE  pwsh scripts/benchmark-pipeline.ps1 -Resume output/benchmarks/20260223-111600 -SkipRuns
.EXAMPLE  pwsh scripts/benchmark-pipeline.ps1 -DryRun
#>
param(
    [int[]]$Issues = @(3400, 3428, 3429, 3430, 3435, 3437, 3440, 3472, 3509, 3511, 3519, 3520),
    [string[]]$Models = @('claude-opus-4.6', 'gpt-5.3-codex', 'gemini-3-pro-preview'),
    [string]$Resume,
    [switch]$SkipRuns,
    [switch]$SkipAnalysis,
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'

# ── CONFIG ────────────────────────────────────────────────────────────────────
$env:SKIASHARP_BENCHMARK = '1'

$ModelShort = @{
    'claude-opus-4.6'       = 'opus'
    'gpt-5.3-codex'         = 'gpt'
    'gemini-3-pro-preview'  = 'gemini'
}

$RepoRoot = git rev-parse --show-toplevel
$RepoSha = git rev-parse HEAD
$DataCacheSha = git -C .data-cache rev-parse HEAD

$TriageValidator = Join-Path $RepoRoot '.github/skills/issue-triage/scripts/validate-triage.ps1'
$ReproValidator  = Join-Path $RepoRoot '.github/skills/issue-repro/scripts/validate-repro.ps1'

if ($Resume) {
    $OutDir = $Resume
} else {
    $OutDir = Join-Path $RepoRoot "output/benchmarks/$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}

# ── PREFLIGHT ─────────────────────────────────────────────────────────────────
foreach ($cmd in 'copilot', 'pwsh', 'git') {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Required command not found: $cmd"; exit 2
    }
}
foreach ($f in $TriageValidator, $ReproValidator) {
    if (-not (Test-Path $f)) { Write-Host "❌ Validator not found: $f"; exit 2 }
}

foreach ($d in 'triage/json', 'triage/logs', 'triage/validation',
                'repro/json', 'repro/logs', 'repro/validation',
                'analysis', 'analysis-logs') {
    New-Item -ItemType Directory -Force (Join-Path $OutDir $d) | Out-Null
}

Write-Host "═══════════════════════════════════════════════════════"
Write-Host "  Benchmark Pipeline"
Write-Host "  Issues:     $($Issues.Count)"
Write-Host "  Models:     $($Models.Count)"
Write-Host "  Total runs: $($Issues.Count * $Models.Count * 2) (triage + repro)"
Write-Host "  Output:     $OutDir"
Write-Host "  Repo SHA:   $RepoSha"
Write-Host "  Cache SHA:  $DataCacheSha"
Write-Host "  Benchmark:  $($env:SKIASHARP_BENCHMARK)"
Write-Host "═══════════════════════════════════════════════════════"

# ── HELPERS ───────────────────────────────────────────────────────────────────
function Reset-Repo {
    Set-Location $RepoRoot
    git reset --hard $RepoSha 2>&1 | Out-Null
    git -C .data-cache reset --hard $DataCacheSha 2>&1 | Out-Null
    git -C .data-cache clean -fdx 2>&1 | Out-Null
    Remove-Item /tmp/triage-*.json -ErrorAction SilentlyContinue
    Remove-Item /tmp/repro-*.json -ErrorAction SilentlyContinue
}

function Invoke-Copilot {
    param([string]$Prompt, [string]$Model, [string]$LogPath)
    if ($script:DryRun) {
        "[dry-run] copilot -p '$Prompt' --model $Model" | Out-File $LogPath
        Write-Host "  [dry-run] Would invoke: copilot --model $Model"
        return
    }
    '' | & copilot -p $Prompt --model $Model --allow-all-tools --deny-tool 'shell(git push)' > $LogPath 2>&1
}

function Collect-Json {
    param([int]$Issue, [string]$Tag, [string]$DestDir, [string]$Skill)

    $candidates = @(
        (Join-Path $RepoRoot ".data-cache/repos/mono-SkiaSharp/ai-$Skill/$Issue.json")
    )

    # Search session-state for recent JSONs
    $sessionFiles = Get-ChildItem "$HOME/.copilot/session-state/*/files/*.json" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTime -gt (Get-Date).AddMinutes(-20) }
    if ($sessionFiles) { $candidates += $sessionFiles.FullName }

    # Search /tmp
    $tmpFiles = Get-ChildItem "/tmp/$Skill-*.json" -ErrorAction SilentlyContinue
    if ($tmpFiles) { $candidates += $tmpFiles.FullName }

    foreach ($src in $candidates) {
        if (Test-Path $src) {
            Copy-Item $src (Join-Path $DestDir "$Tag.json") -Force
            Write-Host "  → Collected from $src"
            return $true
        }
    }
    Write-Host "  → NO JSON collected"
    return $false
}

# ── PHASE 1+2: SKILL RUNS ────────────────────────────────────────────────────
if (-not $SkipRuns) {
    $run = 0
    $total = $Issues.Count * $Models.Count

    foreach ($issue in $Issues) {
        foreach ($model in $Models) {
            $short = $ModelShort[$model]
            $tag = "$issue-$short"
            $run++

            $tDone = Test-Path (Join-Path $OutDir "triage/json/$tag.json")
            $rDone = Test-Path (Join-Path $OutDir "repro/json/$tag.json")
            if ($tDone -and $rDone) {
                Write-Host "[$run/$total] SKIP $tag (both exist)"
                continue
            }

            Write-Host ""
            Write-Host "[$run/$total] ══ Issue #$issue with $short ══"

            Reset-Repo

            # ── TRIAGE ──
            if (-not $tDone) {
                Write-Host "  ── Triage ──"
                $logPath = Join-Path $OutDir "triage/logs/$tag.log"
                Invoke-Copilot -Prompt "triage issue #$issue" -Model $model -LogPath $logPath

                $collected = Collect-Json -Issue $issue -Tag $tag `
                    -DestDir (Join-Path $OutDir "triage/json") -Skill 'triage'

                if ($collected) {
                    $jsonPath = Join-Path $OutDir "triage/json/$tag.json"
                    $valOut = & pwsh $TriageValidator $jsonPath 2>&1
                    $valOut | Out-File (Join-Path $OutDir "triage/validation/$tag.txt")
                    Write-Host "  Validation: $($valOut | Select-Object -Last 1)"
                }
            }

            # Ensure triage JSON is in data-cache for repro to read
            $triageJson = Join-Path $OutDir "triage/json/$tag.json"
            if (Test-Path $triageJson) {
                $triageDest = Join-Path $RepoRoot ".data-cache/repos/mono-SkiaSharp/ai-triage/$issue.json"
                New-Item -ItemType Directory -Force (Split-Path $triageDest) | Out-Null
                Copy-Item $triageJson $triageDest -Force
                Write-Host "  Planted triage JSON for repro"
            }

            # ── REPRO ──
            if (-not $rDone) {
                Write-Host "  ── Repro ──"
                $logPath = Join-Path $OutDir "repro/logs/$tag.log"
                Invoke-Copilot -Prompt "reproduce issue #$issue" -Model $model -LogPath $logPath

                $collected = Collect-Json -Issue $issue -Tag $tag `
                    -DestDir (Join-Path $OutDir "repro/json") -Skill 'repro'

                if ($collected) {
                    $jsonPath = Join-Path $OutDir "repro/json/$tag.json"
                    $valOut = & pwsh $ReproValidator $jsonPath 2>&1
                    $valOut | Out-File (Join-Path $OutDir "repro/validation/$tag.txt")
                    Write-Host "  Validation: $($valOut | Select-Object -Last 1)"
                }
            }
        }
    }
}

# ── PHASE 3: CROSS-MODEL ANALYSIS ────────────────────────────────────────────
if (-not $SkipAnalysis) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════"
    Write-Host "  Phase 3: Cross-Model Analysis"
    Write-Host "═══════════════════════════════════════════════════════"

    $analysisPrompt = @"
You are a triage and reproduction quality reviewer. Read ALL files in these directories:
- $OutDir/triage/json/ (triage JSONs from all models)
- $OutDir/triage/validation/ (validation results)
- $OutDir/repro/json/ (repro JSONs from all models)
- $OutDir/repro/validation/ (validation results)

For each model (opus, gpt, gemini), score 1-10 on these dimensions:
1. Schema compliance — did the JSON pass validation?
2. Classification accuracy (triage) / Conclusion accuracy (repro)
3. Evidence quality — bugSignals, userContext, reproduction steps
4. Code investigation depth / Reproduction thoroughness
5. Response quality — actionable, correct, helpful

Produce a markdown report with:
- Per-model score table
- Notable strengths and weaknesses per model
- Examples of best and worst outputs
- Recommendations for skill improvement

Save your analysis report using the create tool to: $OutDir/analysis/REVIEWER.md
(Replace REVIEWER with your model short name.)
"@

    foreach ($model in $Models) {
        $short = $ModelShort[$model]
        Write-Host "  Analysis by $short..."
        $logPath = Join-Path $OutDir "analysis-logs/$short.log"
        Invoke-Copilot -Prompt $analysisPrompt -Model $model -LogPath $logPath
        Write-Host "  → Done"
    }

    # ── META-ANALYSIS ─────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host "  Meta-analysis: consolidating..."

    $metaPrompt = @"
Read the 3 analysis reports in $OutDir/analysis/ (one from each model).
Produce a final consolidated REPORT.md with:
1. Consensus findings — where all 3 reviewers agree
2. Disagreements — where reviewers differ and why
3. Per-model summary score table (averaged across reviewers)
4. Top 5 recommendations for improving the triage and repro skills
5. Overall assessment: is the pipeline ready for automation?

Save the report using the create tool to: $OutDir/REPORT.md
"@

    $logPath = Join-Path $OutDir "analysis-logs/meta.log"
    Invoke-Copilot -Prompt $metaPrompt -Model 'claude-opus-4.6' -LogPath $logPath
    Write-Host "  → Meta-analysis done"
}

# ── SUMMARY ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════"
Write-Host "  Benchmark Complete"
Write-Host "═══════════════════════════════════════════════════════"

$triageJsons = (Get-ChildItem (Join-Path $OutDir "triage/json/*.json") -ErrorAction SilentlyContinue).Count
$reproJsons  = (Get-ChildItem (Join-Path $OutDir "repro/json/*.json") -ErrorAction SilentlyContinue).Count
$triageLogs  = (Get-ChildItem (Join-Path $OutDir "triage/logs/*.log") -ErrorAction SilentlyContinue).Count
$reproLogs   = (Get-ChildItem (Join-Path $OutDir "repro/logs/*.log") -ErrorAction SilentlyContinue).Count
$totalExpected = $Issues.Count * $Models.Count

Write-Host "  Triage:  $triageJsons/$totalExpected JSONs, $triageLogs logs"
Write-Host "  Repro:   $reproJsons/$totalExpected JSONs, $reproLogs logs"
Write-Host "  Output:  $OutDir"

if (Test-Path (Join-Path $OutDir "REPORT.md")) {
    Write-Host "  Report:  $OutDir/REPORT.md"
}

Write-Host "═══════════════════════════════════════════════════════"
