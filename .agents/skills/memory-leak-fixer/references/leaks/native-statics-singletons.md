# 7 — Disposing native statics / singletons

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "GetDisposeProtectedObject|unrefExisting|CreateSrgb|Empty" binding/SkiaSharp` — immortal objects reached via a non-protected cache.

**What it is.** An *immortal* native object — the default/empty typeface, the sRGB /
sRGB-linear color spaces and gamma color filters, the blend-mode blender cache, `SKData.Empty`
— reached through an accessor that is **not** dispose-protected, so the wrapper's
`DisposeNative` unrefs or deletes an object that must live for the whole process.

**Why it's bad.** Unref'ing / deleting a process-wide singleton corrupts it for **every**
caller — crashes or wrong rendering far from the disposal site.

**Leak/crash (❌):**
```csharp
public static SKColorSpace CreateSrgb() =>
    GetObject<SKColorSpace>(SkiaApi.sk_colorspace_new_srgb(), owns: true);  // ❌ singleton
```

**Fix (✓):** route through the dispose-protected accessor so `DisposeNative` is skipped:
```csharp
public static SKColorSpace CreateSrgb() =>
    GetDisposeProtectedObject<SKColorSpace>(
        SkiaApi.sk_colorspace_new_srgb(), owns: false, unrefExisting: false);
```

**Watch out (❌ don't):** don't null out or replace the cached static, and don't wrap it
`owns:true` "just in case." The only correct fix is the dispose-protected accessor with
`unrefExisting:false`; copy an existing correct singleton (`SKBlender` cache) rather than
inventing a new disposal path.

**Real cases:** #1863, #4080, #1224, #3730. The `SKBlender` mode cache and `SKColorFilter`
gamma filters are the canonical correct implementations to copy.
