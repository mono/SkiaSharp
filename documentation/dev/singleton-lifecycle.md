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
This document describes how those wrappers are created and kept alive in the **current** implementation,
and then traces how the design evolved to get here.

## Two requirements that pull in opposite directions

1. **Dispose-safety (getter-route dedup).** A handle the singleton shares can also arrive through an
   unrelated getter — e.g. `sk_image_get_colorspace` can return the *same* native pointer as the global
   sRGB color space. When that handle is looked up it must dedup to the **dispose-proof immortal
   wrapper**, not to a fresh mortal wrapper that a caller could legitimately dispose. This was the
   3.119.x behaviour and it must be preserved: see [memory-management.md](memory-management.md).

2. **No re-entrant static-init crash (#3817).** The singletons have one cross-type dependency: the
   default typeface is resolved through the default font manager and the `Normal` font style. If that
   dependency is expressed as one wrapper type's static constructor reading **another wrapper type**, the
   whole graph sits inside the CLR type-initializer machinery and can deadlock against its re-entrancy
   rules (see the history section below).

The current design satisfies both: every singleton is built **eagerly and exactly once** by its own
type's static constructor, but the one cross-type dependency is resolved at the **raw-handle level** so
that no wrapper static constructor ever reads another wrapper type.

## The design: per-type static constructors over a shared handle holder

Two pieces work together:

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
into immortal `readonly` fields, and its accessors just return those fields:

```csharp
public unsafe class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted
{
    private static readonly SKColorSpace srgb;
    private static readonly SKColorSpace srgbLinear;

    static SKColorSpace()
    {
        srgb = GetDisposeProtectedObject(SkiaSharpStatics.Srgb);
        srgbLinear = GetDisposeProtectedObject(SkiaSharpStatics.SrgbLinear);
    }

    public static SKColorSpace CreateSrgb() => srgb;
    public static SKColorSpace CreateSrgbLinear() => srgbLinear;
}
```

`GetDisposeProtectedObject(IntPtr)` routes to `GetOrAddImmortalSingletonObject(...)`, which registers the
wrapper in the `HandleDictionary` as immortal. `SKFontStyle` is special — it is
`ISKSkipObjectRegistration`, so it bypasses the dictionary and latches each handle immortal directly.

#### Why static constructors (and why they must be *explicit*)

- **Lock-free and run-once.** The CLR guarantees a type initializer runs exactly once, on first use of
  the type, in a thread-safe way — with no locks of our own and no orchestrator. The backing fields can
  be `readonly`.
- **The fields can satisfy getter-route dedup.** Requirement 1 needs the immortal wrapper to be
  registered **before** any handle lookup on that type dedups a shared handle. The CLR gives us this
  *only if the type is not `beforefieldinit`*. A type whose statics are written as bare field
  initializers (`static readonly SKColorSpace srgb = ...;`) **is** `beforefieldinit`, which lets the CLR
  run a static **method** body (such as `GetObject`) *before* the field initializers. An **explicit**
  `static SKColorSpace() { ... }` removes `beforefieldinit`, so the cctor is guaranteed to complete
  before the body of any static method on the type runs. **Every singleton wrapper type therefore
  declares an explicit static constructor — do not collapse them into field initializers.**

## Eager, one-shot, lazy-on-first-touch

- **Eager within a type.** The first access to any member of, say, `SKColorSpace` triggers its static
  constructor, which builds *both* sRGB wrappers up front. Touching `SkiaSharpStatics` (transitively,
  the first time any singleton type initializes) acquires **all** raw handles at once, matching the
  3.119.x behaviour where touching any `SKObject` created every singleton.
- **Lazy across the assembly.** Nothing forces initialization at assembly load. The handles and wrappers
  come into existence the first time a singleton type is touched — not before.
- **One-shot — there is no retry.** A type initializer that throws is cached by the CLR: the type is
  poisoned and every later access rethrows `TypeInitializationException`. This is deliberate. The only
  realistic failure here is an incompatible / broken native library, in which case the library is
  unusable and retrying cannot help. `SkiaSharpStatics` throws a clear `InvalidOperationException` (via
  `ThrowIfZero`) if any native call returns a null handle, rather than caching a null that would surface
  later as an opaque `NullReferenceException`.

## Reference counting

Each `sk_*_new`/`sk_*_create` call in `SkiaSharpStatics..cctor` returns a native object with a `+1`
reference. That reference is held by the `IntPtr` field and then **adopted** (`owns: true`) by the
immortal wrapper built in the owning type's static constructor; the immortal wrapper never releases it,
so the singleton lives for the process lifetime. The font-manager and `Normal`-style handles passed to
`sk_fontmgr_legacy_create_typeface` are **borrowed** (not consumed) by that call, so they stay owned by
their own wrappers — no double counting.

When the platform has no default typeface, `DefaultTypeface` aliases `EmptyTypeface` (same handle).
`SKTypeface..cctor` detects that aliasing and adopts the single already-registered empty wrapper rather
than registering the same handle twice:

```csharp
defaultTypeface = SkiaSharpStatics.DefaultTypeface == SkiaSharpStatics.EmptyTypeface
    ? empty
    : GetImmortalSingletonObject(SkiaSharpStatics.DefaultTypeface);
```

## Relationship to the module initializer

`binding/SkiaSharp/SkiaSharpModuleInitializer.cs` is **separate** and stays that way. It runs the native
**library-compatibility check** (`SkiaSharpVersion.CheckNativeLibraryCompatible`) from a
`[ModuleInitializer]`, i.e. once at **assembly load**, before any SkiaSharp API call. That guard must run
for *every* consumer — including code that only uses non-singleton types like `SKBitmap` or `SKCanvas` —
so it cannot live in the singleton path, which only initializes when a singleton is first touched.
Conversely, the singleton handles must **not** be forced at module load (that would impose ~40 native
allocations on consumers that never use a singleton). The two concerns have different lifecycles and are
intentionally kept apart.

## How this evolved (and why each step happened)

- **3.119.x — eager, self-contained static initialization.** Each singleton was created eagerly and the
  default typeface was resolved without reading another *managed* singleton across a live cctor, so the
  graph stayed effectively acyclic. Disposing a shared color space was a safe no-op because the wrappers
  were dispose-protected. This is the behaviour the current design re-establishes.

- **`main` — the #3817 regression.** Default-typeface resolution changed to read the managed
  `SKFontManager.Default` and `SKFontStyle.Normal` **wrappers** from inside the type-initializer graph,
  while eager init still chained through `SKObject`'s static constructor. Touching `SKFontManager.Default`
  first re-entered the still-running `SKFontManager`/`SKTypeface` initializer on the same thread; the CLR
  returned the half-built type with a null field, producing a `NullReferenceException` surfaced as a
  cached `TypeInitializationException` (#3817):

  ```
  SKFontManager.Default
    → SKFontManager..cctor
      → SKObject..ctor (base) → triggers SKObject..cctor mid-cctor
        → SKTypeface..cctor → reads SKFontManager.Default (already running on this thread)
          → CLR returns the half-built type with a null field → NullReferenceException
  ```

- **#4080 — the immortal-wrapper mechanism.** Introduced `GetOrAddImmortalSingletonObject` and the
  promote-to-immortal / dispose-protected machinery in `SKObject`/`HandleDictionary` so shared handles
  dedup to a dispose-proof wrapper (requirement 1). An interim version of that PR centralized init in a
  lock-based `SkiaSharpStatics.EnsureInitialized()` orchestrator (a tri-state monitor with retry, plus a
  hook at the top of `GetOrAddObject`).

- **This PR — static constructors over a shared handle holder.** Replaces the lock-based orchestrator and
  its `GetOrAddObject` hook with the design above: a dependency-free `SkiaSharpStatics..cctor` that
  acquires every raw handle, and one explicit static constructor per wrapper type that adopts those
  handles into immortal `readonly` fields. This keeps the immortal mechanism from #4080, drops the locks
  and the retry, and resolves the #3817 cross-type dependency at the handle level so the
  static-initializer graph is acyclic.

## Tests

- `tests/Tests/SkiaSharp/SKSingletonInitTest.cs` — singleton identity, immortality, `DisposeInternal`
  no-op, `CreateDefault` non-null, and `SKPaint` smoke coverage. The class is in a
  `DisableParallelization` collection (`HandleDictionaryThreadingCollection`) because one test asserts
  exact native refcounts on a process-global singleton.
- `tests/SkiaSharp.Tests.SingletonInit.Console/` — a dedicated cold-start project that touches
  `SKFontManager.Default` first in a fresh process, guarding the exact #3817 regression against any
  future re-introduction of a cross-type cctor cycle.
