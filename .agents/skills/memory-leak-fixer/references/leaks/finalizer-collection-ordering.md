# 5 — Finalizer / collection ordering

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "GC.KeepAlive|Referenced\(|KeepAliveObjects|internal .* Handle" binding/SkiaSharp`, and compare sibling wrappers — one that keeps no reference to its parent (no field **and** no `Referenced(...)`) where the others do is suspect.

**What it is.** A child wrapper holds a **raw** pointer into a parent's native object but
keeps no managed reference to the parent. Finalization order is non-deterministic, so the
parent can be collected/finalized while the child is still alive.

**Why it's bad.** The child then dereferences freed parent memory → **use-after-free**.
The fix is cheap (root the parent) and has zero ABI impact.

**Leak (❌):** a child cursor is built from a parent's *raw* native handle but drops the
managed parent, so nothing keeps it alive:
```csharp
public class ChildCursor : SKObject, ISKSkipObjectRegistration
{
    internal ChildCursor(SKParent parent)
        : base(SkiaApi.sk_parent_cursor_new(parent.Handle), owns: true)
    {
        // ❌ `parent` is discarded; the native cursor still points at parent's SkParent*.
    }
}
```

**Fix (✓):** root the parent for the child's whole lifetime using the framework's built-in
keep-alive mechanism — **`SKObject.Referenced(owner, child)`**, which parks `child` in the owner's
`KeepAliveObjects` dictionary so the GC can't collect it while the owner lives, and releases it
automatically when the owner is disposed. This is the same helper `SKDocument` (output stream),
`SKColorSpace` (ICC profile), and `SKSVG` (stream) already use — no hand-rolled field required:
```csharp
public class ChildCursor : SKObject, ISKSkipObjectRegistration
{
    internal ChildCursor(SKParent parent)
        : base(SkiaApi.sk_parent_cursor_new(parent.Handle), owns: true)
    {
        Referenced(this, parent);   // parked in this.KeepAliveObjects → rooted for our lifetime
    }
}
```
**Prefer `Referenced(...)` over hand-rolling a `private readonly SKParent parent;` field or a
`List<T>` of children** — the helper is purpose-built (documented in
[`memory-management.md`](../../../../../documentation/dev/memory-management.md#keeping-related-objects-alive-owned--ownedby--referenced)),
needs no extra field, and clears on dispose. A plain `private readonly` field is still *correct* and
may be used to match a class's **existing local convention** — e.g. the `SKRegion` iterators all keep
a `private readonly SKRegion` field, so a new sibling there should follow suit rather than mix styles.

For a **set** of rooted children that changes over time (e.g. `SKNWayCanvas.AddCanvas` /
`RemoveCanvas`), use the helper family instead of a hand-rolled `List<T>`: `Referenced(this, child)`
to root, `Unreferenced(this, child)` to drop one, and `UnreferencedAll(this)` to drop them all —
never reach into `KeepAliveObjects` directly from a derived type.

For a one-shot P/Invoke (no stored child), `GC.KeepAlive(parent)` after the call is enough.

**How to find it.** Look for wrapper types constructed from a parent's `.Handle` that keep **no**
managed reference to that parent — neither a field nor a `Referenced(...)` / `KeepAliveObjects`
entry — especially when *sibling* wrappers of the same parent type DO root it (e.g. one
iterator/cursor stores/`Referenced`s its `SKParent` and another doesn't). The odd one out is the
prime suspect. Prove it before believing it (Phase 2).

**Watch out (❌ don't):** don't root the parent with a pinned `GCHandle` — `Referenced(this, parent)`
(or a plain managed field) is enough, and a pinned handle is its own leak (area 9). Don't hand-roll a
`List<T>`/field — or reach into `KeepAliveObjects` directly from a derived type — when
`Referenced` / `Unreferenced` / `UnreferencedAll` already exist for exactly this. And don't lean on
`GC.KeepAlive` for a *long-lived* child (an iterator you hold across calls); KeepAlive only covers the
current method, so a stored child needs `Referenced(...)` (or the field).

**Real cases:** #3796 (SKPath/SKPathBuilder finalizer race), #3291 (SKAutoCanvasRestore).
