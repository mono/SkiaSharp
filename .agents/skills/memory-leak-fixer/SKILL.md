---
name: memory-leak-fixer
description: >
  Scan SkiaSharp for native ownership / disposal memory leaks AND fix them with a
  red→green regression test. Two combined modes in one skill: (1) SCAN — hunt the
  SkiaSharp leak signature (undisposed SKObject handle, wrong `owns:` flag,
  same-instance double-dispose, unremoved event/handler subscription,
  `fixed`-pointer lifetime) and empirically confirm it; (2) FIX — write
  a failing regression test, implement the minimal idiomatic fix, prove it goes green,
  and open a PR.

  Triggers: "memory leak", "leak scan", "leak hunter", "find leaks", "disposal bug",
  "not disposed", "undisposed handle", "owns flag", "double free", "AccessViolation on
  dispose", "native memory grows", "handle leak", "GC does not collect SKObject",
  "fix the leak", any request to proactively find or fix SkiaSharp memory/disposal leaks.

  For a user-reported functional bug that is not a leak, use `issue-fix` instead.
---

# Memory Leak Fixer

Proactively **find** and **fix** memory leaks in SkiaSharp — a thin managed
wrapper over native Skia, so its recurring, high-impact leak family is **native
ownership / disposal correctness** — the C# binding failing to dispose, own, pin, or root a
native object correctly — not the managed view-retention leaks a pure-managed app framework
worries about. This skill hunts that family and produces a validated fix.

**Scope: managed C# only.** Work only in the code SkiaSharp owns — the C# bindings
(`binding/**`) and view layers (`source/**`). Everything under `externals/skia/**` is
upstream Skia (including our C shim): out of scope, not buildable on a standard runner, and
handled by a separate process. Every candidate must be provable and fixable from C#.

Read [`documentation/dev/memory-management.md`](../../../documentation/dev/memory-management.md)
first — it is the authoritative model (pointer types, `owns:` flag, ref-count rules, the
same-instance-return contract). This skill assumes that model.

The leak catalogue this skill scans against — **11 real families**, each with a description,
why it's bad, a leak→fix code example, and a leak-specific anti-pattern — is in
[`references/types-of-leaks.md`](references/types-of-leaks.md). Read it before scanning
(Phase 1) and consult the matching family when writing a fix (Phase 3).

## Golden rules (non-negotiable)

1. **One leak per run.** Pick the single strongest candidate; do not batch.
2. **Empirical confirmation before any fix.** A fix is only valid if a regression test
   **FAILS on the current tree without the fix and PASSES with it** (red→green, both
   directions). No demonstrated red→green ⇒ no PR.
3. **Never weaken, skip, mute, `[Obsolete]`-hide, or delete a test to make it pass.** If
   the only thing that turns green is a mute, the fix is wrong — reject it.
4. **Never edit generated files or upstream Skia.** `*.generated.cs` and everything under
   `externals/skia/**` (including our C shim) are off-limits — this skill is **managed-C#
   only**. Every fix lives in `binding/**` or `source/**`.
5. **ABI stability.** Add overloads / methods; never change or remove a public signature.
6. **Honest scope note.** In every issue/PR, say whether it is a clear framework bug or a
   usage footgun the framework could harden — and what is *empirically proven* vs
   *statically reasoned*.
7. **Finding nothing is the expected outcome.** SkiaSharp is mature and heavily hardened;
   there is **no planted/seeded bug** waiting to be found. Most runs should end with **no
   candidate**. Never invent a leak, never rationalize deliberately-hardened, well-documented
   code as "decoys," and never lower the evidence bar to force a result. A quiet run is a
   first-class success — report it and emit a single `noop` (see Phase 5).

---

## Mode selection

Run the phases in order. The skill has two entry points:

| You were asked to… | Start at | Notes |
|---|---|---|
| Find a leak (scan only) / file an issue | Phase 1 → 2 → (report) | Stop after confirmation; file `[memory-leak]` issue. |
| Fix a known/reported leak | Phase 0 → 3 → 4 → 5 | The issue already exists — consume its retention path, still enforce red→green, then open the **PR only** with `Fixes #<that issue number>`. |
| Scan **and** fix (the default, and what the workflow does) | Phase 0 → 1 → 2 → 3 → 4 → 5 | End-to-end: hunt → prove → fix → **file the finding as an issue and open a linked draft PR** that closes it (`Fixes #…`). |

---

## Phase 0 — Setup

> **CI runner reality:** this skill fixes **managed C# only** (`binding/**`, `source/**`).
> The native library is consumed as a **pre-built package** (`externals-download`) — you
> never build native code, so every candidate must be provable and fixable from C#. A leak
> whose only fix is in native / upstream Skia is out of scope: file an issue (Phase 4),
> never open a PR you cannot validate.

1. Confirm the SDK: `dotnet --version`.
2. Restore the pre-built natives so the C# projects build and the tests run:

   ```bash
   dotnet cake --target=externals-download   # pre-built natives for managed-C# work
   ```

---

## Phase 1 — Scan (find ONE candidate)

### 1.1 Choose a focus area (round-robin across runs)
A full 11-family sweep every run is wasteful and the surface is mostly hardened, so start from
ONE focus family and widen only if it's exhausted. Rotate the *starting* family on a
time-based **round-robin** so consecutive runs cover different families. This needs only
`date`, so it behaves identically locally and in CI — no `$GITHUB_RUN_NUMBER` / `$RANDOM`
(which don't exist or aren't deterministic outside GitHub Actions):

```bash
# Round-robin: advance one family every hour, cycling through all 11.
DOY=$(date -u +%j); HOUR=$(date -u +%H)         # day-of-year + hour, both zero-padded
FOCUS=$(( (10#$DOY * 24 + 10#$HOUR) % 11 ))      # 10# forces base-10
echo "focus family: $FOCUS"
```
The `10#` prefix is **required**: `date` zero-pads `%j`/`%H`, and `$(( 08 ))` is an
invalid-octal error without it. For a targeted local run, skip the rotation and just name the
family you want.

Every family is drawn from a **real, historical SkiaSharp leak fix**. Now open
**[references/types-of-leaks.md](references/types-of-leaks.md)** and load family `#FOCUS`: its
**Where to look** line gives the path + grep starting points, and the rest of the entry is the
description, why-it's-bad, a leak→fix example, and the per-family anti-pattern. **Read that
family before scanning.** If it's exhausted (its leaks are already open issues/PRs — see 1.3),
advance to the next index and load that family.

### 1.2 Establish the retention/ownership path
For each candidate write the precise path **with `file:line` citations**:
- Native-handle leaks: `creation site → escape path → missing Dispose/unref`.
- `owns:` bugs: the P/Invoke name that produced the handle (`_new_`/`_create` returns an
  owned object; `_get_`/property-style returns a borrowed pointer) vs the `owns:` value the
  C# wrapper passed.
- Views retention: `long-lived root → subscription/handler → transient view`, and the
  unload path that should have detached but doesn't.

**Skip if already weak/correct:** uses `WeakEventHandler`/`WeakReference`/
`ConditionalWeakTable`, or the ownership already matches the memory-management rules.

### 1.3 De-dup against this project's own open issues/PRs
Before confirming, fetch and skip anything already covered. **Search two ways** — real
SkiaSharp leak fixes are usually filed as `[BUG] …` (not `[memory-leak] …`), so the
title-prefix search alone will miss them. Also search by the specific **type/API name**:

```bash
# 1. Prior runs of THIS workflow (our own prefix):
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[memory-leak]" in:title' \
  --state open --limit 100 --json number,title,body
gh pr list --repo "$GITHUB_REPOSITORY" --search '"[memory-leak]" in:title' \
  --state open --limit 100 --json number,title,body

# 2. Human-reported coverage of the SAME api/type (the important check):
#    e.g. for the Blob.FromStream candidate below, this surfaces open PR #3473.
gh issue list --repo "$GITHUB_REPOSITORY" --search 'Blob.FromStream in:title,body' --state open --json number,title
gh pr    list --repo "$GITHUB_REPOSITORY" --search 'Blob.FromStream in:title,body' --state open --json number,title
```

A candidate is OUT only if an **open** issue/PR already covers the same
handle / ownership path (by our prefix OR by the api/type name). A candidate whose only
prior item is CLOSED may be re-filed. **Worked example:** the `HarfBuzzSharp.Blob.FromStream`
`fixed`-pointer leak (family 4) is a genuine, still-present bug — but open PR #3473 "Make
Blob.FromStream GC safe" already fixes it, so it is OUT: stand down, do **not** open a
duplicate PR, emit a `noop`.

Pick the ONE strongest candidate. If none is convincing, **stop** — a quiet run is a success
(the surface is hardened; the value is catching *new* leaks as code lands). Do not keep
digging past a reasonable single pass hoping to manufacture a finding: report the quiet result
and emit a `noop` (Phase 5).

---

## Phase 2 — Prove (empirical confirmation)

Every in-scope leak is observable from **managed** code, so prove it with a `WeakReference` +
forced-GC probe that mirrors the existing memory tests. Prove it against the **shipped
`SkiaSharp` NuGet** in a throwaway project — no source build, no display:

```bash
mkdir -p /tmp/leakprobe && cd /tmp/leakprobe
```

`leakprobe.csproj` referencing `<PackageReference Include="SkiaSharp" Version="*" />` — the
floating `*` resolves to the **latest stable** SkiaSharp on nuget.org automatically (use
`Version="*-*"` to include the latest preview) — plus `xunit` + `Microsoft.NET.Test.Sdk`,
then a single `[Fact]` that:
1. runs **Control** (correct usage), **Leaky** (the suspect path), **Mitigation** (the
   proposed workaround), each allocating N subjects tracked by `WeakReference`;
2. forces GC (`for (i=0;i<6;i++){ GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }`);
3. asserts the leaky subjects stay **alive** while control + mitigation are **collected**.

```bash
cd /tmp/leakprobe && dotnet test --logger "console;verbosity=normal"
```

- Passes ⇒ leak confirmed. Failed to build / assertions don't hold ⇒ hypothesis wrong;
  iterate once on another candidate or stop.
- For undisposed-handle leaks, prefer proving the wrapper is **not collected /
  `Dispose` is never reached**; the in-repo equivalent uses `SKObject.GetInstance<T>(handle,
  out _)` + `CollectGarbage()` (see `tests/Tests/SkiaSharp/SKObjectTest.cs`).

---

## Phase 3 — Fix (red→green regression test + minimal change)

### 3.1 Write the failing test FIRST (red)
Add a focused regression test to the console test project (source lives under `tests/Tests/`,
run via `tests/SkiaSharp.Tests.Console`). Model it on existing disposal/leak tests:
- `AssertEx.EventuallyGC(weakRef, …)` — GC-based (`tests/Tests/Xunit/AssertEx.cs`).
- `SKObject.GetInstance<T>(handle, out inst)` + `CollectGarbage()` — wrapper lifecycle
  (`tests/Tests/SkiaSharp/SKObjectTest.cs`).
- For views: the handler pattern in
  `tests/SkiaSharp.Tests.Devices/Tests/Maui/MemoryLeakTests.cs`.

Build and **confirm the test FAILS** on the current tree (proves it catches the leak):

```bash
dotnet cake --target=externals-download      # pre-built natives
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~<YourTestName>"
```

If a test you *expected* to be red is green, your hypothesis is wrong — go back to Phase 1.

### 3.2 Implement the minimal idiomatic fix
Apply the **Fix (✓)** for the matching family in
[`references/types-of-leaks.md`](references/types-of-leaks.md) — every family has a worked
before/after there. Then re-read that family's **Watch out (❌ don't):** note: it names the
specific *wrong fix* that turns one leak into another (an unconditional `Dispose`, flipping
`owns:` blind, nulling a field before disposing, a pinned `GCHandle` where a plain field
suffices, …).

Touch only the minimal code, and keep it inside `binding/**` / `source/**`. Never change a
public signature to fix ownership — add an overload or fix internals (ABI stability). If the
only correct fix is in native / upstream Skia, **stop and file an issue** (Phase 4) — this
skill does not open native PRs.

### 3.3 Confirm green + no regressions
Rebuild and re-run the regression test (now PASSES) plus neighbouring tests:

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~<YourTestName>"
# then a wider relevant slice, e.g. the type's test class, to catch regressions
```

Enforce red→green **in both directions**: revert the fix ⇒ red; re-apply ⇒ green.

### 3.4 Self-review gate — before you open the PR
Run this checklist **before** committing or opening the PR, so a bad attempt is dropped now
instead of pushed and reverted. If any box can't be ticked, **fix it or stand down** (emit a
`noop`) — do not open the PR.

- [ ] The test genuinely goes **red without the fix, green with it**, both directions
      (§3.3) — not green because a test was muted, `[Obsolete]`-hidden, skipped, or weakened.
- [ ] The fix is inside `binding/**` / `source/**` only — no `*.generated.cs`, no
      `externals/skia/**`, no native / upstream change.
- [ ] **No public signature changed** — overloads / internals only (ABI stable).
- [ ] The matching family's **Watch out (❌ don't):** note in
      [`references/types-of-leaks.md`](references/types-of-leaks.md) does **not** describe
      what you just did (no unconditional same-instance `Dispose`, no blind `owns:` flip, no
      field nulled before dispose, no pinned `GCHandle` where a plain field suffices, …).
- [ ] This is a real leak with a citable path — not hardened, documented code rationalised
      as a "decoy," and not a finding manufactured to avoid a quiet run.
- [ ] Not already covered by an open issue/PR (§1.3).

All ticked ⇒ proceed to Phase 4 (file the finding, then open the PR). Any unticked ⇒ no PR
(fix it, file the finding as an issue on its own, or `noop`).

---

## Phase 4 — File the finding, then open the linked fix PR

A confirmed, managed-C#-fixable leak produces **two linked safe outputs** so the *finding* and
the *fix* are tracked separately and the issue **auto-closes when the PR merges**.

### 4.1 The issue — the finding
Emit a `create_issue` that describes the **leak, not the fix**. Give it a `temporary_id`
(format `aw_` + 3–12 letters/digits/underscores, e.g. `aw_leak1`) so the PR can reference it
before its real number exists. Body (markdown):
- **AI-generated banner** naming this workflow + the `memory-leak-fixer` skill.
- **Family**, and the **retention/ownership path** with `file:line` citations.
- **Evidence**: the Phase 2 proof — the probe you ran and its alive/collected counts.
- **Scope note**: framework bug vs footgun; empirically-proven vs statically-reasoned; ABI impact.

### 4.2 The PR — the fix
Create a feature branch (`dev/memory-leak-<short-desc>`), commit the test + fix, and open a
**draft** `create_pull_request`. Body (markdown):
- **AI-generated banner** naming this workflow + skill.
- **The fix**: what changed and why it is the idiomatic pattern (point at the family's `Fix ✓`).
- **Proof (red→green)**: the failing-then-passing test and the exact `dotnet test` commands.
- **A closing keyword on its own line so merging auto-closes the finding:**
  - **Scan-and-fix** (you filed the issue in 4.1 this run): `Fixes #<temporary_id>` — e.g.
    `Fixes #aw_leak1`. gh-aw rewrites it to the real issue number once the issue is created.
  - **Fix-a-known-leak** (a maintainer supplied a real `issue_number`): the issue already
    exists — **do not file a new one**; open the PR only, with `Fixes #<that number>`.

### 4.3 Out of scope (native / upstream only)
If the leak is real but the only correct fix lives under `externals/skia/**` (incl. the C
shim), do **not** open a PR. Emit the `create_issue` from 4.1 **alone** — finding plus the
proposed native fix — so nothing is lost.

---

## Phase 5 — Report

Write a short summary: which family, the candidate (`file:line`), the proof result, and the
resulting issue + PR links. When run from the agentic workflow, append this to the run's step
summary.

**End with the right safe output(s):**
- **Confirmed + managed-C# fix** → the **issue + PR pair** from Phase 4 (the PR body carries
  `Fixes #…` so merging closes the issue). In *fix-a-known-leak* mode the issue already exists,
  so it is the **PR alone**.
- **Confirmed but native/upstream-only fix** → the **`create-issue`** alone (finding + proposal).
- **Quiet run** (no convincing candidate) **or** a **dry run** → a single **`noop`** carrying
  this summary.

A `noop` is the correct "nothing to do / analysis only" signal — never finish with no safe
output, which makes the run look incomplete.
