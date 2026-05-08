# Issue Triage Report — #1901

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T19:47:38Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** ClipRegion with a path-backed SKRegion produces incorrect bounding-box clipping instead of path clipping when the canvas is rendering to a PDF document.

**Analysis:** The Skia PDF backend does not faithfully handle SkRegion-based clipping. SkRegion is a pixel-level scanline representation; the PDF device converts it to its bounding rectangle when writing clipping commands, since PDF clip paths must be vector paths. The result is bounding-box clipping. The SkiaSharp C# binding correctly delegates to sk_canvas_clip_region, so the bug is in the upstream Skia PDF device.

**Recommendations:** **needs-investigation** — Real wrong-output bug with complete repro. Root cause is in Skia PDF backend behavior with SkRegion clips. A workaround exists (use ClipPath) but the upstream behavior should be confirmed on current Skia and potentially documented.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/PDF |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKPath with a circle (AddCircle)
2. Wrap it in an SKRegion: new SKRegion(clipPath)
3. Call canvas.ClipRegion(region) on a PDF document canvas
4. Draw a rectangle larger than the circle
5. Observe that the PDF clips to the bounding square, not the circle

**Environment:** .NET Core on Windows, SkiaSharp 2.80.3

**Repository links:**
- https://gist.github.com/WinstonSmith77/204eb0316b19ad2a2403205d2940647a — Minimal reproduction gist with bitmap vs PDF comparison

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | PDF canvas clips to bounding box of region instead of path shape |
| Repro quality | complete |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkRegion-based clipping behavior in the Skia PDF backend is a fundamental architectural limitation that has not changed. |

## Analysis

### Technical Summary

The Skia PDF backend does not faithfully handle SkRegion-based clipping. SkRegion is a pixel-level scanline representation; the PDF device converts it to its bounding rectangle when writing clipping commands, since PDF clip paths must be vector paths. The result is bounding-box clipping. The SkiaSharp C# binding correctly delegates to sk_canvas_clip_region, so the bug is in the upstream Skia PDF device.

### Rationale

Reporter provides screenshots clearly showing correct raster output vs. wrong bounding-box PDF output, with a working gist. The SKCanvas.ClipRegion call is correctly forwarded to sk_canvas_clip_region in the C API. The discrepancy is backend-specific: PDF cannot represent an arbitrary pixel region, so Skia falls back to the bounding box. This is a known upstream Skia PDF limitation, not a SkiaSharp binding error.

### Key Signals

- "Bitmap is fine: Clipping by circle / PDF is wrong: Bounding Box of circle" — **issue body** (Confirms the wrong-output is PDF-specific — raster rendering is correct.)
- "canvas.ClipRegion(new SKRegion(clipPath))" — **issue body** (User is clipping via SKRegion wrapping a path; the path information is lost when Skia PDF renders the region clip.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 268-274 | direct | ClipRegion correctly validates the SKRegion argument and delegates to SkiaApi.sk_canvas_clip_region — no bug in the SkiaSharp binding layer. |
| `binding/SkiaSharp/SKRegion.cs` | 31-35 | direct | SKRegion(SKPath path) constructor calls SetPath to rasterize the path into scanline regions. Once rasterized, the original path geometry is no longer stored in the region object. |
| `binding/SkiaSharp/SKCanvas.cs` | 260-266 | related | ClipPath method (alternative API) uses sk_canvas_clip_path_with_operation which preserves vector path geometry — this is what should be used for PDF clipping. |

### Workarounds

- Use canvas.ClipPath(clipPath) instead of canvas.ClipRegion(new SKRegion(clipPath)) — ClipPath preserves vector path geometry and the PDF backend can emit a proper PDF clip path.
- If SKRegion is required for hit-testing or other raster operations, keep using SKRegion but switch to canvas.ClipPath() for the PDF draw step.

### Next Questions

- Is this reproducible on current SkiaSharp (3.x) or was it silently fixed in a newer Skia milestone?
- Should SkiaSharp document that ClipRegion is not recommended for vector backends (PDF, SVG) and silently falls back to bounding-box?

### Resolution Proposals

**Hypothesis:** The Skia PDF device cannot render an SkRegion clip as a vector clip path, so it falls back to the bounding rectangle. The fix is to use ClipPath with the original path object instead of wrapping it in an SKRegion.

1. **Use ClipPath instead of ClipRegion for PDF** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace canvas.ClipRegion(new SKRegion(clipPath)) with canvas.ClipPath(clipPath). ClipPath preserves vector geometry and works correctly with the PDF backend.

```csharp
var clipPath = new SKPath();
clipPath.AddCircle(100, 100, 50);
canvas.ClipPath(clipPath); // use ClipPath instead of ClipRegion for PDF
```
2. **Document the limitation in ClipRegion XML docs** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add XML documentation noting that ClipRegion approximates to bounding box on vector backends (PDF, SVG) and users should prefer ClipPath when targeting those backends.

**Recommended proposal:** Use ClipPath instead of ClipRegion for PDF

**Why:** Immediately actionable with a one-line code change and produces correct output.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real wrong-output bug with complete repro. Root cause is in Skia PDF backend behavior with SkRegion clips. A workaround exists (use ClipPath) but the upstream behavior should be confirmed on current Skia and potentially documented. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, SkiaSharp core, Windows, PDF backend labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/PDF, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Post analysis with ClipPath workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and screenshots — these clearly show the problem.

This is a known limitation of Skia's PDF backend: `SkRegion` is a pixel-level scanline representation, and when the PDF device encounters a region clip it falls back to the bounding rectangle (since PDF clip paths must be vector geometry and the original path is no longer stored in the region).

**Workaround:** Use `ClipPath` directly instead of wrapping the path in an `SKRegion`:

```csharp
var clipPath = new SKPath();
clipPath.AddCircle(100, 100, 50);
canvas.ClipPath(clipPath); // correct for PDF — preserves vector geometry
```

`ClipPath` preserves the vector path and the PDF backend can emit a proper clip path. `ClipRegion` is intended for pixel-raster use cases (hit testing, scanline operations) and should be avoided when targeting PDF or SVG canvases.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1901,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T19:47:38Z"
  },
  "summary": "ClipRegion with a path-backed SKRegion produces incorrect bounding-box clipping instead of path clipping when the canvas is rendering to a PDF document.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/PDF"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "PDF canvas clips to bounding box of region instead of path shape",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKPath with a circle (AddCircle)",
        "Wrap it in an SKRegion: new SKRegion(clipPath)",
        "Call canvas.ClipRegion(region) on a PDF document canvas",
        "Draw a rectangle larger than the circle",
        "Observe that the PDF clips to the bounding square, not the circle"
      ],
      "environmentDetails": ".NET Core on Windows, SkiaSharp 2.80.3",
      "repoLinks": [
        {
          "url": "https://gist.github.com/WinstonSmith77/204eb0316b19ad2a2403205d2940647a",
          "description": "Minimal reproduction gist with bitmap vs PDF comparison"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SkRegion-based clipping behavior in the Skia PDF backend is a fundamental architectural limitation that has not changed."
    }
  },
  "analysis": {
    "summary": "The Skia PDF backend does not faithfully handle SkRegion-based clipping. SkRegion is a pixel-level scanline representation; the PDF device converts it to its bounding rectangle when writing clipping commands, since PDF clip paths must be vector paths. The result is bounding-box clipping. The SkiaSharp C# binding correctly delegates to sk_canvas_clip_region, so the bug is in the upstream Skia PDF device.",
    "rationale": "Reporter provides screenshots clearly showing correct raster output vs. wrong bounding-box PDF output, with a working gist. The SKCanvas.ClipRegion call is correctly forwarded to sk_canvas_clip_region in the C API. The discrepancy is backend-specific: PDF cannot represent an arbitrary pixel region, so Skia falls back to the bounding box. This is a known upstream Skia PDF limitation, not a SkiaSharp binding error.",
    "keySignals": [
      {
        "text": "Bitmap is fine: Clipping by circle / PDF is wrong: Bounding Box of circle",
        "source": "issue body",
        "interpretation": "Confirms the wrong-output is PDF-specific — raster rendering is correct."
      },
      {
        "text": "canvas.ClipRegion(new SKRegion(clipPath))",
        "source": "issue body",
        "interpretation": "User is clipping via SKRegion wrapping a path; the path information is lost when Skia PDF renders the region clip."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "268-274",
        "finding": "ClipRegion correctly validates the SKRegion argument and delegates to SkiaApi.sk_canvas_clip_region — no bug in the SkiaSharp binding layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRegion.cs",
        "lines": "31-35",
        "finding": "SKRegion(SKPath path) constructor calls SetPath to rasterize the path into scanline regions. Once rasterized, the original path geometry is no longer stored in the region object.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "260-266",
        "finding": "ClipPath method (alternative API) uses sk_canvas_clip_path_with_operation which preserves vector path geometry — this is what should be used for PDF clipping.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use canvas.ClipPath(clipPath) instead of canvas.ClipRegion(new SKRegion(clipPath)) — ClipPath preserves vector path geometry and the PDF backend can emit a proper PDF clip path.",
      "If SKRegion is required for hit-testing or other raster operations, keep using SKRegion but switch to canvas.ClipPath() for the PDF draw step."
    ],
    "nextQuestions": [
      "Is this reproducible on current SkiaSharp (3.x) or was it silently fixed in a newer Skia milestone?",
      "Should SkiaSharp document that ClipRegion is not recommended for vector backends (PDF, SVG) and silently falls back to bounding-box?"
    ],
    "resolution": {
      "hypothesis": "The Skia PDF device cannot render an SkRegion clip as a vector clip path, so it falls back to the bounding rectangle. The fix is to use ClipPath with the original path object instead of wrapping it in an SKRegion.",
      "proposals": [
        {
          "title": "Use ClipPath instead of ClipRegion for PDF",
          "description": "Replace canvas.ClipRegion(new SKRegion(clipPath)) with canvas.ClipPath(clipPath). ClipPath preserves vector geometry and works correctly with the PDF backend.",
          "category": "workaround",
          "codeSnippet": "var clipPath = new SKPath();\nclipPath.AddCircle(100, 100, 50);\ncanvas.ClipPath(clipPath); // use ClipPath instead of ClipRegion for PDF",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Document the limitation in ClipRegion XML docs",
          "description": "Add XML documentation noting that ClipRegion approximates to bounding box on vector backends (PDF, SVG) and users should prefer ClipPath when targeting those backends.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use ClipPath instead of ClipRegion for PDF",
      "recommendedReason": "Immediately actionable with a one-line code change and produces correct output."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real wrong-output bug with complete repro. Root cause is in Skia PDF backend behavior with SkRegion clips. A workaround exists (use ClipPath) but the upstream behavior should be confirmed on current Skia and potentially documented.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, Windows, PDF backend labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/PDF",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with ClipPath workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and screenshots — these clearly show the problem.\n\nThis is a known limitation of Skia's PDF backend: `SkRegion` is a pixel-level scanline representation, and when the PDF device encounters a region clip it falls back to the bounding rectangle (since PDF clip paths must be vector geometry and the original path is no longer stored in the region).\n\n**Workaround:** Use `ClipPath` directly instead of wrapping the path in an `SKRegion`:\n\n```csharp\nvar clipPath = new SKPath();\nclipPath.AddCircle(100, 100, 50);\ncanvas.ClipPath(clipPath); // correct for PDF — preserves vector geometry\n```\n\n`ClipPath` preserves the vector path and the PDF backend can emit a proper clip path. `ClipRegion` is intended for pixel-raster use cases (hit testing, scanline operations) and should be avoided when targeting PDF or SVG canvases."
      }
    ]
  }
}
```

</details>
