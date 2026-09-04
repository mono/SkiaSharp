# 3 — Managed retention (Views / handlers)

**Where to look.** `source/SkiaSharp.Views*/**`. `grep -rnE "\+= |event |WeakReference|base\.Dispose|Detach" source/SkiaSharp.Views*` — every `+=` needs a matching teardown.

**What it is.** In `source/SkiaSharp.Views*`: a handler, control, or renderer subscribes to
an event (`PaintSurface`, `PropertyChanged`, an invalidation ticker, a platform peer callback)
in a ctor / `Connect` / `Loaded`, but the matching `-=` / `Disconnect` / `Unloaded` /
`Dispose` is missing. Or a derived control's `Dispose(bool)` never chains `base.Dispose(bool)`
when the base owns native resources.

**Why it's bad.** The long-lived event *source* now roots the transient view, so the whole
visual subtree — and the native surfaces/GL contexts it owns — is never collected. Repeated
navigation leaks a surface each time.

**Leak (❌):**
```csharp
public MyCanvasControl()
{
    _ticker.Tick += OnTick;         // subscribe
}
protected override void Dispose(bool disposing)
{
    _surface?.Dispose();
    // ❌ _ticker.Tick -= OnTick never happens → ticker roots `this` forever
}
```

**Fix (✓):**
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
        _ticker.Tick -= OnTick;      // symmetric teardown
    _surface?.Dispose();
    base.Dispose(disposing);          // chain if the base owns native resources
}
```

**Watch out (❌ don't):** don't unsubscribe from inside a finalizer — a finalizer must not
touch other managed objects (the event source may already be finalized). Do the `-=` in
`Dispose(bool disposing)` under `if (disposing)`. And don't forget to chain
`base.Dispose(disposing)` — a subtle leak that looks fixed but isn't.

**Real cases:** #3309, #2955, #2472, #1095 — event/handler teardown and `base.Dispose(bool)`
chaining fixes across the WPF / Forms / MAUI view layers.
