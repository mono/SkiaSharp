# Process-Global Singleton Lifecycle

SkiaSharp exposes a handful of **process-global singleton wrappers** that alias native objects Skia
itself shares for the lifetime of the process:

| Accessor | Native source |
|----------|---------------|
| `SKColorSpace.CreateSrgb()` / `CreateSrgbLinear()` | the global sRGB / sRGB-linear color spaces |
| `SKData.Empty` | the shared empty `SkData` |
| `SKColorFilter.CreateSrgbToLinearGamma()` / `CreateLinearToSrgbGamma()` | the gamma color filters |
| `SKBlender.CreateBlendMode(mode)` | the per-blend-mode blenders |
| `SKFontStyle.Normal` / `Bold` / `Italic` / `BoldItalic` | the preset font styles |
| `SKFontManager.Default` | the platform default font manager |
| `SKTypeface.Default` / `SKTypeface.Empty` | the default / empty typefaces |

Because these native objects are shared, the managed wrappers that front them must never tear down their
native object while other live aliases still point at it: disposing one (directly, via a finalizer, or via
a transitive `DisposeInternal`) would free or unref a native object that other live aliases still point at,
corrupting every other consumer of that handle.

> **This document was originally written for the immortal-latch design (#4119) and updated when the latch
> was removed.** The current implementation makes every singleton a **dispose-protected** wrapper rooted by
> a static field — there is no "immortal" latch. The latch was found to be redundant under the *eager*
> design; the reasoning is in
> [Why no immortal latch](#why-no-immortal-latch-and-why-it-was-redundant). Passages below have been
> updated to match; the Part 2 history deliberately preserves the immortal-era reasoning as the record of
> how we got here.

> **How to read this document.** It is deliberately split into two parts.
> **Part 1 is the current implementation and the reasoning behind each decision** — read this to
> understand what the code does today and why. **Part 2 is the historical record** — the bugs we chased,
> the designs we rejected, the measurements we took, and the testing lessons we learned. Part 2 exists so
> that a future maintainer (or our future selves) can recover *why* we landed here and avoid re-walking
> the same dead ends. This area is touched by virtually every SkiaSharp program, on every platform and
> every interop configuration, so the cost of a wrong change is high and the value of preserving this
> context is correspondingly large.

---

# Part 1 — The current implementation (and why)

## Two requirements that pull in opposite directions

1. **Dispose-safety (getter-route dedup).** A handle the singleton shares can also arrive through an
   unrelated getter — e.g. `sk_image_get_colorspace` can return the *same* native pointer as the global
   sRGB color space. When that handle is looked up it must dedup to the **dispose-protected
   wrapper**, not to a fresh mortal wrapper that a caller could legitimately dispose. This was the
   3.119.x behaviour and it must be preserved: see [memory-management.md](memory-management.md).

2. **No re-entrant static-init crash (#3817).** The singletons have one cross-type dependency: the
   default typeface is resolved through the default font manager and the `Normal` font style. If that
   dependency is expressed as one wrapper type's static constructor reading **another wrapper type**, the
   whole graph sits inside the CLR type-initializer machinery and can deadlock against its re-entrancy
   rules (see [Part 2](#part-2--how-we-got-here-history-rejected-designs-and-learnings)).

The current design satisfies both: every singleton is built **eagerly and exactly once** by its own
type's static constructor, but the one cross-type dependency is resolved at the **raw-handle level** so
that no wrapper static constructor ever reads another wrapper type.

## The design: per-type static constructors over a shared handle holder

Two pieces work together.

### 1. `SkiaSharpStatics` — the raw-handle holder

`binding/SkiaSharp/SkiaSharpStatics.cs` is a plain `static class`. Its static constructor calls **only**
`SkiaApi.sk_*` functions and stores the resulting `IntPtr` handles in `internal static readonly` fields
(`Srgb`, `EmptyData`, `NormalFontStyle`, `DefaultFontManager`, `EmptyTypeface`, `DefaultTypeface`,
the blend-mode handle map, …). It touches **no managed `SKObject` type**.

The one true cross-type dependency — the default typeface — is resolved here, from the font-manager and
`Normal`-style **handles** (not their wrappers):

```csharp
IntPtr matched = SkiaApi.sk_fontmgr_legacy_create_typeface(DefaultFontManager, IntPtr.Zero, NormalFontStyle);
DefaultTypeface = matched == IntPtr.Zero ? EmptyTypeface : matched;
```

Because `SkiaSharpStatics..cctor` depends on no wrapper type, and each wrapper type's `.cctor` reads
only `SkiaSharpStatics` (an `IntPtr`), the static-initializer graph is **acyclic** and the #3817
re-entrancy is gone.

### 2. Each wrapper type's static constructor adopts its handles

Every singleton wrapper type has an **explicit** static constructor that wraps the relevant handle(s)
into dispose-protected `readonly` fields, and its accessors just return those fields:

```csharp
public unsafe class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted
{
    private static readonly SKColorSpace srgb;
    private static readonly SKColorSpace srgbLinear;

    static SKColorSpace()
    {
        srgb = GetDisposeProtectedSingletonObject(SkiaSharpStatics.Srgb);
        srgbLinear = GetDisposeProtectedSingletonObject(SkiaSharpStatics.SrgbLinear);
    }

    public static SKColorSpace CreateSrgb() => srgb;
    public static SKColorSpace CreateSrgbLinear() => srgbLinear;
}
```

Every singleton-creating type uses a `GetDisposeProtectedSingletonObject(IntPtr)` helper, which routes to
the base `GetOrAddDisposeProtectedObject(...)`: it dedups the handle in the `HandleDictionary` and sets
`IgnorePublicDispose` on the resulting wrapper (the wrapper itself is registered in `SKObject`'s
constructor via `RegisterHandle`). The naming is uniform across `SKColorSpace`, `SKData`, `SKColorFilter`,
`SKBlender` and `SKFontManager`. `SKTypeface` reuses its general-purpose `GetDisposeProtectedObject` helper
(the dispose-protected path is identical for the singletons and for match-family results).

`SKTypeface`'s `GetDisposeProtectedObject` helper (routing to `GetOrAddDisposeProtectedObject`) is also
used for *ordinary collectible* typefaces returned from match-family / match-style lookups — those are
dispose-protected (public `Dispose()` ignored) but, unlike the singletons, are **not** rooted by a static
field, so they remain collectible. Dispose-protection is the same; only the static-field root differs.

`SKFontStyle` is special — it is `ISKSkipObjectRegistration`, so it bypasses the dictionary and, for each
preset handle, calls `PreventPublicDisposal()` directly.

`SKPaint.DefaultFont` is the other deliberate exception. Unlike the wrapper singletons above, it is built
**lazily** via `LazyInitializer.EnsureInitialized` (guarded by `defaultFontLock`), *not* in a static
constructor. The reason is the same cross-type rule that motivates the whole design: its factory reads
`SKTypeface.Default.Handle` — the *wrapper*, not a raw handle from `SkiaSharpStatics` — so running it inside
`static SKPaint()` would reintroduce exactly the wrapper-cctor-reads-another-wrapper dependency the rework
exists to forbid. Deferring it to first use moves that read out of the type-initializer graph entirely. The
lazy one-shot still gives a clean happens-before barrier, after which it calls `PreventPublicDisposal()`
directly, just like the eager singletons.

### Why static constructors (and why they must be *explicit*)

- **Lock-free and run-once.** The CLR guarantees a type initializer runs exactly once, on first use of
  the type, in a thread-safe way — with no locks of our own and no orchestrator. The backing fields can
  be `readonly`.
- **The fields can satisfy getter-route dedup.** Requirement 1 needs the dispose-protected wrapper to be
  registered **before** any handle lookup on that type dedups a shared handle. The CLR gives us this
  *only if the type is not `beforefieldinit`*. A type whose statics are written as bare field
  initializers (`static readonly SKColorSpace srgb = ...;`) **is** `beforefieldinit`, which lets the CLR
  run a static **method** body (such as `GetObject`) *before* the field initializers. An **explicit**
  `static SKColorSpace() { ... }` removes `beforefieldinit`, so the cctor is guaranteed to complete
  before the body of any static method on the type runs. **Every singleton wrapper type therefore
  declares an explicit static constructor — do not collapse them into field initializers.**

## The native-library compatibility check lives in `SkiaApi`'s static constructor

The native **library-compatibility check** (`SkiaSharpVersion.CheckNativeLibraryCompatible`) is a
**separate** concern from singleton initialization, with a different lifecycle, and is gated in a
different place: the **explicit static constructor of `SkiaApi`** (`binding/SkiaSharp/SkiaApi.cs`).

```csharp
internal partial class SkiaApi
{
    static SkiaApi ()
    {
        SkiaSharpVersion.CheckNativeLibraryCompatible (true);
    }
}
```

`SkiaApi` is the single internal type through which **every** P/Invoke into `libSkiaSharp` flows. Defining
an explicit `static SkiaApi()` strips the type's `beforefieldinit` flag, so the CLR guarantees the
constructor — and therefore the compatibility check — runs **before the first real `sk_*` call** (the
version probe itself aside; see the re-entrancy note below). This covers *every* consumer, including
code that only touches non-singleton types like `SKBitmap` or `SKCanvas`, structs like `SKMatrix`, and
static utility classes like `SKGraphics` — they all reach native only through `SkiaApi`. The check runs
once; a throw becomes a cached `TypeInitializationException` (inner exception = the clean
`InvalidOperationException` carrying the supported-version-range message), which is the desired one-shot
poison behaviour for an unusable library.

This is deliberately **not** a `[ModuleInitializer]`. A module initializer runs at **assembly load**,
which would force an eager P/Invoke before a consumer has had a chance to configure native-library loading
(registering a `DllImportResolver` / `NativeLibrary.SetDllImportResolver`, or adjusting the search path).
Moving the gate into `SkiaApi`'s static constructor defers it to the **first P/Invoke** — as late as
possible — while still always preceding any native call. "Set up the native binary, *then* use Skia
later" works again.

**Re-entrancy is safe.** The check reaches native through `SkiaApi.sk_version_get_milestone` /
`sk_version_get_increment` while the `SkiaApi` constructor is still running. The CLR's same-thread
type-initializer rule returns the in-progress `SkiaApi` type without re-running the body, and the only
static state the version path reads (the `USE_DELEGATES` `Lazy<IntPtr>` library handle and its delegate
cache) is a *field initializer*, which the CLR runs **before** the constructor body. The version path
touches only `SkiaApi` and `LibraryLoader`, never an `SKObject`, so it introduces no cross-type cctor
cycle. **Guard for future edits:** do not add a `SkiaApi` static field that the version path depends on
and that is assigned *in the constructor body after* the check — that would be read as null during the
re-entrant version call.

**The per-call cost is negligible.** Losing `beforefieldinit` on this hot type lets the JIT require a
type-init readiness check before accessing any `SkiaApi` static member. We benchmarked it (see
[Part 2](#the-version-check-placement-rejected-options-and-the-benchmark)): the check is masked by the
P/Invoke transition itself — ~0.35% on CoreCLR, statistically zero (sign-unstable) on Mono/net48.

## Eager, one-shot, lazy-on-first-touch

- **Eager within a type.** The first access to any member of, say, `SKColorSpace` triggers its static
  constructor, which builds *both* sRGB wrappers up front. Touching `SkiaSharpStatics` (transitively,
  the first time any singleton type initializes) acquires **all** raw handles at once, matching the
  3.119.x behaviour where touching any `SKObject` created every singleton.
- **Lazy across the assembly.** Nothing forces initialization at assembly load. The handles and wrappers
  come into existence the first time a singleton type is touched — not before. The version check is
  likewise lazy: it fires on the first P/Invoke, not at load.
- **One-shot — there is no retry.** A type initializer that throws is cached by the CLR: the type is
  poisoned and every later access rethrows `TypeInitializationException`. This is deliberate. The only
  realistic failure here is an incompatible / broken native library, in which case the library is
  unusable and retrying cannot help. `SkiaSharpStatics` throws a clear `InvalidOperationException` (via
  `ThrowIfZero`) if any native call returns a null handle, rather than caching a null that would surface
  later as an opaque `NullReferenceException`.

## Reference counting

Each `sk_*_new`/`sk_*_create` call in `SkiaSharpStatics..cctor` returns a native object with a `+1`
reference. That reference is held by the `IntPtr` field and then **adopted** (`owns: true`) by the
dispose-protected wrapper built in the owning type's static constructor. The wrapper's public `Dispose()`
is short-circuited and the static-field root keeps it alive (so its finalizer never runs), so the
singleton lives for the process lifetime. The font-manager and `Normal`-style handles passed to
`sk_fontmgr_legacy_create_typeface` are **borrowed** (not consumed) by that call, so they stay owned by
their own wrappers — no double counting.

When the platform has no default typeface, `DefaultTypeface` aliases `EmptyTypeface` (same handle).
`SKTypeface..cctor` detects that aliasing and adopts the single already-registered empty wrapper rather
than registering the same handle twice:

```csharp
defaultTypeface = SkiaSharpStatics.DefaultTypeface == SkiaSharpStatics.EmptyTypeface
    ? empty
    : GetDisposeProtectedObject(SkiaSharpStatics.DefaultTypeface);
```

## What makes a singleton persistent (the `HandleDictionary` mechanism)

Persistence is implemented by **dispose-protection plus a static-field root**, in
`SKObject`/`HandleDictionary` and the accessor types — not by any per-wrapper "immortal" latch. The
relevant pieces:

- `HandleDictionary` stores wrappers as **weak** references (`Dictionary<IntPtr, WeakReference>`) keyed by
  native handle, guarded by a **platform-abstracted lock** (`IPlatformLock`, see `PlatformLock.cs`): on
  non-Windows a `ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion)`, on Windows a Win32
  `CRITICAL_SECTION` (chosen to fix the #1383 STA message-pump alertable-deadlock). `GetOrAddObject`
  deduplicates: if a live wrapper already fronts a handle it is returned; flags on the call
  (`unrefExisting`, `disposeProtected`) control how a duplicate handle is reconciled.
- **Dispose-protected** (`PreventPublicDisposal` / `IgnorePublicDispose`) makes the **public** `Dispose()`
  a no-op. Set under `GetOrAddObject`'s upgradeable-read lock, it is read by public `Dispose()` under the
  mutually-exclusive write lock, so the flag set cannot race a concurrent public disposal.
- **The static-field root** in each accessor type (`private static readonly SKColorSpace srgb;`, …) keeps
  the singleton wrapper reachable for the whole process. Because it is never unreachable, its **finalizer
  never runs**. And because the handle is deduped (`unrefExisting: true`), no second *disposable* wrapper
  is ever minted over the same handle — so the only `DisposeInternal`/finalizer-capable wrapper for the
  shared handle is the rooted, dispose-protected one, which never becomes garbage. See
  [Why no immortal latch](#why-no-immortal-latch-and-why-it-was-redundant).

This is the mechanism that satisfies requirement 1: a shared handle arriving via an unrelated getter
dedups in `GetOrAddObject` to the already-registered dispose-protected wrapper, so a caller who disposes
"their" color space cannot free the global one.

## Why no immortal latch (and why it was redundant)

An earlier iteration (#4119) added a one-way **immortal latch** (`MakeImmortalSingleton` /
`IsImmortalSingleton`) checked *before the unref* on all three teardown paths — public `Dispose`, the
finalizer, and the transitive `DisposeInternal` — so none of them could ever free a singleton's shared
native object. This PR removes it. Under the **eager** design above, every teardown path the latch
defended is already structurally unreachable for a singleton:

1. **Public `Dispose()`** — already a no-op via `IgnorePublicDispose` (dispose-protection), which every
   singleton sets. The latch added nothing here.
2. **The finalizer** — never runs. Each singleton wrapper is rooted by a `static readonly` field for the
   whole process, so it is never eligible for finalization.
3. **`DisposeInternal()`** — only reached two ways, neither of which applies to a singleton:
   (a) *owned-child teardown / ownership handoff* (`RevokeOwnership`, `TransferOwnershipToNative`): no
   production path hands a singleton wrapper to any of these; and (b) *dictionary replacement* in
   `RegisterHandle`, which disposes the *previous* wrapper when a **new** wrapper registers the same
   handle. Because singleton handles are obtained through the deduping dispose-protected accessors with
   `unrefExisting: true`, a second wrapper for a singleton handle is never constructed, so the replacement
   branch never fires for a singleton.

In short: **eager registration closes the lazy window, static rooting removes the finalizer, and dedup
removes the disposable sibling.** What remained for the latch to guard was the empty set. (This is exactly
why #4080's *lazy* design needed more care: before a singleton's accessor had run, a getter route could
mint a transient `owns: true` sibling over the shared handle whose teardown *would* unref the global. The
eager static constructors register the dispose-protected wrapper first, eliminating that window.)

The net48/x86 `AccessViolationException` that the immortal-era history (Part 2) attributes to singleton
over-unref was, in #4119, separately root-caused and fixed as a **callback-exception-isolation** bug (a
managed-stream callback throwing *through* native Skia frames corrupted the heap on net48/x86). With that
real fix in place, and the three structural arguments above, the latch guards nothing reachable. The
net48/x86 CI leg is the oracle: removing the latch must leave it green.

## Tests

- `tests/Tests/SkiaSharp/SKSingletonInitTest.cs` — singleton identity, dispose-protection,
  `CreateDefault` non-null, and `SKPaint` smoke coverage. The class is in a
  `DisableParallelization` collection (`HandleDictionaryThreadingCollection`) because one test asserts
  exact native refcounts on a process-global singleton.
- `tests/SkiaSharp.Tests.SingletonInit.Console/` — a dedicated **cold-start** project that touches
  `SKFontManager.Default` first in a fresh process, guarding the exact #3817 regression against any
  future re-introduction of a cross-type cctor cycle. This must stay in its own assembly: `dotnet test`
  launches one host per assembly, which is what reproduces "first SkiaSharp type touched in the process."
- `tests/Tests/SkiaSharp/SKVersionTest.cs` and the version cases in `ApiTests.cs` — call
  `CheckNativeLibraryCompatible` directly; they do not depend on *where* the check is wired, so the
  `SkiaApi`-cctor placement leaves them valid.
- The `HandleDictionary`/`SKManagedStream` concurrency and reservation suites
  (`SKHandleDictionary*Test.cs`, `SKManagedStreamConcurrencyTest.cs`, `SKSingletonConcurrencyTest.cs`) —
  see the testing-learnings section in Part 2 for what each class of test is guarding and the determinism
  rules they must follow.

---

# Part 2 — How we got here (history, rejected designs, and learnings)

This part is the archaeological record. None of it is required to *use* the implementation in Part 1, but
all of it is required to *change* it safely. It was assembled over a long multi-PR investigation (#4080,
#4116, #4117, #4119) and many cold-start / net48 CI failures.

## The #3817 root cause, decoded

#3817 was a CLR **type-initializer re-entrancy** crash. In the pre-rework design, eager initialization
lived in `static SKObject()` (SKObject.cs:41 on `main`), which called `CheckNativeLibraryCompatible(true)`
and then an `EnsureStaticInstanceAreInitialized()` cascade for all six singleton families. The fatal
cycle:

```
SKFontManager.Default
  → SKFontManager..cctor
    → SKObject..ctor (base)  → triggers SKObject..cctor mid-way through SKFontManager.cctor
      → SKTypeface.EnsureStaticInstanceAreInitialized → SKTypeface..cctor
        → reads SKFontManager.Default  (SKFontManager.cctor is ALREADY running on this thread)
          → CLR returns the half-built type with a null Default field → NullReferenceException
```

surfaced as a cached `TypeInitializationException`. The structural fault: `SKObject`'s base constructor
is on **every** wrapper construction path, so `SKObject..cctor` fires re-entrantly inside other types'
cctors, and the one cross-type dependency (typeface → font-manager) closes the loop. **A naive per-type
static cctor does not fix this** — touching `SKTypeface` still reaches into `SKFontManager` mid-init. The
only robust fix is to remove the cross-type read from the type-initializer graph entirely, which is
exactly what `SkiaSharpStatics` (raw handles, no wrapper reads) does.

## PR lineage — four attempts at the same bug

| PR | Author | Approach | Why it wasn't the end state |
|----|--------|----------|------------------------------|
| 3.119.x | — | Eager, self-contained static init; default typeface resolved without reading another *managed* singleton across a live cctor. Disposing a shared color space was a safe no-op (wrappers were dispose-protected). | The behaviour we were trying to *restore*. Lost during the rework. |
| `main` | — | Default-typeface resolution changed to read the managed `SKFontManager.Default`/`SKFontStyle.Normal` **wrappers** from inside the type-initializer graph. | Introduced the #3817 re-entrancy crash. |
| **#4080** | ramezgerges | Full rework: fully **lazy** init + heal/promote-to-immortal. Eliminates the cctor graph (no re-entrancy by construction). Introduced `GetOrAddImmortalSingletonObject` and the immortal/dispose-protected machinery. | Lazy window opened a getter-route disposal hazard and a net48 teardown AccessViolation (see below). Immortality only partially covered until "healed." |
| **#4116** | mattleibow | Minimal alternative: keep eager `SK*Static`, surgically fix the one re-entrant cctor (typeface) via #4107's raw-handle try/finally, add `PreventPublicDisposal`. | Smaller and green, but keeps the fragile cctor graph (patches the symptom site, not the structure) and the "ugly" raw-handle code. |
| **#4117** | mattleibow | Immortal latch on top of #4080's lazy orchestrator. | Helped, but a residual teardown crash remained; still built on the lazy orchestrator + a `GetOrAddObject` hook. |
| **#4119** | mattleibow | Replace the lazy orchestrator and its `GetOrAddObject` hook with the Part 1 design: a dependency-free `SkiaSharpStatics..cctor` plus one explicit static cctor per wrapper type. Kept #4080's immortal mechanism; dropped the locks, the retry, and the orchestrator hook. | Superseded: the immortal latch was later shown redundant under this eager design. |
| **this PR** (current) | mattleibow | The #4119 design with the **immortal latch removed**: singletons are dispose-protected wrappers rooted by static fields. | The current implementation. See [Why no immortal latch](#why-no-immortal-latch-and-why-it-was-redundant). |

The throughline: keep the **dedup-on-the-shared-handle** mechanism from #4080 (it is what makes
getter-route dedup dispose-safe), but move *initialization* out of the CLR type-initializer cycle and out
of any custom lock-based orchestrator, into a small acyclic set of static constructors. The immortal latch
that #4080–#4119 layered on top turned out to be redundant once init was eager (the rooted,
dispose-protected, deduped wrapper is the only teardown-capable wrapper for the handle, and it never
becomes garbage).

## The net48 AccessViolationException saga

A large fraction of this investigation was a Windows **.NET Framework** `AccessViolationException` that
only appeared in CI and was repeatedly masked by the test-retry harness. Key findings, in case it recurs:

- It was a **teardown / finalizer** crash, not a render crash — native heap corruption surfaced late,
  during process teardown or a later unrelated test, which made stack traces misleading.
- Root cause was an **over-unref of a process-global singleton**: under the lazy design a singleton
  wrapper could be created `owns: true` and then have its single shared native reference released (via
  `Dispose`/finalizer) while other aliases were still live — corrupting the shared `SkData`/colorspace/
  typeface. The immortal latch (checked *before* any unref on all three teardown paths) closed this **for
  the lazy design**. *Update:* under the current **eager** design this over-unref is structurally
  unreachable (eager registration closes the lazy window, static rooting removes the finalizer, dedup
  removes the disposable sibling), and the net48/x86 AVE was separately root-caused to a
  callback-exception-isolation bug and fixed there — so the latch was removed as redundant. See
  [Why no immortal latch](#why-no-immortal-latch-and-why-it-was-redundant).
- **Windows vs non-Windows lock semantics matter.** `HandleDictionary`'s lock is platform-abstracted
  (`IPlatformLock`). On non-Windows it is a `ReaderWriterLockSlim` with `LockRecursionPolicy.NoRecursion`,
  so a re-entrant/nested `GetOrAddObject` from inside a factory **throws `LockRecursionException`**. On
  Windows it is a Win32 `CRITICAL_SECTION` (introduced to fix the #1383 STA message-pump alertable
  deadlock), which is **recursive** — a nested acquire on the same thread silently succeeds instead of
  throwing. That asymmetry — throw on macOS/Linux, silent re-entry on Windows — is the mechanism behind
  the platform-dependent symptoms. Documented as an open caveat: re-entrant construction from a factory
  is unsupported.
- A separate, smaller net48 flake was an **exact-refcount assertion** on the global sRGB color space
  (`SKColorSpaceTest.ColorSpaceIsNotDisposedPrematurely`): parallel tests creating/disposing sRGB perturb
  the process-global refcount. Fixed by keeping the colorspace alive across the measurement
  (`GC.KeepAlive` + serialized collection) rather than loosening the semantics.

**CI access note.** The `hlx-azdo` MCP tooling does **not** cover the `xamarin` AzDO org. Query builds
anonymously: `curl https://dev.azure.com/xamarin/public/_apis/build/builds/{id}?api-version=7.0` (and
`/timeline`). Pipeline definition id `4` = "SkiaSharp (Public)". Several "red" runs during this work were
unrelated iOS device-agent timeouts (180-min hard cap) and keychain/provisioning infra flakes, not test
assertion failures — always drill into the timeline before blaming the change.

## Singleton-init designs we rejected

- **Keep eager init in `static SKObject()`** (the `main`/3.119.x shape). Rejected: it *is* the #3817
  cycle — `SKObject`'s base ctor is on every construction path, so its cctor re-enters other types' cctors.
- **A per-type static cctor that reads the dependency wrapper** (e.g. `SKTypeface..cctor` reads
  `SKFontManager.Default`). Rejected: same re-entrancy; the cross-type *wrapper* read is the problem, not
  where it physically sits.
- **A custom lock-based `SkiaSharpStatics.EnsureInitialized()` orchestrator** (tri-state monitor with
  retry + a hook at the top of `GetOrAddObject`), used by an interim version of #4080/#4117. Rejected in
  favour of plain static constructors: the CLR already gives us run-once, thread-safe,
  lazy-on-first-touch semantics for free, with `readonly` fields and no retry logic to get wrong. The one
  thing to respect is that the initializer must wire inter-singleton dependencies via **already-assigned
  internal handles**, never via public accessors (which would recurse). `SkiaSharpStatics` does exactly
  that at the raw-handle level.
- **Folding the singleton init into the module initializer.** Rejected: it would force ~40 native
  allocations at assembly load on consumers that never touch a singleton, and a `[ModuleInitializer]`
  compiles to the module type-initializer — reintroducing the very type-init machinery #3817 was about.

## The version-check placement: rejected options and the benchmark

The native-library compatibility check (`CheckNativeLibraryCompatible`, itself a P/Invoke into
`sk_version_get_milestone`/`_increment`) must run **before the first real P/Invoke** so a wrong-milestone-
but-loadable library is converted into a clean `InvalidOperationException` instead of silent memory
corruption or an opaque `EntryPointNotFoundException`. The constraint the user set: it must **not** fire
at assembly load (consumers need to configure native loading first), must **not** require wrapping all
~2662 externs, and must run "before bytes cross the wall."

Options considered and why they lost to the `SkiaApi` static constructor:

- **`[ModuleInitializer]` (the state in #4119 before this change).** Runs at assembly load → eager
  P/Invoke before a consumer can register a `DllImportResolver` or adjust the search path. Rejected for
  exactly that reason.
- **First line of `SkiaSharpStatics..cctor`.** Only runs when a *singleton* is first touched. The
  overwhelmingly common workflows (only `SKBitmap`/`SKCanvas`/`SKImage`) never touch a singleton, so the
  check would frequently never run. Incomplete coverage → rejected.
- **A base-`SKObject` / `SKNativeObject` instance-ctor gate.** Too late for static factories: a factory
  like `GetObject(SkiaApi.sk_image_new_*(...))` evaluates the **P/Invoke argument before** `GetObject`
  runs, so one native call escapes the gate. (Verified empirically with a throwaway net8 program against
  ECMA-335 §I.8.9.5 `beforefieldinit` semantics: a base type's cctor is **not** triggered by a derived
  type's static method, and the factory pinvoke runs before any base ctor.)
- **A "three-hook" gate** (tiny `SkiaSharpInit` type; hook from the base instance ctor + the
  `SkiaSharpStatics` cctor + an explicit cctor on each of ~25–30 factory types). Works, but has two
  residual coverage holes: **Gap 1** = non-`SKObject` static utility classes (`SKGraphics` ~16 pinvokes,
  `SKSwizzle`, `SKWebpEncoder`) the base ctor never covers; **Gap 2** = pinvoking **structs**
  (`SKColorSpacePrimaries` ~22, `SKMatrix` ~14, `SKColorSpaceTransferFn`, `SKMatrix44`) where
  `default(T).InstanceMethod()` has no construction hook. Plus ~30 hand-or-codegen-maintained cctors.
  More churn, more surface area, still not 100% — rejected.
- **A `DllImportResolver` registered at module load** (one AI-brainstormed "library-load boundary"
  idea). Rejected: `NativeLibrary.SetDllImportResolver` is a **single-slot, per-assembly** callback with
  no getter/unregister/chaining; a second call throws `InvalidOperationException`. SkiaSharp consumers
  already use that slot to redirect native loading, so claiming it would be a breaking regression. (It is
  also modern-runtime only — absent on net48.)
- **Codegen-injected stubs** (have `generate.ps1` prepend `EnsureCompatible()` to every generated `sk_*`
  wrapper). The other AI-brainstormed option. Full coverage, but adds a check to *every* call site,
  bloats generated code, and is the "wrapping every extern" the user explicitly ruled out.

**Why `static SkiaApi()` wins.** `SkiaApi` is the single type through which **every** P/Invoke flows, so
one explicit cctor there is the unique choke point that covers all interop configs (USE_DELEGATES on net4x
incl. net48; USE_LIBRARY_IMPORT on net7+; direct `DllImport` on netstandard2.0/2.1), all platforms (net48 /
modern / wasm), structs, static utility classes, and factories alike — with **zero per-type churn**, no
module init, no resolver slot, and no codegen. The only substantive objection was performance (losing
`beforefieldinit` on the hottest type), so we measured it.

**The benchmark** (full record in the session artifact `cctor-bench-results.md`). Two structurally
identical static classes differing *only* in the presence of an explicit static ctor, 8 distinct
`NoInlining` call sites each (so the JIT can't hoist the type-init readiness check out of the loop),
P/Invoke target = libc `abs(int)` (the cheapest possible pinvoke — most favorable to the concern):

| Runtime | Call kind | `beforefieldinit` | explicit cctor | delta |
|---------|-----------|-------------------|----------------|-------|
| CoreCLR (.NET 10, M3 Pro) | managed-only (worst case) | 3.250 ns | 3.300 ns | +0.05 ns (+1.5%) |
| CoreCLR | **P/Invoke (realistic)** | 6.342 ns | 6.364 ns | **+0.022 ns (+0.35%)** |
| Mono 6.12 / net48 (paired A/B median) | managed-only (worst case) | 9.12 ns | 10.36 ns | +1.24 ns (+13.7%) |
| Mono / net48 | **P/Invoke (realistic)** | 74.74 ns | 73.72 ns | **−1.02 ns (−1.3%, sign-unstable)** |

CoreCLR backpatches the init check away after first run. Mono does *not* (a pure managed static call
really does pay ~1.24 ns), but every `SkiaApi` call is a ~74 ns P/Invoke, so the check disappears into the
transition noise (the pinvoke delta even flips sign between methodologies). With the cheapest possible
pinvoke; real Skia marshalling makes the relative cost smaller still. **Conclusion: the perf objection
does not survive measurement.**

Methodology lessons worth keeping for the next time we benchmark interop:
- Need **8+ distinct `NoInlining` call sites** or the JIT hoists the readiness check out of the loop and
  you measure nothing.
- Mono timing is noisy (±2–3 ns); use **paired interleaved A/B sampling** (median of many pairs), not
  two separate runs — a non-paired run once showed the cctor *faster*, which is physically impossible.
- BenchmarkDotNet's child-process toolchain fails to launch on a .NET 10 preview host; use
  `InProcessEmitToolchain`.

## Testing learnings (the concurrency/lifecycle suite)

This PR family added a large body of `HandleDictionary`/`SKObject`/managed-stream concurrency tests.
Hard-won rules that keep them deterministic and meaningful:

- **Never use `Parallel.For`/`Parallel.Invoke` + `Barrier(N)` for race tests.** Thread-pool scheduling
  makes them non-deterministic (and they deadlock if a participant never gets a pool thread). Use one
  dedicated `Thread` per participant with an explicit barrier and a timeout
  (`RunConcurrent`/`RunWithTimeout`).
- **Absence assertions are racy under xUnit parallel collections.** `Assert.False(GetInstance(handle, out
  _))` can fail spuriously because a freed native pointer may be **reused** by another test running in
  parallel, repopulating the dictionary entry. Assert on **reference identity**
  (`AssertDeregistered<T>(handle, instance)`), not on the address being absent. There are ~20+ latent
  pre-existing absence-asserts in the author's tests that share this pattern; they were not swept
  unilaterally (risk + diff size) but are documented for follow-up.
- **The cold-start guard must stay in its own assembly.** `dotnet test` runs one host process per test
  assembly; that is the only way to guarantee "this is the first SkiaSharp type touched in the process,"
  which is what reproduces #3817. Do not add other SkiaSharp-touching tests to
  `SkiaSharp.Tests.SingletonInit.Console`.
- **Exact-refcount assertions on process-global singletons must be serialized.** Put them in a
  `DisableParallelization` collection and keep the object alive across the measurement; otherwise
  parallel sRGB create/dispose perturbs the count (the net48 colorspace flake).
- **Guard concurrency tests on single-threaded WASM** — they can't spawn real threads there.

### Open findings reported to the maintainer (not fixed here)

- Re-entrant / nested `GetOrAddObject` from inside a factory throws `LockRecursionException` on
  macOS/Linux (`NoRecursion` policy) but not necessarily on Windows — platform-dependent, likely
  pre-existing; the re-entrancy contract should be documented.
- Concurrent `Dispose()` of an `SKManagedStream` *during* `SKCodec.Create` (before the stream is
  reparented) is an unguarded use-after-free — classified as unsupported misuse, not fixed.
- The ~20+ latent address-reuse absence-asserts noted above.

## Why each concern is kept apart (summary)

| Concern | Where it lives | Why not elsewhere |
|---------|----------------|-------------------|
| Native version/ABI check | `static SkiaApi()` (first P/Invoke) | Module init = eager at load; `SkiaSharpStatics` = singleton-only coverage; base ctor = too late for factories. |
| Singleton handle acquisition | `SkiaSharpStatics..cctor` (raw handles) | In a wrapper cctor it would re-enter another wrapper cctor (#3817). |
| Singleton wrapper adoption + dispose-protection | each wrapper type's explicit `static T()` | Field initializers are `beforefieldinit` → a static method could run before registration, breaking getter-route dedup. |
| Dispose-safety of shared handles | `HandleDictionary` dedup + dispose-protection + static-field root | Must apply to *all* arrival routes (accessor and unrelated getter), so it belongs at the dedup layer, not the accessor. |

The recurring principle: **keep initialization out of the CLR type-initializer cycle where it can
re-enter, push the version gate to the single P/Invoke choke point, and let dedup + dispose-protection +
static rooting — not the accessor — enforce dispose-safety.**
