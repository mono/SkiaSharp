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

**Issue pipeline: Step 2 of 3 (Repro).** See [`documentation/issue-pipeline.md`](../../../documentation/issue-pipeline.md).

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

```
Phase 1 (Fetch) → Phase 2 (Assess) → Phase 3 (Reproduce) → Phase 4 (JSON + Output) → Phase 5 (Validate) → Phase 6 (Persist & Present)
```

---

## Phase 1 — Fetch Issue

1. **Read the issue** (preferred):
   ```bash
   CACHE=".data-cache/repos/mono-SkiaSharp"
   [ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
   git -C .data-cache pull --rebase origin docs-data-cache
   cat $CACHE/github/items/{number}.json
   ```
   **Fallback:** `gh issue view {number} --repo mono/SkiaSharp --json title,body,labels,comments,state,createdAt,closedAt,author`
2. **Triage boost** — if `$CACHE/ai-triage/{number}.json` exists, extract `classification.platforms[]`, `evidence.bugSignals`, `analysis.nextQuestions[]`, and `output.actionability.suggestedReproPlatform` as **hints** (verify independently). The `suggestedReproPlatform` (`linux`|`macos`|`windows`) indicates which CI runner was selected for reproduction.

---

## Phase 2 — Assess & Plan

### 1. Classify

Classify the issue type and read the matching category file for reproduction strategy:

| If the issue describes... | Category file | Conclusion type |
|--------------------------|---------------|-----------------|
| Wrong output, crash, exception, broken behavior | [category-bugs.md](references/category-bugs.md) | `reproduced` / `not-reproduced` |
| Missing feature, new API request, docs gap | [category-features.md](references/category-features.md) | `confirmed` / `not-confirmed` |
| Slow rendering, low FPS, performance regression | [category-performance.md](references/category-performance.md) | either (see file) |

Read the matching file for signals, goal, strategy, and pitfalls specific to the issue type.

### 2. Extract reporter's version & TFM

- `{reporter_version}`: exact SkiaSharp NuGet version
- `{reporter_tfm}`: target framework (e.g., `net10.0`)
- `{reporter_code}`: reproduction code from the issue
- Reporter's platform/OS

If not stated, use the latest stable release. **.NET is forward-compatible** — `net8.0` libraries work on `net10.0` apps. Never say "doesn't support .NET X" for newer TFMs.

### 3. Environment check

**⚠️ Run `dotnet --info` in `/tmp/skiasharp/repro/{number}/`** (NOT the SkiaSharp repo, which has `global.json` pinning SDK 8.0). Record SDK version, workload versions, and runtime version.

Also check: Docker (`docker --version`), Playwright MCP tools, GPU availability, .NET workloads (`dotnet workload list`). Install missing workloads now — don't wait.

### 4. Determine reproduction platform

| Priority | Signals | Platform file |
|----------|---------|---------------|
| 1 | Blazor, WASM, WebAssembly, SKHtmlCanvas, browser error | [platform-wasm-blazor.md](references/platform-wasm-blazor.md) |
| 2 | WPF, WinForms, WinUI, UWP | [platform-windows-desktop.md](references/platform-windows-desktop.md) |
| 3 | macOS, Mac Catalyst, SKGLView, SKMetalView, NSOpenGLView, MTKView | [platform-macos.md](references/platform-macos.md) |
| 4 | iOS, Android, MAUI, Xamarin | [platform-mobile.md](references/platform-mobile.md) |
| 5 | Linux, Docker, container, NativeAssets.Linux | [platform-docker-linux.md](references/platform-docker-linux.md) |
| 6 | (none) | [platform-console.md](references/platform-console.md) |

Core SkiaSharp bugs (no platform signals) use `platform-console.md`. **Exception:** GPU/view performance bugs must use the GPU platform file — console apps test a different code path.

**Tie-breaking:** If multiple platform signals (e.g., "WASM + WPF"), use the highest priority. If reporter says "works on X, fails on Y", reproduce on Y first, test X in Phase 3D.

**Read the selected platform file.** Follow its Create → Build → Run → Verify steps, substituting `{reporter_version}`, `{reporter_tfm}`, and `{reporter_code}`.

### 5. Plan

Output a brief plan before executing (5-10 lines: what platform, what version, what approach).

---

## Key Rules (read before Phase 3)

Read [references/anti-patterns.md](references/anti-patterns.md) for the full list. Critical rules:

1. **Source code investigation.** Stop at "did it reproduce." Root cause is the `issue-fix` skill's job.
2. **Editorial judgment in conclusion.** If the reported behavior occurred, it's `reproduced` — even if by-design.
3. **Build ≠ runtime.** Many bugs manifest at runtime. For WASM: serve and check browser console. For rendering: inspect the output. Build success is a step, not a conclusion.
4. **Stale build artifacts.** Fresh project dirs or `rm -rf bin/ obj/` between versions. Stale native binaries look like version differences but are caching bugs.
5. **Honesty over completion.** `not-reproduced` and `needs-platform` are VALID SUCCESS conclusions. Reporting inability to reproduce is correct behavior, NOT failure. NEVER invent output you did not observe from an actual command execution.
6. **NEVER modify product source.** Do not edit files in `binding/`, `externals/`, `samples/`, `source/`, `tests/`, `utils/`, or any other product source during reproduction. Repro creates NEW test projects in `/tmp/skiasharp/repro/` only. If you find yourself editing SkiaSharp source, you have crossed into fix territory — stop.
7. **Run the reporter's code first.** Run what they provide before creating your own repro. Run BOTH sides of any comparison — measure numbers yourself, never use the reporter's claimed numbers.
8. **Reproduce, don't hypothesize.** Let data speak, not triage theories. If you're building something the reporter didn't describe, you're experimenting — stop and go back to their code.
9. **Match the reporter's conditions.** Same rendering backend (GL→GL, Metal→Metal), same data, same API, same platform. Do NOT switch backends or create a simpler test with a different rendering mode to "isolate variables."

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

> **Overview — 5 steps in order:**
> - **3A:** Run the reporter's baseline, if provided *(native C++, working version, etc.)*
> - **3B:** Run the reporter's broken code *(always)*
> - **3C:** Create YOUR minimal repro *(always — this is the core deliverable)*
> - **3D:** Test your repro across versions *(reporter's → latest → main)*
> - **3E:** Cross-platform verification *(conditional)*
>
> **🛑 MINIMUM 2 VERSIONS REQUIRED in 3D.** You must test at least the reporter's version AND
> latest stable. Single-version reproductions are incomplete and will fail schema validation.

For each step, capture:

| Field | Limit |
|-------|-------|
| `command` | Exact command (redact paths) |
| `exitCode` | 0=success, non-zero=failure |
| `output` | 2KB success, 4KB failure |
| `filesCreated` | Filename + source code content for repro files |
| `layer` | `setup` / `csharp` / `c-api` / `native` / `deployment` / `investigation` |
| `result` | `success` / `failure` / `wrong-output` / `skip` |

**Step `result` = what actually happened** (technical outcome), not whether it was expected. A build that fails is `result: "failure"` even if that confirms the bug.

### 3A. Run the reporter's baseline (if provided)

Some reporters provide both sides of a comparison — working code alongside broken code. Examples:
- Native C++ that renders correctly, alongside C# that renders incorrectly
- Native C++ at 120fps, alongside SkiaSharp at 8fps
- An older SkiaSharp version that works, alongside a newer one that doesn't
- A different platform where it works, alongside one where it fails

If the reporter provides a baseline, **build and run it yourself** (Rule 7). Record your own
measurements. Don't use the reporter's claimed numbers. If the baseline requires building native
code (GN, CMake, Ninja), follow the instructions — these builds may be slow (10-20 min). Be
patient, retry on network timeouts.

If no baseline is provided, skip to 3B.

### 3B. Run the reporter's broken code

Run the reporter's code or project as-is with their exact version and configuration. This
verifies what they described. Record the observable: crash, wrong output, slow FPS, build error,
missing feature — whatever the issue reports.

**Push hard.** Don't bail early. Only conclude `not-reproduced` after genuinely exhausting approaches.

> **Note:** Use `layer: "investigation"` for source-code analysis steps in enhancement
> confirmations (grep, file reading).

### 3C. Create YOUR minimal repro

This is the core deliverable. The reporter's code is evidence, but you can't trust it — they may
have added framework overhead, debug settings, or other variables. Create your own minimal app
that reproduces the issue with the least possible code.

**Use the platform file from Phase 2.4** to create the project. The platform determines the
app type:
- Core API bug (SKBitmap, SKCanvas, SKPath) → console app ([platform-console.md](references/platform-console.md))
- GPU rendering bug → GPU app matching reporter's backend: SKGLView for OpenGL, SKMetalView for Metal ([platform-macos.md](references/platform-macos.md))
- WASM bug → Blazor app ([platform-wasm-blazor.md](references/platform-wasm-blazor.md))
- Build/deployment bug → your repro IS the build attempt (same as 3B)

If the reporter uses a framework (Avalonia, MAUI, WPF) but the bug might be in core SkiaSharp,
strip the framework. Your repro should use SkiaSharp directly — the platform's native view, not
a framework wrapper. This isolates whether the bug is in SkiaSharp or the framework integration.

> **Performance bugs:** Read [category-performance.md](references/category-performance.md) for
> additional guidance — instrument with per-phase timing, disable VSync, and see the platform
> file for backend-specific templates (SKGLView, SKMetalView).

### 3D. Test your repro across versions

Run YOUR minimal repro (from 3C) with multiple SkiaSharp versions:

1. **Reporter's version** — does your minimal repro show the same issue?
2. **Latest stable release** — is it fixed? Use a fresh project directory or `rm -rf bin/ obj/`
   between versions (Key Rule 4).
3. **Main branch (MANDATORY if reproduced on latest)** — is a fix unreleased?
   - Bootstrap: `[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download`
   - Build & run the platform-appropriate sample under `samples/Basic/<platform>/`. Each platform
     file has a "Main Source Testing (Phase 3C)" section — follow it.
   - Clean up: `git checkout -- samples/`

Record each version in `versionResults`.

### 3E. Cross-platform verification (conditional)

| Primary result | Run 3E? |
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

Write to `/tmp/skiasharp/repro/{number}.json`. Use the exact literal path `/tmp/skiasharp/repro/` — do NOT substitute `$TMPDIR` or any other variable. Run `mkdir -p /tmp/skiasharp/repro` first if needed.

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
# Try pwsh first, fall back to python3
pwsh .github/skills/issue-repro/scripts/validate-repro.ps1 /tmp/skiasharp/repro/{number}.json \
  || python3 .github/skills/issue-repro/scripts/validate-repro.py /tmp/skiasharp/repro/{number}.json
```

- **Exit 0** = ✅ valid → proceed to Phase 6
- **Exit 1** = ❌ fix the errors listed in the output, then re-run. Repeat up to 3 times.
- **Exit 2** = fatal error, stop and report

> **⚠️ NEVER hand-roll your own validation. NEVER assume it passes. RUN THE SCRIPT.**
> **If you have not seen ✅ from the validator, DO NOT proceed to Phase 6.**

> **🛑 PHASE GATE: Validator MUST have printed ✅ before proceeding to Phase 6.**

## Phase 6 — Persist & Present

### 1. Persist (only after validator prints ✅)

> **🛑 MANDATORY: Use the persist script. NEVER manually `cp`, `git add`, `git commit`, or `git push`.**
> The script handles copying, committing, and pushing with retry logic. Manual git commands
> will leave unpushed commits that are lost when the runner shuts down.

```bash
pwsh .github/skills/issue-repro/scripts/persist-repro.ps1 /tmp/skiasharp/repro/{number}.json
```

This copies the JSON to data-cache and handles git commit + push automatically (skips in benchmark mode).

### 2. Present summary

```
✅ Reproduction: ai-repro/{number}.json

Conclusion:  reproduced
Steps:       5 (3 success, 1 failure, 1 skip)
Environment: macOS arm64, SDK 10.0.102

Version results:
  SkiaSharp 2.88.9 (reporter): ❌ REPRODUCED
  SkiaSharp 3.116.1 (latest):  ❌ REPRODUCED
  main (source):               ✅ not-reproduced
```
