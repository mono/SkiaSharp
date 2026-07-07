---
# =============================================================================
# Fixer - Performance
#
# Periodic AI-driven workflow that SCANS the repo's managed C# for a hot-path
# performance opportunity, PROVES the win with a BenchmarkDotNet New-vs-Old
# measurement AND proves behaviour is unchanged with a correctness/equivalence
# test, FIXES it, and opens a DRAFT PR. Modelled on the memory-leak-fixer
# workflow and on the repo's real perf PRs (SKMatrix native→managed port #4241,
# SKColor allocation-free parse #4345, SKSurface.Canvas caching #4247). Scope:
# managed C# only — the native Skia under externals/skia/** is upstream and out
# of scope.
#
# All the domain knowledge lives in the reusable skill
# `.agents/skills/performance-fixer/SKILL.md`; this file is the schedule + the
# guardrails (one finding per run, de-dup, two-proof-before-PR).
# =============================================================================
description: "Scan the repo's managed C# for a hot-path performance opportunity, prove it with a benchmark + a behaviour-parity test, fix it, and open a draft PR."

environment: gh-aw-agents

# -- Engine ------------------------------------------------------------
# Perf reasoning (a faithful, bit-exact optimization that must not change
# rendering output) is hard and benefits from the stronger model, matching
# memory-leak-fixer's choice.
engine:
  id: copilot
  model: claude-opus-4.8

# -- Triggers ----------------------------------------------------------
# Every 12h + manual + PR-driven self-test. Every run does the same full
# scan→prove-faster→prove-identical→fix→file pipeline; the knobs are `dry_run`
# (do everything but open nothing) and `focus_area` (force one optimization
# area instead of the time-based rotation — for testing a specific area on
# demand). A `pull_request` that edits THIS workflow or its skill re-runs the
# whole pipeline in FORCED DRY-RUN (see Step 2.6) so we can iterate on the
# prompt/skill and watch the run without ever opening a real PR/issue.
on:
  schedule: every 12h
  workflow_dispatch:
    inputs:
      dry_run:
        description: "Do the full scan→prove→fix locally but do NOT open a PR/issue."
        required: false
        default: false
        type: boolean
      focus_area:
        description: "Force a specific optimization area 0-4 (see references/hot-paths/) instead of the time-based round-robin. Leave blank to rotate."
        required: false
        default: ""
        type: string
  pull_request:
    paths:
      - ".github/workflows/performance-fixer.md"
      - ".github/workflows/performance-fixer.lock.yml"
      - ".agents/skills/performance-fixer/**"

# Only ever run on the canonical repo — never on forks.
if: |
  github.repository == 'mono/SkiaSharp'

# Give the skill's Phase 5 report a writable step-summary sink (mirrors the
# convention used by memory-leak-fixer / auto-triage).
steps:
  - name: Redirect step summary into agent-writable directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md

concurrency:
  group: "performance-fixer"
  cancel-in-progress: false

# Benchmarks (BenchmarkDotNet warms up + runs multiple iterations) plus a build
# and the equivalence tests take real time, so keep the generous budget.
timeout-minutes: 120

# create-pull-request is commit-based and needs full history for its branch ops
# (and for the skill's date-based focus rotation). This skill is managed-C#
# only and builds/benchmarks against pre-built natives (externals-download), so
# the skia source submodule is NOT needed — a plain checkout keeps runs fast.
checkout:
  - fetch-depth: 0

permissions:
  contents: read
  issues: read
  pull-requests: read

# -- Agent tools -----------------------------------------------------
# The agent reads source, writes a BenchmarkDotNet benchmark + a correctness/
# equivalence test, edits managed C# to optimize, builds, benchmarks, tests,
# then commits. `dotnet`/`pwsh`/`git` are the work tools; `gh` powers de-dup
# queries. Use the edit tool so changes are reviewable.
tools:
  github:
    toolsets: [issues, pull_requests, search]
    allowed-repos: ["mono/skiasharp"]
    min-integrity: none
  edit:
  bash: ["dotnet", "pwsh", "git", "gh", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "cp", "mv", "rm"]

# -- Network allowlist -----------------------------------------------
# The SkiaSharp-CI Azure DevOps feed + blob storage for `externals-download`
# pre-built natives; the `dotnet` ecosystem covers nuget.org (BenchmarkDotNet +
# restore) and pkgs.dev.azure.com and the SDK/restore hosts.
network:
  allowed:
    - defaults
    - github
    - dotnet
    - "*.blob.core.windows.net"

# -- Safe outputs ------------------------------------------------------
# A confirmed, managed-C#-fixable optimization emits a PAIR: a [performance]
# issue (the finding + measured numbers) + a draft fix PR that closes it on
# merge (Fixes #<temp-id>, resolved by gh-aw to the real number). Both carry
# `tenet/performance` (the quality-tenet umbrella) plus one agent-chosen
# `perf/*` sub-type (the win's dominant driver — see Step 2 guardrail 8 and the
# taxonomy in .agents/skills/issue-triage/references/labels.md). When the only
# real win is native / upstream Skia (out of scope here), the issue is filed
# alone. Quiet/dry runs emit a noop.
safe-outputs:
  create-pull-request:
    title-prefix: "[performance] "
    labels: [tenet/performance]
    allowed-labels: [tenet/performance, perf/allocations, perf/interop, perf/memory-leak, perf/rendering, perf/size, perf/startup, perf/throughput]
    draft: true
    allowed-base-branches: [main]
  create-issue:
    title-prefix: "[performance] "
    labels: [tenet/performance]
    allowed-labels: [tenet/performance, perf/allocations, perf/interop, perf/memory-leak, perf/rendering, perf/size, perf/startup, perf/throughput]
    max: 1
  noop:
    report-as-issue: false
---

# Fixer - Performance

You **scan** the repo's **managed C#** for one hot-path performance opportunity, **prove it is
faster** with a BenchmarkDotNet New-vs-Old measurement, **prove it is behaviour-identical** with a
correctness/equivalence test, **fix** it, then **file the finding as a `[performance]` issue and
open a draft PR that closes it** (`Fixes #…`) — or, when the only real win is in native / upstream
Skia (out of scope here), file the `[performance]` issue alone. **One finding per run.**

**Read this first — set your expectations correctly:**

- This codebase is **mature and much of the obvious overhead is already optimized** (the canvas
  getter is cached, the color parser is allocation-free, matrix batch ops are bulk/SIMD). There is
  **no planted/seeded win** to find. Many runs will (and should) find **nothing that clears the
  bar** — that is the normal, expected, successful outcome, **not** a failure. Do **not** invent a
  micro-optimization, do **not** claim a speedup you did not measure, and do **not** ship a 2% win
  on a synthetic loop that no real caller hits.
- **Two proofs or it is not a fix.** A candidate only becomes a PR when a benchmark shows it is
  **measurably, repeatably faster** (with no allocation regression) **and** an equivalence test
  shows the result is **identical to the original / native path** across normal and edge inputs
  (bit-exact for numeric ports). A faster answer that differs from Skia is a **rendering
  regression**, not an optimization — reject it.
- A quiet run is a **first-class success**: when you find nothing that clears the bar, emit exactly
  **one `noop`** summarizing what you scanned and why nothing qualified. A `noop` is the correct
  "nothing to do" signal — silence makes the run look incomplete.
- **Scope is managed C# only.** Optimize only code the repo owns — `binding/**` and `source/**`.
  Everything under `externals/skia/**` (including our C shim) is upstream Skia: not checked out,
  not buildable here (native work uses pre-built packages via `externals-download`), and out of
  scope. A win whose only real fix is native is **issue-only** (see 2.4).
- **Timebox the scan.** Do one focused pass over the optimization areas, pick the single
  strongest candidate with the clearest hot caller early, and stop. If nothing clears the bar,
  emit the `noop` and finish — do not launch open-ended explorations that may not return in budget.

All the methodology lives in the skill. Follow it exactly.

## Step 1 — Run the performance-fixer skill

Read and follow `.agents/skills/performance-fixer/SKILL.md` end-to-end.

**Every run does the same thing — "Scan and fix"** (skill Phase 1 → 2 → 3 → 4 → 5): hunt a new
opportunity, prove it faster, prove it identical, fix it, then file the finding issue + linked fix
PR. The only variation is dry-run — forced on `pull_request`, opt-in via the `dry_run` input (see
Guardrail 6).

**Focus area override:** `${{ github.event.inputs.focus_area }}` — if that shows a bare number
**0–4**, pass it to the skill's Phase 1.1 as the focus area and **skip the round-robin**
computation. If it is blank (schedule / PR / dispatch without the input), use the normal
time-based round-robin. This is only a testing knob to target a specific area on demand.

Persist intermediate state (notes, benchmark logs) under `/tmp/gh-aw/agent/`. Commit the benchmark
itself to `benchmarks/SkiaSharp.Benchmarks/Benchmarks/` and the equivalence test to `tests/Tests/` —
those are repo artifacts, not scratch. Each bash call is a fresh subshell — re-`cd` as needed.

## Step 2 — Guardrails (in addition to the skill's golden rules)

1. **What to emit — one finding per run.** For a confirmed, managed-C#-fixable optimization, emit
   the **linked pair**: one `create-issue` (the finding, with a `temporary_id` and the measured
   numbers) **and** one `create-pull-request` (the fix, whose body ends with
   `Fixes #<that temporary_id>` so merging auto-closes the issue) — see skill Phase 4. If the only
   real win is native / upstream (out of scope) → emit the **`create-issue` alone**. If nothing
   clears the bar → exactly one **`noop`**. Never finish a run with no safe output at all.
2. **De-dup first.** Run skill Phase 1.3 — skip any candidate already covered by an OPEN
   `[performance]` / `perf(...)` / `Optimize …` issue or PR on `mono/SkiaSharp` (e.g. #4241,
   #3489, #4182, #3033). A candidate whose only prior item is CLOSED may be re-filed.
3. **Two proofs before you open a PR.** Only open a PR when you have (a) a BenchmarkDotNet
   New-vs-Old result showing a real, repeatable speedup with no allocation regression (skill
   Phase 2) **and** (b) an equivalence test proving the result is identical to the original/native
   path, including edge inputs, and that the test catches a deliberately-wrong result (skill
   Phase 3). No speedup ⇒ nothing to fix. Any behaviour change ⇒ reject the fix.
4. **Managed-C# fixes only.** The fix must live in `binding/**` / `source/**`, bootstrapped with
   `dotnet cake --target=externals-download` (pre-built natives), benchmarked with
   `dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter ...`, and validated
   with `dotnet test`. If the strongest candidate's only real win is in native / upstream Skia
   (`externals/skia/**`, including the C shim), do **not** open an unvalidated PR — file a
   `[performance]` issue with the Phase 1–2 evidence and the proposed native change instead.
5. **Never change behaviour, never weaken/skip/mute/delete a test, and never edit `*.generated.cs`
   or anything under `externals/skia/**`.** SkiaSharp must render identically before and after; a
   faster wrong answer is a bug, not a fix.
6. **Dry run (forced on PRs).** Decide from the trigger `${{ github.event_name }}`:
   `pull_request` → **always DRY-RUN** (a PR editing this workflow/skill is a self-test);
   `workflow_dispatch` with the **dry_run** input `true` (shown as
   `${{ github.event.inputs.dry_run }}`) → **DRY-RUN**; `schedule` → real run. In DRY-RUN you do
   the full scan→prove→fix locally but you **MUST NOT** emit any `create-pull-request` or
   `create-issue` safe output under any circumstances. Instead, report your findings in the step
   summary **and** emit exactly one **`noop`** whose body is that same summary (what you scanned,
   the strongest candidate if any, its measured benchmark numbers, and — had this been a real run —
   the finding you would have filed and the fix PR you would have opened).
7. **AI attribution.** Every PR/issue body must clearly state it was produced by this agentic
   workflow + the `performance-fixer` skill, and include the honest numeric scope note (the actual
   measured ns/allocations/ratio on named hardware/TFM; empirically-measured vs statically-reasoned;
   ABI impact; any TFM/runtime caveat).
8. **Label the finding.** `tenet/performance` is applied automatically to the issue and PR. On top of
   that, add the matching **`perf/*` sub-type** (to both) chosen by the **dominant, measured driver of
   the win** — not mechanically by focus area. Use the canonical taxonomy in
   `.agents/skills/issue-triage/references/labels.md` (the same source `memory-leak-fixer` uses);
   include the label in the `labels` field of the `create-issue` / `create-pull-request` outputs
   (only the `perf/*` set + `tenet/performance` are allowed). Usually **one** sub-type — pick
   multiple only if genuinely distinct concerns apply. Quick guide: a removed P/Invoke / native
   marshalling → `perf/interop` (the taxonomy's example is literally "remove native interop in
   SKMatrix"); removed managed allocations / per-frame GC churn → `perf/allocations`; slow draw/GPU
   frame → `perf/rendering`; slow decode/encode/convert/readback → `perf/throughput`; slow init /
   first-use → `perf/startup`; a bounded-previously-unbounded cache → `perf/memory-leak`; binary/
   package size → `perf/size`. If a fix removes both an allocation *and* a P/Invoke, label the one the
   benchmark shows is the **primary** driver.

## Step 3 — Report

Append a short summary to `/tmp/gh-aw/agent/step-summary.md` (this file is symlinked to the run's
step summary — do **not** use `$GITHUB_STEP_SUMMARY`): the optimization area, the candidate (with
`file:line`), the benchmark result (New vs Old, ratio, allocations), the equivalence coverage, the
`perf/*` label you chose (and why), and the resulting issue + PR links — or "no convincing candidate
this run" for a quiet run.

Then make sure you have emitted the safe output(s) from Step 2.1: the **issue + PR pair** (or a
`create-issue` alone when the fix is out of scope), or — for a dry run or a quiet run — a single
`noop` carrying this same summary. Never finish the run with no safe output.
