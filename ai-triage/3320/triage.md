# Issue Triage Report — #3320

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T23:59:18Z |
| Type | type/feature-request (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Request for a WPF control (SKDXElement) using the Direct3D backend, similar to the existing SKGLElement, along with usage examples for the SkiaSharp.Direct3D.Vortice package in a WPF context.

**Analysis:** The reporter wants a DirectX-backed WPF control analogous to SKGLElement. The SkiaSharp.Direct3D.Vortice package exists and provides GRVorticeD3DBackendContext and GRVorticeD3DTextureResourceInfo, but there is no ready-made WPF control wrapping the D3D12 backend. Creating SKDXElement would require implementing a WPF D3D12-interop control, initializing the Skia GRContext with the D3D backend, and rendering into a D3D swap chain exposed through WPF interop (D3DImage or similar).

**Recommendations:** **keep-open** — Valid feature request — the Direct3D backend exists but no WPF control wraps it. The request is reasonable for Windows-only development but requires significant new API surface and WPF D3D interop work.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/Direct3D |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** WPF on Windows; currently using SKGLElement as alternative

## Analysis

### Technical Summary

The reporter wants a DirectX-backed WPF control analogous to SKGLElement. The SkiaSharp.Direct3D.Vortice package exists and provides GRVorticeD3DBackendContext and GRVorticeD3DTextureResourceInfo, but there is no ready-made WPF control wrapping the D3D12 backend. Creating SKDXElement would require implementing a WPF D3D12-interop control, initializing the Skia GRContext with the D3D backend, and rendering into a D3D swap chain exposed through WPF interop (D3DImage or similar).

### Rationale

Title and body both explicitly ask for a new feature (a DirectX WPF element). The SkiaSharp.Direct3D.Vortice package exists and provides the backend plumbing but no WPF visual control. No SKDXElement or D3D WPF control exists in the views layer. This is a new-feature gap, not a bug.

### Key Signals

- "[FEATURE] SKDXElement?" — **issue title** (Explicit feature request for a DirectX-backed WPF rendering element.)
- "Can you provide an example about how to use the new directx backend? in particular in a wpf context" — **issue body** (Reporter is also asking for usage documentation/examples for the new SkiaSharp.Direct3D.Vortice package in WPF.)
- "I'm using SKGLElement but I'm developing on windows only" — **issue body** (Reporter currently uses the OpenGL path but prefers native D3D for Windows-only development.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 1-221 | direct | SKGLElement is the WPF OpenGL control backed by OpenTK.Wpf. No parallel Direct3D control exists in the WPF views layer. |
| `source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DBackendContext.cs` | 1-47 | direct | GRVorticeD3DBackendContext wraps adapter/device/queue pointers for the Skia D3D12 backend but does not provide any WPF interop or rendering loop. |
| `source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DTextureResourceInfo.cs` | 1-37 | related | GRVorticeD3DTextureResourceInfo wraps a D3D12 texture resource with Skia resource state and format — confirms the package is focused on backend binding, not a visual control. |

### Next Questions

- Should SKDXElement live in SkiaSharp.Views.WPF or in the SkiaSharp.Direct3D.Vortice package?
- Does WPF D3DImage interop provide a suitable surface for the Skia D3D12 backend?

### Resolution Proposals

**Hypothesis:** A WPF D3D control does not yet exist. Implementing SKDXElement would require D3DImage or hardware surface interop plus a Skia GRContext backed by the D3D12 context from SkiaSharp.Direct3D.Vortice.

1. **Create SKDXElement in SkiaSharp.Views.WPF** — fix, confidence 0.70 (70%), cost/xl, validated=untested
   - Add a new WPF UserControl using WPF D3DImage interop to expose a swap chain surface to Skia via GRVorticeD3DBackendContext. Mirror the pattern used by SKGLElement.
2. **Provide a sample/example for using SkiaSharp.Direct3D.Vortice with WPF** — workaround, confidence 0.65 (65%), cost/m, validated=untested
   - Add a WPF sample that manually sets up the D3D12 device, creates a GRVorticeD3DBackendContext, and renders into a D3DImage-backed surface as a stopgap until a packaged control is ready.

**Recommended proposal:** Provide a sample/example for using SkiaSharp.Direct3D.Vortice with WPF

**Why:** A sample gives reporters an immediate path forward while a full WPF D3D control is considered for future packaging.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request — the Direct3D backend exists but no WPF control wraps it. The request is reasonable for Windows-only development but requires significant new API surface and WPF D3D interop work. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply feature-request, views area, Windows, and Direct3D backend labels | labels=type/feature-request, area/SkiaSharp.Views, os/Windows-Classic, backend/Direct3D |
| add-comment | medium | 0.80 (80%) | Acknowledge request and point to existing Direct3D package while explaining that a WPF control does not yet exist | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! The `SkiaSharp.Direct3D.Vortice` NuGet package already provides the Direct3D 12 backend plumbing (`GRVorticeD3DBackendContext`), but there is currently no ready-made WPF control equivalent to `SKGLElement` that wraps it.

In the meantime, you can set up Direct3D rendering manually:
1. Create a D3D12 device and command queue using Vortice.Direct3D12.
2. Build a `GRVorticeD3DBackendContext` and create a Skia `GRContext` with `GRContext.CreateDirect3D(ctx)`.
3. Render into a D3D texture and present it via WPF's `D3DImage` for display in a WPF window.

A full `SKDXElement` WPF control is tracked in this issue. Upvoting with 👍 helps prioritize it.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3320,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T23:59:18Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Request for a WPF control (SKDXElement) using the Direct3D backend, similar to the existing SKGLElement, along with usage examples for the SkiaSharp.Direct3D.Vortice package in a WPF context.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Direct3D"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "WPF on Windows; currently using SKGLElement as alternative"
    }
  },
  "analysis": {
    "summary": "The reporter wants a DirectX-backed WPF control analogous to SKGLElement. The SkiaSharp.Direct3D.Vortice package exists and provides GRVorticeD3DBackendContext and GRVorticeD3DTextureResourceInfo, but there is no ready-made WPF control wrapping the D3D12 backend. Creating SKDXElement would require implementing a WPF D3D12-interop control, initializing the Skia GRContext with the D3D backend, and rendering into a D3D swap chain exposed through WPF interop (D3DImage or similar).",
    "rationale": "Title and body both explicitly ask for a new feature (a DirectX WPF element). The SkiaSharp.Direct3D.Vortice package exists and provides the backend plumbing but no WPF visual control. No SKDXElement or D3D WPF control exists in the views layer. This is a new-feature gap, not a bug.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "lines": "1-221",
        "finding": "SKGLElement is the WPF OpenGL control backed by OpenTK.Wpf. No parallel Direct3D control exists in the WPF views layer.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DBackendContext.cs",
        "lines": "1-47",
        "finding": "GRVorticeD3DBackendContext wraps adapter/device/queue pointers for the Skia D3D12 backend but does not provide any WPF interop or rendering loop.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DTextureResourceInfo.cs",
        "lines": "1-37",
        "finding": "GRVorticeD3DTextureResourceInfo wraps a D3D12 texture resource with Skia resource state and format — confirms the package is focused on backend binding, not a visual control.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "[FEATURE] SKDXElement?",
        "source": "issue title",
        "interpretation": "Explicit feature request for a DirectX-backed WPF rendering element."
      },
      {
        "text": "Can you provide an example about how to use the new directx backend? in particular in a wpf context",
        "source": "issue body",
        "interpretation": "Reporter is also asking for usage documentation/examples for the new SkiaSharp.Direct3D.Vortice package in WPF."
      },
      {
        "text": "I'm using SKGLElement but I'm developing on windows only",
        "source": "issue body",
        "interpretation": "Reporter currently uses the OpenGL path but prefers native D3D for Windows-only development."
      }
    ],
    "nextQuestions": [
      "Should SKDXElement live in SkiaSharp.Views.WPF or in the SkiaSharp.Direct3D.Vortice package?",
      "Does WPF D3DImage interop provide a suitable surface for the Skia D3D12 backend?"
    ],
    "resolution": {
      "hypothesis": "A WPF D3D control does not yet exist. Implementing SKDXElement would require D3DImage or hardware surface interop plus a Skia GRContext backed by the D3D12 context from SkiaSharp.Direct3D.Vortice.",
      "proposals": [
        {
          "title": "Create SKDXElement in SkiaSharp.Views.WPF",
          "description": "Add a new WPF UserControl using WPF D3DImage interop to expose a swap chain surface to Skia via GRVorticeD3DBackendContext. Mirror the pattern used by SKGLElement.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xl",
          "validated": "untested"
        },
        {
          "title": "Provide a sample/example for using SkiaSharp.Direct3D.Vortice with WPF",
          "description": "Add a WPF sample that manually sets up the D3D12 device, creates a GRVorticeD3DBackendContext, and renders into a D3DImage-backed surface as a stopgap until a packaged control is ready.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Provide a sample/example for using SkiaSharp.Direct3D.Vortice with WPF",
      "recommendedReason": "A sample gives reporters an immediate path forward while a full WPF D3D control is considered for future packaging."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request — the Direct3D backend exists but no WPF control wraps it. The request is reasonable for Windows-only development but requires significant new API surface and WPF D3D interop work.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views area, Windows, and Direct3D backend labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/Direct3D"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and point to existing Direct3D package while explaining that a WPF control does not yet exist",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the feature request! The `SkiaSharp.Direct3D.Vortice` NuGet package already provides the Direct3D 12 backend plumbing (`GRVorticeD3DBackendContext`), but there is currently no ready-made WPF control equivalent to `SKGLElement` that wraps it.\n\nIn the meantime, you can set up Direct3D rendering manually:\n1. Create a D3D12 device and command queue using Vortice.Direct3D12.\n2. Build a `GRVorticeD3DBackendContext` and create a Skia `GRContext` with `GRContext.CreateDirect3D(ctx)`.\n3. Render into a D3D texture and present it via WPF's `D3DImage` for display in a WPF window.\n\nA full `SKDXElement` WPF control is tracked in this issue. Upvoting with 👍 helps prioritize it."
      }
    ]
  }
}
```

</details>
