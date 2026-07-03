---
name: memory-leak-fixer
description: >
  Scan SkiaSharp for native ownership / disposal memory leaks AND fix them with a
  red→green regression test. Two combined modes in one skill: (1) SCAN — hunt the
  SkiaSharp leak signature (undisposed SKObject handle, wrong `owns:` flag, C-API
  ref-count mismatch, same-instance double-dispose, unremoved event/handler
  subscription, `fixed`-pointer lifetime) and empirically confirm it; (2) FIX — write
  a failing regression test, implement the minimal idiomatic fix, prove it goes green,
  and open a PR.

  Triggers: "memory leak", "leak scan", "leak hunter", "find leaks", "disposal bug",
  "not disposed", "undisposed handle", "owns flag", "double free", "AccessViolation on
  dispose", "native memory grows", "handle leak", "GC does not collect SKObject",
  "fix the leak", any request to proactively find or fix SkiaSharp memory/disposal leaks.

  For a user-reported functional bug that is not a leak, use `issue-fix` instead.
---

# Memory Leak Fixer — SkiaSharp

Proactively **find** and **fix** memory leaks in SkiaSharp. SkiaSharp is a thin managed
wrapper over native Skia, so its recurring, high-impact leak family is **native
ownership / disposal correctness** — not the managed view-retention leaks a pure-managed
app framework worries about. This skill hunts that family and produces a validated fix.

Read [`documentation/dev/memory-management.md`](../../../documentation/dev/memory-management.md)
first — it is the authoritative model (pointer types, `owns:` flag, ref-count rules, the
C-API `sk_ref_sp` / `.release()` conventions). This skill assumes that model.

## Golden rules (non-negotiable)

1. **One leak per run.** Pick the single strongest candidate; do not batch.
2. **Empirical confirmation before any fix.** A fix is only valid if a regression test
   **FAILS on the current tree without the fix and PASSES with it** (red→green, both
   directions). No demonstrated red→green ⇒ no PR.
3. **Never weaken, skip, mute, `[Obsolete]`-hide, or delete a test to make it pass.** If
   the only thing that turns green is a mute, the fix is wrong — reject it.
4. **Never edit generated files or upstream Skia.** `*.generated.cs` and everything under
   `externals/skia/**` except our C shim (`externals/skia/src/c/**`,
   `externals/skia/include/c/**`) are off-limits. Regenerate with `pwsh ./utils/generate.ps1`
   after any C-API change — never hand-edit the generated bindings.
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
| Fix a known/reported leak | Phase 0 → 3 → 4 → 5 | Consume the issue's retention path; still enforce red→green. |
| Scan **and** fix (the default, and what the workflow does) | Phase 0 → 1 → 2 → 3 → 4 → 5 | End-to-end: hunt → prove → fix → PR. |

---

## Phase 0 — Setup

> **CI runner reality:** the agentic workflow checks out the skia submodule read-only
> (`checkout: submodules: true`), so you **can read** our C shim under `externals/skia/src/c`
> and `externals/skia/include/c` to verify a managed `owns:` flag against the real C ref-count
> contract. You **cannot build** native code on the runner, though — native tests run against
> pre-built packages (`externals-download`). So prefer managed-C# fixes you can validate; a fix
> that must change the C shim is issue-only (Phase 4).

1. Confirm the SDK: `dotnet --version`.
2. Decide the **fix layer** you are willing to validate this run (this gates which
   candidates are in scope — see the table). For a fix confined to **managed C#**
   (`binding/**/*.cs`, `source/**` views), you can validate on a standard runner with
   pre-built natives:

   ```bash
   dotnet cake --target=externals-download   # C#-ONLY fixes. FORBIDDEN after native changes.
   ```

   For a fix that touches the **C API** (`externals/skia/src/c`, `externals/skia/include/c`)
   you MUST rebuild natives from source (`dotnet cake --target=externals-{platform} --arch={arch}`)
   — `externals-download` is forbidden and produces `EntryPointNotFoundException`.

| Candidate fix lives in… | Validate with | Runner-friendly? |
|---|---|---|
| C# binding / views (`owns:` flag, missing `Dispose`, same-instance guard, event teardown, `GCHandle` pin) | `externals-download` + `dotnet test` | ✅ yes — preferred |
| C API shim (`sk_ref_sp` / `.release()` / delete-vs-unref) | `externals-{platform}` source build + `dotnet test` | ⚠️ heavy — needs native build |

---

## Phase 1 — Scan (find ONE candidate)

### 1.1 Rotate the focus area
So successive runs explore different surface, pick a rotating focus:

```bash
FOCUS=$(( ${GITHUB_RUN_NUMBER:-$RANDOM} % 12 ))
echo "focus family: $FOCUS"
```

Each family below is drawn from a **real, historical SkiaSharp leak fix** (issue/PR cited),
so the hunt targets patterns that have actually shipped as bugs here — not hypotheticals.

| # | Leak family (SkiaSharp signature) | Where to look | Grep starting points · real cases |
|--:|---|---|---|
| 0 | **Undisposed native handle** — a factory/getter/cache creates an *owned* or *ref-counted* `SKObject` that escapes without being disposed (or is held in a static/instance cache never cleared). | `binding/SkiaSharp/**` | `grep -rnE "GetObject\(|new SK[A-Za-z]+\(" binding/SkiaSharp` (then trace ownership) |
| 1 | **Wrong `owns:` flag** — `owns: true` on a *borrowed*/getter return (premature dispose / double-free) OR an owned `new`/`_new_` handle wrapped `owns: false` (leak). | `binding/SkiaSharp/**` | `grep -rnE "owns: *(true|false)|GetOrAddObject" binding/SkiaSharp` |
| 2 | **C-API ownership mismatch** — C++ expects `sk_sp<T>` but no `sk_ref_sp`; a returned `sk_sp` missing `.release()`; `delete` used on a `SkRefCnt`/`SkNVRefCnt` type; unref vs delete. | `externals/skia/src/c/**` | `grep -rnE "\.release\(\)|sk_ref_sp|delete As|SkSafeUnref" externals/skia/src/c` |
| 3 | **Same-instance double-dispose** — a method that may return the *same* instance (`Subset`, `ToRasterImage`, …) whose caller disposes both source and result. | `binding/SkiaSharp/**` | `grep -rnE "Subset\|ToRasterImage\|== source\|!= source" binding/SkiaSharp` |
| 4 | **Managed retention (Views)** — a handler / control / `SKObject`-backed view subscribes to an event (`PaintSurface`, `PropertyChanged`, invalidation ticker, platform peer) with **no teardown on unload**, rooting the transient; or a base-class `Dispose` is never chained. | `source/SkiaSharp.Views*/**` | `grep -rnE "\+= \|event \|WeakReference\|base\.Dispose\|Detach" source/SkiaSharp.Views*` · **cf. #3309 (SKGLElement missing `base.Dispose()`), #2955, #2472, #1095** |
| 5 | **`fixed`-pointer lifetime** — a temporary `fixed` pointer handed to native code that *stores* it beyond the block (`MemoryMode.ReadOnly`, non-copying); GC then moves/frees the array. | `binding/**`, `source/**` | `grep -rnE "fixed *\(" binding source` · **canonical: `HarfBuzzSharp.Blob.FromStream` (open #3472 / PR #3473)** |
| 6 | **Finalizer / collection ordering** — a child wrapper holds a *raw* pointer into a parent; if the parent is GC'd/disposed first, use-after-free. Needs `GC.KeepAlive` or an owns-link back to the parent. | `binding/SkiaSharp/**` | `grep -rnE "GC.KeepAlive\|internal .* Handle" binding/SkiaSharp` (flag children missing KeepAlive) · **cf. #3796 (SKPath/SKPathBuilder), #3291 (SKAutoCanvasRestore)**. *Real un-filed examples this workflow surfaced:* `SKRegion.SpanIterator` keeps no parent field though its sibling `RectIterator`/`ClipIterator` do; `SKPixmap.ExtractSubset`/`With*` don't propagate `pixelSource` the way `PeekPixels` does. |
| 7 | **Clone / copy double-free** — a `Clone()`/copy that *shares* one native pointer between two managed wrappers that both dispose it. Must mint a fresh native (`_clone`) wrapped `owns:true`. | `binding/SkiaSharp/**` | `grep -rnE "Clone\|MemberwiseClone\|_clone" binding/SkiaSharp` · **cf. #2904 (SKPaint.Clone), #2899** |
| 8 | **Disposing native statics/singletons** — an *immortal* native object (default typeface, srgb color space/filter, blend-mode/empty-data singletons) reached via a cache that ISN'T dispose-protected, so `DisposeNative` unrefs an object it must never touch. | `binding/SkiaSharp/**` | `grep -rnE "GetDisposeProtectedObject\|unrefExisting\|CreateSrgb\|_empty\|Empty" binding/SkiaSharp` (flag singletons NOT dispose-protected) · **cf. #1863, #4080, #1224, #3730** |
| 9 | **Field not nulled on dispose** — a `Dispose`/`DisposeManaged` that frees a native child but leaves the managed field pointing at it, enabling a later double-dispose or blocking GC of a graph. | `binding/SkiaSharp/**` | `grep -rnE "DisposeManaged\|= null;" binding/SkiaSharp` (check disposed children are nulled) · **cf. #1256, #1344** |
| 10 | **Managed stream / callback / delegate-proxy lifetime** — a `SKManagedStream`/`SKManagedWStream`/`SKAbstract*Stream`, delegate/function-pointer proxy, or pinned `GCHandle` handed to native code but freed *too early* (dangling callback) or *never* (leak). | `binding/SkiaSharp/**` | `grep -rnE "DelegateProxies\|GCHandle\|ManagedStream\|ReleaseDelegate" binding/SkiaSharp` · **cf. #3589, #2916, #996, #2446** |
| 11 | **Allocation-failure path** — a factory that wraps and returns a managed object even when the native create returned `null`/failed, or leaks a half-built object on the error path. | `binding/SkiaSharp/**` | `grep -rnE "GetObject\(\s*[a-z]\|if \(handle == " binding/SkiaSharp` (check null-return handling) · **cf. #1784, #1642** |

Treat the table as a starting point, not a cage. If the focus family is exhausted (its
leaks are already open issues/PRs — see 1.3), advance to the next index.

### 1.2 Establish the retention/ownership path
For each candidate write the precise path **with `file:line` citations**:
- Native-handle leaks: `creation site → escape path → missing Dispose/unref`.
- `owns:` bugs: the C-API function that produced the handle (owned `new` vs borrowed
  getter vs ref-counted `sk_sp`) vs the `owns:` value the C# wrapper passed.
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
`fixed`-pointer leak (family 5) is a genuine, still-present bug — but open PR #3473 "Make
Blob.FromStream GC safe" already fixes it, so it is OUT: stand down, do **not** open a
duplicate PR, emit a `noop`.

Pick the ONE strongest candidate. If none is convincing, **stop** — a quiet run is a success
(the surface is hardened; the value is catching *new* leaks as code lands). Do not keep
digging past a reasonable single pass hoping to manufacture a finding: report the quiet result
and emit a `noop` (Phase 5).

---

## Phase 2 — Prove (empirical confirmation)

The proof technique depends on whether the leak is observable from managed code.

### 2A — Managed-observable leaks → shipped-package probe, no native build

For undisposed wrappers, views retention, same-instance, and `fixed`-pointer leaks, mirror
the existing memory tests: `WeakReference` + forced GC. You can prove these against the
**shipped `SkiaSharp` NuGet** in a throwaway project — no source build, no display:

```bash
mkdir -p /tmp/leakprobe && cd /tmp/leakprobe
```

`leakprobe.csproj` referencing `<PackageReference Include="SkiaSharp" Version="3.*" />`
(pick a version that restores from nuget.org) + `xunit` + `Microsoft.NET.Test.Sdk`, then a
single `[Fact]` that:
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

### 2B — Native-only leaks (C-API `sk_ref_sp` / `.release()` / delete-vs-unref)

These grow native memory that a managed `WeakReference` can't see. Proof is either a
source-built regression test that asserts on handle/ref-count/disposal state, or a tight,
citable violation of a memory-management rule that the Phase 3 red→green test captures. Be
explicit that this is native-level and requires the source native build (Phase 0 heavy path).

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
dotnet cake --target=externals-download      # C#-only fixes; skip for native-source path
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~<YourTestName>"
```

If a test you *expected* to be red is green, your hypothesis is wrong — go back to Phase 1.

### 3.2 Implement the minimal idiomatic fix
Pick the fix that matches the family (all from the memory-management model):

| Family | Idiomatic fix |
|---|---|
| Undisposed handle | Dispose the escaped object, or make the wrapper own+dispose it; clear the cache on the missing path. |
| Wrong `owns:` | Flip the flag to match the C-API contract (`owns: false` for borrowed/getters; `true` only for owned `_new_`). |
| C-API mismatch | Add `sk_ref_sp(...)` where C++ takes `sk_sp<T>`; add `.release()` on `sk_sp` returns; replace `delete` with `SkSafeUnref` for ref-counted types. Then `pwsh ./utils/generate.ps1`. |
| Same-instance | Guard: `if (result != source) source.Dispose();` — never dispose a same-instance return. |
| Views retention | Detach the subscription on unload/dispose, or route it through a weak subscription mirroring existing view teardown. Chain `base.Dispose(disposing)` if the base owns native resources (cf. #3309). |
| `fixed`-pointer | Replace the temporary `fixed` with a pinned `GCHandle.Alloc(obj, GCHandleType.Pinned)`, hand native `AddrOfPinnedObject()`, and `Free()` the handle in the release delegate (cf. Blob.FromStream / PR #3473). |
| Finalizer/ordering | Add `GC.KeepAlive(parent)` after the P/Invoke, or give the child an owns-link so the parent can't be collected first. |
| Clone/copy | Mint a fresh native via the `_clone` C-API wrapped `owns:true` (never share one pointer across two wrappers). |
| Dispose static/singleton | Route the accessor through `GetDisposeProtectedObject(..., owns:false, unrefExisting:false)` so `DisposeNative` skips the immortal native. |
| Field-not-nulled | Null the managed field after disposing the native child so a later dispose is a no-op and the graph can be collected. |
| Stream/callback/proxy | Keep the `GCHandle`/proxy rooted for exactly the native object's lifetime; free it in the release/destroy delegate — not before, not never. |
| Allocation-failure | Return `null` (factory) when the native create returns 0; free any half-built partials on the error path. |

Touch only the minimal code. If the fix is in the C API, regenerate bindings and rebuild
natives from source (do **not** `externals-download`).

### 3.3 Confirm green + no regressions
Rebuild and re-run the regression test (now PASSES) plus neighbouring tests:

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~<YourTestName>"
# then a wider relevant slice, e.g. the type's test class, to catch regressions
```

Enforce red→green **in both directions**: revert the fix ⇒ red; re-apply ⇒ green.

---

## Phase 4 — Open the PR

Create a feature branch (`dev/memory-leak-<short-desc>`), commit the test + fix, and open a
**draft** PR. Body (markdown):

- **AI-generated banner** naming this workflow/skill.
- **The leak**: family, and the retention/ownership path with `file:line` citations.
- **Proof (red→green)**: the failing-then-passing test, the exact `dotnet test` commands,
  and (for 2A) the alive/collected counts.
- **The fix**: what changed and why it is the idiomatic SkiaSharp pattern.
- **Scope note**: framework bug vs footgun; empirically-proven vs statically-reasoned;
  ABI impact (should be none).
- `Fixes #N` when fixing a filed issue.

If the strongest candidate's fix would require a **native/C-API source build** you cannot
validate this run, do **not** open an unvalidated PR — file a `[memory-leak]` issue with the
Phase 1–2 evidence and the proposed fix instead, so nothing is lost.

---

## Phase 5 — Report

Write a short summary: which family, the candidate, proof result, and the PR/issue link.
When run from the agentic workflow, append this to the run's step summary.

**Always end with exactly one safe output.** If you shipped a fix, that is the
`create-pull-request`; if you filed an issue, that is the `create-issue`; if the run was quiet
(no convincing candidate) **or** a dry run, emit a single **`noop`** carrying this summary. A
`noop` is the correct "nothing to do / analysis only" signal — never finish with no safe
output, which makes the run look incomplete.

## Anti-patterns (reject your own attempt if you catch these)

| Anti-pattern | Why it's wrong |
|---|---|
| PR without a demonstrated red→green test | Unproven; could be a false positive. |
| Muting/`[Obsolete]`/skipping a test to go green | Hides the leak instead of fixing it. |
| `externals-download` after touching C-API | `EntryPointNotFoundException`; stale natives. |
| Editing `*.generated.cs` by hand | Overwritten on regenerate; use `generate.ps1`. |
| Changing a public signature to "fix" ownership | ABI break — add an overload or fix internals. |
| Disposing a same-instance return unconditionally | Double-free crash. |
| Assuming a bug must exist / calling hardened code "decoys" | There is no seeded bug; manufacturing a finding produces false positives. |
