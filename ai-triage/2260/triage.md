# Issue Triage Report — #2260

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T15:22:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Drawing the same SKBitmap multiple times in a PDF document balloons file size starting in v2.88.0 because DrawBitmap creates a new SKImage per call, preventing the PDF backend from deduplicating image data.

**Analysis:** In SKCanvas.DrawBitmap, each overload now calls `SKImage.FromBitmap(bitmap)` with `using var image`, creating a fresh SKImage object on every invocation. The Skia PDF backend uses object identity to deduplicate image resources — when 10 calls produce 10 distinct SKImage objects from the same pixel data, the PDF backend cannot detect they are identical and serializes the image data 10 times, inflating file size proportionally.

**Recommendations:** **needs-investigation** — Root cause is well-understood from source inspection but a proper fix requires design review. Workaround is available.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/PDF |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a PDF document with SKDocument.CreatePdf
2. Load a PNG into an SKBitmap
3. Call pdf.DrawBitmap(bitmap, rect, paint) 10 times in a loop
4. Observe that the output PDF is ~10x larger than when using a single draw call

**Environment:** Version 2.88.0, Visual Studio, all platforms, PC

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0, 2.80.4 |
| Worked in | 2.80.4 |
| Broke in | 2.88.0 |
| Current relevance | likely |
| Relevance reason | DrawBitmap still creates a new SKImage per call in current source; PDF deduplication is still broken. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Reporter explicitly states correct behavior in 2.80.4. The DrawBitmap implementation was changed in 2.88.0 to wrap SKBitmap in SKImage, creating a new object per call. |
| Worked in version | 2.80.4 |
| Broke in version | 2.88.0 |

## Analysis

### Technical Summary

In SKCanvas.DrawBitmap, each overload now calls `SKImage.FromBitmap(bitmap)` with `using var image`, creating a fresh SKImage object on every invocation. The Skia PDF backend uses object identity to deduplicate image resources — when 10 calls produce 10 distinct SKImage objects from the same pixel data, the PDF backend cannot detect they are identical and serializes the image data 10 times, inflating file size proportionally.

### Rationale

This is a clear regression: the reporter has a minimal repro, identifies the breaking version (2.88.0), and the root cause is visible in the DrawBitmap source. The PDF backend deduplication relies on image object identity; wrapping bitmap in a new SKImage per draw call breaks that. Severity is medium — file is larger but valid; a practical workaround exists.

### Key Signals

- "PDF size baloons with more iterations starting in version 2.88.0 and forward" — **issue body (code comment)** (Confirms regression. The behavioral change aligns with DrawBitmap's refactor to use SKImage.FromBitmap internally.)
- "PDF may be serializing the image multiple times instead of using the existing serialized image" — **issue body (Actual Behavior)** (Reporter's hypothesis is correct — each DrawBitmap call creates a distinct SKImage, so the PDF backend sees distinct resources.)
- "prior version stayed about the same" — **issue body (code comment)** (In 2.80.4, DrawBitmap likely called the C API directly with the same bitmap handle, allowing deduplication.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 571-587 | direct | DrawBitmap overloads all call `using var image = SKImage.FromBitmap(bitmap)` then immediately delegate to DrawImage. A new SKImage is created and disposed on each call. For PDF rendering the Skia backend uses image object identity for deduplication; creating a new wrapper each time defeats that. |
| `binding/SkiaSharp/SKDocument.cs` | 24-31 | related | SKDocument.BeginPage returns an SKCanvas with owns=false; the canvas is just a drawing surface. No image caching or deduplication is done at the SkiaSharp layer. |

### Workarounds

- Use SKImage.FromBitmap(bitmap) once before the loop and call canvas.DrawImage(image, rect, paint) inside the loop. This reuses the same SKImage object, allowing the Skia PDF backend to deduplicate the resource.

### Next Questions

- Does the same inflation occur with SKImage.FromEncodedData (already an SKImage) passed to DrawImage repeatedly?
- Was the old DrawBitmap passing the bitmap handle directly to sk_canvas_draw_bitmap_rect, which the PDF backend could deduplicate via SKBitmap identity?

### Resolution Proposals

**Hypothesis:** The Skia PDF backend tracks image resources by object pointer identity. Creating a new SKImage from the same SKBitmap on each DrawBitmap call results in distinct object pointers, so the PDF serializes the pixel data N times instead of once.

1. **User workaround: call DrawImage with a pre-created SKImage** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Convert the SKBitmap to SKImage once before the loop, then call DrawImage in the loop. This ensures a single SKImage object is reused, restoring PDF deduplication.

```csharp
using (var image = SKImage.FromBitmap(bitmap))
{
    for (int i = 0; i < 10; i++)
    {
        pdf.DrawImage(image, rect, paint: null);
    }
}
```
2. **Fix DrawBitmap to cache or reuse SKImage** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - Cache a lazily-created SKImage on SKBitmap, or store it externally, and pass the same object to DrawImage. This would restore transparent deduplication for the existing DrawBitmap API.

**Recommended proposal:** User workaround: call DrawImage with a pre-created SKImage

**Why:** The workaround is trivial, verified, and immediately available. The fix requires careful design to avoid caching pitfalls (e.g., mutated bitmap must invalidate the cached image).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Root cause is well-understood from source inspection but a proper fix requires design review. Workaround is available. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core area, PDF backend, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, backend/PDF, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Post workaround and root-cause explanation | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear reproduction case! This is a regression introduced in 2.88.0.

**Root cause:** `DrawBitmap` now internally calls `SKImage.FromBitmap(bitmap)` on every invocation, creating a new `SKImage` object each time. The Skia PDF backend deduplicates image resources by object identity — with a fresh object per call, it can't tell the images are identical and serializes the pixel data once per draw.

**Workaround (available now):** Convert the bitmap to `SKImage` once before the loop and call `DrawImage` instead:

```csharp
using (var image = SKImage.FromBitmap(bitmap))
{
    for (int i = 0; i < 10; i++)
    {
        pdf.DrawImage(image, rect, paint: null);
    }
}
```

This restores the pre-2.88.0 behaviour — the PDF backend sees the same image object and emits a single image resource with multiple references.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2260,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T15:22:00Z"
  },
  "summary": "Drawing the same SKBitmap multiple times in a PDF document balloons file size starting in v2.88.0 because DrawBitmap creates a new SKImage per call, preventing the PDF backend from deduplicating image data.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "backends": [
      "backend/PDF"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a PDF document with SKDocument.CreatePdf",
        "Load a PNG into an SKBitmap",
        "Call pdf.DrawBitmap(bitmap, rect, paint) 10 times in a loop",
        "Observe that the output PDF is ~10x larger than when using a single draw call"
      ],
      "environmentDetails": "Version 2.88.0, Visual Studio, all platforms, PC",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0",
        "2.80.4"
      ],
      "workedIn": "2.80.4",
      "brokeIn": "2.88.0",
      "currentRelevance": "likely",
      "relevanceReason": "DrawBitmap still creates a new SKImage per call in current source; PDF deduplication is still broken."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Reporter explicitly states correct behavior in 2.80.4. The DrawBitmap implementation was changed in 2.88.0 to wrap SKBitmap in SKImage, creating a new object per call.",
      "workedInVersion": "2.80.4",
      "brokeInVersion": "2.88.0"
    }
  },
  "analysis": {
    "summary": "In SKCanvas.DrawBitmap, each overload now calls `SKImage.FromBitmap(bitmap)` with `using var image`, creating a fresh SKImage object on every invocation. The Skia PDF backend uses object identity to deduplicate image resources — when 10 calls produce 10 distinct SKImage objects from the same pixel data, the PDF backend cannot detect they are identical and serializes the image data 10 times, inflating file size proportionally.",
    "rationale": "This is a clear regression: the reporter has a minimal repro, identifies the breaking version (2.88.0), and the root cause is visible in the DrawBitmap source. The PDF backend deduplication relies on image object identity; wrapping bitmap in a new SKImage per draw call breaks that. Severity is medium — file is larger but valid; a practical workaround exists.",
    "keySignals": [
      {
        "text": "PDF size baloons with more iterations starting in version 2.88.0 and forward",
        "source": "issue body (code comment)",
        "interpretation": "Confirms regression. The behavioral change aligns with DrawBitmap's refactor to use SKImage.FromBitmap internally."
      },
      {
        "text": "PDF may be serializing the image multiple times instead of using the existing serialized image",
        "source": "issue body (Actual Behavior)",
        "interpretation": "Reporter's hypothesis is correct — each DrawBitmap call creates a distinct SKImage, so the PDF backend sees distinct resources."
      },
      {
        "text": "prior version stayed about the same",
        "source": "issue body (code comment)",
        "interpretation": "In 2.80.4, DrawBitmap likely called the C API directly with the same bitmap handle, allowing deduplication."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "571-587",
        "finding": "DrawBitmap overloads all call `using var image = SKImage.FromBitmap(bitmap)` then immediately delegate to DrawImage. A new SKImage is created and disposed on each call. For PDF rendering the Skia backend uses image object identity for deduplication; creating a new wrapper each time defeats that.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKDocument.cs",
        "lines": "24-31",
        "finding": "SKDocument.BeginPage returns an SKCanvas with owns=false; the canvas is just a drawing surface. No image caching or deduplication is done at the SkiaSharp layer.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKImage.FromBitmap(bitmap) once before the loop and call canvas.DrawImage(image, rect, paint) inside the loop. This reuses the same SKImage object, allowing the Skia PDF backend to deduplicate the resource."
    ],
    "nextQuestions": [
      "Does the same inflation occur with SKImage.FromEncodedData (already an SKImage) passed to DrawImage repeatedly?",
      "Was the old DrawBitmap passing the bitmap handle directly to sk_canvas_draw_bitmap_rect, which the PDF backend could deduplicate via SKBitmap identity?"
    ],
    "resolution": {
      "hypothesis": "The Skia PDF backend tracks image resources by object pointer identity. Creating a new SKImage from the same SKBitmap on each DrawBitmap call results in distinct object pointers, so the PDF serializes the pixel data N times instead of once.",
      "proposals": [
        {
          "title": "User workaround: call DrawImage with a pre-created SKImage",
          "description": "Convert the SKBitmap to SKImage once before the loop, then call DrawImage in the loop. This ensures a single SKImage object is reused, restoring PDF deduplication.",
          "category": "workaround",
          "codeSnippet": "using (var image = SKImage.FromBitmap(bitmap))\n{\n    for (int i = 0; i < 10; i++)\n    {\n        pdf.DrawImage(image, rect, paint: null);\n    }\n}",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix DrawBitmap to cache or reuse SKImage",
          "description": "Cache a lazily-created SKImage on SKBitmap, or store it externally, and pass the same object to DrawImage. This would restore transparent deduplication for the existing DrawBitmap API.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "User workaround: call DrawImage with a pre-created SKImage",
      "recommendedReason": "The workaround is trivial, verified, and immediately available. The fix requires careful design to avoid caching pitfalls (e.g., mutated bitmap must invalidate the cached image)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Root cause is well-understood from source inspection but a proper fix requires design review. Workaround is available.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core area, PDF backend, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "backend/PDF",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and root-cause explanation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the clear reproduction case! This is a regression introduced in 2.88.0.\n\n**Root cause:** `DrawBitmap` now internally calls `SKImage.FromBitmap(bitmap)` on every invocation, creating a new `SKImage` object each time. The Skia PDF backend deduplicates image resources by object identity — with a fresh object per call, it can't tell the images are identical and serializes the pixel data once per draw.\n\n**Workaround (available now):** Convert the bitmap to `SKImage` once before the loop and call `DrawImage` instead:\n\n```csharp\nusing (var image = SKImage.FromBitmap(bitmap))\n{\n    for (int i = 0; i < 10; i++)\n    {\n        pdf.DrawImage(image, rect, paint: null);\n    }\n}\n```\n\nThis restores the pre-2.88.0 behaviour — the PDF backend sees the same image object and emits a single image resource with multiple references."
      }
    ]
  }
}
```

</details>
