# Issue Triage Report — #3422

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T16:26:21Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Blazor (0.93 (93%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SkiaSharp fails to load in Blazor WebAssembly projects targeting .NET 10 (and sometimes .NET 9) with TypeInitializationException or DllNotFoundException for libSkiaSharp, while the same app works on .NET 8.

**Analysis:** SkiaSharp Blazor WASM static native library fails to link or load at runtime under .NET 10. The NativeAssets targets map net9.0 and later to a single Emscripten 3.1.56 static library variant, but .NET 10 may ship a newer incompatible Emscripten toolchain, or users may inadvertently block native relinking by setting WasmBuildNative=false.

**Recommendations:** **needs-investigation** — Real, reproducible bug affecting multiple users on .NET 10 Blazor WASM. Root cause (Emscripten version compatibility or SDK build pipeline change) needs investigation before a fix can be implemented. Repro should determine exact failure mode.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Universal-UWP, os/WASM, area/SkiaSharp.Views.Blazor, tenet/compatibility, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a Blazor WebAssembly project targeting net10.0
2. Add SkiaSharp.Views.Blazor (3.119.x) package reference
3. Add an SKCanvasView component to a page
4. Run the application and navigate to the page with the component

**Environment:** Windows 11 Pro / macOS, Visual Studio 2026 / Rider, .NET 10, Edge/Chrome, SkiaSharp 3.118.0-preview.2 / 3.119.1 / 3.119.2

**Related issues:** #3224, #2745, #3068, #3037, #3435

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3224 — Same TypeInitializationException for SKImageInfo in Blazor WASM (NET 9)
- https://github.com/mono/SkiaSharp/issues/2745 — DllNotFoundException in Blazor WASM .NET 8
- https://github.com/mono/SkiaSharp/issues/3068 — DllNotFoundException in Blazor WASM .NET 9 on upgrade
- https://github.com/mono/SkiaSharp/issues/3037 — DllNotFoundException in Blazor WASM .NET 9 RC2

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | ManagedError: TypeInitialization_Type, SkiaSharp.SKImageInfo / System.DllNotFoundException: libSkiaSharp |
| Repro quality | partial |
| Target frameworks | net10.0 |

**Stack trace:**

```text
System.TypeInitializationException: The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.
 ---> System.DllNotFoundException: libSkiaSharp
   at SkiaSharp.SKImageInfo..cctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.118.0-preview.2.3, 3.119.1, 3.119.2 |
| Worked in | net8.0 (most users), net9.0 (some users) |
| Broke in | net10.0 (all users), net9.0 (some users) |
| Current relevance | likely |
| Relevance reason | Issue is still open, multiple users confirm it reproduces on the latest SkiaSharp 3.119.2 with .NET 10. SkiaSharp targets map net9.0+ to the same Emscripten 3.1.56 static library with no .NET 10-specific entry. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | App works on .NET 8 / .NET 9 (for some users) but fails on .NET 10. The breakage correlates with .NET 10 changes to the WASM build pipeline / Emscripten version, not a SkiaSharp version change. |
| Worked in version | net8.0 |
| Broke in version | net10.0 |

## Analysis

### Technical Summary

SkiaSharp Blazor WASM static native library fails to link or load at runtime under .NET 10. The NativeAssets targets map net9.0 and later to a single Emscripten 3.1.56 static library variant, but .NET 10 may ship a newer incompatible Emscripten toolchain, or users may inadvertently block native relinking by setting WasmBuildNative=false.

### Rationale

Multiple users confirm total failure to use SkiaSharp in Blazor WASM on .NET 10, with the identical error fingerprint (TypeInitializationException / DllNotFoundException for libSkiaSharp). The SkiaSharp.targets file maps net9.0+ to Emscripten 3.1.56 but has no .NET 10-specific entry; if .NET 10 uses a newer Emscripten version the .a static libraries may be ABI-incompatible. A secondary cause is users explicitly setting WasmBuildNative=false which prevents SkiaSharp's targets from enabling native compilation. Different partial workarounds (workload restore, explicit NativeAssets package, WasmBuildNative removal) help some users but not all.

### Key Signals

- "ManagedError: TypeInitialization_Type, SkiaSharp.SKImageInfo" — **issue body** (SKImageInfo's static initializer calls into libSkiaSharp — the DLL load itself is the failure point.)
- "System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKPath..ctor()" — **comment by jahnotto** (libSkiaSharp not linked into dotnet.wasm at all — NativeFileReference was not processed.)
- "Rolling back to .NET8 and it works again however." — **comment by tolzy88** (Issue is specific to .NET 9/10 WASM toolchain change, not a SkiaSharp version issue.)
- "Removing <WasmBuildNative>false</WasmBuildNative> fixed it" — **comment by DrDruid** (Explicit WasmBuildNative=false prevents SkiaSharp's targets from enabling native linking; SkiaSharp only sets it if empty.)
- "I was able to fix it by including the NativeAssets package explicitly" — **comment by tolzy88** (Some users are missing the auto-include; may be related to project type or SDK detection conditions.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | 39-40 | direct | NativeFileReference for net9.0+ uses Emscripten 3.1.56 static libraries (VersionGreaterThanOrEquals '9.0'). There is no net10.0-specific entry. If .NET 10 ships a different Emscripten ABI, the static library will be incompatible. |
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | 22-23 | direct | WasmBuildNative is only set to 'true' if the property was previously empty. If a user explicitly sets WasmBuildNative=false in their project, SkiaSharp's NativeFileReference items are still added but the native compilation step is skipped — causing DllNotFoundException at runtime. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | 43-44 | related | SkiaSharp.NativeAssets.WebAssembly is a ProjectReference when building the solution (not when installed via NuGet). NuGet package auto-includes it via the props/targets in the nuget build folder — this dependency chain may have been broken in .NET 10's SDK. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/nuget/buildTransitive/SkiaSharp.Views.Blazor.props` | 6 | related | EmccExtraLDFlags adds --js-library SkiaSharpInterop.js as a workaround for https://github.com/dotnet/runtime/issues/76077. This dependency on the Emscripten linker flags pipeline means any changes to how .NET 10 invokes Emscripten could silently break the build. |

**Error fingerprint:** `DllNotFoundException:libSkiaSharp@SKImageInfo..cctor/WASM`

### Workarounds

- Run 'dotnet workload restore' and restart IDE (works for some users)
- Explicitly add SkiaSharp.NativeAssets.WebAssembly to the application project: <PackageReference Include="SkiaSharp.NativeAssets.WebAssembly" Version="3.119.2" />
- Remove explicit <WasmBuildNative>false</WasmBuildNative> from the project file if present
- Add <RunAOTCompilation>false</RunAOTCompilation> to the project PropertyGroup
- Target net9.0 or net8.0 instead of net10.0 (confirmed working)

### Next Questions

- What Emscripten version does .NET 10 WASM workload ship, and is it compatible with Emscripten 3.1.56 static libraries?
- Does SkiaSharp need to add a net10.0-specific NativeFileReference mapping to a new Emscripten version?
- Why does 'dotnet workload restore' fix it for some users but not others?
- Does the issue reproduce with a brand-new default Blazor WASM project (no explicit WasmBuildNative setting)?
- Is there a .NET 10 SDK behavior change that prevents SkiaSharp.Views.Blazor's auto-include of NativeAssets from working?

### Resolution Proposals

**Hypothesis:** SkiaSharp's NativeFileReference mapping does not have a .NET 10-specific entry and falls back to the net9.0+ Emscripten 3.1.56 libraries. If .NET 10's wasm-tools workload ships a newer Emscripten version with an incompatible ABI, the static linking fails silently and the symbols are absent from dotnet.wasm at runtime.

1. **Explicitly add net10.0 NativeFileReference or build .NET 10 WASM static libraries** — fix, confidence 0.75 (75%), cost/l, validated=untested
   - Investigate whether .NET 10 requires a new Emscripten version and, if so, build and ship net10.0-specific libSkiaSharp.a files in SkiaSharp.NativeAssets.WebAssembly.
2. **Document and surface the WasmBuildNative=false pitfall** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Add a clear error or warning in SkiaSharp.targets when WasmBuildNative=false is set explicitly, to surface the issue at build time rather than at runtime.
3. **Workaround: add SkiaSharp.NativeAssets.WebAssembly explicitly** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Users should add SkiaSharp.NativeAssets.WebAssembly and run dotnet workload restore as an immediate workaround until native .NET 10 libraries are built.

**Recommended proposal:** Explicitly add net10.0 NativeFileReference or build .NET 10 WASM static libraries

**Why:** The long-term fix is ensuring SkiaSharp ships Emscripten-compatible static libraries for .NET 10. The other proposals are mitigations.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Real, reproducible bug affecting multiple users on .NET 10 Blazor WASM. Root cause (Emscripten version compatibility or SDK build pipeline change) needs investigation before a fix can be implemented. Repro should determine exact failure mode. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post analysis summary with known workarounds and request for reproduction details | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This is a known issue with SkiaSharp in Blazor WASM on .NET 10.

**Root cause hypothesis:** SkiaSharp's WASM build targets use Emscripten 3.1.56 static libraries for `net9.0+`. If .NET 10's wasm-tools workload ships a newer Emscripten version with an incompatible ABI, the static linking step fails and `libSkiaSharp` symbols are absent from `dotnet.wasm` at runtime.

**Known workarounds (try in order):**
1. Run `dotnet workload restore` and restart your IDE
2. Explicitly add `<PackageReference Include="SkiaSharp.NativeAssets.WebAssembly" Version="3.119.2" />` to your application project
3. If you have `<WasmBuildNative>false</WasmBuildNative>` in your project file, remove it (SkiaSharp needs native compilation enabled)
4. Add `<RunAOTCompilation>false</RunAOTCompilation>` to your project

We are investigating .NET 10 WASM Emscripten compatibility. To help reproduce: could you confirm which .NET 10 SDK version you're using (`dotnet --version`) and whether `dotnet workload list` shows the `wasm-tools` workload installed?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3422,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T16:26:21Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Universal-UWP",
      "os/WASM",
      "area/SkiaSharp.Views.Blazor",
      "tenet/compatibility",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SkiaSharp fails to load in Blazor WebAssembly projects targeting .NET 10 (and sometimes .NET 9) with TypeInitializationException or DllNotFoundException for libSkiaSharp, while the same app works on .NET 8.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.93
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "ManagedError: TypeInitialization_Type, SkiaSharp.SKImageInfo / System.DllNotFoundException: libSkiaSharp",
      "stackTrace": "System.TypeInitializationException: The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.\n ---> System.DllNotFoundException: libSkiaSharp\n   at SkiaSharp.SKImageInfo..cctor()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WebAssembly project targeting net10.0",
        "Add SkiaSharp.Views.Blazor (3.119.x) package reference",
        "Add an SKCanvasView component to a page",
        "Run the application and navigate to the page with the component"
      ],
      "environmentDetails": "Windows 11 Pro / macOS, Visual Studio 2026 / Rider, .NET 10, Edge/Chrome, SkiaSharp 3.118.0-preview.2 / 3.119.1 / 3.119.2",
      "relatedIssues": [
        3224,
        2745,
        3068,
        3037,
        3435
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3224",
          "description": "Same TypeInitializationException for SKImageInfo in Blazor WASM (NET 9)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2745",
          "description": "DllNotFoundException in Blazor WASM .NET 8"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3068",
          "description": "DllNotFoundException in Blazor WASM .NET 9 on upgrade"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3037",
          "description": "DllNotFoundException in Blazor WASM .NET 9 RC2"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.118.0-preview.2.3",
        "3.119.1",
        "3.119.2"
      ],
      "workedIn": "net8.0 (most users), net9.0 (some users)",
      "brokeIn": "net10.0 (all users), net9.0 (some users)",
      "currentRelevance": "likely",
      "relevanceReason": "Issue is still open, multiple users confirm it reproduces on the latest SkiaSharp 3.119.2 with .NET 10. SkiaSharp targets map net9.0+ to the same Emscripten 3.1.56 static library with no .NET 10-specific entry."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "App works on .NET 8 / .NET 9 (for some users) but fails on .NET 10. The breakage correlates with .NET 10 changes to the WASM build pipeline / Emscripten version, not a SkiaSharp version change.",
      "workedInVersion": "net8.0",
      "brokeInVersion": "net10.0"
    }
  },
  "analysis": {
    "summary": "SkiaSharp Blazor WASM static native library fails to link or load at runtime under .NET 10. The NativeAssets targets map net9.0 and later to a single Emscripten 3.1.56 static library variant, but .NET 10 may ship a newer incompatible Emscripten toolchain, or users may inadvertently block native relinking by setting WasmBuildNative=false.",
    "rationale": "Multiple users confirm total failure to use SkiaSharp in Blazor WASM on .NET 10, with the identical error fingerprint (TypeInitializationException / DllNotFoundException for libSkiaSharp). The SkiaSharp.targets file maps net9.0+ to Emscripten 3.1.56 but has no .NET 10-specific entry; if .NET 10 uses a newer Emscripten version the .a static libraries may be ABI-incompatible. A secondary cause is users explicitly setting WasmBuildNative=false which prevents SkiaSharp's targets from enabling native compilation. Different partial workarounds (workload restore, explicit NativeAssets package, WasmBuildNative removal) help some users but not all.",
    "keySignals": [
      {
        "text": "ManagedError: TypeInitialization_Type, SkiaSharp.SKImageInfo",
        "source": "issue body",
        "interpretation": "SKImageInfo's static initializer calls into libSkiaSharp — the DLL load itself is the failure point."
      },
      {
        "text": "System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKPath..ctor()",
        "source": "comment by jahnotto",
        "interpretation": "libSkiaSharp not linked into dotnet.wasm at all — NativeFileReference was not processed."
      },
      {
        "text": "Rolling back to .NET8 and it works again however.",
        "source": "comment by tolzy88",
        "interpretation": "Issue is specific to .NET 9/10 WASM toolchain change, not a SkiaSharp version issue."
      },
      {
        "text": "Removing <WasmBuildNative>false</WasmBuildNative> fixed it",
        "source": "comment by DrDruid",
        "interpretation": "Explicit WasmBuildNative=false prevents SkiaSharp's targets from enabling native linking; SkiaSharp only sets it if empty."
      },
      {
        "text": "I was able to fix it by including the NativeAssets package explicitly",
        "source": "comment by tolzy88",
        "interpretation": "Some users are missing the auto-include; may be related to project type or SDK detection conditions."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "lines": "39-40",
        "finding": "NativeFileReference for net9.0+ uses Emscripten 3.1.56 static libraries (VersionGreaterThanOrEquals '9.0'). There is no net10.0-specific entry. If .NET 10 ships a different Emscripten ABI, the static library will be incompatible.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "lines": "22-23",
        "finding": "WasmBuildNative is only set to 'true' if the property was previously empty. If a user explicitly sets WasmBuildNative=false in their project, SkiaSharp's NativeFileReference items are still added but the native compilation step is skipped — causing DllNotFoundException at runtime.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "lines": "43-44",
        "finding": "SkiaSharp.NativeAssets.WebAssembly is a ProjectReference when building the solution (not when installed via NuGet). NuGet package auto-includes it via the props/targets in the nuget build folder — this dependency chain may have been broken in .NET 10's SDK.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/nuget/buildTransitive/SkiaSharp.Views.Blazor.props",
        "lines": "6",
        "finding": "EmccExtraLDFlags adds --js-library SkiaSharpInterop.js as a workaround for https://github.com/dotnet/runtime/issues/76077. This dependency on the Emscripten linker flags pipeline means any changes to how .NET 10 invokes Emscripten could silently break the build.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What Emscripten version does .NET 10 WASM workload ship, and is it compatible with Emscripten 3.1.56 static libraries?",
      "Does SkiaSharp need to add a net10.0-specific NativeFileReference mapping to a new Emscripten version?",
      "Why does 'dotnet workload restore' fix it for some users but not others?",
      "Does the issue reproduce with a brand-new default Blazor WASM project (no explicit WasmBuildNative setting)?",
      "Is there a .NET 10 SDK behavior change that prevents SkiaSharp.Views.Blazor's auto-include of NativeAssets from working?"
    ],
    "workarounds": [
      "Run 'dotnet workload restore' and restart IDE (works for some users)",
      "Explicitly add SkiaSharp.NativeAssets.WebAssembly to the application project: <PackageReference Include=\"SkiaSharp.NativeAssets.WebAssembly\" Version=\"3.119.2\" />",
      "Remove explicit <WasmBuildNative>false</WasmBuildNative> from the project file if present",
      "Add <RunAOTCompilation>false</RunAOTCompilation> to the project PropertyGroup",
      "Target net9.0 or net8.0 instead of net10.0 (confirmed working)"
    ],
    "errorFingerprint": "DllNotFoundException:libSkiaSharp@SKImageInfo..cctor/WASM",
    "resolution": {
      "hypothesis": "SkiaSharp's NativeFileReference mapping does not have a .NET 10-specific entry and falls back to the net9.0+ Emscripten 3.1.56 libraries. If .NET 10's wasm-tools workload ships a newer Emscripten version with an incompatible ABI, the static linking fails silently and the symbols are absent from dotnet.wasm at runtime.",
      "proposals": [
        {
          "title": "Explicitly add net10.0 NativeFileReference or build .NET 10 WASM static libraries",
          "description": "Investigate whether .NET 10 requires a new Emscripten version and, if so, build and ship net10.0-specific libSkiaSharp.a files in SkiaSharp.NativeAssets.WebAssembly.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Document and surface the WasmBuildNative=false pitfall",
          "description": "Add a clear error or warning in SkiaSharp.targets when WasmBuildNative=false is set explicitly, to surface the issue at build time rather than at runtime.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: add SkiaSharp.NativeAssets.WebAssembly explicitly",
          "description": "Users should add SkiaSharp.NativeAssets.WebAssembly and run dotnet workload restore as an immediate workaround until native .NET 10 libraries are built.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explicitly add net10.0 NativeFileReference or build .NET 10 WASM static libraries",
      "recommendedReason": "The long-term fix is ensuring SkiaSharp ships Emscripten-compatible static libraries for .NET 10. The other proposals are mitigations."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Real, reproducible bug affecting multiple users on .NET 10 Blazor WASM. Root cause (Emscripten version compatibility or SDK build pipeline change) needs investigation before a fix can be implemented. Repro should determine exact failure mode.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis summary with known workarounds and request for reproduction details",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report. This is a known issue with SkiaSharp in Blazor WASM on .NET 10.\n\n**Root cause hypothesis:** SkiaSharp's WASM build targets use Emscripten 3.1.56 static libraries for `net9.0+`. If .NET 10's wasm-tools workload ships a newer Emscripten version with an incompatible ABI, the static linking step fails and `libSkiaSharp` symbols are absent from `dotnet.wasm` at runtime.\n\n**Known workarounds (try in order):**\n1. Run `dotnet workload restore` and restart your IDE\n2. Explicitly add `<PackageReference Include=\"SkiaSharp.NativeAssets.WebAssembly\" Version=\"3.119.2\" />` to your application project\n3. If you have `<WasmBuildNative>false</WasmBuildNative>` in your project file, remove it (SkiaSharp needs native compilation enabled)\n4. Add `<RunAOTCompilation>false</RunAOTCompilation>` to your project\n\nWe are investigating .NET 10 WASM Emscripten compatibility. To help reproduce: could you confirm which .NET 10 SDK version you're using (`dotnet --version`) and whether `dotnet workload list` shows the `wasm-tools` workload installed?"
      }
    ]
  }
}
```

</details>
