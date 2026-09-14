# 4 — `fixed`-pointer lifetime

**Where to look.** `binding/**` and `source/**`. `grep -rnE "fixed *\(" binding source`, then check whether the native call copies the buffer or retains the pointer past the block.

**What it is.** A `fixed` block produces a pointer into a managed array and hands it to native
code that **stores** the pointer (a non-copying mode) and outlives the block. Once the block
exits, the array is unpinned.

**Why it's bad.** After `fixed` ends the GC is free to move or collect the array, but native
code still holds the old address → **use-after-free / silent data corruption** under GC
pressure. Intermittent, load-dependent, extremely hard to reproduce.

**Leak (❌):** a non-copying native API stores the pointer, but the array is unpinned the
moment the `fixed` block exits:
```csharp
byte[] data = GetManagedBuffer();
fixed (byte* ptr = data)
{
    // ❌ native keeps `ptr`, yet `data` is free to move/collect once this block ends
    return new SKNativeThing(ptr, data.Length, copy: false, () => { /* release */ });
}
```

**Fix (✓):** pin stably with a `GCHandle` and free it only when native releases the object:
```csharp
byte[] data = GetManagedBuffer();
var handle = GCHandle.Alloc(data, GCHandleType.Pinned);     // stable pin; GC can't move it
return new SKNativeThing(handle.AddrOfPinnedObject(), data.Length,
                         copy: false, () => handle.Free());  // freed in the release callback
```
(Or have the native API copy the buffer, so no pin is needed at all.)

**Watch out (❌ don't):** don't "fix" this by adding `GC.KeepAlive(data)` *inside* the `fixed`
block — the pointer escapes the block, so KeepAlive there proves nothing. And don't free the
`GCHandle` before native is finished with the memory; free it in the release delegate.

**Real cases:** #3472 / PR #3473 (a `fixed`-pointer that escapes into a non-copying native
API); the ownership model in `documentation/dev/memory-management.md`.
