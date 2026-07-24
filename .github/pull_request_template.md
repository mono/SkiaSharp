<!--
Thanks for contributing to SkiaSharp!

• Target the `main` branch — maintainers backport to release branches.
• Adding or changing API? See documentation/dev/adding-apis.md and documentation/dev/api-design.md.
• Replace each prompt below. Write "None." for sub-sections that don't apply —
  but never delete the "API changes" or "Required skia PR" parts.
-->

### Description

<!-- What does this change do, and why? Reviewers need the *why* most. -->

**Fixes**

Fixes #

<!-- "Fixes #123" / "Closes #123" to auto-close, or "Related to #123" for context. -->

**Areas affected**

- [ ] Managed API (`binding/`)
- [ ] Native / C API (`externals/skia/src/c`, `include/c`)
- [ ] Generated P/Invoke bindings
- [ ] Native dependency or Skia update (libpng, HarfBuzz, FreeType, zlib, milestone bump, …)
- [ ] Views & integrations (MAUI, Uno, WPF, WinUI, Blazor, …)
- [ ] Rendering output / visual behavior
- [ ] Performance
- [ ] Tests
- [ ] Build, packaging, or CI
- [ ] Documentation or samples

**Required skia PR**

None.

<!--
  If this touches the C API or externals/skia submodule, paste the companion PR:
    Requires https://github.com/mono/skia/pull/<number>
  Native changes also require:
   • commit inside externals/skia first, then `git add externals/skia` here
   • re-run `pwsh ./utils/generate.ps1` and commit SkiaApi.generated.cs
-->

### API changes

<!--
  SkiaSharp keeps a STRICT stable ABI: additive only — no removals, no signature
  or return-type changes. Deprecate with [Obsolete] instead of removing.
  Fill the fences below, or write "None." if there are no public API changes.
-->

None.

**Added**

```csharp
// New public signatures, e.g.:
// void SKCanvas.DrawFoo(float x, float y, SKPaint paint);
```

**Obsoleted**

```csharp
// Deprecated APIs + their replacement, e.g.:
// [Obsolete] void SKCanvas.OldMethod();  // use SKCanvas.NewMethod()
```

<!-- Changing or removing an existing public signature is not allowed — deprecate instead. -->

### Behavioral changes

None.

<!-- Any change to rendering output, defaults, exceptions, threading, or memory
     ownership that an app would notice after upgrading. -->

### Testing

<!--
  • What tests did you add or update, and where?
  • How can a reviewer reproduce your verification?
  • Which backends/platforms did you run on (CPU, GPU/Metal/Vulkan/ANGLE,
    Windows/macOS/Linux/Android/iOS/tvOS/Tizen/WASM)? Note anything you could NOT test.
  • Rendering change? Mention any golden-image updates (tests/Content/Goldens/).
-->

**Screenshots (before / after)**

<details>
<summary>Show rendering output</summary>

| Before | After |
| --- | --- |
|  |  |

</details>

### Checklist

- [ ] Tests added or updated (if omitted, explain in Testing above)
- [ ] `API changes` above is complete (or "None.")
- [ ] New/changed public API? Filed a docs issue in [mono/SkiaSharp-API-docs](https://github.com/mono/SkiaSharp-API-docs/issues) so reference docs can be written later
- [ ] Native change? Companion `mono/skia` PR linked above and bindings regenerated
