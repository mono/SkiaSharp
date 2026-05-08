# Issue Triage Report — #3224

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T23:52:15Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Blazor (0.88 (88%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** DllNotFoundException for libSkiaSharp on Blazor WebAssembly starting with SkiaSharp 3.x previews causes TypeInitializationException on SKImageInfo static constructor; multiple users report the issue persisting through 3.119.0 and .NET 9/10 when native static library is not linked into dotnet.wasm.

**Analysis:** DllNotFoundException in Blazor WASM is a static linking failure — libSkiaSharp.a is not being linked into dotnet.wasm at build time. SkiaSharp.NativeAssets.WebAssembly auto-configures WasmBuildNative=true and the correct NativeFileReference when the Blazor WASM SDK is detected, but only triggers when UsingMicrosoftNETSdkBlazorWebAssembly is true in the application project. Users who reference SkiaSharp only through a class library (without SkiaSharp.NativeAssets.WebAssembly or SkiaSharp.Views.Blazor in the executable project) miss the static linking step.

**Recommendations:** **needs-info** — The most likely cause is missing SkiaSharp.NativeAssets.WebAssembly in the application project, but this needs confirmation via the .csproj. The official demo works on .NET 10, strongly suggesting configuration rather than a packaging bug.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Blazor WebAssembly project targeting .NET 8+
2. Reference a class library that uses SkiaSharp without SkiaSharp.NativeAssets.WebAssembly in the application project
3. Run the Blazor WASM project in the browser
4. Observe DllNotFoundException: libSkiaSharp thrown from SKImageInfo static constructor

**Environment:** SkiaSharp 3.118.0-preview.2 through 3.119.0, Windows, Visual Studio 2022, Blazor WebAssembly, .NET 8/9/10

**Repository links:**
- https://github.com/user-attachments/assets/0e541868-0015-4009-ba61-220d0d9129a2 — Screenshot of DllNotFoundException error in browser
- https://github.com/tossnet/Blazor-Captcha/tree/master/BlazorCaptcha — Reporter's Blazor Captcha class library using SkiaSharp
- https://github.com/mono/SkiaSharp/issues/3068 — Related: .NET9 SkiaSharp.Views.Blazor app won't start (closed as completed)
- https://github.com/mono/SkiaSharp/issues/3037 — Related: DllNotFoundException in Blazor WASM .NET 9 RC2 (workaround in comments)
- https://github.com/mono/SkiaSharp/issues/3422 — Related open issue: Blazor WASM NET10 same TypeInitialization exception (triaged)
- https://github.com/mono/SkiaSharp/issues/2732 — Related open issue: SKImageInfo TypeInitializationException in Blazor WASM

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. ---> System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKImageInfo..cctor() |
| Repro quality | partial |
| Target frameworks | net8.0, net9.0, net10.0 |

**Stack trace:**

```text
at SkiaSharp.SKImageInfo..cctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.118.0-preview.2, 2.88.9, 3.119.0 |
| Worked in | 2.88.9 |
| Broke in | 3.118.0-preview.2 |
| Current relevance | likely |
| Relevance reason | Multiple commenters confirm the issue on 3.119.0 with .NET 8, 9, and 10. The issue reporter confirmed the official SkiaSharp demo works on .NET 10, suggesting a configuration gap rather than a fundamental packaging failure. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter confirms 2.88.9 worked; broke at 3.118.0-preview.2. SkiaSharp 3.x changed the WASM integration model from dynamic loading to Emscripten static linking requiring explicit NativeFileReference in the application project. |
| Worked in version | 2.88.9 |
| Broke in version | 3.118.0-preview.2 |

## Analysis

### Technical Summary

DllNotFoundException in Blazor WASM is a static linking failure — libSkiaSharp.a is not being linked into dotnet.wasm at build time. SkiaSharp.NativeAssets.WebAssembly auto-configures WasmBuildNative=true and the correct NativeFileReference when the Blazor WASM SDK is detected, but only triggers when UsingMicrosoftNETSdkBlazorWebAssembly is true in the application project. Users who reference SkiaSharp only through a class library (without SkiaSharp.NativeAssets.WebAssembly or SkiaSharp.Views.Blazor in the executable project) miss the static linking step.

### Rationale

The error is clearly a WASM static linking issue, not a runtime P/Invoke failure. The .targets file in SkiaSharp.NativeAssets.WebAssembly sets WasmBuildNative=true and NativeFileReference automatically for Blazor WASM SDK projects. The inconsistency in reports (some setups work, others don't) and the reporter confirming the official demo works on .NET 10 all point to a configuration gap: the native assets package is either not referenced in the application project or the project SDK type prevents the auto-include condition from firing.

### Key Signals

- "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. ---> System.DllNotFoundException: libSkiaSharp" — **issue body** (libSkiaSharp not found in WASM context means the .a file was not statically linked into dotnet.wasm at build time — not a runtime P/Invoke failure.)
- "no problem on Blazor Server" — **issue body** (Blazor Server uses the server-side native binary auto-included for Windows/Linux. The problem is exclusively in the WASM static linking step.)
- "From #3068, I've noticed that this problem still persists in the latest version" — **comment by capdiem** (The fix applied in #3068 did not fully resolve the issue for all project configurations.)
- "And yet this demo project here on GitHub Pages works... (.net10)" — **comment by tossnet (issue reporter, Feb 2026)** (The official SkiaSharp demo works on .NET 10, confirming the packaging is correct. The issue is in the user's project setup.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | 22-41 | direct | Auto-sets WasmBuildNative=true and adds NativeFileReference for the correct Emscripten version per TFM (net8.0→3.1.34, net9.0+→3.1.56). ONLY fires when UsingMicrosoftNETSdkBlazorWebAssembly or UsingMicrosoftNETSdkWebAssembly is 'true'. Class library projects do not set these properties; the package must be referenced by the executable Blazor project. |
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.props` | 4-9 | direct | Defines SkiaSharpStaticLibraryPath pointing to the bundled .a files inside the package directory. Required for NativeFileReference to resolve correctly. |
| `documentation/dev/packages.md` | 95-99 | related | Confirms WASM uses static linking (NativeFileReference), not P/Invoke dynamic loading. SkiaSharp.NativeAssets.WebAssembly is auto-included by SkiaSharp.Views.Blazor but must be manually added when using SkiaSharp directly. Must be referenced in the application project, not a class library. |

### Workarounds

- Add SkiaSharp.NativeAssets.WebAssembly NuGet package to the Blazor WASM application project (not the class library) — the package auto-configures WasmBuildNative and NativeFileReference
- Use SkiaSharp.Views.Blazor in the application project, which auto-includes SkiaSharp.NativeAssets.WebAssembly as a transitive dependency
- Manually add both <WasmBuildNative>true</WasmBuildNative> and a NativeFileReference pointing to the .a files in the application .csproj (only necessary if already referencing SkiaSharp.NativeAssets.WebAssembly and the auto-include is not triggering)

### Next Questions

- Does the reporter's Blazor WASM test project reference SkiaSharp.NativeAssets.WebAssembly directly in its .csproj?
- Is the test project using the Blazor WebAssembly Standalone SDK (Microsoft.NET.Sdk.BlazorWebAssembly) or a Blazor Web App with server-side render mode?
- Can the reporter confirm whether adding SkiaSharp.NativeAssets.WebAssembly to the application project resolves the error?

### Resolution Proposals

**Hypothesis:** The Blazor WASM application project is not referencing SkiaSharp.NativeAssets.WebAssembly, so the native static library is not linked into dotnet.wasm at build time. The DllNotFoundException is the WASM runtime's way of reporting a missing statically-linked symbol.

1. **Add SkiaSharp.NativeAssets.WebAssembly to application project** — workaround, confidence 0.80 (80%), cost/xs, validated=yes
   - Add SkiaSharp.NativeAssets.WebAssembly to the Blazor WASM application project .csproj. This package auto-sets WasmBuildNative=true and adds the correct NativeFileReference for the detected .NET version (3.1.34 for net8.0, 3.1.56 for net9.0+).

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.WebAssembly" Version="3.119.0" />
```
2. **Switch to SkiaSharp.Views.Blazor** — alternative, confidence 0.90 (90%), cost/s, validated=yes
   - Reference SkiaSharp.Views.Blazor in the application project. It auto-includes SkiaSharp.NativeAssets.WebAssembly and provides SKCanvasView for canvas rendering in Blazor WASM.

```csharp
<PackageReference Include="SkiaSharp.Views.Blazor" Version="3.119.0" />
```
3. **Investigate SDK condition gate in SkiaSharp.targets** — investigation, confidence 0.65 (65%), cost/m, validated=untested
   - The UsingMicrosoftNETSdkBlazorWebAssembly condition in SkiaSharp.targets may not be set in all Blazor Web App configurations (e.g., projects using Microsoft.NET.Sdk.Web with WASM interactivity). Investigate whether the targets file needs an additional condition to cover the Blazor Web App hosted model.

**Recommended proposal:** Add SkiaSharp.NativeAssets.WebAssembly to application project

**Why:** Simplest one-line fix in the application project. The package auto-handles WasmBuildNative and the correct Emscripten library version for all supported .NET versions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | The most likely cause is missing SkiaSharp.NativeAssets.WebAssembly in the application project, but this needs confirmation via the .csproj. The official demo works on .NET 10, strongly suggesting configuration rather than a packaging bug. |
| Suggested repro platform | linux |

### Missing Info

- Full .csproj content of the Blazor WASM application project (the one that runs in the browser)
- Whether SkiaSharp.NativeAssets.WebAssembly is referenced in the application project
- Whether the project SDK is Microsoft.NET.Sdk.BlazorWebAssembly or Microsoft.NET.Sdk.Web

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, WASM, Views.Blazor, reliability labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Ask for project file and provide workaround with static linking explanation | — |
| link-related | low | 0.90 (90%) | Link to related open issue #3422 covering same error on .NET 10 | linkedIssue=#3422 |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this. The `DllNotFoundException: libSkiaSharp` on Blazor WebAssembly is a **static linking** issue — unlike other platforms, WASM does not use P/Invoke dynamic loading. The native `.a` library must be statically linked into `dotnet.wasm` at build time.

The most common cause is that `SkiaSharp.NativeAssets.WebAssembly` is not referenced in your Blazor WASM **application** project. It can't come through a class library reference alone — it must be in the executable project's `.csproj`.

**Quick fix:** Add to your Blazor WASM app project's `.csproj`:
```xml
<PackageReference Include="SkiaSharp.NativeAssets.WebAssembly" Version="3.119.0" />
```
This package automatically sets `<WasmBuildNative>true</WasmBuildNative>` and links the correct `.a` file for your .NET version.

Alternatively, if you'd prefer to use Blazor canvas controls, `SkiaSharp.Views.Blazor` already includes this automatically.

Could you share the `.csproj` of your Blazor WASM **application** project (the one that runs in the browser, not the `BlazorCaptcha` class library)? That will help confirm whether this is a configuration issue or a packaging bug in SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3224,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T23:52:15Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "DllNotFoundException for libSkiaSharp on Blazor WebAssembly starting with SkiaSharp 3.x previews causes TypeInitializationException on SKImageInfo static constructor; multiple users report the issue persisting through 3.119.0 and .NET 9/10 when native static library is not linked into dotnet.wasm.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.88
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. ---> System.DllNotFoundException: libSkiaSharp at SkiaSharp.SKImageInfo..cctor()",
      "stackTrace": "at SkiaSharp.SKImageInfo..cctor()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0",
        "net9.0",
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WebAssembly project targeting .NET 8+",
        "Reference a class library that uses SkiaSharp without SkiaSharp.NativeAssets.WebAssembly in the application project",
        "Run the Blazor WASM project in the browser",
        "Observe DllNotFoundException: libSkiaSharp thrown from SKImageInfo static constructor"
      ],
      "environmentDetails": "SkiaSharp 3.118.0-preview.2 through 3.119.0, Windows, Visual Studio 2022, Blazor WebAssembly, .NET 8/9/10",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/assets/0e541868-0015-4009-ba61-220d0d9129a2",
          "description": "Screenshot of DllNotFoundException error in browser"
        },
        {
          "url": "https://github.com/tossnet/Blazor-Captcha/tree/master/BlazorCaptcha",
          "description": "Reporter's Blazor Captcha class library using SkiaSharp"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3068",
          "description": "Related: .NET9 SkiaSharp.Views.Blazor app won't start (closed as completed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3037",
          "description": "Related: DllNotFoundException in Blazor WASM .NET 9 RC2 (workaround in comments)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3422",
          "description": "Related open issue: Blazor WASM NET10 same TypeInitialization exception (triaged)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2732",
          "description": "Related open issue: SKImageInfo TypeInitializationException in Blazor WASM"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.118.0-preview.2",
        "2.88.9",
        "3.119.0"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.118.0-preview.2",
      "currentRelevance": "likely",
      "relevanceReason": "Multiple commenters confirm the issue on 3.119.0 with .NET 8, 9, and 10. The issue reporter confirmed the official SkiaSharp demo works on .NET 10, suggesting a configuration gap rather than a fundamental packaging failure."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter confirms 2.88.9 worked; broke at 3.118.0-preview.2. SkiaSharp 3.x changed the WASM integration model from dynamic loading to Emscripten static linking requiring explicit NativeFileReference in the application project.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.118.0-preview.2"
    }
  },
  "analysis": {
    "summary": "DllNotFoundException in Blazor WASM is a static linking failure — libSkiaSharp.a is not being linked into dotnet.wasm at build time. SkiaSharp.NativeAssets.WebAssembly auto-configures WasmBuildNative=true and the correct NativeFileReference when the Blazor WASM SDK is detected, but only triggers when UsingMicrosoftNETSdkBlazorWebAssembly is true in the application project. Users who reference SkiaSharp only through a class library (without SkiaSharp.NativeAssets.WebAssembly or SkiaSharp.Views.Blazor in the executable project) miss the static linking step.",
    "rationale": "The error is clearly a WASM static linking issue, not a runtime P/Invoke failure. The .targets file in SkiaSharp.NativeAssets.WebAssembly sets WasmBuildNative=true and NativeFileReference automatically for Blazor WASM SDK projects. The inconsistency in reports (some setups work, others don't) and the reporter confirming the official demo works on .NET 10 all point to a configuration gap: the native assets package is either not referenced in the application project or the project SDK type prevents the auto-include condition from firing.",
    "keySignals": [
      {
        "text": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. ---> System.DllNotFoundException: libSkiaSharp",
        "source": "issue body",
        "interpretation": "libSkiaSharp not found in WASM context means the .a file was not statically linked into dotnet.wasm at build time — not a runtime P/Invoke failure."
      },
      {
        "text": "no problem on Blazor Server",
        "source": "issue body",
        "interpretation": "Blazor Server uses the server-side native binary auto-included for Windows/Linux. The problem is exclusively in the WASM static linking step."
      },
      {
        "text": "From #3068, I've noticed that this problem still persists in the latest version",
        "source": "comment by capdiem",
        "interpretation": "The fix applied in #3068 did not fully resolve the issue for all project configurations."
      },
      {
        "text": "And yet this demo project here on GitHub Pages works... (.net10)",
        "source": "comment by tossnet (issue reporter, Feb 2026)",
        "interpretation": "The official SkiaSharp demo works on .NET 10, confirming the packaging is correct. The issue is in the user's project setup."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "lines": "22-41",
        "finding": "Auto-sets WasmBuildNative=true and adds NativeFileReference for the correct Emscripten version per TFM (net8.0→3.1.34, net9.0+→3.1.56). ONLY fires when UsingMicrosoftNETSdkBlazorWebAssembly or UsingMicrosoftNETSdkWebAssembly is 'true'. Class library projects do not set these properties; the package must be referenced by the executable Blazor project.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.props",
        "lines": "4-9",
        "finding": "Defines SkiaSharpStaticLibraryPath pointing to the bundled .a files inside the package directory. Required for NativeFileReference to resolve correctly.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "95-99",
        "finding": "Confirms WASM uses static linking (NativeFileReference), not P/Invoke dynamic loading. SkiaSharp.NativeAssets.WebAssembly is auto-included by SkiaSharp.Views.Blazor but must be manually added when using SkiaSharp directly. Must be referenced in the application project, not a class library.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add SkiaSharp.NativeAssets.WebAssembly NuGet package to the Blazor WASM application project (not the class library) — the package auto-configures WasmBuildNative and NativeFileReference",
      "Use SkiaSharp.Views.Blazor in the application project, which auto-includes SkiaSharp.NativeAssets.WebAssembly as a transitive dependency",
      "Manually add both <WasmBuildNative>true</WasmBuildNative> and a NativeFileReference pointing to the .a files in the application .csproj (only necessary if already referencing SkiaSharp.NativeAssets.WebAssembly and the auto-include is not triggering)"
    ],
    "nextQuestions": [
      "Does the reporter's Blazor WASM test project reference SkiaSharp.NativeAssets.WebAssembly directly in its .csproj?",
      "Is the test project using the Blazor WebAssembly Standalone SDK (Microsoft.NET.Sdk.BlazorWebAssembly) or a Blazor Web App with server-side render mode?",
      "Can the reporter confirm whether adding SkiaSharp.NativeAssets.WebAssembly to the application project resolves the error?"
    ],
    "resolution": {
      "hypothesis": "The Blazor WASM application project is not referencing SkiaSharp.NativeAssets.WebAssembly, so the native static library is not linked into dotnet.wasm at build time. The DllNotFoundException is the WASM runtime's way of reporting a missing statically-linked symbol.",
      "proposals": [
        {
          "title": "Add SkiaSharp.NativeAssets.WebAssembly to application project",
          "description": "Add SkiaSharp.NativeAssets.WebAssembly to the Blazor WASM application project .csproj. This package auto-sets WasmBuildNative=true and adds the correct NativeFileReference for the detected .NET version (3.1.34 for net8.0, 3.1.56 for net9.0+).",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.WebAssembly\" Version=\"3.119.0\" />",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Switch to SkiaSharp.Views.Blazor",
          "description": "Reference SkiaSharp.Views.Blazor in the application project. It auto-includes SkiaSharp.NativeAssets.WebAssembly and provides SKCanvasView for canvas rendering in Blazor WASM.",
          "category": "alternative",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.Views.Blazor\" Version=\"3.119.0\" />",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Investigate SDK condition gate in SkiaSharp.targets",
          "description": "The UsingMicrosoftNETSdkBlazorWebAssembly condition in SkiaSharp.targets may not be set in all Blazor Web App configurations (e.g., projects using Microsoft.NET.Sdk.Web with WASM interactivity). Investigate whether the targets file needs an additional condition to cover the Blazor Web App hosted model.",
          "category": "investigation",
          "confidence": 0.65,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add SkiaSharp.NativeAssets.WebAssembly to application project",
      "recommendedReason": "Simplest one-line fix in the application project. The package auto-handles WasmBuildNative and the correct Emscripten library version for all supported .NET versions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "The most likely cause is missing SkiaSharp.NativeAssets.WebAssembly in the application project, but this needs confirmation via the .csproj. The official demo works on .NET 10, strongly suggesting configuration rather than a packaging bug.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Full .csproj content of the Blazor WASM application project (the one that runs in the browser)",
      "Whether SkiaSharp.NativeAssets.WebAssembly is referenced in the application project",
      "Whether the project SDK is Microsoft.NET.Sdk.BlazorWebAssembly or Microsoft.NET.Sdk.Web"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WASM, Views.Blazor, reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for project file and provide workaround with static linking explanation",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for reporting this. The `DllNotFoundException: libSkiaSharp` on Blazor WebAssembly is a **static linking** issue — unlike other platforms, WASM does not use P/Invoke dynamic loading. The native `.a` library must be statically linked into `dotnet.wasm` at build time.\n\nThe most common cause is that `SkiaSharp.NativeAssets.WebAssembly` is not referenced in your Blazor WASM **application** project. It can't come through a class library reference alone — it must be in the executable project's `.csproj`.\n\n**Quick fix:** Add to your Blazor WASM app project's `.csproj`:\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.WebAssembly\" Version=\"3.119.0\" />\n```\nThis package automatically sets `<WasmBuildNative>true</WasmBuildNative>` and links the correct `.a` file for your .NET version.\n\nAlternatively, if you'd prefer to use Blazor canvas controls, `SkiaSharp.Views.Blazor` already includes this automatically.\n\nCould you share the `.csproj` of your Blazor WASM **application** project (the one that runs in the browser, not the `BlazorCaptcha` class library)? That will help confirm whether this is a configuration issue or a packaging bug in SkiaSharp."
      },
      {
        "type": "link-related",
        "description": "Link to related open issue #3422 covering same error on .NET 10",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3422
      }
    ]
  }
}
```

</details>
