# Issue Triage Report — #3153

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T22:08:00Z |
| Type | type/enhancement (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Request to drop the OpenTK metapackage dependency from SkiaSharp.Views.WindowsForms (and SkiaSharp.Views.WPF per comments), as the metapackage pulls in unused packages (OpenTK.Audio.OpenAL, OpenTK.Compute, OpenTK.Input) while OpenTK.GLControl/OpenTK.GLWpfControl already transitively provide everything needed.

**Analysis:** SkiaSharp.Views.WindowsForms and SkiaSharp.Views.WPF both reference the OpenTK root metapackage (v4.8.2 and v4.3.0 respectively) in addition to OpenTK.GLControl/OpenTK.GLWpfControl. The metapackage pulls in OpenTK.Audio.OpenAL, OpenTK.Compute, and OpenTK.Input which are not used by either view library. Removing the explicit OpenTK metapackage reference and relying solely on GLControl/GLWpfControl transitive dependencies would produce a leaner package graph. The net4 (old .NET Framework) path uses OpenTK 3.x which has a monolithic package structure and cannot be split the same way.

**Recommendations:** **needs-investigation** — Enhancement request is well-specified and technically plausible. Requires build verification that removing the OpenTK metapackage still leaves all needed namespaces resolvable through GLControl/GLWpfControl transitive dependencies before shipping.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** net9.0-windows (WPF, NU1701 warnings), net8.0+ WinForms — OpenTK 4.8.2 metapackage pulled in for both

**Repository links:**
- https://github.com/mono/SkiaSharp/discussions/3151 — Related discussion about Windows target version requirement

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The csproj files still explicitly reference the OpenTK metapackage alongside GLControl/GLWpfControl as of the current main branch. |

## Analysis

### Technical Summary

SkiaSharp.Views.WindowsForms and SkiaSharp.Views.WPF both reference the OpenTK root metapackage (v4.8.2 and v4.3.0 respectively) in addition to OpenTK.GLControl/OpenTK.GLWpfControl. The metapackage pulls in OpenTK.Audio.OpenAL, OpenTK.Compute, and OpenTK.Input which are not used by either view library. Removing the explicit OpenTK metapackage reference and relying solely on GLControl/GLWpfControl transitive dependencies would produce a leaner package graph. The net4 (old .NET Framework) path uses OpenTK 3.x which has a monolithic package structure and cannot be split the same way.

### Rationale

The issue is correctly classified as type/enhancement — it improves packaging quality by removing unnecessary transitive dependencies (OpenTK.Audio.OpenAL, OpenTK.Compute, OpenTK.Input) from view packages that have no audio, compute or input needs. The code investigation confirms both .csproj files explicitly include the OpenTK metapackage alongside their GL control packages. The source files (SKGLControl.cs and SKGLElement.cs) only use rendering-related OpenTK namespaces that are available through GLControl/GLWpfControl's own transitive dependencies. The request is well-specified, technically sound, and low-risk. Confidence is 0.88 on enhancement classification because the label 'type/feature-request' was applied by automation, but removing a dependency is improving what exists rather than adding new functionality.

### Key Signals

- "The dependency on OpenTK.GLControl will still transitively resolve everything that's needed (OpenTK.Windowing.Desktop etc.)" — **issue body** (Reporter has verified or expects that GLControl alone covers the needed API surface — plausible given OpenTK 4.x modular design.)
- "I propose consider doing this for WPF side too" — **comment #2647151399** (SkiaSharp.Views.WPF has the same structural issue (OpenTK metapackage + GLWpfControl).)
- "warning NU1701: Package 'OpenTK 3.3.1' was restored using '.NETFramework,...' instead of 'net9.0-windows10.0.17763'" — **comment #2818602076** (On WPF, the stale OpenTK 3.x reference in the net4 condition is causing compatibility warnings even for net9.0-windows users, compounding the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SkiaSharp.Views.WindowsForms.csproj` | 17-20 | direct | For non-net4 targets, references both `OpenTK` (v4.8.2 metapackage) and `OpenTK.GLControl` (v4.0.1). The metapackage brings in OpenTK.Audio.OpenAL, OpenTK.Compute, and OpenTK.Input which are not used by the WinForms GL control. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 4-9 | direct | Uses namespaces from: `OpenTK` (toolkit root), `OpenTK.GLControl`, `OpenTK.Graphics`, and `OpenTK.Graphics.ES20`. These are all provided by OpenTK.GLControl's transitive dependencies, not exclusive to the metapackage. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | 17-20 | direct | For non-net4 targets, references both `OpenTK` (v4.3.0 metapackage) and `OpenTK.GLWpfControl` (v4.2.3). Same unnecessary metapackage bloat applies to WPF path. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 11-23 | direct | Uses `OpenTK`, `OpenTK.Graphics`, `OpenTK.Wpf`, `OpenTK.Graphics.OpenGL`, `OpenTK.Platform.Windows`, `OpenTK.Platform`, and `OpenTK.Mathematics`. All of these originate from OpenTK.GLWpfControl's transitive closure, not from the metapackage-exclusive sub-packages. |

### Next Questions

- Can the net4 path (OpenTK 3.x) also remove the metapackage, or is OpenTK 3.x monolithic only?
- Does OpenTK.GLControl 4.0.1 transitively expose all the OpenTK.Mathematics types used in SKGLElement.cs?
- Should both WindowsForms and WPF packages be updated in the same PR?

### Resolution Proposals

**Hypothesis:** Removing the explicit `OpenTK` metapackage reference from both .csproj files (non-net4 condition) and relying on GLControl/GLWpfControl transitive dependencies will eliminate the unused sub-packages from the consumer's package graph.

1. **Remove OpenTK metapackage from WindowsForms project** — fix, confidence 0.82 (82%), cost/s, validated=untested
   - In SkiaSharp.Views.WindowsForms.csproj, remove the `<PackageReference Include="OpenTK" Version="4.8.2" />` for the non-net4 condition. Verify the build succeeds and no namespace references break.
2. **Remove OpenTK metapackage from WPF project** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - In SkiaSharp.Views.WPF.csproj, remove the `<PackageReference Include="OpenTK" Version="4.3.0" />` for the non-net4 condition. Same approach as WindowsForms fix.
3. **Consumer workaround: exclude unused assemblies** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Consumers can exclude the unused assemblies from their publish output until the fix is released. In the app .csproj, add `<ExcludeAssets>` or use publish profiles with exclusions for OpenTK.Audio.OpenAL.dll, OpenTK.Compute.dll, OpenTK.Input.dll.

**Recommended proposal:** Remove OpenTK metapackage from WindowsForms project

**Why:** Direct fix at source. The reporter already identified the correct approach; implementation just requires removing one package reference and verifying the build.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Enhancement request is well-specified and technically plausible. Requires build verification that removing the OpenTK metapackage still leaves all needed namespaces resolvable through GLControl/GLWpfControl transitive dependencies before shipping. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct type to enhancement (removing unnecessary deps is improving existing packaging, not adding new functionality), apply area and platform labels | labels=type/enhancement, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge request, confirm code investigation findings, and note WPF side is similarly affected | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed analysis! You're correct that `SkiaSharp.Views.WindowsForms` unnecessarily references the `OpenTK` metapackage alongside `OpenTK.GLControl`. Looking at the code, `SKGLControl.cs` only uses rendering-related namespaces (`OpenTK.Graphics`, `OpenTK.Graphics.ES20`, `OpenTK.GLControl`) that are available transitively through `OpenTK.GLControl` itself — so the explicit `OpenTK` metapackage reference is indeed redundant and pulls in `OpenTK.Audio.OpenAL`, `OpenTK.Compute`, and `OpenTK.Input` unnecessarily.

As noted in the comments, the same applies to `SkiaSharp.Views.WPF` which similarly references the `OpenTK` metapackage alongside `OpenTK.GLWpfControl`.

Note: The `.NET Framework` (net4x) path uses OpenTK 3.x which is a monolithic package, so the fix would only apply to the modern .NET condition.

As a workaround until this is addressed, you can exclude the unused assemblies from your publish output.
```
