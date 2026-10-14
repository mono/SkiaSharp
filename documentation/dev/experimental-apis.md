# Experimental APIs

This document owns the experimental API policy, diagnostic namespaces and feature
registry for the products in this repository. Reserving a range does **not** mark
existing libraries or stable APIs experimental. Only allocated features are
currently opted in to this mechanism.

## Diagnostic namespaces

SkiaSharp and HarfBuzzSharp are separate products, even though they share this
repository:

| Product | Format | Allocation |
| --- | --- | --- |
| SkiaSharp and its integrations | `SKIA####` | First digit selects an API family; the remaining three identify its feature |
| HarfBuzzSharp | `HARFBUZZ####` | Independent four-digit sequence, starting at `0001` |

The digits are allocation conventions, not compiler rules. They do not encode
severity, package version, native milestone or target framework. `HARFBUZZ0001`
is the first available HarfBuzzSharp ID, not an allocated feature.

### SkiaSharp API families

| Range | API family | Projects and scope |
| --- | --- | --- |
| `SKIA0xxx` | Core graphics | `binding/SkiaSharp`, including its `GR` GPU APIs |
| `SKIA1xxx` | Native/platform views | `SkiaSharp.Views` platform views and desktop common, Windows Forms, WPF, WinUI, GTK3 and GTK4 integrations |
| `SKIA2xxx` | MAUI | `SkiaSharp.Views.Maui.Core`, `SkiaSharp.Views.Maui.Controls` and future MAUI-owned experimental backends |
| `SKIA3xxx` | Uno | Uno WinUI, Skia and WASM integrations, whether native or web |
| `SKIA4xxx` | Rendering extensions | `SkiaSharp.Skottie`, `SkiaSharp.SceneGraph` and `SkiaSharp.Resources` |
| `SKIA5xxx` | Testing libraries | `SkiaSharp.Testing`; not test-runner or analyzer diagnostics |
| `SKIA6xxx` | SkiaSharp text/shaping integration | `SkiaSharp.HarfBuzz`; HarfBuzzSharp itself uses `HARFBUZZ####` |
| `SKIA7xxx` | External GPU integrations | `SkiaSharp.Vulkan.SharpVk`, `SkiaSharp.Vulkan.Silk.NET` and `SkiaSharp.Direct3D.Vortice`; core `GR` APIs remain in `0xxx` |
| `SKIA8xxx` | Blazor | `SkiaSharp.Views.Blazor` |
| `SKIA9xxx` | Unassigned | Reserved for a future library family |

Feature numbering within a SkiaSharp family starts at `001`, so `SKIA5001` is
testing feature 001. Group by framework/API ownership, not physical folder,
rendering target or one prefix per DLL. Uno is a framework family like MAUI,
not a web-only integration. Blazor belongs to `8xxx` even though its project lives
under `source/SkiaSharp.Views`.

A coherent feature can span several DLLs within a product and share an ID;
independent features within one DLL need distinct IDs. Native-assets packages
are payloads, not managed API feature groups. Use the owning product's namespace
when an integration depends on another product; sharing a repository does not
merge their opt-ins.

## Allocated features

| Diagnostic | Feature | Scope | Status |
| --- | --- | --- | --- |
| `SKIA5001` | Pixel comparison, tolerance, acceptance budgets and diagnostics | Entire `SkiaSharp.Testing` assembly | Experimental |

No `HARFBUZZ####` IDs are allocated yet.

### SKIA5001: Pixel comparison

`SkiaSharp.Testing` is a separate testing library whose public API is still
evolving. The assembly-level experimental marker covers all its comparison types
and entry points. It does not mark `SkiaSharp.dll`, existing views or the
separately published Extended API experimental.

The package follows normal SkiaSharp family versioning. A stable package version
does not promise stability for this explicitly experimental API. See the
[pixel comparison guide](pixel-comparison.md) for behavior, ownership, budgets,
ignore coverage and package-consumer requirements.

## Marking APIs and opting in

Use `System.Diagnostics.CodeAnalysis.ExperimentalAttribute` for new APIs that
may change while collecting feedback. Do not retroactively mark existing stable
APIs experimental to relax their ABI contract.

Use type/member annotations inside stable product or views assemblies. Assembly
scope is appropriate only when the whole component is experimental, as with
`SkiaSharp.Testing`:

```csharp
[assembly: Experimental("SKIA5001")]
```

Supported compilers report that diagnostic when consumers use the API. Consumers
explicitly opt in to the feature, for example:

```xml
<NoWarn>$(NoWarn);SKIA5001</NoWarn>
```

A narrower `#pragma warning disable SKIA5001` / `restore` is also possible.
Suppressing an ID opts into every API carrying it, regardless of which referenced
DLL declares it. Do not use one ID for unrelated experiments or suppress a whole
family.

This is a compiler opt-in, not a runtime feature switch. It requires no preview
runtime, SDK, language version or `<EnablePreviewFeatures>`. For targets before
.NET 8, retain the metadata through an internal compatibility attribute; older
compilers may not enforce the diagnostic. Experimental APIs may change or be
removed, but stable API contracts remain unchanged.

## Adding and graduating a feature

1. Choose the owning product and API family, then allocate the next unused ID.
2. Add a registry entry and a short feature section describing scope, limitations
   and links to its API documentation.
3. Apply the marker at the narrowest appropriate scope and give consumers an
   explicit opt-in. Preserve the normal package versioning and native compatibility
   requirements.
4. Keep published IDs fixed. Once the feature is stable, remove its marker and
   retain the registry entry as retired; never recycle the ID.

Existing `SK00xx` build-warning categories and other analyzer/source-generator
diagnostics are separate from this registry.

## References

- [API design rules](api-design.md)
- [Choosing diagnostic IDs](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/choosing-diagnostic-ids)
- [.NET preview APIs](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/preview-apis)
- [.NET runtime diagnostic registry](https://github.com/dotnet/runtime/blob/main/docs/project/list-of-diagnostics.md#experimental-diagnostics-syslib5001---syslib5999)
