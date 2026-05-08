# Issue Triage Report — #1808

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T01:50:00Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.87 (87%)) |

**Issue Summary:** Feature request to expose background blur (backdrop filter) via SkCanvas::SaveLayerRec; the requested SKCanvasSaveLayerRec.Backdrop API is already present in SkiaSharp, making this request largely fulfilled.

**Analysis:** The core feature — backdrop blur via SKCanvasSaveLayerRec — is already implemented. SKCanvasSaveLayerRec.Backdrop accepts any SKImageFilter (including CreateBlur), which is the mechanism Skia exposes for backdrop/frosted-glass effects. The newer BackdropTileMode field (added in Skia m131 to control edge tiling) is not yet bound and is tracked separately in #3621.

**Recommendations:** **close-as-fixed** — SKCanvasSaveLayerRec.Backdrop API is already present and enables backdrop blur. The issue can be closed with a comment showing the usage pattern. The newer BackdropTileMode field is tracked separately in #3621.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No specific platform mentioned; cross-platform feature request filed 2021-09-14

**Repository links:**
- https://webkit.org/blog/3632/introducing-backdrop-filters/ — WebKit backdrop-filter examples the reporter wants to replicate
- https://bugs.chromium.org/p/skia/issues/detail?id=4915 — Upstream Skia issue reporter referenced as the solution path
- https://api.skia.org/structSkCanvas_1_1SaveLayerRec.html — Skia SaveLayerRec docs – fBackdrop is the backdrop image filter field
- https://github.com/mono/SkiaSharp/pull/2962 — PR #2962 referenced in comments as likely dependent on / related to this issue
- https://github.com/mono/SkiaSharp/issues/3621 — Issue #3621 tracks SKCanvasSaveLayerRec.BackdropTileMode (Skia m131) as an incomplete binding

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.88 (88%) |
| Reason | SKCanvasSaveLayerRec.Backdrop (mapped to SkCanvas::SaveLayerRec::fBackdrop) already exists in binding/SkiaSharp/SKCanvas.cs. Users can pass an SKImageFilter.CreateBlur() as the Backdrop field to achieve backdrop blur today. The only missing piece is BackdropTileMode (Skia m131), which is tracked in #3621. |
| Related PRs | #2962 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The core feature — backdrop blur via SKCanvasSaveLayerRec — is already implemented. SKCanvasSaveLayerRec.Backdrop accepts any SKImageFilter (including CreateBlur), which is the mechanism Skia exposes for backdrop/frosted-glass effects. The newer BackdropTileMode field (added in Skia m131 to control edge tiling) is not yet bound and is tracked separately in #3621.

### Rationale

Reporter asked for exposure of SkCanvas::SaveLayerRec fBackdrop. Code investigation confirms this field is already bound as SKCanvasSaveLayerRec.Backdrop. The fix status is high-confidence because the test SaveLayerRecWithImageFilterIsCorrect demonstrates the same mechanism. The only missing piece is BackdropTileMode (m131), which is a separate enhancement.

### Key Signals

- "Skia has this method https://api.skia.org/structSkCanvas_1_1SaveLayerRec.html — Can SkiaSharp implement it?" — **issue body** (Reporter specifically requests the SaveLayerRec.fBackdrop exposure, which is already done.)
- "Probably dependent on this https://github.com/mono/SkiaSharp/pull/2962" — **comment by michaldobrodenka (2024-12-12)** (PR #2962 likely adds BackdropTileMode, the remaining missing enhancement.)
- "SKCanvasSaveLayerRec.BackdropTileMode | m131 | ⚠️ Incomplete | Struct needs new field" — **issue #3621 body (related issue)** (Confirms core Backdrop is there but the newer tile-mode field is still pending.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 1090-1112 | direct | SKCanvasSaveLayerRec struct has Backdrop property (type SKImageFilter?) that maps to fBackdrop in the native SkCanvas::SaveLayerRec. Already fully bound and usable. |
| `binding/SkiaSharp/SKImageFilter.cs` | 37-58 | direct | SKImageFilter.CreateBlur(sigmaX, sigmaY, tileMode) API exists with multiple overloads — can be passed directly as the Backdrop field to achieve background blur. |
| `tests/Tests/SkiaSharp/SKCanvasTest.cs` | 726-757 | related | SaveLayerRecWithImageFilterIsCorrect test demonstrates setting Backdrop on SKCanvasSaveLayerRec with an image filter and calling canvas.SaveLayer(rec) — confirms the API works end-to-end. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 18185-18215 | context | SKCanvasSaveLayerRecNative struct has fBackdrop field (sk_imagefilter_t). The field is included in ToNative() conversion. No BackdropTileMode field present — confirms that enhancement is still missing. |

### Workarounds

- Use SKCanvasSaveLayerRec.Backdrop = SKImageFilter.CreateBlur(sigmaX, sigmaY) with canvas.SaveLayer(rec) to apply a backdrop blur effect today.
- For shader-based blur alternative, use SKRuntimeEffect with a custom blur shader passed as a backdrop filter.

### Resolution Proposals

**Hypothesis:** The reporter's request is fulfilled by SKCanvasSaveLayerRec.Backdrop, which already exposes the Skia fBackdrop field. A code example was never posted, so the reporter may be unaware.

1. **Post working code example and close** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Reply with a code snippet showing how to use SKCanvasSaveLayerRec.Backdrop = SKImageFilter.CreateBlur(...) and reference #3621 for the pending BackdropTileMode enhancement.

```csharp
// Backdrop blur (frosted glass effect)
using var blurFilter = SKImageFilter.CreateBlur(20, 20);
var rec = new SKCanvasSaveLayerRec { Backdrop = blurFilter };
canvas.SaveLayer(rec);
// Draw your overlay content here
canvas.Restore();
```

**Recommended proposal:** Post working code example and close

**Why:** Core feature is already implemented. A clear code example and close-as-fixed is the most helpful response. BackdropTileMode remains tracked in #3621.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.87 (87%) |
| Reason | SKCanvasSaveLayerRec.Backdrop API is already present and enables backdrop blur. The issue can be closed with a comment showing the usage pattern. The newer BackdropTileMode field is tracked separately in #3621. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request and core area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Post code example showing SKCanvasSaveLayerRec.Backdrop usage for backdrop blur | — |
| close-issue | medium | 0.85 (85%) | Close as fixed — core feature is already present in SKCanvasSaveLayerRec.Backdrop | stateReason=completed |
| link-related | low | 0.90 (90%) | Link to #3621 which tracks the pending BackdropTileMode enhancement | linkedIssue=#3621 |

**Comment draft for `add-comment`:**

```markdown
Good news: this functionality is already available in SkiaSharp! You can use `SKCanvasSaveLayerRec.Backdrop` with `SKImageFilter.CreateBlur()` to achieve background blur:

```csharp
// Backdrop blur (frosted glass effect)
using var blurFilter = SKImageFilter.CreateBlur(20, 20);
var rec = new SKCanvasSaveLayerRec { Backdrop = blurFilter };
canvas.SaveLayer(rec);
// Draw your overlay content here (e.g. semi-transparent panel)
canvas.Restore();
```

The `Backdrop` field on `SKCanvasSaveLayerRec` maps directly to `SkCanvas::SaveLayerRec::fBackdrop` — anything drawn before `SaveLayer` is processed through the filter before appearing in the new layer.

For edge tile-mode control (to avoid edge artifacts), that enhancement (`BackdropTileMode`) is tracked in #3621 and depends on PR #2962.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1808,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T01:50:00Z"
  },
  "summary": "Feature request to expose background blur (backdrop filter) via SkCanvas::SaveLayerRec; the requested SKCanvasSaveLayerRec.Backdrop API is already present in SkiaSharp, making this request largely fulfilled.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific platform mentioned; cross-platform feature request filed 2021-09-14",
      "repoLinks": [
        {
          "url": "https://webkit.org/blog/3632/introducing-backdrop-filters/",
          "description": "WebKit backdrop-filter examples the reporter wants to replicate"
        },
        {
          "url": "https://bugs.chromium.org/p/skia/issues/detail?id=4915",
          "description": "Upstream Skia issue reporter referenced as the solution path"
        },
        {
          "url": "https://api.skia.org/structSkCanvas_1_1SaveLayerRec.html",
          "description": "Skia SaveLayerRec docs – fBackdrop is the backdrop image filter field"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2962",
          "description": "PR #2962 referenced in comments as likely dependent on / related to this issue"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3621",
          "description": "Issue #3621 tracks SKCanvasSaveLayerRec.BackdropTileMode (Skia m131) as an incomplete binding"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.88,
      "reason": "SKCanvasSaveLayerRec.Backdrop (mapped to SkCanvas::SaveLayerRec::fBackdrop) already exists in binding/SkiaSharp/SKCanvas.cs. Users can pass an SKImageFilter.CreateBlur() as the Backdrop field to achieve backdrop blur today. The only missing piece is BackdropTileMode (Skia m131), which is tracked in #3621.",
      "relatedPRs": [
        2962
      ]
    }
  },
  "analysis": {
    "summary": "The core feature — backdrop blur via SKCanvasSaveLayerRec — is already implemented. SKCanvasSaveLayerRec.Backdrop accepts any SKImageFilter (including CreateBlur), which is the mechanism Skia exposes for backdrop/frosted-glass effects. The newer BackdropTileMode field (added in Skia m131 to control edge tiling) is not yet bound and is tracked separately in #3621.",
    "rationale": "Reporter asked for exposure of SkCanvas::SaveLayerRec fBackdrop. Code investigation confirms this field is already bound as SKCanvasSaveLayerRec.Backdrop. The fix status is high-confidence because the test SaveLayerRecWithImageFilterIsCorrect demonstrates the same mechanism. The only missing piece is BackdropTileMode (m131), which is a separate enhancement.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "1090-1112",
        "finding": "SKCanvasSaveLayerRec struct has Backdrop property (type SKImageFilter?) that maps to fBackdrop in the native SkCanvas::SaveLayerRec. Already fully bound and usable.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "37-58",
        "finding": "SKImageFilter.CreateBlur(sigmaX, sigmaY, tileMode) API exists with multiple overloads — can be passed directly as the Backdrop field to achieve background blur.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKCanvasTest.cs",
        "lines": "726-757",
        "finding": "SaveLayerRecWithImageFilterIsCorrect test demonstrates setting Backdrop on SKCanvasSaveLayerRec with an image filter and calling canvas.SaveLayer(rec) — confirms the API works end-to-end.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "18185-18215",
        "finding": "SKCanvasSaveLayerRecNative struct has fBackdrop field (sk_imagefilter_t). The field is included in ToNative() conversion. No BackdropTileMode field present — confirms that enhancement is still missing.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Skia has this method https://api.skia.org/structSkCanvas_1_1SaveLayerRec.html — Can SkiaSharp implement it?",
        "source": "issue body",
        "interpretation": "Reporter specifically requests the SaveLayerRec.fBackdrop exposure, which is already done."
      },
      {
        "text": "Probably dependent on this https://github.com/mono/SkiaSharp/pull/2962",
        "source": "comment by michaldobrodenka (2024-12-12)",
        "interpretation": "PR #2962 likely adds BackdropTileMode, the remaining missing enhancement."
      },
      {
        "text": "SKCanvasSaveLayerRec.BackdropTileMode | m131 | ⚠️ Incomplete | Struct needs new field",
        "source": "issue #3621 body (related issue)",
        "interpretation": "Confirms core Backdrop is there but the newer tile-mode field is still pending."
      }
    ],
    "workarounds": [
      "Use SKCanvasSaveLayerRec.Backdrop = SKImageFilter.CreateBlur(sigmaX, sigmaY) with canvas.SaveLayer(rec) to apply a backdrop blur effect today.",
      "For shader-based blur alternative, use SKRuntimeEffect with a custom blur shader passed as a backdrop filter."
    ],
    "resolution": {
      "hypothesis": "The reporter's request is fulfilled by SKCanvasSaveLayerRec.Backdrop, which already exposes the Skia fBackdrop field. A code example was never posted, so the reporter may be unaware.",
      "proposals": [
        {
          "title": "Post working code example and close",
          "description": "Reply with a code snippet showing how to use SKCanvasSaveLayerRec.Backdrop = SKImageFilter.CreateBlur(...) and reference #3621 for the pending BackdropTileMode enhancement.",
          "category": "workaround",
          "codeSnippet": "// Backdrop blur (frosted glass effect)\nusing var blurFilter = SKImageFilter.CreateBlur(20, 20);\nvar rec = new SKCanvasSaveLayerRec { Backdrop = blurFilter };\ncanvas.SaveLayer(rec);\n// Draw your overlay content here\ncanvas.Restore();",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Post working code example and close",
      "recommendedReason": "Core feature is already implemented. A clear code example and close-as-fixed is the most helpful response. BackdropTileMode remains tracked in #3621."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.87,
      "reason": "SKCanvasSaveLayerRec.Backdrop API is already present and enables backdrop blur. The issue can be closed with a comment showing the usage pattern. The newer BackdropTileMode field is tracked separately in #3621.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and core area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post code example showing SKCanvasSaveLayerRec.Backdrop usage for backdrop blur",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Good news: this functionality is already available in SkiaSharp! You can use `SKCanvasSaveLayerRec.Backdrop` with `SKImageFilter.CreateBlur()` to achieve background blur:\n\n```csharp\n// Backdrop blur (frosted glass effect)\nusing var blurFilter = SKImageFilter.CreateBlur(20, 20);\nvar rec = new SKCanvasSaveLayerRec { Backdrop = blurFilter };\ncanvas.SaveLayer(rec);\n// Draw your overlay content here (e.g. semi-transparent panel)\ncanvas.Restore();\n```\n\nThe `Backdrop` field on `SKCanvasSaveLayerRec` maps directly to `SkCanvas::SaveLayerRec::fBackdrop` — anything drawn before `SaveLayer` is processed through the filter before appearing in the new layer.\n\nFor edge tile-mode control (to avoid edge artifacts), that enhancement (`BackdropTileMode`) is tracked in #3621 and depends on PR #2962."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — core feature is already present in SKCanvasSaveLayerRec.Backdrop",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Link to #3621 which tracks the pending BackdropTileMode enhancement",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3621
      }
    ]
  }
}
```

</details>
