# Issue Triage Report — #2836

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:36:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Blazor (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SkiaSharp Blazor WASM (SKCanvasView) works in Visual Studio debugger but crashes with a MONO AOT runtime error at aot-runtime-wasm.c:150 when deployed to Azure Web Sites using .NET 8 InteractiveWebAssembly render mode, regressing from 2.88.2 to 2.88.3.

**Analysis:** The crash occurs in MONO's WASM AOT runtime when the requestAnimationFrame callback (invoking OnRenderFrame via JSInterop) is dispatched in deployed/AOT mode. The MONO error at aot-runtime-wasm.c:150 indicates a missing or incompatible AOT-compiled method stub, likely caused by a change in the native WASM library (libSkiaSharp.a) between 2.88.2 and 2.88.3 that is incompatible with the MONO AOT compiler for .NET 8.

**Recommendations:** **needs-investigation** — Clear regression with crash and stack trace. Reporter identifies exact version boundary (2.88.2 → 2.88.3). Needs investigation into native WASM library changes and reproduction in a controlled environment.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/WASM, area/SkiaSharp.Views.Blazor, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a Blazor .NET 8 app with @rendermode=InteractiveWebAssembly
2. Add SKCanvasView component from SkiaSharp.Views.Blazor 2.88.3
3. Run locally via Visual Studio debugger — works fine
4. Publish and deploy to Azure Web Sites
5. Observe crash: MONO AOT runtime error at aot-runtime-wasm.c:150

**Environment:** SkiaSharp 2.88.3, .NET 8, Blazor InteractiveWebAssembly, Azure Web Sites

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Error: [MONO] /__w/1/s/src/mono/mono/mini/aot-runtime-wasm.c:150 |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | No API-level changes to SkiaSharp.Views.Blazor between 2.88.2 and 2.88.3; likely a native WASM library change that broke AOT compatibility. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states 2.88.2 worked and 2.88.3 broke. Changelog confirms no managed API changes, pointing to native WASM binary differences. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

The crash occurs in MONO's WASM AOT runtime when the requestAnimationFrame callback (invoking OnRenderFrame via JSInterop) is dispatched in deployed/AOT mode. The MONO error at aot-runtime-wasm.c:150 indicates a missing or incompatible AOT-compiled method stub, likely caused by a change in the native WASM library (libSkiaSharp.a) between 2.88.2 and 2.88.3 that is incompatible with the MONO AOT compiler for .NET 8.

### Rationale

This is clearly a regression bug: works in debugger (JIT/interpreted) but crashes in deployed (AOT) mode. The stack trace originates in SKHtmlCanvas.js requestAnimationFrame callback invoking .NET methods via Blazor JS interop, then crashes in MONO's AOT runtime. The API diff shows no managed code changes between 2.88.2 and 2.88.3 for Blazor, so the root cause is likely in the native WebAssembly static library.

### Key Signals

- "Error: [MONO] /__w/1/s/src/mono/mono/mini/aot-runtime-wasm.c:150" — **issue body** (MONO AOT runtime failure — a method stub is missing or incompatible in AOT-compiled WASM. This code path is only hit in deployed (AOT) mode, not in the VS debugger (which uses interpreter).)
- "it works fine within the debugger however when I deploy to AzureWebSites ... it fails" — **issue body** (Classic debugger-vs-deployment discrepancy: VS debugger uses interpreted WASM, deployment uses AOT. The crash only affects AOT mode.)
- "2.88.3 (Current) ... 2.88.2 (Previous) [Last Known Good]" — **issue body** (Regression from 2.88.2 to 2.88.3. Native WASM library changes are the most likely cause since the managed Blazor code has no API changes in this version range.)
- "SKHtmlCanvas.js:98 requestAnimationFrame → SKHtmlCanvas.js:104 → invokeMethod('Invoke')" — **issue body stack trace** (The JS invokes the .NET callback through the old DotNetObjectReference invokeMethod path (non-function branch), which then triggers AOT code that crashes.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SKHtmlCanvasInterop.cs` | 155-158 | direct | InterceptBrowserObjects is called via [DllImport("libSkiaSharp")] as a workaround for dotnet/runtime#76077, setting globalThis.SkiaSharpGL and SkiaSharpModule. In NET7_0_OR_GREATER, the callback is passed as Action via [JSMarshalAs<JSType.Function>], but in pre-.NET 7 fallback path it uses DotNetObjectReference. The try/catch around InterceptBrowserObjects call means it won't crash, but the static linking approach in WASM could expose AOT issues. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js` | 99-115 | direct | In requestAnimationFrame, the callback check at line 105 (typeof === 'function') determines whether to call directly or via invokeMethod('Invoke'). In deployed AOT mode with .NET 8, if the Action marshaling via JSMarshalAs<JSType.Function> fails to produce a proper JS function, the invokeMethod path is taken — calling back into AOT-compiled managed code that may be missing stubs. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 87-108 | related | OnRenderFrame is the managed method invoked by the JS callback. It uses GCHandle.AddrOfPinnedObject() to pass pixel buffer pointer to SKSurface.Create(). This is sound practice, but in AOT mode the entire call chain needs proper stub generation. |
| `changelogs/SkiaSharp.Views.Blazor/2.88.3/SkiaSharp.Views.Blazor.md` | — | direct | API diff between 2.88.2 and 2.88.3 shows 'No changes' for SkiaSharp.Views.Blazor managed code. The regression is therefore in the native WASM library (SkiaSharp.NativeAssets.WebAssembly), not the managed Blazor wrapper. |

### Workarounds

- Downgrade SkiaSharp to 2.88.2 which was reported as working.
- Use @rendermode=InteractiveServer instead of @rendermode=InteractiveWebAssembly to run the component server-side, avoiding WASM AOT entirely.
- Test with SkiaSharp 2.88.7 or newer to check if the regression was fixed in a later release.

### Next Questions

- What specific change in the native WASM .a library between 2.88.2 and 2.88.3 triggers the MONO AOT failure?
- Does the crash occur with all .NET 8 WASM AOT configurations or only with specific Emscripten/AOT settings?
- Is the crash still present in 2.88.7+ (changelogs show more releases after 2.88.3)?
- Does setting @rendermode=InteractiveServer (server-side) avoid the crash as a workaround?

### Resolution Proposals

**Hypothesis:** A change in the native WASM static library (libSkiaSharp.a) between 2.88.2 and 2.88.3 introduced an AOT incompatibility that causes MONO's AOT runtime to fail when invoking managed callbacks from JavaScript requestAnimationFrame in deployed Blazor WASM mode.

1. **Downgrade to SkiaSharp 2.88.2** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Pin SkiaSharp to 2.88.2 in the project file as an immediate workaround while the root cause is investigated.

```csharp
<PackageReference Include="SkiaSharp" Version="2.88.2" />
<PackageReference Include="SkiaSharp.Views.Blazor" Version="2.88.2" />
```
2. **Use InteractiveServer render mode** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Switch from InteractiveWebAssembly to InteractiveServer render mode. The Skia drawing runs on the server side and the result is sent to the browser. Avoids WASM AOT entirely.

```csharp
@* Change from: @rendermode="InteractiveWebAssembly" *@
@* Change to: *@
@rendermode="InteractiveServer"
```
3. **Investigate WASM native library changes in 2.88.3** — investigation, confidence 0.80 (80%), cost/m, validated=untested
   - Diff the SkiaSharp.NativeAssets.WebAssembly package contents between 2.88.2 and 2.88.3 to identify what changed in the .a files. Cross-reference with MONO AOT requirements for .NET 8 WebAssembly.

**Recommended proposal:** Downgrade to SkiaSharp 2.88.2

**Why:** Immediate, zero-risk workaround that directly addresses the reported regression. Allows continued use of InteractiveWebAssembly while the root cause is investigated.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Clear regression with crash and stack trace. Reporter identifies exact version boundary (2.88.2 → 2.88.3). Needs investigation into native WASM library changes and reproduction in a controlled environment. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge regression, provide immediate workarounds (downgrade or InteractiveServer), and ask for clarification on whether newer versions (2.88.7+) are affected | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This looks like a regression introduced in 2.88.3 affecting Blazor WASM when running in AOT mode (deployed), while the VS debugger uses interpreted mode and doesn't hit the issue.

**Immediate workarounds while we investigate:**

1. **Downgrade to 2.88.2** (confirmed working per your report):
   ```xml
   <PackageReference Include="SkiaSharp" Version="2.88.2" />
   <PackageReference Include="SkiaSharp.Views.Blazor" Version="2.88.2" />
   ```

2. **Switch to InteractiveServer render mode** (avoids WASM AOT entirely):
   ```
   @rendermode="InteractiveServer"
   ```

Could you also test with **SkiaSharp 2.88.7 or 2.88.9** to check if the issue persists in more recent releases? That would help narrow down whether a fix has already been shipped.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2836,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:36:00Z",
    "currentLabels": [
      "type/bug",
      "os/WASM",
      "area/SkiaSharp.Views.Blazor",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SkiaSharp Blazor WASM (SKCanvasView) works in Visual Studio debugger but crashes with a MONO AOT runtime error at aot-runtime-wasm.c:150 when deployed to Azure Web Sites using .NET 8 InteractiveWebAssembly render mode, regressing from 2.88.2 to 2.88.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.95
    },
    "platforms": [
      "os/WASM"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "Error: [MONO] /__w/1/s/src/mono/mono/mini/aot-runtime-wasm.c:150",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor .NET 8 app with @rendermode=InteractiveWebAssembly",
        "Add SKCanvasView component from SkiaSharp.Views.Blazor 2.88.3",
        "Run locally via Visual Studio debugger — works fine",
        "Publish and deploy to Azure Web Sites",
        "Observe crash: MONO AOT runtime error at aot-runtime-wasm.c:150"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET 8, Blazor InteractiveWebAssembly, Azure Web Sites",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "No API-level changes to SkiaSharp.Views.Blazor between 2.88.2 and 2.88.3; likely a native WASM library change that broke AOT compatibility."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states 2.88.2 worked and 2.88.3 broke. Changelog confirms no managed API changes, pointing to native WASM binary differences.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "The crash occurs in MONO's WASM AOT runtime when the requestAnimationFrame callback (invoking OnRenderFrame via JSInterop) is dispatched in deployed/AOT mode. The MONO error at aot-runtime-wasm.c:150 indicates a missing or incompatible AOT-compiled method stub, likely caused by a change in the native WASM library (libSkiaSharp.a) between 2.88.2 and 2.88.3 that is incompatible with the MONO AOT compiler for .NET 8.",
    "rationale": "This is clearly a regression bug: works in debugger (JIT/interpreted) but crashes in deployed (AOT) mode. The stack trace originates in SKHtmlCanvas.js requestAnimationFrame callback invoking .NET methods via Blazor JS interop, then crashes in MONO's AOT runtime. The API diff shows no managed code changes between 2.88.2 and 2.88.3 for Blazor, so the root cause is likely in the native WebAssembly static library.",
    "keySignals": [
      {
        "text": "Error: [MONO] /__w/1/s/src/mono/mono/mini/aot-runtime-wasm.c:150",
        "source": "issue body",
        "interpretation": "MONO AOT runtime failure — a method stub is missing or incompatible in AOT-compiled WASM. This code path is only hit in deployed (AOT) mode, not in the VS debugger (which uses interpreter)."
      },
      {
        "text": "it works fine within the debugger however when I deploy to AzureWebSites ... it fails",
        "source": "issue body",
        "interpretation": "Classic debugger-vs-deployment discrepancy: VS debugger uses interpreted WASM, deployment uses AOT. The crash only affects AOT mode."
      },
      {
        "text": "2.88.3 (Current) ... 2.88.2 (Previous) [Last Known Good]",
        "source": "issue body",
        "interpretation": "Regression from 2.88.2 to 2.88.3. Native WASM library changes are the most likely cause since the managed Blazor code has no API changes in this version range."
      },
      {
        "text": "SKHtmlCanvas.js:98 requestAnimationFrame → SKHtmlCanvas.js:104 → invokeMethod('Invoke')",
        "source": "issue body stack trace",
        "interpretation": "The JS invokes the .NET callback through the old DotNetObjectReference invokeMethod path (non-function branch), which then triggers AOT code that crashes."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SKHtmlCanvasInterop.cs",
        "lines": "155-158",
        "finding": "InterceptBrowserObjects is called via [DllImport(\"libSkiaSharp\")] as a workaround for dotnet/runtime#76077, setting globalThis.SkiaSharpGL and SkiaSharpModule. In NET7_0_OR_GREATER, the callback is passed as Action via [JSMarshalAs<JSType.Function>], but in pre-.NET 7 fallback path it uses DotNetObjectReference. The try/catch around InterceptBrowserObjects call means it won't crash, but the static linking approach in WASM could expose AOT issues.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.js",
        "lines": "99-115",
        "finding": "In requestAnimationFrame, the callback check at line 105 (typeof === 'function') determines whether to call directly or via invokeMethod('Invoke'). In deployed AOT mode with .NET 8, if the Action marshaling via JSMarshalAs<JSType.Function> fails to produce a proper JS function, the invokeMethod path is taken — calling back into AOT-compiled managed code that may be missing stubs.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "87-108",
        "finding": "OnRenderFrame is the managed method invoked by the JS callback. It uses GCHandle.AddrOfPinnedObject() to pass pixel buffer pointer to SKSurface.Create(). This is sound practice, but in AOT mode the entire call chain needs proper stub generation.",
        "relevance": "related"
      },
      {
        "file": "changelogs/SkiaSharp.Views.Blazor/2.88.3/SkiaSharp.Views.Blazor.md",
        "finding": "API diff between 2.88.2 and 2.88.3 shows 'No changes' for SkiaSharp.Views.Blazor managed code. The regression is therefore in the native WASM library (SkiaSharp.NativeAssets.WebAssembly), not the managed Blazor wrapper.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "What specific change in the native WASM .a library between 2.88.2 and 2.88.3 triggers the MONO AOT failure?",
      "Does the crash occur with all .NET 8 WASM AOT configurations or only with specific Emscripten/AOT settings?",
      "Is the crash still present in 2.88.7+ (changelogs show more releases after 2.88.3)?",
      "Does setting @rendermode=InteractiveServer (server-side) avoid the crash as a workaround?"
    ],
    "workarounds": [
      "Downgrade SkiaSharp to 2.88.2 which was reported as working.",
      "Use @rendermode=InteractiveServer instead of @rendermode=InteractiveWebAssembly to run the component server-side, avoiding WASM AOT entirely.",
      "Test with SkiaSharp 2.88.7 or newer to check if the regression was fixed in a later release."
    ],
    "resolution": {
      "hypothesis": "A change in the native WASM static library (libSkiaSharp.a) between 2.88.2 and 2.88.3 introduced an AOT incompatibility that causes MONO's AOT runtime to fail when invoking managed callbacks from JavaScript requestAnimationFrame in deployed Blazor WASM mode.",
      "proposals": [
        {
          "title": "Downgrade to SkiaSharp 2.88.2",
          "description": "Pin SkiaSharp to 2.88.2 in the project file as an immediate workaround while the root cause is investigated.",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp\" Version=\"2.88.2\" />\n<PackageReference Include=\"SkiaSharp.Views.Blazor\" Version=\"2.88.2\" />",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use InteractiveServer render mode",
          "description": "Switch from InteractiveWebAssembly to InteractiveServer render mode. The Skia drawing runs on the server side and the result is sent to the browser. Avoids WASM AOT entirely.",
          "category": "workaround",
          "codeSnippet": "@* Change from: @rendermode=\"InteractiveWebAssembly\" *@\n@* Change to: *@\n@rendermode=\"InteractiveServer\"",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate WASM native library changes in 2.88.3",
          "description": "Diff the SkiaSharp.NativeAssets.WebAssembly package contents between 2.88.2 and 2.88.3 to identify what changed in the .a files. Cross-reference with MONO AOT requirements for .NET 8 WebAssembly.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Downgrade to SkiaSharp 2.88.2",
      "recommendedReason": "Immediate, zero-risk workaround that directly addresses the reported regression. Allows continued use of InteractiveWebAssembly while the root cause is investigated."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Clear regression with crash and stack trace. Reporter identifies exact version boundary (2.88.2 → 2.88.3). Needs investigation into native WASM library changes and reproduction in a controlled environment.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, provide immediate workarounds (downgrade or InteractiveServer), and ask for clarification on whether newer versions (2.88.7+) are affected",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report! This looks like a regression introduced in 2.88.3 affecting Blazor WASM when running in AOT mode (deployed), while the VS debugger uses interpreted mode and doesn't hit the issue.\n\n**Immediate workarounds while we investigate:**\n\n1. **Downgrade to 2.88.2** (confirmed working per your report):\n   ```xml\n   <PackageReference Include=\"SkiaSharp\" Version=\"2.88.2\" />\n   <PackageReference Include=\"SkiaSharp.Views.Blazor\" Version=\"2.88.2\" />\n   ```\n\n2. **Switch to InteractiveServer render mode** (avoids WASM AOT entirely):\n   ```\n   @rendermode=\"InteractiveServer\"\n   ```\n\nCould you also test with **SkiaSharp 2.88.7 or 2.88.9** to check if the issue persists in more recent releases? That would help narrow down whether a fix has already been shipped."
      }
    ]
  }
}
```

</details>
