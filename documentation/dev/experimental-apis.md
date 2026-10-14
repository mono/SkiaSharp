# Experimental APIs

Mark evolving APIs with `System.Diagnostics.CodeAnalysis.ExperimentalAttribute`.
Use type/member scope inside stable assemblies; assembly scope is only for an
entire experimental component. Keep normal package versioning and do not
retroactively weaken existing stable API contracts.

## Diagnostic registry

SkiaSharp uses `SKIA####`: the first digit selects a family, the remaining three
identify its feature, starting at `001`. HarfBuzzSharp is a separate product and
uses an independent `HARFBUZZ####` sequence starting at `0001`.

| Range | Family |
| --- | --- |
| `SKIA0xxx` | Core and Skia modules, including `GR`, Skottie, SceneGraph and Resources |
| `SKIA1xxx` | All native, MAUI, Uno and Blazor views, including framework backends |
| `SKIA2xxx` | `SkiaSharp.HarfBuzz` integration, not HarfBuzzSharp itself |
| `SKIA3xxx` | External extensions, including Vulkan and Direct3D integrations |
| `SKIA4xxx` | Testing libraries |
| `SKIA5xxx`-`SKIA9xxx` | Unassigned |

Ranges are reserved, not declarations that existing APIs are experimental.
Allocate per independently opted-in feature, not per DLL. Related APIs can share
an ID; unrelated features must not, because suppressing it opts into every API
carrying it. Native-assets payloads and build/analyzer diagnostics are separate.

| ID | Feature | Scope | Status |
| --- | --- | --- | --- |
| `SKIA4001` | [Pixel comparison](../docfx/guides/pixel-comparison.md) | `SkiaSharp.Testing` assembly | Experimental |

No HarfBuzzSharp IDs are allocated. When adding an experiment, record its ID and
link its conceptual/API documentation here. Keep published IDs fixed; when an
API becomes stable, remove its marker but retain the entry as retired. Never
recycle an ID.

Consumers opt in with `<NoWarn>$(NoWarn);SKIA4001</NoWarn>` or a narrower `#pragma`.
No preview SDK/runtime is needed. For targets before .NET 8, preserve the metadata
using an internal compatibility attribute; older compilers may not enforce it.

See [.NET preview APIs](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/preview-apis)
and [diagnostic ID guidance](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/choosing-diagnostic-ids).
