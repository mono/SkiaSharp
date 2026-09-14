# 1 — Wrong `owns:` flag

**Where to look.** `binding/SkiaSharp/**`. `grep -rnE "owns: *(true|false)|GetOrAddObject" binding/SkiaSharp`, then match each against the P/Invoke name that produced the handle.

**What it is.** The `owns:` argument to `GetObject`/the wrapper ctor doesn't match the
ownership contract: a *borrowed* pointer (a `_get_` getter that returns an internal pointer)
is wrapped `owns:true`, or an *owned* handle (a `_new_`/create that returns a fresh object)
is wrapped `owns:false`.

**Why it's bad.** `owns:true` on a borrowed pointer → the wrapper's `DisposeNative` deletes
or unrefs an object it doesn't own → **double-free / `AccessViolationException`**, often in
unrelated code later. `owns:false` on an owned handle → **the object is never freed → leak**.

**Leak (❌):**
```csharp
// sk_foo_get_bar returns a BORROWED pointer owned by the parent foo.
public SKBar Bar =>
    GetObject<SKBar>(SkiaApi.sk_foo_get_bar(Handle), owns: true);  // ❌ double-free
```

**Fix (✓):**
```csharp
public SKBar Bar =>
    GetObject<SKBar>(SkiaApi.sk_foo_get_bar(Handle), owns: false); // borrowed → don't free
```
Conversely, a `sk_bar_new(...)` result is a fresh object and must be `owns: true`.

**Watch out (❌ don't):** don't guess the flag or flip it to make a crash/leak "go away."
Read the P/Invoke name: `_new_`/`_create` returns owned → `owns:true`; `_get_`/property-style
accessors return borrowed → `owns:false`. Getting this backwards just swaps a leak for a
crash. When the contract is genuinely unclear from the managed side, file an issue rather
than flipping blind.

**Real cases:** the counterpart of area 7 (dispose-protected singletons); verify each new
getter against whether it returns a fresh ref or a borrowed pointer.
