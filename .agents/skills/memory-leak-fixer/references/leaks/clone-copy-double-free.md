# 6 — Clone / copy double-free

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "Clone|MemberwiseClone|_clone" binding/SkiaSharp` — check whether the copy shares or duplicates the native pointer.

**What it is.** A `Clone()`/copy that **shares** one native pointer between two managed
wrappers, both of which believe they own it and will dispose it.

**Why it's bad.** Both wrappers call `DisposeNative` on the same handle → **double-free**.

**Leak (❌):**
```csharp
public SKThing Clone() =>
    new SKThing(Handle, owns: true);   // ❌ two wrappers own the same native object
```

**Fix (✓):** mint a *fresh* native object via the clone API:
```csharp
public SKThing Clone() =>
    GetObject<SKThing>(SkiaApi.sk_thing_clone(Handle));  // fresh handle, owns:true
```

**Watch out (❌ don't):** don't "fix" the double-free by setting `owns:false` on the clone —
that just swaps a double-free for a leak (or a use-after-free if the original is disposed
first). The clone must own a *separate* native object, not borrow the source's.

**Real cases:** #2904 (SKPaint.Clone), #2899.
