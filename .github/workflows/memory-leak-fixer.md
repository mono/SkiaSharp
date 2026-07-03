---
# =============================================================================
# Fixer - Memory Leak — SkiaSharp
#
# Periodic AI-driven workflow that SCANS SkiaSharp's managed C# for a native
# ownership / disposal memory leak, PROVES it with a red→green regression test,
# FIXES it, and opens a DRAFT PR. Adapted from dotnet/maui's leak scanner+fixer
# idea to SkiaSharp's real leak family (undisposed SKObject handles, wrong
# `owns:` flags, same-instance double-dispose, unremoved view/handler
# subscriptions, `fixed`-pointer lifetime). Scope: managed C# only — the native
# Skia under externals/skia/** is upstream and out of scope.
#
# All the domain knowledge lives in the reusable skill
# `.agents/skills/memory-leak-fixer/SKILL.md`; this file is the schedule + the
# guardrails (one action per run, de-dup, validate-before-PR).
# =============================================================================
description: "Scan SkiaSharp's managed C# for a native ownership/disposal memory leak, prove it with a red→green test, fix it, and open a draft PR."

environment: gh-aw-agents

# -- Engine ------------------------------------------------------------
# Leak reasoning (ownership tracing + a correct minimal fix) is hard and
# benefits from the stronger model, matching auto-skia-sync's choice.
engine:
  id: copilot
  model: claude-opus-4.8

# -- Triggers ----------------------------------------------------------
# Every 12h + manual + PR-driven self-test. Manual dispatch may pin a specific
# reported leak (`issue_number`) or do everything-but-open-nothing (`dry_run`).
# A `pull_request` that edits THIS workflow or its skill re-runs the whole
# pipeline in FORCED DRY-RUN (see Step 2.6) so we can iterate on the prompt/skill
# and watch the run without ever opening a real PR/issue.
on:
  schedule: every 12h
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Fix a specific reported [memory-leak] issue (leave blank to scan for a new leak)."
        required: false
        type: string
      dry_run:
        description: "Do the full scan→prove→fix locally but do NOT open a PR/issue."
        required: false
        default: false
        type: boolean
  pull_request:
    paths:
      - ".github/workflows/memory-leak-fixer.md"
      - ".github/workflows/memory-leak-fixer.lock.yml"
      - ".agents/skills/memory-leak-fixer/**"

# Only ever run on the canonical repo — never on forks.
if: |
  github.repository == 'mono/SkiaSharp'

# Give the skill's Phase 5 report a writable step-summary sink (mirrors the
# convention used by auto-triage).
steps:
  - name: Redirect step summary into agent-writable directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md

concurrency:
  group: "memory-leak-fixer"
  cancel-in-progress: false

timeout-minutes: 120

# create-pull-request is commit-based and needs full history for its branch ops
# (and for the skill's git-commit-count focus rotation). This skill is managed-C#
# only and builds against pre-built natives (externals-download), so the skia
# source submodule is NOT needed — a plain checkout keeps runs fast.
checkout:
  - fetch-depth: 0

permissions:
  contents: read
  issues: read
  pull-requests: read

# -- Agent tools -----------------------------------------------------
# The agent reads source, writes a throwaway probe + a regression test, edits
# managed C# to fix, builds, tests, then commits. `dotnet`/`pwsh`/`git` are the
# work tools; `gh` powers de-dup queries. No `sed`/`awk` in-place edits — use the
# edit tool so changes are reviewable.
tools:
  github:
    toolsets: [issues, pull_requests, search]
    allowed-repos: ["mono/skiasharp"]
    min-integrity: none
  edit:
  bash: ["dotnet", "pwsh", "git", "gh", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "cp", "mv", "rm"]

# -- Network allowlist -----------------------------------------------
# nuget.org for the shipped-package leak probe; the SkiaSharp-CI Azure DevOps
# feed + blob storage for `externals-download` pre-built natives; the `dotnet`
# ecosystem covers pkgs.dev.azure.com and the SDK/restore hosts.
network:
  allowed:
    - defaults
    - github
    - dotnet
    - "*.blob.core.windows.net"

# -- Safe outputs ------------------------------------------------------
# Primary: a DRAFT fix PR (managed C#). Fallback: a [memory-leak] issue when the
# only correct fix is in native / upstream Skia (out of scope here). At most ONE
# of the two per run (enforced in the prompt). Quiet runs create nothing.
safe-outputs:
  create-pull-request:
    title-prefix: "[memory-leak] "
    labels: [agentic-workflows]
    draft: true
    allowed-base-branches: [main]
  create-issue:
    labels: [agentic-workflows]
    allowed-labels: [agentic-workflows]
    max: 1
  noop:
    report-as-issue: false
---

# Fixer - Memory Leak

You **scan** SkiaSharp's **managed C#** for one native ownership / disposal memory leak,
**prove** it with a red→green regression test, **fix** it, and open **one draft PR** — or,
when the only correct fix is in native / upstream Skia (out of scope here), file **one**
`[memory-leak]` issue instead. **At most one action per run.**

**Read this first — set your expectations correctly:**

- SkiaSharp is a **mature, heavily-hardened** codebase. There is **no planted/seeded/injected
  bug** to find. Most runs will (and should) find **nothing convincing** — that is the normal,
  expected, successful outcome, **not** a failure and **not** a puzzle with a guaranteed answer.
  Do **not** invent a leak, do **not** rationalize well-documented, deliberately-hardened code
  as "decoys," and do **not** lower your evidence bar just to produce a result.
- A quiet run is a **first-class success**: when you find nothing that clears the bar, emit
  exactly **one `noop`** safe output summarizing what you scanned and why nothing qualified.
  A `noop` is the correct "nothing to do" signal — silence makes the run look incomplete.
- **Scope is managed C# only.** Fix only code SkiaSharp owns — `binding/**` and `source/**`.
  Everything under `externals/skia/**` (including our C shim) is upstream Skia: not checked
  out, not buildable here (native tests use pre-built packages via `externals-download`), and
  out of scope. A leak whose only correct fix is native is **issue-only** (see 2.4).
- **Timebox the scan.** Do one focused pass over the leak families, pick the single strongest
  candidate early, and stop. If nothing clears the bar in that pass, emit the `noop` and finish
  — do not launch open-ended sub-agent explorations that may not return within the budget.

All the methodology lives in the skill. Follow it exactly.

## Step 1 — Run the memory-leak-fixer skill

Read and follow `.agents/skills/memory-leak-fixer/SKILL.md` end-to-end.

- If `${{ github.event.inputs.issue_number }}` is set, **fix that specific reported leak**
  (skill Mode = "Fix a known/reported leak"; consume the issue's retention path, then still
  enforce Phase 3 red→green).
- Otherwise **scan and fix** (skill Mode = "Scan and fix"): Phase 1 → 2 → 3 → 4 → 5.

Persist all intermediate state (the `/tmp/leakprobe` project, notes) under `/tmp/gh-aw/agent/`.
Each bash call is a fresh subshell — re-`cd` as needed.

## Step 2 — Guardrails (in addition to the skill's golden rules)

1. **One action per run.** Emit either one `create-pull-request` **or** one `create-issue`
   **or** one `noop`, never more than one. When you have nothing to ship, the action is a
   single `noop` — never finish with no safe output at all (that makes the run look incomplete).
2. **De-dup first.** Run skill Phase 1.3 — skip any candidate already covered by an OPEN
   `[memory-leak]` issue or PR on `mono/SkiaSharp`. A candidate whose only prior item is
   CLOSED may be re-filed.
3. **Validate before you open a PR.** Only open a PR when you have demonstrated the
   regression test **fails without the fix and passes with it** (skill Phase 3, both
   directions). No red→green ⇒ no PR.
4. **Managed-C# fixes only.** The fix must live in `binding/**` / `source/**`, bootstrapped
   with `dotnet cake --target=externals-download` (pre-built natives) and validated with
   `dotnet test`. If the strongest candidate's only correct fix is in native / upstream Skia
   (`externals/skia/**`, including the C shim), do **not** open an unvalidated PR — file a
   `[memory-leak]` issue with the Phase 1–2 evidence and the proposed fix instead.
5. **Never weaken, skip, mute, or delete a test, and never edit `*.generated.cs` or anything
   under `externals/skia/**`** (upstream Skia + the C shim are out of scope for this skill).
6. **Dry run (forced on PRs).** You are in **DRY-RUN** whenever either
   `${{ github.event.inputs.dry_run }}` is `true` **or** `${{ github.event_name }}` is
   `pull_request` (a PR editing this workflow/skill is a self-test). In DRY-RUN you do the
   full scan→prove→fix locally but you **MUST NOT** emit any `create-pull-request` or
   `create-issue` safe output under any circumstances. Instead, report your findings in the
   step summary **and** emit exactly one **`noop`** whose body is that same summary (what you
   scanned, the strongest candidate if any, and — had this been a real run — whether you would
   have opened a PR or filed an issue). Never finish a dry run with no safe output.
7. **AI attribution.** Every PR/issue body must clearly state it was produced by this
   agentic workflow + the `memory-leak-fixer` skill, and include an honest scope note
   (framework bug vs footgun; empirically-proven vs statically-reasoned; ABI impact).

## Step 3 — Report

Append a short summary to `/tmp/gh-aw/agent/step-summary.md` (this file is symlinked to the
run's step summary — do **not** use `$GITHUB_STEP_SUMMARY`): the leak family, the candidate
(with `file:line`), the proof result (alive/collected counts or red→green status), and the
resulting PR/issue link — or "no convincing candidate this run" for a quiet run.

Then make sure you have emitted exactly one safe output (Step 2.1): a `create-pull-request`, a
`create-issue`, or — for a dry run or a quiet run — a single `noop` carrying this same summary.
Never finish the run with no safe output.
