# Issue Triage Report — #2104

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T18:02:20Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to create a new cross-platform GPU view for SkiaSharp.Views.Maui (and Views.Forms) that automatically selects the optimal GPU backend per platform — Metal on Apple, Vulkan on Android/Linux, Direct3D on Windows — instead of being hardcoded to OpenGL.

**Analysis:** The existing SKGLView is hardcoded to OpenGL ES on all platforms. On iOS 12+, OpenGL is deprecated in favour of Metal (reflected in an [ObsoletedOSPlatform] attribute on the iOS handler). SKMetalView already exists for native Apple views but is not integrated into any MAUI handler. A new view abstraction (e.g., SKGpuView) is needed that delegates to Metal on Apple, Vulkan on Android/Linux, and Direct3D on Windows while keeping a unified C# API.

**Recommendations:** **keep-open** — Valid feature request with significant community interest (14 reactions). Native building blocks exist; a MAUI handler abstraction layer is missing. Ongoing design discussion around backend enum and fallback strategy is needed before implementation can start.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | — |
| Backends | backend/Metal, backend/Vulkan, backend/OpenGL, backend/Direct3D |
| Tenets | tenet/compatibility |
| Partner | partner/maui |

## Evidence

### Reproduction

**Environment:** SkiaSharp.Views.Forms and SkiaSharp.Views.Maui, all platforms

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2747#issuecomment-1959771472 — Discussion of UseSkiaSharp(settings) API for backend selection at initialization

## Analysis

### Technical Summary

The existing SKGLView is hardcoded to OpenGL ES on all platforms. On iOS 12+, OpenGL is deprecated in favour of Metal (reflected in an [ObsoletedOSPlatform] attribute on the iOS handler). SKMetalView already exists for native Apple views but is not integrated into any MAUI handler. A new view abstraction (e.g., SKGpuView) is needed that delegates to Metal on Apple, Vulkan on Android/Linux, and Direct3D on Windows while keeping a unified C# API.

### Rationale

Clearly a feature request: the reporter explicitly states a new view type is needed, describes the desired solution, and identifies the only alternative as forking the library. The request is well-understood — the native building blocks (SKMetalView, SKGLTextureView, SKSwapChainPanel) already exist; the missing piece is a unified MAUI handler that selects the right backend per platform. Categorised as type/feature-request because it adds a new view type; area/SkiaSharp.Views.Maui because MAUI handlers are where the work lives. Ongoing design discussion (comments on backend enum, fallback strategy, runtime switching) indicates the feature requires design decisions before implementation.

### Key Signals

- "both SkiaSharp.Views.Forms and SkiaSharp.Views.Maui only support using OpenGL in the SKGLView since this was the only backend at the time" — **issue body** (Confirms the request is about lack of backend selection, not a regression or bug.)
- "we need a GPU view that swaps out the backend controls based on the preferred backend for each platform" — **issue body** (Clear feature scope: new view that auto-selects native GPU backend.)
- "[ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")]" — **SKGLViewHandler.iOS.cs line 13** (Urgency signal: OpenGL is already deprecated on Apple platforms and the codebase acknowledges it.)
- "We can let people choose the rendering backend at initialization with .UseSkiaSharp(optional settings) with enum of kind Vulkan, GL, ..., Auto" — **comment #issuecomment-1959949115** (Community has started design discussions around global backend configuration with fallback.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs` | 13-14 | direct | Handler carries [ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")] — OpenGL is deprecated on iOS 12+ and there is no MAUI Metal handler to replace it. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/ISKGLView.cs` | — | direct | Interface is hardcoded to GL semantics: GRContext property, OnPaintSurface(SKPaintGLSurfaceEventArgs). A backend-agnostic interface would need a different event args type or a union type. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs` | 11-17 | related | Android handler creates SKGLTextureView (OpenGL ES). No Vulkan path exists. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs` | 7 | related | Windows handler wraps SKSwapChainPanel (DXGI/D3D swap chain) — already uses a D3D-based native control, but still exposes SKPaintGLSurfaceEventArgs at the MAUI layer. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | — | direct | SKMetalView exists for native iOS/macOS/tvOS using MTKView and GRContext.CreateMetal(). It is fully functional but not exposed through any MAUI handler — the needed infrastructure already exists. |

### Workarounds

- Use the native SKMetalView directly (SkiaSharp.Views.iOS/Mac namespace) for iOS/macOS-specific projects that need Metal acceleration today.
- Continue using SKGLView — OpenGL ES still works on all current platforms despite being deprecated on iOS 12+.
- Create a custom renderer by forking the platform handler from the SkiaSharp repo (noted as the only alternative by the reporter).

### Next Questions

- Should the new view expose a backend-agnostic event args type, or reuse SKPaintGLSurfaceEventArgs with a different name?
- What is the preferred API for backend selection: per-view property, global UseSkiaSharp() initializer setting, or both?
- How should backend availability failures (e.g., no Vulkan driver) be surfaced — exception, event, or silent fallback to raster?

### Resolution Proposals

**Hypothesis:** A new view type (e.g., SKGpuView) with a unified MAUI handler that delegates to SKMetalView on Apple platforms, a Vulkan-backed view on Android/Linux where available, and the existing SKSwapChainPanel on Windows.

1. **Create SKGpuView with per-platform backend handlers** — fix, confidence 0.80 (80%), cost/xl, validated=untested
   - Define ISKGpuView interface with backend-agnostic event args and a GrBackend property. Implement platform handlers: Metal on iOS/macOS/tvOS/MacCatalyst (using existing SKMetalView infrastructure), Vulkan on Android (where available with GL fallback), and D3D/GL on Windows. Add SKGpuView MAUI control class.
2. **Add backend configuration to UseSkiaSharp initializer** — alternative, confidence 0.70 (70%), cost/l, validated=untested
   - Expose a GpuBackend enum in the UseSkiaSharp(settings) startup API (as discussed in comments) so devs can globally specify preferred backend and fallback. The existing SKGLViewHandler could then conditionally use Metal on Apple if configured.

**Recommended proposal:** Create SKGpuView with per-platform backend handlers

**Why:** Creates a clean new API without breaking existing SKGLView usage. The native building blocks (SKMetalView, SKGLTextureView, SKSwapChainPanel) already exist; the main work is writing the MAUI handler layer and a backend-agnostic interface.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid feature request with significant community interest (14 reactions). Native building blocks exist; a MAUI handler abstraction layer is missing. Ongoing design discussion around backend enum and fallback strategy is needed before implementation can start. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, MAUI views area, backend, tenet, and partner labels | labels=type/feature-request, area/SkiaSharp.Views.Maui, backend/Metal, backend/Vulkan, backend/OpenGL, backend/Direct3D, tenet/compatibility, partner/maui |
| add-comment | medium | 0.80 (80%) | Acknowledge design status and summarise open design questions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this — it's a well-understood gap. The native building blocks already exist (`SKMetalView` for Apple, `SKGLTextureView` for Android, `SKSwapChainPanel` for Windows), and the iOS handler already carries an `[ObsoletedOSPlatform("ios12.0", "Use 'Metal' instead.")]` attribute acknowledging the urgency.

Before implementation can start, a few design questions need answers:
1. **API shape** — should the new view expose a backend-agnostic event args type, or reuse an adapted `SKPaintGLSurfaceEventArgs`?
2. **Backend selection** — per-view property, global `UseSkiaSharp(settings)` initializer, or both?
3. **Failure handling** — if the preferred backend is unavailable (e.g., no Vulkan driver), should the view throw, fire an event, or silently fall back to raster?

Tracking this open until the design discussion converges. Community input welcome on the above.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2104,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T18:02:20Z"
  },
  "summary": "Feature request to create a new cross-platform GPU view for SkiaSharp.Views.Maui (and Views.Forms) that automatically selects the optimal GPU backend per platform — Metal on Apple, Vulkan on Android/Linux, Direct3D on Windows — instead of being hardcoded to OpenGL.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "backends": [
      "backend/Metal",
      "backend/Vulkan",
      "backend/OpenGL",
      "backend/Direct3D"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp.Views.Forms and SkiaSharp.Views.Maui, all platforms",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2747#issuecomment-1959771472",
          "description": "Discussion of UseSkiaSharp(settings) API for backend selection at initialization"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The existing SKGLView is hardcoded to OpenGL ES on all platforms. On iOS 12+, OpenGL is deprecated in favour of Metal (reflected in an [ObsoletedOSPlatform] attribute on the iOS handler). SKMetalView already exists for native Apple views but is not integrated into any MAUI handler. A new view abstraction (e.g., SKGpuView) is needed that delegates to Metal on Apple, Vulkan on Android/Linux, and Direct3D on Windows while keeping a unified C# API.",
    "rationale": "Clearly a feature request: the reporter explicitly states a new view type is needed, describes the desired solution, and identifies the only alternative as forking the library. The request is well-understood — the native building blocks (SKMetalView, SKGLTextureView, SKSwapChainPanel) already exist; the missing piece is a unified MAUI handler that selects the right backend per platform. Categorised as type/feature-request because it adds a new view type; area/SkiaSharp.Views.Maui because MAUI handlers are where the work lives. Ongoing design discussion (comments on backend enum, fallback strategy, runtime switching) indicates the feature requires design decisions before implementation.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.iOS.cs",
        "lines": "13-14",
        "finding": "Handler carries [ObsoletedOSPlatform(\"ios12.0\", \"Use 'Metal' instead.\")] — OpenGL is deprecated on iOS 12+ and there is no MAUI Metal handler to replace it.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/ISKGLView.cs",
        "finding": "Interface is hardcoded to GL semantics: GRContext property, OnPaintSurface(SKPaintGLSurfaceEventArgs). A backend-agnostic interface would need a different event args type or a union type.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs",
        "lines": "11-17",
        "finding": "Android handler creates SKGLTextureView (OpenGL ES). No Vulkan path exists.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Windows.cs",
        "lines": "7",
        "finding": "Windows handler wraps SKSwapChainPanel (DXGI/D3D swap chain) — already uses a D3D-based native control, but still exposes SKPaintGLSurfaceEventArgs at the MAUI layer.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "finding": "SKMetalView exists for native iOS/macOS/tvOS using MTKView and GRContext.CreateMetal(). It is fully functional but not exposed through any MAUI handler — the needed infrastructure already exists.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "both SkiaSharp.Views.Forms and SkiaSharp.Views.Maui only support using OpenGL in the SKGLView since this was the only backend at the time",
        "source": "issue body",
        "interpretation": "Confirms the request is about lack of backend selection, not a regression or bug."
      },
      {
        "text": "we need a GPU view that swaps out the backend controls based on the preferred backend for each platform",
        "source": "issue body",
        "interpretation": "Clear feature scope: new view that auto-selects native GPU backend."
      },
      {
        "text": "[ObsoletedOSPlatform(\"ios12.0\", \"Use 'Metal' instead.\")]",
        "source": "SKGLViewHandler.iOS.cs line 13",
        "interpretation": "Urgency signal: OpenGL is already deprecated on Apple platforms and the codebase acknowledges it."
      },
      {
        "text": "We can let people choose the rendering backend at initialization with .UseSkiaSharp(optional settings) with enum of kind Vulkan, GL, ..., Auto",
        "source": "comment #issuecomment-1959949115",
        "interpretation": "Community has started design discussions around global backend configuration with fallback."
      }
    ],
    "workarounds": [
      "Use the native SKMetalView directly (SkiaSharp.Views.iOS/Mac namespace) for iOS/macOS-specific projects that need Metal acceleration today.",
      "Continue using SKGLView — OpenGL ES still works on all current platforms despite being deprecated on iOS 12+.",
      "Create a custom renderer by forking the platform handler from the SkiaSharp repo (noted as the only alternative by the reporter)."
    ],
    "nextQuestions": [
      "Should the new view expose a backend-agnostic event args type, or reuse SKPaintGLSurfaceEventArgs with a different name?",
      "What is the preferred API for backend selection: per-view property, global UseSkiaSharp() initializer setting, or both?",
      "How should backend availability failures (e.g., no Vulkan driver) be surfaced — exception, event, or silent fallback to raster?"
    ],
    "resolution": {
      "hypothesis": "A new view type (e.g., SKGpuView) with a unified MAUI handler that delegates to SKMetalView on Apple platforms, a Vulkan-backed view on Android/Linux where available, and the existing SKSwapChainPanel on Windows.",
      "proposals": [
        {
          "title": "Create SKGpuView with per-platform backend handlers",
          "description": "Define ISKGpuView interface with backend-agnostic event args and a GrBackend property. Implement platform handlers: Metal on iOS/macOS/tvOS/MacCatalyst (using existing SKMetalView infrastructure), Vulkan on Android (where available with GL fallback), and D3D/GL on Windows. Add SKGpuView MAUI control class.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Add backend configuration to UseSkiaSharp initializer",
          "description": "Expose a GpuBackend enum in the UseSkiaSharp(settings) startup API (as discussed in comments) so devs can globally specify preferred backend and fallback. The existing SKGLViewHandler could then conditionally use Metal on Apple if configured.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Create SKGpuView with per-platform backend handlers",
      "recommendedReason": "Creates a clean new API without breaking existing SKGLView usage. The native building blocks (SKMetalView, SKGLTextureView, SKSwapChainPanel) already exist; the main work is writing the MAUI handler layer and a backend-agnostic interface."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid feature request with significant community interest (14 reactions). Native building blocks exist; a MAUI handler abstraction layer is missing. Ongoing design discussion around backend enum and fallback strategy is needed before implementation can start.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, MAUI views area, backend, tenet, and partner labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Maui",
          "backend/Metal",
          "backend/Vulkan",
          "backend/OpenGL",
          "backend/Direct3D",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge design status and summarise open design questions",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for filing this — it's a well-understood gap. The native building blocks already exist (`SKMetalView` for Apple, `SKGLTextureView` for Android, `SKSwapChainPanel` for Windows), and the iOS handler already carries an `[ObsoletedOSPlatform(\"ios12.0\", \"Use 'Metal' instead.\")]` attribute acknowledging the urgency.\n\nBefore implementation can start, a few design questions need answers:\n1. **API shape** — should the new view expose a backend-agnostic event args type, or reuse an adapted `SKPaintGLSurfaceEventArgs`?\n2. **Backend selection** — per-view property, global `UseSkiaSharp(settings)` initializer, or both?\n3. **Failure handling** — if the preferred backend is unavailable (e.g., no Vulkan driver), should the view throw, fire an event, or silently fall back to raster?\n\nTracking this open until the design discussion converges. Community input welcome on the above."
      }
    ]
  }
}
```

</details>
