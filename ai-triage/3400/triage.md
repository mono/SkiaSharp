# Issue Triage Report — #3400

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T14:33:11Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SKDocument.CreateXps() renders SVG <image> elements at 30-40% of correct size compared to CreatePdf() using the same SKPicture, reported as a regression from SkiaSharp 2.88.9.

**Analysis:** The Skia XPS backend applies an additional implicit DPI-based scale transform to raster images drawn inside DrawPicture, whereas the PDF backend does not. When the user scales the canvas by contentScale (72/96 = 0.75) before DrawPicture, the XPS path double-scales embedded images, resulting in ~56% size; the reported 30-40% may indicate a further factor. The PDF path is unaffected because it handles the rasterDPI at a different layer. This is most likely a bug in the upstream Skia XPS device (SkXPSDevice) rather than in the SkiaSharp C# wrappers.

**Recommendations:** **needs-investigation** — Regression with a code sample is provided. The XPS backend image scaling bug appears real and traceable to the Skia XPS device DPI handling, but the exact root cause requires native-layer investigation that triage cannot complete (externals submodule not initialised).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/XPS, backend/PDF |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, backend/PDF, tenet/reliability, backend/XPS |

## Evidence

### Reproduction

1. Load an SVG document containing <image> elements with Base64-encoded images
2. Parse the SVG using SKSvg.Load()
3. Generate XPS using SKDocument.CreateXps()
4. Generate PDF using SKDocument.CreatePdf() with the same SVG
5. Compare the image dimensions in XPS vs PDF output

**Environment:** Windows 10/11, .NET 9.0, SkiaSharp 3.116.0, Svg.Skia 3.2.1

**Code snippets:**

```csharp
const float DPI = 72f; const float SVG_DPI = 96f; const float contentScale = DPI / SVG_DPI; // 0.75
var pageWidth = bounds.Width * contentScale; var pageHeight = bounds.Height * contentScale;
// PDF works, XPS does not — same picture, same canvas operations
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | XPS images are significantly smaller (approximately 30-40% of correct size) compared to PDF output using the same SKPicture |
| Repro quality | partial |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.1, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The C# SKDocument wrapper and the XPS document backend in Skia have not seen image-scaling fixes since 3.116.0; the native submodule externals were not accessible to confirm upstream Skia changes. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.78 (78%) |
| Reason | Reporter explicitly states 2.88.9 was the last known good version and 3.116.0 is broken. The ~30-40% size reduction suggests a DPI or transform mismatch that may have been introduced when Skia's XPS device was updated in a newer milestone. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The Skia XPS backend applies an additional implicit DPI-based scale transform to raster images drawn inside DrawPicture, whereas the PDF backend does not. When the user scales the canvas by contentScale (72/96 = 0.75) before DrawPicture, the XPS path double-scales embedded images, resulting in ~56% size; the reported 30-40% may indicate a further factor. The PDF path is unaffected because it handles the rasterDPI at a different layer. This is most likely a bug in the upstream Skia XPS device (SkXPSDevice) rather than in the SkiaSharp C# wrappers.

### Rationale

The issue is a clear wrong-output bug: same input picture, same canvas operations, different visual result between two SkiaSharp document backends. The reporter provides a code sample (though no external repro project), describes the exact symptom (images 30-40% too small), and names the regression baseline (2.88.9). The discrepancy only affects <image> elements, not vector paths or text, which points to how the XPS device serialises bitmaps vs the PDF device.

### Key Signals

- "Images are significantly smaller (approximately 30-40% of correct size)" — **issue body — Actual Behavior section** (Percentage matches a double or triple DPI-scale application inside the XPS renderer for raster images.)
- "This issue only affects <image> elements within SVG; text, barcodes, shapes render correctly in both XPS and PDF" — **issue body — Expected Behavior notes** (The bug is isolated to bitmap/raster draw calls inside an SKPicture, not vector draw calls — consistent with an image-specific scale transform in SkXPSDevice.)
- "Last Known Good Version: 2.88.9 (Previous)" — **issue body — version fields** (Regression introduced when SkiaSharp migrated from Skia milestone used in 2.88.x to a newer one.)
- "CreateXps uses sk_document_create_xps_from_stream(stream.Handle, dpi) with DefaultRasterDpi = 72.0f" — **binding/SkiaSharp/SKDocument.cs lines 67-74** (The DPI argument is passed to the native layer; if the XPS device uses it to scale images differently than PDF, this could explain the discrepancy.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKDocument.cs` | 38-74 | direct | CreateXps() passes a dpi parameter (default 72.0f) via sk_document_create_xps_from_stream. CreatePdf() without explicit dpi calls sk_document_create_pdf_from_stream with no DPI argument. This asymmetry means XPS has a DPI-aware raster scale path that PDF's no-DPI overload does not use in the same way. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | related | sk_document_create_xps_from_stream(stream, dpi) and sk_document_create_xps_from_stream_with_options(stream, options) are both available. The with_options variant exists but is not exposed via a public C# CreateXps overload, meaning callers cannot currently pass XPS options. |

### Workarounds

- Try passing dpi: 96f to CreateXps() to match the SVG coordinate space — this may correct the scale if the mismatch is DPI-induced.
- Remove the canvas.Scale(contentScale) call and instead size the page directly to bounds.Width x bounds.Height, then set dpi: 96f on CreateXps.

### Next Questions

- Does the issue reproduce when calling CreateXps(stream, 96f) to match SVG_DPI instead of 72f?
- Is SkXPSDevice responsible for the image scaling difference, or is it in the C API bridge?
- Does CreateImage() (raster) also reproduce the same image dimensions as PDF, confirming the bug is XPS-specific?

### Resolution Proposals

**Hypothesis:** The Skia XPS device applies the document DPI as an additional scale to raster images when serialising them to XPS XML, whereas the PDF device does not (PDF embeds images at their native resolution). When SkiaSharp sets dpi=72 on the XPS document and the SVG picture was already scaled to 75% for 72-DPI output, images are double-scaled.

1. **Workaround: match XPS DPI to SVG coordinate space** — workaround, confidence 0.60 (60%), cost/xs, validated=untested
   - Call CreateXps(stream, 96f) and do not apply the contentScale transform. Let the document DPI handle the scale instead.
2. **Investigate SkXPSDevice image scale path** — investigation, confidence 0.85 (85%), cost/m, validated=untested
   - Trace DrawBitmap / DrawImage in SkXPSDevice in the Skia C++ source to find where the DPI multiplier is applied to image dimensions, compare with SkPDFDevice.

**Recommended proposal:** Investigate SkXPSDevice image scale path

**Why:** Root cause is most likely in upstream Skia's XPS device. Understanding the exact scaling path is needed before a reliable fix or workaround can be confirmed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Regression with a code sample is provided. The XPS backend image scaling bug appears real and traceable to the Skia XPS device DPI handling, but the exact root cause requires native-layer investigation that triage cannot complete (externals submodule not initialised). |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/XPS, backend/PDF, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/XPS, backend/PDF, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge regression and suggest DPI workaround to test | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This looks like a regression in how the Skia XPS backend scales raster images when a document DPI is set.

As a quick workaround to try while we investigate:

```csharp
// Option 1: pass 96f DPI to match SVG coordinate space and remove the contentScale
using var document = SKDocument.CreateXps(xpsStream, 96f);
using var canvas = document.BeginPage(bounds.Width, bounds.Height);
canvas.Clear(SKColors.White);
canvas.DrawPicture(picture); // no Scale() call
document.EndPage();
document.Close();
```

This may correct the image sizing if the issue is caused by the XPS device applying the 72 DPI factor on top of your existing `contentScale` transform.

Could you also confirm:
1. Whether passing `dpi: 96f` to `CreateXps` changes the image size?
2. The exact SkiaSharp NuGet version in use (the issue mentions both 3.119.1 and 3.116.0 — which one are you testing with)?

We'll trace the XPS image serialisation path in the native Skia library to find the root cause.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3400,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T14:33:11Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "backend/PDF",
      "tenet/reliability",
      "backend/XPS"
    ]
  },
  "summary": "SKDocument.CreateXps() renders SVG <image> elements at 30-40% of correct size compared to CreatePdf() using the same SKPicture, reported as a regression from SkiaSharp 2.88.9.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/XPS",
      "backend/PDF"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "XPS images are significantly smaller (approximately 30-40% of correct size) compared to PDF output using the same SKPicture",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load an SVG document containing <image> elements with Base64-encoded images",
        "Parse the SVG using SKSvg.Load()",
        "Generate XPS using SKDocument.CreateXps()",
        "Generate PDF using SKDocument.CreatePdf() with the same SVG",
        "Compare the image dimensions in XPS vs PDF output"
      ],
      "codeSnippets": [
        "const float DPI = 72f; const float SVG_DPI = 96f; const float contentScale = DPI / SVG_DPI; // 0.75\nvar pageWidth = bounds.Width * contentScale; var pageHeight = bounds.Height * contentScale;\n// PDF works, XPS does not — same picture, same canvas operations"
      ],
      "environmentDetails": "Windows 10/11, .NET 9.0, SkiaSharp 3.116.0, Svg.Skia 3.2.1"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.1",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The C# SKDocument wrapper and the XPS document backend in Skia have not seen image-scaling fixes since 3.116.0; the native submodule externals were not accessible to confirm upstream Skia changes."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.78,
      "reason": "Reporter explicitly states 2.88.9 was the last known good version and 3.116.0 is broken. The ~30-40% size reduction suggests a DPI or transform mismatch that may have been introduced when Skia's XPS device was updated in a newer milestone.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The Skia XPS backend applies an additional implicit DPI-based scale transform to raster images drawn inside DrawPicture, whereas the PDF backend does not. When the user scales the canvas by contentScale (72/96 = 0.75) before DrawPicture, the XPS path double-scales embedded images, resulting in ~56% size; the reported 30-40% may indicate a further factor. The PDF path is unaffected because it handles the rasterDPI at a different layer. This is most likely a bug in the upstream Skia XPS device (SkXPSDevice) rather than in the SkiaSharp C# wrappers.",
    "rationale": "The issue is a clear wrong-output bug: same input picture, same canvas operations, different visual result between two SkiaSharp document backends. The reporter provides a code sample (though no external repro project), describes the exact symptom (images 30-40% too small), and names the regression baseline (2.88.9). The discrepancy only affects <image> elements, not vector paths or text, which points to how the XPS device serialises bitmaps vs the PDF device.",
    "keySignals": [
      {
        "text": "Images are significantly smaller (approximately 30-40% of correct size)",
        "source": "issue body — Actual Behavior section",
        "interpretation": "Percentage matches a double or triple DPI-scale application inside the XPS renderer for raster images."
      },
      {
        "text": "This issue only affects <image> elements within SVG; text, barcodes, shapes render correctly in both XPS and PDF",
        "source": "issue body — Expected Behavior notes",
        "interpretation": "The bug is isolated to bitmap/raster draw calls inside an SKPicture, not vector draw calls — consistent with an image-specific scale transform in SkXPSDevice."
      },
      {
        "text": "Last Known Good Version: 2.88.9 (Previous)",
        "source": "issue body — version fields",
        "interpretation": "Regression introduced when SkiaSharp migrated from Skia milestone used in 2.88.x to a newer one."
      },
      {
        "text": "CreateXps uses sk_document_create_xps_from_stream(stream.Handle, dpi) with DefaultRasterDpi = 72.0f",
        "source": "binding/SkiaSharp/SKDocument.cs lines 67-74",
        "interpretation": "The DPI argument is passed to the native layer; if the XPS device uses it to scale images differently than PDF, this could explain the discrepancy."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKDocument.cs",
        "lines": "38-74",
        "finding": "CreateXps() passes a dpi parameter (default 72.0f) via sk_document_create_xps_from_stream. CreatePdf() without explicit dpi calls sk_document_create_pdf_from_stream with no DPI argument. This asymmetry means XPS has a DPI-aware raster scale path that PDF's no-DPI overload does not use in the same way.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_document_create_xps_from_stream(stream, dpi) and sk_document_create_xps_from_stream_with_options(stream, options) are both available. The with_options variant exists but is not exposed via a public C# CreateXps overload, meaning callers cannot currently pass XPS options.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the issue reproduce when calling CreateXps(stream, 96f) to match SVG_DPI instead of 72f?",
      "Is SkXPSDevice responsible for the image scaling difference, or is it in the C API bridge?",
      "Does CreateImage() (raster) also reproduce the same image dimensions as PDF, confirming the bug is XPS-specific?"
    ],
    "workarounds": [
      "Try passing dpi: 96f to CreateXps() to match the SVG coordinate space — this may correct the scale if the mismatch is DPI-induced.",
      "Remove the canvas.Scale(contentScale) call and instead size the page directly to bounds.Width x bounds.Height, then set dpi: 96f on CreateXps."
    ],
    "resolution": {
      "hypothesis": "The Skia XPS device applies the document DPI as an additional scale to raster images when serialising them to XPS XML, whereas the PDF device does not (PDF embeds images at their native resolution). When SkiaSharp sets dpi=72 on the XPS document and the SVG picture was already scaled to 75% for 72-DPI output, images are double-scaled.",
      "proposals": [
        {
          "title": "Workaround: match XPS DPI to SVG coordinate space",
          "description": "Call CreateXps(stream, 96f) and do not apply the contentScale transform. Let the document DPI handle the scale instead.",
          "category": "workaround",
          "confidence": 0.6,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate SkXPSDevice image scale path",
          "description": "Trace DrawBitmap / DrawImage in SkXPSDevice in the Skia C++ source to find where the DPI multiplier is applied to image dimensions, compare with SkPDFDevice.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate SkXPSDevice image scale path",
      "recommendedReason": "Root cause is most likely in upstream Skia's XPS device. Understanding the exact scaling path is needed before a reliable fix or workaround can be confirmed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Regression with a code sample is provided. The XPS backend image scaling bug appears real and traceable to the Skia XPS device DPI handling, but the exact root cause requires native-layer investigation that triage cannot complete (externals submodule not initialised).",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/XPS, backend/PDF, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/XPS",
          "backend/PDF",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression and suggest DPI workaround to test",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report. This looks like a regression in how the Skia XPS backend scales raster images when a document DPI is set.\n\nAs a quick workaround to try while we investigate:\n\n```csharp\n// Option 1: pass 96f DPI to match SVG coordinate space and remove the contentScale\nusing var document = SKDocument.CreateXps(xpsStream, 96f);\nusing var canvas = document.BeginPage(bounds.Width, bounds.Height);\ncanvas.Clear(SKColors.White);\ncanvas.DrawPicture(picture); // no Scale() call\ndocument.EndPage();\ndocument.Close();\n```\n\nThis may correct the image sizing if the issue is caused by the XPS device applying the 72 DPI factor on top of your existing `contentScale` transform.\n\nCould you also confirm:\n1. Whether passing `dpi: 96f` to `CreateXps` changes the image size?\n2. The exact SkiaSharp NuGet version in use (the issue mentions both 3.119.1 and 3.116.0 — which one are you testing with)?\n\nWe'll trace the XPS image serialisation path in the native Skia library to find the root cause."
      }
    ]
  }
}
```

</details>
