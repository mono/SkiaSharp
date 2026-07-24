<!--
Thanks for contributing to SkiaSharp!

• Target the `main` branch — maintainers backport to release branches.
• Adding or changing API? See documentation/dev/adding-apis.md and documentation/dev/api-design.md.
• Just fill in the Description — the other sections have sensible defaults. Each
  section's comment holds a copy-paste block; drop it in only when you have something.
-->

## Description

<!-- What does this change do, and why? Reviewers need the *why* most. -->

**Related issues**

Fixes #

<!-- "Fixes #123" / "Closes #123" auto-closes the issue; use "Related to #123" for context only. -->

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
Touching the C API or the externals/skia submodule? Replace "None." above with:

Requires https://github.com/mono/skia/pull/<number>

Native changes also require committing inside externals/skia (then `git add externals/skia`
here) and re-running `pwsh ./utils/generate.ps1` to regenerate + commit SkiaApi.generated.cs.
-->

## Changes

None.

<!--
SkiaSharp keeps a STRICT stable ABI: additive only — no removals or signature /
return-type changes; deprecate with [Obsolete] instead of removing.

Changed public API or behavior? Copy the parts that apply over "None." above:

**Added**

```csharp
void SKCanvas.DrawFoo(float x, float y, SKPaint paint);
```

**Obsoleted**

```csharp
[Obsolete] void SKCanvas.OldMethod();  // use SKCanvas.NewMethod()
```

**Behavioral**

A change to rendering output, defaults, exceptions, threading, or memory
ownership that an app would notice after upgrading.
-->

## Testing

<!--
How did you verify this? Add or update tests where relevant, and note which
backends/platforms you ran on (CPU, GPU/Metal/Vulkan/ANGLE, Windows/macOS/Linux/
Android/iOS/tvOS/Tizen/WASM) and anything you could NOT test.

Rendering change? Mention golden-image updates (tests/Content/Goldens/) and copy
this in to show the difference:

<details>
<summary>Screenshots (before / after)</summary>

| Before | After |
| --- | --- |
|  |  |

</details>
-->

## Checklist

- [ ] Tests added or updated (if omitted, explain why above)
- [ ] `Changes` above lists all public API and behavioral changes (or "None.")
- [ ] New/changed public API? Filed a docs issue in [mono/SkiaSharp-API-docs](https://github.com/mono/SkiaSharp-API-docs/issues) so reference docs can be written later
- [ ] Native change? Companion `mono/skia` PR linked above and bindings regenerated
