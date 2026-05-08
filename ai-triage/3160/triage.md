# Issue Triage Report — #3160

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T22:27:00Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views.Blazor (0.88 (88%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** MAUI Blazor Hybrid app using SkiaSharp.Views.Blazor.SKCanvasView crashes on Windows and Android (Net9 MAUI+Web template) while the standalone web variant works fine, due to the Blazor view being browser-only.

**Analysis:** The crash is caused by SkiaSharp.Views.Blazor.SKCanvasView being exclusively a browser-platform component (marked [SupportedOSPlatform("browser")]) that uses JSHost.ImportAsync from System.Runtime.InteropServices.JavaScript — a browser-only .NET API. In a MAUI Blazor Hybrid app, Blazor components run on the native .NET runtime (not in the browser), so JSHost.ImportAsync throws PlatformNotSupportedException when the component initializes.

**Recommendations:** **needs-info** — Exception details only available as screenshots (unreadable); need plain-text exception to confirm PlatformNotSupportedException from JSHost.ImportAsync. Also need to confirm which SKCanvasView namespace/package the reporter is using.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/Windows-Classic, os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a new .NET 9 MAUI + Web (Blazor Hybrid) template project in Visual Studio
2. Add SkiaSharp NuGet packages including SkiaSharp.Views.Blazor
3. Add SKCanvasView component from SkiaSharp.Views.Blazor to a shared Razor component
4. Run the MAUI app target (Windows or Android)
5. App crashes; removing the SKCanvasView component prevents the crash

**Environment:** SkiaSharp 3.116.0, .NET 9, Visual Studio on Windows, tested on Windows and Android

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2648 — Related: SKCanvasView crashes in MAUI — was missing UseSkiaSharp() init (closed)
- https://github.com/mono/SkiaSharp/issues/3113 — Related: MAUI 9 Windows SKGLView ExecutionEngineException (closed)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Exception shown only in screenshot (text not provided); crash triggered by SKCanvasView in Razor component within MAUI Blazor Hybrid context |
| Repro quality | partial |
| Target frameworks | net9.0-windows, net9.0-android |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The JSModuleInterop.cs uses JSHost.ImportAsync (NET7_0_OR_GREATER) which is strictly browser-only; this code path is active in 3.116.0 on .NET 9. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.55 (55%) |
| Reason | Reporter claims 2.88.9 worked, but .NET 9 MAUI+Web Hybrid template is new in .NET 9. The regression may be due to JSHost.ImportAsync (NET7+) replacing the older IJSInProcessRuntime path that may have worked in MAUI Hybrid. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The crash is caused by SkiaSharp.Views.Blazor.SKCanvasView being exclusively a browser-platform component (marked [SupportedOSPlatform("browser")]) that uses JSHost.ImportAsync from System.Runtime.InteropServices.JavaScript — a browser-only .NET API. In a MAUI Blazor Hybrid app, Blazor components run on the native .NET runtime (not in the browser), so JSHost.ImportAsync throws PlatformNotSupportedException when the component initializes.

### Rationale

Code investigation confirms SkiaSharp.Views.Blazor declares <SupportedPlatform Include='browser' /> in its project file and uses [SupportedOSPlatform("browser")] on all internal classes. On .NET 7+, JSModuleInterop uses JSHost.ImportAsync which is strictly browser-only. The .NET 9 MAUI+Web template is new, explaining why 2.88.9 was cited as last-good; on NET6 paths, the older IJSInProcessRuntime code was used instead. The exception text is only available as a screenshot; confirming it is PlatformNotSupportedException would solidify the diagnosis.

### Key Signals

- "The MAUI App crashes (Windows and Android tested)" — **issue body** (Crash occurs specifically in native MAUI context, not in browser/web context — consistent with browser-only API being called on native platform.)
- "removing this [SKCanvasView Blazor component] — All turns ok" — **issue body** (The crash is directly caused by instantiating the Blazor SKCanvasView, confirming it is the component initialization (JSHost.ImportAsync) that fails.)
- "Last Known Good Version: 2.88.9" — **issue body** (On SkiaSharp 2.88.x the older IJSInProcessRuntime code path was used; the JSHost.ImportAsync (browser-only) path was introduced with NET7+ support. Also the .NET 9 MAUI+Web Hybrid template is new.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 12 | direct | [SupportedOSPlatform("browser")] applied to the entire SKCanvasView class — not intended for non-browser platforms |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs` | 22-24 | direct | On NET7_0_OR_GREATER, JSHost.ImportAsync is called (System.Runtime.InteropServices.JavaScript) — this is browser-only and throws PlatformNotSupportedException on MAUI native targets (Windows, Android) |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs` | 25-29 | related | Pre-NET7 fallback checks 'if (js is not IJSInProcessRuntime) throw NotSupportedException' — MAUI Hybrid IJSRuntime may implement IJSInProcessRuntime, possibly explaining why older SkiaSharp (.NET 6) partially worked |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | 27-29 | direct | <SupportedPlatform Include='browser' /> explicitly marks this package as browser-only; not intended for MAUI Hybrid use |

### Workarounds

- Do not use SkiaSharp.Views.Blazor.SKCanvasView in MAUI Blazor Hybrid — it is browser-only
- In MAUI Hybrid, use platform-specific rendering: add a native MAUI SKCanvasView overlay (SkiaSharp.Views.Maui) positioned over the BlazorWebView for graphics, keeping Blazor for UI layout
- Use runtime platform detection (#if directive or RuntimeInformation.IsOSPlatform) to conditionally skip or replace the SKCanvasView component when running in MAUI Hybrid context

### Next Questions

- What is the exact exception message and stack trace? (Screenshots not readable; plain text needed)
- Is the reporter using SkiaSharp.Views.Blazor.SKCanvasView or SkiaSharp.Views.Maui.Controls.SKCanvasView?
- Should SkiaSharp.Views.Blazor support MAUI Blazor Hybrid (requires alternate implementation without JSHost)?
- Was a MAUI Blazor Hybrid-compatible SKCanvasView ever planned or discussed?

### Resolution Proposals

**Hypothesis:** SkiaSharp.Views.Blazor.SKCanvasView is browser-only and cannot be used in MAUI Blazor Hybrid. Reporter needs to use SkiaSharp.Views.Maui for native rendering or restructure their app to avoid the browser-only component on native platforms.

1. **Use MAUI SKCanvasView for hybrid rendering** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - For MAUI Blazor Hybrid apps, use SkiaSharp.Views.Maui.Controls.SKCanvasView as a native MAUI view overlaid on the BlazorWebView. The native view handles SkiaSharp rendering while Blazor handles the UI shell.
2. **Add MAUI Hybrid support to SkiaSharp.Views.Blazor** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Create a Blazor Hybrid-compatible variant of SKCanvasView that avoids JSHost.ImportAsync and browser-specific JS interop, using the native platform's rendering APIs instead. Would need a separate implementation or conditional compilation for MAUI Hybrid.

**Recommended proposal:** Use MAUI SKCanvasView for hybrid rendering

**Why:** Immediate workaround available with existing APIs; a proper Blazor Hybrid fix is a larger feature request.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | Exception details only available as screenshots (unreadable); need plain-text exception to confirm PlatformNotSupportedException from JSHost.ImportAsync. Also need to confirm which SKCanvasView namespace/package the reporter is using. |
| Suggested repro platform | windows |

### Missing Info

- Exact exception message and stack trace (plain text, not screenshot)
- Which package/namespace is SKCanvasView coming from — SkiaSharp.Views.Blazor or SkiaSharp.Views.Maui?
- Minimal public reproduction project

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, Blazor views area, Windows + Android platforms, MAUI partner, compatibility tenet | labels=type/bug, area/SkiaSharp.Views.Blazor, os/Windows, os/Android, partner/maui, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Request exception text and package confirmation; provide workaround for MAUI Blazor Hybrid | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! Based on the code you shared, you appear to be using `SkiaSharp.Views.Blazor.SKCanvasView` (Blazor WASM component with `@ref`, `OnPaintSurface` parameter, Blazor events) in a MAUI Blazor Hybrid project.

`SkiaSharp.Views.Blazor` is browser-only — it uses `JSHost.ImportAsync` from `System.Runtime.InteropServices.JavaScript` which only works in Blazor WASM (browser), not in MAUI Blazor Hybrid where the .NET runtime is native (Windows/Android).

**To help us investigate further, please provide:**
1. The exact exception message and stack trace (as plain text, not a screenshot)
2. Confirmation of which NuGet packages you're referencing (`SkiaSharp.Views.Blazor` vs `SkiaSharp.Views.Maui.Controls`)

**Workaround (immediate):** `SkiaSharp.Views.Blazor.SKCanvasView` cannot be used in the MAUI Hybrid target. For the native MAUI side, use `SkiaSharp.Views.Maui.Controls.SKCanvasView` as a native MAUI view positioned over (or alongside) your `BlazorWebView`. Use platform-conditional code to switch between the Blazor component (for web) and the MAUI view (for native targets).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3160,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T22:27:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "MAUI Blazor Hybrid app using SkiaSharp.Views.Blazor.SKCanvasView crashes on Windows and Android (Net9 MAUI+Web template) while the standalone web variant works fine, due to the Blazor view being browser-only.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "Exception shown only in screenshot (text not provided); crash triggered by SKCanvasView in Razor component within MAUI Blazor Hybrid context",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-windows",
        "net9.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new .NET 9 MAUI + Web (Blazor Hybrid) template project in Visual Studio",
        "Add SkiaSharp NuGet packages including SkiaSharp.Views.Blazor",
        "Add SKCanvasView component from SkiaSharp.Views.Blazor to a shared Razor component",
        "Run the MAUI app target (Windows or Android)",
        "App crashes; removing the SKCanvasView component prevents the crash"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, .NET 9, Visual Studio on Windows, tested on Windows and Android",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2648",
          "description": "Related: SKCanvasView crashes in MAUI — was missing UseSkiaSharp() init (closed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3113",
          "description": "Related: MAUI 9 Windows SKGLView ExecutionEngineException (closed)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The JSModuleInterop.cs uses JSHost.ImportAsync (NET7_0_OR_GREATER) which is strictly browser-only; this code path is active in 3.116.0 on .NET 9."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.55,
      "reason": "Reporter claims 2.88.9 worked, but .NET 9 MAUI+Web Hybrid template is new in .NET 9. The regression may be due to JSHost.ImportAsync (NET7+) replacing the older IJSInProcessRuntime path that may have worked in MAUI Hybrid.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The crash is caused by SkiaSharp.Views.Blazor.SKCanvasView being exclusively a browser-platform component (marked [SupportedOSPlatform(\"browser\")]) that uses JSHost.ImportAsync from System.Runtime.InteropServices.JavaScript — a browser-only .NET API. In a MAUI Blazor Hybrid app, Blazor components run on the native .NET runtime (not in the browser), so JSHost.ImportAsync throws PlatformNotSupportedException when the component initializes.",
    "rationale": "Code investigation confirms SkiaSharp.Views.Blazor declares <SupportedPlatform Include='browser' /> in its project file and uses [SupportedOSPlatform(\"browser\")] on all internal classes. On .NET 7+, JSModuleInterop uses JSHost.ImportAsync which is strictly browser-only. The .NET 9 MAUI+Web template is new, explaining why 2.88.9 was cited as last-good; on NET6 paths, the older IJSInProcessRuntime code was used instead. The exception text is only available as a screenshot; confirming it is PlatformNotSupportedException would solidify the diagnosis.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "12",
        "finding": "[SupportedOSPlatform(\"browser\")] applied to the entire SKCanvasView class — not intended for non-browser platforms",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs",
        "lines": "22-24",
        "finding": "On NET7_0_OR_GREATER, JSHost.ImportAsync is called (System.Runtime.InteropServices.JavaScript) — this is browser-only and throws PlatformNotSupportedException on MAUI native targets (Windows, Android)",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs",
        "lines": "25-29",
        "finding": "Pre-NET7 fallback checks 'if (js is not IJSInProcessRuntime) throw NotSupportedException' — MAUI Hybrid IJSRuntime may implement IJSInProcessRuntime, possibly explaining why older SkiaSharp (.NET 6) partially worked",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "lines": "27-29",
        "finding": "<SupportedPlatform Include='browser' /> explicitly marks this package as browser-only; not intended for MAUI Hybrid use",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The MAUI App crashes (Windows and Android tested)",
        "source": "issue body",
        "interpretation": "Crash occurs specifically in native MAUI context, not in browser/web context — consistent with browser-only API being called on native platform."
      },
      {
        "text": "removing this [SKCanvasView Blazor component] — All turns ok",
        "source": "issue body",
        "interpretation": "The crash is directly caused by instantiating the Blazor SKCanvasView, confirming it is the component initialization (JSHost.ImportAsync) that fails."
      },
      {
        "text": "Last Known Good Version: 2.88.9",
        "source": "issue body",
        "interpretation": "On SkiaSharp 2.88.x the older IJSInProcessRuntime code path was used; the JSHost.ImportAsync (browser-only) path was introduced with NET7+ support. Also the .NET 9 MAUI+Web Hybrid template is new."
      }
    ],
    "workarounds": [
      "Do not use SkiaSharp.Views.Blazor.SKCanvasView in MAUI Blazor Hybrid — it is browser-only",
      "In MAUI Hybrid, use platform-specific rendering: add a native MAUI SKCanvasView overlay (SkiaSharp.Views.Maui) positioned over the BlazorWebView for graphics, keeping Blazor for UI layout",
      "Use runtime platform detection (#if directive or RuntimeInformation.IsOSPlatform) to conditionally skip or replace the SKCanvasView component when running in MAUI Hybrid context"
    ],
    "nextQuestions": [
      "What is the exact exception message and stack trace? (Screenshots not readable; plain text needed)",
      "Is the reporter using SkiaSharp.Views.Blazor.SKCanvasView or SkiaSharp.Views.Maui.Controls.SKCanvasView?",
      "Should SkiaSharp.Views.Blazor support MAUI Blazor Hybrid (requires alternate implementation without JSHost)?",
      "Was a MAUI Blazor Hybrid-compatible SKCanvasView ever planned or discussed?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp.Views.Blazor.SKCanvasView is browser-only and cannot be used in MAUI Blazor Hybrid. Reporter needs to use SkiaSharp.Views.Maui for native rendering or restructure their app to avoid the browser-only component on native platforms.",
      "proposals": [
        {
          "title": "Use MAUI SKCanvasView for hybrid rendering",
          "description": "For MAUI Blazor Hybrid apps, use SkiaSharp.Views.Maui.Controls.SKCanvasView as a native MAUI view overlaid on the BlazorWebView. The native view handles SkiaSharp rendering while Blazor handles the UI shell.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add MAUI Hybrid support to SkiaSharp.Views.Blazor",
          "description": "Create a Blazor Hybrid-compatible variant of SKCanvasView that avoids JSHost.ImportAsync and browser-specific JS interop, using the native platform's rendering APIs instead. Would need a separate implementation or conditional compilation for MAUI Hybrid.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use MAUI SKCanvasView for hybrid rendering",
      "recommendedReason": "Immediate workaround available with existing APIs; a proper Blazor Hybrid fix is a larger feature request."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "Exception details only available as screenshots (unreadable); need plain-text exception to confirm PlatformNotSupportedException from JSHost.ImportAsync. Also need to confirm which SKCanvasView namespace/package the reporter is using.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Exact exception message and stack trace (plain text, not screenshot)",
      "Which package/namespace is SKCanvasView coming from — SkiaSharp.Views.Blazor or SkiaSharp.Views.Maui?",
      "Minimal public reproduction project"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Blazor views area, Windows + Android platforms, MAUI partner, compatibility tenet",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/Windows",
          "os/Android",
          "partner/maui",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request exception text and package confirmation; provide workaround for MAUI Blazor Hybrid",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! Based on the code you shared, you appear to be using `SkiaSharp.Views.Blazor.SKCanvasView` (Blazor WASM component with `@ref`, `OnPaintSurface` parameter, Blazor events) in a MAUI Blazor Hybrid project.\n\n`SkiaSharp.Views.Blazor` is browser-only — it uses `JSHost.ImportAsync` from `System.Runtime.InteropServices.JavaScript` which only works in Blazor WASM (browser), not in MAUI Blazor Hybrid where the .NET runtime is native (Windows/Android).\n\n**To help us investigate further, please provide:**\n1. The exact exception message and stack trace (as plain text, not a screenshot)\n2. Confirmation of which NuGet packages you're referencing (`SkiaSharp.Views.Blazor` vs `SkiaSharp.Views.Maui.Controls`)\n\n**Workaround (immediate):** `SkiaSharp.Views.Blazor.SKCanvasView` cannot be used in the MAUI Hybrid target. For the native MAUI side, use `SkiaSharp.Views.Maui.Controls.SKCanvasView` as a native MAUI view positioned over (or alongside) your `BlazorWebView`. Use platform-conditional code to switch between the Blazor component (for web) and the MAUI view (for native targets)."
      }
    ]
  }
}
```

</details>
