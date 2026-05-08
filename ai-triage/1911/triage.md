# Issue Triage Report — #1911

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T22:55:00Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp.Views.Blazor (0.97 (97%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Feature request to expose the private canvasSize field as a public property on both SKCanvasView and SKGLView in SkiaSharp.Views.Blazor, analogous to the already-exposed Dpi property added in PR #1832.

**Analysis:** Both SKCanvasView and SKGLView Blazor components have a private SKSize canvasSize field that tracks current canvas CSS dimensions, but neither exposes it publicly. The Dpi property was already exposed via a similar request (#1831). Adding a CanvasSize property is a small, well-scoped enhancement with a clear precedent.

**Recommendations:** **needs-investigation** — Well-specified enhancement with clear precedent and trivial implementation. Ready for repro confirmation and implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1831 — Analogous issue: expose dpi field in Blazor components (closed as completed)
- https://github.com/mono/SkiaSharp/pull/1832 — PR that added the Dpi public property to SKCanvasView and SKGLView Blazor components
- https://github.com/mono/SkiaSharp/discussions/1888 — Discussion suggesting canvasSize exposure in Blazor components

## Analysis

### Technical Summary

Both SKCanvasView and SKGLView Blazor components have a private SKSize canvasSize field that tracks current canvas CSS dimensions, but neither exposes it publicly. The Dpi property was already exposed via a similar request (#1831). Adding a CanvasSize property is a small, well-scoped enhancement with a clear precedent.

### Rationale

This is a well-scoped enhancement request with clear precedent: the same pattern was used to expose the Dpi field in both Blazor components. The private canvasSize field exists in both SKCanvasView and SKGLView but is inaccessible to consumers. Implementation is trivial (a single read-only expression-body property). Classified as type/enhancement since it adds visibility to an existing internal field following an established pattern.

### Key Signals

- "analogous to https://github.com/mono/SkiaSharp/issues/1831 & https://github.com/mono/SkiaSharp/pull/1832 it may is useful to expose the canvasSize Property, too." — **issue body** (Reporter is requesting parity with the already-shipped dpi exposure. The pattern and precedent are established.)
- "More than just the canvas size, anyone who wants to do non-trivial JS interop (in addition to merely drawing) with this <canvas> element needs the htmlCanvas ElementReference exposed." — **comment by simonsarris** (A secondary request to also expose ElementReference for JS interop, broadening the potential scope.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs` | 25-25 | direct | Private `SKSize canvasSize` field exists but is not exposed publicly. The analogous `dpi` field IS exposed as `public double Dpi => dpi;` (line 65), establishing the precedent for this enhancement. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs` | 33-33 | direct | Private `SKSize canvasSize` field exists but is not exposed publicly. The analogous `dpi` field IS exposed as `public double Dpi => dpi;` (line 72), same pattern. |

### Workarounds

- Access canvas logical size indirectly from SKPaintSurfaceEventArgs within OnPaintSurface: divide e.Info.Width / Dpi and e.Info.Height / Dpi to obtain CSS-pixel dimensions.

### Next Questions

- Should the property name be CanvasSize (matching the backing field) for consistency with other SkiaSharp view types?
- Should ElementReference htmlCanvas also be exposed to support the JS interop use case raised in the comment?

### Resolution Proposals

**Hypothesis:** Add a public read-only CanvasSize property to both SKCanvasView and SKGLView Blazor components, mirroring the existing Dpi property pattern.

1. **Expose CanvasSize as a public read-only property** — fix, confidence 0.92 (92%), cost/xs, validated=yes
   - Add `public SKSize CanvasSize => canvasSize;` to both SKCanvasView and SKGLView in SkiaSharp.Views.Blazor, following the same pattern as the existing `public double Dpi => dpi;`.

```csharp
public SKSize CanvasSize => canvasSize;
```
2. **Workaround via OnPaintSurface event args** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Obtain logical canvas dimensions from SKPaintSurfaceEventArgs during rendering by dividing the pixel size by the Dpi value.

```csharp
// In OnPaintSurface handler (SKPaintSurfaceEventArgs e):
var logicalWidth  = e.Info.Width  / (float)canvasView.Dpi;
var logicalHeight = e.Info.Height / (float)canvasView.Dpi;
```

**Recommended proposal:** Expose CanvasSize as a public read-only property

**Why:** Trivial xs-effort change with established precedent (Dpi property). No design ambiguity.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Well-specified enhancement with clear precedent and trivial implementation. Ready for repro confirmation and implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement, Blazor views area, and WASM platform labels | labels=type/enhancement, area/SkiaSharp.Views.Blazor, os/WASM |
| add-comment | medium | 0.88 (88%) | Confirm the feature request is valid with workaround and implementation guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! This is a valid enhancement — exposing `CanvasSize` as a public property on both `SKCanvasView` and `SKGLView` follows the same pattern as the `Dpi` property that was added in #1832.

In the meantime, you can approximate the logical canvas size from within your `OnPaintSurface` handler:

```csharp
// SKPaintSurfaceEventArgs.Info.Size gives pixel dimensions
// Divide by Dpi to get the logical CSS size
var logicalWidth  = e.Info.Width  / (float)canvasView.Dpi;
var logicalHeight = e.Info.Height / (float)canvasView.Dpi;
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1911,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T22:55:00Z"
  },
  "summary": "Feature request to expose the private canvasSize field as a public property on both SKCanvasView and SKGLView in SkiaSharp.Views.Blazor, analogous to the already-exposed Dpi property added in PR #1832.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.97
    },
    "platforms": [
      "os/WASM"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1831",
          "description": "Analogous issue: expose dpi field in Blazor components (closed as completed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1832",
          "description": "PR that added the Dpi public property to SKCanvasView and SKGLView Blazor components"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/discussions/1888",
          "description": "Discussion suggesting canvasSize exposure in Blazor components"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Both SKCanvasView and SKGLView Blazor components have a private SKSize canvasSize field that tracks current canvas CSS dimensions, but neither exposes it publicly. The Dpi property was already exposed via a similar request (#1831). Adding a CanvasSize property is a small, well-scoped enhancement with a clear precedent.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKCanvasView.razor.cs",
        "lines": "25-25",
        "finding": "Private `SKSize canvasSize` field exists but is not exposed publicly. The analogous `dpi` field IS exposed as `public double Dpi => dpi;` (line 65), establishing the precedent for this enhancement.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/SKGLView.razor.cs",
        "lines": "33-33",
        "finding": "Private `SKSize canvasSize` field exists but is not exposed publicly. The analogous `dpi` field IS exposed as `public double Dpi => dpi;` (line 72), same pattern.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "analogous to https://github.com/mono/SkiaSharp/issues/1831 & https://github.com/mono/SkiaSharp/pull/1832 it may is useful to expose the canvasSize Property, too.",
        "source": "issue body",
        "interpretation": "Reporter is requesting parity with the already-shipped dpi exposure. The pattern and precedent are established."
      },
      {
        "text": "More than just the canvas size, anyone who wants to do non-trivial JS interop (in addition to merely drawing) with this <canvas> element needs the htmlCanvas ElementReference exposed.",
        "source": "comment by simonsarris",
        "interpretation": "A secondary request to also expose ElementReference for JS interop, broadening the potential scope."
      }
    ],
    "rationale": "This is a well-scoped enhancement request with clear precedent: the same pattern was used to expose the Dpi field in both Blazor components. The private canvasSize field exists in both SKCanvasView and SKGLView but is inaccessible to consumers. Implementation is trivial (a single read-only expression-body property). Classified as type/enhancement since it adds visibility to an existing internal field following an established pattern.",
    "workarounds": [
      "Access canvas logical size indirectly from SKPaintSurfaceEventArgs within OnPaintSurface: divide e.Info.Width / Dpi and e.Info.Height / Dpi to obtain CSS-pixel dimensions."
    ],
    "nextQuestions": [
      "Should the property name be CanvasSize (matching the backing field) for consistency with other SkiaSharp view types?",
      "Should ElementReference htmlCanvas also be exposed to support the JS interop use case raised in the comment?"
    ],
    "resolution": {
      "hypothesis": "Add a public read-only CanvasSize property to both SKCanvasView and SKGLView Blazor components, mirroring the existing Dpi property pattern.",
      "proposals": [
        {
          "title": "Expose CanvasSize as a public read-only property",
          "description": "Add `public SKSize CanvasSize => canvasSize;` to both SKCanvasView and SKGLView in SkiaSharp.Views.Blazor, following the same pattern as the existing `public double Dpi => dpi;`.",
          "codeSnippet": "public SKSize CanvasSize => canvasSize;",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Workaround via OnPaintSurface event args",
          "description": "Obtain logical canvas dimensions from SKPaintSurfaceEventArgs during rendering by dividing the pixel size by the Dpi value.",
          "codeSnippet": "// In OnPaintSurface handler (SKPaintSurfaceEventArgs e):\nvar logicalWidth  = e.Info.Width  / (float)canvasView.Dpi;\nvar logicalHeight = e.Info.Height / (float)canvasView.Dpi;",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Expose CanvasSize as a public read-only property",
      "recommendedReason": "Trivial xs-effort change with established precedent (Dpi property). No design ambiguity."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Well-specified enhancement with clear precedent and trivial implementation. Ready for repro confirmation and implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, Blazor views area, and WASM platform labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the feature request is valid with workaround and implementation guidance",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the request! This is a valid enhancement — exposing `CanvasSize` as a public property on both `SKCanvasView` and `SKGLView` follows the same pattern as the `Dpi` property that was added in #1832.\n\nIn the meantime, you can approximate the logical canvas size from within your `OnPaintSurface` handler:\n\n```csharp\n// SKPaintSurfaceEventArgs.Info.Size gives pixel dimensions\n// Divide by Dpi to get the logical CSS size\nvar logicalWidth  = e.Info.Width  / (float)canvasView.Dpi;\nvar logicalHeight = e.Info.Height / (float)canvasView.Dpi;\n```"
      }
    ]
  }
}
```

</details>
