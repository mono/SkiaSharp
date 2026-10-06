# 9 — Managed stream / callback / delegate-proxy lifetime

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "DelegateProxies|GCHandle|ManagedStream|ReleaseDelegate" binding/SkiaSharp` — a `GCHandle`/proxy freed too early (dangling) or never (leak).

**What it is.** A managed object handed to native code as a callback sink — an
`SKManagedStream`/`SKManagedWStream`/`SKAbstractManagedStream`, a delegate or function-pointer
proxy, or a `GCHandle` pinned for a release/destroy callback — is freed at the wrong time.

**Why it's bad.** Freed **too early** → native invokes a delegate/`GCHandle` that's already
gone → **crash**. Freed **never** → the `GCHandle` and everything it roots leak for the
process lifetime.

**Leak (❌):**
```csharp
// Allocate a GCHandle for the release proc, but never wire the destroy proxy that frees it.
DelegateProxies.Create(releaseProc, out _, out var ctx);
return HarfBuzzApi.hb_blob_create(ptr, len, mode, (void*)ctx, null);  // ❌ proxy = null → ctx leaks
```

**Fix (✓):** pass the destroy proxy so native frees the `GCHandle` when it's done:
```csharp
DelegateProxies.Create(releaseProc, out _, out var ctx);
var proxy = releaseProc != null ? DelegateProxies.DestroyProxy : null;
return HarfBuzzApi.hb_blob_create(ptr, len, mode, (void*)ctx, proxy);
```
Keep the handle rooted for **exactly** the native object's lifetime — not shorter, not longer.

**Watch out (❌ don't):** don't `Free()` the `GCHandle` in the same method that hands it to
native — native still holds it. And don't leave the destroy proxy `null` to "avoid a crash";
that leaks. Free it in the destroy/release callback, and only there.

**Real cases:** #3589, #2916, #996, #2446. `SKManagedStream`/`DelegateProxies` use a `Weak`
user-data `GCHandle` freed by the destroy proxy — the reference implementation.
