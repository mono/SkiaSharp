# Issue Triage Report — #4351

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-29T05:14:22Z |
| Type | type/question (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** Reporter believes canvas.RotateDegrees with a center-point pivot rotates around (0,0) instead of the supplied pivot, but code inspection confirms the SkiaSharp implementation is mathematically correct; the observed behavior is likely caused by the map rendering layer computing screen coordinates independently of the canvas transform.

**Analysis:** The SkiaSharp canvas rotation APIs are correctly implemented. RotateDegrees(degrees, px, py) performs Translate(px,py) → RotateDegrees(degrees) → Translate(-px,-py), which is the standard matrix decomposition for rotation around an arbitrary pivot. The reporter's manual Translate/RotateDegrees/Translate sequence is also correct. The most likely cause of the observed behavior is that the underlying map rendering layer independently computes screen coordinates from geo-coordinates on each frame without accounting for the canvas transform, so the rotation appears to be around (0,0) from the user's perspective.

**Recommendations:** **close-as-not-a-bug** — The SkiaSharp RotateDegrees(degrees, px, py) and manual Translate/Rotate/Translate implementations are mathematically correct (verified in source). The reported behavior is consistent with the map rendering framework independently projecting geo-coordinates, not a SkiaSharp defect.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a map control that renders geo-coordinates to canvas coordinates
2. Apply canvas.Translate(cx,cy) + canvas.RotateDegrees(angle) + canvas.Translate(-cx,-cy) around the draw call
3. Observe that the map visual rotates around the canvas origin (0,0) rather than (cx,cy)

**Environment:** SkiaSharp 4.148.0, Visual Studio Windows, Android 11+, Windows 10.0.19041.0+

**Attachments:**
- map-0-rotation.png — https://github.com/user-attachments/assets/ec82a76b-a4b4-493f-ad1d-962343e68e04 — Map with 0 rotation centered on Salt Lake City
- map-rotated.png — https://github.com/user-attachments/assets/86b121b1-1a01-4197-b8c2-b05d33e868b5 — Map after rotation showing pin off center

**Code snippets:**

```csharp
canvas.Save();
var cx = _canvasSize.Width / 2f;
var cy = _canvasSize.Height / 2f;
canvas.Translate(cx, cy);
canvas.RotateDegrees(MapRotation);
canvas.Translate(-cx, -cy);
// Also tried: canvas.RotateDegrees(MapRotation, cx, cy)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.148.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The RotateDegrees pivot overload has been implemented as Translate/Rotate/Translate for multiple releases and is mathematically correct. |

## Analysis

### Technical Summary

The SkiaSharp canvas rotation APIs are correctly implemented. RotateDegrees(degrees, px, py) performs Translate(px,py) → RotateDegrees(degrees) → Translate(-px,-py), which is the standard matrix decomposition for rotation around an arbitrary pivot. The reporter's manual Translate/RotateDegrees/Translate sequence is also correct. The most likely cause of the observed behavior is that the underlying map rendering layer independently computes screen coordinates from geo-coordinates on each frame without accounting for the canvas transform, so the rotation appears to be around (0,0) from the user's perspective.

### Rationale

Code inspection confirms the SkiaSharp rotate-around-pivot API is mathematically sound. The Translate/Rotate/Translate pattern is standard and correct. The symptom (rotation around (0,0) despite correct pivot values) is characteristic of a map rendering framework that projects geo-coordinates to screen coordinates independently per frame, bypassing the canvas transform. Classified as type/question because the SkiaSharp API behaves as designed.

### Key Signals

- "my center point is still set for Salt Lake City, yet the rotation still appears to be rotating around (0, 0)" — **issue body** (Indicates the map layer's geo-center binding is separate from the canvas transform; the map may be re-projecting all points from scratch each frame, making the rotation appear to occur around the canvas origin.)
- "I have also tried doing canvas.RotateDegrees(MapRotation, cx, cy)" — **issue body** (Reporter tried both manual and convenience overload; both use the same Translate/Rotate/Translate sequence internally, so they would behave identically — consistent with a rendering-pipeline issue rather than an API bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 242-250 | direct | RotateDegrees(float degrees, float px, float py) is implemented as: Translate(px,py); RotateDegrees(degrees); Translate(-px,-py). This is the correct matrix decomposition for rotating around pivot (px,py): the resulting transform is T(px,py)*R(θ)*T(-px,-py), which maps point P to R(θ)*(P-(px,py))+(px,py). |
| `binding/SkiaSharp/SKCanvas.cs` | 224-230 | direct | RotateDegrees(float degrees) delegates directly to sk_canvas_rotate_degrees(Handle, degrees) after a no-op short-circuit for full-circle multiples. No anomalies in the base rotation path. |

### Next Questions

- Does the map rendering library expose a 'bearing' or 'heading' property that should be used instead of canvas rotation?
- Is the canvas transform being applied before or after the map renders its tile/content layers?
- Does wrapping the entire map render in canvas.Save()/transform/canvas.Restore() still show rotation around (0,0)?

### Resolution Proposals

**Hypothesis:** The map rendering layer calculates screen positions from geo-coordinates on every PaintSurface call, so canvas transforms affect only the final pixel output after projection — effectively rotating the rendered bitmap rather than the geo-coordinate space. To achieve geo-space rotation the map's own bearing/rotation property must be used.

1. **Use the map library's built-in bearing/heading property** — workaround, cost/xs, validated=untested
   - Most interactive map libraries (Mapsui, MapTiler, etc.) expose a Map.Viewport.Rotation or Bearing property. Setting this causes the projection to rotate geo-coordinates before rendering, achieving rotation around the correct geo-center.
2. **Apply canvas transform to a pre-rendered bitmap** — workaround, cost/s, validated=untested
   - Render the map to an SKBitmap at its natural orientation first, then apply the rotation transform when blitting that bitmap to the final canvas. This way the pivot point is in bitmap coordinates, not canvas coordinates, and the full bitmap is rotated correctly.

**Recommended proposal:** proposal-1

**Why:** If the map library has a native rotation/bearing property it should always be preferred over canvas-level transforms, as it accounts for geo-coordinate projection.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | The SkiaSharp RotateDegrees(degrees, px, py) and manual Translate/Rotate/Translate implementations are mathematically correct (verified in source). The reported behavior is consistent with the map rendering framework independently projecting geo-coordinates, not a SkiaSharp defect. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.82 (82%) | Correct label to type/question since the SkiaSharp API is working as designed | labels=type/question, area/SkiaSharp, os/Android, os/Windows-Classic |
| add-comment | high | 0.78 (78%) | Explain that the SkiaSharp rotate API is correct and the issue is likely the map rendering pipeline; provide workaround pointers | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed screenshots — they really help illustrate the behavior.

After inspecting the SkiaSharp source, the `RotateDegrees(degrees, px, py)` overload and the manual `Translate/RotateDegrees/Translate` sequence are both mathematically correct: they produce the standard rotation-around-pivot matrix `T(px,py)·R(θ)·T(-px,-py)`, which rotates any drawn point around the supplied center. There is no bug in the SkiaSharp canvas transform layer.

The behavior you're describing — rotation appearing to happen around (0,0) — is a common symptom when a map rendering library independently projects geo-coordinates to screen pixels on every paint frame. In that case the canvas transform is applied _after_ the projection, so it rotates the final rendered pixels rather than the geo-coordinate space. The map's internal binding still records Salt Lake City as the center, but the pixels have already been laid out as if there were no rotation.

**Suggested workaround:** Check whether your map library exposes a native `Rotation`, `Bearing`, or `Heading` property on the viewport or map object. Setting that will rotate the projection itself around the geo-center, which is what you're looking for. Canvas-level transforms are generally not the right tool for rotating an interactive map.

If you are rendering the map entirely yourself (custom tile renderer), one alternative is to render the full map to an `SKBitmap` at 0° and then rotate that bitmap onto the canvas with the desired pivot.

Would you be able to check whether your map library has a built-in rotation property?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4351,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-29T05:14:22Z"
  },
  "summary": "Reporter believes canvas.RotateDegrees with a center-point pivot rotates around (0,0) instead of the supplied pivot, but code inspection confirms the SkiaSharp implementation is mathematically correct; the observed behavior is likely caused by the map rendering layer computing screen coordinates independently of the canvas transform.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android",
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a map control that renders geo-coordinates to canvas coordinates",
        "Apply canvas.Translate(cx,cy) + canvas.RotateDegrees(angle) + canvas.Translate(-cx,-cy) around the draw call",
        "Observe that the map visual rotates around the canvas origin (0,0) rather than (cx,cy)"
      ],
      "codeSnippets": [
        "canvas.Save();\nvar cx = _canvasSize.Width / 2f;\nvar cy = _canvasSize.Height / 2f;\ncanvas.Translate(cx, cy);\ncanvas.RotateDegrees(MapRotation);\ncanvas.Translate(-cx, -cy);\n// Also tried: canvas.RotateDegrees(MapRotation, cx, cy)"
      ],
      "attachments": [
        {
          "url": "https://github.com/user-attachments/assets/ec82a76b-a4b4-493f-ad1d-962343e68e04",
          "filename": "map-0-rotation.png",
          "description": "Map with 0 rotation centered on Salt Lake City"
        },
        {
          "url": "https://github.com/user-attachments/assets/86b121b1-1a01-4197-b8c2-b05d33e868b5",
          "filename": "map-rotated.png",
          "description": "Map after rotation showing pin off center"
        }
      ],
      "environmentDetails": "SkiaSharp 4.148.0, Visual Studio Windows, Android 11+, Windows 10.0.19041.0+",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.148.0"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The RotateDegrees pivot overload has been implemented as Translate/Rotate/Translate for multiple releases and is mathematically correct."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp canvas rotation APIs are correctly implemented. RotateDegrees(degrees, px, py) performs Translate(px,py) → RotateDegrees(degrees) → Translate(-px,-py), which is the standard matrix decomposition for rotation around an arbitrary pivot. The reporter's manual Translate/RotateDegrees/Translate sequence is also correct. The most likely cause of the observed behavior is that the underlying map rendering layer independently computes screen coordinates from geo-coordinates on each frame without accounting for the canvas transform, so the rotation appears to be around (0,0) from the user's perspective.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "242-250",
        "finding": "RotateDegrees(float degrees, float px, float py) is implemented as: Translate(px,py); RotateDegrees(degrees); Translate(-px,-py). This is the correct matrix decomposition for rotating around pivot (px,py): the resulting transform is T(px,py)*R(θ)*T(-px,-py), which maps point P to R(θ)*(P-(px,py))+(px,py).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "224-230",
        "finding": "RotateDegrees(float degrees) delegates directly to sk_canvas_rotate_degrees(Handle, degrees) after a no-op short-circuit for full-circle multiples. No anomalies in the base rotation path.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "my center point is still set for Salt Lake City, yet the rotation still appears to be rotating around (0, 0)",
        "source": "issue body",
        "interpretation": "Indicates the map layer's geo-center binding is separate from the canvas transform; the map may be re-projecting all points from scratch each frame, making the rotation appear to occur around the canvas origin."
      },
      {
        "text": "I have also tried doing canvas.RotateDegrees(MapRotation, cx, cy)",
        "source": "issue body",
        "interpretation": "Reporter tried both manual and convenience overload; both use the same Translate/Rotate/Translate sequence internally, so they would behave identically — consistent with a rendering-pipeline issue rather than an API bug."
      }
    ],
    "rationale": "Code inspection confirms the SkiaSharp rotate-around-pivot API is mathematically sound. The Translate/Rotate/Translate pattern is standard and correct. The symptom (rotation around (0,0) despite correct pivot values) is characteristic of a map rendering framework that projects geo-coordinates to screen coordinates independently per frame, bypassing the canvas transform. Classified as type/question because the SkiaSharp API behaves as designed.",
    "nextQuestions": [
      "Does the map rendering library expose a 'bearing' or 'heading' property that should be used instead of canvas rotation?",
      "Is the canvas transform being applied before or after the map renders its tile/content layers?",
      "Does wrapping the entire map render in canvas.Save()/transform/canvas.Restore() still show rotation around (0,0)?"
    ],
    "resolution": {
      "hypothesis": "The map rendering layer calculates screen positions from geo-coordinates on every PaintSurface call, so canvas transforms affect only the final pixel output after projection — effectively rotating the rendered bitmap rather than the geo-coordinate space. To achieve geo-space rotation the map's own bearing/rotation property must be used.",
      "proposals": [
        {
          "title": "Use the map library's built-in bearing/heading property",
          "description": "Most interactive map libraries (Mapsui, MapTiler, etc.) expose a Map.Viewport.Rotation or Bearing property. Setting this causes the projection to rotate geo-coordinates before rendering, achieving rotation around the correct geo-center.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Apply canvas transform to a pre-rendered bitmap",
          "description": "Render the map to an SKBitmap at its natural orientation first, then apply the rotation transform when blitting that bitmap to the final canvas. This way the pivot point is in bitmap coordinates, not canvas coordinates, and the full bitmap is rotated correctly.",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "If the map library has a native rotation/bearing property it should always be preferred over canvas-level transforms, as it accounts for geo-coordinate projection."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "The SkiaSharp RotateDegrees(degrees, px, py) and manual Translate/Rotate/Translate implementations are mathematically correct (verified in source). The reported behavior is consistent with the map rendering framework independently projecting geo-coordinates, not a SkiaSharp defect.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label to type/question since the SkiaSharp API is working as designed",
        "risk": "low",
        "confidence": 0.82,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Android",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that the SkiaSharp rotate API is correct and the issue is likely the map rendering pipeline; provide workaround pointers",
        "risk": "high",
        "confidence": 0.78,
        "comment": "Thanks for the detailed screenshots — they really help illustrate the behavior.\n\nAfter inspecting the SkiaSharp source, the `RotateDegrees(degrees, px, py)` overload and the manual `Translate/RotateDegrees/Translate` sequence are both mathematically correct: they produce the standard rotation-around-pivot matrix `T(px,py)·R(θ)·T(-px,-py)`, which rotates any drawn point around the supplied center. There is no bug in the SkiaSharp canvas transform layer.\n\nThe behavior you're describing — rotation appearing to happen around (0,0) — is a common symptom when a map rendering library independently projects geo-coordinates to screen pixels on every paint frame. In that case the canvas transform is applied _after_ the projection, so it rotates the final rendered pixels rather than the geo-coordinate space. The map's internal binding still records Salt Lake City as the center, but the pixels have already been laid out as if there were no rotation.\n\n**Suggested workaround:** Check whether your map library exposes a native `Rotation`, `Bearing`, or `Heading` property on the viewport or map object. Setting that will rotate the projection itself around the geo-center, which is what you're looking for. Canvas-level transforms are generally not the right tool for rotating an interactive map.\n\nIf you are rendering the map entirely yourself (custom tile renderer), one alternative is to render the full map to an `SKBitmap` at 0° and then rotate that bitmap onto the canvas with the desired pivot.\n\nWould you be able to check whether your map library has a built-in rotation property?"
      }
    ]
  }
}
```

</details>
