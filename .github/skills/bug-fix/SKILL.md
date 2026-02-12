---
name: bug-fix
description: >
  Fix bugs in SkiaSharp C# bindings. Structured workflow for investigating, fixing,
  and testing bug reports.
  
  Triggers: Crash, exception, AccessViolationException, incorrect output, wrong behavior,
  memory leak, disposal issues, "fails", "broken", "doesn't work", "investigate issue",
  "fix issue", "look at #NNNN", any GitHub issue number referencing a bug.
  
  For adding new APIs, use `add-api` skill instead.
---

# Bug Fix Skill

**Bug pipeline: Step 3 of 3 (Fix).** See [`documentation/bug-pipeline.md`](../../../documentation/bug-pipeline.md).

Fix bugs in SkiaSharp with minimal, surgical changes.

## ⛔ CRITICAL: SEQUENTIAL EXECUTION REQUIRED

> **🛑 PHASES MUST BE EXECUTED IN STRICT ORDER. NO PARALLELIZATION. NO REORDERING.**
>
> ```
> Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6 → Phase 7 → Phase 8
> ```
>
> **STOP** at each phase gate. Do not proceed until gate criteria are met.
> **Phases may be abbreviated** when `ai-triage/{n}.json` and/or `ai-repro/{n}.json` exist — but you must explicitly consume them and meet the gate with evidence (don’t redo the work).
> **NEVER** say "in parallel" — phases are strictly sequential.
> **NEVER** start *new* research (Phase 3) before PR exists (Phase 2).

---

## Workflow Overview

```
1. Understand   → Fetch issue, consume ai-triage/ai-repro if present
2. Create PR    → 🛑 STOP: Create PR before ANY *new* research
3. Research     → Delta research (triage already did first-pass)
4. Reproduce    → Prefer ai-repro project; Docker only if needed
5. Investigate  → Root cause (guided by repro version matrix + triage codeInvestigation)
6. Fix          → Minimal change
7. Test         → Regression test + existing tests
8. Finalize     → Rewrite PR description, link all fixed issues
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

### ⛔ AFTER PHASE 1: STOP AND CREATE PR

> **🛑 DO NOT search for related issues yet. DO NOT investigate yet.**
> **🛑 Your ONLY next action is Phase 2: Create the Draft PR.**
> 
> The PR must exist BEFORE any research or investigation begins.

---

## Phase 2: Create Draft PR

> 🛑 **THIS PHASE IS BLOCKING. Complete it before ANY other work.**
> 
> Do NOT:
> - Search for related issues (that's Phase 3)
> - Read comments on other issues (that's Phase 3)
> - Look at code (that's Phase 5)
> - Try to reproduce (that's Phase 4)
>
> Do ONLY:
> - Create branch
> - Push empty commit
> - Create draft PR with template
> - Add "copilot" label

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

### ⛔ AFTER PHASE 2: Verify PR exists before continuing

> **🛑 STOP. Verify the PR URL exists before proceeding to Phase 3.**
> 
> Only after confirming the PR is created should you begin research.

---

## Phase 3: Research Related Issues (delta)

> 🛑 **PREREQUISITE: Phase 2 must be complete. PR must exist.**
> 
> If you have not created the draft PR yet, STOP and go back to Phase 2.

If `ai-triage/NNNN.json` exists, it already contains:
- related issues discovered during workaround/duplicate search
- code investigation entry points
- workaround proposals and missing info

**Your job in Phase 3 is delta research only:**
- confirm/expand on the *most relevant* related issues (especially ones with diagnosis in comments)
- run additional searches only if triage confidence is low, triage is stale, or repro contradicts triage

> 🛑 **CRITICAL: This phase often SOLVES the bug.**
> 
> The community may have already diagnosed the root cause in issue comments.
> READ ALL COMMENTS on the most relevant related issues before investigating yourself.

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

**If a related issue already contains the root cause diagnosis, document it in the PR,
but you MUST still proceed to Phase 4 (Reproduce) to validate the hypothesis.**

---

## Phase 4: Reproduce (prefer ai-repro)

> ⛔ **REPRODUCTION IS MANDATORY.**
>
> This phase is satisfied either by:
> - **Re-running** the minimal repro locally, OR
> - **Consuming an existing** `ai-repro/NNNN.json` that already reproduced the issue on the relevant version/platform and includes the minimal repro source.
>
> Even if you think you know the root cause from Phase 3:
> - Community diagnosis could be a workaround, not the real fix
> - The hypothesis could be wrong or incomplete
> - You need evidence, not assumptions

If `ai-repro/NNNN.json` exists and `conclusion` is `reproduced`/`wrong-output`:
- Rehydrate the repro source from `reproductionSteps[].filesCreated[].content` into a local folder (e.g., `/tmp/repro-NNNN/`) and run it.
- Prefer this NuGet-based repro as the baseline; use Docker only if the host cannot exercise the target platform.

If no `ai-repro` exists:
- Reproduce using the **same approach as bug-repro** (standalone NuGet project first; Docker only when needed).

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

### 5.3 For C# Issues: Locate the Code

```bash
grep -rn "MethodName" binding/SkiaSharp/
grep -r "sk_.*methodname" binding/SkiaSharp/
```

### 5.4 Workaround vs Root Cause

> ⚠️ **CRITICAL: Don't mistake a workaround for the root cause fix.**

**Example from #3369:**
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

Tests MUST pass. Verify fix on original platform.

### ✅ GATE: Do not proceed until you have:
- [ ] Built successfully
- [ ] All tests pass
- [ ] Verified fix resolves the original issue (if possible)

---

## Phase 8: Finalize

Rewrite PR description using final template from [references/pr-templates.md](references/pr-templates.md).

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

### 1. Generate JSON

Write to `/tmp/fix-{number}.json`:

- `meta`: schemaVersion `"1.0"`, number, repo, analyzedAt (ISO 8601 UTC)
- `inputs`: `{ triageFile, reproFile }` — paths to upstream files consumed (if any)
- `status`: one of `in-progress`, `fixed`, `cannot-fix`, `needs-info`, `duplicate`
- `summary`: one-line description of what was fixed and how
- `rootCause`: `{ category, area, description, affectedFiles? }` — what was wrong and why
- `changes`: `{ files: [{ path, changeType, summary }], breakingChange, risk }`
- `tests`: `{ regressionTestAdded, testsAdded?, command?, result }`
- `verification`: `{ reproScenario, notes? }` — did the repro scenario pass after the fix?
- `pr`: `{ number?, url, status }` (optional)
- `feedback`: corrections to triage/repro findings (optional, see below)
- `relatedIssues`: other issue numbers fixed or related (optional)

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
pwsh .github/skills/bug-fix/scripts/validate-fix.ps1 /tmp/fix-{number}.json
```

### 4. Persist

```bash
cd .data-cache
mkdir -p repos/mono-SkiaSharp/ai-fix
cp /tmp/fix-{number}.json repos/mono-SkiaSharp/ai-fix/{number}.json
git add repos/mono-SkiaSharp/ai-fix/{number}.json
git commit -m "ai-fix: fix #{number}"
git push  # Rebase up to 3x on conflict
cd ..
```

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
- [ ] **Phase 9**: Fix JSON generated, validated, and persisted
