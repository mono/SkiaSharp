# 10 — Allocation-failure path

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "GetObject\(\s*[a-z]|if \(handle == " binding/SkiaSharp` — a wrapper returned (or half-built) even when the native create returned null.

**What it is.** A factory wraps and returns a managed object even when the native
create/decode returned `null`/`0` or failed, or leaks a half-built native object on the error
path.

**Why it's bad.** A wrapper around `IntPtr.Zero` throws `NullReferenceException` /
`AccessViolationException` on first use, far from the real failure. A half-built native left
un-freed on the error branch is a straight leak.

**Leak/crash (❌):**
```csharp
public static SKFoo Create(...)
{
    var handle = SkiaApi.sk_foo_new(...);   // may return IntPtr.Zero on failure
    return new SKFoo(handle, owns: true);   // ❌ wraps a null handle
}
```

**Fix (✓):**
```csharp
public static SKFoo? Create(...)
{
    var handle = SkiaApi.sk_foo_new(...);
    if (handle == IntPtr.Zero)
        return null;                        // factory returns null on failure
    return new SKFoo(handle, owns: true);
}
```
On multi-step builds, free any partial native objects before returning on the error path.

**Watch out (❌ don't):** don't make a *factory* throw when its contract is to return `null`
(that's an ABI/behavior break — add the null-return, don't change the exception surface).
And don't return `null` while leaving an earlier half-built native object un-freed on the
error branch.

**Real cases:** #1784, #1642. `SKCodec.Create` (revokes stream ownership before disposing on
`codec == null`) and `SKColorSpaceIccProfile.Create` (disposes the half-built profile on parse
failure) are the correct patterns.
