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

### 4. Triage boost (optional)

Check for existing triage data to bootstrap context:

```bash
TRIAGE="$CACHE/ai-triage/{number}.json"
[ -f "$TRIAGE" ] && echo "Triage data available"
```

If triage exists:
- Read the JSON and extract useful hints: `codeInvestigation`, `bugSignals`, `classification`, `resolution.proposals`
- Use these as **hints only** — reproduction must verify independently
- If `classification.type.value` is NOT `type/bug`: log a warning and auto-proceed (do NOT prompt the user)
- Record the triage file path in output JSON (`triageFile` field)

If triage does NOT exist: proceed from issue data alone — no blocking.

---

## Phase 2 — Assess & Plan

### 1. Classify bug category

Read [references/repro-strategies.md](references/repro-strategies.md) and classify:

| Category | Signals |
|----------|---------|
| C# API | Wrong return value, ArgumentException, incorrect calculation |
| Native loading | DllNotFoundException, EntryPointNotFoundException, library not found |
| Rendering | Wrong colors, garbled output, missing content, visual diff |
| Platform-specific | "works on X but not Y", OS-specific crash |
| Build/deployment | NuGet restore, TFM issues, project setup |
| Memory/disposal | AccessViolationException, use-after-dispose, crash on GC |

### 2. Identify reporter's version

Extract from the issue:
- Which **SkiaSharp NuGet version** was the reporter using? (e.g., `2.88.9`, `3.116.1`)
- Which **.NET TFM** were they targeting? (e.g., `net8.0`, `net9.0`)
- Which **platform/OS** were they on?

If not stated, use the latest stable release as the default.

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
- Docker available? (unreliable on macOS — treat as optional)
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

### 4. Plan

Output a brief plan before executing. Example:
> "C# API bug about SKMatrix.MapRect. Reporter used SkiaSharp 2.88.9 on .NET 8. Will create a standalone console app with that version first, then test with latest release and source (main)."

---

## Phase 3 — Reproduce

The goal is to create a **standalone reproduction project** that anyone can run to observe the bug.
This is NOT about building SkiaSharp from source — it's about creating a self-contained repro
that uses published NuGet packages.

### 3A. Reproduce with reporter's version (primary)

#### 1. Create a standalone project

Create a new project in `/tmp/repro-{number}/`:

```bash
mkdir -p /tmp/repro-{number} && cd /tmp/repro-{number}
dotnet new console -n Repro --framework {reporter_tfm}
cd Repro
dotnet add package SkiaSharp --version {reporter_version}
# Add other packages the reporter mentioned
```

#### 2. Write reproduction code

- **Use the reporter's code** if provided in the issue — copy it as closely as possible
- If no code provided: create minimal code from the issue description
- The code should **clearly demonstrate** the bug (print values, save images, assert conditions)

#### 3. Run and capture

```bash
dotnet run
```

For each step, capture:

| What | How | Limit |
|------|-----|-------|
| Command run | Exact command (redact absolute paths) | — |
| Output | stdout/stderr | **2KB** for success, **4KB** for failure/wrong-output |
| Files created | Filename, description, and **source code content** for repro files | — |
| Errors | Error message + first 50 lines of stack trace | 5KB |
| Layer | `setup` / `csharp` / `c-api` / `native` / `deployment` | — |
| Result | `success` / `failure` / `wrong-output` / `skip` | — |

**Truncation:** If output exceeds limits, keep first and last portions with `[...truncated N lines...]` in the middle.

**Binary assets:** If reproduction needs files from the issue (images, fonts, ICC profiles):
- Check if available as issue attachments (download URL)
- Check if similar files exist in `tests/` (PathToImages, PathToFonts)
- Record in `artifacts` array with `available: true/false`
- If unavailable, **keep trying** — adapt the repro to use available test data if possible

#### 4. Iterate — try hard

If the first attempt doesn't clearly reproduce, **keep going**:

1. Try different input data (different images, fonts, matrix values)
2. Try a different API approach (the reporter may have simplified — try the full scenario)
3. Try a different NuGet version (nearby versions — maybe reporter was slightly off)
4. Try Docker for cross-platform if the bug is platform-specific
5. Simplify the reproduction — strip it to the absolute minimum

> **🔥 Push hard.** Don't bail early. The value of this skill is in persistent, creative
> attempts to reproduce. Only conclude `not-reproduced` after genuinely exhausting approaches.
> Only conclude `needs-platform` / `needs-hardware` when there is truly no workaround.

### 3B. Test on latest release (if reproduced)

If the bug reproduced with the reporter's version, test whether it's **already fixed**:

```bash
cd /tmp/repro-{number}/Repro
# Update to latest stable release (omit --version to get latest stable)
dotnet add package SkiaSharp
dotnet run
```

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

---

## Phase 4 — Generate JSON

### 1. Choose conclusion

Read [references/conclusion-guide.md](references/conclusion-guide.md) and select the appropriate value:

| Conclusion | When |
|------------|------|
| `reproduced` | Bug confirmed — crash, exception, or wrong values |
| `wrong-output` | Process succeeds but visual/rendered output is incorrect |
| `not-reproduced` | All steps passed, reported behavior not observed |
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

Required fields:
- `meta`: schemaVersion `"1.0"`, number, repo (`"mono/SkiaSharp"`), analyzedAt (ISO 8601 UTC)
- `conclusion`: one of the enum values
- `notes`: free-text summary (min 10 chars)
- `reproductionSteps`: array of steps with stepNumber, description, layer
- `environment`: os, arch, dotnetVersion, skiaSharpVersion, dockerUsed

Conditional requirements:
- `reproduced` → ≥1 step with result `failure` or `wrong-output`
- `wrong-output` → ≥1 step with result `wrong-output`
- `not-reproduced` → ≥1 step with result `success`
- `needs-platform` / `needs-hardware` / `partial` / `inconclusive` → `blockers` required

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
| File content | Never inline | Filename + description only |
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

8. **Pre-emptive version assumptions.** NEVER assume a NuGet version is incompatible without inspecting the nupkg. All blockers about version compatibility must cite evidence (TFMs found, native assets present/missing, actual error when attempted).

9. **Abandoning on environment issues.** NEVER give up when hitting solvable environment problems (missing workloads, missing tools, `sudo` prompts, SDK version mismatches). These are setup steps, not blockers. Fix them — install the workload, update the SDK, ask the user for elevated permissions. If the error message tells you the fix, do the fix.
