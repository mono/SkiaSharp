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
Phase 1 (Fetch) → Phase 2 (Assess) → Phase 3 (Reproduce) → Phase 4 (JSON) → Phase 5 (Validate & Persist)
```

---

## Phase 1 — Fetch Issue

1. **Set up data cache** (run once per session):
   ```bash
   pwsh --version    # Requires 7.5+
   [ -d ".data-cache" ] || git worktree add .data-cache docs-data-cache
   git -C .data-cache pull --rebase origin docs-data-cache
   CACHE=".data-cache/repos/mono-SkiaSharp"
   ```
2. **Read the issue:** `cat $CACHE/github/items/{number}.json`
   - Fallback: `gh issue view {number} --repo mono/SkiaSharp --json title,body,labels,comments,state,createdAt,closedAt,author`
3. **Convert** (optional): `pwsh .github/skills/triage-issue/scripts/issue-to-markdown.ps1 $CACHE/github/items/{number}.json > /tmp/issue-{number}.md`
4. **Triage boost** — if `$CACHE/ai-triage/{number}.json` exists, extract `classification.platforms[]`, `evidence.bugSignals`, and `resolution.proposals` as **hints** (verify independently). Record path in `inputs.triageFile`.

---

## Phase 2 — Assess & Plan

### 1. Classify

Read [references/bug-categories.md](references/bug-categories.md) to classify the bug type.

### 2. Extract reporter's version & TFM

- `{reporter_version}`: exact SkiaSharp NuGet version
- `{reporter_tfm}`: target framework (e.g., `net10.0`)
- `{reporter_code}`: reproduction code from the issue
- Reporter's platform/OS

If not stated, use the latest stable release. **.NET is forward-compatible** — `net8.0` libraries work on `net10.0` apps. Never say "doesn't support .NET X" for newer TFMs.

### 3. Environment check

**⚠️ Run `dotnet --info` in `/tmp/repro-{number}/`** (NOT the SkiaSharp repo, which has `global.json` pinning SDK 8.0). Record SDK version, wasm-tools version, and runtime version.

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

**Read the selected platform file.** Follow its Create → Build → Run → Verify steps, substituting `{reporter_version}`, `{reporter_tfm}`, and `{reporter_code}`.

### 5. Plan

Output a brief plan before executing (1-2 sentences: what platform, what version, what approach).

---

## Phase 3 — Reproduce

> **Overview — you will test up to 4 configurations:**
> - **3A:** Reporter's version on primary platform *(always)*
> - **3B:** Latest stable release *(if 3A reproduced)*
> - **3C:** Main branch source *(MANDATORY if 3B still reproduced)*
> - **3D:** Cross-platform verification *(conditional — see table below)*

### 3A. Reproduce with reporter's version

Follow the platform file from Phase 2.4. For each step, capture:

| Field | Limit |
|-------|-------|
| `command` | Exact command (redact paths) |
| `exitCode` | 0=success, non-zero=failure |
| `output` | 2KB success, 4KB failure |
| `filesCreated` | Filename + source code content for repro files |
| `layer` | `setup` / `csharp` / `c-api` / `native` / `deployment` |
| `result` | `success` / `failure` / `wrong-output` / `skip` |

**Step `result` = what actually happened** (technical outcome), not whether it was expected. A build that fails is `result: "failure"` even if that confirms the bug. See [references/anti-patterns.md](references/anti-patterns.md) for details.

**Push hard.** Don't bail early. Only conclude `not-reproduced` after genuinely exhausting approaches.

### 3B. Test on latest release (if reproduced)

> **⚠️ Clean build required:** Create a fresh project directory per version (`/tmp/repro-{number}-latest/`) or `rm -rf bin/ obj/` before building. Never just `sed` the version — stale native binaries produce unreliable results. See [references/anti-patterns.md](references/anti-patterns.md) #7.

Use the same platform strategy from 3A with the latest stable SkiaSharp. Record in `versionResults`.

### 3C. Test on main branch (MANDATORY if reproduced on latest)

> **🛑 Do NOT skip.** Every repro JSON MUST have a `main (source)` entry in `versionResults`.

1. Bootstrap: `[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download`
2. **Build & run the platform-appropriate sample** under `samples/Basic/<platform>/`. Each platform file has a "Main Source Testing (Phase 3C)" section — follow it.
3. Record result. If fixed on main but not released, note the version gap.

> **Clean up:** Revert sample file changes with `git checkout`.

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

---

## Phase 4 — Generate JSON

### 1. Choose conclusion

Read [references/conclusion-guide.md](references/conclusion-guide.md). The conclusion is **factual only** — did the reported behavior occur? Never let editorial judgment affect the conclusion.

| Conclusion | When |
|------------|------|
| `reproduced` | Reported behavior occurred (even if by-design) |
| `wrong-output` | Process succeeds but output incorrect |
| `not-reproduced` | Reported behavior did not occur |
| `needs-platform` / `needs-hardware` | Requires unavailable platform/hardware |
| `partial` / `inconclusive` | Partial or ambiguous results |

### 2. Generate JSON

Write to `/tmp/repro-{number}.json`. Schema: [references/repro-schema.json](references/repro-schema.json)

**Schema rules:**
- `meta.schemaVersion`: `"1.0"`, number, repo (`"mono/SkiaSharp"`), analyzedAt (ISO 8601 UTC)
- **Optional fields: OMIT entirely** — do NOT set to `null`
- `additionalProperties: false`
- `environment.dotnetSdkVersion`: exact SDK from `dotnet --info` (e.g., `"10.0.102"`). Include `wasmToolsVersion` for WASM bugs.
- `versionResults`: include `platform` field (e.g., `"host-macos-arm64"`, `"docker-linux-x64"`)

Conditional: `reproduced` → ≥1 step with `failure`/`wrong-output`. `not-reproduced` → ≥1 `success`. `needs-platform`/`partial`/`inconclusive` → `blockers` required.

Redact paths (`/Users/{name}/` → `$HOME/`), tokens, credentials.

### 3. Feedback (when triage was consumed)

If reproduction contradicts triage, record in `feedback.triageCorrections[]`:
```json
{ "topic": "classification", "upstream": "...", "corrected": "..." }
```

---

## Phase 5 — Validate & Persist

```bash
# Validate
pwsh .github/skills/bug-repro/scripts/validate-repro.ps1 /tmp/repro-{number}.json
# Exit 0=valid, 1=fix+retry, 2=fatal

# Persist
cp /tmp/repro-{number}.json $CACHE/ai-repro/{number}.json
cd .data-cache
git add repos/mono-SkiaSharp/ai-repro/{number}.json
git commit -m "ai-repro: reproduce #{number}"
git push  # Rebase up to 3x on conflict
cd ..
```

### Present summary

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

---

## Anti-Patterns (Critical Rules)

These 7 rules address **non-obvious failure modes** discovered through real skill usage. For the full list with examples, see [references/anti-patterns.md](references/anti-patterns.md).

1. **Source code investigation.** Stop at "did it reproduce." Root cause and fixes are the `bug-fix` skill's job.
2. **Editorial judgment in conclusion.** If the reported behavior occurred, it's `reproduced` — even if by-design. Editorial opinion goes in `notes`, not `conclusion`.
3. **Giving up too early.** Never conclude `not-reproduced` after one attempt. Try different versions, data, platforms. Persistence is this skill's core value.
4. **Mismarking step results.** Step `result` = technical outcome. A build failure is `failure` even if it confirms the bug. See [references/anti-patterns.md](references/anti-patterns.md) for the decision table.
5. **Stopping at build success for WASM.** WASM bugs manifest at RUNTIME in the browser. Serve the app and check browser console with Playwright. Build ≠ runtime.
6. **Assuming TFM incompatibility.** .NET is forward-compatible. `net8.0` works on `net10.0` apps. Never say "doesn't support .NET X" without evidence.
7. **Reusing build artifacts across versions.** Always use fresh project directories or `rm -rf bin/ obj/` between version changes. Stale WASM native binaries cause false results.
