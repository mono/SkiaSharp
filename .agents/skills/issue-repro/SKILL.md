---
name: issue-repro
description: >-
  Reproduce a SkiaSharp issue systematically and capture structured reproduction
  results. Handles bugs (verify reported behavior) and enhancements (confirm feature
  is missing). Produces schema-validated JSON with step-by-step commands, outputs,
  environment details, and conclusion.
  Triggers: "repro #123", "reproduce #123", "reproduce issue", "try to reproduce",
  "can you reproduce", "repro this bug", "create reproduction".
---

# Issue Reproduction

**Issue pipeline: Step 2 of 3 (Repro).** See [`documentation/dev/issue-pipeline.md`](../../../documentation/dev/issue-pipeline.md).

Systematically reproduce a SkiaSharp issue and produce structured, schema-validated reproduction JSON.

## ⛔ MANDATORY FIRST STEPS (do not skip)

1. Read THIS entire SKILL.md before any investigation
2. Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields and enums
3. Read [references/anti-patterns.md](references/anti-patterns.md) for critical rules

These 3 reads are REQUIRED. Do not proceed to Phase 1 until all three are loaded.

> **Quick flow:**
> 1. Load issue + any prior triage JSON
> 2. Read references: [schema-cheatsheet](references/schema-cheatsheet.md), [anti-patterns](references/anti-patterns.md)
> 3. Create brief plan (5-10 lines: strategy, platform, expected outcome)
> 4. Check environment: `docker info`, `dotnet --version`, available simulators
> 5. Build repro project and attempt reproduction
> 6. Test multiple SkiaSharp versions (3A reporter's → 3B latest → 3C main)
> 7. Generate JSON → validate → persist

### Data sources

**Primary source: GitHub itself.** Prefer `gh` CLI for repeatable issue/PR fetches. If the environment exposes GitHub MCP issue/PR lookup tools, those are also valid. Do not set up or depend on a cache worktree or cache branch for this skill.

```
Phase 0 (Setup) → Phase 1 (Fetch) → Phase 2 (Assess) → Phase 3 (Reproduce) → Phase 4 (JSON + Output) → Phase 5 (Validate) → Phase 6 (Persist & Present)
```

---

## Phase 0 — Setup

Run once per session:

```bash
pip3 install -r .agents/skills/issue-repro/scripts/requirements.txt --quiet
python3 --version  # Requires 3.9+
python3 -c "import jinja2; print('jinja2', jinja2.__version__)"
gh --version
gh auth status
```

If `gh` is unavailable or unauthenticated, use the GitHub MCP issue/PR retrieval tools available in the environment.

---

## Phase 1 — Fetch Issue

1. **Read the issue** (preferred):
   ```bash
   gh issue view {number} --repo mono/SkiaSharp \
     --json number,title,body,labels,comments,state,createdAt,updatedAt,closedAt,author,milestone \
     > /tmp/{number}.issue.json
   ```
   GitHub MCP issue/PR retrieval tools are equally valid if the environment exposes them.
2. **Triage boost** — if `output/ai/repos/mono-SkiaSharp/ai-triage/{number}.json` exists, extract `classification.platforms[]`, `evidence.bugSignals`, `analysis.nextQuestions[]`, and `output.actionability.suggestedReproPlatform` as **hints** (verify independently). The `suggestedReproPlatform` (`linux`|`macos`|`windows`) indicates which CI runner was selected for reproduction.

---

## Phase 2 — Assess & Plan

### 1. Classify

Read [references/bug-categories.md](references/bug-categories.md) to classify the issue type and determine the reproduction strategy. That file covers bugs (Sections 1–6), enhancements (Section 7), platform parity gaps (Section 8), and documentation issues (Section 9) — including which conclusion values and `layer` to use for each.

### 2. Extract reporter's version & TFM

- `{reporter_version}`: exact SkiaSharp NuGet version
- `{reporter_tfm}`: target framework (e.g., `net10.0`)
- `{reporter_code}`: reproduction code from the issue
- Reporter's platform/OS

If not stated, use the latest stable release. **.NET is forward-compatible** — `net8.0` libraries work on `net10.0` apps. Never say "doesn't support .NET X" for newer TFMs.

### 3. Environment check

**⚠️ Run `dotnet --info` in `/tmp/repro-{number}/`** (NOT the SkiaSharp repo, which has `global.json` pinning SDK 8.0). Record SDK version, workload versions, and runtime version.

Also check: Docker (`docker --version`), Playwright MCP tools, GPU availability, .NET workloads (`dotnet workload list`). Install missing workloads now — don't wait.

### 4. Determine reproduction platform

| Priority | Signals | Platform file |
|----------|---------|---------------|
| 1 | Blazor, WASM, WebAssembly, SKHtmlCanvas, browser error | [platform-wasm-blazor.md](references/platform-wasm-blazor.md) |
| 2 | WPF, WinForms, WinUI, UWP | [platform-windows-desktop.md](references/platform-windows-desktop.md) |
| 3 | iOS, Android, MAUI, Xamarin | [platform-mobile.md](references/platform-mobile.md) |
| 4 | Linux, Docker, container, NativeAssets.Linux | [platform-docker-linux.md](references/platform-docker-linux.md) |
| 5 | (none) | [platform-console.md](references/platform-console.md) |

All platform files fall back to `platform-console.md` for core SkiaSharp bugs.

**Tie-breaking:** If multiple platform signals (e.g., "WASM + WPF"), use the highest priority. If reporter says "works on X, fails on Y", reproduce on Y first, test X in Phase 3D.

**Read the selected platform file.** Follow its Create → Build → Run → Verify steps, substituting `{reporter_version}`, `{reporter_tfm}`, and `{reporter_code}`.

### 5. Plan

Output a brief plan before executing (5-10 lines: what platform, what version, what approach).

---

## Key Rules (read before Phase 3)

Read [references/anti-patterns.md](references/anti-patterns.md) for the full list. Critical rules:

1. **Source code investigation.** Stop at "did it reproduce." Root cause is the `issue-fix` skill's job.
2. **Editorial judgment in conclusion.** If the reported behavior occurred, it's `reproduced` — even if by-design.
3. **Stopping at build success.** Many bugs manifest at RUNTIME. Build ≠ runtime.
4. **Stale build artifacts.** Fresh project dirs or `rm -rf bin/ obj/` between versions.
5. **Honesty over completion.** `not-reproduced` and `needs-platform` are VALID SUCCESS conclusions. Reporting inability to reproduce is correct behavior, NOT failure. NEVER invent output you did not observe from an actual command execution.
6. **NEVER modify product source.** Do not edit files in `binding/`, `externals/`, `samples/`, `source/`, `tests/`, `utils/`, or any other product source during reproduction. Repro creates NEW test projects in `/tmp/repro-{number}/` only. If you find yourself editing SkiaSharp source, you have crossed into fix territory — stop.
7. **NEVER use `store_memory`.** Reproduction produces JSON artifacts, not memories. Storing unverified observations as permanent facts pollutes all future sessions.
8. **NEVER skip validation.** You MUST run `validate-repro.py` and see ✅ before persisting. Mentally checking fields is not validation. If the script isn't run, the reproduction is invalid.

**Intermittent bugs:** If results are inconsistent, run 3–5 times. Reproduced ≥1 time → `reproduced` with note "Intermittent: X/Y runs". Never reproduced after 5 → `not-reproduced`.

**Effort budget:** Phases 1–2: ~5 min. Phase 3A: ~15–20 min. Phases 3B–3D: ~10–15 min. Total: ~30–50 min. If stuck after 3+ substantially different approaches, conclude with what you have.

---

> **Pre-flight — confirm before reproducing:**
>
> - [ ] Issue data loaded (cache JSON or GitHub API)
> - [ ] Prior triage JSON loaded (if exists in `ai-triage/`)
> - [ ] Read [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for required fields and enums
> - [ ] Read [references/anti-patterns.md](references/anti-patterns.md) — at least the critical rules
> - [ ] Environment checked: `docker info` (if needed), `dotnet --version`, `xcodebuild -showsdks` (if iOS)
> - [ ] Created a brief plan (5-10 lines: reproduction strategy, which platform, what you expect)
> - [ ] Never use `sudo` — if a command requires it, find an alternative approach

## Phase 3 — Reproduce

> **Overview — you will test up to 4 configurations:**
> - **3A:** Reporter's version on primary platform *(always)*
> - **3B:** Latest stable release *(always)*
> - **3C:** Main branch source *(MANDATORY if 3B still reproduced)*
> - **3D:** Cross-platform verification *(conditional — see table below)*
>
> **🛑 MINIMUM 2 VERSIONS REQUIRED.** You must test at least the reporter's version (3A) AND latest stable (3B). Single-version reproductions are incomplete and will fail schema validation. This applies to ALL conclusion types — bugs (`reproduced`/`not-reproduced`) AND enhancements (`confirmed`/`not-confirmed`). For enhancements: a feature may exist in one version but not another, or may have been removed. Version testing reveals this.

### 3A. Reproduce with reporter's version

Follow the platform file from Phase 2.4. For each step, capture:

| Field | Limit |
|-------|-------|
| `command` | Exact command (redact paths) |
| `exitCode` | 0=success, non-zero=failure |
| `output` | 2KB success, 4KB failure |
| `filesCreated` | Filename + source code content for repro files |
| `layer` | `setup` / `csharp` / `c-api` / `native` / `deployment` / `investigation` |
| `result` | `success` / `failure` / `wrong-output` / `skip` |

**Step `result` = what actually happened** (technical outcome), not whether it was expected. A build that fails is `result: "failure"` even if that confirms the bug. See [references/anti-patterns.md](references/anti-patterns.md) for details.

> **Note:** Use `layer: "investigation"` for source-code analysis steps in enhancement confirmations (grep, file reading). See Phase 2.1 for the full enhancement flow.

**Push hard.** Don't bail early. Only conclude `not-reproduced` after genuinely exhausting approaches.

### 3B. Test on latest release

> **⚠️ Clean build required:** Create a fresh project directory per version (`/tmp/repro-{number}-latest/`) or `rm -rf bin/ obj/` before building. Never just `sed` the version — stale native binaries produce unreliable results. See [references/anti-patterns.md](references/anti-patterns.md) #7.

Use the same platform strategy from 3A with the latest stable SkiaSharp. Record in `versionResults`.

### 3C. Test on main branch (if reproduced on latest)

> **🛑 Do NOT skip when reproduced on latest.** If the bug reproduces on the latest stable release, testing main is MANDATORY — it tells us whether a fix exists but hasn't been released.

1. Bootstrap: `[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download`
2. **Build & run the platform-appropriate sample** under `samples/Basic/<platform>/`. Each platform file has a "Main Source Testing (Phase 3C)" section — follow it.
3. Record result. If fixed on main but not released, note the version gap.

> **Clean up:** Revert sample file changes with `git checkout -- samples/`.

### 3D. Cross-platform verification (conditional)

| Primary result | Run 3D? |
|----------------|---------|
| `reproduced` (platform-specific) | **Yes** — test alternative platform |
| `not-reproduced` + reporter on different platform | **Yes** |
| `reproduced` (pure API bug, no platform signals) | **Skip** — note why |
| `not-reproduced` + same platform as reporter | No |

**Time cap:** 5 minutes. **Default alternative:** Docker Linux x64.

| Primary | Best alternative |
|---------|-----------------|
| Console macOS | Docker Linux x64 |
| WASM/Blazor | Console on host |
| Docker Linux | Console on host |
| Windows (reported) | Console + Docker Linux |

Test reporter's version only. Derive `scope`: reproduced on ≥2 platforms → `"universal"`, primary only → `"platform-specific/{platform}"`, skipped → `"unknown"`.

**For `confirmed`/`not-confirmed` conclusions:** Derive `scope` from your investigation — if the gap exists in ALL platform views → `"universal"`, if the gap is specific to one platform's view (e.g., Blazor only) → `"platform-specific/{platform}"`. Always set `scope` for confirmed conclusions (schema requires it).

### Simulation Strategy (last resort)

Some bugs live in framework-specific code (MAUI views, WPF handlers, Uno controls) that can't be run in a console app. Use this **escalation order** — only move to the next level if the previous one fails:

1. **Direct run** — Build and run the actual project type (MAUI, WPF, etc.)
2. **Real platform** — Use a simulator/emulator (iOS Simulator, Android Emulator)
3. **Automation** — Use Appium or Playwright to drive UI testing
4. **Docker** — Run in a container (Linux-only scenarios)
5. **Simulate** — Extract the logic into a console app and model the inputs

**When to simulate:** Only when steps 1-4 are impossible (wrong OS, no SDK, no device). Example: a WPF bug on a macOS host with no Windows VM.

**How to simulate:** Extract the suspect code path into a standalone console app. Model the framework inputs (e.g., resize events, binding updates) and verify the logic path triggers the bug.

**Recording:** Set `reproProject.type: "simulation"` in the JSON. The triage's `classifiedPlatform` vs repro's `type: "simulation"` shows the gap clearly.

**Example:** Opus reproduced SKXamlCanvas NRE (#3430) by extracting the resize handler logic into a console app, feeding 73 dimension combinations, and confirming 100% NRE rate when width/height were 0.

---

## Phase 4 — Generate JSON

### 1. Choose conclusion

Read [references/conclusion-guide.md](references/conclusion-guide.md). Key question: did the reported claim hold true?

| Conclusion | When | Issue Types |
|------------|------|-------------|
| `reproduced` | Reported behavior occurred, including wrong/incomplete output (even if by-design) | Bugs |
| `not-reproduced` | Reported behavior did not occur | Bugs |
| `confirmed` | Reporter's claim verified (feature IS missing, docs ARE wrong) | Enhancements, features, docs |
| `not-confirmed` | Reporter's claim not verified (feature exists, docs are correct) | Enhancements, features, docs |
| `needs-platform` / `needs-hardware` | Requires unavailable platform/hardware | Any |
| `partial` / `inconclusive` | Partial or ambiguous results | Any |

### 2. Generate JSON

Write to `/tmp/{number}.json`. Use this exact literal path structure — do NOT substitute `$TMPDIR` or any other variable. Do NOT use `mkdir` — write directly to `/tmp/`.

Schema: [references/repro-schema.json](references/repro-schema.json). See [references/repro-examples.md](references/repro-examples.md) for full worked examples.

**Key rules** (schema enforces the rest):
- **Optional fields: OMIT entirely** — do NOT set to `null`
- `environment.dotnetSdkVersion`: exact SDK from `dotnet --info`. Include `wasmToolsVersion` for WASM.
- `versionResults`: include `platform` field (e.g., `"host-macos-arm64"`, `"docker-linux-x64"`)
- Redact paths (`/Users/{name}/` → `$HOME/`), tokens, credentials.

### 3. Feedback (when triage was consumed)

If reproduction contradicts triage, record in `feedback.corrections[]`:
```json
{ "source": "triage", "topic": "classification", "upstream": "...", "corrected": "..." }
```

### 4. Generate output (required for definitive conclusions)

When conclusion is `reproduced`, `not-reproduced`, `confirmed`, or `not-confirmed`, generate the `output` object with actionability, actions, and a proposed response. Skip for blocked conclusions (`needs-platform`, `needs-hardware`, `partial`, `inconclusive`).

#### Choosing suggestedAction

| Scenario | suggestedAction | Confidence |
|----------|----------------|------------|
| Reproduced on all versions including latest/main | `needs-investigation` | 0.90+ |
| Reproduced on reporter's version, fixed on latest | `close-as-fixed` | 0.85+ |
| Reproduced on reporter's version, fixed on main (unreleased) | `keep-open` | 0.80+ |
| Not reproduced — likely environment/config issue | `request-info` | 0.70+ |
| Not reproduced — works on all tested versions | `close-as-fixed` | 0.75+ |
| Wrong output confirmed | `needs-investigation` | 0.85+ |
| Reproduced but appears working-as-designed | `close-with-docs` | 0.70+ |
| Confirmed — feature/docs gap verified | `needs-investigation` | 0.85+ |
| Not confirmed — feature/docs actually exist | `close-with-docs` | 0.75+ |

#### Writing proposedResponse

See [references/response-guidelines.md](references/response-guidelines.md) for tone, evidence requirements, status thresholds (`ready`/`needs-human-edit`/`do-not-post`), and conclusion-specific templates.

#### Workarounds

If reproduction testing reveals a workaround (version upgrade, config change, workload install, API alternative), add to `output.workarounds[]` as simple strings and include in the proposed response body.

#### Missing info

When conclusion is `not-reproduced` or the response asks for more details, populate `output.missingInfo[]` with specific items needed (e.g., "Exact .NET SDK version", "Full exception stack trace", "Minimal reproduction project"). Reference these in `proposedResponse.body`.

#### Actions

Use the same action types as triage. Common repro actions:

| Action | When | Risk |
|--------|------|------|
| `update-labels` | Add platform labels confirmed by repro, remove incorrect ones | low |
| `close-issue` | Bug fixed in latest + reporter can upgrade | medium |
| `set-milestone` | Bug confirmed, target a release | low |
| `link-related` | Discovered related issue during repro | low |

---

## Phase 5 — Validate

> **🛑 PHASE GATE: You CANNOT persist without passing validation.**
> **Skipping validation = INVALID reproduction. The task is incomplete.**

### 1. Validate (MANDATORY — run first)

```bash
python3 .agents/skills/issue-repro/scripts/validate-repro.py /tmp/{number}.json
```

- **Exit 0** = ✅ valid → proceed to Phase 6
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> **⚠️ NEVER hand-roll your own validation. NEVER assume it passes. RUN THE SCRIPT.**
> **If you have not seen ✅ from the validator, DO NOT proceed to Phase 6.**

> **🛑 PHASE GATE: Validator MUST have printed ✅ before proceeding to Phase 6.**

## Phase 6 — Persist & Present

### 1. Persist (only after validator prints ✅)

Copy the validated JSON to `output/ai/` for collection, then render companion Markdown and HTML reports.

```bash
python3 .agents/skills/issue-repro/scripts/persist-repro.py /tmp/{number}.json
```

This produces:

- `output/ai/repos/mono-SkiaSharp/ai-repro/{number}.json`
- `output/ai/repos/mono-SkiaSharp/ai-repro/{number}.md`
- `output/ai/repos/mono-SkiaSharp/ai-repro/{number}.html`

### 2. Present summary

```
✅ Reproduction: ai-repro/{number}.json
✅ Report: ai-repro/{number}.md
✅ Viewer: ai-repro/{number}.html

Conclusion:  reproduced
Steps:       5 (3 success, 1 failure, 1 skip)
Environment: macOS arm64, SDK 10.0.102

Version results:
  SkiaSharp 2.88.9 (reporter): ❌ REPRODUCED
  SkiaSharp 3.116.1 (latest):  ❌ REPRODUCED
  main (source):               ✅ not-reproduced
```

---

## Scripts

- **`scripts/validate-repro.py <repro.json>`** — Validate against schema + step/conclusion consistency
- **`scripts/persist-repro.py <repro.json>`** — Copy validated JSON to `output/ai/` and invoke renderer
- **`scripts/render-repro-report.py <repro.json>`** — Render validated repro JSON → `.md` + `.html` (uses Jinja2 template for markdown)
- **`scripts/repro-report.md.jinja2`** — Jinja2 template for the Markdown report
- **`scripts/viewer.html`** — Bootstrap 5 HTML template for the interactive viewer
