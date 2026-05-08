# Issue Triage Report — #1194

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:21:58Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Blazor (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to support SkiaSharp as a 'Blazor Extension' — enabling server-side Blazor (Blazor Server) to marshal SkiaSharp drawing calls over SignalR to the browser client, alongside the already-delivered Blazor WASM client-side support.

**Analysis:** The WASM (client-side Blazor) part of this request was delivered in SkiaSharp v2.88.0 via issue #1219. The remaining request — a 'Blazor Extension' model where server-side Blazor marshals SkiaSharp calls over SignalR to the browser — has not been implemented. Current SKCanvasView and SKGLView are annotated [SupportedOSPlatform("browser")] and require JS interop that only functions in WASM context. The maintainer explored both a server-renders-image approach and an embedded Uno iframe workaround, but neither is a first-class SkiaSharp feature.

**Recommendations:** **keep-open** — Valid long-standing feature request with strong community interest. Partially delivered (WASM client-side in v2.88.0) but the server-side Blazor rendering model is still unimplemented. Should be reassigned to an active milestone.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request, os/WASM, area/SkiaSharp.Views.Blazor, tenet/compatibility, triage/triaged |

## Evidence

### Reproduction

**Environment:** Blazor Server and Blazor WebAssembly, .NET 5+

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1219 — Related tracking issue for WASM support (closed/completed in v2.88.0)
- https://github.com/mono/SkiaSharp/issues/1132 — Related question: SkiaSharp WASM support (closed as duplicate of #1219)
- https://github.com/BlazorExtensions/Canvas — Reference implementation of a Blazor Extension for HTML5 Canvas (mentioned in issue)
- https://github.com/mattleibow/SkiaSharpBlazorComponents — Maintainer POC: SkiaSharp Blazor server-side image rendering component (unofficial/exploratory)
- https://github.com/mattleibow/SkiaSharpUnoBlazorApp — Maintainer POC: Uno iframe embedded in Blazor app workaround

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Blazor WASM client-side support was delivered in v2.88.0 (SKCanvasView/SKGLView), but Blazor Server (server-side rendering) remains unimplemented. The SkiaSharp.Views.Blazor package is still marked [SupportedOSPlatform("browser")] which means it does not work in Blazor Server hosting model. |

## Analysis

### Technical Summary

The WASM (client-side Blazor) part of this request was delivered in SkiaSharp v2.88.0 via issue #1219. The remaining request — a 'Blazor Extension' model where server-side Blazor marshals SkiaSharp calls over SignalR to the browser — has not been implemented. Current SKCanvasView and SKGLView are annotated [SupportedOSPlatform("browser")] and require JS interop that only functions in WASM context. The maintainer explored both a server-renders-image approach and an embedded Uno iframe workaround, but neither is a first-class SkiaSharp feature.

### Rationale

This is a feature request — new functionality that adds server-side Blazor rendering capability. The WASM portion was delivered; the server-side marshaling portion is the outstanding scope. The issue has strong community interest (14 +1, 5 ❤️, 48 comments) and the maintainer actively engaged with exploration. Labels already correctly reflect type/feature-request, area/SkiaSharp.Views.Blazor, os/WASM.

### Key Signals

- "once SkiaSharp can run in Client Blazor, please support SkiaSharp as a Blazor Extension" — **issue body** (Two distinct requests: WASM client support (done) and server-side Blazor marshaling (not done).)
- "I have been hacking away on this, and I think I may be able to do something for Blazor server-side" — **comment by @mattleibow** (Maintainer explored server-side rendering POC but it was unofficial/experimental — not shipped as a SkiaSharp feature.)
- "We really need the ability to link native libraries with Blazor. Like reeeeeeaaaaly need it." — **comment by @mattleibow** (Fundamental technical blocker was the inability to dynamically link native libs in Blazor. This was resolved with WASM AOT in .NET 6+ but the server-side marshaling model remains a separate architectural challenge.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 12-13 | direct | [SupportedOSPlatform("browser")] attribute and JS interop (SKHtmlCanvasInterop, SizeWatcherInterop, DpiWatcherInterop) confirm the component is WASM-only. Cannot run in Blazor Server hosting model. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | 12-13 | direct | [SupportedOSPlatform("browser")] also on SKGLView — same WASM-only constraint. The OpenGL/WebGL initialization via jsGLInfo requires a browser context unavailable in Blazor Server. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj` | 27-28 | direct | <SupportedPlatform Include="browser" /> confirms the package is scoped to browser platform only. No server-side rendering path exists. |

### Workarounds

- Blazor Server: Use SkiaSharp on the server to render to a byte array (SKBitmap/SKSurface), encode as PNG/JPEG, then serve as a data URL in an <img> tag — works today without the Blazor Extension model.
- Blazor Server: Use the maintainer's exploratory SkiaSharpBlazorComponents POC (https://github.com/mattleibow/SkiaSharpBlazorComponents) as a reference for the server-side image approach.
- Hybrid approach: Embed a Blazor WASM (or Uno) app in an iframe within the Blazor Server app and communicate via JS messaging.

### Next Questions

- Is a full call-marshaling Blazor Extension model still the desired direction, or is server-side image rendering sufficient for modern Blazor (given .NET 8 Static SSR and streaming rendering)?
- Should this be split into separate issues: (a) server-side Blazor image rendering component and (b) call-marshaling extension model?
- Has .NET 8 Blazor's new rendering modes (Interactive Server, Interactive WebAssembly, Interactive Auto) changed the feasibility or approach for this feature?

### Resolution Proposals

**Hypothesis:** The server-side Blazor scenario can be addressed in two phases: (1) a server-compatible SKCanvasView that renders server-side and sends images via SignalR (achievable now), and (2) a full call-marshaling 'Extension' model (architecturally complex and may not be necessary given modern Blazor modes).

1. **Server-side image rendering component** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Add a Blazor Server variant of SKCanvasView that renders to an SKBitmap on the server and sends it as a data URL or binary blob to the client via Blazor's normal update mechanism. Similar to the maintainer's POC at mattleibow/SkiaSharpBlazorComponents.
2. **Use server-side SkiaSharp with manual image streaming** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - As a workaround today: use SkiaSharp on the server (it works fine), render to SKBitmap, encode to PNG via SKImage.Encode(), convert to base64 data URL, and bind to an <img> src attribute in Blazor Server. No additional packages needed.

**Recommended proposal:** Use server-side SkiaSharp with manual image streaming

**Why:** Immediately actionable without waiting for a new feature. SkiaSharp core works on server today; only the interactive Canvas view is WASM-specific.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid long-standing feature request with strong community interest. Partially delivered (WASM client-side in v2.88.0) but the server-side Blazor rendering model is still unimplemented. Should be reassigned to an active milestone. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm correct labels: type/feature-request, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility | labels=type/feature-request, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge partial delivery and clarify remaining scope | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for this feature request! To clarify the current state:

**What's been delivered:** Blazor WebAssembly (client-side) support landed in SkiaSharp v2.88.0 — `SKCanvasView` and `SKGLView` in `SkiaSharp.Views.Blazor` run fully in the browser.

**What's still open:** Server-side Blazor (Blazor Server) support — the 'Blazor Extension' model where SkiaSharp runs on the server and marshals rendering to the client — has not been implemented.

**Workaround available today:** SkiaSharp core works perfectly in Blazor Server for server-side rendering. You can render to an `SKBitmap` or `SKSurface` on the server, encode as PNG, and serve as a data URL in an `<img>` tag:

```csharp
using var surface = SKSurface.Create(new SKImageInfo(800, 600));
var canvas = surface.Canvas;
// ... your drawing code ...
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
var base64 = Convert.ToBase64String(data.ToArray());
var src = $"data:image/png;base64,{base64}";
// bind 'src' to <img src="@src" />
```

This tracks the remaining work to add a first-class server-side component.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1194,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:21:58Z",
    "currentLabels": [
      "type/feature-request",
      "os/WASM",
      "area/SkiaSharp.Views.Blazor",
      "tenet/compatibility",
      "triage/triaged"
    ]
  },
  "summary": "Feature request to support SkiaSharp as a 'Blazor Extension' — enabling server-side Blazor (Blazor Server) to marshal SkiaSharp drawing calls over SignalR to the browser client, alongside the already-delivered Blazor WASM client-side support.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.95
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Blazor Server and Blazor WebAssembly, .NET 5+",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1219",
          "description": "Related tracking issue for WASM support (closed/completed in v2.88.0)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1132",
          "description": "Related question: SkiaSharp WASM support (closed as duplicate of #1219)"
        },
        {
          "url": "https://github.com/BlazorExtensions/Canvas",
          "description": "Reference implementation of a Blazor Extension for HTML5 Canvas (mentioned in issue)"
        },
        {
          "url": "https://github.com/mattleibow/SkiaSharpBlazorComponents",
          "description": "Maintainer POC: SkiaSharp Blazor server-side image rendering component (unofficial/exploratory)"
        },
        {
          "url": "https://github.com/mattleibow/SkiaSharpUnoBlazorApp",
          "description": "Maintainer POC: Uno iframe embedded in Blazor app workaround"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Blazor WASM client-side support was delivered in v2.88.0 (SKCanvasView/SKGLView), but Blazor Server (server-side rendering) remains unimplemented. The SkiaSharp.Views.Blazor package is still marked [SupportedOSPlatform(\"browser\")] which means it does not work in Blazor Server hosting model."
    }
  },
  "analysis": {
    "summary": "The WASM (client-side Blazor) part of this request was delivered in SkiaSharp v2.88.0 via issue #1219. The remaining request — a 'Blazor Extension' model where server-side Blazor marshals SkiaSharp calls over SignalR to the browser — has not been implemented. Current SKCanvasView and SKGLView are annotated [SupportedOSPlatform(\"browser\")] and require JS interop that only functions in WASM context. The maintainer explored both a server-renders-image approach and an embedded Uno iframe workaround, but neither is a first-class SkiaSharp feature.",
    "rationale": "This is a feature request — new functionality that adds server-side Blazor rendering capability. The WASM portion was delivered; the server-side marshaling portion is the outstanding scope. The issue has strong community interest (14 +1, 5 ❤️, 48 comments) and the maintainer actively engaged with exploration. Labels already correctly reflect type/feature-request, area/SkiaSharp.Views.Blazor, os/WASM.",
    "keySignals": [
      {
        "text": "once SkiaSharp can run in Client Blazor, please support SkiaSharp as a Blazor Extension",
        "source": "issue body",
        "interpretation": "Two distinct requests: WASM client support (done) and server-side Blazor marshaling (not done)."
      },
      {
        "text": "I have been hacking away on this, and I think I may be able to do something for Blazor server-side",
        "source": "comment by @mattleibow",
        "interpretation": "Maintainer explored server-side rendering POC but it was unofficial/experimental — not shipped as a SkiaSharp feature."
      },
      {
        "text": "We really need the ability to link native libraries with Blazor. Like reeeeeeaaaaly need it.",
        "source": "comment by @mattleibow",
        "interpretation": "Fundamental technical blocker was the inability to dynamically link native libs in Blazor. This was resolved with WASM AOT in .NET 6+ but the server-side marshaling model remains a separate architectural challenge."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "12-13",
        "finding": "[SupportedOSPlatform(\"browser\")] attribute and JS interop (SKHtmlCanvasInterop, SizeWatcherInterop, DpiWatcherInterop) confirm the component is WASM-only. Cannot run in Blazor Server hosting model.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "lines": "12-13",
        "finding": "[SupportedOSPlatform(\"browser\")] also on SKGLView — same WASM-only constraint. The OpenGL/WebGL initialization via jsGLInfo requires a browser context unavailable in Blazor Server.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SkiaSharp.Views.Blazor.csproj",
        "lines": "27-28",
        "finding": "<SupportedPlatform Include=\"browser\" /> confirms the package is scoped to browser platform only. No server-side rendering path exists.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Blazor Server: Use SkiaSharp on the server to render to a byte array (SKBitmap/SKSurface), encode as PNG/JPEG, then serve as a data URL in an <img> tag — works today without the Blazor Extension model.",
      "Blazor Server: Use the maintainer's exploratory SkiaSharpBlazorComponents POC (https://github.com/mattleibow/SkiaSharpBlazorComponents) as a reference for the server-side image approach.",
      "Hybrid approach: Embed a Blazor WASM (or Uno) app in an iframe within the Blazor Server app and communicate via JS messaging."
    ],
    "nextQuestions": [
      "Is a full call-marshaling Blazor Extension model still the desired direction, or is server-side image rendering sufficient for modern Blazor (given .NET 8 Static SSR and streaming rendering)?",
      "Should this be split into separate issues: (a) server-side Blazor image rendering component and (b) call-marshaling extension model?",
      "Has .NET 8 Blazor's new rendering modes (Interactive Server, Interactive WebAssembly, Interactive Auto) changed the feasibility or approach for this feature?"
    ],
    "resolution": {
      "hypothesis": "The server-side Blazor scenario can be addressed in two phases: (1) a server-compatible SKCanvasView that renders server-side and sends images via SignalR (achievable now), and (2) a full call-marshaling 'Extension' model (architecturally complex and may not be necessary given modern Blazor modes).",
      "proposals": [
        {
          "title": "Server-side image rendering component",
          "description": "Add a Blazor Server variant of SKCanvasView that renders to an SKBitmap on the server and sends it as a data URL or binary blob to the client via Blazor's normal update mechanism. Similar to the maintainer's POC at mattleibow/SkiaSharpBlazorComponents.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Use server-side SkiaSharp with manual image streaming",
          "description": "As a workaround today: use SkiaSharp on the server (it works fine), render to SKBitmap, encode to PNG via SKImage.Encode(), convert to base64 data URL, and bind to an <img> src attribute in Blazor Server. No additional packages needed.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use server-side SkiaSharp with manual image streaming",
      "recommendedReason": "Immediately actionable without waiting for a new feature. SkiaSharp core works on server today; only the interactive Canvas view is WASM-specific."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid long-standing feature request with strong community interest. Partially delivered (WASM client-side in v2.88.0) but the server-side Blazor rendering model is still unimplemented. Should be reassigned to an active milestone.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm correct labels: type/feature-request, area/SkiaSharp.Views.Blazor, os/WASM, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge partial delivery and clarify remaining scope",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for this feature request! To clarify the current state:\n\n**What's been delivered:** Blazor WebAssembly (client-side) support landed in SkiaSharp v2.88.0 — `SKCanvasView` and `SKGLView` in `SkiaSharp.Views.Blazor` run fully in the browser.\n\n**What's still open:** Server-side Blazor (Blazor Server) support — the 'Blazor Extension' model where SkiaSharp runs on the server and marshals rendering to the client — has not been implemented.\n\n**Workaround available today:** SkiaSharp core works perfectly in Blazor Server for server-side rendering. You can render to an `SKBitmap` or `SKSurface` on the server, encode as PNG, and serve as a data URL in an `<img>` tag:\n\n```csharp\nusing var surface = SKSurface.Create(new SKImageInfo(800, 600));\nvar canvas = surface.Canvas;\n// ... your drawing code ...\nusing var image = surface.Snapshot();\nusing var data = image.Encode(SKEncodedImageFormat.Png, 100);\nvar base64 = Convert.ToBase64String(data.ToArray());\nvar src = $\"data:image/png;base64,{base64}\";\n// bind 'src' to <img src=\"@src\" />\n```\n\nThis tracks the remaining work to add a first-class server-side component."
      }
    ]
  }
}
```

</details>
