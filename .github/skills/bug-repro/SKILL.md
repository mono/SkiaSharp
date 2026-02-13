---
name: bug-repro
description: >-
  Reproduce a SkiaSharp bug systematically and capture structured reproduction
  results. Produces schema-validated JSON with step-by-step commands, outputs,
  environment details, and conclusion.
  Triggers: "repro #123", "reproduce #123", "reproduce issue", "try to reproduce",
  "can you reproduce", "repro this bug", "create reproduction".
---

# Bug Reproduction

**Bug pipeline: Step 2 of 3 (Repro).** See [`documentation/bug-pipeline.md`](../../../documentation/bug-pipeline.md).

Systematically reproduce a SkiaSharp bug and produce structured, schema-validated reproduction JSON.

```
Phase 1 (Setup & Fetch) → Phase 2 (Assess & Feasibility) → Phase 3 (Bootstrap & Reproduce) → Phase 4 (Generate JSON) → Phase 5 (Validate & Persist)
```

---

## Phase 1 — Setup & Fetch Issue

### 1. Set up data cache

Run once per session:

```bash
pwsh --version    # Requires 7.5+

# Cache worktree
[ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
git -C .data-cache pull --rebase origin docs-data-cache
CACHE=".data-cache/repos/mono-SkiaSharp"
```

### 2. Read the issue

```bash
cat $CACHE/github/items/{number}.json
```

If not in cache, fetch via GitHub CLI or MCP tools:

```bash
gh issue view {number} --repo mono/SkiaSharp --json title,body,labels,comments,state,createdAt,closedAt,author
```

### 3. Convert to markdown

If using cached JSON:

```bash
pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json > /tmp/issue-{number}.md
```

### 4. Triage boost (optional but valuable)

Check for existing triage data to bootstrap context:

```bash
TRIAGE="$CACHE/ai-triage/{number}.json"
[ -f "$TRIAGE" ] && echo "Triage data available"
```

If triage exists:
- Read the JSON and extract useful hints: `codeInvestigation`, `bugSignals`, `classification`, `resolution.proposals`
- **Read `classification.platforms[]`** — use to inform platform selection in Phase 2.4
- **Read `evidence.bugSignals.errorType`** — helps guide platform file choice (e.g., `DllNotFoundException` + Linux → Docker)
- Use these as **hints only** — reproduction must verify independently
- If `classification.type.value` is NOT `type/bug`: log a warning and auto-proceed (do NOT prompt the user)
- Record the triage file path in output JSON (`inputs.triageFile` field)

If triage does NOT exist: proceed from issue data alone — no blocking.

---

## Phase 2 — Assess & Plan

### 1. Classify bug category

Read [references/bug-categories.md](references/bug-categories.md) and classify:

| Category | Signals |
|----------|---------|
| C# API | Wrong return value, ArgumentException, incorrect calculation |
| Native loading | DllNotFoundException, EntryPointNotFoundException, library not found |
| Rendering | Wrong colors, garbled output, missing content, visual diff |
| Platform-specific | "works on X but not Y", OS-specific crash |
| Build/deployment | NuGet restore, TFM issues, project setup |
| Memory/disposal | AccessViolationException, use-after-dispose, crash on GC |

### 2. Extract reporter's version & TFM

Extract from the issue:
- `{reporter_version}`: exact SkiaSharp NuGet version (e.g., `2.88.9`, `3.116.1`)
- `{reporter_tfm}`: target framework (e.g., `net8.0`, `net10.0`, `net8.0-browser`)
- `{reporter_code}`: reproduction code from the issue body
- Platform/OS the reporter was on

If not stated, use the latest stable release as the default.

**⚠️ .NET Forward Compatibility:** A library targeting `net8.0` is compatible with `net10.0`,
`net11.0`, etc. NuGet TFM fallback is a FEATURE, not a bug. NEVER say "doesn't support
.NET X" when the library targets an older TFM. NEVER suggest "downgrade to .NET 8" as a
workaround. If the reporter says `net10.0`, test with `net10.0`.

### 2b. Verify "last known good" version feasibility

If the reporter names a "last known good" version, verify it's testable before planning:

```bash
# Quick-check: download nupkg and inspect contents
curl -sLo /tmp/check.nupkg "https://api.nuget.org/v3-flatcontainer/skiasharp/{version}/skiasharp.{version}.nupkg"
unzip -l /tmp/check.nupkg | grep -E "lib/|runtimes/"
```

Check:
- **TFMs in `lib/`**: `netstandard1.3` or `netstandard2.0` = compatible with .NET 8+
- **Native assets in `runtimes/`**: need a match for current platform/arch
- **No arm64 native?** Try Rosetta: `arch -x86_64 dotnet run` (note arch in results)

**NEVER write "version X is too old" without running this check.**

### 3. Environment check

- What platform are we on? (macOS/Linux/Windows)
- Docker available? (`docker --version` — if yes, can test Linux x64/arm64)
- Playwright available? (for WASM/Blazor bugs — check MCP browser tools)
- GPU available? (for rendering bugs)
- **.NET workloads installed?** Phase 3C requires building from source, which needs platform workloads:
  ```bash
  dotnet workload list
  ```
  If `maui`, `ios`, `macos`, `maccatalyst`, or `android` are missing, install them **now** — don't wait until Phase 3C:
  ```bash
  dotnet workload install maui ios macos maccatalyst android
  ```
  If this requires `sudo` or elevated permissions, ask the user for help. Do **NOT** skip, look for workarounds, or treat this as a blocker. Missing workloads are a solvable setup problem.

### 4. Determine reproduction platform

Scan the issue (and triage data if available) for platform signals and select the primary platform file:

| Priority | Signals | Platform file | Fallback |
|----------|---------|---------------|----------|
| 1 | Blazor, WASM, WebAssembly, SKHtmlCanvas, browser console error | [platform-wasm-blazor.md](references/platform-wasm-blazor.md) | [platform-console.md](references/platform-console.md) |
| 2 | WPF, WinForms, WinUI, UWP, XPS, GDI+ | [platform-windows-desktop.md](references/platform-windows-desktop.md) | [platform-console.md](references/platform-console.md) |
| 3 | iOS, Android, MAUI, Xamarin, mobile | [platform-mobile.md](references/platform-mobile.md) | [platform-console.md](references/platform-console.md) |
| 4 | Linux, Docker, container, NativeAssets.Linux | [platform-docker-linux.md](references/platform-docker-linux.md) | N/A |
| 5 | (none of above) | [platform-console.md](references/platform-console.md) | N/A |

**Sources for platform signals** (in priority order):
1. Issue text (keywords, error messages, project type)
2. Triage `classification.platforms[]` (if triage exists)
3. Triage `evidence.bugSignals.errorType` (e.g., `DllNotFoundException` + Linux → Docker)

**Read the selected platform file.** Follow its Create → Build → Run → Verify steps,
substituting `{reporter_version}`, `{reporter_tfm}`, and `{reporter_code}`.

### 5. Plan

Output a brief plan before executing. Example:
> "WASM/Blazor bug about TypeInitializationException. Reporter used SkiaSharp 3.119.2-preview.1 on .NET 10. Will create a Blazor WASM app (platform-wasm-blazor.md), test in browser with Playwright, then test with latest stable and main source."

---

## Phase 3 — Reproduce

The goal is to create a **standalone reproduction project** that anyone can run to observe the bug.
This is NOT about building SkiaSharp from source — it's about creating a self-contained repro
that uses published NuGet packages.

### 3A. Reproduce with reporter's version (primary)

**Read the platform file** selected in Phase 2.4. Follow its steps:

1. **Create** a project in `/tmp/repro-{number}/` using the platform file's template
2. **Add repro code** — use the reporter's code if provided, otherwise create minimal code
3. **Build** — record exit code, warnings, errors
4. **Run & Verify** — platform-specific (console output, browser console, Docker stdout)
5. **Iterate** if not clearly reproduced (different data, different approach, different version)

**Handoff requirement (for bug-fix):** when you create/edit repro files (`Program.cs`, `.csproj`, helper `.cs` files), capture their full text in JSON via `reproductionSteps[].filesCreated[].content` (text only; omit binaries).

For each step, capture:

| What | How | Limit |
|------|-----|-------|
| Command run | Exact command (redact absolute paths) | — |
| Exit code | Process/command exit code (0=success) | — |
| Output | stdout/stderr | **2KB** for success, **4KB** for failure/wrong-output |
| Files created | Filename, description, and **source code content** for repro files | — |
| Errors | Error message + first 50 lines of stack trace | 5KB |
| Layer | `setup` / `csharp` / `c-api` / `native` / `deployment` | — |
| Result | `success` / `failure` / `wrong-output` / `skip` | — |

**⚠️ Step Result = Observed Outcome, Not Expected Outcome**

Step `result` values describe **what actually happened**, not whether it was "supposed to happen":

| You Run | What Happens | Step Result | Why |
|---------|--------------|-------------|-----|
| `dotnet build` | Build succeeds, exits 0 | `success` | Command succeeded |
| `dotnet build` | Build fails with CS0117 | `failure` | Command failed (non-zero exit) |
| `dotnet build` with old API | Build fails with CS0117 (reporter said it fails) | **`failure`** | **Command still failed — matching the report doesn't make it a success** |
| Render image | Exits 0 but pixels wrong | `wrong-output` | Process succeeded, output incorrect |

**Anti-pattern**: Marking a build failure as `result: "success"` because "we expected it to fail" or "the reporter said it would fail". The step FAILED — that's `result: "failure"`. The fact that this matches the report goes in the overall **conclusion** (`reproduced`), not the step result.

**Truncation:** If output exceeds limits, keep first and last portions with `[...truncated N lines...]` in the middle.

**Binary assets:** If reproduction needs files from the issue (images, fonts, ICC profiles):
- Check if available as issue attachments (download URL)
- Check if similar files exist in `tests/` (PathToImages, PathToFonts)
- Record in `artifacts` array with `available: true/false`
- If unavailable, **keep trying** — adapt the repro to use available test data if possible

> **🔥 Push hard.** Don't bail early. The value of this skill is in persistent, creative
> attempts to reproduce. Only conclude `not-reproduced` after genuinely exhausting approaches.
> Only conclude `needs-platform` / `needs-hardware` when there is truly no workaround.

### 3B. Test on latest release (if reproduced)

If the bug reproduced with the reporter's version, test whether it's **already fixed**.
Use the **same platform strategy** from Phase 3A — just change the version:

```bash
cd /tmp/repro-{number}/Repro
# Update to latest stable release (omit --version to get latest stable)
dotnet add package SkiaSharp
```

Then re-run using the same platform file's Run & Verify steps.
Record the result. If the bug is gone on latest, this is valuable — note it in `fixedInVersion`.

### 3C. Test on main branch (required, if reproduced)

> **🛑 MANDATORY:** If the bug reproduced on latest release, you MUST test on main.
> Do NOT skip this step. Do NOT declare the task complete without a `versionResults` entry for `main (source)`.
> If the build fails, fix the build (install workloads, resolve errors). If you truly cannot build,
> record `result: "not-tested"` with a `notes` field explaining exactly what failed and what you tried.

If still reproduced on latest release, test against source on main:

#### Step 1 — Build

```bash
# Back in the SkiaSharp repo working directory
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download
dotnet build tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

> Build the **test project** (not just SkiaSharp.csproj) — it transitively builds all dependencies
> and sets up the native library loading that tests need.

**If the build fails** (e.g., missing workloads, SDK errors):
1. Read the error message — it usually tells you exactly what to do
2. Fix it (install workloads, update SDK, etc.)
3. If fixing requires `sudo` or user input, ask the user — do NOT give up
4. Only record `not-tested` after genuinely exhausting all options

#### Step 2 — Test

**If an existing test covers the bug** (search with `grep -r "MethodName\|BugKeyword" tests/`):

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --no-build --filter "FullyQualifiedName~RelevantTestName"
```

**If no existing test covers the bug** (most reproduction scenarios), use `dotnet test` with the test project infrastructure. Write a one-off test or reuse the reproduction approach from Phase 3A:

```bash
# Copy any needed test assets into the test content directory
cp /tmp/repro-{number}/test-file.xyz tests/Content/images/

# Run the full test suite (or a relevant subset) to verify the infrastructure works
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --no-build --filter "FullyQualifiedName~SKCodecTest"
```

Then write a targeted xUnit test in the existing test files and run it:

```bash
# Add a test method to the appropriate test file (e.g., tests/Tests/SkiaSharp/SKCodecTest.cs)
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~YourNewTestName"
```

> **Do NOT** create standalone `/tmp` projects that reference source-built DLLs and manually copy
> native libraries. The test project already handles native library resolution. Use it.

#### Step 3 — Record

Record whether the bug exists on main. If fixed on main but not released, note the version gap.

**versionResults:** The output JSON MUST always include an entry for `main (source)` in `versionResults`. This is true even if you used `result: "not-tested"` — in that case, explain why in `notes`.

> **Clean up:** Remove any test assets you copied into `tests/Content/` and any test methods you
> added. These are throwaway reproduction artifacts, not permanent additions. Use `git checkout`
> to revert test file changes.

### 3D. Cross-platform verification (conditional)

After primary reproduction, test on an alternative platform to determine bug scope.

**When to run:**

| Primary result | Condition | Run 3D? |
|----------------|-----------|---------|
| `reproduced` | Any (except pure API/logic bug with no platform signals) | **Yes** |
| `not-reproduced` | Reporter on different platform than host | **Yes** |
| `not-reproduced` | Reporter on same platform as host | No |
| `needs-platform` | Any | **Yes** — try console fallback + Docker |
| `partial` / `inconclusive` | Any | **Yes** |
| `reproduced` | Pure API/logic bug, no platform signals | **Skip** — note "cross-platform skipped: pure API bug" |

**Time cap:** 5 minutes max for Phase 3D. If setup would exceed this, skip and record why.

**Which alternative platform (orthogonality rule):**

| Primary platform | Best alternative | Why |
|-----------------|-----------------|-----|
| Console on macOS | Docker Linux x64 | Different OS, catches font/case/native diffs |
| WASM/Blazor | Console on host | Isolate: WASM-specific or core? |
| Docker Linux | Console on host | Different OS perspective |
| Windows (reported, host is Mac) | Console on Mac, then Docker Linux | Linux is "neutral arbiter" |
| iOS/Android (no device) | Console, then Docker Linux | Core bugs often repro without mobile |

**Default:** Docker Linux x64 is the best default alternative when no better signal exists.

**Test reporter's version only** (not the full version matrix — cost vs value tradeoff).

**Record results** in `versionResults` with `platform` field:
```json
{ "version": "3.116.1", "source": "nuget", "result": "not-reproduced",
  "notes": "Works on Docker Linux x64", "platform": "docker-linux-x64" }
```

**Derive `scope`** from comparison:
- Reproduced on ≥2 platforms → `"universal"`
- Reproduced on primary only → `"platform-specific/{platform}"`
- Only tested one platform (3D skipped) → `"unknown"`

---

## Phase 4 — Generate JSON

### 1. Choose conclusion

Read [references/conclusion-guide.md](references/conclusion-guide.md) and select the appropriate value.

> **⚠️ CRITICAL: Factual vs Editorial (NO JUDGMENT ALLOWED)**
> Your ONLY job is to determine: **Did the reported symptoms occur?**
> Do NOT judge if it is "working as designed", a "breaking change", or "user error".
> - If reporter says "Build fails with error X" and you get error X → `reproduced`
> - If reporter says "Crash on startup" and you get a crash → `reproduced`
> - Even if the error is "Working as Designed" or a known breaking change, the fact remains: **it reproduced.**
>
> **Do not use `not-reproduced` just because you think the behavior is correct.**
>
> **Anti-pattern from real failure (NEVER DO THIS):**
> - ❌ Reporter: "MakeIdentity() gives CS0117" → You: CS0117 observed → Conclusion: `not-reproduced` + Note: "API was renamed"
> - ✅ Reporter: "MakeIdentity() gives CS0117" → You: CS0117 observed → Conclusion: `reproduced` + Note: "Confirmed CS0117. This is an intentional API rename in SkiaSharp 3.x."

| Conclusion | When |
|------------|------|
| `reproduced` | **Reported behavior occurred** — crash, exception, compiler error, or wrong values (even if intentional/by-design) |
| `wrong-output` | Process succeeds but visual/rendered output is incorrect |
| `not-reproduced` | Reported behavior **did not occur** — steps passed when reporter said they'd fail |
| `needs-platform` | Requires unavailable OS/platform or native rebuild |
| `needs-hardware` | Requires specific hardware (GPU, device) |
| `partial` | Some aspects reproduced, others unverifiable |
| `inconclusive` | Insufficient info or ambiguous results |

### 2. Redact sensitive data

Before generating JSON:
- Replace absolute paths containing usernames (e.g., `/Users/matthew/...` → `$HOME/...`)
- Remove usernames, tokens, or credentials from command output
- Strip machine-specific paths from stack traces

### 3. Generate JSON

Write to `/tmp/repro-{number}.json`. Schema: [references/repro-schema.json](references/repro-schema.json)

> **⚠️ Schema rules:**
> - `meta.schemaVersion` must be `"1.0"`
> - **Optional fields: OMIT them entirely** — do NOT set them to `null`. If a field is not applicable, leave it out of the JSON.
> - `inputs.triageFile` replaces the old root-level `triageFile`/`triageNotes` fields (those no longer exist)
> - `additionalProperties: false` — no extra fields allowed at any level

Required fields:
- `meta`: schemaVersion `"1.0"`, number, repo (`"mono/SkiaSharp"`), analyzedAt (ISO 8601 UTC)
- `conclusion`: one of the enum values
- `notes`: free-text summary (min 10 chars)
- `reproductionSteps`: array of steps with stepNumber, description, layer
- `environment`: os, arch, dotnetVersion, skiaSharpVersion, dockerUsed

Optional (recommended) — include only when applicable, otherwise omit entirely:
- `inputs`: `{ triageFile }` if triage data was consumed
- `scope`: cross-platform scope derived from Phase 3D — `"universal"`, `"platform-specific/{platform}"`, or `"unknown"` (if 3D was skipped)
- `assessment`: editorial classification (e.g., `"breaking-change"`) without corrupting factual `conclusion`
- `versionResults`: include `platform` field on each entry (e.g., `"host-macos-arm64"`, `"docker-linux-x64"`)
- `reproProject`, `artifacts`, `errorMessages`
- `feedback`: corrections to triage findings (see below)

Conditional requirements:
- `reproduced` → ≥1 step with result `failure` or `wrong-output`
- `wrong-output` → ≥1 step with result `wrong-output`
- `not-reproduced` → ≥1 step with result `success`
- `needs-platform` / `needs-hardware` / `partial` / `inconclusive` → `blockers` required

For steps that ran a `command` and have `result` of `success` / `failure` / `wrong-output`, include `exitCode` (0 = success, non-zero = failure).

### 4. Feedback (when triage was consumed)

If triage data was consumed (`inputs.triageFile` is set) and reproduction contradicts any triage finding, record the correction in the `feedback.triageCorrections` array:

```json
"feedback": {
  "triageCorrections": [
    {
      "topic": "classification",
      "upstream": "Classified as type/question with confidence 0.7",
      "corrected": "Confirmed crash with AccessViolationException — this is a real bug"
    }
  ]
}
```

Common correction scenarios:
- Triage classified as "question" but repro confirmed a real crash → correct classification
- Triage said "platform/Windows" but repro shows it affects all platforms → correct scope
- Triage missed evidence that repro discovered → add to corrections
- Triage's codeInvestigation pointed to wrong files → note the correct location

---

## Phase 5 — Validate & Persist

### 1. Validate

```bash
pwsh .github/skills/bug-repro/scripts/validate-repro.ps1 /tmp/repro-{number}.json
```

- Exit 0 = valid → continue
- Exit 1 = fixable errors → fix and retry
- Exit 2 = fatal → stop and report

### 2. Persist (local first)

```bash
cp /tmp/repro-{number}.json $CACHE/ai-repro/{number}.json
```

### 3. Push

```bash
cd .data-cache
mkdir -p repos/mono-SkiaSharp/ai-repro
cp /tmp/repro-{number}.json repos/mono-SkiaSharp/ai-repro/{number}.json
git add repos/mono-SkiaSharp/ai-repro/{number}.json
git commit -m "ai-repro: reproduce #{number}"
git push  # Rebase up to 3x on conflict
cd ..
```

If push fails: record as a blocker with the local path. The JSON is still valid locally.

### 4. Present summary

```
✅ Reproduction: ai-repro/{number}.json

Conclusion:  reproduced
Time:        ~8 minutes
Steps:       5 (3 success, 1 failure, 1 skip)
Environment: macOS arm64, .NET 10.0.100

Version results:
  SkiaSharp 2.88.9 (reporter): ❌ REPRODUCED
  SkiaSharp 3.116.1 (latest):  ❌ REPRODUCED
  SkiaSharp main (source):     ✅ not tested

Primary error:
  SKMatrix.MapRect returns SKRect(0, 0, 100, 100) instead of SKRect(100, 100, 0, 0)
  when using a matrix with negative scale factors.

Blockers: none
```

---

## Output Limits & Redaction

| Field | Max Size | Truncation |
|-------|----------|------------|
| `reproductionSteps[].output` (success) | 2KB | Head/tail with `[...truncated...]` |
| `reproductionSteps[].output` (failure) | 4KB | Head/tail with `[...truncated...]` |
| `errorMessages.stackTrace` | 5KB / 50 lines | First 50 lines |
| File content (text) | Inline OK for small repro source files | Include `Program.cs`, `.csproj`, etc. via `filesCreated[].content` (redact paths); omit binaries |
| Binary assets | Never inline | URL/filename reference in `artifacts` |

**Redaction rules:**
- `/Users/{name}/` → `$HOME/`
- `/home/{name}/` → `$HOME/`
- `C:\Users\{name}\` → `$HOME\`
- Tokens, API keys, passwords → `[REDACTED]`

---

## Anti-Patterns (NEVER DO)

1. **Inline binary content.** NEVER put image/font/binary data in JSON. Use `artifacts` array with URLs and filenames.

2. **Unlimited output.** NEVER capture full build logs. Truncate to size limits.

3. **Source code investigation.** NEVER trace root cause or propose fixes. That's the `bug-fix` skill's job. Bug-repro stops at "did it reproduce or not."

4. **Prompting the user.** NEVER ask "Proceed anyway?" — auto-proceed with a logged warning. The skill must work non-interactively.

5. **Absolute paths in output.** NEVER leave machine-specific paths in the JSON. Redact everything.

6. **Giving up too early.** NEVER conclude `not-reproduced` or `needs-platform` after just one attempt. Try multiple approaches, different versions, different test data. The value of this skill is persistence.

7. **Building from source first.** ALWAYS start with released NuGet packages in a standalone project. Only build from source in Phase 3C after reproducing with released versions.

8. **Editorial judgment in conclusion.** NEVER let "this isn't really a bug" influence the conclusion. If the reported behavior occurred, that's `reproduced`. Your opinion that it's intentional, documented, or working-as-designed goes in `notes`, not `conclusion`. Reproduction is factual observation, not bug triage.

9. **Mismarking step results.** NEVER mark a step `result: "success"` just because the failure was expected. Step `result` describes the TECHNICAL outcome of the command (exit code 0 vs 1). A build that fails with 4 compiler errors is `result: "failure"` even if that confirms the bug.

10. **Pre-emptive version assumptions.** NEVER assume a NuGet version is incompatible without inspecting the nupkg. All blockers about version compatibility must cite evidence (TFMs found, native assets present/missing, actual error when attempted).

11. **Abandoning on environment issues.** NEVER give up when hitting solvable environment problems (missing workloads, missing tools, `sudo` prompts, SDK version mismatches). These are setup steps, not blockers. Fix them — install the workload, update the SDK, ask the user for elevated permissions. If the error message tells you the fix, do the fix.

12. **Skipping Docker for Linux bugs.** NEVER conclude `needs-platform` for Linux-related issues without trying Docker first. Docker Desktop supports linux/amd64 and linux/arm64 from macOS. The only valid `needs-platform` for Linux is when the bug requires a specific kernel feature or hardware that Docker can't provide.

13. **Assuming TFM incompatibility.** NEVER say "SkiaSharp doesn't support .NET X" when X is newer than the library's TFM. .NET is forward-compatible by design — a `net8.0` library works on `net10.0` apps. NuGet TFM fallback is a feature, not a bug. The only exception is platform-specific TFMs (e.g., `net8.0-ios`) where platform assets are needed. NEVER suggest "downgrade to .NET 8" as a workaround.

14. **Stopping at build success for browser/WASM bugs.** NEVER conclude `not-reproduced` because `dotnet build` succeeded for a Blazor/WASM issue. WASM bugs manifest at RUNTIME in the browser. You MUST serve the app and check browser console with Playwright. Build success ≠ runtime success.

15. **Retrying the same platform without changing variables.** If it failed on macOS, running the same code on macOS again won't produce new signal. Change the platform, version, or approach — not the retry count.

16. **Misinterpreting setup failures as "not reproduced."** Docker pull timeout, missing Playwright, or `wasm-tools` install failure are SETUP blockers, not reproduction results. Record as blocker, not `not-reproduced`.

17. **Testing WASM for every bug.** WASM is a specialized runtime (AOT, single-threaded, no filesystem). Only test it when issue signals suggest browser/web. Testing WASM for an `SKMatrix` calculation bug is noise that produces false positives.

18. **Silently skipping cross-platform verification.** If you skip Phase 3D, you MUST record why in `notes` and set `scope` to `"unknown"`. Never leave the downstream fix skill guessing about scope.
