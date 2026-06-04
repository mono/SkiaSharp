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

Because these native objects are shared, the managed wrappers that front them must be **immortal**:
disposing one (directly, via a finalizer, or via a transitive `DisposeInternal`) would free or unref a
native object that other live aliases still point at, corrupting every other consumer of that handle.
This document explains how those wrappers are created and kept alive.

## Two requirements that pull in opposite directions

1. **Dispose-safety (getter-route dedup).** A handle the singleton shares can also arrive through an
   unrelated getter — e.g. `sk_image_get_colorspace` can return the *same* native pointer as the global
   sRGB color space. When that handle is looked up it must dedup to the **dispose-proof immortal
   wrapper**, not to a fresh mortal wrapper that a caller could legitimately dispose. This was the
   3.119.x behaviour and it must be preserved: see [memory-management.md](memory-management.md).

2. **No re-entrant static-init crash (#3817).** The singletons have cross-type dependencies (the default
   typeface needs the font manager and the `Normal` font style). Historically the eager initialization
   lived in `SKObject`'s static constructor, which chained into every singleton type. That put the whole
   graph inside the CLR type-initializer machinery and produced a re-entrant `NullReferenceException`.

Requirement 1 wants eager, dedup-aware registration of every singleton *before* any handle lookup.
Requirement 2 forbids doing that from a type/module initializer. The solution is to keep initialization
**eager-on-first-touch** but move it **out of the CLR type-initializer graph**.

## Why not a `.cctor` or `[ModuleInitializer]`? (the #3817 root cause)

A `[ModuleInitializer]` compiles to the module's type initializer, so it has exactly the same hazards as
a `.cctor`. With initialization in a type initializer, the dependency cycle deadlocks against two CLR
rules:

```
SKFontManager.Default
  → SKFontManager..cctor                 (constructs the default-manager wrapper)
    → SKObject..ctor (base)              → triggers SKObject..cctor for the FIRST time, mid-cctor
      → SKTypeface..cctor                → reads SKFontManager.Default
        → SKFontManager..cctor is ALREADY running on this thread
          → the CLR returns the half-built type with a null field → NullReferenceException
```

1. **Same-thread re-entrancy returns the partially-initialized type** instead of blocking, so a
   cross-type dependency observes null fields.
2. **A throw is cached for the lifetime of the process.** The type/module is permanently poisoned and
   every later access rethrows the same `TypeInitializationException`, with no way to retry.

## The design: `SkiaSharpStatics.EnsureInitialized()`

All initialization is centralized in `binding/SkiaSharp/SkiaSharpStatics.cs`, a plain
self-synchronized orchestrator — **not** a type or module initializer.

- **Tri-state + reentrant monitor.** A `state` field moves `Uninitialized → Initializing → Initialized`
  under `lock (gate)`. The fast path is a single `Volatile.Read`; once initialized there is no locking.
- **Explicit re-entrancy handling.** A re-entrant call during initialization (a registered singleton's
  construction routes back through the `HandleDictionary` hook) re-enters the monitor on the same thread,
  sees `state != Uninitialized`, and bails — returning the already-assigned backing field of an
  **earlier-initialized** dependency. This replaces the CLR's "return the half-built type" rule with a
  deterministic, correct answer.
- **Recoverable failure.** If any `InitializeStatics` throws, the orchestrator resets `state` to
  `Uninitialized` and rethrows the *original* exception at the real call site (with a meaningful stack).
  The assembly is never poisoned; a later touch retries the whole chain.

### Where it is called from

```
EnsureInitialized()  ← every public singleton accessor (CreateSrgb, Default, Empty, …)
EnsureInitialized()  ← the top of HandleDictionary.GetOrAddObject, BEFORE its lock is taken
```

The accessor hook lets each accessor simply `return` its eagerly-populated field. The
`GetOrAddObject` hook is what satisfies **requirement 1**: the very first time *any* native handle is
looked up, the singletons are already registered as immortal wrappers, so a shared handle dedups to the
dispose-proof wrapper.

### Initialization order (dependencies last)

```
SKColorSpace → SKColorFilter → SKData → SKFontStyle → SKBlender → SKFontManager → SKTypeface
```

A singleton that references another is initialized **after** it, so the dependant reads an
already-assigned backing field. `SKFontStyle` precedes `SKBlender`/`SKFontManager` because the default
typeface (initialized last) reads `SKFontStyle.Normal`; `SKFontManager` precedes `SKTypeface` because
the default typeface is matched through the default manager.

## Lock ordering (deadlock-freedom)

There are two process-wide locks: the `SkiaSharpStatics` `gate` and the single
`HandleDictionary.instancesLock`. The ordering is **always** `gate → instancesLock` and can never
invert:

- `EnsureInitialized()` runs at the **top** of `GetOrAddObject`, *before* `instancesLock` is taken.
- The `SKObject` constructors that register singletons run **inside** `GetOrAddObject`'s factory while
  `instancesLock` is held — i.e. strictly after the gate was already passed on this thread.

No path takes `instancesLock` and then the gate, so the two locks cannot deadlock. **Do not move the
hook into `SKObject..ctor`** — that would invert the order to `instancesLock → gate`.

## Idempotency (retry after partial failure)

Because a failed init resets to `Uninitialized` and a retry re-runs the **whole** chain, every
`InitializeStatics` must be idempotent — it must reuse the wrappers it already created and never
construct a second immortal wrapper. The patterns:

- **Registered types** (`SKColorSpace`, `SKData`, `SKColorFilter`, `SKFontManager`, `SKTypeface`) use
  `field ??= GetDisposeProtectedObject(...)`. Each roots its wrapper in its own static field, and the
  helper routes to `GetOrAddImmortalSingletonObject(..., unrefExisting: true)`, which dedups against the
  `HandleDictionary`, unrefs the redundant native ref, and re-promotes to immortal.
- **`SKFontStyle`** (`ISKSkipObjectRegistration`, so it **bypasses** the `HandleDictionary` dedup) uses
  `??=` per field. Without the guard a retry would construct four new immortal native font styles whose
  finalizers never run — a leak. The guard is the fix, not an optimization.
- **`SKBlender`** roots all blenders in a single static dictionary, so it cannot rebuild the dictionary
  on retry (that would drop the only strong references to the already-created immortal wrappers). It
  roots the dictionary up-front, fills only still-missing modes (`if (!ContainsKey(mode))`), and uses a
  **separate** `blendModeBlendersInitialized` latch as the "done" signal — the dictionary's non-nullness
  cannot be the signal because that would early-out on a half-filled dictionary.

## Behavioural note: all-or-nothing failure isolation

The old per-type `LazyInitializer` initialized each singleton independently, so a failure in one did not
affect the others. Under the centralized orchestrator, the **first touch initializes the entire graph**,
so a failure anywhere makes every accessor and every `GetOrAddObject` retry the full chain (e.g.
`CreateSrgb()` can throw because typeface init is broken). This is acceptable because the only realistic
failure mode is native-library incompatibility — in which case the whole library is unusable anyway — and
it buys the dependency-ordered, re-entrancy-safe, retriable behaviour described above.

## Tests

- `tests/Tests/SkiaSharp/SKSingletonInitTest.cs` — identity, immortality, `DisposeInternal` no-op,
  cross-type ordering, and the retry-idempotency tests
  (`ReinitializingStaticsReusesSameSingletonInstances`,
  `ReinitializingBlendersAfterPartialResetFillsOnlyMissingModes`). The class is in a
  `DisableParallelization` collection so the reflection-based `state` resets are safe.
- `tests/SkiaSharp.Tests.SingletonInit.Console/` — a dedicated cold-start project that touches
  `SKFontManager.Default` first in a fresh process to guard the exact #3817 regression.
