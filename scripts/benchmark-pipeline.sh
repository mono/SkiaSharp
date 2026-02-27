#!/bin/bash
# benchmark-pipeline.sh — Run triage+repro benchmark across issues × models.
#
# Usage:
#   ./scripts/benchmark-pipeline.sh                                    # full run
#   ./scripts/benchmark-pipeline.sh --issues "3400 3428" --models opus # subset
#   ./scripts/benchmark-pipeline.sh --resume output/benchmarks/...     # resume
#   ./scripts/benchmark-pipeline.sh --skip-runs                        # analysis only
#   ./scripts/benchmark-pipeline.sh --dry-run                          # no copilot calls
set -euo pipefail

# ── CLEANUP TRAP ──────────────────────────────────────────────────────────────
COPILOT_PID=""
cleanup() {
    if [[ -n "$COPILOT_PID" ]] && kill -0 "$COPILOT_PID" 2>/dev/null; then
        echo ""
        echo "  ⚠ Killing copilot (pid $COPILOT_PID)..."
        kill "$COPILOT_PID" 2>/dev/null || true
        wait "$COPILOT_PID" 2>/dev/null || true
    fi
    exit 130
}
trap cleanup INT TERM

# ── DEFAULTS ──────────────────────────────────────────────────────────────────
ISSUES=(3400 3428 3429 3430 3435 3437 3440 3472 3509 3511 3519 3520)
MODELS=("claude-opus-4.6" "gpt-5.3-codex" "gemini-3-pro-preview")
RESUME=""
SKIP_RUNS=false
SKIP_ANALYSIS=false
DRY_RUN=false

# ── ARG PARSE ─────────────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
    case "$1" in
        --issues)       read -ra ISSUES <<< "$2"; shift 2 ;;
        --models)       read -ra MODELS <<< "$2"; shift 2 ;;
        --resume)       RESUME="$2"; shift 2 ;;
        --skip-runs)    SKIP_RUNS=true; shift ;;
        --skip-analysis) SKIP_ANALYSIS=true; shift ;;
        --dry-run)      DRY_RUN=true; shift ;;
        -h|--help)
            sed -n '2,7p' "$0"; exit 0 ;;
        *) echo "Unknown arg: $1"; exit 1 ;;
    esac
done

# ── CONFIG ────────────────────────────────────────────────────────────────────
export SKIASHARP_BENCHMARK=1

REPO_ROOT=$(git rev-parse --show-toplevel)
REPO_SHA=$(git rev-parse HEAD)
DATA_CACHE_SHA=$(git -C .data-cache rev-parse HEAD)
TRIAGE_VALIDATOR="$REPO_ROOT/.github/skills/issue-triage/scripts/validate-triage.ps1"
REPRO_VALIDATOR="$REPO_ROOT/.github/skills/issue-repro/scripts/validate-repro.ps1"

if [[ -n "$RESUME" ]]; then
    OUTDIR="$(cd "$RESUME" 2>/dev/null && pwd || echo "$REPO_ROOT/$RESUME")"
else
    OUTDIR="$REPO_ROOT/output/benchmarks/$(date +%Y%m%d-%H%M%S)"
fi

# ── PREFLIGHT ─────────────────────────────────────────────────────────────────
for cmd in copilot pwsh git; do
    command -v "$cmd" >/dev/null || { echo "❌ Required: $cmd"; exit 2; }
done
for f in "$TRIAGE_VALIDATOR" "$REPRO_VALIDATOR"; do
    [[ -f "$f" ]] || { echo "❌ Validator not found: $f"; exit 2; }
done

for d in triage/json triage/logs triage/validation repro/json repro/logs repro/validation analysis analysis-logs; do
    mkdir -p "$OUTDIR/$d"
done

TOTAL=$(( ${#ISSUES[@]} * ${#MODELS[@]} ))
echo "═══════════════════════════════════════════════════════"
echo "  Benchmark Pipeline"
echo "  Issues:     ${#ISSUES[@]}"
echo "  Models:     ${#MODELS[@]}"
echo "  Total runs: $((TOTAL * 2)) (triage + repro)"
echo "  Output:     $OUTDIR"
echo "  Repo SHA:   $REPO_SHA"
echo "  Cache SHA:  $DATA_CACHE_SHA"
echo "  Dry run:    $DRY_RUN"
echo "═══════════════════════════════════════════════════════"

# ── HELPERS ───────────────────────────────────────────────────────────────────
reset_repo() {
    cd "$REPO_ROOT"
    # git reset --hard "$REPO_SHA" >/dev/null 2>&1
    git -C .data-cache reset --hard "$DATA_CACHE_SHA" >/dev/null 2>&1
    git -C .data-cache clean -fdx >/dev/null 2>&1
    rm -rf /tmp/skiasharp/triage /tmp/skiasharp/repro /tmp/skiasharp/fix
}

invoke_copilot() {
    local prompt="$1" model="$2" logpath="$3"
    if $DRY_RUN; then
        mkdir -p "$(dirname "$logpath")"
        echo "[dry-run] copilot -p '$prompt' --model $model" > "$logpath"
        echo "  [dry-run] Would invoke: copilot --model $model"
        return
    fi
    local LOG_WINDOW=24

    # Start copilot in background
    mkdir -p "$(dirname "$logpath")"
    echo '' | copilot -p "$prompt" --model "$model" \
        --yolo --deny-tool 'shell(git push)' \
        > "$logpath" 2>&1 &
    local pid=$!
    COPILOT_PID=$pid
    sleep 1

    # Draw initial empty window
    for ((i=0; i<LOG_WINDOW; i++)); do echo "  │"; done
    echo "  └ waiting..."

    # Stream: redraw the window in-place as log grows
    local prev_count=0
    while kill -0 "$pid" 2>/dev/null; do
        if [[ -f "$logpath" ]]; then
            local cur_count
            cur_count=$(wc -l < "$logpath" 2>/dev/null | tr -d ' ')
            if (( cur_count != prev_count )); then
                # Move cursor up LOG_WINDOW+1 lines (window + status), clear each
                echo -ne "\033[$((LOG_WINDOW + 1))A"
                local window
                window=$(tail -n "$LOG_WINDOW" "$logpath" 2>/dev/null || true)
                local drawn=0
                while IFS= read -r line; do
                    printf "\033[2K  │ %.120s\n" "$line"
                    drawn=$((drawn + 1))
                done <<< "$window"
                # Pad remaining empty lines
                for ((i=drawn; i<LOG_WINDOW; i++)); do
                    printf "\033[2K  │\n"
                done
                printf "\033[2K  └ %d lines | pid %d | %s\n" "$cur_count" "$pid" "$logpath"
                prev_count=$cur_count
            fi
        fi
        sleep 1
    done
    wait "$pid" 2>/dev/null || true
    COPILOT_PID=""

    # Final redraw
    local lines=0
    [[ -f "$logpath" ]] && lines=$(wc -l < "$logpath" | tr -d ' ')
    echo -ne "\033[$((LOG_WINDOW + 1))A"
    local window
    window=$(tail -n "$LOG_WINDOW" "$logpath" 2>/dev/null || true)
    local drawn=0
    while IFS= read -r line; do
        printf "\033[2K  │ %.120s\n" "$line"
        drawn=$((drawn + 1))
    done <<< "$window"
    for ((i=drawn; i<LOG_WINDOW; i++)); do
        printf "\033[2K  │\n"
    done
    printf "\033[2K  └ Done (%d lines logged)\n" "$lines"
    [[ -f "$logpath" ]] && lines=$(wc -l < "$logpath" | tr -d ' ')
    echo "  └ Done ($lines lines logged)"
}

collect_json() {
    local issue="$1" tag="$2" dest_dir="$3" skill="$4"
    # Search: data-cache, /tmp/skiasharp, session-state
    local candidates=(
        "$REPO_ROOT/.data-cache/repos/mono-SkiaSharp/ai-$skill/$issue.json"
        "/tmp/skiasharp/$skill/$issue.json"
    )
    # Recent session-state JSONs
    while IFS= read -r f; do
        candidates+=("$f")
    done < <(find "$HOME/.copilot/session-state" -name "*.json" -path "*/files/*" -newer "$dest_dir/../logs/$tag.log" 2>/dev/null || true)

    for src in "${candidates[@]}"; do
        if [[ -f "$src" ]]; then
            cp "$src" "$dest_dir/$tag.json"
            echo "  → Collected from $src"
            return 0
        fi
    done
    echo "  → NO JSON collected"
    return 1
}

validate_json() {
    local json_path="$1" validator="$2" val_path="$3"
    if [[ -f "$json_path" ]]; then
        local val_out
        val_out=$(pwsh "$validator" "$json_path" 2>&1) || true
        echo "$val_out" > "$val_path"
        local last_line
        last_line=$(echo "$val_out" | tail -1)
        echo "  Validation: $last_line"
    fi
}

# ── PHASE 1: SKILL RUNS ───────────────────────────────────────────────────────
if ! $SKIP_RUNS; then
    echo ""
    echo "═══════════════════════════════════════════════════════"
    echo "  Phase 1: Skill Runs"
    echo "═══════════════════════════════════════════════════════"
    run=0
    for issue in "${ISSUES[@]}"; do
        for model in "${MODELS[@]}"; do
            tag="${issue}-${model}"
            run=$((run + 1))

            t_done=false; r_done=false
            [[ -f "$OUTDIR/triage/json/$tag.json" ]] && t_done=true
            [[ -f "$OUTDIR/repro/json/$tag.json" ]] && r_done=true
            if $t_done && $r_done; then
                echo "[$run/$TOTAL] SKIP $tag (both exist)"
                continue
            fi

            echo ""
            echo "[$run/$TOTAL] ══ Issue #$issue with $model ══"
            reset_repo

            # ── TRIAGE ──
            if ! $t_done; then
                echo "  ── Triage ──"
                invoke_copilot "triage issue #$issue" "$model" "$OUTDIR/triage/logs/$tag.log"
                collect_json "$issue" "$tag" "$OUTDIR/triage/json" "triage" || true
                validate_json "$OUTDIR/triage/json/$tag.json" "$TRIAGE_VALIDATOR" "$OUTDIR/triage/validation/$tag.txt"
            fi

            # Plant triage JSON for repro to read
            if [[ -f "$OUTDIR/triage/json/$tag.json" ]]; then
                mkdir -p "$REPO_ROOT/.data-cache/repos/mono-SkiaSharp/ai-triage"
                cp "$OUTDIR/triage/json/$tag.json" "$REPO_ROOT/.data-cache/repos/mono-SkiaSharp/ai-triage/$issue.json"
                echo "  Planted triage JSON for repro"
            fi

            # ── REPRO ──
            if ! $r_done; then
                echo "  ── Repro ──"
                invoke_copilot "reproduce issue #$issue" "$model" "$OUTDIR/repro/logs/$tag.log"
                collect_json "$issue" "$tag" "$OUTDIR/repro/json" "repro" || true
                validate_json "$OUTDIR/repro/json/$tag.json" "$REPRO_VALIDATOR" "$OUTDIR/repro/validation/$tag.txt"
            fi
        done
    done
fi

# ── PHASE 2: CROSS-MODEL ANALYSIS ────────────────────────────────────────────
if ! $SKIP_ANALYSIS; then
    echo ""
    echo "═══════════════════════════════════════════════════════"
    echo "  Phase 2: Cross-Model Analysis"
    echo "═══════════════════════════════════════════════════════"

    ANALYSIS_PROMPT="You are a triage and reproduction quality reviewer. Read ALL files in these directories:
- $OUTDIR/triage/json/ (triage JSONs from all models)
- $OUTDIR/triage/validation/ (validation results)
- $OUTDIR/repro/json/ (repro JSONs from all models)
- $OUTDIR/repro/validation/ (validation results)

For each model (opus, gpt, gemini), score 1-10 on these dimensions:
1. Schema compliance — did the JSON pass validation?
2. Classification accuracy (triage) / Conclusion accuracy (repro)
3. Evidence quality — bugSignals, userContext, reproduction steps
4. Code investigation depth / Reproduction thoroughness
5. Response quality — actionable, correct, helpful

Produce a markdown report with per-model score table, notable strengths/weaknesses, examples of best/worst outputs, and recommendations.
Save your analysis report using the create tool to: $OUTDIR/analysis/REVIEWER.md
(Replace REVIEWER with your full model name, e.g. claude-opus-4.6.)"

    for model in "${MODELS[@]}"; do
        echo "  Analysis by $model..."
        invoke_copilot "$ANALYSIS_PROMPT" "$model" "$OUTDIR/analysis-logs/$model.log"
    done
fi

# ── PHASE 3: FINAL ANALYSIS ──────────────────────────────────────────────────
if ! $SKIP_ANALYSIS; then
    echo ""
    echo "═══════════════════════════════════════════════════════"
    echo "  Phase 3: Final Analysis"
    echo "═══════════════════════════════════════════════════════"

    META_PROMPT="Read the 3 analysis reports in $OUTDIR/analysis/ (one from each model).
Produce a final consolidated REPORT.md with:
1. Consensus findings — where all 3 reviewers agree
2. Disagreements — where reviewers differ and why
3. Per-model summary score table (averaged across reviewers)
4. Top 5 recommendations for improving the triage and repro skills
5. Overall assessment: is the pipeline ready for automation?
Save the report using the create tool to: $OUTDIR/REPORT.md"

    invoke_copilot "$META_PROMPT" "claude-opus-4.6" "$OUTDIR/analysis-logs/meta.log"
fi

# ── SUMMARY ───────────────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════"
echo "  Benchmark Complete"
echo "═══════════════════════════════════════════════════════"
triage_jsons=$(find "$OUTDIR/triage/json" -name '*.json' 2>/dev/null | wc -l | tr -d ' ')
repro_jsons=$(find "$OUTDIR/repro/json" -name '*.json' 2>/dev/null | wc -l | tr -d ' ')
triage_logs=$(find "$OUTDIR/triage/logs" -name '*.log' 2>/dev/null | wc -l | tr -d ' ')
repro_logs=$(find "$OUTDIR/repro/logs" -name '*.log' 2>/dev/null | wc -l | tr -d ' ')
echo "  Triage:  $triage_jsons/$TOTAL JSONs, $triage_logs logs"
echo "  Repro:   $repro_jsons/$TOTAL JSONs, $repro_logs logs"
echo "  Output:  $OUTDIR"
[[ -f "$OUTDIR/REPORT.md" ]] && echo "  Report:  $OUTDIR/REPORT.md"
echo "═══════════════════════════════════════════════════════"
