---
name: issue-fix
description: >
  Fix bugs in SkiaSharp C# bindings. Structured workflow for investigating, fixing,
  and testing bug reports. Use whenever someone reports something broken, crashing,
  or behaving incorrectly — whether they describe a specific exception, reference a
  GitHub issue number, or simply say "this doesn't work". Covers crashes, exceptions,
  AccessViolationException, segfaults, undefined symbols, wrong output, memory leaks,
  disposal issues, performance regressions, and any request to debug, investigate, or
  fix a SkiaSharp problem. For adding new APIs, use `add-api` skill instead.
---

# Bug Fix Skill

**Issue pipeline: Step 3 of 3 (Fix).** See [`documentation/issue-pipeline.md`](../../../documentation/issue-pipeline.md).

Fix bugs in SkiaSharp with minimal, surgical changes.

## Before You Start

Read these three files first — they define the output format and hard constraints:

1. This SKILL.md (the workflow below)
2. [references/schema-cheatsheet.md](references/schema-cheatsheet.md) — fix JSON fields and enums
3. [references/anti-patterns.md](references/anti-patterns.md) — critical rules (the authoritative list)

## Workflow Overview

Execute phases in order. Each gate must be met before advancing. Phases may be abbreviated when `ai-triage/{n}.json` or `ai-repro/{n}.json` already exist — consume them and meet the gate with evidence rather than redoing the work.

The PR is created early (Phase 2) because it serves as the living document for the entire investigation — every finding, hypothesis, and result gets logged there in real time. Without it, work happens invisibly.

```
1. Understand   → Fetch issue, consume ai-triage/ai-repro if present
2. Create PR    → Create PR as living investigation document
3. Research     → Delta research (triage already did first-pass)
4. Reproduce    → Prefer ai-repro project; Docker only if needed
5. Investigate  → Root cause (guided by repro version matrix + triage codeInvestigation)
6. Fix          → Minimal change
7. Test         → Regression test + existing tests
8. Finalize     → Rewrite PR description, link all fixed issues
9. Fix JSON     → Generate, validate, and persist ai-fix/{n}.json
```

---

## Prerequisites

- GitHub API access (fetch issues, search issues, read comments)
- Git with push access
- Local data cache worktree (`docs-data-cache`) for ai-triage/ai-repro handoff
- Docker for cross-platform testing (optional; check: `docker --version`)

---

## Phase 1: Understand the Issue (pipeline intake)

### 1. Prefer the data cache (handoff)

```bash
pwsh --version    # Requires 7.5+

# Cache worktree
[ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
git -C .data-cache pull --rebase origin docs-data-cache
CACHE=".data-cache/repos/mono-SkiaSharp"

TRIAGE="$CACHE/ai-triage/NNNN.json"
REPRO="$CACHE/ai-repro/NNNN.json"
```

- If `TRIAGE` exists: treat it as the **authoritative classification + codeInvestigation**. Extract key details and uncertainties.
- If `REPRO` exists: treat it as the **authoritative factual reproduction record** (versions tested + minimal repro source).

If cache is missing the issue/JSONs, fall back to `gh`.

### 2. Extract only what you need to open the PR

Extract (from issue + triage/repro if present):
- Symptoms, error messages, stack traces
- Platform (OS, arch, .NET version, SkiaSharp version)
- Version status (reproduces on latest? on main?)
- Minimal reproduction steps / code (prefer `ai-repro`)

**Do not redo triage’s work here.** No deep code investigation and no broad related-issue search yet.

### ✅ GATE: Do not proceed until you have:
- [ ] Issue title, symptoms, and error message (if any)
- [ ] Target platform identified
- [ ] Noted whether `ai-triage/NNNN.json` exists
- [ ] Noted whether `ai-repro/NNNN.json` exists

---

## Phase 2: Create Draft PR

Create the PR now — before any research or investigation. The PR is your living document where every finding gets recorded, making the process transparent and reviewable.

```bash
git checkout -b dev/issue-NNNN-short-description
git commit --allow-empty -m "Investigating #NNNN: [description]"
git push -u origin dev/issue-NNNN-short-description
gh pr create --draft --title "Investigating #NNNN: [description]" --body "[template]"
gh pr edit --add-label "copilot"
```

Create PR using investigation template from [references/pr-templates.md](references/pr-templates.md).

**The PR description is your living document:**
- All collected info, links, and related issues (added as you find them)
- WHY each related issue is similar (same platform? same error? same root cause?)
- Your investigation plan with checkboxes
- Progress log (add rows as you work)
- Alternatives tried (add when something doesn't work)

**Update the PR description OFTEN** — after every significant step.

### ✅ GATE: Do not proceed until you have:
- [ ] Feature branch created and pushed
- [ ] Draft PR opened with investigation template
- [ ] "copilot" label added to PR

---

## Phase 3: Research Related Issues (delta)

> **Prerequisite:** Phase 2 complete (PR exists to log findings into).

If `ai-triage/NNNN.json` exists, it already contains:
- related issues discovered during workaround/duplicate search
- code investigation entry points
- workaround proposals and missing info

**Your job in Phase 3 is delta research only:**
- confirm/expand on the *most relevant* related issues (especially ones with diagnosis in comments)
- run additional searches only if triage confidence is low, triage is stale, or repro contradicts triage

This phase often solves the bug outright — the community may have already diagnosed the root cause in issue comments. Read all comments on the most relevant related issues before investigating yourself.

Search GitHub issues for:
- Same error message (e.g., `undefined symbol: uuid_generate_random`)
- Same platform (e.g., `Linux ARM64`)
- Same SkiaSharp version
- Keywords from title

**For EACH related issue found:**
1. **Read ALL comments** (not just the issue body) — diagnosis is often in comments!
2. Note: issue number, title, WHY it's related
3. Extract: workarounds mentioned, root cause analysis, resolution if closed
4. Check for links to external issues (other projects that use SkiaSharp)

**Update PR** with all related issues and extracted information.

### ✅ GATE: Do not proceed until you have:
- [ ] Searched for related issues (at least 2-3 search queries)
- [ ] Read ALL comments on the most relevant related issues
- [ ] Updated PR with related issues and any diagnosis found

**If a related issue already contains the root cause diagnosis, document it in the PR
and still proceed to Phase 4 — community diagnoses can be workarounds rather than true fixes,
and hypotheses need reproduction evidence.**

---

## Phase 4: Reproduce (prefer ai-repro)

Reproduction is required even if Phase 3 found a likely root cause — community diagnosis could be a workaround rather than the real fix, and hypotheses need evidence. Satisfy this phase either by:
- **Re-running** the minimal repro locally, OR
- **Consuming** an existing `ai-repro/NNNN.json` that already reproduced the issue on the relevant version/platform

If `ai-repro/NNNN.json` exists and `conclusion` is `reproduced`:
- Rehydrate the repro source from `reproductionSteps[].filesCreated[].content` into a local folder (e.g., `/tmp/skiasharp/repro/NNNN/`) and run it.
- Prefer this NuGet-based repro as the baseline; use Docker only if the host cannot exercise the target platform.

If no `ai-repro` exists:
- Reproduce using the **same approach as issue-repro** (standalone NuGet project first; Docker only when needed).

### 4.1 Target Platform Requirements

| Attribute | Must Match |
|-----------|------------|
| OS (macOS/Windows/Linux) | ✅ |
| Architecture (x64/ARM64) | ✅ |
| .NET version | ✅ |
| SkiaSharp version | ✅ |

### 4.2 Docker Testing

For cross-platform testing, see [references/docker-testing.md](references/docker-testing.md).

Example (adapt platform to match the issue):
```bash
# Replace with the platform from the issue
docker run --platform linux/arm64 -it <dotnet-sdk-image> bash
```

### 4.3 Document Results in PR

Add to PR description:

| Environment | Version | Result |
|-------------|---------|--------|
| [Platform from issue] | [version] | ❌ Crashes |
| [Different platform] | [version] | ✅ Works |

### 4.4 If Reproduction Fails

**Try hard and exhaust all options** before giving up:
1. Try different Docker base images (different Linux distros)
2. Try older/newer .NET versions
3. Try the exact SkiaSharp version AND the last known working version
4. Check if issue mentions specific hardware or configurations
5. Download and use any reproduction project attached to the issue
6. Try minimal reproduction code from the issue verbatim

Document each attempt in PR. After exhausting ALL options: ask user for details, but still proceed with code review while waiting.

### ✅ GATE: Do not proceed until you have:
- [ ] Either (a) re-ran the minimal repro, or (b) consumed `ai-repro/NNNN.json` as the baseline reproduction record
- [ ] Documented the reproduction evidence/results in the PR (including version matrix)
- [ ] If reproduction failed: documented what was tried and asked user for help

---

## Phase 5: Investigate Root Cause

> 💡 **Often already done!** If Phase 3 found a diagnosis in related issue comments,
> this phase is just confirmation. Don't re-investigate what's already known.

For detailed debugging methodology, see [documentation/debugging-methodology.md](../../../documentation/debugging-methodology.md).

### 5.1 Start with the Key Question

**"Why does this work on [other platform/version] but fail here?"**

The answer to this question IS the root cause. Focus your investigation on finding the difference.

### 5.2 For Platform-Specific Issues: Build and Compare

When a bug affects one platform but not another, **build both platforms locally** and compare:

```bash
# Build x64 native (in Docker)
bash ./scripts/Docker/debian/amd64/build-local.sh

# Build ARM64 cross-compile (in Docker)
bash ./scripts/Docker/debian/clang-cross/build-local.sh arm64 10

# Compare DT_NEEDED entries (the linked libraries)
docker run --rm -v $(pwd):/work debian:bookworm-slim bash -c \
  "apt-get update -qq && apt-get install -y -qq binutils >/dev/null && \
   echo '=== x64 ===' && readelf -d /work/externals/skia/out/linux/x64/libSkiaSharp.so.* | grep NEEDED && \
   echo && echo '=== ARM64 ===' && readelf -d /work/externals/skia/out/linux/arm64/libSkiaSharp.so.* | grep NEEDED"
```

**If a library appears in one but not the other, investigate:**
1. Does the ninja file have `-lfoo` for both? → Check `externals/skia/out/linux/{arch}/obj/SkiaSharp.ninja`
2. If yes, the linker is silently failing → Check if library exists in sysroot
3. If no, the GN configuration differs → Check `native/linux/build.cake` and `externals/skia/gn/skia.gni`

### 5.2a For Performance Bugs: Systematic Profiling

When the bug involves slow rendering, low FPS, or performance degradation:

1. **Read [references/perf-investigation.md](references/perf-investigation.md)** — full methodology for phase profiling, isolation experiments, and AI model consultation
2. **Establish baselines** — native C++ if available, or compare across SkiaSharp versions
3. **Instrument per-phase timing** — render, flush, finish, swap using `Stopwatch`
4. **Run isolation experiments** — change one variable at a time, maintain a debugging table
5. **Consult AI models** — use 3 models via `task` tool with model overrides, require 2/3 consensus

> ⚠️ Console apps are NOT sufficient for view rendering performance bugs. Use the actual view type (SKGLView, SKMetalView) from the correct platform.

### 5.2b For macOS-Specific Issues: GL/Metal Diagnostics

When the bug involves macOS rendering, GL context, Metal, or macOS-specific views:

1. **Test both backends** — Metal and GL may behave differently
2. **Check GL state** — `glGetIntegerv` values vs pixel format values (they can disagree on macOS; e.g., `GL_STENCIL_BITS` returns 0 for the default framebuffer even when 8 bits are allocated)
3. **Disable VSync** before any timing measurement
4. **Key source files:** `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs`, `SKMetalView.cs`, and `externals/skia/tools/window/mac/GLWindowContext_mac.mm`

### 5.3 For C# Issues: Locate the Code

```bash
grep -rn "MethodName" binding/SkiaSharp/
grep -r "sk_.*methodname" binding/SkiaSharp/
```

### 5.4 Workaround vs Root Cause

The most common investigation mistake is shipping a workaround instead of fixing the root cause. The #3369 example illustrates why this matters:
- Symptom: `undefined symbol: uuid_generate_random` on ARM64
- **Wrong fix (workaround):** Add `-luuid` to linker flags
- **Root cause:** fontconfig wasn't being linked at all (broken symlink in sysroot)
- **Correct fix:** Add the fontconfig runtime library to the cross-compile sysroot

**How to tell the difference:**
- If your fix adds something NEW to compensate → probably a workaround
- If your fix restores something that SHOULD have been there → probably root cause
- If x64 doesn't need it but ARM64 does → ask WHY the difference exists

### 5.5 Exit Criteria

Stop investigating when you can answer:
- [ ] What exact code/config causes the bug?
- [ ] Why does it fail?
- [ ] Why doesn't it fail elsewhere?
- [ ] What single change fixes it?

### ✅ GATE: Do not proceed until you have:
- [ ] Identified root cause
- [ ] Updated PR with root cause analysis

---

## Phase 6: Fix

**Principle: Minimal change.** Fix only what's broken.

For guidance on specific types of fixes, see:
- [Memory management](../../../documentation/memory-management.md) - disposal patterns, ownership
- [API design](../../../documentation/api-design.md) - same-instance returns, null checks
- [Debugging methodology](../../../documentation/debugging-methodology.md) - native linking issues

### ✅ GATE: Do not proceed until you have:
- [ ] Made the fix
- [ ] Committed with descriptive message referencing issue number

---

## Phase 7: Build & Test

### If Modified Native Code

```bash
dotnet cake --target=externals-macos --arch=arm64   # macOS ARM64
dotnet cake --target=externals-linux --arch=arm64   # Linux ARM64 (in Docker)
```

### If Modified C# Only

```bash
dotnet cake --target=externals-download  # If output/native/ empty
```

### Write Regression Test

| Affected Class | Test File |
|----------------|-----------|
| SKCanvas | `tests/Tests/SKCanvasTest.cs` |
| SKBitmap | `tests/Tests/SKBitmapTest.cs` |
| SKImage | `tests/Tests/SKImageTest.cs` |
| Other | Find matching `*Test.cs` |

Name: `Issue_NNNN_BriefDescription()`

### Run Tests

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj
```

All tests must pass — a fix that breaks existing tests isn't ready. Verify on the original platform.

### Performance Verification (for perf bugs only)

For performance fixes, running correctness tests is necessary but not sufficient. Also:

1. Re-run the full benchmark matrix from Phase 5 with the fix applied
2. Verify timing improvement at ALL complexity levels tested
3. Verify no regression on other backends (e.g., Metal still works if you fixed GL)
4. Compare against the native C++ baseline if one was established
5. Record before/after/native comparison in the PR description

### ✅ GATE: Do not proceed until you have:
- [ ] Built successfully
- [ ] All tests pass
- [ ] Verified fix resolves the original issue (if possible)

---

## Phase 8: Finalize

Rewrite PR description using final template from [references/pr-templates.md](references/pr-templates.md).

For PR description and issue comment formatting, see [references/response-guidelines.md](references/response-guidelines.md).

Link ALL fixed issues (including related issues that have the same root cause):
```markdown
Fixes #3369
Fixes #3272
```

Mark PR as ready for review (remove draft status).

### ✅ GATE: Complete when:
- [ ] PR description rewritten with final template
- [ ] All related issues linked with "Fixes #NNNN"
- [ ] PR marked ready for review

---

## Phase 9: Generate Fix JSON

Generate structured output for the pipeline. Schema: [references/fix-schema.json](references/fix-schema.json)
Examples: [references/fix-examples.md](references/fix-examples.md)

### 1. Generate JSON

Write to `/tmp/skiasharp/fix/{number}.json` — use this exact literal path, do NOT substitute `$TMPDIR` or any other variable.

Refer to [references/schema-cheatsheet.md](references/schema-cheatsheet.md) for all required fields and enum values. Key top-level fields: `meta`, `status`, `summary`, `rootCause`, `changes`, `tests`, `verification`, and conditionally `pr`, `blockers`, `relatedIssues`, `feedback`.

### 2. Record upstream corrections

If the fix discovered that triage or repro got something wrong, record it:

```json
"feedback": {
  "corrections": [
    {
      "source": "triage",
      "topic": "root-cause",
      "upstream": "Triage suggested native Skia bug",
      "corrected": "Actually a missing managed-side validation"
    }
  ]
}
```

### 3. Validate

```bash
# Try pwsh first, fall back to python3
pwsh .github/skills/issue-fix/scripts/validate-fix.ps1 /tmp/skiasharp/fix/{number}.json \
  || python3 .github/skills/issue-fix/scripts/validate-fix.py /tmp/skiasharp/fix/{number}.json
```

> Always use the validation scripts — hand-checking fields is error-prone.

### 4. Persist

```bash
pwsh .github/skills/issue-fix/scripts/persist-fix.ps1 /tmp/skiasharp/fix/{number}.json
```

This copies the JSON to data-cache and handles git automatically (skips in benchmark mode).

---

## Error Recovery

| Issue | Recovery |
|-------|----------|
| Can't reproduce | Ask user for exact environment details |
| Fix causes test failures | Revert, re-analyze root cause |
| `EntryPointNotFoundException` | Rebuild natives after C API changes |
| Can't test on required platform | Use Docker or ask user to verify |
| Proposed fix is a workaround | Stop — find why it works on other platforms |
| Docker build uses cached layers | Use `docker build --no-cache` to rebuild |
| Linker silently skips library | See [debugging-methodology.md](../../../documentation/debugging-methodology.md#native-library-debugging-linux) |

---

## Final Checklist

Before marking complete, verify ALL gates were passed:

- [ ] **Phase 1**: Issue understood, key details extracted
- [ ] **Phase 2**: Draft PR created and used as living document
- [ ] **Phase 3**: Related issues searched, ALL comments read, diagnosis collected
- [ ] **Phase 4**: Baseline reproduction established (re-ran repro or consumed `ai-repro/NNNN.json`)
- [ ] **Phase 5**: Root cause identified and documented
- [ ] **Phase 6**: Minimal fix implemented
- [ ] **Phase 7**: Build passes, tests pass
- [ ] **Phase 8**: PR finalized with "Fixes #NNNN"
- [ ] **Phase 9**: Fix JSON generated, validated with `validate-fix.ps1`/`.py` (saw ✅), and persisted

---

## Anti-Patterns

See [references/anti-patterns.md](references/anti-patterns.md) for the full list of critical rules (loaded at startup).
