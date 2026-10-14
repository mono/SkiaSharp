# SkiaSharp Documentation

SkiaSharp is a cross-platform 2D graphics API for .NET that wraps Google's Skia Graphics Library.

## Prerequisites

Before building or contributing to SkiaSharp:

- **.NET SDK pinned by the repository** - See the root `global.json` for the required version
- **MAUI workload** - Required for mobile platform targets:
  ```bash
  dotnet workload install maui
  ```

## Architecture

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

**Key principles:**
- C# is the safety boundary (validates parameters, throws exceptions)
- C API is minimal pass-through (trusts C#, returns bool/null on failure)
- Three pointer types: raw (borrowed), owned (delete), ref-counted (unref)

## Documentation Index

### Architecture & Concepts
| Document | Description |
|----------|-------------|
| [architecture.md](architecture.md) | Three layers, type mappings, call flow, threading |
| [memory-management.md](memory-management.md) | Pointer types, ownership, lifecycle |
| [error-handling.md](error-handling.md) | Error patterns across layers |

### Contributing
| Document | Description |
|----------|-------------|
| [adding-apis.md](adding-apis.md) | Step-by-step guide to add bindings |
| [api-design.md](api-design.md) | Stable API rules and experimental feature opt-ins |
| [adding-libraries.md](adding-libraries.md) | Adding new projects and NuGet packages |
| [writing-docs.md](writing-docs.md) | API documentation process |
| [maintaining.md](maintaining.md) | Maintainer responsibilities |

### Documentation system
| Document | Description |
|----------|-------------|
| [docs-overview.md](docs-overview.md) | **Start here** — map of the whole docs system: the four artifacts, engines, skills, and cross-repo automation |
| [writing-docs.md](writing-docs.md) | Operator how-to: generate & edit API docs and api diffs locally |
| [release-notes-and-api-diffs.md](release-notes-and-api-diffs.md) | Behavior spec for the release-notes & API-diff engines |
| [site.md](site.md) | Build, preview, and theme the conceptual docs website |

### Building
| Document | Description |
|----------|-------------|
| [building.md](building.md) | Build on Windows & macOS |
| [building-linux.md](building-linux.md) | Build native libraries for Linux |

### Testing
| Document | Description |
|----------|-------------|
| [golden-image-tests.md](golden-image-tests.md) | The cross-backend visual-regression matrix: scenes, renderers, goldens, tolerance and how to seed them |
| [gpu-test-policy.md](gpu-test-policy.md) | When a GPU backend is required vs skipped, and the `SKIASHARP_TEST_SKIP_GPU` opt-out |
| [containerized-testing.md](containerized-testing.md) | Run the console test suite inside a Docker container (Linux glibc/Alpine + Nano Server) via the bootstrapper `docker:` feature + the `tests-container` cake target |

### Releasing
| Document | Description |
|----------|-------------|
| [releasing.md](releasing.md) | Maintainer runbook: workflows, inputs, approvals, and release checklist |
| [release-process-internals.md](release-process-internals.md) | Release automation architecture, state, invariants, and implementation map |
| [versioning.md](versioning.md) | Version numbering scheme |

### Reference
| Document | Description |
|----------|-------------|
| [dependencies.md](dependencies.md) | Native dependencies: security relevance, CVE tracking, cgmanifest.json |
| [debugging-methodology.md](debugging-methodology.md) | Best practices for complex debugging |

## Experimental API diagnostic IDs

Use `SKIA` followed by **four digits** for compiler-enforced experimental APIs.
The first digit identifies the API family; the remaining three are a sequential
feature number within that family, starting at `001`. For example, `SKIA5001`
is testing feature 001. This is our allocation convention, not a compiler rule:
the number does not encode severity, package version, Skia milestone or target
framework.

These ranges collect the existing managed library families. Reserving a range
does **not** mark those libraries or their existing stable APIs experimental.
Only IDs in the allocated-feature table below are currently in use.

| Range | API family | Existing projects covered |
| --- | --- | --- |
| `SKIA0xxx` | Core graphics | `binding/SkiaSharp`, including its `GR` GPU APIs |
| `SKIA1xxx` | Native/platform views | `SkiaSharp.Views` platform views and desktop common, Windows Forms, WPF, WinUI, GTK3 and GTK4 integrations |
| `SKIA2xxx` | MAUI views | `SkiaSharp.Views.Maui.Core` and `SkiaSharp.Views.Maui.Controls` |
| `SKIA3xxx` | Other UI frameworks and web views | Uno WinUI/Skia/WASM integrations and `SkiaSharp.Views.Blazor` |
| `SKIA4xxx` | Rendering extensions | `SkiaSharp.Skottie`, `SkiaSharp.SceneGraph` and `SkiaSharp.Resources` |
| `SKIA5xxx` | Testing libraries | `SkiaSharp.Testing`; not test-runner or analyzer diagnostics |
| `SKIA6xxx` | Text and shaping | `HarfBuzzSharp` bindings and `SkiaSharp.HarfBuzz` integration |
| `SKIA7xxx` | External GPU integrations | `SkiaSharp.Vulkan.SharpVk`, `SkiaSharp.Vulkan.Silk.NET` and `SkiaSharp.Direct3D.Vortice`; core `GR` APIs remain in `0xxx` |
| `SKIA8xxx` | Unassigned | Reserved for a future library family |
| `SKIA9xxx` | Unassigned | Reserved for a future library family |

Group by API ownership, not physical folder or one prefix per DLL. In particular,
Blazor belongs to `3xxx` even though its project lives under `source/SkiaSharp.Views`.
One coherent feature can span several DLLs and share an ID; independent features
within one DLL need distinct IDs. Native-assets packages are payloads, not
managed API feature groups.

### Allocated features

| Diagnostic | Feature | Scope | Status |
| --- | --- | --- | --- |
| `SKIA5001` | Pixel comparison, tolerance, acceptance budgets and diagnostics | Entire `SkiaSharp.Testing` assembly | Experimental |

Add an entry when introducing an experimental feature. Keep its ID fixed once
published. When it graduates to stable, remove the attribute and retain the entry
as retired; never recycle the ID. Do not suppress an entire range or use one ID
to opt into unrelated experiments.

Consumers opt in to a specific feature, for example:

```xml
<NoWarn>$(NoWarn);SKIA5001</NoWarn>
```

Use type/member attributes for experiments inside stable assemblies; assembly
scope is appropriate only when the whole component is experimental. See
[API design rules](api-design.md#experimental-apis) and Microsoft's
[diagnostic ID guidance](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/choosing-diagnostic-ids).
Existing `SK00xx` build-warning categories and other analyzer/source-generator
diagnostics are separate from this experimental API registry.


## Quick Build

```bash
dotnet cake --target=externals-download  # Get native libs
dotnet build binding/SkiaSharp/SkiaSharp.csproj  # Build managed
dotnet test tests/SkiaSharp.Tests.Console.slnx    # Run all tests
```

## External Resources

- [Skia Documentation](https://skia.org/)
- [Skia API Reference](https://api.skia.org/)
- [SkiaSharp on Microsoft Learn](https://learn.microsoft.com/dotnet/api/skiasharp)
- [SkiaSharp Samples](https://github.com/mono/SkiaSharp/tree/main/samples)
